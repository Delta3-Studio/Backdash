using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backdash.Data;

/// <summary>
///     Defines an object pooling contract
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObjectPool<T>
{
    /// <summary>
    ///     Rent an instance on <typeparamref name="T" /> from the pool
    /// </summary>
    T Rent();

    /// <summary>
    ///     Return <paramref name="value" /> to the pool
    /// </summary>
    bool Return(T value);
}

/// <summary>
///     Default object pool for types with empty constructor
/// </summary>
public sealed class ObjectPool<T> : IObjectPool<T>, IEnumerable<T>, IDisposable where T : class
{
    /// <summary>
    ///     Maximum number of objects allowed in the pool
    /// </summary>
    public int Capacity { get; } // -1 to account for fastItem

    int numItems;
    T? fastItem;
    readonly Stack<T> items;
    readonly HashSet<T> set;
    readonly Func<T> createFunc;
    readonly Action<T>? returnFunc;
    readonly IEqualityComparer<T> comparer;

    /// <summary>
    ///     Instantiate new <see cref="ObjectPool{T}" />
    /// </summary>
    public ObjectPool(
        Func<T> createFunc,
        Action<T>? returnFunc = null,
        int? capacity = null,
        IEqualityComparer<T>? comparer = null
    )
    {
        this.createFunc = createFunc;
        this.returnFunc = returnFunc;
        this.comparer = comparer ?? ReferenceEqualityComparer.Instance;
        Capacity = (capacity ?? 100) - 1;
        items = new(Capacity);
        set = new(Capacity, this.comparer);
    }

    bool Contains(T value) => comparer.Equals(fastItem, value) || set.Contains(value);

    /// <inheritdoc />
    public T Rent()
    {
        var item = fastItem;

        if (item is not null)
        {
            fastItem = null;
            return item;
        }

        if (!items.TryPop(out item))
            return createFunc();

        numItems--;
        set.Remove(item);
        return item;
    }

    /// <inheritdoc />
    public bool Return(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (Contains(value)) return true;

        returnFunc?.Invoke(value);

        if (fastItem is null)
        {
            fastItem = value;
            return true;
        }

        if (numItems >= Capacity)
            return false;

        if (!set.Add(value)) return true;
        numItems++;
        items.Push(value);
        return true;
    }

    /// <inheritdoc cref="Return"/>
    public bool ReturnMany(params T[] values)
    {
        var result = true;

        ref var current = ref MemoryMarshal.GetReference(values.AsSpan());
        ref var limit = ref Unsafe.Add(ref current, values.Length);

        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            result = result && Return(current);
            current = ref Unsafe.Add(ref current, 1)!;
        }

        return result;
    }

    /// <inheritdoc cref="ReturnMany(T[])"/>
    public bool ReturnMany(IEnumerable<T> values) =>
        values.Aggregate(true, (result, current) => result && Return(current));

    /// <summary>
    ///     Clear the object pool
    /// </summary>
    public void Clear()
    {
        numItems = 0;
        fastItem = null;
        items.Clear();
        set.Clear();
    }

    /// <summary>
    ///     Preload <paramref name="count"/> pool items.
    /// </summary>
    public void WarmUp(int count)
    {
        List<T> temp = [];
        for (var i = 0; i < count; i++) temp.Add(Rent());
        foreach (var player in temp) Return(player);
    }

    /// <summary>
    ///     Number of instances in the object pool
    /// </summary>
    public int Count => numItems + (fastItem is null ? 0 : 1);

    /// <summary>
    ///     Dispose all disposable objects in the pool
    /// </summary>
    public void Dispose()
    {
        (fastItem as IDisposable)?.Dispose();
        while (items.TryPop(out var item))
            if (item is IDisposable disposable)
                disposable.Dispose();

        Clear();
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public Stack<T>.Enumerator GetEnumerator() => items.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Factory for <see cref="ObjectPool{T}"/>
/// </summary>
public static class ObjectPool
{
    /// <summary>
    /// Create new instance of object pool for <typeparamref name="T"/> with constructor.
    /// </summary>
    public static ObjectPool<T> Create<T>(int? capacity = null, Action<T>? returnWith = null)
        where T : class, new() =>
        new(static () => new(), returnWith, capacity);

    /// <summary>
    /// Create new instance of object pool for <typeparamref name="T"/> with constructor.
    /// </summary>
    public static ObjectPool<T> Create<T>(Action<T> returnWith) where T : class, new() =>
        Create(null, returnWith);

    /// <summary>
    /// Object pool singleton factory
    /// </summary>
    public static ObjectPool<T> Singleton<T>() where T : class, new() => SingletonWrapper<T>.Instance;

    static class SingletonWrapper<T> where T : class, new()
    {
        public static readonly ObjectPool<T> Instance = Create<T>();
    }
}

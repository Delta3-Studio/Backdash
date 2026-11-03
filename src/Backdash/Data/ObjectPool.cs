using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
///     Default object pool for types with empty constructor.
/// </summary>
public sealed class ObjectPool<T> : IObjectPool<T>, IEnumerable<T>, IDisposable where T : class
{
    readonly Stack<T> items;
    readonly HashSet<T> set;
    readonly Func<T> createFunc;
    readonly Action<T>? returnFunc;
    readonly IEqualityComparer<T> comparer;
    readonly int poolCapacity;
    T? fastItem;

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
        ArgumentNullException.ThrowIfNull(createFunc);
        if (capacity is not null)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity.Value);

        this.createFunc = createFunc;
        this.returnFunc = returnFunc;
        this.comparer = comparer ?? ReferenceEqualityComparer.Instance;
        poolCapacity = Math.Max((capacity ?? 100) - 1, 0); // -1 to account for fastItem
        items = new(poolCapacity);
        set = new(poolCapacity, this.comparer);
    }

    /// <summary>
    ///     Maximum number of objects allowed in the pool
    /// </summary>
    public int Capacity => poolCapacity + 1;

    /// <summary>
    ///     Number of instances in the object pool
    /// </summary>
    public int Count => items.Count + (fastItem is null ? 0 : 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        set.Remove(item);
        return item;
    }

    /// <inheritdoc cref="IObjectPool{T}.Rent" />
    public void Rent(ICollection<T> values, int count)
    {
        for (var i = 0; i < count; i++)
            values.Add(Rent());
    }

    /// <summary>
    /// Ensure an object exists, if null rent the next value into it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Ensure([NotNull] ref T? value) =>
        value ??= Rent();

    /// <inheritdoc />
    public bool Return(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (Contains(value)) return true;

        if (fastItem is null)
        {
            returnFunc?.Invoke(value);
            fastItem = value;
            return true;
        }

        if (items.Count >= poolCapacity)
            return false;

        returnFunc?.Invoke(value);
        if (set.Add(value)) items.Push(value);
        return true;
    }

    /// <inheritdoc cref="Return(T)"/>
    public bool Return(ref T? value)
    {
        if (value is null || !Return(value)) return false;
        value = null;
        return true;
    }

    /// <inheritdoc cref="Return(T)"/>
    public bool ReturnAll(List<T> list)
    {
        var result = true;
        var values = CollectionsMarshal.AsSpan(list);
        ref var current = ref MemoryMarshal.GetReference(values);
        ref var limit = ref Unsafe.Add(ref current, values.Length);

        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            var returned = Return(current);
            ref var next = ref Unsafe.Add(ref current, 1)!;
            if (returned)
                current = null!;
            else
                result = false;

            current = ref next;
        }

        if (result)
            list.Clear();
        else
            list.RemoveAll(x => (object?)x is null);

        return result;
    }

    /// <inheritdoc cref="Return(T)"/>
    public bool ReturnMany(ReadOnlySpan<T> values)
    {
        var result = true;
        ref var current = ref MemoryMarshal.GetReference(values);
        ref var limit = ref Unsafe.Add(ref current, values.Length);

        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            result &= Return(current);
            current = ref Unsafe.Add(ref current, 1)!;
        }

        return result;
    }

    /// <inheritdoc cref="ReturnMany(System.ReadOnlySpan{T})"/>
    public bool ReturnMany(IEnumerable<T> values)
    {
        switch (values)
        {
            case T[] array:
                return ReturnMany(array.AsSpan());
            case ImmutableArray<T> array:
                return ReturnMany(array.AsSpan());
            case List<T> list:
                return ReturnMany(CollectionsMarshal.AsSpan(list));
            default:
                var result = true;
                foreach (var item in values)
                    result &= Return(item);

                return result;
        }
    }

    /// <summary>
    ///     Clear the object pool
    /// </summary>
    public void Clear()
    {
        fastItem = null;
        items.Clear();
        set.Clear();
    }

    /// <summary>
    ///     Preload <paramref name="count"/> pool items.
    /// </summary>
    public void WarmUp(int count)
    {
        count = Math.Min(count, Capacity);
        List<T> temp = new(count);
        for (var i = 0; i < count; i++) temp.Add(Rent());
        foreach (var player in temp) Return(player);
    }

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
    public IEnumerator<T> GetEnumerator()
    {
        if (fastItem is not null)
            yield return fastItem;

        foreach (var item in items)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Factory for <see cref="ObjectPool{T}"/>
/// </summary>
public static class ObjectPool
{
    /// <summary>
    /// Create a new instance of the object pool for <typeparamref name="T"/> with constructor.
    /// </summary>
    public static ObjectPool<T> Create<T>(int? capacity = null, Action<T>? returnWith = null)
        where T : class, new() =>
        new(static () => new(), returnWith, capacity);

    /// <summary>
    /// Create a new instance of the object pool for <typeparamref name="T"/> with constructor.
    /// </summary>
    public static ObjectPool<T> Create<T>(Action<T> returnWith) where T : class, new() =>
        Create(null, returnWith);

    /// <summary>
    /// Object pool singleton factory
    /// </summary>
    public static ObjectPool<T> Singleton<T>() where T : class, new() => SingletonWrapper<T>.Instance;

    /// <inheritdoc cref="ObjectPool{T}.Rent()"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Rent<T>() where T : class, new() => Singleton<T>().Rent();

    /// <inheritdoc cref="ObjectPool{T}.Rent(ICollection{T}, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rent<T>(ICollection<T> values, int count) where T : class, new() =>
        Singleton<T>().Rent(values, count);

    /// <inheritdoc cref="ObjectPool{T}.Ensure(ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure<T>([NotNull] ref T? value) where T : class, new() => Singleton<T>().Ensure(ref value);

    /// <inheritdoc cref="ObjectPool{T}.Return(T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Return<T>(T value) where T : class, new() => Singleton<T>().Return(value);

    /// <inheritdoc cref="ObjectPool{T}.Return(ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Return<T>(ref T? value) where T : class, new() => Singleton<T>().Return(ref value);

    /// <inheritdoc cref="ObjectPool{T}.ReturnAll"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReturnAll<T>(List<T> value) where T : class, new() => Singleton<T>().ReturnAll(value);

    static class SingletonWrapper<T> where T : class, new()
    {
        public static readonly ObjectPool<T> Instance = Create<T>();
    }
}

using System.Runtime.InteropServices;
using Backdash.Core;

namespace Backdash;

/// <summary>
/// Library extensions
/// </summary>
public static class NetcodeExtensions
{
    internal static void PushFirst<T>(this Queue<T> queue, in T value)
    {
        var count = queue.Count;
        queue.Enqueue(value);
        for (var i = 0; i < count; i++)
            queue.Enqueue(queue.Dequeue());
    }

    /// <summary>
    /// Return random gaussian number.
    /// </summary>
    public static double NextGaussian(this Random @this)
    {
        double u, v, s;
        do
        {
            u = (2.0 * @this.NextDouble()) - 1.0;
            v = (2.0 * @this.NextDouble()) - 1.0;
            s = (u * u) + (v * v);
        } while (s >= 1.0);

        var fac = Math.Sqrt(-2.0 * Math.Log(s) / s);
        return u * fac;
    }

    /// <summary>
    /// Returns a random <see cref="bool"/>
    /// </summary>
    public static bool NextBool(this Random random) => random.Next(2) is 0;

    /// <summary>
    /// Returns a random <see cref="bool"/> using a success percentage defined by the parameter <paramref name="percentage"/>.
    /// The <paramref name="percentage"/> must be in between 0 and 1.
    /// </summary>
    public static bool NextBool(this Random random, double percentage) =>
        random.NextDouble() < Math.Clamp(percentage, 0.0, 1.0);

    /// <summary>
    /// Fill <paramref name="value"/> with random values.
    /// </summary>
    public static void Generate<T>(this Random random, scoped ref T value) where T : unmanaged
    {
        var bytes = Mem.AsBytes(ref value);
        random.NextBytes(bytes);
    }

    /// <summary>
    /// Return a random <typeparamref name="T"/> value.
    /// </summary>
    public static T Generate<T>(this Random random) where T : unmanaged
    {
        T result = new();
        random.Generate(ref result);
        return result;
    }

    /// <summary>
    /// Fill <paramref name="buffer"/> with random <typeparamref name="T"/> values.
    /// </summary>
    public static void Generate<T>(this Random random, Span<T> buffer) where T : unmanaged =>
        random.NextBytes(MemoryMarshal.AsBytes(buffer));

    /// <summary>
    /// Return an  array with random <typeparamref name="T"/> values.
    /// </summary>
    public static T[] Generate<T>(this Random random, int count) where T : unmanaged
    {
        var result = new T[count];
        random.Generate(result.AsSpan());
        return result;
    }
}

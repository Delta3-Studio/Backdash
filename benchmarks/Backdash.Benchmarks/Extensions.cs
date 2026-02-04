using System.Runtime.InteropServices;
using Backdash.Core;

namespace Backdash.Benchmarks;

public static class Extensions
{
    public static bool NextBool(this Random random) => random.Next(0, 2) == 1;

    public static T Next<T>(this Random random) where T : unmanaged
    {
        var result = new T();
        var bytes = Mem.AsBytes(ref result);
        random.NextBytes(bytes);
        return result;
    }

    public static void Next<T>(this Random random, Span<T> buffer) where T : unmanaged =>
        random.NextBytes(MemoryMarshal.AsBytes(buffer));

    public static T[] Next<T>(this Random random, int count) where T : unmanaged
    {
        var result = new T[count];
        random.NextBytes(MemoryMarshal.AsBytes(result.AsSpan()));
        return result;
    }
}

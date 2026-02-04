using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backdash;

/// <summary>
///     Integer Math
/// </summary>
public static class MathI
{
    /// <summary>
    ///     Divide two integers ceiling the result
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CeilDiv(int x, int y) => x is 0 ? 0 : 1 + ((x - 1) / y);

    /// <summary>
    ///     Returns the sum of a span of <see cref="IBinaryInteger{TSelf}"/>.
    ///     Use SIMD if available.
    /// </summary>
    public static T Sum<T>(ReadOnlySpan<T> span)
        where T : unmanaged, IBinaryInteger<T>, IAdditionOperators<T, T, T>
    {
        unchecked
        {
            var sum = T.Zero;
            ref var current = ref MemoryMarshal.GetReference(span);
            ref var limit = ref Unsafe.Add(ref current, span.Length);

            if (Vector.IsHardwareAccelerated && span.Length >= Vector<T>.Count)
            {
                var vecSize = Vector<T>.Count;
                var sumVec = Vector<T>.Zero;
                ref var vecLimit = ref Unsafe.Add(ref current, span.Length - vecSize);

                while (Unsafe.IsAddressLessThan(ref current, ref vecLimit))
                {
                    sumVec += new Vector<T>(MemoryMarshal.CreateSpan(ref current, vecSize));
                    current = ref Unsafe.Add(ref current, vecSize);
                }

                for (var i = 0; i < vecSize; i++)
                    sum += sumVec[i];
            }

            while (Unsafe.IsAddressLessThan(ref current, ref limit))
            {
                sum += current;
                current = ref Unsafe.Add(ref current, 1);
            }

            return sum;
        }
    }

    /// <inheritdoc cref="Sum{T}(ReadOnlySpan{T})"/>
    public static T Sum<T>(T[] values)
        where T : unmanaged, IBinaryInteger<T>, IAdditionOperators<T, T, T> => Sum((ReadOnlySpan<T>)values);

    /// <summary>
    ///     Returns the sum of a span of <see cref="IBinaryInteger{TSelf}"/>
    /// </summary>
    public static T SumRaw<T>(ReadOnlySpan<T> span)
        where T : unmanaged, IBinaryInteger<T>, IAdditionOperators<T, T, T>
    {
        unchecked
        {
            var sum = T.Zero;
            ref var current = ref MemoryMarshal.GetReference(span);
            ref var limit = ref Unsafe.Add(ref current, span.Length);

            while (Unsafe.IsAddressLessThan(ref current, ref limit))
            {
                sum += current;
                current = ref Unsafe.Add(ref current, 1);
            }

            return sum;
        }
    }

    /// <inheritdoc cref="SumRaw{T}(ReadOnlySpan{T})"/>
    public static T SumRaw<T>(T[] values)
        where T : unmanaged, IBinaryInteger<T>, IAdditionOperators<T, T, T> => Sum((ReadOnlySpan<T>)values);

    /// <summary>
    ///     Returns the average sum of a span of <see cref="int"/>
    /// </summary>
    public static double Avg(ReadOnlySpan<int> span)
    {
        if (span.IsEmpty) return 0;
        return Sum(span) / (double)span.Length;
    }

    /// <inheritdoc cref="Avg(ReadOnlySpan{int})"/>
    public static double Avg(int[] values) => Avg((ReadOnlySpan<int>)values);

    /// <summary>
    /// Return the next power of two number greater than <paramref name="number" />
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong NextPowerOfTwo(ulong number) =>
        number switch
        {
            <= 1 => 1UL,
            > 1UL << 63 => ulong.MaxValue,
            _ => 1UL << (64 - BitOperations.LeadingZeroCount(number - 1)),
        };

    /// <inheritdoc cref="NextPowerOfTwo(ulong)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long NextPowerOfTwo(long number) =>
        number switch
        {
            <= 1 => 1L,
            > 1L << 62 => long.MaxValue,
            _ => 1L << (64 - BitOperations.LeadingZeroCount((ulong)(number - 1))),
        };

    /// <inheritdoc cref="NextPowerOfTwo(ulong)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint NextPowerOfTwo(uint number) =>
        number switch
        {
            <= 1 => 1u,
            > 1u << 31 => uint.MaxValue,
            _ => 1u << (32 - BitOperations.LeadingZeroCount(number - 1)),
        };

    /// <inheritdoc cref="NextPowerOfTwo(ulong)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextPowerOfTwo(int number) =>
        number switch
        {
            <= 1 => 1,
            > 1 << 30 => int.MaxValue,
            _ => 1 << (32 - BitOperations.LeadingZeroCount((uint)(number - 1))),
        };
}

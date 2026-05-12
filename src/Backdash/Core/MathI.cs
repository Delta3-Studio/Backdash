using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backdash;

using Vector2I = (int X, int Y);

/// <summary>
///     Integer Math Helpers
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
    public static T SumSimple<T>(ReadOnlySpan<T> span)
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

    /// <inheritdoc cref="SumSimple{T}(ReadOnlySpan{T})"/>
    public static T SumSimple<T>(T[] values)
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
    ///    Returns the number of digits of value
    /// </summary>
    public static int CountDigits(int value) => (int)Math.Floor(Math.Log10(value) + 1);

    /// <summary>
    ///    Returns true if the number is even
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(int number) => (number & 1) is 0;

    /// <summary>
    ///    Returns true if the number is odd
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOdd(int number) => (number & 1) is not 0;

    /// <summary>
    ///    Returns 1 if value is true, 0 otherwise
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(bool value) => Unsafe.As<bool, byte>(ref value);

    /// <summary>
    ///    Returns false if value is 0, true otherwise
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBool(int value) => Unsafe.As<int, bool>(ref value);

    /// <summary>
    ///    Returns the square root of value
    /// </summary>
    public static int Sqrt(int value)
    {
        var result = 0;
        var bit = 1 << 30;
        value = Math.Abs(value);
        while (bit > value) bit >>= 2;

        while (bit is not 0)
        {
            if (value >= result + bit)
            {
                value -= result + bit;
                result = (result >> 1) + bit;
            }
            else
                result >>= 1;

            bit >>= 2;
        }

        return result;
    }

    /// <summary>
    ///    Returns the square root of value
    /// </summary>
    public static long Sqrt(long value)
    {
        var result = 0L;
        value = Math.Abs(value);
        var bit = 1L << 62;
        while (bit > value) bit >>= 2;

        while (bit is not 0)
        {
            if (value >= result + bit)
            {
                value -= result + bit;
                result = (result >> 1) + bit;
            }
            else
                result >>= 1;

            bit >>= 2;
        }

        return result;
    }

    /// <summary>
    ///   Remap a value between to ranges
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Remap(
        int value,
        int inMin, int inMax,
        int outMin, int outMax
    )
    {
        var inRange = inMax - inMin;
        if (inRange is 0) return 0;

        var num = (long)(value - inMin) * (outMax - outMin);
        var result = (num / inRange) + outMin;

        if (outMin < outMax)
        {
            if (result < outMin) return outMin;
            if (result > outMax) return outMax;
        }
        else
        {
            if (result > outMin) return outMin;
            if (result < outMax) return outMax;
        }

        return (int)result;
    }

    /// <summary>
    ///    Returns the value of applying the <paramref name="percentage"/> to <paramref name="value"/>
    ///    Percentage is defined from 0 to 100
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ApplyPercentage(int value, int percentage) => (int)(value * (long)percentage / 100L);

    /// <summary>
    ///    Finds how is the percentage of <paramref name="part"/> from <paramref name="total"/>
    ///    Percentage is defined from 0 to 100
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindPercentage(int part, int total)
    {
        if (total is 0) return 0;
        var scaled = (long)part * 100;

        if (scaled >= 0)
            scaled += total / 2;
        else
            scaled -= total / 2;

        return (scaled / total) switch
        {
            < 0 => 0,
            > 100 => 100,
            var result => (int)result,
        };
    }

    /// <summary>
    ///    Clamps the value between the limits of type <typeparamref name="T"/>
    /// </summary>
    /// <seealso cref="IMinMaxValue{TSelf}"/>
    public static int Clamp<T>(int value) where T : INumberBase<T>, IMinMaxValue<T> =>
        Math.Clamp(value, int.CreateSaturating(T.MinValue), int.CreateSaturating(T.MaxValue));

    /// <summary>
    ///    Converts the value into <typeparamref name="T"/> sarutating the result
    /// </summary>
    public static T Cast<T>(int value) where T : IBinaryInteger<T> => T.CreateSaturating(value);

    #region PowerOf2

    /// <summary>
    ///    Returns true if the value in power of two
    /// </summary>
    public static bool IsPowerOfTwo<T>(T value) where T : unmanaged, IBinaryInteger<T> =>
        value != T.Zero && (value & (value - T.One)) == T.Zero;

    /// <summary>
    ///    Return the next power of two number greater than <paramref name="number" />
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

    /// <summary>
    ///    Returns each power of 2 component from the value
    /// </summary>
    public static IEnumerable<int> DecomposePowerOfTwo(int value)
    {
        while (value is not 0)
        {
            var flag = value & -value;
            yield return flag;
            value ^= flag;
        }
    }

    #endregion

    #region Trig

    const int MaxAngleDeg = 360;
    static readonly short[] sinTable = new short[MaxAngleDeg];
    static readonly short[] cosTable = new short[MaxAngleDeg];

    static MathI() => InitTrigTables();

    static void InitTrigTables()
    {
        for (var i = 0; i < MaxAngleDeg; i++)
        {
            var rad = i * Math.PI / 180.0;
            sinTable[i] = (short)(Math.Sin(rad) * short.MaxValue);
            cosTable[i] = (short)(Math.Cos(rad) * short.MaxValue);
        }
    }

    /// <summary>
    ///    Calculates the angle for the coordinates x and y (degrees)
    /// </summary>
    public static int AngleDegrees(int x, int y)
    {
        if (x is 0 && y is 0) return 0;
        var absY = Math.Abs(y) + 1;
        var angle = x >= 0
            ? 45 - (45 * (x - absY) / (x + absY))
            : 135 - (45 * (x + absY) / (absY - x));

        return y < 0 ? 360 - angle : angle;
    }

    /// <summary>
    ///    Calculates the Sin value of the angle (degrees)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Sin(int angleDeg) => sinTable[angleDeg];

    /// <summary>
    ///    Calculates the Cos value of the angle (degrees)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Cos(int angleDeg) => cosTable[angleDeg];

    /// <summary>
    ///    Calculates the circle point for the <paramref name="angleDeg"/> (degrees) with size of  <paramref name="radius"/>
    /// </summary>
    public static Vector2I CirclePoint(int angleDeg, int radius)
    {
        angleDeg %= MaxAngleDeg;
        if (angleDeg < 0) angleDeg += MaxAngleDeg;
        var x = (int)(((long)Cos(angleDeg) * radius) >> 15);
        var y = (int)(((long)Sin(angleDeg) * radius) >> 15);
        return (x, y);
    }

    /// <summary>
    ///    Calculates the rotation of a point by <paramref name="degrees"/>
    /// </summary>
    public static Vector2I Rotate(in Vector2I v, int degrees)
    {
        degrees %= MaxAngleDeg;
        if (degrees < 0) degrees += MaxAngleDeg;
        var cos = Cos(degrees);
        var sin = Sin(degrees);
        var x = ((v.X * cos) - (v.Y * sin)) >> 14;
        var y = ((v.X * sin) + (v.Y * cos)) >> 14;
        return new(x, y);
    }

    #endregion
}

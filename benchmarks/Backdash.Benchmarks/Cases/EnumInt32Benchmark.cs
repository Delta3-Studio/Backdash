// ReSharper disable UnassignedField.Global

using System.Runtime.CompilerServices;

namespace Backdash.Benchmarks.Cases;

// [InProcess]
[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class EnumInt32Benchmark
{
    const int N = 10_000_000;
    readonly IntEnum[] data = new IntEnum[N];

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(42);
        random.Generate(data);
    }

    [Benchmark(OperationsPerInvoke = N)]
    public int Cast()
    {
        var result = 0;
        for (var i = 0; i < N; i++)
            result = (int)data[i];
        return result;
    }

    [Benchmark(OperationsPerInvoke = N)]
    public int HashCode()
    {
        var result = 0;
        for (var i = 0; i < N; i++)
            result = data[i].GetHashCode();
        return result;
    }

    [Benchmark(OperationsPerInvoke = N)]
    public int DirectUnsafe()
    {
        var result = 0;

        for (var i = 0; i < N; i++)
        {
            var value = data[i];
            result = Unsafe.As<IntEnum, int>(ref value);
        }

        return result;
    }

    [Benchmark(OperationsPerInvoke = N)]
    public int UnderlyingType()
    {
        var result = 0;
        for (var i = 0; i < N; i++)
            result = EnumEx.ToInt32UnderlyingType(data[i]);
        return result;
    }

    [Benchmark(OperationsPerInvoke = N)]
    public int EnumTypeConverter()
    {
        var result = 0;
        for (var i = 0; i < N; i++)
            result = EnumType.ToInt32(data[i]);
        return result;
    }
}

public static class EnumEx
{
    public static int ToInt32UnderlyingType<TEnum>(TEnum value) where TEnum : unmanaged, Enum
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(int)) return Unsafe.As<TEnum, int>(ref value);
        if (type == typeof(byte)) return Unsafe.As<TEnum, byte>(ref value);
        if (type == typeof(sbyte)) return Unsafe.As<TEnum, sbyte>(ref value);
        if (type == typeof(short)) return Unsafe.As<TEnum, short>(ref value);
        if (type == typeof(ushort)) return Unsafe.As<TEnum, ushort>(ref value);
        if (type == typeof(uint)) return checked((int)Unsafe.As<TEnum, uint>(ref value));
        if (type == typeof(long)) return checked((int)Unsafe.As<TEnum, long>(ref value));
        if (type == typeof(ulong)) return checked((int)Unsafe.As<TEnum, ulong>(ref value));
        throw new NotSupportedException();
    }
}

public enum IntEnum
{
    Zero = 0,
    Min = int.MinValue,
    Max = int.MaxValue,

    A1 = 1 << 0,
    A2 = 1 << 1,
    A3 = 1 << 2,
    A4 = 1 << 3,
    A5 = 1 << 4,
    A6 = 1 << 5,
    A7 = 1 << 6,
    A8 = 1 << 7,

    B1 = 1 << 8,
    B2 = 1 << 9,
    B3 = 1 << 10,
    B4 = 1 << 11,
    B5 = 1 << 12,
    B6 = 1 << 13,
    B7 = 1 << 14,
    B8 = 1 << 15,

    C1 = 1 << 16,
    C2 = 1 << 17,
    C3 = 1 << 18,
    C4 = 1 << 19,
    C5 = 1 << 20,
    C6 = 1 << 21,
    C7 = 1 << 22,
    C8 = 1 << 23,

    D1 = 1 << 24,
    D2 = 1 << 25,
    D3 = 1 << 26,
    D4 = 1 << 27,
    D5 = 1 << 28,
    D6 = 1 << 29,
    D7 = 1 << 30,
}

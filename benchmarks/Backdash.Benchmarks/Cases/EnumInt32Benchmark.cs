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
}

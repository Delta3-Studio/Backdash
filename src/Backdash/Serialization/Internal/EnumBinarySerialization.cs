using System.Runtime.CompilerServices;
using Backdash.Serialization;

namespace Backdash;

static class EnumBinarySerialization<TEnum> where TEnum : unmanaged, Enum
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(ref readonly BinaryBufferWriter writer, TEnum value)
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(byte)) writer.WriteAsByte(in value);
        else if (type == typeof(sbyte)) writer.WriteAsSByte(in value);
        else if (type == typeof(short)) writer.WriteAsInt16(in value);
        else if (type == typeof(ushort)) writer.WriteAsUInt16(in value);
        else if (type == typeof(int)) writer.WriteAsInt32(in value);
        else if (type == typeof(uint)) writer.WriteAsUInt32(in value);
        else if (type == typeof(long)) writer.WriteAsInt64(in value);
        else if (type == typeof(ulong)) writer.WriteAsUInt64(in value);
        else throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(ref readonly BinarySpanWriter writer, TEnum value)
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(byte)) writer.WriteAsByte(in value);
        else if (type == typeof(sbyte)) writer.WriteAsSByte(in value);
        else if (type == typeof(short)) writer.WriteAsInt16(in value);
        else if (type == typeof(ushort)) writer.WriteAsUInt16(in value);
        else if (type == typeof(int)) writer.WriteAsInt32(in value);
        else if (type == typeof(uint)) writer.WriteAsUInt32(in value);
        else if (type == typeof(long)) writer.WriteAsInt64(in value);
        else if (type == typeof(ulong)) writer.WriteAsUInt64(in value);
        else throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum Read(ref readonly BinaryBufferReader reader)
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(byte)) return reader.ReadAsByte<TEnum>();
        if (type == typeof(sbyte)) return reader.ReadAsSByte<TEnum>();
        if (type == typeof(short)) return reader.ReadAsInt16<TEnum>();
        if (type == typeof(ushort)) return reader.ReadAsUInt16<TEnum>();
        if (type == typeof(int)) return reader.ReadAsInt32<TEnum>();
        if (type == typeof(uint)) return reader.ReadAsUInt32<TEnum>();
        if (type == typeof(long)) return reader.ReadAsInt64<TEnum>();
        if (type == typeof(ulong)) return reader.ReadAsUInt64<TEnum>();

        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref readonly BinaryBufferReader reader, ref TEnum value)
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(byte)) reader.ReadAsByte(ref value);
        else if (type == typeof(sbyte)) reader.ReadAsSByte(ref value);
        else if (type == typeof(short)) reader.ReadAsInt16(ref value);
        else if (type == typeof(ushort)) reader.ReadAsUInt16(ref value);
        else if (type == typeof(int)) reader.ReadAsInt32(ref value);
        else if (type == typeof(uint)) reader.ReadAsUInt32(ref value);
        else if (type == typeof(long)) reader.ReadAsInt64(ref value);
        else if (type == typeof(ulong)) reader.ReadAsUInt64(ref value);
        else throw new NotSupportedException();
    }
}


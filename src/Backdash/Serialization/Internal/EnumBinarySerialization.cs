using System.Runtime.CompilerServices;
using Backdash.Serialization;

namespace Backdash;

static class EnumBinarySerialization<TEnum> where TEnum : unmanaged, Enum
{
    static readonly BufferWriterFn<TEnum> bufferWriter = CreateBufferWriter();
    static readonly SpanWriterFn<TEnum> spanWriter = CreateSpanWriter();
    static readonly ValueReaderFn<TEnum> valueReader = CreateValueReader();
    static readonly RefReaderFn<TEnum> refReader = CreateRefReader();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(ref readonly BinaryBufferWriter writer, TEnum value) => bufferWriter(in writer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(ref readonly BinarySpanWriter writer, TEnum value) => spanWriter(in writer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum Read(ref readonly BinaryBufferReader reader) => valueReader(in reader);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref readonly BinaryBufferReader reader, ref TEnum value) => refReader(in reader, ref value);

    static BufferWriterFn<TEnum> CreateBufferWriter() => EnumType<TEnum>.TypeCode switch
    {
        EnumTypeCode.Byte => static (ref readonly w, v) => w.WriteAsByte(in v),
        EnumTypeCode.SByte => static (ref readonly w, v) => w.WriteAsSByte(in v),
        EnumTypeCode.Int16 => static (ref readonly w, v) => w.WriteAsInt16(in v),
        EnumTypeCode.UInt16 => static (ref readonly w, v) => w.WriteAsUInt16(in v),
        EnumTypeCode.Int32 => static (ref readonly w, v) => w.WriteAsInt32(in v),
        EnumTypeCode.UInt32 => static (ref readonly w, v) => w.WriteAsUInt32(in v),
        EnumTypeCode.Int64 => static (ref readonly w, v) => w.WriteAsInt64(in v),
        EnumTypeCode.UInt64 => static (ref readonly w, v) => w.WriteAsUInt64(in v),
        _ => throw new NotSupportedException(),
    };

    static SpanWriterFn<TEnum> CreateSpanWriter() => EnumType<TEnum>.TypeCode switch
    {
        EnumTypeCode.Byte => static (ref readonly w, v) => w.WriteAsByte(in v),
        EnumTypeCode.SByte => static (ref readonly w, v) => w.WriteAsSByte(in v),
        EnumTypeCode.Int16 => static (ref readonly w, v) => w.WriteAsInt16(in v),
        EnumTypeCode.UInt16 => static (ref readonly w, v) => w.WriteAsUInt16(in v),
        EnumTypeCode.Int32 => static (ref readonly w, v) => w.WriteAsInt32(in v),
        EnumTypeCode.UInt32 => static (ref readonly w, v) => w.WriteAsUInt32(in v),
        EnumTypeCode.Int64 => static (ref readonly w, v) => w.WriteAsInt64(in v),
        EnumTypeCode.UInt64 => static (ref readonly w, v) => w.WriteAsUInt64(in v),
        _ => throw new NotSupportedException(),
    };

    static ValueReaderFn<TEnum> CreateValueReader() => EnumType<TEnum>.TypeCode switch
    {
        EnumTypeCode.Byte => static (ref readonly r) => r.ReadAsByte<TEnum>(),
        EnumTypeCode.SByte => static (ref readonly r) => r.ReadAsSByte<TEnum>(),
        EnumTypeCode.Int16 => static (ref readonly r) => r.ReadAsInt16<TEnum>(),
        EnumTypeCode.UInt16 => static (ref readonly r) => r.ReadAsUInt16<TEnum>(),
        EnumTypeCode.Int32 => static (ref readonly r) => r.ReadAsInt32<TEnum>(),
        EnumTypeCode.UInt32 => static (ref readonly r) => r.ReadAsUInt32<TEnum>(),
        EnumTypeCode.Int64 => static (ref readonly r) => r.ReadAsInt64<TEnum>(),
        EnumTypeCode.UInt64 => static (ref readonly r) => r.ReadAsUInt64<TEnum>(),
        _ => throw new NotSupportedException(),
    };

    static RefReaderFn<TEnum> CreateRefReader() => EnumType<TEnum>.TypeCode switch
    {
        EnumTypeCode.Byte => static (ref readonly r, ref v) => r.ReadAsByte(ref v),
        EnumTypeCode.SByte => static (ref readonly r, ref v) => r.ReadAsSByte(ref v),
        EnumTypeCode.Int16 => static (ref readonly r, ref v) => r.ReadAsInt16(ref v),
        EnumTypeCode.UInt16 => static (ref readonly r, ref v) => r.ReadAsUInt16(ref v),
        EnumTypeCode.Int32 => static (ref readonly r, ref v) => r.ReadAsInt32(ref v),
        EnumTypeCode.UInt32 => static (ref readonly r, ref v) => r.ReadAsUInt32(ref v),
        EnumTypeCode.Int64 => static (ref readonly r, ref v) => r.ReadAsInt64(ref v),
        EnumTypeCode.UInt64 => static (ref readonly r, ref v) => r.ReadAsUInt64(ref v),
        _ => throw new NotSupportedException(),
    };

    delegate void SpanWriterFn<in T>(ref readonly BinarySpanWriter writer, T value) where T : unmanaged, Enum;

    delegate void BufferWriterFn<in T>(ref readonly BinaryBufferWriter writer, T value) where T : unmanaged, Enum;

    delegate T ValueReaderFn<out T>(ref readonly BinaryBufferReader reader) where T : unmanaged, Enum;

    delegate void RefReaderFn<T>(ref readonly BinaryBufferReader reader, ref T value) where T : unmanaged, Enum;
}

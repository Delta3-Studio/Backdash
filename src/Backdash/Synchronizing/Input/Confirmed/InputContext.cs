using System.Buffers;
using System.Runtime.CompilerServices;
using Backdash.Network;
using Backdash.Options;
using Backdash.Serialization;

namespace Backdash.Synchronizing.Input.Confirmed;

/// <summary>
///   Input data helpers
/// </summary>
public sealed class InputContext<TInput> where TInput : unmanaged
{
    readonly NetcodeOptions options;

    /// <summary>
    /// <see cref="IBinarySerializer{T}"/> serializer implementation for a single <typeparamref name="TInput"/>.
    /// </summary>
    public IBinarySerializer<TInput> PlayerInputSerializer { get; }

    /// <summary>
    /// <see cref="IBinarySerializer{T}"/> serializer implementation for a confirmed (all-players) <typeparamref name="TInput" />
    /// </summary>
    public IBinarySerializer<ConfirmedInputs<TInput>> Serializer { get; }

    /// <summary>
    /// Struct size of <typeparamref name="TInput" />
    /// </summary>
    public int PlayerInputSize { get; }

    /// <summary>
    /// Struct size of <see cref="ConfirmedInputs{TInput}"/>
    /// </summary>
    public int ConfirmedInputSize { get; }

    /// <inheritdoc cref="NetcodeOptions.NumberOfPlayers"/>
    public int NumberOfPlayers => options.NumberOfPlayers;

    /// <inheritdoc cref="ProtocolOptions.SerializationEndianness"/>
    public Endianness Endianness => options.Protocol.SerializationEndianness;

    /// <summary>
    /// Input type code
    /// </summary>
    public TypeCode InputTypeCode { get; }

    internal InputContext(NetcodeOptions options,
        IBinarySerializer<TInput> inputSerializer,
        IBinarySerializer<ConfirmedInputs<TInput>> serializer
    )
    {
        this.options = options;
        Serializer = serializer;
        PlayerInputSerializer = inputSerializer;
        InputTypeCode = Type.GetTypeCode(typeof(TInput));
        PlayerInputSize = Unsafe.SizeOf<TInput>();
        ConfirmedInputSize = Unsafe.SizeOf<ConfirmedInputs<TInput>>();
    }

    /// <summary>
    /// Write single input into a buffer
    /// </summary>
    public void Write(ArrayBufferWriter<byte> bufferWriter, in TInput input)
    {
        var span = bufferWriter.GetSpan(PlayerInputSize);
        var written = PlayerInputSerializer.Serialize(in input, span);
        bufferWriter.Advance(written);
    }

    /// <summary>
    /// Write a confirmed input into a buffer
    /// </summary>
    public void Write(ArrayBufferWriter<byte> bufferWriter, in ConfirmedInputs<TInput> inputs)
    {
        var span = bufferWriter.GetSpan(PlayerInputSize);
        var written = Serializer.Serialize(in inputs, span);
        bufferWriter.Advance(written);
    }

    /// <summary>
    /// Write single input into a span
    /// </summary>
    public int Write(Span<byte> span, in TInput input) => PlayerInputSerializer.Serialize(in input, span);

    /// <summary>
    /// Write a confirmed input into a span
    /// </summary>
    public int Write(Span<byte> span, in ConfirmedInputs<TInput> input) => Serializer.Serialize(in input, span);

    /// <summary>
    /// Reads single input
    /// </summary>
    public int Read(ReadOnlySpan<byte> span, ref TInput input) => PlayerInputSerializer.Deserialize(span, ref input);

    /// <summary>
    /// Reads a confirmed input into
    /// </summary>
    public int Read(ReadOnlySpan<byte> span, ref ConfirmedInputs<TInput> input) =>
        Serializer.Deserialize(span, ref input);

    /// <summary>
    /// Creates new <see cref="BinarySpanWriter"/>
    /// </summary>
    public BinarySpanWriter GetSpanWriter(scoped in Span<byte> buffer, ref int offset) =>
        new(in buffer, ref offset, Endianness);

    /// <summary>
    /// Creates new <see cref="BinaryBufferWriter"/>
    /// </summary>
    public BinaryBufferWriter GetWriter(ArrayBufferWriter<byte> buffer) => new(buffer, Endianness);

    /// <summary>
    /// Creates new <see cref="BinaryBufferReader"/>
    /// </summary>
    public BinaryBufferReader GetReader(ReadOnlySpan<byte> buffer, ref int offset) =>
        new(buffer, ref offset, Endianness);
}

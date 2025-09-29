using Backdash.Core;

namespace Backdash.Synchronizing.Input;

record struct GameInput<T>(T Data, Frame Frame) : IUtf8SpanFormattable where T : struct
{
    public Frame Frame = Frame;
    public T Data = Data;
    public GameInput(Frame frame) : this(default, frame) { }
    public GameInput(T data) : this(data, Frame.Null) { }
    public GameInput() : this(default, Frame.Null) { }
    public void IncrementFrame() => Frame = Frame.Next();
    public void ResetFrame() => Frame = Frame.Null;
    public void Erase() => Data = default;

    public readonly bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        bytesWritten = 0;
        Utf8StringBuilder writer = new(in utf8Destination, ref bytesWritten);
        return writer.Write("Input{Frame: "u8)
               && writer.Write(Frame.Number)
               && writer.Write("}"u8);
    }
}

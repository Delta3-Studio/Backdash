using System.Diagnostics;
using Backdash.Core;
using Backdash.Data;

namespace Backdash.Serialization.Internal;

static class DeltaXorRle
{
    [DebuggerDisplay("{ToString()}")]
    public ref struct Encoder(Span<byte> output, Span<byte> lastBuffer)
    {
        BitOffsetWriter writer = new(output);
        readonly Span<byte> last = lastBuffer;
        public int Count { get; private set; } = 0;

        public readonly ushort BitOffset => writer.Offset;

        public bool Write(ReadOnlySpan<byte> current)
        {
            ThrowIf.Assert(current.Length <= last.Length);
            if (!Mem.EqualBytes(current, last, truncate: true))
            {
                ReadOnlyBitVector currentBits = new(in current);
                ReadOnlyBitVector lastBits = new(last);
                var lastOffset = writer.Offset;
                try
                {
                    for (var i = 0; i < currentBits.BitCount; i++)
                    {
                        if (currentBits[i] == lastBits[i])
                            continue;
                        writer.SetNext();
                        if (currentBits[i])
                            writer.SetNext();
                        else
                            writer.ClearNext();
                        writer.WriteNibble(i);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    writer.Offset = lastOffset;
                    return false;
                }
            }

            writer.ClearNext();
            current.CopyTo(last);
            Count++;
            return true;
        }

        public override readonly string ToString() =>
            $"{{ Count: {Count}, Writer: {writer.ToString()} }}";
    }

    [DebuggerDisplay("{ToString()}")]
    public ref struct Decoder(Span<byte> buffer, ushort bitCount)
    {
        BitOffsetWriter writer = new(buffer);

        public bool Skip(int count)
        {
            if (count <= 0) return true;
            var lastOffset = writer.Offset;
            try
            {
                while (writer.Offset < bitCount)
                {
                    var skipNext = count-- > 0;
                    if (skipNext)
                    {
                        while (writer.Read())
                        {
                            writer.Read();
                            writer.ReadNibble();
                        }

                        continue;
                    }

                    ThrowIf.Assert(writer.Offset <= bitCount);
                    return true;
                }
            }
            catch (IndexOutOfRangeException)
            {
                writer.Offset = lastOffset;
                return false;
            }

            return false;
        }

        public bool Read(Span<byte> output)
        {
            BitVector bits = new(in output);
            if (writer.Offset >= bitCount || writer.Completed)
                return false;
            var lastOffset = writer.Offset;
            try
            {
                while (writer.Read())
                {
                    var on = writer.Read();
                    var button = writer.ReadNibble();
                    if (on)
                        bits.Set(button);
                    else
                        bits.Clear(button);
                }
            }
            catch (IndexOutOfRangeException)
            {
                writer.Offset = lastOffset;
                return false;
            }

            ThrowIf.Assert(writer.Offset <= bitCount);
            return true;
        }

        public override readonly string ToString() =>
            $"{{ Offset: {writer.Offset}/{bitCount}, " +
            $"Read: {Mem.GetBitString(writer.Buffer[..writer.Offset])}," +
            $"Pending: {Mem.GetBitString(writer.Buffer[writer.Offset..])}," +
            $"}}";
    }
}

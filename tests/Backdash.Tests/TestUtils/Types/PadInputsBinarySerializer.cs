using Backdash.Serialization;

namespace Backdash.Tests.TestUtils.Types;

class PadInputsBinarySerializer : BaseBinarySerializer<PadInputs>
{
    protected override void Serialize(in BinarySpanWriter writer, in PadInputs data)
    {
        writer.Write((short)data.Buttons);
        writer.Write(data.LeftTrigger);
        writer.Write(data.RightTrigger);

        writer.Write(data.LeftAxis.X);
        writer.Write(data.LeftAxis.Y);

        writer.Write(data.RightAxis.X);
        writer.Write(data.RightAxis.Y);
    }

    protected override void Deserialize(in BinaryBufferReader reader, ref PadInputs result)
    {
        result.Buttons = (PadInputs.PadButtons)reader.ReadInt16();
        result.LeftTrigger = reader.ReadByte();
        result.RightTrigger = reader.ReadByte();

        result.LeftAxis.X = reader.ReadSByte();
        result.LeftAxis.Y = reader.ReadSByte();

        result.RightAxis.X = reader.ReadSByte();
        result.RightAxis.Y = reader.ReadSByte();
    }
}

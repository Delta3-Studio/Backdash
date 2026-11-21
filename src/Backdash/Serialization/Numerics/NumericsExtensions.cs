using System.Numerics;

namespace Backdash.Serialization.Numerics;

/// <summary>
///     Serialization extensions for System
///     <see cref="System.Numerics" />
/// </summary>
public static class NumericsExtensions
{
    extension(ref readonly BinaryBufferReader reader)
    {
        /// <summary>Reads single <see cref="Vector2" /> from the buffer.</summary>
        public Vector2 ReadVector2()
        {
            var x = reader.ReadFloat();
            var y = reader.ReadFloat();
            return new(x, y);
        }

        /// <inheritdoc cref="ReadVector2(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public Vector2? ReadNullableVector2() =>
            reader.ReadBoolean() ? reader.ReadVector2() : null;

        /// <summary>Reads single <see cref="Vector3" /> from the buffer.</summary>
        public Vector3 ReadVector3()
        {
            var x = reader.ReadFloat();
            var y = reader.ReadFloat();
            var z = reader.ReadFloat();
            return new(x, y, z);
        }

        /// <inheritdoc cref="ReadVector3(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public Vector3? ReadNullableVector3() =>
            reader.ReadBoolean() ? reader.ReadVector3() : null;

        /// <summary>Reads single <see cref="Vector4" /> from the buffer.</summary>
        public Vector4 ReadVector4()
        {
            var x = reader.ReadFloat();
            var y = reader.ReadFloat();
            var z = reader.ReadFloat();
            var w = reader.ReadFloat();
            return new(x, y, z, w);
        }

        /// <inheritdoc cref="ReadVector4(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public Vector4? ReadNullableVector4() =>
            reader.ReadBoolean() ? reader.ReadVector4() : null;

        /// <summary>Reads single <see cref="Quaternion" /> from the buffer.</summary>
        public Quaternion ReadQuaternion()
        {
            var x = reader.ReadFloat();
            var y = reader.ReadFloat();
            var z = reader.ReadFloat();
            var w = reader.ReadFloat();
            return new(x, y, z, w);
        }

        /// <inheritdoc cref="ReadQuaternion(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public Quaternion? ReadNullableQuaternion() =>
            reader.ReadBoolean() ? reader.ReadQuaternion() : null;

        /// <inheritdoc cref="ReadVector2(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Vector2 value)
        {
            value.X = reader.ReadFloat();
            value.Y = reader.ReadFloat();
        }

        /// <inheritdoc cref="ReadVector2(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Vector2? value) =>
            value = reader.ReadNullableVector2();

        /// <inheritdoc cref="ReadVector3(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Vector3 value)
        {
            value.X = reader.ReadFloat();
            value.Y = reader.ReadFloat();
            value.Z = reader.ReadFloat();
        }

        /// <inheritdoc cref="ReadVector3(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Vector3? value) =>
            value = reader.ReadNullableVector3();

        /// <inheritdoc cref="ReadVector4(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Vector4 value)
        {
            value.X = reader.ReadFloat();
            value.Y = reader.ReadFloat();
            value.Z = reader.ReadFloat();
            value.W = reader.ReadFloat();
        }

        /// <inheritdoc cref="ReadVector4(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Vector4? value) =>
            value = reader.ReadNullableVector4();

        /// <inheritdoc cref="ReadQuaternion(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Quaternion value)
        {
            value.X = reader.ReadFloat();
            value.Y = reader.ReadFloat();
            value.Z = reader.ReadFloat();
            value.W = reader.ReadFloat();
        }

        /// <inheritdoc cref="ReadQuaternion(ref readonly Backdash.Serialization.BinaryBufferReader)" />
        public void Read(ref Quaternion? value) =>
            value = reader.ReadNullableQuaternion();
    }

    extension(ref readonly BinarySpanWriter writer)
    {
        /// <summary>Writes single <see cref="Vector2" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        /// <summary>Writes single <see cref="Vector3" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        /// <summary>Writes single <see cref="Vector4" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Vector4 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        /// <summary>Writes single <see cref="Quaternion" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }
    }

    extension(ref readonly BinaryBufferWriter writer)
    {
        /// <summary>Writes single <see cref="Vector2" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        /// <summary>Writes single <see cref="Vector3" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        /// <summary>Writes single <see cref="Vector4" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Vector4 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        /// <summary>Writes single <see cref="Quaternion" /> <paramref name="value" /> into buffer.</summary>
        public void Write(in Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        /// <inheritdoc cref="Write(ref readonly BinarySpanWriter,in System.Numerics.Vector2)" />
        public void Write(in Vector2? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(in Nullable.GetValueRefOrDefaultRef(in value));
        }

        /// <inheritdoc cref="Write(ref readonly BinarySpanWriter,in System.Numerics.Vector3)" />
        public void Write(in Vector3? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(in Nullable.GetValueRefOrDefaultRef(in value));
        }

        /// <inheritdoc cref="Write(ref readonly BinarySpanWriter,in System.Numerics.Vector4)" />
        public void Write(in Vector4? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(in Nullable.GetValueRefOrDefaultRef(in value));
        }

        /// <inheritdoc cref="Write(ref readonly BinarySpanWriter,in System.Numerics.Quaternion)" />
        public void Write(in Quaternion? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(in Nullable.GetValueRefOrDefaultRef(in value));
        }
    }
}

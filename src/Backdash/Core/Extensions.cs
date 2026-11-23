using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using Backdash.Core;

namespace Backdash;

/// <summary>
/// Library extensions
/// </summary>
public static class NetcodeExtensions
{
    internal static void PushFirst<T>(this Queue<T> queue, in T value)
    {
        var count = queue.Count;
        queue.Enqueue(value);
        for (var i = 0; i < count; i++)
            queue.Enqueue(queue.Dequeue());
    }

    /// <summary>
    /// Returns a writable <see cref="Span{T}"/> with the specified <paramref name="length"/>.
    /// </summary>
    public static Span<T> ReadSpan<T>(this ArrayBufferWriter<T> @this, int length) =>
        @this.GetSpan(length)[..length];

    /// <summary>
    /// Advance and returns a <see cref="Span{T}"/> by the specified <paramref name="length"/>.
    /// </summary>
    /// <seealso cref="ReadSpan"/>
    public static Span<T> AllocateSpan<T>(this ArrayBufferWriter<T> @this, int length)
    {
        var result = @this.ReadSpan(length);
        @this.Advance(length);
        return result;
    }

    extension(Random @this)
    {
        /// <summary>
        /// Return random gaussian numbers.
        /// </summary>
        public void NextGaussian(out double first, out double second)
        {
            double u, v, s;
            do
            {
                u = (2.0 * @this.NextDouble()) - 1.0;
                v = (2.0 * @this.NextDouble()) - 1.0;
                s = (u * u) + (v * v);
            } while (s is >= 1.0 or 0.0);

            var fac = Math.Sqrt(-2.0 * Math.Log(s) / s);
            first = u * fac;
            second = v * fac;
        }

        /// <summary>
        /// Return random gaussian number.
        /// </summary>
        public double NextGaussian()
        {
            @this.NextGaussian(out double first, out _);
            return first;
        }

        /// <summary>
        /// Returns a random <see cref="bool"/>
        /// </summary>
        public bool NextBool() => @this.Next(2) is 0;

        /// <summary>
        /// Returns a random <see cref="bool"/> using a success percentage defined by the parameter <paramref name="percentage"/>.
        /// The <paramref name="percentage"/> must be in between 0 and 1.
        /// </summary>
        public bool NextBool(double percentage) =>
            @this.NextDouble() < Math.Clamp(percentage, 0.0, 1.0);

        /// <inheritdoc cref="System.Random.Shuffle{T}(T[])"/>
        public void Shuffle<T>(List<T> values) =>
            @this.Shuffle(CollectionsMarshal.AsSpan(values));

        /// <summary>
        /// Fill <paramref name="value"/> with random values.
        /// </summary>
        public void Generate<T>(scoped ref T value) where T : unmanaged
        {
            var bytes = Mem.AsBytes(ref value);
            @this.NextBytes(bytes);
        }

        /// <summary>
        /// Return a random <typeparamref name="T"/> value.
        /// </summary>
        public T Generate<T>() where T : unmanaged
        {
            T result = new();
            @this.Generate(ref result);
            return result;
        }

        /// <summary>
        /// Fill <paramref name="buffer"/> with random <typeparamref name="T"/> values.
        /// </summary>
        public void Generate<T>(Span<T> buffer) where T : unmanaged =>
            @this.NextBytes(MemoryMarshal.AsBytes(buffer));

        /// <summary>
        /// Fill <paramref name="buffer"/> with random <typeparamref name="T"/> values.
        /// </summary>
        public void Generate<T>(T[] buffer) where T : unmanaged =>
            @this.Generate(buffer.AsSpan());

        /// <summary>
        /// Return an  array with random <typeparamref name="T"/> values.
        /// </summary>
        public T[] Generate<T>(int count) where T : unmanaged
        {
            var result = new T[count];
            @this.Generate(result.AsSpan());
            return result;
        }
    }

    extension(ref Utf8JsonReader reader)
    {
        /// <summary>Reads a <see cref="Frame"/> value from <see cref="Utf8JsonReader"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frame GetFrame() => new(reader.GetInt32());

        /// <summary>Reads a <see cref="FrameSpan"/> value from <see cref="Utf8JsonReader"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameSpan GetFrameSpan() => new(reader.GetInt32());
    }

    extension(Utf8JsonWriter writer)
    {
        /// <summary>Writes a <see cref="Frame"/> value to <see cref="Utf8JsonWriter"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFrame(Frame value) =>
            writer.WriteNumberValue(value.Number);

        /// <summary>Writes a <see cref="FrameSpan"/> value to <see cref="Utf8JsonWriter"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFrameSpan(FrameSpan value) =>
            writer.WriteNumberValue(value.Frames);
    }
}

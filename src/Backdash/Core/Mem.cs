using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Backdash.Data;

namespace Backdash.Core;

static class Mem
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear(Span<byte> bytes) => bytes.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> AsSpan<T>(ref readonly T data) where T : unmanaged => new(in data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> AsBytes<T>(ref T data) where T : unmanaged => MemoryMarshal.AsBytes(new Span<T>(ref data));

    public static bool EqualBytes(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right, bool truncate = false)
    {
        if (!truncate) return right.SequenceEqual(left);
        var minLength = Math.Min(left.Length, right.Length);
        return right[..minLength].SequenceEqual(left[..minLength]);
    }

    public static byte[] AllocatePinnedArray(int size)
    {
        var buffer = GC.AllocateArray<byte>(length: size, pinned: true);
        Array.Clear(buffer);
        return buffer;
    }

    public static Memory<byte> AllocatePinnedMemory(int size)
    {
        var buffer = AllocatePinnedArray(size);
        return MemoryMarshal.CreateFromPinnedArray(buffer, 0, buffer.Length);
    }

    public static int GetHashCode<T>(ReadOnlySpan<T> values) where T : unmanaged
    {
        HashCode hash = new();
        hash.AddBytes(MemoryMarshal.AsBytes(values));
        return hash.ToHashCode();
    }

    public static bool IsUnsigned<T>() where T : IBinaryInteger<T>, IMinMaxValue<T> =>
        T.IsZero(T.MinValue);

    public static string GetBitString(
        ReadOnlySpan<byte> bytes,
        bool trimRightZeros = true,
        int bytePad = ByteSize.ByteToBits
    )
    {
        StringBuilder builder = new(bytes.Length * bytePad * sizeof(char));
        const char byteSep = '-';
        Span<char> binary = stackalloc char[bytePad];
        for (var i = 0; i < bytes.Length; i++)
        {
            if (i > 0)
                builder.Append(byteSep);
            binary.Clear();
            var base10 = bytes[i];
            var binSize = Math.Clamp((int)Math.Ceiling(Math.Log(base10 + 1, 2)), 1, bytePad);
            var padSize = bytePad - binSize;
            binary[..padSize].Fill('0');
            for (var j = binSize - 1; j >= 0; j--)
            {
                binary[padSize + j] = base10 % 2 is 0 ? '0' : '1';
                base10 /= 2;
            }

            builder.Append(binary);
        }

        if (!trimRightZeros)
            return builder.ToString();
        int lastNonZero;
        int lastByteSep = builder.Length;
        for (lastNonZero = builder.Length - 1; lastNonZero >= 0; lastNonZero--)
        {
            if (builder[lastNonZero] is byteSep)
                lastByteSep = lastNonZero;
            if (builder[lastNonZero] is not ('0' or byteSep))
                break;
        }

        lastNonZero = Math.Max(lastNonZero, lastByteSep);
        var trimmed = builder.Remove(lastNonZero, builder.Length - lastNonZero);
        return trimmed.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteSpan<T>(Span<byte> buffer, ReadOnlySpan<T> data) where T : unmanaged
    {
        var values = MemoryMarshal.AsBytes(data);
        values.CopyTo(buffer);
        return values.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSpan<T>(ArrayBufferWriter<byte> buffer, ReadOnlySpan<T> data) where T : unmanaged =>
        buffer.Write(MemoryMarshal.AsBytes(data));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadSpan<T>(ReadOnlySpan<byte> buffer, in Span<T> data) where T : unmanaged
    {
        var values = MemoryMarshal.AsBytes(data);
        buffer[..values.Length].CopyTo(values);
        return values.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> CastSpan<T>(Span<byte> buffer, int size, out int sizeBytes)
        where T : unmanaged
    {
        sizeBytes = Unsafe.SizeOf<T>() * size;
        return MemoryMarshal.Cast<byte, T>(buffer[..sizeBytes]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> CastSpan<T>(ArrayBufferWriter<byte> buffer, int size)
        where T : unmanaged
    {
        var sizeBytes = Unsafe.SizeOf<T>() * size;
        return MemoryMarshal.Cast<byte, T>(buffer.AllocateSpan(sizeBytes));
    }
}

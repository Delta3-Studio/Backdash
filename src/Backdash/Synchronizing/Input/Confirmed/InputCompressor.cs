using System.Buffers;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backdash.Synchronizing.Input.Confirmed;

/// <summary>
///   Compress/Decompress list of inputs
/// </summary>
public static class InputCompressor<TInput> where TInput : unmanaged
{
    /// <summary>
    /// Return all input bytes compressed with Deflate
    /// </summary>
    public static byte[] Compress(
        InputContext<TInput> context,
        ReadOnlySpan<ConfirmedInputs<TInput>> inputs
    )
    {
        if (inputs.IsEmpty) return [];
        ArrayBufferWriter<byte> writer = new(context.ConfirmedInputSize * inputs.Length);
        ref var current = ref MemoryMarshal.GetReference(inputs);
        ref var limit = ref Unsafe.Add(ref current, inputs.Length);
        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            context.Write(writer, in current);
            current = ref Unsafe.Add(ref current, 1)!;
        }

        using MemoryStream compressed = new(writer.WrittenCount);
        using (DeflateStream compressor = new(compressed, CompressionMode.Compress))
        {
            compressed.Seek(0, SeekOrigin.Begin);
            compressor.Write(writer.WrittenSpan);
        }

        return compressed.ToArray();
    }

    /// <summary>
    /// Decompress input bytes
    /// </summary>
    public static IReadOnlyList<ConfirmedInputs<TInput>> Decompress(
        InputContext<TInput> context,
        MemoryStream inputBytes
    )
    {
        if (inputBytes is null or { Length: 0 }) return [];
        var buffer = new byte[context.ConfirmedInputSize];
        List<ConfirmedInputs<TInput>> result = [];
        ConfirmedInputs<TInput> confirmedInput = new();

        inputBytes.Seek(0, SeekOrigin.Begin);
        while (inputBytes.Read(buffer) > 0)
        {
            context.Read(buffer, ref confirmedInput);
            result.Add(confirmedInput);
        }

        return result.AsReadOnly();
    }

    /// <inheritdoc cref="Decompress(InputContext{TInput}, MemoryStream)"/>
    public static IReadOnlyList<ConfirmedInputs<TInput>> Decompress(
        InputContext<TInput> context,
        ReadOnlySpan<byte> bytes
    )
    {
        using var stream = DecompressStream(bytes);
        return Decompress(context, stream);
    }

    /// <summary>
    /// Read inputs stream
    /// </summary>
    public static MemoryStream DecompressStream(ReadOnlySpan<byte> bytes)
    {
        MemoryStream inputStream = new();
        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                UnmanagedMemoryStream stream = new(ptr, bytes.Length);
                using DeflateStream decompressor = new(stream, CompressionMode.Decompress);
                decompressor.CopyTo(inputStream);
            }
        }

        return inputStream;
    }
}

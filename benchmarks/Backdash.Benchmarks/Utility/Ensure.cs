using System.Buffers;

static class Ensure
{
    public static void Equal<T>(ReadOnlySpan<T> oldData, ReadOnlySpan<T> newData, string operation)
    {
        if (!oldData.SequenceEqual(newData))
            throw new InvalidOperationException($"{operation} serialization mismatch.");
    }

    public static void Equal<T>(ArrayBufferWriter<T> oldData, ArrayBufferWriter<T> newData, string operation) =>
        Equal(oldData.WrittenSpan, newData.WrittenSpan, operation);
}

namespace Backdash.Synchronizing.State;

/// <summary>
///     Provider of checksum values
/// </summary>
public interface IChecksumProvider
{
    /// <summary>
    ///     Returns the checksum value for <paramref name="data" />.
    /// </summary>
    /// <param name="data"></param>
    /// <returns><see cref="int" /> checksum value</returns>
    Checksum Compute(ReadOnlySpan<byte> data);
}

/// <inheritdoc cref="IChecksumProvider" />
public delegate uint ChecksumDelegate(ReadOnlySpan<byte> data);

sealed class DelegateChecksumProvider(ChecksumDelegate compute) : IChecksumProvider
{
    public Checksum Compute(ReadOnlySpan<byte> data) => (Checksum)compute(data);
}

/// <summary>
///     Provider always zero checksum
/// </summary>
public sealed class EmptyChecksumProvider : IChecksumProvider
{
    /// <inheritdoc />
    public Checksum Compute(ReadOnlySpan<byte> data) => Checksum.Empty;
}

/// <summary>
///     Fletcher 32 checksum provider
///     see: http://en.wikipedia.org/wiki/Fletcher%27s_checksum
/// </summary>
public sealed class Fletcher32ChecksumProvider : IChecksumProvider
{
    const int BlockSize = 360;

    /// <inheritdoc />
    public unsafe Checksum Compute(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return Checksum.Empty;

        uint sum1 = 0xFFFF, sum2 = 0xFFFF;
        var dataIndex = 0;
        var dataLen = data.Length;
        var len = dataLen / sizeof(ushort);

        fixed (byte* ptr = data)
        {
            while (len > 0)
            {
                var blockLen = len > BlockSize ? BlockSize : len;
                len -= blockLen;

                do
                {
                    sum1 += *(ushort*)(ptr + dataIndex);
                    sum2 += sum1;
                    dataIndex += sizeof(ushort);
                } while (--blockLen > 0);

                sum1 = (sum1 & 0xFFFF) + (sum1 >> 16);
                sum2 = (sum2 & 0xFFFF) + (sum2 >> 16);
            }

            if (dataIndex < dataLen)
            {
                sum1 += *(ptr + dataLen - 1);
                sum2 += sum1;
                sum1 = (sum1 & 0xFFFF) + (sum1 >> 16);
                sum2 = (sum2 & 0xFFFF) + (sum2 >> 16);
            }
        }

        sum1 = (sum1 & 0xFFFF) + (sum1 >> 16);
        sum2 = (sum2 & 0xFFFF) + (sum2 >> 16);
        return new((sum2 << 16) | sum1);
    }
}

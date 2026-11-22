namespace Backdash.Network;

/// <summary>
///     Defines an endianness value
/// </summary>
public enum Endianness : byte
{
    /// <summary>Little endian byte order</summary>
    LittleEndian,

    /// <summary>Big endian byte order</summary>
    BigEndian,
}

/// <summary>
///     Platform Info
/// </summary>
public static class Platform
{
    /// <summary>
    ///     Returns the current machine endianness
    /// </summary>
    public static readonly Endianness Endianness =
        BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;

    /// <summary>
    ///     Gets a suitable <see cref="Endianness" />.<br/>
    ///     If <paramref name="network" /> is <see langword="true"/>, returns <see cref="Endianness.BigEndian" />.
    ///     otherwise, returns <see cref="Platform.Endianness" />.
    /// </summary>
    public static Endianness FindEndianness(bool network = true) => network ? Endianness.BigEndian : Endianness;
}

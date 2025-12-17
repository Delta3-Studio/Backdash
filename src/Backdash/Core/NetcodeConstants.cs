#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
global using Max = Backdash.Core.NetcodeConstants.Max;

namespace Backdash.Core;

/// <summary>
/// SDK constant values
/// </summary>
public static class NetcodeConstants
{
    /// <summary>
    /// maximum values
    /// </summary>
    public static class Max
    {
        public const int NumberOfPlayers = 4;
        public const int NumberOfSpectators = 32;
        public const int CompressedBytes = 512;
        public const int UdpPacketSize = 65_527;
        public const int PackageQueue = 64;
    }
}

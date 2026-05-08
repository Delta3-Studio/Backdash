using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Backdash.Network;

/// <summary>
///     Network utilities
/// </summary>
public static class NetUtils
{
    /// <summary>
    ///     Finds a free TCP port.
    /// </summary>
    public static int FindFreePort()
    {
        TcpListener? tcpListener = null;
        try
        {
            tcpListener = new(IPAddress.Loopback, 0);
            tcpListener.Start();
            return ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        }
        finally
        {
            tcpListener?.Stop();
        }
    }

    /// <summary>
    ///     Finds the current network IPAddress
    /// </summary>
    public static async ValueTask<IPAddress?> FindNetworkIPAddress(
        string host = "8.8.8.8",
        int port = 65530,
        CancellationToken ct = default
    )
    {
        try
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            await socket.ConnectAsync(host, port, ct);
            return socket.LocalEndPoint is not IPEndPoint { Address: { } ipAddress } ? null : ipAddress;
        }
        catch (Exception)
        {
            // skip
        }

        return null;
    }

    /// <summary>
    ///     Checks if the current connection is wireless
    /// </summary>
    public static bool IsWireless() => NetworkInterface
        .GetAllNetworkInterfaces().Any(n => n is
        {
            OperationalStatus: OperationalStatus.Up,
            NetworkInterfaceType: NetworkInterfaceType.Wireless80211,
        });

    /// <inheritdoc cref="NetworkInterface.GetIsNetworkAvailable"/>
    public static bool IsNetworkAvailable() => NetworkInterface.GetIsNetworkAvailable();
}

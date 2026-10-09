#pragma warning disable S1313
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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
    ///     Returns the Internet Protocol (IP) addresses for the specified host and <see cref="AddressFamily" />.
    /// </summary>
    public static IPAddress? FindDnsIpAddress(string host, AddressFamily addressFamily = AddressFamily.InterNetwork) =>
        Dns.GetHostAddresses(host, addressFamily).FirstOrDefault();

    /// <inheritdoc cref="FindDnsIpAddress(string, AddressFamily)"/>
    public static IPAddress? FindDnsIpAddress(Uri uri, AddressFamily addressFamily = AddressFamily.InterNetwork) =>
        FindDnsIpAddress(uri.DnsSafeHost, addressFamily);

    /// <inheritdoc cref="FindDnsIpAddress(string, AddressFamily)"/>
    /// <exception cref="InvalidOperationException">When not found</exception>
    public static IPAddress GetDnsIpAddress(string host, AddressFamily addressFamily = AddressFamily.InterNetwork) =>
        FindDnsIpAddress(host, addressFamily)
        ?? throw new InvalidOperationException($"Unable to retrieve IP Address from host {host}");

    /// <inheritdoc cref="GetDnsIpAddress(string, AddressFamily)" />
    public static IPAddress GetDnsIpAddress(Uri uri, AddressFamily addressFamily = AddressFamily.InterNetwork) =>
        GetDnsIpAddress(uri.DnsSafeHost, addressFamily);

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

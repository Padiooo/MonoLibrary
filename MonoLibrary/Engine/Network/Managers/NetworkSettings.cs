
using MonoLibrary.Dependency;

using System.Net;

namespace MonoLibrary.Engine.Network.Managers;

[Settings(nameof(NetworkSettings))]
public class NetworkSettings
{
    private string ipv4;
    public string IPv4
    {
        get => !string.IsNullOrEmpty(ipv4) ? ipv4 : IPAddress.Any.ToString();
        set
        {
            ipv4 = value;
        }
    }

    private string ipv6;
    public string IPv6
    {
        get => !string.IsNullOrEmpty(ipv6) ? ipv6 : IPAddress.IPv6Any.ToString();
        set
        {
            ipv6 = value;
        }
    }

    public int Port { get; set; }

    public bool IsServer { get; set; }

    public string ConnectionKey { get; set; }

    public override string ToString()
    {
        return $"IsServer {IsServer}, IPv4 '{IPv4}', IPv6 '{IPv6}', Port '{Port}', Key '{ConnectionKey}'";
    }
}

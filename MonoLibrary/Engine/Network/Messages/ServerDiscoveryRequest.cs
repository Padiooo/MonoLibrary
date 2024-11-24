using System.Net;

namespace MonoLibrary.Engine.Network.Messages;

public class ServerDiscoveryRequest
{

}

public class ServerDiscoveryResponse
{
    public IPEndPoint ServerEndPoint { get; set; }
    public string ConnectionKey { get; set; }
}

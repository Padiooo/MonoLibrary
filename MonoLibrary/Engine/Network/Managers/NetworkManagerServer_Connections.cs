using LiteNetLib;

using Microsoft.Extensions.Logging;

using MonoLibrary.Engine.Network.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MonoLibrary.Engine.Network.Managers
{
    public partial class NetworkManagerServer : NetworkManager
    {
        private readonly Dictionary<IPEndPoint, string> endpointToKey = new();

        protected int NextPeerId = 1;

        private NetworkIdentity CreateIdentity(NetPeer peer)
        {
            var identity = new NetworkIdentity(this, NextPeerId++, peer);

            PeerToIdentity.Add(peer, identity);
            IdentityToPeer.Add(identity, peer);

            using var writer = WriterPool.Get();

            var message = new NetworkIdentityMessage()
            {
                Add = true,
                Identities = new NetworkIdentity[] { identity }
            };

            PacketProcessor.Write(writer.Item, message);
            SendToAllBut(peer, writer, DeliveryMethod.ReliableUnordered);

            writer.Item.Reset();
            message.Identities = IdentityToPeer.Keys.ToArray();
            message.MyIdentity = Array.IndexOf(message.Identities, identity);
            PacketProcessor.Write(writer.Item, message);
            peer.Send(writer.Item, DeliveryMethod.ReliableUnordered);

            return identity;
        }

        protected virtual void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            var peer = OnConnectionRequest(request);

            if (peer is not null)
            {
                var netIdentity = CreateIdentity(peer);

                Logger.LogInformation("Client {networkId} connected.", netIdentity);
                using var writer = WriterPool.Get();
                using var localWriter = WriterPool.Get();

                foreach (var item in NetworkIdentityComponents.Values)
                {
                    localWriter.Item.Reset();
                    item.Serialize(localWriter.Item, true);

                    var msg = new SpawnMessage()
                    {
                        NetworkObjectId = item.NetObjectId,
                        PrefabId = item.PrefabId,
                        NetOwnerId = item.NetOwnerId,
                        Data = localWriter.Item.CopyData()
                    };
                    PacketProcessor.Write(writer.Item, msg);
                }

                peer.Send(writer.Item, DeliveryMethod.ReliableOrdered);
            }
        }

        /// <summary>
        /// Called on server when a client tries to connect. Default implementation use <see cref="ConnectionRequest.AcceptIfKey(string)"/> with
        /// the configured <see cref="NetworkSettings.ConnectionKey"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns><see langword="null"/> to reject, <see cref="NetPeer"/> to accept.</returns>
        protected virtual NetPeer OnConnectionRequest(ConnectionRequest request)
        {
            Logger.LogInformation("Connection request received from '{remoteAddress}'.", request.RemoteEndPoint);

            if (endpointToKey.TryGetValue(request.RemoteEndPoint, out var key))
                return request.AcceptIfKey(key);
            else
                return request.AcceptIfKey(Settings.ConnectionKey);
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            PeerToIdentity.Remove(peer, out var identity);
            Logger.LogInformation("Client {networkId} disconnected.", identity);

            foreach (var component in identity.GetIdentityComponents())
            {
                component.Owner.Destroy();

                Logger.LogTrace("Removing NetObjectId {netId} owned by NetworkIdentity {peerId}.", component.NetObjectId, identity.NetId);
            }

            using var writer = WriterPool.Get();
            PacketProcessor.Write(writer.Item, new NetworkIdentityMessage() { Add = false, Identities = new NetworkIdentity[] { identity } });
            NetManager.SendToAll(writer.Item, DeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Handle discovery requests. Answer to the discovery by sending server's <see cref="IPEndPoint"/> and a connection key.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="peer"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void HandleDiscoryRequest(ServerDiscoveryRequest request, IPEndPoint remoteEndPoint)
        {
            using var writer = WriterPool.Get();

            var key = Guid.NewGuid().ToString();
            endpointToKey.Add(remoteEndPoint, key);

            PacketProcessor.Write(writer.Item, new ServerDiscoveryResponse()
            {
                ServerEndPoint = new IPEndPoint(IPAddress.Parse(Settings.IPv4), Settings.Port),
                ConnectionKey = key,
            });

            NetManager.SendUnconnectedMessage(writer.Item, remoteEndPoint);
        }
    }
}

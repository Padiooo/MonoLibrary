using LiteNetLib;
using LiteNetLib.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MonoLibrary.Engine.Network.Messages;
using MonoLibrary.Engine.Network.Serializers;
using MonoLibrary.Engine.Services;
using MonoLibrary.Engine.Services.Updates;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace MonoLibrary.Engine.Network.Managers
{
    public class NetworkManagerClient : NetworkManager
    {
        protected NetPeer Server { get; private set; }

        public NetworkManagerClient(INetworkFactory factory,
                                    IUpdateLoop updater,
                                    IOptions<NetworkSettings> settings,
                                    IGameStateHub stateHub,
                                    ILogger<NetworkManagerClient> logger)
            : base(factory, updater, settings, stateHub, logger)
        {
            EventListener.PeerConnectedEvent += Listener_PeerConnectedEvent;

            PacketProcessor.SubscribeReusable<ServerDiscoveryResponse>(HandleDiscoveryResponse);
            PacketProcessor.SubscribeReusable<SpawnMessage>(HandleSpawn);
            PacketProcessor.SubscribeReusable<UpdateMessage>(HandleUpdate);
            PacketProcessor.SubscribeReusable<DeleteMessage>(HandleDelete);
            PacketProcessor.SubscribeReusable<NetworkIdentityMessage>(HandleIdentity);
        }

        public void Discover()
        {
            var writer = WriterPool.Get();
            PacketProcessor.Write(writer.Item, new ServerDiscoveryRequest());
            NetManager.SendBroadcast(writer.Item, Settings.Port);
        }

        protected virtual void HandleDiscoveryResponse(ServerDiscoveryResponse response)
        {
            Logger.LogInformation("Received DiscoveryResponse from {server} with key {key}. Connecting ...", response.ServerEndPoint, response.ConnectionKey);
            NetManager.Connect(response.ServerEndPoint, response.ConnectionKey);
        }

        private void HandleIdentity(NetworkIdentityMessage message)
        {
            foreach (var identity in message.Identities)
                IdentityToPeer.Add(identity, null);

            if (message.MyIdentity != -1)
            {
                Identity = message.Identities[message.MyIdentity];

                OnConnect();
            }
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Logger.LogInformation("Client connected to server, received PeerId {id}", peer.RemoteId);
            Server = peer;
        }

        /// <summary>
        /// Called when connection succeeded.
        /// </summary>
        protected virtual void OnConnect() { }

        public override void Start()
        {
            NetManager.Start();
            Logger.LogInformation("Client started with settings: {settings}.", Settings);

            NetManager.Connect(Settings.IPv4, Settings.Port, Settings.ConnectionKey);
        }

        public override void Stop()
        {
            Server?.Disconnect();
            NetManager?.Stop();
        }

        protected override void ProcessOutputs(int elaspedMilliseconds)
        {
            if (Server is null)
                return;

            using var writer = WriterPool.Get();

            var dirtyIds = DirtyIdentities.Select(id => NetworkIdentityComponents[id]);

            if (dirtyIds.Any())
            {
                SerializeWorldUpdate(writer.Item, dirtyIds, false, true);

                DirtyIdentities.Clear();

                Server.Send(writer.Item, DeliveryMethod.ReliableOrdered);
            }

            NetManager.TriggerUpdate();
        }

        public override void Spawn<T>(int prefabId, T data, NetworkIdentity identity = null) where T : struct
        {
            using var writer = WriterPool.Get();
            writer.Item.Write(data);

            var msg = new SpawnRequest
            {
                PrefabId = prefabId,
                Data = writer.Item.CopyData(),
                NetworkOwnerId = identity?.NetId ?? 0
            };

            writer.Item.Reset();
            PacketProcessor.Write(writer.Item, msg);
            Server?.Send(writer.Item, DeliveryMethod.ReliableUnordered);
        }

        public override void Delete(int netId)
        {
            using var writer = WriterPool.Get();
            PacketProcessor.Write(writer.Item, new DeleteMessage() { NetIds = new int[] { netId } });
            Server?.Send(writer.Item, DeliveryMethod.ReliableUnordered);
        }

        protected virtual void HandleSpawn(SpawnMessage message)
        {
            var identity = Spawn(message.PrefabId, message.NetworkObjectId, default(NetDataReader));
            identity.IsServer = false;
            identity.IsLocalPlayer = message.NetOwnerId == Identity?.NetId;
            identity.Manager = this;

            NetworkIdentityComponents.Add(identity.NetObjectId, identity);

            var reader = new NetDataReader(message.Data);
            identity.Deserialize(reader);
            identity.Initialize();
            identity.ClearDirty();

            identity.OnDirty += id =>
            {
                DirtyIdentities.Add(id);
            };
        }

        protected virtual void HandleUpdate(UpdateMessage message)
        {
            var reader = new NetDataReader(message.Data);
            foreach (var netId in message.NetIds)
                if (NetworkIdentityComponents.TryGetValue(netId, out var net))
                    net.Deserialize(reader);
        }

        protected virtual void HandleDelete(DeleteMessage message)
        {
            foreach (var netId in message.NetIds)
                if (NetworkIdentityComponents.Remove(netId, out var value))
                {
                    value.Owner.Destroy();
                    value.Dispose();
                }
        }
    }
}

using LiteNetLib;
using LiteNetLib.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MonoLibrary.Engine.Network.Messages;
using MonoLibrary.Engine.Pools;
using MonoLibrary.Engine.Services;
using MonoLibrary.Engine.Services.Updates;

using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace MonoLibrary.Engine.Network.Managers
{
    public partial class NetworkManagerServer : NetworkManager
    {
        /// <summary>
        /// Interval for a full serilization of the world sent to all connected players. In milliseconds.
        /// </summary>
        private int fullSyncPeriod = 1000;
        public int FullSyncPeriod
        {
            get => fullSyncPeriod;
            set
            {
                fullSyncPeriod = value;
                nextSync = value;
            }
        }
        private int nextSync;

        public NetworkManagerServer(INetworkFactory factory,
                                    IUpdateLoop updater,
                                    IOptions<NetworkSettings> settings,
                                    IGameStateHub gameState,
                                    ILogger<NetworkManagerServer> logger)
            : base(factory, updater, settings, gameState, logger)
        {
            PacketProcessor.SubscribeReusable<SpawnRequest, NetPeer>(HandleSpawnRequest);
            PacketProcessor.SubscribeReusable<UpdateMessage, NetPeer>(HandleUpdateMessage);
            PacketProcessor.SubscribeReusable<DeleteMessage, NetPeer>(HandleDeleteMessage);
            PacketProcessor.SubscribeReusable<ServerDiscoveryRequest, IPEndPoint>(HandleDiscoryRequest);

            EventListener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;
            EventListener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;

            NetManager.BroadcastReceiveEnabled = true;
        }

        public override void Start()
        {
            Logger.LogInformation("Server started using settings: {settings}.", Settings);
            NetManager.StartInManualMode(Settings.Port);
        }
        public override void Stop()
        {
            NetManager.DisconnectAll();
            NetManager.Stop();
        }

        protected override void ProcessOutputs(int elaspedMilliseconds)
        {
            nextSync -= elaspedMilliseconds;
            bool full = nextSync < 0;

            if (full)
                nextSync += FullSyncPeriod;

            using var writer = WriterPool.Get();

            if (DeletedIds.Count > 0)
            {
                PacketProcessor.Write(writer.Item, new DeleteMessage() { NetIds = DeletedIds.ToArray() });
                DeletedIds.Clear();
            }

            var dirtyIds = full ? NetworkIdentityComponents.Values : DirtyIdentities.Select(id => NetworkIdentityComponents[id]);

            if (dirtyIds.Any())
            {
                SerializeWorldUpdate(writer.Item, dirtyIds, full, true);

                DirtyIdentities.Clear();
            }

            if (writer.Item.Length > 0)
                NetManager.SendToAll(writer.Item, DeliveryMethod.ReliableUnordered);

            NetManager.ManualUpdate(elaspedMilliseconds);
        }

        protected virtual void HandleUpdateMessage(UpdateMessage message, NetPeer peer)
        {
            var reader = new NetDataReader(message.Data);

            foreach (var identity in message.NetIds.Select(id => NetworkIdentityComponents[id]))
                identity.Deserialize(reader);
        }

        #region HELPERS

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendToAllBut(NetPeer peer, IPooled<NetDataWriter> writer, DeliveryMethod deliveryMethod) => SendToAllBut(peer, writer.Item, deliveryMethod);

        protected void SendToAllBut(NetPeer peer, NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            foreach (var p in NetManager)
            {
                if (p.Equals(peer))
                    continue;
                p.Send(writer, deliveryMethod);
            }
        }

        #endregion
    }
}

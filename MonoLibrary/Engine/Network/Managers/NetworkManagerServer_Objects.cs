using LiteNetLib;
using LiteNetLib.Utils;

using Microsoft.Extensions.Logging;

using MonoLibrary.Engine.Network.Components;
using MonoLibrary.Engine.Network.Messages;
using MonoLibrary.Engine.Network.Serializers;

using System.Collections.Generic;
using System.Linq;

namespace MonoLibrary.Engine.Network.Managers;

public partial class NetworkManagerServer : NetworkManager
{
    protected readonly List<int> DeletedIds = [];

    public override void Spawn<T>(int prefabId, T data, NetworkIdentity identity) where T : struct
    {
        byte[] buffer;
        using (var writer = WriterPool.Get())
        {
            writer.Item.Write<T>(data);
            buffer = writer.Item.CopyData();
        }

        HandleSpawnRequest(new SpawnRequest() { PrefabId = prefabId, NetworkOwnerId = identity?.NetId ?? 0, Data = buffer }, null);
    }

    protected virtual void HandleSpawnRequest(SpawnRequest request, NetPeer requester)
    {
        int networkObjectId = NextNetObjectId++;
        NetworkIdentityComponent identityComponent = Spawn(request.PrefabId, networkObjectId, new NetDataReader(request.Data));
        identityComponent.IsServer = true;
        identityComponent.IsLocalPlayer = request.NetworkOwnerId != 0;
        identityComponent.NetOwnerId = request.NetworkOwnerId;
        identityComponent.Manager = this;

        Logger.LogTrace("Spawned NetObjectId {netId}, NetOwnerId {ownerId}.", identityComponent.NetObjectId, identityComponent.NetOwnerId);

        identityComponent.Initialize();

        identityComponent.OnDirty += id =>
        {
            DirtyIdentities.Add(id);
        };

        NetworkIdentityComponents.Add(networkObjectId, identityComponent);
        DirtyIdentities.Add(networkObjectId);

        SendSpawnMessages(identityComponent, request);

        if (request.NetworkOwnerId != 0)
            AddOwnerShip(request.NetworkOwnerId, identityComponent);
    }

    private void SendSpawnMessages(NetworkIdentityComponent idComp, SpawnRequest request)
    {
        using var writer = WriterPool.Get();
        idComp.Serialize(writer.Item, true);

        var msg = new SpawnMessage()
        {
            NetworkObjectId = idComp.NetObjectId,
            PrefabId = request.PrefabId,
            Data = writer.Item.CopyData(),
            NetOwnerId = request.NetworkOwnerId
        };
        writer.Item.Reset();

        PacketProcessor.Write(writer.Item, msg);

        NetManager.SendToAll(writer.Item, DeliveryMethod.ReliableOrdered);
    }

    private void AddOwnerShip(int netOwnerId, NetworkIdentityComponent netIdentity)
    {
        NetworkIdentity networkIdentity = IdentityToPeer.Keys.FirstOrDefault(netId => netId.NetId == netOwnerId);
        var message = networkIdentity.AddOwnership(netIdentity.NetObjectId);
        Logger.LogTrace("Added ownership to NetworkIdentity {netId} on NetworkComponent {netObjectId}.", networkIdentity, netIdentity.NetObjectId);
    }

    private void HandleDeleteMessage(DeleteMessage message, NetPeer peer)
    {
        var identity = PeerToIdentity[peer];
        var toDelete = new List<int>();
        foreach (var comp in identity.GetIdentityComponents())
        {
            if (comp is null)
                continue;

            foreach (var netId in message.NetIds)
                if (comp.NetObjectId == netId)
                    toDelete.Add(netId);
        }

        foreach (var id in toDelete)
            Delete(id);
    }

    public override void Delete(int netObjectId)
    {
        var identityComponent = NetworkIdentityComponents[netObjectId];
        identityComponent.Owner.Destroy();
        identityComponent.Dispose();

        var message = IdentityToPeer.Keys.FirstOrDefault(netId => netId.Owns(netObjectId))?.RemoveOwnership(netObjectId);
        NetworkIdentityComponents.Remove(identityComponent.NetObjectId);
        DirtyIdentities.Remove(identityComponent.NetObjectId);
        DeletedIds.Add(netObjectId);
        Logger.LogTrace("Deleted NetObjectId {netId}.", identityComponent.NetObjectId);
    }
}

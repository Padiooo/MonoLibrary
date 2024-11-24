using LiteNetLib;
using LiteNetLib.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MonoLibrary.Engine.Network.Components;
using MonoLibrary.Engine.Network.Messages;
using MonoLibrary.Engine.Network.Serializers;
using MonoLibrary.Engine.Pools;
using MonoLibrary.Engine.Services;
using MonoLibrary.Engine.Services.Updates;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MonoLibrary.Engine.Network.Managers;

public abstract class NetworkManager : INetworkManager, IUpdaterService
{
    private readonly IDisposable _subscription;

    protected readonly IGameStateHub GameState;
    protected readonly NetworkSettings Settings;

    protected readonly NetManager NetManager;
    protected readonly EventBasedNetListener EventListener;

    protected readonly NetPacketProcessor PacketProcessor;
    protected readonly AutoPool<NetDataWriter> WriterPool = new(() => new NetDataWriter(), writer => writer.Reset(), 20);

    protected readonly INetworkFactory Factory;
    protected readonly ILogger Logger;

    protected int NextNetObjectId;
    protected readonly SortedList<int, NetworkIdentityComponent> NetworkIdentityComponents = [];
    protected readonly SortedSet<int> DirtyIdentities = [];

    protected readonly Dictionary<NetPeer, NetworkIdentity> PeerToIdentity = [];
    protected readonly Dictionary<NetworkIdentity, NetPeer> IdentityToPeer = [];

    /// <summary>
    /// Local <see cref="NetworkIdentity"/>. Allways <see langword="null"/> on server.
    /// </summary>
    public NetworkIdentity Identity { get; protected set; }

    public NetworkManager(INetworkFactory factory,
                          IUpdateLoop updater,
                          IOptions<NetworkSettings> settings,
                          IGameStateHub stateHub,
                          ILogger logger)
    {
        _subscription = updater.Register(this);
        GameState = stateHub;

        GameState.Exiting += Stop;

        Settings = settings.Value;
        Factory = factory;
        Logger = logger;
        PacketProcessor = new NetPacketProcessor();
        PacketProcessor.RegisterNestedType<NetworkIdentity>(() => new NetworkIdentity(this));
        PacketProcessor.RegisterMonoGameTypes();

        EventListener = new EventBasedNetListener();
        EventListener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
        EventListener.NetworkReceiveUnconnectedEvent += Listener_NetworkReceiveUnconnectedEvent;

        NetManager = new NetManager(EventListener)
        {
            UnconnectedMessagesEnabled = true,
            IPv6Enabled = false
        };
    }

    private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        try
        {
            PacketProcessor.ReadAllPackets(reader, peer);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error during packet processing.");
        }
    }
    private void Listener_NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        try
        {
            PacketProcessor.ReadAllPackets(reader, remoteEndPoint);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error during packet processing from unconnected peer.");
        }
    }

    protected void SerializeWorldUpdate(NetDataWriter writer, IEnumerable<NetworkIdentityComponent> identities, bool full, bool clear)
    {
        var netIds = new List<int>();
        using var localWriter = WriterPool.Get();

        foreach (var identity in identities)
        {
            netIds.Add(identity.NetObjectId);
            identity.Serialize(localWriter.Item, full);
            if (clear)
                identity.ClearDirty();
        }

        var message = new UpdateMessage()
        {
            NetIds = netIds.ToArray(),
            Data = localWriter.Item.CopyData()
        };

        PacketProcessor.Write(writer, message);
    }

    protected NetworkIdentityComponent Spawn(int prefabId, int netObjectId, NetDataReader reader)
    {
        var go = Factory.Spawn(prefabId, reader);
        var identity = new NetworkIdentityComponent(go)
        {
            PrefabId = prefabId,
            NetObjectId = netObjectId,
        };
        go.Components.Add(identity);

        return identity;
    }

    public virtual void BeforeUpdate()
    {
        ProcessInputs();
    }

    int elaspedMilliseconds;
    public void Update(float deltaTime)
    {
        elaspedMilliseconds = (int)(deltaTime * 1_000f);
    }

    public virtual void AfterUpdate()
    {
        ProcessOutputs(elaspedMilliseconds);
    }

    protected virtual void ProcessInputs() => NetManager.PollEvents();
    protected abstract void ProcessOutputs(int elaspedMilliseconds);

    public abstract void Spawn<T>(int prefabId, T data, NetworkIdentity identity = null) where T : struct;
    public abstract void Delete(int netId);

    public abstract void Start();
    public abstract void Stop();

    public NetworkIdentityComponent GetComponent(int netObjectId)
    {
        _ = NetworkIdentityComponents.TryGetValue(netObjectId, out var value);
        return value;
    }
    public NetworkIdentity GetIdentity(int netId)
    {
        return IdentityToPeer.Keys.FirstOrDefault(identity => identity.NetId == netId);
    }

    public NetworkIdentity GetOwner(int netObjectId)
    {
        return IdentityToPeer.Keys.FirstOrDefault(identity => identity.Owns(netObjectId));
    }

    /// <summary>
    /// Send <paramref name="message"/> to the <paramref name="identity"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="identity">If on server: 
    /// <br/> - when <see langword="null"/>, send to all connected peers
    /// <br/> - when not <see langword="null"/>, send to specific peer
    /// <br/>
    /// If on client: 
    /// <br/> - when <see langword="null"/>, send to server
    /// <br/> - when not <see langword="null"/>, send to server, then server forward to specific peer
    /// </param>
    //public abstract void SendMessage<T>(T message, NetworkIdentity identity = null) where T : class;

    /// <summary>
    /// Send <paramref name="message"/> to the <paramref name="identities"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="identities">If on server: 
    /// <br/> - when <see langword="null"/> or <see cref="Enumerable.Empty{T}"/>, send to all connected peers
    /// <br/> - when not <see langword="null"/>, send to specific peers
    /// <br/>
    /// If on client: 
    /// <br/> - when <see langword="null"/> or <see cref="Enumerable.Empty{T}"/>, send to server
    /// <br/> - when not <see langword="null"/>, send to server, then server forward to specific peers
    /// </param>
    //public abstract void SendMessage<T>(T message, IEnumerable<NetworkIdentity> identities) where T : class;

    public void Dispose()
    {
        Stop();

        _subscription?.Dispose();
        GameState.Exiting -= Stop;
    }
}

using LiteNetLib.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MonoLibrary.Engine.Network.Managers;
using MonoLibrary.Engine.Objects;

using System;
using System.Collections.Generic;

namespace MonoLibrary.Engine.Network;

public abstract class NetworkPrefabFactoryBase(IOptions<NetworkSettings> settings, ILogger logger) : INetworkFactory
{
    public delegate GameObject SpawnPrefab(NetDataReader reader);

    protected readonly ILogger Logger = logger;
    private readonly Dictionary<int, SpawnPrefab> _prefabs = [];

    public bool IsServer { get; } = settings.Value.IsServer;

    protected void RegisterPrefab(int prefabId, SpawnPrefab factory)
    {
        if (_prefabs.TryGetValue(prefabId, out var old))
            Logger.LogWarning("Prefab with Id {id} already registered. Prefab '{new}' will override '{old}'.", prefabId, old.Method.Name, factory.Method.Name);

        _prefabs[prefabId] = factory;
        Logger.LogInformation("Prefab '{factory}' registered with Id {id}.", factory.Method.Name, prefabId);
    }

    /// <summary>
    /// Helper to use <see cref="Enum"/> values.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="prefabId"></param>
    /// <param name="factory"></param>
    protected void RegisterPrefab<TEnum>(TEnum prefabId, SpawnPrefab factory) where TEnum : struct, Enum
    {
        RegisterPrefab((int)Convert.ChangeType(prefabId, prefabId.GetTypeCode()), factory);
    }

    public GameObject Spawn(int prefabId, NetDataReader reader)
    {
        SpawnPrefab prefab = _prefabs[prefabId];
        Logger.LogTrace("Spawning prefab {prefabId} '{prefabName}', with {bytes} bytes of data.", prefabId, prefab.Method.Name, reader?.RawDataSize ?? 0);
        return prefab.Invoke(reader);
    }
}

using System;

namespace MonoLibrary.Engine.Network.Managers;

public static class NetworkManagerExtensions
{
    public static void Spawn<T>(this INetworkManager manager, Enum prefabId, T data, NetworkIdentity identity = null) where T : struct
    {
        int prefab = (int)Convert.ChangeType(prefabId, prefabId.GetTypeCode());
        manager.Spawn(prefab, data, identity);
    }
}

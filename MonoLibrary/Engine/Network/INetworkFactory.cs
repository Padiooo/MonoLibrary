using LiteNetLib.Utils;

using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Network;

public interface INetworkFactory
{
    bool IsServer { get; }

    /// <summary>
    /// Locally spawn the <see cref="GameObject"/> and return it.
    /// </summary>
    /// <param name="prefabId">Id of the prefab to use.</param>
    /// <param name="reader"><see langword="null"/> on client.</param>
    /// <returns></returns>
    GameObject Spawn(int prefabId, NetDataReader reader);
}

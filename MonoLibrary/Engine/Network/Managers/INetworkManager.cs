
using MonoLibrary.Engine.Network.Components;

namespace MonoLibrary.Engine.Network.Managers
{
    public interface INetworkManager
    {
        void Start();

        void Spawn<T>(int prefabId, T data, NetworkIdentity identity = null) where T : struct;

        NetworkIdentityComponent GetComponent(int netObjectId);

        NetworkIdentity GetOwner(int netObjectId);
        NetworkIdentity GetIdentity(int netId);

        void Delete(int netId);

        void Stop();
    }
}

using LiteNetLib.Utils;

using System;

namespace MonoLibrary.Engine.Network.Datas;

public interface INetVariable : IDisposable
{
    int DirtyIndex { get; set; }

    event Action<int> OnDirty;

    void Serialize(NetDataWriter writer, bool full);
    void Deserialize(NetDataReader reader);

    void ClearDirty();
}

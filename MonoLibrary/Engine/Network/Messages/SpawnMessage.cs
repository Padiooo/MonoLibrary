namespace MonoLibrary.Engine.Network.Messages;

public class SpawnMessage
{
    public int PrefabId { get; set; }

    public int NetworkObjectId { get; set; }

    public int NetOwnerId { get; set; }

    public byte[] Data { get; set; }
}

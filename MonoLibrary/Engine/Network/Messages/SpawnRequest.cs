namespace MonoLibrary.Engine.Network.Messages;

public class SpawnRequest
{
    public int PrefabId { get; set; }
    public int NetworkOwnerId { get; set; }

    public byte[] Data { get; set; }
}

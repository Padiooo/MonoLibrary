namespace MonoLibrary.Engine.Network.Messages;

public class OwnerShipMessage
{
    public int NetId { get; set; }
    public int[] NetObjectIds { get; set; }
    public bool Add { get; set; }
}

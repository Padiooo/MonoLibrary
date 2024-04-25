namespace MonoLibrary.Engine.Network.Messages
{
    public class UpdateMessage
    {
        public int[] NetIds { get; set; }
        public byte[] Data { get; set; }
    }
}

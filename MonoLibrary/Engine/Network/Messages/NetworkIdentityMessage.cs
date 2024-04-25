using MonoLibrary.Engine.Network.Managers;

namespace MonoLibrary.Engine.Network.Messages
{
    /// <summary>
    /// Message used to maintain server and client identities synchronized.
    /// </summary>
    public class NetworkIdentityMessage
    {
        public NetworkIdentity[] Identities { get; set; }
        public bool Add { get; set; }
        public int MyIdentity { get; set; } = -1;
    }
}

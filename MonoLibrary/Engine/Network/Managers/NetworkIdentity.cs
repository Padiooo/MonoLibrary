using LiteNetLib.Utils;

using MonoLibrary.Engine.Network.Components;
using MonoLibrary.Engine.Network.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MonoLibrary.Engine.Network.Managers
{
    public class NetworkIdentity : INetSerializable, IEquatable<NetworkIdentity>, IComparable<NetworkIdentity>
    {
        private readonly HashSet<int> _belongings = new();

        public INetworkManager NetworkManager { get; }
        public int NetId { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        internal NetworkIdentity(INetworkManager networkManager)
        {
            NetworkManager = networkManager;
        }

        internal NetworkIdentity(INetworkManager networkManager, int netId, IPEndPoint address)
        {
            NetworkManager = networkManager;
            NetId = netId;
            EndPoint = address;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(EndPoint.ToString());
            writer.PutArray(_belongings.ToArray());
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            EndPoint = IPEndPoint.Parse(reader.GetString());
            foreach (var owned in reader.GetIntArray())
                _belongings.Add(owned);
        }

        public bool Owns(int netObjectId) => _belongings.Contains(netObjectId);

        public IEnumerable<NetworkIdentityComponent> GetIdentityComponents()
        {
            foreach (var netObjectId in _belongings)
                yield return NetworkManager.GetComponent(netObjectId);
        }

        internal OwnerShipMessage AddOwnership(int netObjectId)
        {
            if (_belongings.Add(netObjectId))
            {
                var comp = NetworkManager.GetComponent(netObjectId);
                comp.NetOwnerId = NetId;
                return new OwnerShipMessage() { NetId = NetId, NetObjectIds = new int[] { netObjectId }, Add = true };
            }
            else
                return null;
        }

        internal OwnerShipMessage RemoveOwnership(int netObjectId)
        {
            if (_belongings.Remove(netObjectId))
            {
                var comp = NetworkManager.GetComponent(netObjectId);
                comp.NetOwnerId = 0;
                return new OwnerShipMessage() { NetId = NetId, NetObjectIds = new int[] { netObjectId }, Add = true };
            }
            else
                return null;
        }

        internal OwnerShipMessage Delete()
        {
            var msg = new OwnerShipMessage() { NetId = NetId, NetObjectIds = _belongings.ToArray(), Add = false };

            foreach (var comp in GetIdentityComponents())
                comp.NetOwnerId = 0;

            _belongings.Clear();

            return msg;
        }

        #region OVERRIDES

        public int CompareTo(NetworkIdentity other) => NetId - other.NetId;

        public bool Equals(NetworkIdentity other) => NetId == other.NetId;

        public override bool Equals(object obj) => obj is NetworkIdentity other && Equals(other);

        public override int GetHashCode() => NetId;

        public override string ToString()
        {
            return $"NetId {NetId}, '{EndPoint}'";
        }

        #endregion
    }
}

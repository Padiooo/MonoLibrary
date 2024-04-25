using LiteNetLib.Utils;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Network.Datas;
using MonoLibrary.Engine.Network.Managers;
using MonoLibrary.Engine.Network.Serializers;
using MonoLibrary.Engine.Network.Utils;
using MonoLibrary.Engine.Objects;

using System;
using System.Collections.Generic;

namespace MonoLibrary.Engine.Network.Components
{
    public class NetworkIdentityComponent : IComponent, INetVariable
    {
        private readonly NetVarContainer _container;
        private readonly NetVar<int> _netOwner = new();
        private readonly List<NetworkComponent> _networkComponents = new();

        private readonly Dictionary<int, ILocalInvoker> _invokers = new();
        private readonly List<ISerializableCommand> _calls = new();

        private bool isDirty;

        public GameObject Owner { get; }
        public int NetObjectId
        {
            get => _netOwner.Value;
            set => _netOwner.Value = value;
        }
        public int PrefabId { get; internal set; }
        public int NetOwnerId { get; internal set; }
        public NetworkManager Manager { get; internal set; }

        /// <summary>
        /// Tells if a player has ownership.
        /// </summary>
        public bool IsLocalPlayer { get; internal set; }

        /// <summary>
        /// Tells if object is running on server or on client.
        /// </summary>
        public bool IsServer { get; internal set; }

        public int DirtyIndex { get; set; }

        /// <summary>
        /// Uses <see cref="NetObjectId"/> as parameter.
        /// </summary>
        public event Action<int> OnDirty;

        public NetworkIdentityComponent(GameObject owner)
        {
            Owner = owner;
            Owner.Components.OnComponentAdded += Components_OnComponentAdded;
            Owner.Components.OnComponentRemoved += Components_OnComponentRemoved;

            _container = new NetVarContainer();
            _container.OnDirty += Dirty;

            _container.AddNetVar(_netOwner);
            _netOwner.OnValueChanged += OwnerChanged;

            foreach (var component in owner.Components)
                Components_OnComponentAdded(component);
        }

        private void OwnerChanged(int old, int @new)
        {

        }

        private void Components_OnComponentAdded(IComponent component)
        {
            if (component is NetworkComponent net)
            {
                _networkComponents.Add(net);
                _container.AddNetVar(net);
            }
        }

        private void Components_OnComponentRemoved(IComponent component)
        {
            if (component is NetworkComponent net)
            {
                _networkComponents.Remove(net);
                _container.RemoveNetVar(net);
            }
        }

        /// <summary>
        /// On client, called after full spawning and its first deserialization.<br/>
        /// On server, called after full spawning and before its first serialization.
        /// </summary>
        internal void Initialize()
        {
            foreach (var netComp in _networkComponents)
                netComp.Initialize(this);
        }

        public IInvokable RegisterCommand(Action cmd)
        {
            return new Invoker(this, cmd);
        }

        public IInvokable<T> RegisterCommand<T>(Action<T> cmd)
            where T : struct
        {
            return new Invoker<T>(this, cmd);
        }

        public void Serialize(NetDataWriter writer, bool full)
        {
            writer.Put(_calls.Count);
            foreach (var call in _calls)
                call.Serialize(writer);
            _calls.Clear();

            _container.Serialize(writer, full);
        }

        public void Deserialize(NetDataReader reader)
        {
            int calls = reader.GetInt();
            for (int i = 0; i < calls; i++)
            {
                int methodHash = reader.GetInt();
                if (_invokers.TryGetValue(methodHash, out var invoker))
                    invoker.Invoke(reader);
            }

            _container.Deserialize(reader);
        }

        private void Dirty(int index)
        {
            var dirty = isDirty;
            isDirty = true;
            if (!dirty)
                OnDirty?.Invoke(NetObjectId);
        }

        public void ClearDirty()
        {
            isDirty = false;
            _container.ClearDirty();
        }

        public virtual void OnDestroy()
        {
            Manager.Delete(NetObjectId);
        }

        public void Dispose()
        {
            _container.Dispose();
            _container.OnDirty -= OnDirty.Invoke;

            Owner.Components.OnComponentAdded -= Components_OnComponentAdded;
            Owner.Components.OnComponentRemoved -= Components_OnComponentRemoved;
        }

        #region Invokers

        private interface ILocalInvoker
        {
            void Invoke(NetDataReader reader);
        }

        private interface ISerializableCommand
        {
            void Serialize(NetDataWriter writer);
        }

        private readonly struct Invoker : IInvokable, ILocalInvoker
        {
            private readonly NetworkIdentityComponent _identity;
            private readonly Action _cmd;
            public readonly int MethodHash;

            public Invoker(NetworkIdentityComponent identity, Action command)
            {
                _identity = identity;
                _cmd = command;
                MethodHash = command.Method.Name.GetStableHash();

                int i = 0;
                while (!identity._invokers.TryAdd((MethodHash = command.Method.Name.GetStableHash() + i++), this))
                    ;
            }

            void IInvokable.Invoke()
            {
                _identity._calls.Add(new CallMessage(MethodHash));
                _identity.Dirty(0);
            }

            void ILocalInvoker.Invoke(NetDataReader reader)
            {
                _cmd.Invoke();
            }

            void IDisposable.Dispose()
            {
                _identity._invokers.Remove(MethodHash);
            }

            private readonly struct CallMessage : ISerializableCommand
            {
                private readonly int methodHash;

                public CallMessage(int methodHash)
                {
                    this.methodHash = methodHash;
                }

                public void Serialize(NetDataWriter writer)
                {
                    writer.Put(methodHash);
                }
            }
        }

        private readonly struct Invoker<T> : IInvokable<T>, ILocalInvoker
            where T : struct
        {
            private readonly NetworkIdentityComponent _identity;
            private readonly Action<T> _cmd;
            public readonly int MethodHash;

            public Invoker(NetworkIdentityComponent identity, Action<T> command)
            {
                _identity = identity;
                _cmd = command;
                MethodHash = command.Method.Name.GetStableHash();

                int i = 0;
                while (!identity._invokers.TryAdd((MethodHash = command.Method.Name.GetStableHash() + i++), this))
                    ;
            }

            void IInvokable<T>.Invoke(T data)
            {
                _identity._calls.Add(new CallMessage(MethodHash, data));
                _identity.Dirty(0);
            }

            void ILocalInvoker.Invoke(NetDataReader reader)
            {
                T data = reader.Read<T>();
                _cmd.Invoke(data);
            }

            void IDisposable.Dispose()
            {
                _identity._invokers.Remove(MethodHash);
            }

            private readonly struct CallMessage : ISerializableCommand
            {
                private readonly int methodHash;
                private readonly T data;

                public CallMessage(int methodHash, T data)
                {
                    this.methodHash = methodHash;
                    this.data = data;
                }

                public void Serialize(NetDataWriter writer)
                {
                    writer.Put(methodHash);
                    writer.Write<T>(data);
                }
            }
        }

        #endregion
    }
}

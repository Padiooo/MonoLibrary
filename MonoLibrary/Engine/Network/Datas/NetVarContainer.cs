using LiteNetLib.Utils;

using System;
using System.Collections.Generic;
using System.Text;

namespace MonoLibrary.Engine.Network.Datas
{
    public class NetVarContainer : INetVariable
    {
        public const int MaxObjects = 64;

        private readonly List<INetVariable> netVars = new(64);
        private long mask = 0;
        private long fullMask = 0;

        private bool disposedValue;

        public int DirtyIndex { get; set; }

        public event Action<int> OnDirty;

        public void AddNetVar(INetVariable netVar)
        {
            netVar.DirtyIndex = netVars.Count;
            fullMask |= 1L << netVar.DirtyIndex;

            netVar.OnDirty += Dirty;

            netVars.Add(netVar);
        }

        public void RemoveNetVar(INetVariable netVar)
        {
            fullMask &= ~(1L << netVar.DirtyIndex);
            netVar.OnDirty -= Dirty;
            netVars.Remove(netVar);
        }

        private void Dirty(int index)
        {
            var dirty = mask != 0;
            mask |= 1L << index;
            if (!dirty)
                OnDirty?.Invoke(DirtyIndex);
        }

        public void ClearDirty()
        {
            mask = 0;

            foreach (var netVar in netVars)
                netVar.ClearDirty();
        }

        public virtual void Serialize(NetDataWriter writer, bool full)
        {
            writer.Put(full ? fullMask : mask);

            int i = 0;
            if (full)
                while ((fullMask & 1L << i) != 0)
                    netVars[i++].Serialize(writer, full);
            else
                for (i = 0; i < netVars.Count; i++)
                    if ((mask & 1L << i) != 0)
                        netVars[i].Serialize(writer, full);
        }

        public virtual void Deserialize(NetDataReader reader)
        {
            var mask = reader.GetLong();

            for (int i = 0; i < MaxObjects; i++)
                if ((mask & 1L << i) != 0)
                    netVars[i].Deserialize(reader);
        }

        public override string ToString()
        {
            var stb = new StringBuilder(nameof(NetVarContainer) + '\n');
            for (int i = 0; i < netVars.Count; i++)
                stb.AppendLine('\t' + netVars[i].ToString());
            return stb.ToString();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    foreach (var netObject in netVars)
                        netObject.OnDirty -= Dirty;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
                netVars.Clear();
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NetVarContainer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

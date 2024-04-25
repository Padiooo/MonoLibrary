using LiteNetLib.Utils;

using System;
using System.Runtime.InteropServices;

namespace MonoLibrary.Engine.Network.Serializers
{
    public static partial class NetSerialization
    {
        public static T Read<T>(this NetDataReader reader)
            where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(reader.RawData, reader.Position, ptr, size);
                var res = Marshal.PtrToStructure<T>(ptr);
                return res;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                reader.SetPosition(reader.Position + size);
            }
        }
    }
}

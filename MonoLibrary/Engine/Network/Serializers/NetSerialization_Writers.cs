using LiteNetLib.Utils;

using System;
using System.Runtime.InteropServices;

namespace MonoLibrary.Engine.Network.Serializers;

public static partial class NetSerialization
{
    public static void Write<T>(this NetDataWriter writer, T value)
        where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            writer.Put(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}

using LiteNetLib.Utils;

using Microsoft.Xna.Framework;

namespace MonoLibrary.Engine.Network.Serializers
{
    public static partial class NetSerialization
    {
        public static void RegisterMonoGameTypes(this NetPacketProcessor processor)
        {
            processor.RegisterNestedType<Vector2>(Write<Vector2>, Read<Vector2>);
            processor.RegisterNestedType<Rectangle>(Write<Rectangle>, Read<Rectangle>);
            processor.RegisterNestedType<Point>(Write<Point>, Read<Point>);
            processor.RegisterNestedType<Color>(Write<Color>, Read<Color>);
        }
    }
}

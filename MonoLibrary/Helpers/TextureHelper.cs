using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MonoLibrary.Helpers
{
    public static class TextureHelper
    {
        public static Texture2D CreateSquare(this GraphicsDevice graphicsDevice, int size = 256)
        {
            var square = new Texture2D(graphicsDevice, size, size);
            var data = new Color[square.Width * square.Height];

            Array.Fill(data, Color.White);
            square.SetData(data);

            return square;
        }

        public static Texture2D CreateCircleFill(this GraphicsDevice graphicsDevice, int radius = 128)
        {
            var circle = new Texture2D(graphicsDevice, radius * 2, radius * 2);
            var data = new Color[circle.Width * circle.Height];

            int squaredRadius = radius * radius;
            for (int x = 0; x < radius; x++)
                for (int y = 0; y < radius; y++)
                    if (new Vector2(x, y).LengthSquared() <= squaredRadius)
                        foreach (var index in GetIndices(x, y, radius))
                            data[index] = Color.White;

            static IEnumerable<int> GetIndices(int x, int y, int radius)
            {
                yield return CoordToIndex(radius + x, radius + y, radius);
                yield return CoordToIndex(radius + x, radius - y, radius);
                yield return CoordToIndex(radius - x, radius + y, radius);
                yield return CoordToIndex(radius - x, radius - y, radius);
            }

            static int CoordToIndex(int x, int y, int radius)
            {
                return x + y * radius * 2;
            }

            circle.SetData(data);

            return circle;
        }
    }
}

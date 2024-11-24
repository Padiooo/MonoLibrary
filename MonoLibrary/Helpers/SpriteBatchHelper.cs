using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

namespace MonoLibrary.Helpers;

public static class SpriteBatchHelper
{
    private static Texture2D pixel;

    private static void Init(GraphicsDevice graphicsDevice)
    {
        pixel ??= graphicsDevice.CreateSquare(1);
    }

    public static void DrawRectFill(this SpriteBatch spriteBatch, Rectangle rect, Color? color = null, float layerDepth = 0f)
    {
        Init(spriteBatch.GraphicsDevice);

        color ??= Color.White;
        spriteBatch.Draw(pixel, rect, null, color!.Value, 0, Vector2.Zero, SpriteEffects.None, layerDepth);
    }

    public static void DrawRectStroke(this SpriteBatch spriteBatch, Rectangle rect, int thicness = 1, Color? color = null, float layerDepth = 0f)
    {
        Init(spriteBatch.GraphicsDevice);

        color ??= Color.Black;
        var scale = new Vector2(rect.Width, thicness);
        var offset = new Vector2(0, thicness / 2f);

        // TOP
        spriteBatch.Draw(pixel, rect.Location.ToVector2() - offset, null, color.Value, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        // DOWN
        spriteBatch.Draw(pixel, rect.Location.ToVector2() - offset + new Vector2(0, rect.Height), null, color.Value, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);

        scale = new Vector2(thicness, rect.Height);
        offset = new Vector2(offset.Y, offset.X);
        // LEFT
        spriteBatch.Draw(pixel, rect.Location.ToVector2() - offset, null, color.Value, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        // RIGHT
        spriteBatch.Draw(pixel, rect.Location.ToVector2() - offset + new Vector2(rect.Width, 0), null, color.Value, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
    }

    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, int thickness = 1, Color? color = null)
    {
        Init(spriteBatch.GraphicsDevice);

        var diff = p2 - p1;
        spriteBatch.DrawLine(p1, diff, diff.Length(), thickness, color);
    }

    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 position, Vector2 direction, float length, int thickness = 1, Color? color = null)
    {
        Init(spriteBatch.GraphicsDevice);

        color ??= Color.Black;

        if (direction != Vector2.Zero)
            direction.Normalize();

        var scale = new Vector2(length, thickness);
        var rotation = MathF.Atan2(direction.Y, direction.X); // its in rad
        spriteBatch.Draw(pixel, position, null, color.Value, rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
    }

    public static void DrawCircleStroke(this SpriteBatch spriteBatch, Vector2 center, float radius, int points = 32, int thickness = 1, Color? color = null)
    {
        const int minPoints = 32, maxPoints = 512;
        points = MathHelper.Clamp(points, minPoints, maxPoints);

        float rotation = MathHelper.TwoPi / points;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        float ax = radius;
        float ay = 0;

        for (int i = 0; i < points; i++)
        {
            float bx = cos * ax - sin * ay;
            float by = sin * ax + cos * ay;

            spriteBatch.DrawLine(new Vector2(ax + center.X, ay + center.Y), new Vector2(bx + center.X, by + center.Y), thickness, color);

            ax = bx;
            ay = by;
        }
    }
}

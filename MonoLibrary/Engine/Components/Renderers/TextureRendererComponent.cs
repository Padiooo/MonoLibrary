using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Renderers
{
    public class TextureRendererComponent : IDrawComponent
    {
        const float MaxInt = (float)int.MaxValue;

        public GameObject Owner { get; }

        public Texture2D Texture { get; set; }

        public Color Color { get; set; }
        public Vector2 Offset { get; set; }
        public Rectangle? SourceRectangle { get; set; } = null;

        public Vector2 Scale { get; set; } = Vector2.One;

        public int Layer { get; set; } = int.MaxValue;

        public float LayerDepth => Layer / MaxInt;

        public TextureRendererComponent(GameObject owner)
        {
            Owner = owner;
        }

        public virtual void Draw(float time, SpriteBatch spriteBatch)
        {
            if (Texture is null)
                return;

            var position = Owner.Position;
            var origin = SourceRectangle is null ? Texture.Bounds.Size.ToVector2() / 2 : SourceRectangle.Value.Size.ToVector2() / 2;
            spriteBatch.Draw(Texture, position + Offset, SourceRectangle, Color, 0, origin, Scale, SpriteEffects.None, LayerDepth);
        }
    }
}

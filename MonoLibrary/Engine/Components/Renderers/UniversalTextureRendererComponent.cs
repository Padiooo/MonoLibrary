using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Renderers;

/// <summary>
/// Force the base rendering size to be of <see cref="Size"/> rather than based on <see cref="TextureRendererComponent.Texture"/> size.
/// </summary>
public class UniversalTextureRendererComponent(GameObject owner) : TextureRendererComponent(owner)
{
    public Vector2? Size { get; set; }

    public override void Draw(float time, SpriteBatch spriteBatch)
    {
        if (Texture is null)
            return;

        var position = Owner.Position;
        var origin = SourceRectangle is null ? Texture.Bounds.Size.ToVector2() / 2f : SourceRectangle.Value.Size.ToVector2() / 2f;
        var targetSize = Size ?? Texture.Bounds.Size.ToVector2();
        var destination = new Rectangle((position + Offset).ToPoint(), Vector2.Multiply(targetSize, Scale).ToPoint());
        spriteBatch.Draw(Texture, destination, SourceRectangle, Color, 0f, origin, SpriteEffects.None, LayerDepth);
    }
}

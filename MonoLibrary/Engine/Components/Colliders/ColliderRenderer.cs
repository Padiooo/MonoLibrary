using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Colliders;

public abstract class ColliderRenderer(GameObject owner) : IDrawComponent
{
    public GameObject Owner { get; } = owner;

    public virtual Color Color { get; set; } = Color.Yellow;

    public abstract void Draw(float time, SpriteBatch spriteBatch);
}

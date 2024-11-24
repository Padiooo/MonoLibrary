using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Objects;
using MonoLibrary.Helpers;

namespace MonoLibrary.Engine.Components.Colliders;

public class CircleColliderRenderer(GameObject owner, CircleColliderComponent collider) : ColliderRenderer(owner)
{
    public CircleColliderComponent Collider { get; } = collider;

    public override void Draw(float time, SpriteBatch spriteBatch)
    {
        spriteBatch.DrawCircleStroke(Collider.Offset + Owner.Position, Collider.Radius, color: Color);
    }
}

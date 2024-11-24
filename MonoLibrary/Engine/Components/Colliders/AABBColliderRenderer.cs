using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Objects;
using MonoLibrary.Helpers;

namespace MonoLibrary.Engine.Components.Colliders;

public class AABBColliderRenderer(GameObject owner, AABBColliderComponent collider) : ColliderRenderer(owner)
{
    public AABBColliderComponent Collider { get; } = collider;

    public override void Draw(float time, SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectStroke(Collider.Bounds, 1, color: Color);
    }
}

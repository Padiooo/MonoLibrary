using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Colliders;

public class AABBColliderComponent(GameObject owner) : ColliderComponent(owner)
{
    public override Rectangle Bounds => new((Offset + Owner.Position - Size / 2).ToPoint(), Size.ToPoint());

    public Vector2 Offset { get; set; }
    public Vector2 Size { get; set; }

    public override bool IsColliding(IColliderComponent other)
    {
        return other switch
        {
            CircleColliderComponent circle => ColliderResolver.IsColliding(this, circle),
            AABBColliderComponent aabb => ColliderResolver.IsColliding(aabb, this),
            CompositeColliderComponent composite => composite.IsColliding(this),
            _ => false,
        };
    }

    public override AABBColliderRenderer GetRenderer()
    {
        return new AABBColliderRenderer(Owner, this);
    }
}

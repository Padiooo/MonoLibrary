using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Colliders;

public class CircleColliderComponent(GameObject owner) : ColliderComponent(owner)
{
    public override Rectangle Bounds => new((Offset + Owner.Position - new Vector2(Radius)).ToPoint(), new Point((int)Radius * 2));
    public Vector2 Offset { get; set; }
    public float Radius { get; set; }

    public override bool IsColliding(IColliderComponent other)
    {
        if (other is CircleColliderComponent circle)
            return ColliderResolver.IsColliding(this, circle);
        else if (other is AABBColliderComponent aabb)
            return ColliderResolver.IsColliding(aabb, this);
        else if (other is CompositeColliderComponent composite)
            return composite.IsColliding(this);

        return false;
    }

    public override ColliderRenderer GetRenderer()
    {
        return new CircleColliderRenderer(Owner, this);
    }
}

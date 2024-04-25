using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Colliders
{
    public class AABBColliderComponent : ColliderComponent
    {
        public override Rectangle Bounds => new((Offset + Owner.Position - Size / 2).ToPoint(), Size.ToPoint());

        public Vector2 Offset { get; set; }
        public Vector2 Size { get; set; }

        public AABBColliderComponent(GameObject owner) : base(owner)
        {

        }

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

        public override AABBColliderRenderer GetRenderer()
        {
            return new AABBColliderRenderer(Owner, this);
        }
    }
}

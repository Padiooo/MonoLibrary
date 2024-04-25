using Microsoft.Xna.Framework;

using System;

namespace MonoLibrary.Engine.Components.Colliders
{
    public static class ColliderResolver
    {
        public static bool IsColliding(CircleColliderComponent circle1, CircleColliderComponent cirlc2)
        {
            var d = circle1.Owner.Position - cirlc2.Owner.Position;
            if (d == Vector2.Zero)
                return true;

            float r = circle1.Radius + cirlc2.Radius;

            return d.LengthSquared() <= r * r;
        }

        public static bool IsColliding(AABBColliderComponent aabb1, AABBColliderComponent aabb2)
        {
            return aabb1.Bounds.Intersects(aabb2.Bounds);
        }

        public static bool IsColliding(AABBColliderComponent aabb, CircleColliderComponent circle)
        {
            var rect = aabb.Bounds;
            float Xn = Math.Max(rect.Left, Math.Min(circle.Owner.Position.X, rect.Right));
            float Yn = Math.Max(rect.Top, Math.Min(circle.Owner.Position.Y, rect.Bottom));

            return (new Vector2(Xn, Yn) - circle.Owner.Position).LengthSquared() <= circle.Radius * circle.Radius;
        }
    }
}

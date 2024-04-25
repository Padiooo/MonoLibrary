using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Objects;
using MonoLibrary.Helpers;

namespace MonoLibrary.Engine.Components.Colliders
{
    public class CircleColliderRenderer : ColliderRenderer
    {
        public CircleColliderComponent Collider { get; }

        public CircleColliderRenderer(GameObject owner, CircleColliderComponent collider) : base(owner)
        {
            Collider = collider;
        }

        public override void Draw(float time, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawCircleStroke(Collider.Offset + Owner.Position, Collider.Radius, color: Color);
        }
    }
}

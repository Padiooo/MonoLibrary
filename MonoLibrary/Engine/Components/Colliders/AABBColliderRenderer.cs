using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Objects;
using MonoLibrary.Helpers;

namespace MonoLibrary.Engine.Components.Colliders
{
    public class AABBColliderRenderer : ColliderRenderer
    {
        public AABBColliderComponent Collider { get; }

        public AABBColliderRenderer(GameObject owner, AABBColliderComponent collider)
            : base(owner)
        {
            Collider = collider;
        }

        public override void Draw(float time, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectStroke(Collider.Bounds, 1, color: Color);
        }
    }
}

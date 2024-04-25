using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Objects;
using MonoLibrary.Helpers;

using System.Collections.Generic;

namespace MonoLibrary.Engine.Components.Colliders
{
    public class CompositeColliderRenderer : ColliderRenderer
    {
        public CompositeColliderComponent Collider { get; }
        private readonly ColliderRenderer[] _renderers;

        private Color compositeColor = Color.Blue;
        public Color CompositeColor
        {
            get => compositeColor;
            set
            {
                foreach (var renderer in _renderers)
                    renderer.Color = value;
                compositeColor = value;
            }
        }

        public CompositeColliderRenderer(GameObject owner, CompositeColliderComponent collider)
            : base(owner)
        {
            Collider = collider;
            var renderers = new List<ColliderRenderer>();
            var q = new Queue<ColliderComponent>(collider.Colliders);

            while (q.Count > 0)
            {
                var current = q.Dequeue();
                var renderer = current.GetRenderer();
                renderer.Color = compositeColor;
                renderers.Add(renderer);
            }

            _renderers = renderers.ToArray();
        }

        public override void Draw(float time, SpriteBatch spriteBatch)
        {
            foreach (var renderer in _renderers)
                renderer.Draw(time, spriteBatch);

            spriteBatch.DrawRectStroke(Collider.Bounds, color: Color);
        }
    }
}

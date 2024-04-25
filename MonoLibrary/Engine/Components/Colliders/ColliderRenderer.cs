using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Colliders
{
    public abstract class ColliderRenderer : IDrawComponent
    {
        public GameObject Owner { get; }

        public virtual Color Color { get; set; } = Color.Yellow;

        protected ColliderRenderer(GameObject owner)
        {
            Owner = owner;
        }

        public abstract void Draw(float time, SpriteBatch spriteBatch);
    }
}

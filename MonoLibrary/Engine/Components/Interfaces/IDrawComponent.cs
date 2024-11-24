using Microsoft.Xna.Framework.Graphics;

namespace MonoLibrary.Engine.Components.Interfaces;

public interface IDrawComponent : IComponent
{
    void Draw(float time, SpriteBatch spriteBatch);
}

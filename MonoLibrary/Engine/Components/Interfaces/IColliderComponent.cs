using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Components.Colliders;

namespace MonoLibrary.Engine.Components.Interfaces
{
    public interface IColliderComponent : IUpdateComponent
    {
        Rectangle Bounds { get; }

        Layer Layer { get; }

        void OnCollide(IColliderComponent other);

        bool IsColliding(IColliderComponent other);
    }
}

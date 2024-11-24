using MonoLibrary.Engine.Components.Interfaces;

using System.Collections.Generic;

namespace MonoLibrary.Engine.Services.Collision.Algorithms;

public interface ICollisionAlgorithm
{
    void Resolve(IList<IColliderComponent> colliders);

    IEnumerable<IColliderComponent> Query(IColliderComponent area, IList<IColliderComponent> colliders);
}

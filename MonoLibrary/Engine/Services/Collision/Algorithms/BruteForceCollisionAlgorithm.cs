using MonoLibrary.Engine.Components.Interfaces;

using System.Collections.Generic;

namespace MonoLibrary.Engine.Services.Collision.Algorithms;

public class BruteForceCollisionAlgorithm : ICollisionAlgorithm
{
    public void Resolve(IList<IColliderComponent> colliders)
    {
        for (int i = 0; i < colliders.Count - 1; i++)
        {
            var collider = colliders[i];
            for (int j = i + 1; j < colliders.Count; j++)
            {
                var other = colliders[j];

                if (collider.Layer.IsInterested(other.Layer))
                {
                    if (collider.Bounds.Intersects(other.Bounds))
                        if (collider.IsColliding(other))
                        {
                            collider.OnCollide(other);
                            other.OnCollide(collider);
                        }
                }
            }
        }
    }

    public IEnumerable<IColliderComponent> Query(IColliderComponent area, IList<IColliderComponent> colliders)
    {
        foreach (var collider in colliders)
            if (collider.Layer.IsInterested(area.Layer))
                if (collider.IsColliding(area))
                    yield return collider;
    }
}

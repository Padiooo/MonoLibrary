using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoLibrary.Engine.Components.Colliders;

public class CompositeColliderComponent(GameObject owner, params ColliderComponent[] colliders) : ColliderComponent(owner)
{
    public IReadOnlyCollection<ColliderComponent> Colliders { get; } = new ReadOnlyCollection<ColliderComponent>(colliders);

    public override Rectangle Bounds
    {
        get
        {
            int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;

            foreach (var collider in Colliders)
            {
                left = Math.Min(left, collider.Bounds.Left);
                top = Math.Min(top, collider.Bounds.Top);

                right = Math.Max(right, collider.Bounds.Right);
                bottom = Math.Max(bottom, collider.Bounds.Bottom);
            }

            return new(left, top, right - left, bottom - top);
        }
    }

    public override bool IsColliding(IColliderComponent other)
    {
        foreach (var collider in Colliders)
            if (collider.IsColliding(other))
                return true;

        return false;
    }

    public override ColliderRenderer GetRenderer()
    {
        return new CompositeColliderRenderer(Owner, this);
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Objects;
using MonoLibrary.Engine.Services.Collision;

using System;
using System.Collections.Generic;

namespace MonoLibrary.Engine.Components.Colliders;

public delegate void CollisionHandler(IColliderComponent me, IColliderComponent other);

public abstract class ColliderComponent : IColliderComponent
{
    private readonly IDisposable _subscription;

    private HashSet<IColliderComponent> _oldColliders = [];
    private readonly HashSet<IColliderComponent> _colliders = [];

    public GameObject Owner { get; }

    public Layer Layer { get; set; } = new();

    public abstract Rectangle Bounds { get; }

    public event CollisionHandler OnCollisionEnter;
    public event CollisionHandler OnColliding;
    public event CollisionHandler OnCollisionEnd;

    protected ColliderComponent(GameObject owner)
    {
        Owner = owner;
        var collisionService = owner.Game.Services.GetService<ICollisionService>();
        _subscription = collisionService?.Register(this);
    }

    public virtual void Update(float time)
    {
        var lost = new HashSet<IColliderComponent>(_oldColliders);
        lost.ExceptWith(_colliders);

        foreach (var collider in lost)
            OnCollisionEnd?.Invoke(this, collider);

        _oldColliders = new HashSet<IColliderComponent>(_colliders);
        _colliders.Clear();
    }


    public abstract bool IsColliding(IColliderComponent other);
    public abstract ColliderRenderer GetRenderer();

    public void OnCollide(IColliderComponent other)
    {
        if (_colliders.Add(other) && !_oldColliders.Contains(other))
            OnCollisionEnter?.Invoke(this, other);
        else
            OnColliding?.Invoke(this, other);
    }

    public void OnDestroy()
    {
        _subscription?.Dispose();
    }
}

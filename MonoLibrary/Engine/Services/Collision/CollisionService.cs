using Microsoft.Extensions.Logging;

using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Services.Collision.Algorithms;
using MonoLibrary.Engine.Services.Helpers;
using MonoLibrary.Engine.Services.Updates;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MonoLibrary.Engine.Services.Collision
{
    public interface ICollisionService : IUpdaterService
    {
        IDisposable Register(IColliderComponent component);

        IEnumerable<IColliderComponent> Query(IColliderComponent area, IList<IColliderComponent> colliders);
    }

    public class CollisionService : ICollisionService
    {
        private readonly IDisposable _subscription;
        private readonly ICollisionAlgorithm _algorithm;
        private readonly ILogger _logger;

        private readonly List<IColliderComponent> _colliders = new();
        private readonly List<IColliderComponent> _toRemove = new();

        public CollisionService(IUpdateLoop updater, ICollisionAlgorithm algorithm, ILogger<CollisionService> logger)
        {
            _subscription = updater.Register(this);
            _algorithm = algorithm;
            _logger = logger;
        }

        public IDisposable Register(IColliderComponent component)
        {
            _colliders.Add(component);

            _logger.LogTrace("Registered new collider from GameObject {id}. Count {count}.", component.Owner.Id, _colliders.Count);

            return new Subscription<IColliderComponent>(component, _toRemove.Add);
        }

        public void Update(float deltaTime) { }

        public void AfterUpdate()
        {
            foreach (var toRemove in _toRemove)
            {
                _colliders.Remove(toRemove);
                _logger.LogTrace("Unregistered collider from GameObject {id}. Count {count}.", toRemove.Owner.Id, _colliders.Count);
            }

            _toRemove.Clear();

            _algorithm.Resolve(_colliders);
        }

        public IEnumerable<IColliderComponent> Query(IColliderComponent area, IList<IColliderComponent> colliders)
        {
            return _algorithm.Query(area, colliders);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}

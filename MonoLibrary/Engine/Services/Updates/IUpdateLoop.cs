using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Objects;
using MonoLibrary.Engine.Services.Helpers;

using System;
using System.Collections.Generic;

namespace MonoLibrary.Engine.Services.Updates
{
    /// <summary>
    /// Must be thread safe.
    /// </summary>
    public interface IUpdateLoop
    {
        /// <summary>
        /// Registers the <paramref name="updatableService"/>.
        /// </summary>
        /// <param name="updatableService"></param>
        /// <returns>An <see cref="IDisposable"/> to unregister.</returns>
        IDisposable Register(IUpdaterService updatableService);

        /// <summary>
        /// Ran at the beginning of <see cref="Game.Update(GameTime)"/>.
        /// </summary>
        void BeforeUpdate();

        /// <summary>
        /// Ran just before <see cref="Game.Update(GameTime)"/>, hence before <see cref="GameObject.Update(GameTime)"/>.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Ran at then end of <see cref="Game.Update(GameTime)"/>.
        /// </summary>
        void AfterUpdate();
    }

    public class ServiceUpdater : IUpdateLoop
    {
        private readonly ILogger _logger;
        private readonly List<IUpdaterService> _services = new();

        public ServiceUpdater(ILogger<ServiceUpdater> logger)
        {
            _logger = logger;
        }

        public IDisposable Register(IUpdaterService updatableService)
        {
            _services.Add(updatableService);

            _logger.LogInformation("Registered {interface}: {type}. Total: {count}.", nameof(IUpdaterService), updatableService.GetType().Name, _services.Count);

            return new Subscription<IUpdaterService>(updatableService, Remove);
        }

        public void BeforeUpdate()
        {
            int count = _services.Count;
            for (int i = 0; i < Math.Max(count, _services.Count); i++)
                _services[i].BeforeUpdate();
        }

        public void Update(float deltaTime)
        {
            int count = _services.Count;
            for (int i = 0; i < Math.Max(count, _services.Count); i++)
                _services[i].Update(deltaTime);
        }

        public void AfterUpdate()
        {
            int count = _services.Count;
            for (int i = 0; i < Math.Max(count, _services.Count); i++)
                _services[i].AfterUpdate();
        }

        private void Remove(IUpdaterService updatableService)
        {
            _services.Remove(updatableService);

            _logger.LogInformation("Removed {interface}: {type}. Total: {count}", nameof(IUpdaterService), updatableService.GetType().Name, _services.Count);
        }
    }
}

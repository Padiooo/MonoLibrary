using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Objects;

using System;


namespace MonoLibrary.Engine.Services.Updates
{
    public interface IUpdaterService : IDisposable
    {
        /// <summary>
        /// Ran after <see cref="Game.BeginRun"/>.
        /// </summary>
        void BeforeUpdate() { }

        /// <summary>
        /// Ran before <see cref="Game.Update(GameTime)"/>, hence before <see cref="GameObject.Update(GameTime)"/>.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Ran before <see cref="Game.EndRun"/>.
        /// </summary>
        void AfterUpdate() { }
    }
}

using System;

namespace MonoLibrary.Engine.Services
{
    public interface IGameStateHub
    {
        event Action Exiting;
    }

    public sealed class GameStateHub : IGameStateHub
    {
        public event Action Exiting;

        public void OnExit() => Exiting?.Invoke();
    }
}

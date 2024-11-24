using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Input;

using MonoLibrary.Engine.Services.Updates;

using System;
using System.Collections.Concurrent;

namespace MonoLibrary.Engine.Services.Inputs.KeyboardInputs;

public class KeyboardService : IKeyboardInputService
{
    private readonly IDisposable _subscription;
    private readonly GameEngine _game;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Keys, KeyInput> _inputs = new();

    public KeyboardService(GameEngine game, IUpdateLoop updater, ILogger<IKeyboardInputService> logger)
    {
        _game = game;
        _logger = logger;
        _subscription = updater.Register(this);
    }

    public IInput CreateInput(Keys key)
    {
        var input = _inputs.GetOrAdd(key, (k, dict) =>
        {
            _logger.LogInformation("Created input for {key}. Total inputs {count}.", k, dict.Count + 1);

            return new KeyInput()
            {
                Key = key,
            };
        }, _inputs);

        return input;
    }

    public void BeforeUpdate()
    {
        bool isActive = _game.IsActive;
        var state = Keyboard.GetState();
        foreach (var input in _inputs.Values)
            input.Update(state.IsKeyDown(input.Key) && isActive);
    }

    public void Update(float deltaTime) { }

    public void Dispose()
    {
        _subscription?.Dispose();
    }

    private class KeyInput : IInput
    {
        private bool _down;
        private bool _pressed;

        public Keys Key { get; set; }
        public bool IsDown => _down;
        public bool IsUp => !_down;
        public bool IsPressed => _pressed;

        public event Action Down;
        public event Action Up;
        public event Action Pressed;

        public void Update(bool down)
        {
            _pressed = false;
            bool wasDown = _down;
            _down = down;

            if (wasDown && !_down)
            {
                Up?.Invoke();
                Pressed?.Invoke();
                _pressed = true;
            }
            else if (!wasDown && _down)
                Down?.Invoke();
        }
    }
}

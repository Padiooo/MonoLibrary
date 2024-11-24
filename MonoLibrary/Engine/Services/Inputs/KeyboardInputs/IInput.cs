using Microsoft.Xna.Framework.Input;

using System;

namespace MonoLibrary.Engine.Services.Inputs.KeyboardInputs;

public interface IInput
{
    Keys Key { get; set; }

    bool IsDown { get; }
    bool IsUp { get; }

    /// <summary>
    /// <see langword="true"/> only when <see cref="IsDown"/> was <see langword="true"/> and become <see langword="false"/>.
    /// </summary>
    bool IsPressed { get; }

    /// <summary>
    /// Called only when it turns from <see langword="false"/> to <see langword="true"/>.
    /// </summary>
    event Action Down, Up, Pressed;
}

using Microsoft.Xna.Framework.Input;

using MonoLibrary.Engine.Services.Updates;

namespace MonoLibrary.Engine.Services.Inputs.KeyboardInputs;

public interface IKeyboardInputService : IUpdaterService
{
    IInput CreateInput(Keys key);
}

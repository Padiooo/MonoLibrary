using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Components.Interfaces
{
    public interface IComponent
    {
        GameObject Owner { get; }

        void OnDestroy() { }
    }
}

using System;

namespace MonoLibrary.Engine.Network.Components;

public interface IInvokable : IDisposable
{
    void Invoke();
}

public interface IInvokable<T> : IDisposable
    where T : struct
{
    void Invoke(T data);
}

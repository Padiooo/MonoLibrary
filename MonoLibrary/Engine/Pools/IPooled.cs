using System;

namespace MonoLibrary.Engine.Pools;

public interface IPooled<T> : IDisposable
{
    T Item { get; }
}

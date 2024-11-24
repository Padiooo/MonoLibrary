using System;
using System.Runtime.CompilerServices;

namespace MonoLibrary.Engine.Pools;

[SkipLocalsInit]
public readonly struct Pooled<T>(Action<T> returnPool, T item) : IPooled<T>
{
    private readonly Action<T> _return = returnPool;

    public T Item { get; } = item;

    public void Dispose()
    {
        _return.Invoke(Item);
    }
}

using System;

namespace MonoLibrary.Engine.Services.Helpers;

public readonly struct Subscription<T>(T item, Action<T> unsubscribe) : IDisposable
{
    private readonly T _item = item;
    private readonly Action<T> _unsubscribe = unsubscribe;

    public void Dispose()
    {
        _unsubscribe?.Invoke(_item);
    }
}

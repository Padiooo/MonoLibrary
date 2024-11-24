using System;
using System.Collections.Generic;

namespace MonoLibrary.Engine.Pools;

public class Pool<T>
{
    private readonly Queue<T> _queue;
    private readonly Func<T> _factory;
    private readonly Action<T> _reset;

    public Pool(Func<T> factory, Action<T> reset, int initialCount)
    {
        _queue = new Queue<T>();
        _factory = factory;
        _reset = reset;
        for (int i = 0; i < initialCount; i++)
            _queue.Enqueue(factory.Invoke());
    }

    public T Get()
    {
        return _queue.Count > 0 ? _queue.Dequeue() : _factory.Invoke();
    }

    public void Return(T item)
    {
        _reset.Invoke(item);
        _queue.Enqueue(item);
    }
}

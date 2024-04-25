using System;
using System.Runtime.CompilerServices;

namespace MonoLibrary.Engine.Pools
{
    [SkipLocalsInit]
    public readonly struct Pooled<T> : IPooled<T>
    {
        private readonly Action<T> _return;
        public T Item { get; }

        public Pooled(Action<T> returnPool, T item)
        {
            _return = returnPool;
            Item = item;
        }

        public void Dispose()
        {
            _return.Invoke(Item);
        }
    }
}

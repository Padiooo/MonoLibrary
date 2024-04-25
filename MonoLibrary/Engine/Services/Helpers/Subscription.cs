using System;

namespace MonoLibrary.Engine.Services.Helpers
{
    public readonly struct Subscription<T> : IDisposable
    {
        private readonly T _item;
        private readonly Action<T> _unsubscribe;

        public Subscription(T item, Action<T> unsubscribe)
        {
            _item = item;
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe?.Invoke(_item);
        }
    }
}

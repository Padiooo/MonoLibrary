using PubSub;

using System;

namespace MonoLibrary.EventBus;


public static class PubSubExtensions
{
    public static IDisposable Register<T>(this Hub hub, object subscriber, Action<T> action)
    {
        return new HubRegistration<T>(hub, subscriber, action);
    }

    private readonly struct HubRegistration<T> : IDisposable
    {
        private readonly Hub _hub;
        private readonly object _subscriber;
        private readonly Action<T> _action;

        public HubRegistration(Hub hub, object subscriber, Action<T> action)
        {
            _hub = hub;
            _subscriber = subscriber;
            _action = action;

            _hub.Subscribe(subscriber, action);
        }

        public void Dispose()
        {
            _hub.Unsubscribe(_subscriber, _action);
        }
    }
}

using MonoLibrary.Engine.Components.Interfaces;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonoLibrary.Engine.Objects;

public class ComponentCollection(GameObject owner) : IEnumerable<IComponent>
{
    private readonly Dictionary<Type, IComponent> _components = [];

    internal readonly List<IUpdateComponent> updateComponents = [];
    internal readonly List<IDrawComponent> drawComponents = [];

    private bool delayModifications;
    internal bool DelayModifications
    {
        set
        {
            delayModifications = value;
            if (!value)
                ExecuteModifications();
        }
    }

    private readonly Queue<IOperation> _operations = new();

    public GameObject Owner { get; } = owner;

    public event Action<IComponent> OnComponentAdded;
    public event Action<IComponent> OnComponentRemoved;

    private void ExecuteModifications()
    {
        while (_operations.Count > 0)
            _operations.Dequeue().Execute();
    }

    public void Add<T>(T component) where T : class, IComponent
    {
        var op = new AddComponentOperation<T>(this, component);

        if (delayModifications)
            _operations.Enqueue(op);
        else
            op.Execute();
    }

    public void Remove<T>() where T : class, IComponent
    {
        var op = new RemoveComponentOperation<T>(this);

        if (delayModifications)
            _operations.Enqueue(op);
        else
            op.Execute();
    }

    public T Get<T>() where T : class, IComponent
    {
        if (_components.TryGetValue(typeof(T), out var component))
            return (T)component;

        return null;
    }

    public bool TryGet<T>([MaybeNullWhen(false)] out T component) where T : class, IComponent
    {
        var result = _components.TryGetValue(typeof(T), out var c);
        component = result ? (T)c : null;
        return result;
    }

    internal void Destroy()
    {
        _operations.Enqueue(new DestroyOperation(this));
    }

    public IEnumerator<IComponent> GetEnumerator() => _components.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private interface IOperation
    {
        void Execute();
    }

    [SkipLocalsInit]
    private readonly struct AddComponentOperation<T>(ComponentCollection collection, T component) : IOperation
        where T : class, IComponent
    {
        readonly ComponentCollection Collection = collection;
        readonly T Component = component;

        public readonly void Execute()
        {
            Collection._components.Add(typeof(T), Component);

            if (Component is IUpdateComponent update)
                Collection.updateComponents.Add(update);
            if (Component is IDrawComponent draw)
                Collection.drawComponents.Add(draw);

            Collection.OnComponentAdded?.Invoke(Component);
        }
    }

    [SkipLocalsInit]
    private readonly struct RemoveComponentOperation<T>(ComponentCollection collection) : IOperation
        where T : class, IComponent
    {
        readonly ComponentCollection Collection = collection;

        public readonly void Execute()
        {
            if (Collection._components.Remove(typeof(T), out var component))
            {
                if (component is IUpdateComponent update)
                    Collection.updateComponents.Remove(update);
                if (component is IDrawComponent draw)
                    Collection.drawComponents.Remove(draw);

                Collection.OnComponentRemoved?.Invoke(component);
            }
        }
    }

    private readonly struct DestroyOperation(ComponentCollection collection) : IOperation
    {
        readonly ComponentCollection Collection = collection;

        public readonly void Execute()
        {
            foreach (var component in Collection._components.Values)
            {
                component.OnDestroy();
                Collection.OnComponentRemoved?.Invoke(component);
            }

            Collection._components.Clear();
            Collection.updateComponents.Clear();
            Collection.drawComponents.Clear();
        }
    }
}

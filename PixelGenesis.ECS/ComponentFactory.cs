using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis.GameLogic;

public sealed class ComponentFactory
{
    EntityManager EntityManager;

    internal ComponentFactory(EntityManager entityManager)
    {
        EntityManager = entityManager;
    }

    Dictionary<Type, Func<Entity, IComponent>> ComponentFactories = new Dictionary<Type, Func<Entity, IComponent>>();
    
    public void AddComponentFactory(Type type, Func<Entity, IComponent> factory)
    {
        ComponentFactories.Add(type, factory);
        Vector3 vector = new Vector3(0, 0, 0);
        Vector3.Distance(vector, vector);
        
    }

    public T CreateComponent<T>(Entity container) where T : class, IComponent
    {
       return Unsafe.As<T>(CreateComponent(container, typeof(T)));
    }

    public T CreateComponentIfNotExists<T>(Entity container) where T : class, IComponent
    {
        return Unsafe.As<T>(CreateComponentIfNotExists(container, typeof(T)));
    }
        
    public IComponent CreateComponent(Entity container, Type type)
    {        
        if (!ComponentFactories.TryGetValue(type, out var factory))
        {
            throw new InvalidOperationException($"Component of type {type.FullName}, does not contain a factory.");
        }

        var component = factory(container);
        component.Entity = container;

        container.AddComponent(component);

        return component;
    }

    public IComponent CreateComponentIfNotExists(Entity container, Type type)
    {        
        if (container.TryGetComponent(type, out var component))
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return component;
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        if (!ComponentFactories.TryGetValue(type, out var factory))
        {
            throw new InvalidOperationException($"Component of type {type.FullName}, does not contain a factory, make sure the source generator is referenced.");
        }

        component = factory(container);
        container.AddComponent(component);

        return component;
    }
}

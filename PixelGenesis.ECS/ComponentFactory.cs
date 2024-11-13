using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis.ECS;

public sealed class ComponentFactory
{
    EntityManager EntityManager;

    internal ComponentFactory(EntityManager entityManager)
    {
        EntityManager = entityManager;
    }

    static Dictionary<Type, Func<Entity, Component>> ComponentFactories = new Dictionary<Type, Func<Entity, Component>>();
    
    public static void AddComponentFactory(Type type, Func<Entity, Component> factory)
    {
        ComponentFactories.Add(type, factory);       
    }

    public T CreateComponent<T>(Entity container) where T : Component
    {
       return Unsafe.As<T>(CreateComponent(container, typeof(T)));
    }

    public T CreateComponentIfNotExists<T>(Entity container) where T : Component
    {
        return Unsafe.As<T>(CreateComponentIfNotExists(container, typeof(T)));
    }
        
    public Component CreateComponent(Entity container, Type type)
    {        
        if (!ComponentFactories.TryGetValue(type, out var factory))
        {
            throw new InvalidOperationException($"Component of type {type.FullName}, does not contain a factory.");
        }

        var component = factory(container);
        component._entity = container;

        container.AddComponent(component);

        return component;
    }

    public Component CreateComponentIfNotExists(Entity container, Type type)
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

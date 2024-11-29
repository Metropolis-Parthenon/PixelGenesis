using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis.ECS.Components;

public sealed class ComponentsFactory(IServiceProvider provider)
{    
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
        var factory = provider.GetRequiredKeyedService<IComponentFactory>(type);

        var component = factory.CreateComponent(new ComponentDependencyResolver(container, provider));
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

        var factory = provider.GetRequiredKeyedService<IComponentFactory>(type);

        component = factory.CreateComponent(new ComponentDependencyResolver(container, provider));
        container.AddComponent(component);
        component._entity = container;

        return component;
    }
}

public interface IComponentFactory
{
    public Component CreateComponent(ComponentDependencyResolver resolver);
}

public struct ComponentDependencyResolver(Entity entity, IServiceProvider provider)
{
    public T Resolve<T>() where T : class
    {
        var type = typeof(T);
        return Unsafe.As<T>(Resolve(type));
    }

    public object Resolve(Type type)
    {
        if(type.IsAssignableTo(typeof(Component)))
        {
            return entity.AddComponentIfNotExist(type);
        }

        return provider.GetRequiredService(type);
    }

}
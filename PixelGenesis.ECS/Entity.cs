using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PixelGenesis.ECS;

public sealed partial class Entity
{
    EntityManager EntityManager;

    public string Name { get; internal set; } = "";

    public ImmutableArray<string> Tags { get; internal set; }

    public bool IsDisabled {  get; set; }

    internal Entity(EntityManager entityManager) 
    { 
        EntityManager = entityManager;
    }

    public int Id { get; internal set; }
    
    public ReadOnlySpan<Component> GetComponents()
    {        
        return Components.ValuesAsSpan();
    }

    public void RemoveComponent<T>()
    {
        RemoveComponent(typeof(T));
    }

    public void RemoveComponent(Type type)
    {
        var component = GetComponent(type);
        EntityManager.RemoveComponentFromEntity(component);
        Components.Remove(type.GUID);
    }

    public T AddComponentIfNotExist<T>() where T : Component
    {
        return EntityManager.ComponentFactory.CreateComponentIfNotExists<T>(this);
    }

    public Component AddComponentIfNotExist(Type type)
    {
        return EntityManager.ComponentFactory.CreateComponentIfNotExists(this, type);
    }

    public T AddComponent<T>() where T : Component
    {
        return EntityManager.ComponentFactory.CreateComponent<T>(this);
    }

    public Component AddComponent(Type type)
    {
        return EntityManager.ComponentFactory.CreateComponent(this, type);
    }

    public T GetComponent<T>() where T : Component
    {
        return Unsafe.As<T>(GetComponent(typeof(T)));
    }

    public Component GetComponent(Type type)
    {
        if (Components.TryGetValue(type.GUID, out var component))
        {
            return component;
        }

        throw new InvalidOperationException($"Entity Does not contain Component with type {type.FullName}.");
    }

    public bool TryGetComponent<T>([MaybeNullWhen(false)] out T component) where T : Component
    {
        if (TryGetComponent(typeof(T), out var comp))
        {
            component = Unsafe.As<T>(comp);
            return true;
        }

        component = default;
        return false;
    }

}

public sealed partial class Entity
{
    SortedList<Guid, Component> Components = new SortedList<Guid, Component>();

    internal void Clear()
    {
        Components.Clear();
    }

    internal void AddComponent(Component component)
    {
        Components.Add(component.GetType().GUID, component);
        EntityManager.AddComponentToEntity(component);
    }

    internal bool TryGetComponent(Type type, [MaybeNullWhen(false)] out Component component)
    {
        return Components.TryGetValue(type.GUID, out component);
    }
}
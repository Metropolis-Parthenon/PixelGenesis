using PixelGenesis.ECS;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PixelGenesis.GameLogic;

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
    
    public ReadOnlySpan<IComponent> GetComponents()
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
        Components.Remove(type);
    }

    public T AddComponent<T>() where T : class, IComponent
    {
        return EntityManager.ComponentFactory.CreateComponent<T>(this);
    }

    public IComponent AddComponent(Type type)
    {
        return EntityManager.ComponentFactory.CreateComponent(this, type);
    }

    public T GetComponent<T>() where T : class, IComponent
    {
        return Unsafe.As<T>(GetComponent(typeof(T)));
    }

    public IComponent GetComponent(Type type)
    {
        if (Components.TryGetValue(type, out var component))
        {
            return component;
        }

        throw new InvalidOperationException($"Entity Does not contain Component with type {type.FullName}.");
    }

    public bool TryGetComponent<T>([MaybeNullWhen(false)] out T component) where T : class, IComponent
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
    SortedList<Type, IComponent> Components = new SortedList<Type, IComponent>();

    internal void Clear()
    {
        Components.Clear();
    }

    internal void AddComponent(IComponent component)
    {
        Components.Add(component.GetType(), component);
    }

    internal bool TryGetComponent(Type type, [MaybeNullWhen(false)] out IComponent component)
    {
        return Components.TryGetValue(type, out component);
    }
}
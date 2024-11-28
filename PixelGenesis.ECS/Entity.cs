using CommunityToolkit.HighPerformance;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace PixelGenesis.ECS;

public sealed partial class Entity : ISerializableObject, IEquatable<Entity>
{
    PGScene EntityManager;

    public int Index => EntityManager.Entities.IndexOf(this);
    public int Id { get; internal set; }
    public string Name { get; internal set; } = "";

    internal Entity? _parent;

    internal SortedList<int, Entity> _children = new SortedList<int, Entity>();

    public Entity? Parent => _parent;
    public ReadOnlySpan<Entity> Children => _children.ValuesAsSpan();
    public ReadOnlySpan<Component> Components => _components.ValuesAsSpan();
    
    public ImmutableArray<string> Tags { get; internal set; }

    public bool IsDisabled { get; set; }

    internal Entity(PGScene entityManager) 
    { 
        EntityManager = entityManager;        
    }    
    
    public void RemoveComponent<T>()
    {
        RemoveComponent(typeof(T));
    }

    public void RemoveComponent(Type type)
    {
        var component = GetComponent(type);
        EntityManager.RemoveComponentFromEntity(component);
        _components.Remove(type.GUID);
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
        if (_components.TryGetValue(type.GUID, out var component))
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

    public void AddChild(Entity child)
    {
        EntityManager.SetEntityParent(this, child);
    }

    public IEnumerable<KeyValuePair<string, object>> GetSerializableValues()
    {
        yield return new(nameof(Name), Name);
        yield return new(nameof(Tags), Tags);
        yield return new(nameof(IsDisabled), IsDisabled);         
    }

    public void SetSerializableValues(IEnumerable<KeyValuePair<string, object?>> values)
    {
        foreach(var (key, value) in values)
        {
            if(value is null)
            {
                continue;
            }

            switch (key) 
            { 
                case nameof(Name):
                    Name = (string)value;
                    break;
                case nameof(Tags):
                    Tags = ((IEnumerable)value).Cast<string>().ToImmutableArray();
                    break;
                case nameof(IsDisabled):
                    IsDisabled = (bool)value;
                    break;                
            }
        }
    }

    public Type GetPropType(string key)
    {
        switch(key)
        {
            case nameof(Name):
                return typeof(string);
            case nameof(Tags):
                return typeof(ImmutableArray<string>);
            case nameof(IsDisabled):
                return typeof(bool);            
            default:
                throw new InvalidDataException($"Entity does not have a property {key}");
        }
    }

    public bool Equals(Entity? other)
    {
        return Id == other?.Id;
    }
}

public sealed partial class Entity
{
    SortedList<Guid, Component> _components = new SortedList<Guid, Component>();

    internal void Clear()
    {
        _components.Clear();
    }

    internal void AddComponent(Component component)
    {
        _components.Add(component.GetType().GUID, component);
        EntityManager.AddComponentToEntity(component);
    }

    internal bool TryGetComponent(Type type, [MaybeNullWhen(false)] out Component component)
    {
        return _components.TryGetValue(type.GUID, out component);
    }
}

using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Components;
using PixelGenesis.ECS.Serialization;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace PixelGenesis.ECS.Scene;

public sealed class PGScene : ISerializableObject, IAsset
{
    public Guid Id { get; }
    public string Name { get; }

    ObjectPool<Entity> EntityPool;

    List<Entity> _entities = new List<Entity>();

    Dictionary<Type, List<Component>> Components = new Dictionary<Type, List<Component>>();

    Subject<ComponentAddedEvent> _componentAdded = new Subject<ComponentAddedEvent>();
    Subject<ComponentRemovedEvent> _componentRemoved = new Subject<ComponentRemovedEvent>();
    Subject<EntityAddedEvent> _entityAdded = new Subject<EntityAddedEvent>();
    Subject<EntityRemovedEvent> _entityRemoved = new Subject<EntityRemovedEvent>();

    public IObservable<ComponentAddedEvent> ComponentAdded => _componentAdded.AsObservable();
    public IObservable<ComponentRemovedEvent> ComponentRemoved => _componentRemoved.AsObservable();
    public IObservable<EntityAddedEvent> EntityAdded => _entityAdded.AsObservable();
    public IObservable<EntityRemovedEvent> EntityRemoved => _entityRemoved.AsObservable();

    internal ComponentsFactory ComponentFactory { get; }
    public IServiceProvider Provider;

    int currentId = 0;

    public PGScene(
        Guid id, 
        ComponentsFactory componentsFactory,
        IServiceProvider provider,
        string? name = default)
    {
        Id = id;
        Name = name ?? $"{id}.pgscene";
        EntityPool = new DefaultObjectPoolProvider().Create(new EntityPoolPolicy(this));
        ComponentFactory = componentsFactory;
        Provider = provider;
    }

    internal int GetNextId()
    {
        return ++currentId;
    }

    public ReadOnlySpan<Entity> Entities => CollectionsMarshal.AsSpan(_entities);

    public ReadOnlySpan<Component> GetComponents<T>() where T : Component
    {
        return GetComponents(typeof(T));
    }

    public ReadOnlySpan<Component> GetComponents(Type type)
    {
        ref var components = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, type, out var existed);
        if (!existed)
        {
            components = new List<Component>();
        }

        return CollectionsMarshal.AsSpan(components);
    }

    public Entity Clone(Entity entity, Entity? parent = default)
    {
        parent = parent ?? entity.Parent;

        var newEntity = Create(StringPool.Shared.GetOrAdd(entity.Name), entity.Tags, parent);

        if (parent is null)
        {
            SetEntityParent(newEntity, entity.Parent);
        }
        else
        {
            SetEntityParent(newEntity, parent);
        }

        foreach (var child in entity.Children)
        {
            Clone(child, newEntity);
        }

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    public Entity Clone(Entity entity, ReadOnlySpan<char> name)
    {
        var newEntity = Create(StringPool.Shared.GetOrAdd(name), entity.Tags, entity.Parent);

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    public Entity Clone(Entity entity, string name)
    {
        var newEntity = Create(StringPool.Shared.GetOrAdd(name), entity.Tags, entity.Parent);

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    void CloneEntityComponents(Entity entity, Entity newEntity)
    {
        var components = entity.Components;

        for (var i = 0; i < components.Length; ++i)
        {
            var component = components[i];

            var newComponent = ComponentFactory.CreateComponentIfNotExists(newEntity, component.GetType());
            component.CopyToAnother(newComponent);
        }
    }

    public Entity Create(string name, Entity? parent = default)
    {
        return Create(name, [], parent);
    }

    public Entity Create(string name, ImmutableArray<string> tags, Entity? parent = default)
    {
        var newEntity = EntityPool.Get();
        _entities.Add(newEntity);
        newEntity.Name = name;
        newEntity.Id = GetNextId();
        newEntity.Tags = tags;

        if (parent is null)
        {
            SetEntityParent(newEntity, parent);
        }

        _entityAdded.OnNext(new EntityAddedEvent(newEntity));

        return newEntity;
    }

    public void SetEntityParent(Entity entity, Entity? parent)
    {
        if (entity._parent?.Id == parent?.Id)
        {
            return;
        }

        if (entity._parent is not null)
        {
            entity._parent._children.Remove(entity._parent.Id);
        }

        entity._parent = parent;

        if (parent is not null)
        {
            parent._children.Add(entity.Id, entity);
        }
    }

    public void Destroy(Entity entity)
    {
        if (entity._parent is not null)
        {
            entity._children.Remove(entity._parent.Id);
        }

        _entities.Remove(entity);

        foreach(var component in entity.Components)
        {
            RemoveComponentFromEntity(component);
        }

        var id = entity.Id;

        EntityPool.Return(entity);

        _entityRemoved.OnNext(new EntityRemovedEvent(id));
    }

    internal void AddComponentToEntity(Component component)
    {
        var type = component.GetType();

        ref var components = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, type, out var existed);
        if (!existed)
        {
            components = new List<Component>();
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        components.Add(component);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        _componentAdded.OnNext(new ComponentAddedEvent(component));
    }

    internal void RemoveComponentFromEntity(Component component)
    {
        var type = component.GetType();

        var entities = Components[type];

        var index = entities.IndexOf(component);

        entities.RemoveAt(index);

        _componentRemoved.OnNext(new ComponentRemovedEvent(component));
    }

    public IEnumerable<KeyValuePair<string, object>> GetSerializableValues()
    {
        yield return new("Entities", _entities);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        yield return new("ParentMap", _entities.Where(x => x.Parent is not null).Select(x => (x.Index, x.Parent.Index)));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        yield return new("Components", Components.SelectMany(x => x.Value));
    }

    public void SetSerializableValues(IEnumerable<KeyValuePair<string, object?>> values)
    {
        // We only care for the parent map here
        foreach (var (key, value) in values)
        {
            if (key is not "ParentMap" || value is null)
            {
                continue;
            }

            foreach (var (index, parentIndex) in (IEnumerable<(int, int)>)value)
            {
                SetEntityParent(_entities[index], _entities[parentIndex]);
            }
        }
    }

    public Type GetPropType(string key)
    {
        return key switch
        {
            "Entities" => typeof(IEnumerable<Entity>),
            "ParentMap" => typeof(IEnumerable<(int, int)>),
            "Components" => typeof(IEnumerable<Component>),
            _ => throw new NotImplementedException()
        };
    }

    public void WriteToStream(IAssetManager assetManager, Stream stream)
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new SerializableObjectYamlConverter(assetManager, (_,_) => { throw new Exception(); }, ""))
            .WithTypeConverter(PGStructYamlConverter.Instance)
            .DisableAliases() // don't use anchors and aliases (references to identical objects)            
            .Build();

        var yamlStr = serializer.Serialize(this);
        using StreamWriter writer = new StreamWriter(stream);
        writer.Write(yamlStr);
    }

    public class PGSceneFactory(
        ComponentsFactory componentsFactory,
        IServiceProvider provider
        ) : IReadAssetFactory
    {
        public IAsset ReadAsset(Guid id, IAssetManager assetManager, Stream stream)
        {
            var scene = new PGScene(id, componentsFactory, provider);

            var factories = new Func<Type, int, ISerializableObject>((Type type, int ownerIndex) => {
                if (type.IsAssignableTo(typeof(Component)))
                {
                    return scene._entities[ownerIndex].AddComponentIfNotExist(type);
                }

                if (type == typeof(Entity))
                {
                    return scene.Create("");
                }

                if(type == typeof(PGScene))
                {
                    return scene;
                }

                throw new InvalidDataException($"Type factory for type {type.FullName} not found.");
            });

            var deserializer = new DeserializerBuilder()
                .WithTypeConverter(new SerializableObjectYamlConverter(assetManager, factories, ""))
                .WithTypeConverter(PGStructYamlConverter.Instance)
                .Build();

            using var reader = new StreamReader(stream);
            return deserializer.Deserialize<PGScene>(reader);
        }
    }

}

internal sealed class EntityPoolPolicy(PGScene entityManager) : IPooledObjectPolicy<Entity>
{
    public Entity Create()
    {
        var entity = new Entity(entityManager);
        return entity;
    }

    public bool Return(Entity obj)
    {
        obj.Clear();
        obj.Id = 0;
        return true;
    }
}

public struct ComponentAddedEvent(Component component)
{
    public Component Component => component;
}

public struct ComponentRemovedEvent(Component component)
{
    public Component Component => component;
}

public struct EntityAddedEvent(Entity entity)
{
    public Entity Entity => entity;
}

public struct EntityRemovedEvent(int id)
{
    public int Id => id;
}
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.ObjectPool;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PixelGenesis.ECS;

public sealed class PGScene : ISerializableObject, IAsset
{
    public Guid Id { get; }
    public string Name { get; }

    ObjectPool<Entity> EntityPool;

    List<Entity> _entities = new List<Entity>();

    Dictionary<Type, List<Component>> Components = new Dictionary<Type, List<Component>>();

    internal ComponentFactory ComponentFactory { get; }

    int currentId = 0;
        
    public PGScene(Guid id, string? name = default)
    {
        Id = id;
        Name = name ?? $"{id}.pgscene";
        EntityPool = new DefaultObjectPoolProvider().Create(new EntityPoolPolicy(this));
        ComponentFactory = new ComponentFactory();
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
        var newEntity = EntityPool.Get();
        _entities.Add(newEntity);
        newEntity.Id = GetNextId();
        newEntity.Name = StringPool.Shared.GetOrAdd($"{entity.Name}-{newEntity.Id}");
        newEntity.Tags = entity.Tags;

        if(parent is null)
        {
            SetEntityParent(newEntity, entity.Parent);
        }
        else
        {
            SetEntityParent(newEntity, parent);
        }
        

        foreach(var child in entity.Children)
        {
            Clone(child, newEntity);
        }

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    public Entity Clone(Entity entity, ReadOnlySpan<char> name)
    {
        var newEntity = EntityPool.Get();
        _entities.Add(newEntity);
        newEntity.Name = StringPool.Shared.GetOrAdd(name);
        newEntity.Id = GetNextId();
        newEntity.Tags = entity.Tags;

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    public Entity Clone(Entity entity, string name)
    {
        var newEntity = EntityPool.Get();
        _entities.Add(newEntity);
        newEntity.Name = StringPool.Shared.GetOrAdd(name);
        newEntity.Id = GetNextId();
        newEntity.Tags = entity.Tags;

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

    public Entity Create(string name)
    {
        return Create(name, []);
    }

    public Entity Create(string name, ImmutableArray<string> tags)
    {
        var newEntity = EntityPool.Get();
        _entities.Add(newEntity);
        newEntity.Name = name;
        newEntity.Id = GetNextId();
        newEntity.Tags = tags;
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
        EntityPool.Return(entity);
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
    }

    internal void RemoveComponentFromEntity(Component component)
    {
        var type = component.GetType();

        var entities = Components[type];

        var index = entities.IndexOf(component);
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
            if(key is not "ParentMap" || value is null)
            {
                continue;
            }

            foreach(var (index, parentIndex) in (IEnumerable<(int, int)>)value)
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

    public void WriteToStream(AssetManager assetManager, Stream stream)
    {
        var factories = ComponentFactory.ComponentsTypeNames.ToDictionary(x => x.Key, x =>
        {
            var type = x.Value;
            return new Func<int, ISerializableObject>((int ownerIndex) => _entities[ownerIndex].AddComponentIfNotExist(type));
        });

        // entity
        factories.Add(typeof(Entity).FullName ?? string.Empty, (_) =>
        {
            return Create("");
        });

        var serializer = new SerializerBuilder()
            .WithTypeConverter(new SerializableObjectYamlConverter(assetManager, factories, ""))
            .WithTypeConverter(PGStructYamlConverter.Instance)
            .DisableAliases() // don't use anchors and aliases (references to identical objects)            
            .Build();

        var yamlStr = serializer.Serialize(this);
        using StreamWriter writer = new StreamWriter(stream);
        writer.Write(yamlStr);
    }

    public class PGSceneFactory : IReadAssetFactory
    {
        public IAsset ReadAsset(Guid id, AssetManager assetManager, Stream stream)
        {
            var scene = new PGScene(id);

            var factories = ComponentFactory.ComponentsTypeNames.ToDictionary(x => x.Key, x =>
            {
                var type = x.Value;
                return new Func<int, ISerializableObject>((int ownerIndex) => scene._entities[ownerIndex].AddComponentIfNotExist(type));
            });

            // entity
            factories.Add(typeof(Entity).FullName ?? string.Empty, (_) =>
            {
                return scene.Create("");
            });

            // scene
            factories.Add(typeof(PGScene).FullName ?? string.Empty, (_) => scene);

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

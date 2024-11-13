using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace PixelGenesis.ECS;

public sealed class EntityManager
{
    ObjectPool<Entity> EntityPool;
    
    List<Entity> Entities = new List<Entity>();

    Dictionary<Type, List<Component>> Components = new Dictionary<Type, List<Component>>();

    internal ComponentFactory ComponentFactory { get; }

    int currentId = 0;

    public EntityManager()
    {
        EntityPool = new DefaultObjectPoolProvider().Create(new EntityPoolPolicy(this));
        ComponentFactory = new ComponentFactory(this);
    }

    internal int GetNextId()
    {
        return ++currentId;
    }

    ReadOnlySpan<Entity> GetEntities()
    {
        return CollectionsMarshal.AsSpan(Entities);
    }

    public ReadOnlySpan<Component> GetComponents<T>() where T : Component
    {
        return GetComponents(typeof(T));
    }    

    public ReadOnlySpan<Component> GetComponents(Type type)
    {
        ref var components = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, type, out var existed);
        if(!existed)
        {
            components = new List<Component>();
        }

        return CollectionsMarshal.AsSpan(components);
    }

    public Entity Clone(Entity entity)
    {
        var newEntity = EntityPool.Get();
        Entities.Add(newEntity);
        newEntity.Id = GetNextId();
        newEntity.Name = StringPool.Shared.GetOrAdd($"{entity.Name}-{newEntity.Id}");
        newEntity.Tags = entity.Tags;

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    public Entity Clone(Entity entity, ReadOnlySpan<char> name)
    {
        var newEntity = EntityPool.Get();
        Entities.Add(newEntity);
        newEntity.Name = StringPool.Shared.GetOrAdd(name);
        newEntity.Id = GetNextId();
        newEntity.Tags = entity.Tags;

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    public Entity Clone(Entity entity, string name)
    {
        var newEntity = EntityPool.Get();
        Entities.Add(newEntity);
        newEntity.Name = StringPool.Shared.GetOrAdd(name);
        newEntity.Id = GetNextId();
        newEntity.Tags = entity.Tags;

        CloneEntityComponents(entity, newEntity);

        return newEntity;
    }

    void CloneEntityComponents(Entity entity, Entity newEntity)
    {
        var components = entity.GetComponents();

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
        Entities.Add(newEntity);
        newEntity.Name = name;
        newEntity.Id = GetNextId();
        newEntity.Tags = tags;
        return newEntity;
    }

    public void Destroy(Entity entity) 
    { 
        Entities.Remove(entity);
        EntityPool.Return(entity);
    }

    internal void AddComponentToEntity(Component component)
    {
        var type = component.GetType();

        ref var components = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, type, out var existed);
        if(!existed)
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
}

internal sealed class EntityPoolPolicy(EntityManager entityManager) : IPooledObjectPolicy<Entity>
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

namespace PixelGenesis.ECS.Components;

public abstract class Component : ISerializableObject
{
    public bool IsEnabled { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal Entity _entity;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Entity Entity => _entity;
    public abstract void CopyToAnother(Component component);
    public abstract IEnumerable<KeyValuePair<string, object>> GetSerializableValues();
    public abstract void SetSerializableValues(IEnumerable<KeyValuePair<string, object?>> values);
    public abstract int GetOwnerIndex();
    public abstract Type GetPropType(string key);
}

public interface ISerializableObject
{
    IEnumerable<KeyValuePair<string, object>> GetSerializableValues();
    public void SetSerializableValues(IEnumerable<KeyValuePair<string, object?>> values);
    public Type GetPropType(string key);
    public int GetOwnerIndex()
    {
        return -1;
    }
}

public interface IAwake
{
    void Awake();
}
public interface IEnable
{
    void Enable();
}
public interface IReset
{
    void Reset();
}
public interface IStart
{
    void Start();
}
public interface IFixedUpdate
{
    void FixedUpdate();
}

public interface IUpdate
{
    void Update();
}

public interface ILateUpdate
{
    void LateUpdate();
}
public interface IGizmos
{
    void DrawGizmos();
}
public interface IDisable
{
    void Disable();
}
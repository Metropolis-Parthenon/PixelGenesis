using System.Runtime.CompilerServices;

namespace PixelGenesis.GameLogic;

public interface IComponent
{
    public IStateObject StateObj { get; internal set; }
    public bool IsEnabled { get; set; }
    public Entity Entity { get; internal set; }
}

public interface IComponent<S> : IComponent where S : class, IStateObject
{
    public S State => Unsafe.As<S>(StateObj);
}

public interface IStateObject
{
    IStateObject DeepClone();
}
using System.Reactive.Linq;

namespace PixelGenesis.Editor.Core;

public interface ICommandDispatcher
{
    IObservable<object> Commands { get; }

    IObservable<T> GetCommands<T>()
    {
        return Commands.Where(x => x is T).Cast<T>();
    }

    void Dispatch(object command);
}

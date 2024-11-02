using PixelGenesis.Editor.Core;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PixelGenesis.Editor.Services;

internal class CommandDispatcher : ICommandDispatcher
{
    Subject<object> subject = new Subject<object>();

    public IObservable<object> Commands => subject.AsObservable();

    public void Dispatch(object command)
    {
        subject.OnNext(command);
    }
}

using PixelGenesis.Editor.Core;

namespace PixelGenesis.Editor.Services;

internal class EditionCommandDispatcher : IEditionCommandDispatcher
{

    Stack<IEditionCommand> History = new Stack<IEditionCommand>();
    Stack<IEditionCommand> UndoneHistory = new Stack<IEditionCommand>();
    

    public void Dispatch(IEditionCommand command)
    {
        command.Do();
        History.Push(command);
    }

    public void Undo()
    {
        if(History.Count is 0)
        {
            return;
        }

        var command = History.Pop();
        command.Undo();
        UndoneHistory.Push(command);
    }

    public void Redo()
    {
        if(UndoneHistory.Count is 0)
        {
            return;
        }
        var command = History.Pop();
        command.Do();
        History.Push(command);
    }
}

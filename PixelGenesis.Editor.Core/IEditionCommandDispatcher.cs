namespace PixelGenesis.Editor.Core;

public interface IEditionCommandDispatcher
{
    void Dispatch(IEditionCommand command);
    void Undo();
    void Redo();
}

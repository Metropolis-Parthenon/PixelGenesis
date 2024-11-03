namespace PixelGenesis.Editor.Core;

public interface IEditionCommand
{
    void Do();
    void Undo();    
}

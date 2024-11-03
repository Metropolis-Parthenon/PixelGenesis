using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;

namespace PixelGenesis.Editor.BuiltIn.EditorWindows;

internal class FileEditorWindow(ImageLoader imageLoader) : IEditorWindow
{
    public string Name => "EditorWindow";

    public void OnGui()
    {
        
    }
}

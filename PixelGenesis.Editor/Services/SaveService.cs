using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.Services;

public class SaveService
{
    public bool IsCurrentDirty { get; private set; } = true;
    public bool IsAnyDirty { get; private set; } = true;

    public void SaveCurrent()
    {
        IsCurrentDirty = false;
    }

    public void SaveAll()
    {
        IsAnyDirty = false;
    }
}

using PixelGenesis.Editor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.Services;

internal sealed class SaveService(ICommandDispatcher commandDispatcher)
{
    public bool IsCurrentDirty { get; private set; } = true;
    public bool IsAnyDirty { get; private set; } = true;

    public void SaveCurrent()
    {
        commandDispatcher.Dispatch(new SaveCurrentCommand());
        IsCurrentDirty = false;
    }

    public void SaveAll()
    {
        commandDispatcher.Dispatch(new SaveAllCommand());
        IsAnyDirty = false;
    }
}

public class SaveCurrentCommand
{
}

public class SaveAllCommand
{
}
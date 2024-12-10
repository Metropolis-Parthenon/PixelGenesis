using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.Editor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.BuiltIn.AssetEditors;

public class MaterialEditor(
    IDeviceApi deviceApi,
    Material material,
    IPGWindow window
    ) : IAssetEditor
{
    public bool IsDirty => false;

    bool IsInitialized = false;

    public void BeforeGui()
    {
        throw new NotImplementedException();
    }

    public void OnGui()
    {
        throw new NotImplementedException();
    }

    public void OnSave()
    {
        throw new NotImplementedException();
    }

    public void OnClose()
    {
        throw new NotImplementedException();
    }

}

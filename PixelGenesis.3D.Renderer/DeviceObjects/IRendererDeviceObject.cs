using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceObjects;

internal interface IRendererDeviceObject : IDisposable
{
    public void Initialize();
    public void Update();
    public void AfterUpdate();
}

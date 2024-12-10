using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IPGWindow
{
    Size WindowSize => ViewportSize;
    Size ViewportSize { get; }
    IObservable<Size> ViewportSizeObservable { get; }    
}

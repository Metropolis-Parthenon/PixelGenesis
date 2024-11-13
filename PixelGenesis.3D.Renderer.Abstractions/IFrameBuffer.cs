using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IFrameBuffer : IDeviceObject
{
    public ITexture GetTexture();
    public void Rescale(int width, int height);    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IUniformBlockBuffer : IDeviceObject
{
    public void SetData<T>(T data, int index) where T : unmanaged;
    public void SetData(ReadOnlySpan<byte> data, int index);    
}

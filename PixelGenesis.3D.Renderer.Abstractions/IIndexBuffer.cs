using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IIndexBuffer<T> : IIndexBuffer where T : unmanaged, IBinaryInteger<T>
{
    void SetData(int offset, ReadOnlySpan<T> data);
}

public interface IIndexBuffer : IDeviceObject
{ 

}
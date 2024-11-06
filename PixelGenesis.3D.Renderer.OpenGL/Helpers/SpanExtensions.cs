using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL.Helpers;

public static class SpanHelpers
{
    public unsafe static Span<byte> SpanFromIntPtr(IntPtr ptr, int length)
    {
        return new Span<byte>((void*)ptr, length);
    }
}

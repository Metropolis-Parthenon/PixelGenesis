using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.OpenGL;

internal interface IOpenGLObject
{
    public void Create();

    public void Bind();
}

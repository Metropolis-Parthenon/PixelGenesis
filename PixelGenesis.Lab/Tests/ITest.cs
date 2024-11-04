using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Lab.Tests;

internal interface ITest : IDisposable
{
    void OnUpdate(float deltaTime);
    void OnRender();
    void OnGui();
}

using FbxTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Common.ModelFileFormats;

internal static class FbxMeshReader
{
    internal static void ReadFBX(Stream stream, StreamMesh mesh)
    {
        var fbx = FbxParser.Parse(stream);

        foreach(var node in fbx.Nodes)
        {

        }
    }
}

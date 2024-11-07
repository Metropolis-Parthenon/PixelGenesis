using ImGuiNET;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis.Editor.BuiltIn.AssetEditors.Shader;

internal class ShaderRenderer : IDisposable
{
    PGGLSLShaderSource.CompilationResult? _shader;
    Dictionary<int, object[]> _layoutValues = new Dictionary<int, object[]>();

    IDeviceApi _deviceApi;

    public ShaderRenderer(PGGLSLShaderSource source, IDeviceApi deviceApi)
    {
        _deviceApi = deviceApi;
        if(source.ShaderDTO is null)
        {
            _shader = null;
            return;
        }

        _shader = source.CompiledShader();
        _layoutValues = CreateLayoutValues(source.ShaderDTO);
    }

    public void OnSourceChanged(PGGLSLShaderSource changed)
    {
        _shader = changed.CompiledShader();
        UpdateLayout(_layoutValues, changed.ShaderDTO);

    }

    unsafe ref T GetValueRef<T>(int binding, int index) where T : struct
    {
        if(!_layoutValues.TryGetValue(binding, out var values))
        {
            throw new InvalidOperationException("Binding not found");
        }

        if(index >= values.Length)
        {
            throw new IndexOutOfRangeException("Index out of range");
        }

        return ref Unsafe.AsRef<T>(Unsafe.AsPointer(ref values[index]));
    }
    
    public void OnGui()
    {
        if(_shader is null)
        {
            ImGui.Text("Invalid File");
            return;
        }    

        if(_shader.Shader is null)
        {
            ImGui.Text(_shader.Error);
            return;
        }

        _shader.Shader.Layout.Blocks.ForEach(block =>
        {
            for (var i = 0; i < block.Parameters.Count; i++) {

                var parameter = block.Parameters[i];

                if(parameter is null) continue;

                switch (parameter.Type)
                {
                    case _3D.Common.Type.Float:
                        ImGui.SliderFloat(parameter.Name, ref GetValueRef<float>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Float2:
                        ImGui.SliderFloat2(parameter.Name, ref GetValueRef<Vector2>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Float3:
                        ImGui.SliderFloat3(parameter.Name, ref GetValueRef<Vector3>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Float4:
                        ImGui.SliderFloat4(parameter.Name, ref GetValueRef<Vector4>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Mat4:
                        ImGui.Text("Mat4 not supported");
                        break;
                    case _3D.Common.Type.Color4:
                        ImGui.ColorEdit4(parameter.Name, ref GetValueRef<Vector4>(block.Binding, i));
                        break;
                }
            }
        });
    }




    static void UpdateLayout(Dictionary<int, object[]> layoutValues, ShaderSourceDTO layout)
    {
        layout.Blocks.ForEach(block =>
        {
            if(!layoutValues.TryGetValue(block.Binding, out var values))
            {
                values = new object[block.Parameters.Count];
            }

            if(values.Length != block.Parameters.Count)
            {
                Array.Resize(ref values, block.Parameters.Count);
            }

            for (var i = 0; i < block.Parameters.Count; i++)
            {
                var parameter = block.Parameters[i];
                switch (parameter.Type)
                {
                    case _3D.Common.Type.Float:
                        values[i] = values[i] is not null && values[i] is float ? values[i] : 0f;
                        break;
                    case _3D.Common.Type.Float2:
                        values[i] = values[i] is not null && values[i] is Vector2 ? values[i] : new Vector2();
                        break;
                    case _3D.Common.Type.Float3:
                        values[i] = values[i] is not null && values[i] is Vector3 ? values[i] : new Vector3();
                        break;
                    case _3D.Common.Type.Float4:
                        values[i] = values[i] is not null && values[i] is Vector4 ? values[i] : new Vector4();
                        break;
                    case _3D.Common.Type.Mat4:
                        values[i] = values[i] is not null && values[i] is Matrix4x4 ? values[i] : new Matrix4x4();
                        break;
                    case _3D.Common.Type.Color4:
                        values[i] = values[i] is not null && values[i] is Vector4 ? values[i] : new Vector4();
                        break;
                }
            }
                        
            layoutValues[block.Binding] = values;
        });
    }

    static Dictionary<int, object[]> CreateLayoutValues(ShaderSourceDTO layout)
    {
        var result = new Dictionary<int, object[]>();

        layout.Blocks.ForEach(block =>
        {
            var values = new object[block.Parameters.Count];

            for (var i = 0; i < block.Parameters.Count; i++)
            {
                var parameter = block.Parameters[i];
                if(parameter is null) continue;
                switch (parameter.Type)
                {
                    case _3D.Common.Type.Float:
                        values[i] = 0f;
                        break;
                    case _3D.Common.Type.Float2:
                        values[i] = new Vector2();
                        break;
                    case _3D.Common.Type.Float3:
                        values[i] = new Vector3();
                        break;
                    case _3D.Common.Type.Float4:
                        values[i] = new Vector4();
                        break;
                    case _3D.Common.Type.Mat4:
                        values[i] = new Matrix4x4();
                        break;
                    case _3D.Common.Type.Color4:
                        values[i] = new Vector4();
                        break;
                }
            }
            result[block.Binding] = values;
        });

        return result;
    }

    public void Dispose()
    {
        
    }
}

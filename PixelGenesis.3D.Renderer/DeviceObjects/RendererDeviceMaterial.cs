using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelGenesis._3D.Renderer.DeviceObjects;

internal class RendererDeviceMaterial(IDeviceApi deviceApi, Material material, DeviceRenderObjectManager manager) : IRendererDeviceObject
{
    public Material Material => material;

    IUniformBlockBuffer[]? materialUniformBuffers;
    RendererDeviceTexture?[]? materialTextures;

    public IUniformBlockBuffer[] MaterialUniformBuffers => materialUniformBuffers ?? throw new ArgumentNullException(nameof(materialUniformBuffers));
    public RendererDeviceTexture?[] MaterialTextures => materialTextures ?? throw new ArgumentNullException(nameof(materialTextures));

    public bool IsDirty { get; private set; }
    public bool IsTextureDirty { get; private set; }

    public unsafe void Initialize()
    {
        var blocksSpan = CollectionsMarshal.AsSpan(Material.Shader.Layout.Blocks);

        materialUniformBuffers = new IUniformBlockBuffer[blocksSpan.Length];
        for (var i = 0; i < blocksSpan.Length; i++)
        {
            var block = blocksSpan[i];

            var sizes = new int[block.Parameters.Count];

            for (var j = 0; j < sizes.Length; j++)
            {
                var parameter = block.Parameters[j];
                sizes[j] = parameter.Type switch
                {
                    Common.Type.Float => sizeof(float),
                    Common.Type.Float2 => sizeof(Vector2),
                    Common.Type.Float3 => sizeof(Vector4),
                    Common.Type.Float4 => sizeof(Vector4),
                    Common.Type.Mat4 => sizeof(Matrix4x4),
                    Common.Type.Color4 => sizeof(Vector4),
                    _ => throw new NotImplementedException()
                };
            }

            materialUniformBuffers[i] = deviceApi.CreateUniformBlockBuffer(sizes, BufferHint.Dynamic);
        }

        SetUniformBufferValues();

        materialTextures = new RendererDeviceTexture[Material.Shader.Layout.Textures.Count];

        SetTextures();

        Material.IsDirty = false;
        IsDirty = false;
        IsTextureDirty = false;
    }

    public unsafe void Update()
    {
        if(Material.IsDirty)
        {
            SetUniformBufferValues();
        }

        if(Material.IsTexturesDirty)
        {
            SetTextures();
        }

        Material.IsDirty = false;
        Material.IsTexturesDirty = false;
    }

    void SetTextures()
    {
        var texturesSpan = CollectionsMarshal.AsSpan(Material.Shader.Layout.Textures);
        materialTextures = new RendererDeviceTexture[texturesSpan.Length];
        for (var i = 0; i < texturesSpan.Length; i++)
        {
            var textureName = texturesSpan[i].Name;
            var texture = Material.GetTexture(textureName);

            if (texture is null)
            {
                continue;
            }

            var deviceTexture = manager.GetOrAddDeviceTexture(texture);

            var oldTex = materialTextures[i];
            materialTextures[i] = manager.GetOrAddDeviceTexture(texture);

            if(oldTex is not null)
            {
                manager.Return(oldTex);
            }
        }
    }

    void SetUniformBufferValues()
    {
        if (materialUniformBuffers is null)
        {
            return;
        }

        Span<float> floatData = stackalloc float[1];
        Span<Vector2> float2Data = stackalloc Vector2[1];
        Span<Vector4> float4Data = stackalloc Vector4[1];
        Span<Matrix4x4> matData = stackalloc Matrix4x4[1];

        // set uniform buffer values
        for (var i = 0; i < materialUniformBuffers.Length; i++)
        {
            var block = Material.Shader.Layout.Blocks[i];
            var uniformBuffer = materialUniformBuffers[i];

            var parametersSpan = CollectionsMarshal.AsSpan(block.Parameters);
            for (var j = 0; j < parametersSpan.Length; j++)
            {
                var parameter = parametersSpan[j];
                uniformBuffer.SetData(parameter.Type switch
                {
                    Common.Type.Float => GetParameterBytes(floatData, Material.GetParameterFloat(block.Name, parameter.Name)),
                    Common.Type.Float2 => GetParameterBytes(float2Data, Material.GetParameterVector2(block.Name, parameter.Name)),
                    Common.Type.Float3 => GetParameterBytes(float4Data, ToVector4(Material.GetParameterVector3(block.Name, parameter.Name))),
                    Common.Type.Float4 => GetParameterBytes(float4Data, Material.GetParameterVector4(block.Name, parameter.Name)),
                    Common.Type.Mat4 => GetParameterBytes(matData, Material.GetParameterMatrix4x4(block.Name, parameter.Name)),
                    Common.Type.Color4 => GetParameterBytes(float4Data, Material.GetParameterVector4(block.Name, parameter.Name)),
                    _ => throw new NotImplementedException()
                }, j);
            }
        }
    }

    static Vector4 ToVector4(Vector3 vector3)
    {
        return new Vector4(vector3.X, vector3.Y, vector3.Z, 0);
    }

    static ReadOnlySpan<byte> GetParameterBytes(Span<float> buffer, float value)
    {
        buffer[0] = value;
        return buffer.AsBytes();
    }
    static ReadOnlySpan<byte> GetParameterBytes(Span<Vector2> buffer, Vector2 value)
    {
        buffer[0] = value;
        return buffer.AsBytes();
    }

    static ReadOnlySpan<byte> GetParameterBytes(Span<Vector4> buffer, Vector4 value)
    {
        buffer[0] = value;
        return buffer.AsBytes();
    }
    static ReadOnlySpan<byte> GetParameterBytes(Span<Matrix4x4> buffer, Matrix4x4 value)
    {
        buffer[0] = value;
        return buffer.AsBytes();
    }

    public CompiledShader CompileShader(ShaderCompileContext context, out int[] materialBlocksBinding, out int[] materialTextureBinding, out int lightSourceBinding)
    {
        var defines = new List<KeyValuePair<string, string>>();

        StringBuilder vertexPreCode = new StringBuilder();
        StringBuilder fragmentPreCode = new StringBuilder();
        StringBuilder geometryPreCode = new StringBuilder();
        StringBuilder tessallationPreCode = new StringBuilder();

        // vertex layout code
        var mesh = context.DeviceMesh;
        // mesh properties
        if (mesh.PositionLayout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_POSITIONS", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.PositionLayout}) in vec3 position;");
        }
        if (mesh.NormalLayout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_NORMALS", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.NormalLayout}) in vec3 normal;");
        }
        if (mesh.TangentLayout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_TANGENTS", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.TangentLayout}) in vec4 tangent;");
        }
        if (mesh.ColorLayout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_COLORS", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.ColorLayout}) in vec4 color;");
        }
        if (mesh.UV1Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV1", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV1Layout}) in vec2 uv1;");
        }
        if (mesh.UV2Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV2", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV2Layout}) in vec2 uv2;");
        }
        if (mesh.UV3Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV3", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV3Layout}) in vec2 uv3;");
        }
        if (mesh.UV4Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV4", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV4Layout}) in vec2 uv4;");
        }
        if (mesh.UV5Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV5", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV5Layout}) in vec2 uv5;");
        }
        if (mesh.UV6Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV6", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV6Layout}) in vec2 uv6;");
        }
        if (mesh.UV7Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV7", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV7Layout}) in vec2 uv7;");
        }
        if (mesh.UV8Layout >= 0)
        {
            defines.Add(new KeyValuePair<string, string>("HAS_UV8", "1"));
            vertexPreCode.AppendLine($"layout(location={mesh.UV8Layout}) in vec2 uv8;");
        }
        //model for instancing
        vertexPreCode.AppendLine($"layout(location = {context.ModelLayout}) in mat4 model;");

        int uniformBlockBinding = 0;
        // add the details uniform
        fragmentPreCode.AppendLine($$"""
            layout(binding = {{uniformBlockBinding}}) uniform Details {
                mat4 viewProjection;
                vec3 cameraPosition;
            } details;
            """);
        uniformBlockBinding++;

        StringBuilder lightsUniformBlockBuilder = new StringBuilder();
        //fragment lights
        if (context.DirectionalLights > 0)
        {
            defines.Add(new KeyValuePair<string, string>("DIR_LIGHTS_LENGHT", context.DirectionalLights.ToString()));

            fragmentPreCode.AppendLine($$"""
                struct DirLight {
                    vec3 direction;
                    vec3 color;
                    float intensity;
                  }
            """);
            lightsUniformBlockBuilder.AppendLine($"DirLight dirLights[{context.DirectionalLights}];");
        }

        if (context.PointLights > 0)
        {
            defines.Add(new KeyValuePair<string, string>("POINT_LIGHTS_LENGHT", context.PointLights.ToString()));

            fragmentPreCode.AppendLine($$"""
                struct PointLight {    
                    vec3 position;
                    vec3 color;
                    float intensity;
                  };
            """);
            lightsUniformBlockBuilder.AppendLine($"PointLight pointLights[{context.PointLights}];");
        }

        if (context.SpotLights > 0)
        {
            defines.Add(new KeyValuePair<string, string>("SPOT_LIGHTS_LENGHT", context.SpotLights.ToString()));

            fragmentPreCode.AppendLine($$"""
                struct SpotLight {    
                    vec3 position;
                    vec3 direction;
                    vec3 color;
                    float cutOff;
                    float intensity; 
                  };
            """);
            lightsUniformBlockBuilder.AppendLine($"SpotLight spotLights[{context.SpotLights}];");
        }

        if (lightsUniformBlockBuilder.Length > 0)
        {
            fragmentPreCode.AppendLine($"layout(binding={uniformBlockBinding}) uniform LightSources {{");
            fragmentPreCode.AppendLine(lightsUniformBlockBuilder.ToString());
            fragmentPreCode.AppendLine("} lightSources;");
            lightSourceBinding = uniformBlockBinding;
            uniformBlockBinding++;
        }
        else
        {
            lightSourceBinding = -1;
        }

        // add the material uniforms to the fragment shader
        var layout = material.Shader.Layout;
        var blocksSpan = CollectionsMarshal.AsSpan(layout.Blocks);

        materialBlocksBinding = new int[blocksSpan.Length];

        for (var i = 0; i < blocksSpan.Length; i++)
        {
            var block = blocksSpan[i];
            fragmentPreCode.AppendLine($"layout(binding={uniformBlockBinding}) uniform BlockUniform{block.Name} {{");

            var parametersSpan = CollectionsMarshal.AsSpan(block.Parameters);
            for (var j = 0; j < parametersSpan.Length; j++)
            {
                var parameter = parametersSpan[j];
                fragmentPreCode.AppendLine($"{TypeToGLSLType(parameter.Type)} {parameter.Name};");
            }

            fragmentPreCode.AppendLine($"}} {block.Name};");

            materialBlocksBinding[i] = uniformBlockBinding;

            uniformBlockBinding++;
        }
        // add the material textures to the fragment shader
        var textureBinding = 0;
        var texturesSpan = CollectionsMarshal.AsSpan(material.Shader.Layout.Textures);
        materialTextureBinding = new int[texturesSpan.Length];
        int numOfTexturesSet = 0;
        for (var i = 0; i < texturesSpan.Length; i++)
        {
            var texture = texturesSpan[i];
            var textureValue = material.GetTexture(texture.Name);
            if (textureValue is null)
            {
                continue;
            }

            numOfTexturesSet++;

            defines.Add(new KeyValuePair<string, string>($"TEXTURE_{texture.Name}", "1"));

            fragmentPreCode.AppendLine($"layout (binding=0) uniform sampler2D {texture.Name};");
            materialTextureBinding[i] = textureBinding;
            textureBinding++;
        }

        materialTextureBinding = materialTextureBinding.AsSpan().Slice(0, numOfTexturesSet).ToArray();

        return material.GetCompiledShader(
            CollectionsMarshal.AsSpan(defines),
            vertexPreCode.ToString(),
            fragmentPreCode.ToString(),
            geometryPreCode.ToString(),
            tessallationPreCode.ToString());
    }

    static string TypeToGLSLType(Common.Type type)
    {
        return type switch
        {
            Common.Type.Float => "float",
            Common.Type.Float2 => "vec2",
            Common.Type.Float3 => "vec4",
            Common.Type.Float4 => "vec4",
            Common.Type.Mat4 => "mat4",
            Common.Type.Color4 => "vec4",
            _ => throw new NotImplementedException()
        };
    }

    public void Dispose()
    {
        if (materialUniformBuffers is not null)
        {
            for (var i = 0; i < materialUniformBuffers.Length; i++)
            {
                materialUniformBuffers[i].Dispose();
            }
        }

        if(materialTextures is not null)
        {
            foreach (var texture in materialTextures)
            {
                if(texture is null) continue;
                manager.Return(texture);
            }
        }
    }

    public void AfterUpdate()
    {
        IsDirty = false;
        IsTextureDirty = false;
    }
}

internal class ShaderCompileContext
{
    public int DirectionalLights { get; set; }
    public int PointLights { get; set; }
    public int SpotLights { get; set; }
    public int ModelLayout { get; set; }
    public RendererDeviceMesh DeviceMesh { get; set; }
}
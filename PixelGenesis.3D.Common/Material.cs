using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using PixelGenesis.ECS.AssetManagement;

namespace PixelGenesis._3D.Common;

public class Material : IAsset
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public bool IsDirty { get; set; }
    public bool IsTexturesDirty {  get; set; }
    public PGGLSLShaderSource Shader { get; set; }

    private Dictionary<string, Dictionary<string, object>> Parameters = new();
    private Dictionary<string, Texture?> Textures = new();

    public Material(Guid id, PGGLSLShaderSource shader, string? name = default)
    {
        Id = id;
        Name = name ?? $"{id}.pgmat";
        Shader = shader;
        InitializeParametersFromShaderLayout();
    }

    public CompiledShader GetCompiledShader(
        ReadOnlySpan<KeyValuePair<string, string>> defines, 
        string vertexShaderPreCode,
        string fragmentShaderPreCode,
        string geometryShaderPreCode,
        string tessallationShaderPreCode)
    {
        var result = Shader.CompiledShader(
            defines, 
            vertexShaderPreCode,
            fragmentShaderPreCode,
            geometryShaderPreCode,
            tessallationShaderPreCode);

        if(result.Shader is null)
        {
            throw new Exception(result.Error);
        }

        return result.Shader;
    }

    public Texture? GetTexture(string name)
    {
        return Textures[name];
    }

    public float GetParameterFloat(string blockName, string parameterName)
    {
        return Unsafe.Unbox<float>(GetParameter(blockName, parameterName));
    }

    public Vector2 GetParameterVector2(string blockName, string parameterName)
    {
        return Unsafe.Unbox<Vector2>(GetParameter(blockName, parameterName));
    }

    public Vector3 GetParameterVector3(string blockName, string parameterName)
    {
        return Unsafe.Unbox<Vector3>(GetParameter(blockName, parameterName));
    }

    public Vector4 GetParameterVector4(string blockName, string parameterName)
    {
        return Unsafe.Unbox<Vector4>(GetParameter(blockName, parameterName));
    }

    public Matrix4x4 GetParameterMatrix4x4(string blockName, string parameterName)
    {
        return Unsafe.Unbox<Matrix4x4>(GetParameter(blockName, parameterName));
    }

    public object GetParameter(string blockName, string parameterName)
    {
        ref var block = ref CollectionsMarshal.GetValueRefOrNullRef(Parameters, blockName);

        if(Unsafe.IsNullRef(ref block))
        {
            throw new ArgumentException($"Invalid block binding: {blockName}");
        }
                
        return block[parameterName];
    }

    // Overloads for setting parameters of specific types
    public void SetParameter(string blockBinding, string parameterIndex, float value) => SetParameter(blockBinding, parameterIndex, value, Type.Float);
    public void SetParameter(string blockBinding, string parameterIndex, Vector2 value) => SetParameter(blockBinding, parameterIndex, value, Type.Float2);
    public void SetParameter(string blockBinding, string parameterIndex, Vector3 value) => SetParameter(blockBinding, parameterIndex, value, Type.Float3);
    public void SetParameter(string blockBinding, string parameterIndex, Vector4 value) => SetParameter(blockBinding, parameterIndex, value, Type.Float4);
    public void SetParameter(string blockBinding, string parameterIndex, Matrix4x4 value) => SetParameter(blockBinding, parameterIndex, value, Type.Mat4);

    // Sets a texture based on binding
    public void SetTexture(string name, Texture texture)
    {
        Textures[name] = texture;
        IsTexturesDirty = true;
    }

    // Internal method with type checking for setting a parameter
    public void SetParameter(string blockName, string parameterName, object value, Type expectedType)
    {        
        // Retrieve reference to block dictionary
        ref var block = ref CollectionsMarshal.GetValueRefOrNullRef(Parameters, blockName);
        if (Unsafe.IsNullRef(ref block))
        {
            throw new ArgumentException($"Invalid block binding: {blockName}");
        }
        
#warning TODO: verify type from material layout in a way that is no fucking slow
        // Validate type using the shader layout
        //var actualType = Shader.Layout.Blocks[blockBinding].Parameters[parameterIndex].Type;
        //if (actualType != expectedType)
        //{
        //    throw new ArgumentException($"Type mismatch: Expected {actualType} for parameter at block {blockBinding}, index {parameterIndex}, but received {expectedType}.");
        //}

        block[parameterName] = value; // Assign the value if type matches
        IsDirty = true;
    }

    // Initializes parameters and textures based on the shader layout
    private void InitializeParametersFromShaderLayout()
    {
        foreach (var block in Shader.Layout.Blocks)
        {
            ref var parameters = ref CollectionsMarshal.GetValueRefOrAddDefault(Parameters, block.Name, out var existed);

            if (!existed)
            {
                parameters = new Dictionary<string, object>();
            }

            if(parameters is null)
            {
                throw new Exception("This should never be thrown");
            }

            for (int i = 0; i < block.Parameters.Count; ++i)
            {
                var parameter = block.Parameters[i];
                parameters[parameter.Name] = GetDefaultValue(parameter.Type);
            }
        }

        foreach (var texture in Shader.Layout.Textures)
        {
            Textures[texture.Name] = null; // Placeholder for textures to be assigned later
        }
    }

    // Helper to return a default value based on parameter type
    private static object GetDefaultValue(Type type)
    {
        return type switch
        {
            Type.Float => 0f,
            Type.Float2 => Vector2.Zero,
            Type.Float3 => Vector3.Zero,
            Type.Float4 => Vector4.Zero,
            Type.Mat4 => Matrix4x4.Identity,
            Type.Color4 => Vector4.One,
            _ => throw new ArgumentException("Unsupported parameter type.")
        };
    }


    static ISerializer _serializer
        = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public void WriteToStream(IAssetManager assetManager, Stream stream)
    {
        var dto = new MaterialDTO();

        dto.ShaderReference = Shader.Id;

        dto.TextureReferences = new();
        foreach(var (textureName, texture) in Textures)
        {
            if(texture is null)
            {
                continue;
            }

            dto.TextureReferences[textureName] = texture.Id;
        }

        dto.Blocks = new();
        foreach(var (blockName, parameters) in Parameters)
        {
            ref var paramsDTO = ref CollectionsMarshal.GetValueRefOrAddDefault(dto.Blocks, blockName, out var exists);
            if (!exists)
            {
                paramsDTO = new();
            }

            foreach(var (paramName, paramValue) in parameters)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                paramsDTO[paramName] = paramValue;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        using var writer = new StreamWriter(stream);
        _serializer.Serialize(writer, dto);
    }

    // Factory for reading and creating materials from assets
    public class Factory : IReadAssetFactory
    {
        static IDeserializer _deserializer
            = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public IAsset ReadAsset(Guid id, IAssetManager assetManager, Stream stream)
        {
            // read the dto
            var dto = _deserializer.Deserialize<MaterialDTO>(new StreamReader(stream));

            var shader = assetManager.LoadAsset<PGGLSLShaderSource>(dto.ShaderReference);
            var material = new Material(id, shader);

            foreach(var (textureName, textureId) in dto.TextureReferences)
            {
                material.SetTexture(textureName, assetManager.LoadAsset<Texture>(textureId));
            }

            foreach(var (block, parameters) in dto.Blocks)
            {
                foreach(var (paramName, paramValue) in parameters)
                {
                    var convertedParam = paramValue switch
                    {
                        string val => float.Parse(val),
                        Dictionary<object, object> val => DictToVector(val),                        
                        _ => throw new NotImplementedException()
                    };

                    material.SetParameter(block, paramName, convertedParam, convertedParam switch
                    {
                        float => Type.Float,
                        Vector2 => Type.Float2,
                        Vector3 => Type.Float3,
                        Vector4 => Type.Float4,
                        Matrix4x4 => Type.Mat4,
                        _ => throw new NotImplementedException()
                    });
                }
            }

            return material;
        }

        static object DictToVector(Dictionary<object, object> dict)
        {
            if (dict.Count == 2)
            {
                return new Vector2(GetFloatVal(dict, "x"), GetFloatVal(dict, "y"));
            }

            if(dict.Count == 3)
            {
                return new Vector3(GetFloatVal(dict, "x"), GetFloatVal(dict, "y"), GetFloatVal(dict, "z"));
            }

            if (dict.Count == 4)
            {
                return new Vector4(GetFloatVal(dict, "x"), GetFloatVal(dict, "y"), GetFloatVal(dict, "z"), GetFloatVal(dict, "w"));
            }

            if(dict.Count == 16)
            {
                return new Matrix4x4(
                    GetFloatVal(dict, "m11"),
                    GetFloatVal(dict, "m12"),
                    GetFloatVal(dict, "m13"),
                    GetFloatVal(dict, "m14"),
                    GetFloatVal(dict, "m21"),
                    GetFloatVal(dict, "m22"),
                    GetFloatVal(dict, "m23"),
                    GetFloatVal(dict, "m24"),
                    GetFloatVal(dict, "m31"),
                    GetFloatVal(dict, "m32"),
                    GetFloatVal(dict, "m33"),
                    GetFloatVal(dict, "m34"),
                    GetFloatVal(dict, "m41"),
                    GetFloatVal(dict, "m42"),
                    GetFloatVal(dict, "m43"),
                    GetFloatVal(dict, "m44")
                    );
            }

            throw new NotImplementedException();
        }

        static float GetFloatVal(Dictionary<object, object> dict, string name)
        {
            return float.Parse((string)dict[name]);
        }
    }
}


file class MaterialDTO
{
    public Guid ShaderReference { get; set; }
    public Dictionary<string, Guid> TextureReferences { get; set; }
    public Dictionary<string, Dictionary<string, object>> Blocks { get; set; }

}

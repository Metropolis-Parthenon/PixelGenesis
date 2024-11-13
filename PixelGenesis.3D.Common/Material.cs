using PixelGenesis.ECS;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixelGenesis._3D.Common;

public class Material : IWritableAsset, IReadableAsset
{
    public string Reference { get; set; }
    public CompiledShader Shader { get; set; }

    private Dictionary<int, Dictionary<int, object>> Parameters = new();
    private Dictionary<int, Texture> Textures = new();

    private Material(string reference, CompiledShader shader)
    {
        Reference = reference;
        Shader = shader;
        InitializeParametersFromShaderLayout();
    }

    // Overloads for setting parameters of specific types
    public void SetParameter(int blockBinding, int parameterIndex, float value) => SetParameterInternal(blockBinding, parameterIndex, value, Type.Float);
    public void SetParameter(int blockBinding, int parameterIndex, Vector2 value) => SetParameterInternal(blockBinding, parameterIndex, value, Type.Float2);
    public void SetParameter(int blockBinding, int parameterIndex, Vector3 value) => SetParameterInternal(blockBinding, parameterIndex, value, Type.Float3);
    public void SetParameter(int blockBinding, int parameterIndex, Vector4 value) => SetParameterInternal(blockBinding, parameterIndex, value, Type.Float4);
    public void SetParameter(int blockBinding, int parameterIndex, Matrix4x4 value) => SetParameterInternal(blockBinding, parameterIndex, value, Type.Mat4);

    // Sets a texture based on binding
    public void SetTexture(int binding, Texture texture)
    {
        Textures[binding] = texture;
    }

    // Internal method with type checking for setting a parameter
    private void SetParameterInternal(int blockBinding, int parameterIndex, object value, Type expectedType)
    {
        // Retrieve reference to block dictionary
        ref var block = ref CollectionsMarshal.GetValueRefOrNullRef(Parameters, blockBinding);
        if (Unsafe.IsNullRef(ref block))
        {
            throw new ArgumentException($"Invalid block binding: {blockBinding}");
        }

        // Retrieve reference to parameter value in the block dictionary
        ref var parameterValue = ref CollectionsMarshal.GetValueRefOrNullRef(block, parameterIndex);
        if (Unsafe.IsNullRef(ref parameterValue))
        {
            throw new ArgumentException($"Invalid parameter index: {parameterIndex} in block {blockBinding}");
        }

        // Validate type using the shader layout
        var actualType = Shader.Layout.Blocks[blockBinding].Parameters[parameterIndex].Type;
        if (actualType != expectedType)
        {
            throw new ArgumentException($"Type mismatch: Expected {actualType} for parameter at block {blockBinding}, index {parameterIndex}, but received {expectedType}.");
        }

        parameterValue = value; // Assign the value if type matches
    }

    // Initializes parameters and textures based on the shader layout
    private void InitializeParametersFromShaderLayout()
    {
        foreach (var block in Shader.Layout.Blocks)
        {
            if (!Parameters.ContainsKey(block.Binding))
            {
                Parameters[block.Binding] = new Dictionary<int, object>();
            }

            for (int i = 0; i < block.Parameters.Count; i++)
            {
                var parameter = block.Parameters[i];
                Parameters[block.Binding][i] = GetDefaultValue(parameter.Type);
            }
        }

        foreach (var texture in Shader.Layout.Textures)
        {
            Textures[texture.Binding] = null; // Placeholder for textures to be assigned later
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

    public void WriteToStream(Stream stream)
    {
        using var textWriter = new StreamWriter(stream);
        textWriter.WriteLine(Shader.Reference);
    }

    // Factory for reading and creating materials from assets
    public class Factory : IReadableAssetFactory<Material>
    {
        public Material ReadAsset(string reference, Stream stream)
        {
            using var textReader = new StreamReader(stream);
            var shaderReference = textReader.ReadLine() ?? throw new InvalidOperationException();
            var shader = AssetReader.ReadAsset<CompiledShader, CompiledShader.Factory>(shaderReference);

            return new Material(reference, shader);
        }
    }
}

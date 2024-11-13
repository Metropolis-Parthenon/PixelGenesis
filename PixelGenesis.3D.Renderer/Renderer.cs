using PixelGenesis._3D.Common;
using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis._3D.Renderer;
public class Renderer(IDeviceApi deviceApi, IPGWindow pGWindow)
{
    public void RenderScene(EntityManager entityManager, PerspectiveCameraComponent camera)
    {
        // Set up view and projection matrices from the camera
        var viewMatrix = camera.GetViewMatrix();
        var aspectRatio = pGWindow.Width / pGWindow.Height;
        var projectionMatrix = camera.GetProjectionMatrix(aspectRatio);

        // Dictionary to group MeshRendererComponents by (Mesh, Material) for instancing
        var instanceGroups = new Dictionary<(IMesh, Material), List<Matrix4x4>>();

        // Get all MeshRendererComponents directly
        var meshRenderers = entityManager.GetComponents<MeshRendererComponent>();

        // Group MeshRendererComponents based on shared Mesh and Material components
        foreach (MeshRendererComponent meshRenderer in meshRenderers)
        {
            if (meshRenderer.Mesh == null || meshRenderer.Material == null)
                continue;

            // Get the model matrix from the associated Transform3DComponent
            var modelMatrix = meshRenderer.GetTransform().GetModelMatrix();
            var key = (meshRenderer.Mesh, meshRenderer.Material);

            if (!instanceGroups.ContainsKey(key))
            {
                instanceGroups[key] = new List<Matrix4x4>();
            }
            instanceGroups[key].Add(modelMatrix);
        }

        // Render each group with instancing if it contains multiple instances
        foreach (var group in instanceGroups)
        {
            RenderInstancedEntities(group.Key.Item1, group.Key.Item2, group.Value, deviceApi, viewMatrix, projectionMatrix);
        }
    }

    private void RenderInstancedEntities(IMesh mesh, Material material, List<Matrix4x4> modelMatrices, IDeviceApi deviceApi, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        // Create or update an instance buffer with model matrices
        var instanceBuffer = CreateInstanceBuffer(deviceApi, modelMatrices);

        // Set up the drawing context with the shader, vertex buffer, and index buffer
        var drawContext = new DrawContext
        {
            ShaderProgram = deviceApi.GetShaderProgramById(material.Shader.ProgramId),
            VertexBuffer = deviceApi.GetVertexBufferById(mesh.VertexBufferId),
            IndexBuffer = deviceApi.GetIndexBufferById<uint>(mesh.IndexBufferId)
        };

        // Set view-projection matrix as a single uniform
        var viewProjectionMatrix = viewMatrix * projectionMatrix;
        var uniformBuffer = deviceApi.GetUniformBlockBufferById(material.UniformBufferId);
        uniformBuffer.SetData(new[] { viewProjectionMatrix });

        // Set additional material parameters
        foreach (var param in material.Parameters)
        {
            SetUniformParameter(deviceApi, drawContext.ShaderProgram, param.Key, param.Value);
        }

        // Set texture bindings
        foreach (var texture in material.Textures)
        {
            var textureObj = deviceApi.GetTextureById(texture.Value.TextureId);
            drawContext.ShaderProgram.SetTexture(texture.Key, textureObj);
        }

        // Render using instancing, with instance count set to the number of model matrices
        deviceApi.DrawTriangles(drawContext, modelMatrices.Count, instanceBuffer, new VertexBufferLayout());
    }

    private IInstanceBuffer CreateInstanceBuffer(IDeviceApi deviceApi, List<Matrix4x4> modelMatrices)
    {
        // Convert the list of model matrices to a byte array to upload to the instance buffer
        var data = new byte[modelMatrices.Count * sizeof(Matrix4x4)];
        Buffer.BlockCopy(modelMatrices.ToArray(), 0, data, 0, data.Length);
        return deviceApi.CreateInstanceBuffer(data, BufferHint.Dynamic);
    }

    private void SetUniformParameter(IDeviceApi deviceApi, IShaderProgram shader, int location, object value)
    {
        switch (value)
        {
            case Matrix4x4 matrix:
                shader.SetUniformMatrix4(location, matrix);
                break;
            case Vector3 vector:
                shader.SetUniformVector3(location, vector);
                break;
            case Vector4 vector4:
                shader.SetUniformVector4(location, vector4);
                break;
            case float f:
                shader.SetUniformFloat(location, f);
                break;
            case Vector2 vector2:
                shader.SetUniformVector2(location, vector2);
                break;
            default:
                throw new ArgumentException("Unsupported parameter type for uniform", nameof(value));
        }
    }
}
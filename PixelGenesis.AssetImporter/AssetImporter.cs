using Assimp;
using CommunityToolkit.HighPerformance;
using Microsoft.Extensions.DependencyInjection;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Components;
using PixelGenesis.ECS.Scene;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PixelGenesis.AssetImporter;


public class AssetImporter(IAssetManager assetManager, IKeyedServiceProvider provider, ComponentsFactory factory, Guid pbrShaderId, Guid nonPBRShaderId)
{
    public PGScene Import(string file, string outputRelativeFile)
    {
        var outputFolder = Path.GetDirectoryName(outputRelativeFile) ?? throw new Exception();
        var fileNamePrefix = Path.GetFileNameWithoutExtension(file);
        var pgScene = new PGScene(Guid.NewGuid(), factory, provider, Path.GetFileName(outputRelativeFile));

        var context = new AssimpContext();

        var scene = context.ImportFile(file);

        var importedMeshes = ImportMeshes(scene.Meshes, outputFolder, fileNamePrefix);
        var importedTextures = ImportTextures(scene.Textures, outputFolder, fileNamePrefix);
        var importedMaterials = ImportMaterials(scene.Materials, file, outputFolder, fileNamePrefix);

        ImportNode(scene.RootNode, null, pgScene, scene, importedMeshes, importedMaterials);

        assetManager.SaveAsset(pgScene, Path.Combine(outputFolder, pgScene.Name));

        return pgScene;
    }

    void ImportNode(Node node, Entity? parent, PGScene pgScene, Scene scene, Dictionary<int, IMesh> meshes, Dictionary<int, _3D.Common.Material> materials)
    {
        var entity = pgScene.Create(node.Name);
        if(parent is not null)
        {
            pgScene.SetEntityParent(entity, parent);
        }

        var transform = entity.Transform;

        // set transformation
        node.Transform.Decompose(out var scaling, out var rotation, out var translation);
        transform.Position = new Vector3(translation.X, translation.Y, translation.Z);
        transform.Rotation = new System.Numerics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        transform.Scale = new Vector3(scaling.X, scaling.Y, scaling.Z);

        //set meshes
        if(node.MeshIndices.Count == 1)
        {
            var meshIndex = node.MeshIndices[0];

            var mesh = meshes[meshIndex];
            var assMesh = scene.Meshes[meshIndex];

            var matIndex = assMesh.MaterialIndex;
            var material = materials[matIndex];

            var meshRenderer = entity.AddComponent<MeshRendererComponent>();
            meshRenderer.Mesh = mesh;
            meshRenderer.Material = material;
        }
        else if(node.MeshIndices.Count > 1)
        {
            var meshesContainer = pgScene.Create("mesh_container");
            pgScene.SetEntityParent(meshesContainer, entity);
            for (var i = 0; i < node.MeshIndices.Count; i++)
            {
                var meshEntity = pgScene.Create($"mesh_{i}");
                pgScene.SetEntityParent(meshEntity, meshesContainer);                

                var meshIndex = node.MeshIndices[0];
                var mesh = meshes[meshIndex];
                var assMesh = scene.Meshes[meshIndex];
                var matIndex = assMesh.MaterialIndex;
                var material = materials[matIndex];

                var meshRenderer = meshEntity.AddComponent<MeshRendererComponent>();
                meshRenderer.Mesh = mesh;
                meshRenderer.Material = material;
            }
        }

        for(var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            ImportNode(child, entity, pgScene, scene, meshes, materials);
        }

    }

    //static void ImportAnimation(List<Assimp.Animation> animation, string outputFolder, string filePrefix)
    //{
    //    // TODO import animations        
    //}

    Dictionary<int, IMesh> ImportMeshes(List<Assimp.Mesh> meshes, string outputFolder, string filePrefix)
    {        
        var result = new Dictionary<int, IMesh>();

        var meshesSpan = CollectionsMarshal.AsSpan(meshes);
        for(var i = 0; i < meshesSpan.Length; i++)
        {
            var mesh = meshesSpan[i];
            var asset = new MutableMesh(Guid.NewGuid(), $"{filePrefix}_{mesh.Name}_{i}.pgmesh");

            asset.MutableVertices = CollectionsMarshal.AsSpan(mesh.Vertices).Cast<Vector3D, Vector3>().ToArray();
            asset.MutableTriangles = new Memory<int>(mesh.GetIndices()).Cast<int, uint>();
            asset.MutableNormals = CollectionsMarshal.AsSpan(mesh.Normals).Cast<Vector3D, Vector3>().ToArray();
            asset.MutableTangents = mesh.Tangents.Select(x => new Vector4(x.X, x.Y, x.Z, 1f)).ToArray();
            asset.MutableColors = mesh.HasVertexColors(0) ? CollectionsMarshal.AsSpan(mesh.VertexColorChannels[0]).Cast<Color4D, Vector4>().ToArray() : Memory<Vector4>.Empty;

            asset.MutableUV1 = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;
            asset.MutableUV2 = mesh.HasTextureCoords(1) ? mesh.TextureCoordinateChannels[1].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;
            asset.MutableUV3 = mesh.HasTextureCoords(2) ? mesh.TextureCoordinateChannels[2].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;
            asset.MutableUV4 = mesh.HasTextureCoords(3) ? mesh.TextureCoordinateChannels[3].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;
            asset.MutableUV5 = mesh.HasTextureCoords(4) ? mesh.TextureCoordinateChannels[4].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;
            asset.MutableUV6 = mesh.HasTextureCoords(5) ? mesh.TextureCoordinateChannels[5].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;
            asset.MutableUV7 = mesh.HasTextureCoords(6) ? mesh.TextureCoordinateChannels[6].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;
            asset.MutableUV8 = mesh.HasTextureCoords(7) ? mesh.TextureCoordinateChannels[7].Select(x => new Vector2(x.X, x.Y)).ToArray() : Memory<Vector2>.Empty;

            assetManager.SaveAsset(asset, Path.Combine(outputFolder, asset.Name));

            result.Add(i, asset);
        }
        
        return result;
    }

    Dictionary<int, Texture> ImportTextures(List<EmbeddedTexture> textures, string outputFolder, string filePrefix)
    {
        var result = new Dictionary<int, Texture>();

        var texturesSpan = CollectionsMarshal.AsSpan(textures);
        for(var i = 0; i < texturesSpan.Length; i++)
        {
            var tex = texturesSpan[i];            
        }

        return result;
    }

    Dictionary<int, _3D.Common.Material> ImportMaterials(List<Assimp.Material> materials, string importFile, string outputFolder, string filePrefix)
    {
        var result = new Dictionary<int, _3D.Common.Material>();

        var materialsSpan = CollectionsMarshal.AsSpan(materials);
        for(var i = 0; i < materialsSpan.Length; i++)
        {
            var material = materialsSpan[i];
            if(material.IsPBRMaterial)
            {
                result.Add(i, ImportPBRMaterial(material, outputFolder, $"{filePrefix}_{material.Name}_{i}.pgmat"));
            }
            else
            {
                result.Add(i, ImportNonPBRMaterial(material, importFile, outputFolder, $"{filePrefix}_{material.Name}_{i}.pgmat"));
            }
        }

        return result;
    }

    _3D.Common.Material ImportNonPBRMaterial(Assimp.Material material, string importFile, string outputFolder, string fileName)
    {
        var shader = assetManager.LoadAsset<PGGLSLShaderSource>(nonPBRShaderId);

        var result = new _3D.Common.Material(Guid.NewGuid(), shader);
        
        if(material.HasTextureDiffuse)
        {
            result.SetTexture("diffuseMap", ImportTextureSlot(material.TextureDiffuse, importFile, outputFolder, $"{fileName}.diffuse.pgtex"));
        }

        if (material.HasTextureSpecular) 
        {
            result.SetTexture("specularMap", ImportTextureSlot(material.TextureSpecular, importFile, outputFolder, $"{fileName}.spec.pgtex"));
        }

        if(material.HasColorAmbient)
        {
            result.SetParameter("material", "ambient", new Vector3(material.ColorAmbient.R, material.ColorAmbient.G, material.ColorAmbient.B));
        }
        else
        {
            result.SetParameter("material", "ambient", Vector3.Zero);
        }

        if (material.HasColorDiffuse) 
        {
            result.SetParameter("material", "diffuse", new Vector3(material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B));
        }
        else
        {
            result.SetParameter("material", "diffuse", Vector3.Zero);
        }

        if (material.HasColorSpecular) 
        {
            result.SetParameter("material", "specular", new Vector3(material.ColorSpecular.R, material.ColorSpecular.G, material.ColorSpecular.B));
        }

        result.SetParameter("material", "shininess", material.Shininess);

        assetManager.SaveAsset(result, Path.Combine(outputFolder, fileName));

        return result;
    }

    _3D.Common.Material ImportPBRMaterial(Assimp.Material material, string outputFolder, string fileName)
    {
        throw new NotImplementedException();
    }

    Texture ImportTextureSlot(TextureSlot texture, string importFile, string outputFolder, string fileName)
    {
        var filePath = texture.FilePath;

        if (!Path.IsPathRooted(filePath))
        {
            filePath = Path.Combine(Path.GetDirectoryName(importFile) ?? "", filePath);
        }

        var result = Texture.FromImageFile(filePath, StbImageSharp.ColorComponents.RedGreenBlueAlpha, fileName);

        assetManager.SaveAsset(result, Path.Combine(outputFolder, fileName));

        return result;
    }
}

using PixelGenesis._3D.Common;
using PixelGenesis._3D.Common.Components;
using PixelGenesis.AssetImporter;
using PixelGenesis.ECS;
using Shader.Sandbox;

ComponentInitializer.Initialize();
AssetManager.AddAssetLoaderFactory(new PGGLSLShaderSource.Factory(), ".pgshader");
AssetManager.AddAssetLoaderFactory(new Material.Factory(), ".pgmat");
AssetManager.AddAssetLoaderFactory(new Texture.Factory(), ".pgtex");
AssetManager.AddAssetLoaderFactory(new Mesh.MeshFactory(), ".pgmesh");

var modelPath = "C:\\Users\\thesk\\OneDrive\\Documents\\Projects\\GameEngine\\PixelGenesis\\Shader.Sandbox\\backpack\\backpack.obj";
//var outputPath = "C:\\Users\\thesk\\OneDrive\\Documents\\Projects\\GameEngine\\PixelGenesis\\Shader.Sandbox\\Survival_Backpack_PG\\";

AssetManager assetManager = new AssetManager("C:\\Users\\thesk\\OneDrive\\Documents\\Projects\\GameEngine\\PixelGenesis\\Shader.Sandbox\\Assets\\");
//var nonPBRShaderId = assetManager.AddAssetFromFile("simple_lit_shader.pgshader");


//var assetImporter = new AssetImporter(assetManager, Guid.Empty, nonPBRShaderId);

//var scene = assetImporter.Import(modelPath, "model.pgscene");

using (RendererWindowTest game = new RendererWindowTest(800, 600, "LearnOpenTK", assetManager, Guid.Parse("ef8eedd2-0ad5-499b-b3e0-97069dd5d63f")))
{
    game.Run();
}



//var scene = assetManager.LoadAsset<PGScene>(Guid.Parse("dc7b032b-3943-401f-adf3-e01ab80db468"));
//var entity = scene.Entities[0];

//var child = scene.Create("Child");
//scene.SetEntityParent(child, entity);

//var meshRenderer = child.AddComponent<MeshRendererComponent>();

//Console.ReadKey();

//var scene = new PGScene(Guid.NewGuid());

//var entity = scene.Create("Test Entity");
//entity.AddComponent<Transform3DComponent>();

//assetManager.SaveAsset(scene, "scene.pgscene");
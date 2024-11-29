using Microsoft.Extensions.DependencyInjection;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Common.Components;
using PixelGenesis.AssetImporter;
using PixelGenesis.ECS;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Components;
using Shader.Sandbox;

var services = new ServiceCollection();

services.AddPixelGenesisECS();
services.AddSingleton<IAssetManager>(
    (IServiceProvider provider) => 
    new AssetManager("C:\\Users\\thesk\\OneDrive\\Documents\\Projects\\GameEngine\\PixelGenesis\\Shader.Sandbox\\Assets\\", 
    provider));

PixelGenesis._3D.Common.Components.ServiceCollectionComponentFactoriesExtensions.AddComponentFactories(services);

services.AddPixelGenesisReadAssetFactory<PGGLSLShaderSource.Factory>(".pgshader");
services.AddPixelGenesisReadAssetFactory< Material.Factory> (".pgmat");
services.AddPixelGenesisReadAssetFactory<Texture.Factory>(".pgtex");
services.AddPixelGenesisReadAssetFactory<Mesh.Factory>(".pgmesh");

//var outputPath = "C:\\Users\\thesk\\OneDrive\\Documents\\Projects\\GameEngine\\PixelGenesis\\Shader.Sandbox\\Survival_Backpack_PG\\";

var serviceProvider = services.BuildServiceProvider();

IAssetManager assetManager = serviceProvider.GetRequiredService<IAssetManager>();
var nonPBRShaderId = assetManager.AddAssetFromFile("simple_lit_shader.pgshader");

var assetImporter = new AssetImporter(
    serviceProvider.GetRequiredService<IAssetManager>(),
    serviceProvider,
    serviceProvider.GetRequiredService<ComponentsFactory>(),
    Guid.Empty,
    nonPBRShaderId);
//var assetImporter = new AssetImporter(assetManager, Guid.Empty, nonPBRShaderId);

var modelPath = "C:\\Users\\thesk\\OneDrive\\Documents\\Projects\\GameEngine\\PixelGenesis\\Shader.Sandbox\\backpack\\backpack.obj";
//var scene = assetImporter.Import(modelPath, "model.pgscene");



using (RendererWindowTest game = new RendererWindowTest(800, 600, "LearnOpenTK", assetManager, Guid.Parse("af1fca2e-e287-46f4-ab03-e0674f969051")))
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
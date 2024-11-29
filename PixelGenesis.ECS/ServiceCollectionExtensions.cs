using Microsoft.Extensions.DependencyInjection;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Components;
using PixelGenesis.ECS.Scene;

namespace PixelGenesis.ECS;

public static class ServiceCollectionExtensions
{
    public static void AddPixelGenesisECS(this IServiceCollection services)
    {        
        services.AddSingleton<ComponentsFactory>();
        services.AddKeyedSingleton<IReadAssetFactory, PGScene.PGSceneFactory>(".pgscene");
        services.AddComponentFactories();
    }

    public static void AddPixelGenesisComponentFactory<T, F>(this IServiceCollection services) where T : Component where F : class, IComponentFactory
    {
        services.AddKeyedSingleton<IComponentFactory, F>(typeof(T));
    }

    public static void AddPixelGenesisReadAssetFactory<T>(this IServiceCollection services, string fileExtension) where T : class, IReadAssetFactory
    {
        services.AddKeyedSingleton<IReadAssetFactory, T>(fileExtension);
    }
}

using Microsoft.Extensions.DependencyInjection;
using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common;

public static class ServiceCollectionExtensions
{
    public static void Add3DAssetsFactories(this IServiceCollection services)
    {
        services.AddPixelGenesisReadAssetFactory<PGGLSLShaderSource.Factory>(".pgshader");
        services.AddPixelGenesisReadAssetFactory<Material.Factory>(".pgmat");
        services.AddPixelGenesisReadAssetFactory<Texture.Factory>(".pgtex");
        services.AddPixelGenesisReadAssetFactory<Mesh.Factory>(".pgmesh");
    }
}

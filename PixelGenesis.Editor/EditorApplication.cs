using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DeviceApi.OpenGL;
using PixelGenesis.ECS;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Scene;
using PixelGenesis.Editor.BuiltIn.AssetEditors;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.GUI;
using PixelGenesis.Editor.Services;

namespace PixelGenesis.Editor;

internal sealed class EditorApplication(EditorWindow window, IHost host) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        window.Closing += Window_Closing;
        window.Run();
        return Task.CompletedTask;
    }

    private void Window_Closing(System.ComponentModel.CancelEventArgs obj)
    {
        host.StopAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        window.Dispose();
        return Task.CompletedTask;
    }


    public static IHost CreateEditorApplicationHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        ConfigureService(builder);
        return builder.Build();
    }

    static void ConfigureService(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ImageLoader>();
        builder.Services.AddSingleton<SaveService>();
        builder.Services.AddSingleton<SolutionService>();
        builder.Services.AddSingleton<MenuItemGUIRenderer>();
        builder.Services.AddSingleton<EditorWindowsGUIRenderer>();
        builder.Services.AddSingleton<PixelGenesisEditor>();
        builder.Services.AddSingleton<IEditionCommandDispatcher, EditionCommandDispatcher>();
        builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        builder.Services.AddSingleton<IDeviceApi, OpenGLDeviceApi>();
        builder.Services.AddSingleton((sp) => new EditorWindow(
            1920, 
            1080, 
            "Pixel Genesis Editor",
            sp.GetRequiredService<IDeviceApi>(), 
            sp.GetRequiredKeyedService<PixelGenesisEditor>(default),
            sp.GetRequiredService<ICommandDispatcher>()
        ));
        builder.Services.AddSingleton<IPGWindow>(provider => provider.GetRequiredService<EditorWindow>());

        builder.Services.Scan(
            scan => 
            scan
            .FromCallingAssembly()
            .AddClasses(
                classes => 
                classes.AssignableTo<IEditorMenuAction>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        builder.Services.Scan(
            scan =>
            scan
            .FromCallingAssembly()
            .AddClasses(
                classes =>
                classes.AssignableTo<IEditorWindow>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        builder.Services.Scan(
            scan =>
            scan
            .FromCallingAssembly()
            .AddClasses(
                classes =>
                classes.AssignableTo<IAssetEditor>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());


        builder.Services.AddSingleton<IEditorAssetManager, EditorAssetManager>();
        builder.Services.AddSingleton<IAssetManager>(provider => provider.GetRequiredService<IEditorAssetManager>());

        AddFileEditorFactories(builder);
        builder.Services.AddPixelGenesisECS();
        builder.Services.Add3DAssetsFactories();
        _3D.Common.Components.ServiceCollectionComponentFactoriesExtensions.AddComponentFactories(builder.Services);

        builder.Services.AddHostedService<EditorApplication>();
    }

    static void AddFileEditorFactories(HostApplicationBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IAssetEditorFactory, SceneEditorFactory>(".pgscene");
    }
}
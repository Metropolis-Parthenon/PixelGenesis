using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        builder.Services.AddSingleton<SaveService>();
        builder.Services.AddSingleton<SolutionService>();
        builder.Services.AddSingleton<MenuItemGUIRenderer>();
        builder.Services.AddSingleton<EditorWindowsGUIRenderer>();
        builder.Services.AddSingleton<PixelGenesisEditor>();
        builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        builder.Services.AddSingleton((sp) => new EditorWindow(1920, 1080, "Pixel Genesis Editor", sp.GetRequiredKeyedService<PixelGenesisEditor>(default)));

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

        builder.Services.AddHostedService<EditorApplication>();
    }

}

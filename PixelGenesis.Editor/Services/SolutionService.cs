using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using PixelGenesis.Editor.Core;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;


namespace PixelGenesis.Editor.Services;

internal sealed class SolutionService
{
    ICommandDispatcher commandDispatcher;

    public SolutionService(ICommandDispatcher commandDispatcher)
    {
        this.commandDispatcher = commandDispatcher;
        MSBuildLocator.RegisterDefaults();
    }

    [MemberNotNullWhen(true, nameof(IsProjectOpen))]
    public MSBuildWorkspace? Workspace { get; private set; }

    [MemberNotNullWhen(true, nameof(IsProjectOpen))]
    public string? SolutionPath { get; private set; }

    [MemberNotNullWhen(true, nameof(IsProjectOpen))]
    public Solution? Solution { get; private set; }

    [MemberNotNullWhen(true, nameof(HasEditorProject))]
    public Project? EditorProject { get; private set; }
    public bool HasEditorProject => EditorProject is not null;

    [MemberNotNullWhen(true, nameof(IsProjectOpen))]
    public Project? EntryProject { get; private set; }

    public bool IsProjectOpen => !string.IsNullOrEmpty(SolutionPath) && Solution is not null && Workspace is not null && EntryProject is not null;

    public void OpenSolution(string solutionPath)
    {
        SolutionPath = solutionPath;
        Workspace?.Dispose();
        Workspace = MSBuildWorkspace.Create();

        Solution = Workspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();

        var directory = Path.GetDirectoryName(solutionPath);
        var configurationFile = Path.Combine(directory, "pixelgenesis.json");

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configurationFile));

        EntryProject = Solution.Projects.FirstOrDefault(p => p.Name == config.entryProject);
        EditorProject = Solution.Projects.FirstOrDefault(p => p.Name == config.editorProject);
                
        commandDispatcher.Dispatch(new SolutionOpened());
    }

    public void CreateNewProject()
    {
        // TODO
    }

}

public record SolutionOpened();

public record Config(string entryProject, string editorProject);

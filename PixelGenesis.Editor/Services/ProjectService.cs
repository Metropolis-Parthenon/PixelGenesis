using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.Services;

public class ProjectService
{
    string SolutionPath = "";

    bool IsProjectOpen => !string.IsNullOrEmpty(SolutionPath);

    public void OpenSolution(string solutionPath)
    {
        SolutionPath = solutionPath;
    }

    public string GetProjectPath()
    {
        return SolutionPath;
    }

    public void CreateNewProject()
    {
        // TODO
    }

}

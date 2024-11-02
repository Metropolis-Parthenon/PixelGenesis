using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.Services;

public class ProjectService
{
    string ProjectPath = "";

    bool IsProjectOpen => !string.IsNullOrEmpty(ProjectPath);

    public void OpenProject(string projectPath)
    {        
        ProjectPath = projectPath;
    }

    public string GetProjectPath()
    {
        return ProjectPath;
    }
}

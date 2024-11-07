using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public static class ShadersHelper
{
#warning: Change this to a more scalable solution, maybe a way to embed the glslc.exe in the project
    const string GLSLCPath = "C:\\VulkanSDK\\1.3.296.0\\Bin\\glslc.exe";

    public static ReadOnlyMemory<byte> CompileGLSLSourceToSpirvBytecode(string source, string extension)
    {
        var outputPath = CompileGLSLSourceToSpirv(source, extension);
        var bytes = File.ReadAllBytes(outputPath);
        File.Delete(outputPath);
        return bytes;
    }

    public static string CompileGLSLSourceToSpirv(string source, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "." + extension);

        File.WriteAllText(path, source);
        var outputPath = CompileGLSLFileToSpirv(path);

        File.Delete(path);

        return outputPath;
    }

    public static string CompileGLSLFileToSpirv(string path)
    {
        var outputPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".spv");

        var startInfo = new ProcessStartInfo()
        {
            FileName = GLSLCPath,
            ArgumentList = { path, "-o", outputPath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var process = Process.Start(startInfo);

        process.WaitForExit();
        if (process.ExitCode is not 0)
        {
            var message = process.StandardError.ReadToEnd();
            throw new InvalidOperationException("Cannot compile spirv: " + message);
        }

        return outputPath;
    }
}

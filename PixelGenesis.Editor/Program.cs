// See https://aka.ms/new-console-template for more information


using Microsoft.Extensions.Hosting;
using PixelGenesis.Editor;

var app = EditorApplication.CreateEditorApplicationHost(args);

app.Run();
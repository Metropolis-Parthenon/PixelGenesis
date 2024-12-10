using ImGuiNET;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System;
using System.Drawing;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PixelGenesis.Editor.Core;

public class FrameBufferGuiWindow(IDeviceApi deviceApi, IPGWindow mainWindow, Action onFrame) : IPGWindow
{
    bool isInitialized = false;

    Vector2 LastWindowSize;

    IFrameBuffer frameBuffer;

    //public Size ViewportSize => ViewportSizeObservableSubject.Value;
    //public IObservable<Size> ViewportSizeObservable => ViewportSizeObservableSubject.AsObservable();
    //BehaviorSubject<Size> ViewportSizeObservableSubject = new BehaviorSubject<Size>(new Size(0,0));

    public Size WindowSize { get; private set; }

    public int FrameBufferId => frameBuffer.Id;

    public Size ViewportSize => mainWindow.ViewportSize;

    public IObservable<Size> ViewportSizeObservable => mainWindow.ViewportSizeObservable;

    bool TryInitialize()
    {
        if(isInitialized) return true;
                
        LastWindowSize = ImGui.GetContentRegionAvail();

        if (LastWindowSize.X <= 0 || LastWindowSize.Y <= 0) 
        {
            return false;
        }

        var newSize = new Size((int)LastWindowSize.X, (int)LastWindowSize.Y);
        WindowSize = newSize;        

        //ViewportSizeObservableSubject.OnNext(newSize);

        frameBuffer = deviceApi.CreateFrameBuffer(mainWindow.ViewportSize.Width, mainWindow.ViewportSize.Height);

        isInitialized = true;
        return true;
    }

    public void OnGui()
    {
        if(!TryInitialize()) return;
                
        var windowRegionSize = ImGui.GetContentRegionAvail();
        if (LastWindowSize != windowRegionSize && (int)windowRegionSize.X > 0 && (int)windowRegionSize.Y > 0)
        {
            var newSize = new Size((int)windowRegionSize.X, (int)windowRegionSize.Y);
            //frameBuffer.Rescale(newSize.Width, newSize.Height);
            LastWindowSize = windowRegionSize;
            WindowSize = newSize;
            //ViewportSizeObservableSubject.OnNext(newSize);
        }

        Console.WriteLine(windowRegionSize);

        frameBuffer.Bind();
        //deviceApi.Viewport(0,0,ViewportSize.Width, ViewportSize.Height);
        onFrame();
        frameBuffer.Unbind();
        
        ImGui.Image(frameBuffer.GetTexture().Id, windowRegionSize, new Vector2(0, 1), new Vector2(1, 0));
    }
}

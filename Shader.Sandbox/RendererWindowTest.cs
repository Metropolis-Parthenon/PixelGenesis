using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DeviceApi.OpenGL;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer;
using PixelGenesis.ECS;
using PixelGenesis._3D.Common.Components;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using PixelGenesis._3D.Common.Components.Lighting;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Scene;
using PixelGenesis._3D.Renderer.DrawPipeline;
using Assimp;
using static Assimp.Metadata;

namespace Shader.Sandbox;


internal class RendererWindowTest : GameWindow, IPGWindow
{
    IAssetManager assetManager;
    Guid sceneId;

    public RendererWindowTest(int width, int height, string title, IAssetManager assetManager, Guid sceneId) : base(SetSettings(GameWindowSettings.Default), new NativeWindowSettings() { ClientSize = (width, height), Title = title }) 
    {
        this.assetManager = assetManager;
        this.sceneId = sceneId;
    }

    static GameWindowSettings SetSettings(GameWindowSettings settings)
    {
        settings.UpdateFrequency = 60;
        return settings;
    }

    IDeviceApi deviceApi = new OpenGLDeviceApi();
    ForwardRenderer renderer;
    PGScene scene;

    PerspectiveCameraComponent PerspectiveCameraComponent;

    public float Width => Size.X;

    public float Height => Size.Y;

    Entity light;
    Entity original;
    int index = 1;

    protected override void OnLoad()
    {
        base.OnLoad();

        scene = assetManager.LoadAsset<PGScene>(sceneId); //new PGScene(Guid.NewGuid());
        renderer = new ForwardRenderer(scene, deviceApi, this);

        // setup scene

        //var goldMat = GoldMaterial();
        //var chromeMat = ChromeMaterial();
        //var jadeMat = JadeMaterial();

        light = scene.Create("PointLight");
        var pointLightComponent = light.AddComponent<PointLightComponent>();
        pointLightComponent.Color = new Vector3(1, 1, 1);
        pointLightComponent.Intensity = 1f;
        pointLightComponent.Transform.Position = new Vector3(0, 0, -1);

        original = scene.Entities[0];        

        ////entityManager.Clone(light).GetComponent<Transform3DComponent>().Position = new Vector3(0, -1f, 0);

        ////entityManager.Clone(light).GetComponent<Transform3DComponent>().Position = new Vector3(1, 0, 0);
        ////entityManager.Clone(light).GetComponent<Transform3DComponent>().Position = new Vector3(-1, 0, 0);

        ////entityManager.Clone(light).GetComponent<Transform3DComponent>().Position = new Vector3(0, 0, 1);
        ////entityManager.Clone(light).GetComponent<Transform3DComponent>().Position = new Vector3(0, 0, -1);

        ////bunny


        //var cube = entityManager.Create("Cube");
        //var meshRenderer = cube.AddComponent<MeshRendererComponent>();
        //meshRenderer.Material = goldMat;

        //using var bunnyMeshFileStream = File.OpenRead(bunnyMeshFile);
        //meshRenderer.Mesh = (IMesh)new Mesh.MeshFactory().ReadAsset(Guid.NewGuid(), null, bunnyMeshFileStream);


        //meshRenderer = entityManager.Clone(cube).GetComponent<MeshRendererComponent>();
        //meshRenderer.Material = chromeMat;
        //meshRenderer.GetTransform().Position = new Vector3(1, 0, 0);

        //meshRenderer = entityManager.Clone(cube).GetComponent<MeshRendererComponent>();
        //meshRenderer.Material = jadeMat;
        //meshRenderer.GetTransform().Position = new Vector3(-1, 0, 0);

        //cube.GetComponent<Transform3DComponent>().Rotation = Quaternion.CreateFromYawPitchRoll((MathF.PI / 180f) * 45f, 0, 0f);
        //cube.GetComponent<Transform3DComponent>().Rotation = Quaternion.CreateFromYawPitchRoll(-180f * (MathF.PI / 180), 0f, 0f);

        //var offset = 0f;
        //for (var x = -2; x < 2; x++)
        //{
        //    for (var y = -2; y < 2; y++)
        //    {
        //        var clone = entityManager.Clone(cube, $"Cube{x}{y}");
        //        clone.GetComponent<Transform3DComponent>().Position = new Vector3(x * 2f + offset, y * 2f + offset, 0f);
        //    }
        //}

        //camera
        var camera = scene.Create("Camera");
        PerspectiveCameraComponent = camera.AddComponent<PerspectiveCameraComponent>();
        // skybox
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "skybox");
        PerspectiveCameraComponent.Skybox = [
            Texture.FromImageFile(Path.Combine(basePath, "right.jpg"), ColorComponents.RedGreenBlue),
            Texture.FromImageFile(Path.Combine(basePath, "left.jpg"), ColorComponents.RedGreenBlue),
            Texture.FromImageFile(Path.Combine(basePath, "top.jpg"), ColorComponents.RedGreenBlue),
            Texture.FromImageFile(Path.Combine(basePath, "bottom.jpg"), ColorComponents.RedGreenBlue),
            Texture.FromImageFile(Path.Combine(basePath, "front.jpg"), ColorComponents.RedGreenBlue),
            Texture.FromImageFile(Path.Combine(basePath, "back.jpg"), ColorComponents.RedGreenBlue)
            ];

        camera.GetComponent<Transform3DComponent>().Position = new Vector3(0, 0, 5);

        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        renderer.Initialize();

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);

    }

    float timePassed;
    protected override void OnRenderFrame(FrameEventArgs args)
    {        
        base.OnRenderFrame(args);

        HandleInput((float)args.Time);

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (timePassed >= 0.02f)
        {
            var transform = scene.Clone(original).GetComponent<Transform3DComponent>();
            transform.Position.X = index * 4;

            if (index % 10 is 0)
            {
                var lightTransform = scene.Clone(light).GetComponent<Transform3DComponent>();
                lightTransform.Position.X = index * 4;
            }
            timePassed = 0f;
            index++;
        }
        //timePassed += (float)args.Time;
        

        

        renderer.Update();
        renderer.Draw();

        Console.Clear();
        Console.WriteLine($"FPS: {1d / args.Time}");
        //Console.WriteLine($"Update: {renderer.Metrics.UpdateTime}");
        //Console.WriteLine($"Render: {renderer.Metrics.RenderTime}");
        //Console.WriteLine($"Draw Calls: {renderer.Metrics.DrawCalls}");

        SwapBuffers();
    }

    void HandleInput(float deltaTime)
    {
        float speed = 2f;
        var cameraTransform = PerspectiveCameraComponent.Entity.GetComponent<Transform3DComponent>();
        if (KeyboardState.IsKeyDown(Keys.W))
        {
            cameraTransform.Position.Z -= speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.S))
        {
            cameraTransform.Position.Z += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.A))
        {
            cameraTransform.Position.X -= speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.D))
        {
            cameraTransform.Position.X += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            cameraTransform.Position.Y += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.LeftControl))
        {
            cameraTransform.Position.Y -= speed * deltaTime;
        }
        
        if(KeyboardState.IsKeyDown(Keys.Left))
        {            
            cameraTransform.Rotation.Y += speed * deltaTime;
        }

        if(KeyboardState.IsKeyDown(Keys.Right))
        {
            cameraTransform.Rotation.Y -= speed * deltaTime;
        }



    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

}

﻿using OpenTK.Windowing.Common;
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
using PixelGenesis._3D.Common.Geometry;
using StbImageSharp;
using PixelGenesis._3D.Common.Components.Lighting;

namespace Shader.Sandbox;


internal class RendererWindowTest : GameWindow, IPGWindow
{
    AssetManager assetManager;
    Guid sceneId;

    public RendererWindowTest(int width, int height, string title, AssetManager assetManager, Guid sceneId) : base(SetSettings(GameWindowSettings.Default), new NativeWindowSettings() { ClientSize = (width, height), Title = title }) 
    {
        this.assetManager = assetManager;
        this.sceneId = sceneId;
    }

    static GameWindowSettings SetSettings(GameWindowSettings settings)
    {
        settings.UpdateFrequency = 60;
        return settings;
    }

    const string bunnyMeshFile = "C:\\Users\\thesk\\OneDrive\\Documents\\Projects\\GameEngine\\PixelGenesis\\Shader.Sandbox\\buddha.pgmesh";

    IDeviceApi deviceApi = new OpenGLDeviceApi();
    PG3DRenderer renderer;
    PGScene entityManager;

    PerspectiveCameraComponent PerspectiveCameraComponent;

    public float Width => Size.X;

    public float Height => Size.Y;

    protected override void OnLoad()
    {
        base.OnLoad();

        entityManager = assetManager.LoadAsset<PGScene>(sceneId); //new PGScene(Guid.NewGuid());
        renderer = new PG3DRenderer(deviceApi, this, entityManager);

        // setup scene

        //var goldMat = GoldMaterial();
        //var chromeMat = ChromeMaterial();
        //var jadeMat = JadeMaterial();

        var light = entityManager.Create("PointLight");
        var pointLightComponent = light.AddComponent<PointLightComponent>();
        pointLightComponent.Color = new Vector3(1, 1, 1);
        pointLightComponent.Intensity = 1f;
        pointLightComponent.Transform.Position = new Vector3(0, 0, -1);

        var entity = entityManager.Entities[0];

        for (var i = 1; i <= 50; i++)
        {
            var transform = entityManager.Clone(entity).GetComponent<Transform3DComponent>();
            transform.Position.X = i * 4;

            if(i % 10 is 0)
            {
                var lightTransform = entityManager.Clone(light).GetComponent<Transform3DComponent>();
                lightTransform.Position.X = i * 4;
            }
        }

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
        var camera = entityManager.Create("Camera");
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

        camera.GetComponent<Transform3DComponent>().Position = new Vector3(0, 0, -5);

        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        renderer.Initialize();

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);

    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {        
        base.OnRenderFrame(args);

        HandleInput((float)args.Time);

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        renderer.Update();
        renderer.Render();

        Console.Clear();
        Console.WriteLine($"FPS: {1d / args.Time}");
        Console.WriteLine($"Update: {renderer.Metrics.UpdateTime}");
        Console.WriteLine($"Render: {renderer.Metrics.RenderTime}");
        Console.WriteLine($"Draw Calls: {renderer.Metrics.DrawCalls}");

        SwapBuffers();
    }

    void HandleInput(float deltaTime)
    {
        float speed = 2f;
        var cameraTransform = PerspectiveCameraComponent.Entity.GetComponent<Transform3DComponent>();
        if (KeyboardState.IsKeyDown(Keys.W))
        {
            cameraTransform.Position.Z += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.S))
        {
            cameraTransform.Position.Z -= speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.A))
        {
            cameraTransform.Position.X += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.D))
        {
            cameraTransform.Position.X -= speed * deltaTime;
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
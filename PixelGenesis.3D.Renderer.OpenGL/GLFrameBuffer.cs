using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

internal class GLFrameBuffer : IFrameBuffer
{
    int frameBuffer;
    int renderBuffer;
    int renderTexture;

    OpenGLDeviceApi _deviceApi;

    FrameBufferTexture texture;

    public int Id => frameBuffer;

    public GLFrameBuffer(int width, int height, OpenGLDeviceApi deviceApi)
    {
        _deviceApi = deviceApi;

        frameBuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
        renderTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, renderTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, [(uint)TextureMinFilter.Linear]);
        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, [(uint)TextureMagFilter.Linear]);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, renderTexture, 0);

        renderBuffer = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderBuffer);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            throw new Exception("Framebuffer is not complete");
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        texture = new FrameBufferTexture(renderTexture, deviceApi);

        deviceApi._frameBuffers.Add(frameBuffer, this);
        deviceApi._textures.Add(renderTexture, texture);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
    }

    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
        
    public ITexture GetTexture()
    {
        return texture;
    }

    public void Rescale(int width, int height)
    {
        GL.BindTexture(TextureTarget.Texture2D, renderTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, [(uint)TextureMinFilter.Linear]);
        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, [(uint)TextureMagFilter.Linear]);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, renderTexture, 0);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            throw new Exception("Framebuffer is not complete");
        }

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderBuffer);
    }

    public void Dispose()
    {
        GL.DeleteFramebuffer(frameBuffer);        
        GL.DeleteRenderbuffer(renderBuffer);
        texture.Dispose();
        _deviceApi._frameBuffers.Remove(frameBuffer);
    }

    class FrameBufferTexture(int textureId, OpenGLDeviceApi _api) : ITexture
    {
        public int Id => textureId;

        public void SetSlot(int slot)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            OpenGLDeviceApi.ThrowOnGLError();
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            OpenGLDeviceApi.ThrowOnGLError();
        }

        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            OpenGLDeviceApi.ThrowOnGLError();
        }
        public void Dispose()
        {
            GL.DeleteTexture(textureId);
            OpenGLDeviceApi.ThrowOnGLError();
            _api._textures.Remove(textureId);
        }

    }

}

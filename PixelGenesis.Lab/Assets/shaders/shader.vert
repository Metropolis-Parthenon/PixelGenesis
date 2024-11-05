#version 450

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 textCoord;

layout(location = 0) out vec2 v_TexCoord;

layout(binding = 0) uniform Projection {
    mat4 u_MVP;
} projection;

void main()
{
    gl_Position = projection.u_MVP * position;

    // pass the texcoord to the fragment shader
    v_TexCoord = textCoord;
}
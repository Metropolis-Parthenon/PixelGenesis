#version 330 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 textCoord;

out vec2 v_TexCoord;

uniform mat4 u_MVP;

void main()
{
    gl_Position = u_MVP * position;

    // pass the texcoord to the fragment shader
    v_TexCoord = textCoord;
}
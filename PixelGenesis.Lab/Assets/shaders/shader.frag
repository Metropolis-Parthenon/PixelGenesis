#version 450

layout(location = 0) in vec2 v_TexCoord;

layout(location = 0) out vec4 color;

layout(binding = 0) uniform Material
{
    vec4 u_Color;    
} material;

layout(binding = 1) uniform sampler2D u_Texture;

void main()
{
    //vec4 texColor = texture(u_Texture, v_TexCoord);
    color = vec4(1,1,1,1);
}
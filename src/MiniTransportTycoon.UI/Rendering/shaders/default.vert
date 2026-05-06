#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

layout (location = 3) in mat4 iModel; // location [3,6]
layout (location = 7) in vec3 iColor; // location 7

uniform mat4 m_viewProj;

out vec3 vs_out_col;
out vec2 vs_out_texCoord;
out vec3 vs_out_normal;

void main() 
{
    vs_out_col = iColor;
    vs_out_texCoord = aTexCoord;
    mat3 invTranspose = transpose(inverse(mat3(iModel)));
    vs_out_normal = invTranspose * aNormal;
    
    gl_Position = m_viewProj * iModel * vec4(aPosition, 1.0);
}
#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

layout (location = 3) in mat4 iModel; // location [3,6]
layout (location = 7) in vec3 iColor; // location 7

uniform mat4 m_viewProj;
uniform mat4 m_model;
uniform int m_isInstanced;


out vec3 vs_out_col;
out vec2 vs_out_texCoord;
out vec3 vs_out_normal;

void main() 
{
    vs_out_col = (m_isInstanced == 1) ? iColor : vec3(0.5, 0.5, 0.88);
    vs_out_texCoord = aTexCoord;
    mat4 mvp = (m_isInstanced == 1) ? m_viewProj * iModel : m_viewProj * m_model;
    mat3 invTranspose = transpose(inverse(mat3(mvp)));
    vs_out_normal = invTranspose * aNormal;
    
    gl_Position = mvp * vec4(aPosition, 1.0);
}
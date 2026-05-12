#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

layout (location = 3) in vec3 iPosition;
layout (location = 4) in float iScale;
layout (location = 5) in float iRotationY;
layout (location = 6) in vec3 iColor; 

uniform mat4 m_viewProj;
uniform mat4 m_model;
uniform int m_isInstanced;

out vec3 vs_out_col;
out vec2 vs_out_texCoord;
out vec3 vs_out_normal;

mat4 buildModelMatrix(vec3 pos, float scale, float rotY) {
    float c = cos(rotY);
    float s = sin(rotY);
    
    return mat4(
        scale * c,  0.0,   scale * s, 0.0,  // col1
        0.0,        scale, 0.0,       0.0,  // col2
        -scale * s, 0.0,   scale * c, 0.0,  // col3
        pos.x,      pos.y, pos.z,     1.0   // col4
    );
}

void main() 
{
    vs_out_col = (m_isInstanced == 1) ? iColor : vec3(0.5, 0.5, 0.88);
    vs_out_texCoord = aTexCoord;

    mat4 model = (m_isInstanced == 1) ? buildModelMatrix(iPosition, iScale, iRotationY) : m_model;
    
    mat3 invTranspose = transpose(inverse(mat3(model)));
    vs_out_normal = invTranspose * aNormal;
    
    gl_Position = m_viewProj * model * vec4(aPosition, 1.0);
}
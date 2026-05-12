#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

layout (location = 3) in vec3 iPosition;
layout (location = 4) in float iScale;
layout (location = 5) in vec2 aRot;
layout (location = 6) in vec3 iColor; 

uniform mat4 m_viewProj;
uniform mat4 m_model;
uniform int m_isInstanced;

out vec3 vs_out_col;
out vec2 vs_out_texCoord;
out vec3 vs_out_normal;

const float PI = 3.14159265359;

mat4 buildModelMatrix(vec3 pos, float scale, float rotX, float rotY) {
    mat4 S = mat4(
        scale, 0.0,   0.0,   0.0,
        0.0,   scale, 0.0,   0.0,
        0.0,   0.0,   scale, 0.0,
        0.0,   0.0,   0.0,   1.0
    );

    float cx = cos(rotX);
    float sx = sin(rotX);
    mat4 Rx = mat4(
        1.0, 0.0,  0.0, 0.0,
        0.0, cx,   sx,  0.0,
        0.0, -sx,  cx,  0.0,
        0.0, 0.0,  0.0, 1.0
    );

    float cy = cos(rotY);
    float sy = sin(rotY);
    mat4 Ry = mat4(
        cy,  0.0, -sy, 0.0,
        0.0, 1.0,  0.0, 0.0,
        sy,  0.0,  cy,  0.0,
        0.0, 0.0,  0.0, 1.0
    );

    mat4 T = mat4(
        1.0,   0.0,   0.0,   0.0,
        0.0,   1.0,   0.0,   0.0,
        0.0,   0.0,   1.0,   0.0,
        pos.x, pos.y, pos.z, 1.0
    );

    return T * Ry * Rx * S;
}

void main() 
{
    vs_out_col = (m_isInstanced == 1) ? iColor : vec3(0.5, 0.5, 0.88);
    vs_out_texCoord = aTexCoord;

    mat4 model;
    if (m_isInstanced == 1) {
        float rotX = aRot.x * PI;
        float rotY = aRot.y * PI;
        model = buildModelMatrix(iPosition, iScale, rotX, rotY);
    } else {
        model = m_model;
    }
    
    mat3 invTranspose = transpose(inverse(mat3(model)));
    vs_out_normal = invTranspose * aNormal;
    
    gl_Position = m_viewProj * model * vec4(aPosition, 1.0);
}
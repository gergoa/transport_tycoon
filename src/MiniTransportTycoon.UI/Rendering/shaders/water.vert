#version 460 core
layout (location = 0) in vec3 aPos;

// Instancing attributes
layout (location = 3) in vec3 aInstancePos;
layout (location = 4) in float aScale;
layout (location = 5) in vec2 aRot;
layout (location = 6) in vec3 aColor;

uniform mat4 m_viewProj;
uniform float m_elapsedTime;

out vec3 vs_out_col;

const float PI = 3.14159265359;

void main()
{
    float rotX = aRot.x * PI;
    float rotY = aRot.y * PI;
    
    float cx = cos(rotX);
    float sx = sin(rotX);
    mat3 rotXMat = mat3(
        1.0, 0.0, 0.0,
        0.0, cx,  sx,
        0.0, -sx, cx
    );
    
    float cy = cos(rotY);
    float sy = sin(rotY);
    mat3 rotYMat = mat3(
        cy,  0.0, -sy,
        0.0, 1.0,  0.0,
        sy,  0.0,  cy
    );
    
    vec3 worldPos = (rotYMat * rotXMat * (aPos * aScale)) + aInstancePos;

    // Parametric wave displacement
    float wave1 = sin(worldPos.x * 2.0 + m_elapsedTime * 1.5) * 0.05;
    float wave2 = cos(worldPos.z * 2.5 + m_elapsedTime * 1.2) * 0.05;
    worldPos.y += wave1 + wave2;

    gl_Position = m_viewProj * vec4(worldPos, 1.0);
    vs_out_col = aColor;
}
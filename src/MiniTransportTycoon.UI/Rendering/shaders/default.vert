#version 460 core

layout (location = 0) in vec3 aPosition;
// location 1 is Normal (unused)
// location 2 is TexCoord (unused)

uniform mat4 mvp;

void main() 
{
    gl_Position = mvp * vec4(aPosition, 1.0);
}
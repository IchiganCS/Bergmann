#version 330 core

in vec3 position;
in vec2 texCoord;

out vec4 fragColor;

void main() 
{
    fragColor = vec4(position / 128.0, 1.0);
}
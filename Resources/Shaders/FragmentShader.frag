#version 330 core

in vec3 pos;
in vec2 texCoord;

out vec4 outColor;

void main() 
{
    outColor = vec4(pos / 16.0, 1.0);
    //outColor = vec4(texCoord, 1.0, 1.0);
}
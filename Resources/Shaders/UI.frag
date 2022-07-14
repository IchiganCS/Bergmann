#version 330 core

in vec3 texCoord;

uniform sampler2DArray text;

out vec4 fragColor;

void main() 
{
    vec4 texColor = texture(text, texCoord);
    fragColor = texColor;
}
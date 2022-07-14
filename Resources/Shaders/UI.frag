#version 330 core

in vec2 texCoord;

uniform sampler2D text;

out vec4 fragColor;

void main() 
{
    vec4 texColor = texture(text, texCoord);
    fragColor = texColor;
}
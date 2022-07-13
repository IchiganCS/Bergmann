#version 330 core

in vec2 texCoord;

uniform sampler2D atlas;

out vec4 fragColor;

void main() 
{
    //fragColor = vec4(1.0, 1.0, 1.0, 1.0);
    fragColor = texture(atlas, texCoord);
}
#version 330 core

//position is in global space!
in vec3 position;
in vec3 texCoord;

uniform sampler2DArray stack;

out vec4 fragColor;

void main() 
{
    float d = clamp(length(position) / 16.0, 0, 1);

    fragColor = texture(stack, texCoord) * max(1 - d, 0.13);
}
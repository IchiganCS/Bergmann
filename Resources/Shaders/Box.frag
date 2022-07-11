#version 330 core

//position is in global space!
in vec3 position;
in vec2 texCoord;

uniform sampler2D atlas;

out vec4 fragColor;

void main() 
{
    float d = clamp(length(position) / 16.0, 0, 1);

    fragColor = texture(atlas, texCoord) * max(1 - d, 0.13);
}
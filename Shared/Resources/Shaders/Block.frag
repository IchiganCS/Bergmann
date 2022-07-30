#version 330 core

//position is in camera space!
in vec3 position;
in vec3 texCoord;

uniform sampler2DArray stack;

out vec4 fragColor;

void main() 
{
    float d = clamp(length(position) / 16.0, 0, 0.55);
    vec4 texColor = texture(stack, texCoord);
    fragColor = texColor * (1 - d);
    fragColor.a = texColor.a; //don't reduce alpha value, otherwise transparency could occur
}
#version 330 core

in vec3 texCoord;

uniform sampler2DArray textStack;
uniform sampler2D textureUni;
uniform bool useStack;

out vec4 fragColor;

void main() 
{
    if (useStack) {
        fragColor = texture(textStack, texCoord);
    }
    else {
        fragColor = texture(textureUni, texCoord.xy);
    }

    if (fragColor.a < 0.01)
        discard;
}
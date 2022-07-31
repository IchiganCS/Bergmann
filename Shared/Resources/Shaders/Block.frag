#version 330 core

//position is in camera space!
in vec3 position;
in vec3 texCoord;
in vec3 normal;

uniform sampler2DArray stack;

out vec4 fragColor;

const vec3 sunColor = vec3(0.7);
const vec3 sunDirection = -vec3(0.4, -0.5, 0);

const vec3 ambientMult = vec3(0.1);

void main() 
{
    //don't reduce alpha value, otherwise transparency could occur
    vec4 texColor = texture(stack, texCoord);
    vec3 rgbColor = texColor.rgb;

    vec3 sunMult = sunColor * clamp(dot(normal, normalize(sunDirection)), 0, 1);
    

    fragColor.rgb = rgbColor * (ambientMult + sunMult);
    fragColor.a = texColor.a;
}
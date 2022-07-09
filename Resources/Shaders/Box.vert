#version 330 core

//This has to be with an origin same as the world center
in vec3 position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

//transform matrices to apply to the front to get all other sides
uniform mat4 models[6];

//a new origin for a block
uniform blockPositions {
    vec3 blocks[4096];
};

out vec3 pos;
out vec2 texCoord;

void main() {

    int posIdx = gl_InstanceID / 6;
    int modelIdx = gl_InstanceID % 6;

    texCoord = position.xy;

    vec4 currentSide = models[modelIdx] * vec4(position, 1.0);
    vec4 reoriginedSide = currentSide + vec4(blocks[posIdx], 0.0);
    vec4 transformedSide = model * reoriginedSide;
    

    pos = transformedSide.xyz;
    gl_Position = projection * view * transformedSide;
}
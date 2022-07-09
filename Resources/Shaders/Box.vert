#version 330 core

//already in world space!
layout(location=0) in vec3 inPosition;
layout(location=1) in vec2 inTexCoord;

uniform mat4 pvm;

out vec3 position;
out vec2 texCoord;

void main() {
    position = inPosition;
    texCoord = inTexCoord;

    gl_Position = pvm * vec4(inPosition, 1.0);
}
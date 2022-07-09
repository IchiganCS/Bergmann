#version 330 core

//already in world space!
layout(location=0) in vec3 inPosition;
layout(location=3) in vec2 inTexCoord;

uniform mat4 proj;
uniform mat4 view;

out vec3 position;
out vec2 texCoord;

void main() {
    position = inPosition;
    texCoord = inTexCoord;

    gl_Position = (proj * view) * vec4(inPosition, 1.0);
}
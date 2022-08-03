#version 330 core

//already in world space!
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inTexCoord;
layout(location = 2) in vec3 inNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 position;
out vec3 texCoord;
out vec3 normal;

void main() {
    position = (view * model * vec4(inPosition, 1.0)).xyz;
    texCoord = inTexCoord;
    normal = inNormal;

    gl_Position = projection * vec4(position, 1.0);
}
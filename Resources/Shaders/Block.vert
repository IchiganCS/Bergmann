#version 330 core

//already in world space!
layout(location=0) in vec3 inPosition;
layout(location=1) in vec3 inTexCoord;

uniform mat4 view;
uniform mat4 projection;

out vec3 position;
out vec3 texCoord;

void main() {
    position = (view * vec4(inPosition, 1.0)).xyz;
    texCoord = inTexCoord;

    gl_Position = projection * vec4(position, 1.0);
}
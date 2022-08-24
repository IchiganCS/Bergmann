#version 330 core

layout(location=0) in vec2 percent;
layout(location=1) in vec2 absolute;
layout(location=2) in vec3 inTexCoord;

uniform ivec2 windowsize;

out vec3 texCoord;

void main() {
    texCoord = inTexCoord;

    vec2 position = (percent * 2 - 1.0)
        + absolute / windowsize;

    gl_Position = vec4(position, -1.0, 1.0);
}
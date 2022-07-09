#version 330 core
in vec3 position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 pos;

void main()
{
    gl_Position = projection * view * model * vec4(position, 1.0);
    pos = (model * vec4(position, 1.0)).xyz;
}
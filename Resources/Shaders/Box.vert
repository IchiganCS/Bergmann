#version 330 core

in vec3 position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform mat4 models[6];

out vec3 pos;
out vec2 texCoord;

void main()
{
    texCoord = position.xy;
    
    vec4 instancePosition = models[gl_InstanceID] * vec4(position, 1.0);
    pos = (model * instancePosition).xyz;
    gl_Position = projection * view * model * instancePosition;
}
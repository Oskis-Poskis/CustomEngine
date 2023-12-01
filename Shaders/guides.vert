#version 330 core
layout(location = 0) in vec2 aPosition;

out vec2 uvs;
uniform vec2 offset;
uniform float zoom = 0.5;
uniform float aspect = 1.0; 

void main()
{
    vec2 pos = aPosition;
    pos.x /= aspect;
    pos *= zoom;

    gl_Position = vec4((pos + offset), 0.0, 1.0);
    uvs = aPosition * 0.5 + 1.0;
}

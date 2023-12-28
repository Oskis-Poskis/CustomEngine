#version 330 core
out vec3 color;

in vec2 UVs;
in vec3 normals;
in vec3 fragPos;

uniform vec3 viewPos;

void main()
{ 
    vec3 v_pos = viewPos;
    color = vec3(1.0, 0.0, 0.0);
}
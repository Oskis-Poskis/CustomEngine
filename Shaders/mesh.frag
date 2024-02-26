#version 330 core
out vec3 color;

in vec2 UVs;
in vec3 normals;
in vec3 fragPos;

uniform vec3 viewDir;

void main()
{ 
    vec3 lightDir = normalize(vec3(1.0));

    vec3 norm = normalize(normals);
    float diff = max(dot(norm, -viewDir), 0.0);
    color = vec3(diff);
    
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2)); 
}
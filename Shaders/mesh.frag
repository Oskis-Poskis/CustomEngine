#version 330 core
layout (location = 0) out vec3 color;
layout (location = 1) out vec3 screenspace_normal;

in vec3 normals;
in vec3 fragPos;

uniform vec3 viewDir;
uniform mat4 view;

void main()
{ 
    vec3 lightDir = normalize(vec3(1.0));
    vec3 n = normalize(normals);
    screenspace_normal = (vec4(n, 1.0) * view).xyz;
    float depth = length(gl_FragCoord.z);

	vec3 dx = dFdx(n);
	vec3 dy = dFdy(n);
	vec3 xneg = n - dx;
	vec3 xpos = n + dx;
	vec3 yneg = n - dy;
	vec3 ypos = n + dy;
	float curvature = (cross(xneg, xpos).y - cross(yneg, ypos).x) * 4.0 / (depth);

    float dirt = clamp(0.25 - curvature * 1.0, 0.0, 1.0);
    vec3 ambient = vec3(0.04); // vec3(0.1, 0.05, 0.0);
    vec3 diffuse = mix(vec3(0.5), vec3(0.8), curvature) - ambient;
    vec3 specular = mix(vec3(0.15) - ambient, vec3(0.0), curvature);
    float shininess = 32.0;

    float cosAngle = max(dot(n, -viewDir), 0.0);
    color = ambient + diffuse * max(0.0, cosAngle) + specular * pow(max(0.0, cosAngle), shininess);
    color = normals;
}
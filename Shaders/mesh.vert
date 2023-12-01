#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormals;
layout(location = 2) in vec2 aUVs;
layout(location = 3) in vec3 aTangents;
layout(location = 4) in vec3 aBiTangents;

out vec3 normals;
out vec2 uvs;
out vec3 fragPos;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 model;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    
    uvs = aUVs;
    normals = aNormals * mat3(transpose(inverse(model)));
    fragPos = (vec4(aPosition, 1.0) * model).xyz;
}
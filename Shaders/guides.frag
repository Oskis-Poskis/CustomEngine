#version 330 core
out vec4 FragColor;

in vec2 uvs;

uniform float aspect;

void main()
{  
    float squareSize = 0.025;

    int xCheck = int(mod(uvs.x / squareSize + squareSize, 2.0));
    int yCheck = int(mod(uvs.y / squareSize + squareSize, 2.0));

    vec3 color = (xCheck + yCheck) % 2 == 0 ? vec3(1.0) : vec3(0.0);
    if (color == vec3(0.0)) discard;

    FragColor = vec4(color, 1.0);
}

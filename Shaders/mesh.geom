#version 330 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

uniform mat4 projection;

void main()
{
  gl_Position = gl_in[0].gl_Position * projection;
  EmitVertex();

  gl_Position = gl_in[1].gl_Position * projection;
  EmitVertex();

  gl_Position = gl_in[2].gl_Position * projection;
  EmitVertex();

  EndPrimitive();
}

#version 300 es
#ifdef GL_ES
precision highp float;
#endif

out vec4 FragColor;
uniform vec3 color;

void main()
{
    FragColor = vec4(color, 1.0);
}
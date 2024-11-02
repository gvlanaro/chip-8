#version 300 es
#ifdef GL_ES
precision highp float;
#endif

layout(location = 0) in vec3 aPosition;

uniform mat4 projection;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * projection;
}
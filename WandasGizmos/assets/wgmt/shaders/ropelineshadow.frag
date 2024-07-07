#version 330 core

uniform sampler2D tex2d;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outGlow;

in vec2 uvIn;
in vec4 colorIn;

in float fogAmount;
in vec4 rgbaFog;

#include vertexflagbits.ash
#include fogandlight.fsh

void main() {
    vec4 texColor = texture(tex2d, uvIn) * colorIn;
    outColor = texColor;
    outColor = applyFogAndShadow(texColor, fogAmount);
    outGlow = vec4(0);
}
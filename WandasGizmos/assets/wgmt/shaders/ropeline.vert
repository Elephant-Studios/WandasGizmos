#version 330 core

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uv;

uniform vec3 offset;
uniform float droop;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

out vec2 uvIn;
out vec4 colorIn;

uniform vec3 color;

uniform vec3 rgbaAmbientIn;
uniform vec4 rgbaLightIn;
uniform vec4 rgbaFogIn;
uniform float fogMinIn;
uniform float fogDensityIn;

out float fogAmount;
out vec4 rgbaFog;

#include vertexflagbits.ash
#include shadowcoords.vsh
#include fogandlight.vsh

float catenary(float x, float d, float a) {
    return a * (cosh((x - (d / 2.0)) / a) - cosh((d / 2.0) / a));
}

vec3 calcPoint(float progress, vec3 vertex) {
    float cat = catenary(progress, 1.0, 0.4);

    //y - cat * 1.0,
    return vec3(vertex.x + offset.x * progress, vertex.y + offset.y * progress + cat * droop, vertex.z + offset.z * progress);
}

mat3 rotateToVector(vec3 targetVector) {
    vec3 zAxis = normalize(targetVector);
    vec3 xAxis = normalize(cross(vec3(0.0, 1.0, 0.0), zAxis));
    vec3 yAxis = cross(zAxis, xAxis);

    return mat3(xAxis, yAxis, zAxis);
}

// Standard shader with vertices, uv, and rgba for xray and lines.
void main() {
    vec3 pointA = calcPoint(uv.x - 0.1, vertexIn);
    vec3 pointB = calcPoint(uv.x + 0.1, vertexIn);

    vec3 normal = normalize(pointB - pointA);

    mat3 rotMatrix = rotateToVector(normal);

    vec3 rotatedPoint = rotMatrix * vertexIn;

    vec3 pointMid = calcPoint(uv.x, rotatedPoint);

    vec4 worldPos = modelMatrix * vec4(pointMid, 1.0);
    vec4 cameraPos = viewMatrix * worldPos;
    gl_Position = projectionMatrix * cameraPos;

    // Shadows.
    calcShadowMapCoords(viewMatrix, worldPos);
    fogAmount = getFogLevel(worldPos, fogMinIn, fogDensityIn);

    uvIn = uv;
    uvIn.x *= length(offset);

    colorIn = applyLight(rgbaAmbientIn, rgbaLightIn, 0, cameraPos);
    colorIn.rgb *= color;

    rgbaFog = rgbaFogIn;
}
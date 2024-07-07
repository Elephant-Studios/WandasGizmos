#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;

// Rgb = block light, a = sun light level. Not needed?
//layout(location = 2) in vec4 rgbaLightIn;
//layout(location = 3) in int renderFlagsIn;

uniform vec3 origin = vec3(0);
uniform mat4 mvpMatrix;
uniform vec3 offset;
uniform float droop;

#include vertexflagbits.ash
#include vertexwarp.vsh

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

void main(void) {
	vec3 pointA = calcPoint(uvIn.x - 0.1, vertexIn);
	vec3 pointB = calcPoint(uvIn.x + 0.1, vertexIn);

	vec3 normal = normalize(pointB - pointA);

	mat3 rotMatrix = rotateToVector(normal);

	vec3 rotatedPoint = rotMatrix * vertexIn;

	vec3 pointMid = calcPoint(uvIn.x, rotatedPoint);

	vec4 worldPos = vec4(pointMid + origin, 1.0);

	//worldPos = applyVertexWarping(renderFlagsIn, worldPos);
	//worldPos = applyGlobalWarping(worldPos);

	gl_Position = mvpMatrix * worldPos;
}
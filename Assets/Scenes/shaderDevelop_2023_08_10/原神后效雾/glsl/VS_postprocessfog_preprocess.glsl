#version 450

#define _23__m0 _23._m0
#define _23__m1 _23._m1
#define _23__m2 _23._m2



struct _27
{
    float _m0;
    float _m1;
    float _m2;
    float _m3;
};

layout(set = 1, binding = 0, std140) uniform _21_23
{
    vec4 _m0;
    vec4 _m1[4];
    vec4 _m2[4];
} _23;

layout(set = 0, binding = 0, std140) uniform _28_30
{
    vec4 _m0;
    uint _m1;
    uint _m2;
    int _m3;
    int _m4;
    ivec4 _m5;
    uvec4 _m6;
    _27 _m7;
} _30;

layout(location = 0) in vec4 Vertex_Position;
layout(location = 1) in vec3 Vertex_1;
layout(location = 0) out vec3 Varying_0;
layout(location = 1) out vec2 Varying_1;
vec4 _14;
vec4 _15;
float _17;
vec4 _32;

void main()
{
    _14 = Vertex_Position.yyyy * _23__m1[1u];
    _14 = (_23__m1[0u] * Vertex_Position.xxxx) + _14;
    _14 = (_23__m1[2u] * Vertex_Position.zzzz) + _14;
    _14 += _23__m1[3u];
    _15 = _14.yyyy * _23__m2[1u];
    _15 = (_23__m2[0u] * _14.xxxx) + _15;
    _15 = (_23__m2[2u] * _14.zzzz) + _15;
    _14 = (_23__m2[3u] * _14.wwww) + _15;
    gl_Position = _14;
    _17 = _14.y * _23__m0.x;
    vec2 _108 = _14.xw * vec2(0.5);
    _14 = vec4(_108.x, _14.y, _108.y, _14.w);
    _14.w = _17 * 0.5;
    Varying_1 = _14.zz + _14.xw;
    Varying_0 = Vertex_1;
}


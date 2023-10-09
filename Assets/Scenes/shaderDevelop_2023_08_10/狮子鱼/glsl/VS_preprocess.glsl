#version 450

#define _29__m0 _29._m0
#define _29__m1 _29._m1
#define _29__m2 _29._m2



struct _33
{
    float _m0;
    float _m1;
    float _m2;
    float _m3;
};

layout(set = 1, binding = 0, std140) uniform _27_29
{
    vec4 _m0[4];
    vec4 _m1[4];
    vec4 _m2[4];
} _29;

layout(set = 0, binding = 0, std140) uniform _34_36
{
    vec4 _m0;
    uint _m1;
    uint _m2;
    int _m3;
    int _m4;
    ivec4 _m5;
    uvec4 _m6;
    _33 _m7;
} _36;

layout(location = 0) in vec4 Vertex_Position;
layout(location = 1) in vec3 Vertex_1;
layout(location = 2) in vec2 Vertex_2;
layout(location = 3) in vec2 Vertex_3;
layout(location = 0) out vec4 Varying_0;
layout(location = 1) out vec3 Varying_1;
layout(location = 2) out vec2 Varying_2;
layout(location = 3) out vec2 Varying_3;
vec4 _20;
vec4 _21;
float _23;
vec4 _37;

void main()
{
    _20 = Vertex_Position.yyyy * _29__m0[1u];
    _20 = (_29__m0[0u] * Vertex_Position.xxxx) + _20;
    _20 = (_29__m0[2u] * Vertex_Position.zzzz) + _20;
    _21 = _20 + _29__m0[3u];
    Varying_0 = (_29__m0[3u] * Vertex_Position.wwww) + _20;
    _20 = _21.yyyy * _29__m2[1u];
    _20 = (_29__m2[0u] * _21.xxxx) + _20;
    _20 = (_29__m2[2u] * _21.zzzz) + _20;
    gl_Position = (_29__m2[3u] * _21.wwww) + _20;
    Varying_2 = Vertex_2;
    Varying_3 = Vertex_3;
    _20.x = dot(Vertex_1, _29__m1[0u].xyz);
    _20.y = dot(Vertex_1, _29__m1[1u].xyz);
    _20.z = dot(Vertex_1, _29__m1[2u].xyz);
    _23 = dot(_20.xyz, _20.xyz);
    _23 = inversesqrt(_23);
    Varying_1 = vec3(_23) * _20.xyz;
}


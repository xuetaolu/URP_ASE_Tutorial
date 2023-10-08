#version 450

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

layout(location = 0) in vec4 _4;
layout(location = 1) in vec3 _7;
layout(location = 2) in vec2 _10;
layout(location = 3) in vec2 _11;
layout(location = 2) out vec2 _13;
layout(location = 3) out vec2 _14;
layout(location = 0) out vec4 _16;
layout(location = 1) out vec3 _18;
vec4 _20;
vec4 _21;
float _23;
vec4 _37;

void main()
{
    _20 = _4.yyyy * _29._m0[1u];
    _20 = (_29._m0[0u] * _4.xxxx) + _20;
    _20 = (_29._m0[2u] * _4.zzzz) + _20;
    _21 = _20 + _29._m0[3u];
    _16 = (_29._m0[3u] * _4.wwww) + _20;
    _20 = _21.yyyy * _29._m2[1u];
    _20 = (_29._m2[0u] * _21.xxxx) + _20;
    _20 = (_29._m2[2u] * _21.zzzz) + _20;
    gl_Position = (_29._m2[3u] * _21.wwww) + _20;
    _13 = _10;
    _14 = _11;
    _20.x = dot(_7, _29._m1[0u].xyz);
    _20.y = dot(_7, _29._m1[1u].xyz);
    _20.z = dot(_7, _29._m1[2u].xyz);
    _23 = dot(_20.xyz, _20.xyz);
    _23 = inversesqrt(_23);
    _18 = vec3(_23) * _20.xyz;
}


#version 450

struct _6
{
    vec4 _m0[4];
    vec4 _m1[4];
};

struct _45
{
    float _m0;
    float _m1;
    float _m2;
    float _m3;
};

layout(set = 3, binding = 0, std140) uniform _9_11
{
    _6 _m0[32];
} _11;

layout(set = 1, binding = 0, std140) uniform _40_42
{
    vec4 _m0;
    vec4 _m1;
    vec4 _m2;
    vec4 _m3;
    vec4 _m4;
    vec4 _m5;
    vec4 _m6;
    vec4 _m7[4];
    int _m8;
    vec4 _m9;
} _42;

layout(set = 0, binding = 0, std140) uniform _46_48
{
    vec4 _m0;
    uint _m1;
    uint _m2;
    int _m3;
    int _m4;
    ivec4 _m5;
    uvec4 _m6;
    _45 _m7;
} _48;

layout(location = 0) in vec4 _13;
layout(location = 1) in vec3 _16;
layout(location = 2) in vec4 _17;
layout(location = 0) out vec2 _20;
vec3 _22;
vec3 _23;
vec3 _24;
vec4 _26;
uint _28;
vec4 _30;
int _33;
vec4 _34;
vec4 _35;
vec4 _36;
vec3 _38;
vec3 _39;
vec4 _49;

void main()
{
    _33 = gl_InstanceIndex + _42._m8;
    _33 = _33 << 3;
    _34 = _13.yyyy * _11._m0[_33 / 8]._m0[1u];
    _34 = (_11._m0[_33 / 8]._m0[0u] * _13.xxxx) + _34;
    _34 = (_11._m0[_33 / 8]._m0[2u] * _13.zzzz) + _34;
    _36 = _34 + _11._m0[_33 / 8]._m0[3u];
    _23 = (_11._m0[_33 / 8]._m0[3u].xyz * _13.www) + _34.xyz;
    _34 = _36.yyyy * _42._m7[1u];
    _34 = (_42._m7[0u] * _36.xxxx) + _34;
    _34 = (_42._m7[2u] * _36.zzzz) + _34;
    gl_Position = (_42._m7[3u] * _36.wwww) + _34;
    _20 = (_17.xy * _42._m9.xy) + _42._m9.zw;
    _34.x = dot(_16, _11._m0[_33 / 8]._m1[0u].xyz);
    _34.y = dot(_16, _11._m0[_33 / 8]._m1[1u].xyz);
    _34.z = dot(_16, _11._m0[_33 / 8]._m1[2u].xyz);
    _30.x = dot(_34.xyz, _34.xyz);
    _30.x = inversesqrt(_30.x);
    vec3 _196 = _30.xxx * _34.xyz;
    _30 = vec4(_196.x, _196.y, _196.z, _30.w);
    _22 = _30.xyz;
    _38.x = _30.y * _30.y;
    _38.x = (_30.x * _30.x) + (-_38.x);
    _35 = _30.yzzx * _30.xyzz;
    _39.x = dot(_42._m3, _35);
    _39.y = dot(_42._m4, _35);
    _39.z = dot(_42._m5, _35);
    _38 = (_42._m6.xyz * _38.xxx) + _39;
    _30.w = 1.0;
    _39.x = dot(_42._m0, _30);
    _39.y = dot(_42._m1, _30);
    _39.z = dot(_42._m2, _30);
    _24 = _38 + _39;
    _26 = vec4(0.0);
    _28 = uint(gl_InstanceIndex);
}


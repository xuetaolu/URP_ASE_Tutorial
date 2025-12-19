#version 450

#define _11__m0__0 vec4[] {
    4849.17432, -1206.20605, -3321.23096, 0.00 ,
    -3361.22192, -3314.02588, -3703.97461, 0.00,
    -1089.81433, 4854.10254, -3354.10229, 0.00 ,
    -1.82556, 500.00, 3.505, 1.00              ,
} // _11._m0


#define _42__m0 vec4(0.00, 0.08546, 0.00, 0.20387      ) //_42._m0
#define _42__m1 vec4(0.00, 0.18747, 0.00, 0.36436      ) //_42._m1
#define _42__m2 vec4(0.00, 0.27045, 0.00, 0.60278      ) //_42._m2
#define _42__m3 vec4(0.00, 0.00, -0.08331, 0.00        ) //_42._m3
#define _42__m4 vec4(0.00, 0.00, -0.11274, 0.00        ) //_42._m4
#define _42__m5 vec4(0.00, 0.00, -0.12677, 0.00        ) //_42._m5
#define _42__m6 vec4(-0.08331, -0.11274, -0.12677, 1.00) //_42._m6
#define _42__m7 vec4[] {
    -1.32406, -0.0001, 0.22249, 0.22247  ,
    4.28463E-10, 2.41421, 0.00, 0.00     ,
    -0.30169, 0.00045, -0.97502, -0.97494,
    -1.35973, -1207.10852, 3.3236, 3.8233,
} // _42._m7
#define _42__m8 0 //_42._m8
#define _42__m9 vec4(1.00, 1.00, 0.00, 0.00)           // _42._m9



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

layout(location = 0) in vec4 Vertex_Position;
layout(location = 1) in vec3 Vertex_1;
layout(location = 2) in vec4 Vertex_2;
layout(location = 0) out vec2 Varying_0;
vec3 _22;
vec3 _23;
vec3 _24;
vec4 _26;
// uint _28;
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
    _33 = 0 + 0;
    _33 = _33 << 3;
    _34 = Vertex_Position.yyyy * _11__m0__0._m0[1u];
    _34 = (_11__m0__0._m0[0u] * Vertex_Position.xxxx) + _34;
    _34 = (_11__m0__0._m0[2u] * Vertex_Position.zzzz) + _34;
    _36 = _34 + _11__m0__0._m0[3u];
    _23 = (_11__m0__0._m0[3u].xyz * Vertex_Position.www) + _34.xyz;
    _34 = _36.yyyy * _42__m7[1u];
    _34 = (_42__m7[0u] * _36.xxxx) + _34;
    _34 = (_42__m7[2u] * _36.zzzz) + _34;
    gl_Position = (_42__m7[3u] * _36.wwww) + _34;
    Varying_0 = (Vertex_2.xy * _42__m9.xy) + _42__m9.zw;
    _34.x = dot(Vertex_1, _11__m0__0._m1[0u].xyz);
    _34.y = dot(Vertex_1, _11__m0__0._m1[1u].xyz);
    _34.z = dot(Vertex_1, _11__m0__0._m1[2u].xyz);
    _30.x = dot(_34.xyz, _34.xyz);
    _30.x = inversesqrt(_30.x);
    vec3 _196 = _30.xxx * _34.xyz;
    _30 = vec4(_196.x, _196.y, _196.z, _30.w);
    _22 = _30.xyz;
    _38.x = _30.y * _30.y;
    _38.x = (_30.x * _30.x) + (-_38.x);
    _35 = _30.yzzx * _30.xyzz;
    _39.x = dot(_42__m3, _35);
    _39.y = dot(_42__m4, _35);
    _39.z = dot(_42__m5, _35);
    _38 = (_42__m6.xyz * _38.xxx) + _39;
    _30.w = 1.0;
    _39.x = dot(_42__m0, _30);
    _39.y = dot(_42__m1, _30);
    _39.z = dot(_42__m2, _30);
    _24 = _38 + _39;
    _26 = vec4(0.0);
    // _28 = uint(0);
}


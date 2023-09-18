#version 450

struct _3
{
    vec4 _m0;
    vec4 _m1;
};

struct _56
{
    float _m0;
    float _m1;
    float _m2;
    float _m3;
};

layout(set = 3, binding = 0, std140) uniform _7_9
{
    _3 _m0[32];
} _9;

layout(set = 1, binding = 0, std140) uniform _51_53
{
    vec4 _m0;
    vec4 _m1[4];
    int _m2;
    vec4 _m3[4];
    vec4 _m4[4];
    vec4 _m5;
    vec4 _m6;
} _53;

layout(set = 0, binding = 0, std140) uniform _57_59
{
    vec4 _m0;
    uint _m1;
    uint _m2;
    int _m3;
    int _m4;
    ivec4 _m5;
    uvec4 _m6;
    _56 _m7;
} _59;

layout(set = 2, binding = 0) uniform sampler2D _13;
layout(set = 2, binding = 1) uniform sampler2D _14;

layout(location = 0) in vec4 _16;
layout(location = 1) in vec4 _17;
layout(location = 0) out vec4 _19;
layout(location = 1) out vec4 _20;
layout(location = 2) out vec4 _21;
layout(location = 3) out vec4 _22;
vec4 _23;
uint _25;
vec4 _27;
bvec4 _31;
vec3 _34;
int _37;
vec4 _38;
vec4 _39;
vec4 _40;
vec3 _41;
vec3 _42;
vec2 _45;
vec2 _46;
float _48;
vec4 _60;

void main()
{
    _27 = _17.zzzz * vec4(0.5, 0.25, 0.125, 0.0625);
    _27 = fract(_27);
    _31 = greaterThanEqual(_27, vec4(0.5));
    _27 = mix(vec4(0.0), vec4(1.0), vec4(_31));
    _37 = gl_InstanceIndex + _53._m2;
    _37 = _37 << 1;
    _27 *= _9._m0[_37 / 2]._m1;
    _27 *= vec4(0.5);
    _27 = fract(_27);
    _31 = greaterThanEqual(_27, vec4(0.5));
    _31.x = _31.z || _31.x;
    _31.y = _31.w || _31.y;
    _31.x = _31.y || _31.x;
    _27.x = float(_31.x);
    vec2 _149 = (_17.xy * _27.xx) + _16.xz;
    _27 = vec4(_149.x, _149.y, _27.z, _27.w);
    vec2 _159 = _27.xy + _9._m0[_37 / 2]._m0.xy;
    _27 = vec4(_159.x, _159.y, _27.z, _27.w);
    _45 = (_27.xy * _9._m0[_37 / 2]._m0.zz) + (-_53._m6.xy);
    vec2 _183 = _27.xy * _9._m0[_37 / 2]._m0.zz;
    _27 = vec4(_183.x, _183.y, _27.z, _27.w);
    _45 += vec2(0.5);
    _45 = vec2(_45.x / _53._m6.z, _45.y / _53._m6.w);
    _34.x = textureLod(_13, _45, 0.0).x;
    _45 = textureLod(_14, _45, 0.0).xy;
    _45 = (_45 * vec2(4.0)) + vec2(-2.0);
    _38 = _34.xxxx * _53._m3[1u];
    _34 = _34.xxx * _53._m3[1u].xyz;
    vec2 _239 = vec2(_27.x * _53._m5.z, _27.y * _53._m5.w);
    _39 = vec4(_239.x, _239.y, _39.z, _39.w);
    _38 = (_53._m3[0u] * _39.xxxx) + _38;
    _38 = (_53._m3[2u] * _39.yyyy) + _38;
    _38 += _53._m3[3u];
    _40 = _38.yyyy * _53._m1[1u];
    _40 = (_53._m1[0u] * _38.xxxx) + _40;
    _40 = (_53._m1[2u] * _38.zzzz) + _40;
    _38 = (_53._m1[3u] * _38.wwww) + _40;
    gl_Position = _38;
    _46 = _53._m5.xy + vec2(-1.0);
    vec2 _297 = _27.xy / _46;
    _19 = vec4(_297.x, _297.y, _19.z, _19.w);
    _19 = vec4(_19.x, _19.y, vec2(0.0).x, vec2(0.0).y);
    _34 = (_53._m3[0u].xyz * _39.xxx) + _34;
    _34 = (_53._m3[2u].xyz * _39.yyy) + _34;
    _34 += _53._m3[3u].xyz;
    _20.w = _34.x;
    _27.x = dot(_45, _45);
    vec2 _338 = ((-_27.xx) * vec2(0.25, 0.5)) + vec2(1.0);
    _39 = vec4(_39.x, _39.y, _338.x, _338.y);
    _27.x = sqrt(_39.z);
    vec2 _348 = _27.xx * _45;
    _39 = vec4(_348.x, _348.y, _39.z, _39.w);
    _27.y = dot(_39.xyw, _53._m4[0u].xyz);
    _27.z = dot(_39.xyw, _53._m4[1u].xyz);
    _27.x = dot(_39.xyw, _53._m4[2u].xyz);
    _48 = dot(_27.xyz, _27.xyz);
    _48 = inversesqrt(_48);
    vec3 _383 = vec3(_48) * _27.xyz;
    _27 = vec4(_383.x, _383.y, _383.z, _27.w);
    _20.z = _27.y;
    _41 = _27.yzx * vec3(1.0, 0.0, 0.0);
    _41 = (_27.xyz * vec3(0.0, 0.0, 1.0)) + (-_41);
    _42 = _27.xyz * _41;
    _42 = (_27.zxy * _41.yzx) + (-_42);
    _20.y = -_42.x;
    _20.x = _41.z;
    _21.x = _41.x;
    _21.z = _27.z;
    _22.z = _27.x;
    _21.w = _34.y;
    _22.w = _34.z;
    _21.y = -_42.y;
    _22.y = -_42.z;
    _22.x = 0.0;
    _27.x = _38.y * _53._m0.x;
    _27.w = _27.x * 0.5;
    vec2 _455 = _38.xw * vec2(0.5);
    _27 = vec4(_455.x, _27.y, _455.y, _27.w);
    _23 = vec4(_23.x, _23.y, _38.zw.x, _38.zw.y);
    vec2 _466 = _27.zz + _27.xw;
    _23 = vec4(_466.x, _466.y, _23.z, _23.w);
    _25 = uint(gl_InstanceIndex);
}


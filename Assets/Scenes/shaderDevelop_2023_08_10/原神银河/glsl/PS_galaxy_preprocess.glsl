#version 450

#define _34__m0 vec4(84.2623, 1685.24597, 3370.49194, 5055.73779)// _34._m0
#define _34__m1 vec4(0.00, 0.74265, 0.03585, 1.00               )// _34._m1
#define _34__m2 vec4(1.00, 0.40, 0.00, 0.00                     )// _34._m2
#define _34__m3 (-0.005) //_34._m3
#define _34__m4 (0.38  ) //_34._m4
#define _34__m5 (10.82 ) //_34._m5
#define _34__m6 vec4(1.00, 1.00, 0.00, 0.00) // _34._m6
#define _34__m7 vec4(3.50, 1.00, 0.00, 0.00) // _34._m7
#define _34__m8 (0.005) // _34._m8
#define _34__m9 (0.23 ) // _34._m9
#define _34__m10 vec4(4.35, 1.10, 0.00, 0.00 ) // _34._m10
#define _34__m11 (-0.05 ) // _34._m11
#define _34__m12 (1.60  ) // _34._m12
#define _34__m13 (700.00) // _34._m13
#define _34__m14 vec4(8.00, 6.00, 0.00, 0.00   ) // _34._m14
#define _34__m15 vec4(1.00, 0.00, 0.00, 1.00   ) // _34._m15
#define _34__m16 vec4(0.14706, 0.40, 1.00, 1.00) // _34._m16
#define _34__m17 (5.00 ) // _34._m17
#define _34__m18 (0.00 ) // _34._m18
#define _34__m19 (18.00) // _34._m19


layout(early_fragment_tests) in;

layout(constant_id = 4) const uint _2 = 0u;

struct _38
{
    float _m0;
    float _m1;
    float _m2;
    float _m3;
};

const float _313[4] = float[](-0.01171875, 0.00390625, 0.01171875, -0.00390625);

layout(set = 1, binding = 1, std140) uniform _32_34
{
    vec4 _m0;
    vec4 _m1;
    vec4 _m2;
    float _m3;
    float _m4;
    float _m5;
    vec4 _m6;
    vec4 _m7;
    float _m8;
    float _m9;
    vec4 _m10;
    float _m11;
    float _m12;
    float _m13;
    vec4 _m14;
    vec4 _m15;
    vec4 _m16;
    float _m17;
    float _m18;
    float _m19;
} _34;

layout(set = 0, binding = 0, std140) uniform _39_41
{
    vec4 _m0;
    uint _m1;
    uint _m2;
    int _m3;
    int _m4;
    ivec4 _m5;
    uvec4 _m6;
    _38 _m7;
} _41;

layout(set = 2, binding = 0) uniform sampler2D _7;
layout(set = 2, binding = 1) uniform sampler2D _8;
layout(set = 2, binding = 2) uniform sampler2D _9;
layout(set = 2, binding = 3) uniform sampler2D _10;
layout(set = 2, binding = 4) uniform sampler2D _11;

layout(location = 0) in vec2 Varying_0;
layout(location = 0) out vec4 Output_0;
vec3 _20;
float _22;
vec3 _23;
vec2 _25;
vec3 _26;
vec3 _27;
vec2 _28;
float _29;
float _30;
float _31;
float _332;
uint _336;
vec3 _342 = vec3(255.0);

void _44()
{
    vec2 _56 = (Varying_0 * _34__m2.xy) + _34__m2.zw;
    _20 = vec3(_56.x, _56.y, _20.z);
    vec2 _71 = (_34__m0.yy * vec2(_34__m3)) + _20.xy;
    _20 = vec3(_71.x, _71.y, _20.z);
    _22 = texture(_10, _20.xy).x;
    _20.x = _22 + _34__m4;
    _20 = _20.xxx * _34__m1.xyz;
    _20 *= vec3(_34__m5, _34__m5, _34__m5);
    vec2 _111 = (Varying_0 * _34__m14.xy) + _34__m14.zw;
    _23 = vec3(_111.x, _111.y, _23.z);
    _31 = texture(_11, _23.xy).x;
    _30 = _31 * _34__m13;
    vec2 _133 = (Varying_0 * _34__m7.xy) + _34__m7.zw;
    _23 = vec3(_133.x, _133.y, _23.z);
    vec2 _146 = (_34__m0.yy * vec2(_34__m8)) + _23.xy;
    _23 = vec3(_146.x, _146.y, _23.z);
    _25.x = texture(_7, _23.xy).x;
    vec2 _164 = (Varying_0 * _34__m6.xy) + _34__m6.zw;
    _27 = vec3(_164.x, _164.y, _27.z);
    vec2 _178 = (_25.xx * vec2(_34__m9, _34__m9)) + _27.xy;
    _23 = vec3(_178.x, _178.y, _23.z);
    _25 = texture(_8, _23.xy).xy;
    _30 *= _25.x;
    _28 = (Varying_0 * _34__m10.xy) + _34__m10.zw;
    _29 = texture(_9, _28).x;
    _28.x = _29 + _34__m11;
    _28.x = (_28.x * _34__m12) + (-1.0);
    _27.x = (_25.y * _28.x) + 1.0;
    _23.x = _27.x * _25.x;
    _27.x = (-_27.x) + 1.0;
    _20 = (_20 * _23.xxx) + vec3(_30);
    _26 = _34__m15.xyz * vec3(_34__m19, _34__m19, _34__m19);
    _27 = _27.xxx * _26;
    _20 = (_27 * _23.xxx) + _20;
    _23 = _23.xxx * _34__m16.xyz;
    _20 = (_23 * vec3(_34__m17)) + _20;
    _20 = (vec3(_34__m18, _34__m18, _34__m18) * (-_20)) + _20;
    Output_0 = vec4(_20.x, _20.y, _20.z, Output_0.w);
    Output_0.w = 1.0;
}

void main()
{
    vec3 _339 = vec3(0.0);
    _44();
    if (_2 != 0u)
    {
        _332 = _313[((uint(gl_FragCoord.x) & 1u) << 1u) | (uint(gl_FragCoord.y) & 1u)];
        _336 = (_2 >> 0u) & 3u;
        switch (_336)
        {
            case 1u:
            {
                _339 = vec3(_332 * 2.0);
                _342 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _339 = vec3(_332);
                _342 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _339 = vec3(_332, _332 * 0.5, _332);
                _342 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _369 = Output_0.xyz + _339;
        Output_0 = vec4(_369.x, _369.y, _369.z, Output_0.w);
        vec3 _379 = round(Output_0.xyz * _342) / _342;
        Output_0 = vec4(_379.x, _379.y, _379.z, Output_0.w);
    }
}


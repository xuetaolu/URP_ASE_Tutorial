#version 450

#define _110__m0 _110._m0
#define _110__m1 _110._m1
#define _110__m2 _110._m2
#define _110__m3 _110._m3
#define _110__m4 _110._m4
#define _110__m5 _110._m5
#define _110__m6 _110._m6
#define _110__m7 _110._m7
#define _110__m8 _110._m8
#define _110__m9 _110._m9
#define _110__m10 _110._m10
#define _110__m11 _110._m11
#define _110__m12 _110._m12
#define _110__m13 _110._m13
#define _110__m14 _110._m14
#define _110__m15 _110._m15
#define _110__m16 _110._m16
#define _110__m17 _110._m17
#define _110__m18 _110._m18
#define _110__m19 _110._m19
#define _110__m20 _110._m20
#define _110__m21 _110._m21
#define _110__m22 _110._m22
#define _110__m23 _110._m23
#define _110__m24 _110._m24
#define _110__m25 _110._m25
#define _110__m26 _110._m26
#define _110__m27 _110._m27
#define _110__m28 _110._m28
#define _110__m29 _110._m29
#define _110__m30 _110._m30
#define _110__m31 _110._m31
#define _110__m32 _110._m32
#define _110__m33 _110._m33
#define _110__m34 _110._m34
#define _110__m35 _110._m35
#define _110__m36 _110._m36
#define _110__m37 _110._m37
#define _110__m38 _110._m38
#define _110__m39 _110._m39
#define _110__m40 _110._m40
#define _110__m41 _110._m41
#define _110__m42 _110._m42


layout(early_fragment_tests) in;

layout(constant_id = 4) const uint _2 = 0u;

struct _114
{
    float _m0;
    float _m1;
    float _m2;
    float _m3;
};

const float _1134[4] = float[](-0.01171875, 0.00390625, 0.01171875, -0.00390625);

layout(set = 1, binding = 1, std140) uniform _108_110
{
    vec4 _m0;
    float _m1;
    vec4 _m2;
    vec4 _m3;
    vec4 _m4;
    vec4 _m5;
    vec4 _m6;
    float _m7;
    float _m8;
    float _m9;
    float _m10;
    float _m11;
    float _m12;
    float _m13;
    vec4 _m14;
    vec4 _m15;
    vec4 _m16;
    vec4 _m17;
    vec4 _m18;
    vec4 _m19;
    vec4 _m20;
    float _m21;
    float _m22;
    float _m23;
    float _m24;
    float _m25;
    float _m26;
    float _m27;
    float _m28;
    float _m29;
    float _m30;
    float _m31;
    float _m32;
    float _m33;
    float _m34;
    float _m35;
    float _m36;
    float _m37;
    float _m38;
    float _m39;
    float _m40;
    float _m41;
    float _m42;
} _110;

layout(set = 0, binding = 0, std140) uniform _115_117
{
    vec4 _m0;
    uint _m1;
    uint _m2;
    int _m3;
    int _m4;
    ivec4 _m5;
    uvec4 _m6;
    _114 _m7;
} _117;

layout(set = 2, binding = 2) uniform sampler2D _7;
layout(set = 2, binding = 3) uniform sampler2D _8;
layout(set = 2, binding = 4) uniform sampler2D _9;
layout(set = 2, binding = 5) uniform sampler2D _10;
layout(set = 2, binding = 6) uniform sampler2D _11;
layout(set = 2, binding = 7) uniform sampler2D _12;
layout(set = 2, binding = 8) uniform sampler2D _13;
layout(set = 2, binding = 9) uniform sampler2D _14;
layout(set = 2, binding = 10) uniform sampler2D _15;
layout(set = 2, binding = 11) uniform sampler2D _16;
layout(set = 2, binding = 12) uniform sampler2D _17;
layout(set = 2, binding = 13) uniform sampler2D _18;

layout(location = 0) in vec4 Varying_0;
layout(location = 1) in vec4 Varying_1;
layout(location = 2) in vec4 Varying_2;
layout(location = 3) in vec4 Varying_3;
layout(location = 0) out vec4 Output_0;
layout(location = 1) out vec4 Output_1;
layout(location = 2) out vec4 Output_2;
vec2 _31;
vec4 _33;
vec4 _34;
vec4 _35;
vec4 _36;
vec4 _37;
vec4 _38;
vec2 _39;
vec4 _40;
bvec4 _44;
vec3 _47;
vec4 _48;
vec2 _49;
vec4 _50;
bool _52;
float _54;
float _55;
vec3 _56;
vec3 _57;
vec3 _58;
vec3 _59;
float _60;
bvec2 _63;
vec3 _64;
vec2 _65;
float _66;
vec3 _67;
vec3 _68;
float _69;
vec4 _70;
float _71;
bvec3 _74;
vec2 _75;
float _76;
bvec2 _77;
vec3 _78;
vec3 _79;
float _80;
vec3 _81;
vec3 _82;
float _83;
float _84;
bool _85;
float _86;
bool _87;
vec3 _88;
bool _89;
float _90;
float _91;
bvec2 _92;
bool _93;
float _94;
float _95;
vec2 _96;
float _97;
float _98;
float _99;
float _100;
float _101;
float _102;
float _103;
float _104;
float _105;
float _106;
float _107;
float _1151;
uint _1155;
vec3 _1161 = vec3(255.0);
uint _1201;
vec3 _1203 = vec3(255.0);
uint _1236;
vec3 _1238 = vec3(255.0);

void _120()
{
    _31.x = Varying_1.w + _110__m0.x;
    _31.y = Varying_3.w + _110__m0.z;
    vec3 _145 = textureLod(_7, Varying_0.xy, 0.0).xyz;
    _37 = vec4(_145.x, _145.y, _145.z, _37.w);
    vec3 _152 = _37.xyz + vec3(9.9999997473787516355514526367188e-05);
    _36 = vec4(_152.x, _152.y, _152.z, _36.w);
    vec3 _161 = _36.xyz * vec3(_110__m21);
    _38 = vec4(_161.x, _161.y, _161.z, _38.w);
    _35 = textureLod(_8, Varying_0.xy, 0.0);
    _44 = lessThan(vec4(0.001000000047497451305389404296875), _35);
    _92 = lessThan(vec4(-1.0), vec4(_110__m29, _110__m30, _110__m29, _110__m30)).xy;
    _92.x = _92.x && _44.x;
    _92.y = _92.y && _44.y;
    if (_92.x)
    {
        _91 = 1.0 / _110__m7;
        _44.x = any(notEqual(vec4(0.0), vec4(_110__m22)));
        _47 = _38.xyz * _110__m14.xyz;
        _47 = mix(_110__m14.xyz, _47, bvec3(_44.x));
        _39 = vec2(_91) * _31;
        _50 = texture(_9, _39);
        _91 = (_35.x * 1.2000000476837158203125) + _50.w;
        _83 = max(_91, 0.0);
        _56 = _47 * _50.xyz;
        _98 = _91;
        _99 = _110__m36;
    }
    else
    {
        _98 = 0.0;
        _83 = 0.0;
        _56.x = 0.0;
        _56.y = 0.0;
        _56.z = 0.0;
        _99 = 0.0;
    }
    if (_92.y)
    {
        _35.x = 1.0 / _110__m8;
        _52 = any(notEqual(vec4(0.0), vec4(_110__m23)));
        _57 = _38.xyz * _110__m15.xyz;
        _57 = mix(_110__m15.xyz, _57, bvec3(_52));
        _49 = _31 * _35.xx;
        _48 = texture(_10, _49);
        _35.x = (_35.y * 1.2000000476837158203125) + _48.w;
        _83 = max(_35.x, _83);
        _58 = _57 * _48.xyz;
        _36.x = _35.x;
        _100 = _110__m37;
    }
    else
    {
        _36.x = 0.0;
        _58.x = 0.0;
        _58.y = 0.0;
        _58.z = 0.0;
        _100 = 0.0;
    }
    _63 = lessThan(vec4(-1.0, -1.0, 0.0, 0.0), vec4(_110__m31, _110__m32, _110__m31, _110__m31)).xy;
    _63.x = _44.z && _63.x;
    _63.y = _44.w && _63.y;
    if (_63.x)
    {
        _59.x = 1.0 / _110__m9;
        _93 = any(notEqual(vec4(0.0), vec4(_110__m24)));
        _64 = _38.xyz * _110__m16.xyz;
        _64 = mix(_110__m16.xyz, _64, bvec3(_93));
        vec2 _382 = _31 * _59.xx;
        _59 = vec3(_382.x, _59.y, _382.y);
        _40 = texture(_11, _59.xz);
        _59.x = (_35.z * 1.2000000476837158203125) + _40.w;
        _83 = max(_83, _59.x);
        _64 *= _40.xyz;
        _60 = _59.x;
        _101 = _110__m38;
    }
    else
    {
        _60 = 0.0;
        _64.x = 0.0;
        _64.y = 0.0;
        _64.z = 0.0;
        _101 = 0.0;
    }
    if (_63.y)
    {
        _65.x = 1.0 / _110__m10;
        _85 = any(notEqual(vec4(0.0), vec4(_110__m25)));
        _67 = _38.xyz * _110__m17.xyz;
        _67 = mix(_110__m17.xyz, _67, bvec3(_85));
        _65 = _31 * _65.xx;
        _40 = texture(_12, _65);
        _65.x = (_35.w * 1.2000000476837158203125) + _40.w;
        _83 = max(_83, _65.x);
        _67 *= _40.xyz;
        _66 = _65.x;
        _102 = _110__m39;
    }
    else
    {
        _66 = 0.0;
        _67.x = 0.0;
        _67.y = 0.0;
        _67.z = 0.0;
        _102 = 0.0;
    }
    _68 = textureLod(_13, Varying_0.xy, 0.0).xyz;
    _74 = lessThan(vec4(0.001000000047497451305389404296875, 0.001000000047497451305389404296875, 0.001000000047497451305389404296875, 0.0), _68.xyzx).xyz;
    _77 = lessThan(vec4(-1.0, -1.0, 0.0, 0.0), vec4(_110__m33, _110__m34, _110__m33, _110__m33)).xy;
    _74.x = _74.x && _77.x;
    _74.y = _74.y && _77.y;
    if (_74.x)
    {
        _103 = 1.0 / _110__m11;
        _74.x = any(notEqual(vec4(0.0), vec4(_110__m26)));
        _78 = _38.xyz * _110__m18.xyz;
        _78 = mix(_110__m18.xyz, _78, bvec3(_74.x));
        vec2 _541 = _31 * vec2(_103);
        _70 = vec4(_541.x, _70.y, _70.z, _541.y);
        _40 = texture(_14, _70.xw);
        _68.x = (_68.x * 1.2000000476837158203125) + _40.w;
        _83 = max(_83, _68.x);
        _78 *= _40.xyz;
        _69 = _68.x;
        _104 = _110__m40;
    }
    else
    {
        _69 = 0.0;
        _78.x = 0.0;
        _78.y = 0.0;
        _78.z = 0.0;
        _104 = 0.0;
    }
    if (_74.y)
    {
        _70.x = 1.0 / _110__m12;
        _87 = any(notEqual(vec4(0.0), vec4(_110__m27)));
        _79 = _38.xyz * _110__m19.xyz;
        _79 = mix(_110__m19.xyz, _79, bvec3(_87));
        vec2 _604 = _31 * _70.xx;
        _70 = vec4(_604.x, _604.y, _70.z, _70.w);
        _40 = texture(_15, _70.xy);
        _70.x = (_68.y * 1.2000000476837158203125) + _40.w;
        _83 = max(_83, _70.x);
        _79 *= _40.xyz;
        _71 = _70.x;
        _105 = _110__m41;
    }
    else
    {
        _71 = 0.0;
        _79.x = 0.0;
        _79.y = 0.0;
        _79.z = 0.0;
        _105 = 0.0;
    }
    _77.x = (-1.0) < _110__m35;
    _77.x = _74.z && _77.x;
    if (_77.x)
    {
        _75.x = 1.0 / _110__m13;
        _89 = any(notEqual(vec4(0.0), vec4(_110__m28)));
        vec3 _667 = _38.xyz * _110__m20.xyz;
        _38 = vec4(_667.x, _667.y, _667.z, _38.w);
        vec3 _677 = mix(_110__m20.xyz, _38.xyz, bvec3(_89));
        _38 = vec4(_677.x, _677.y, _677.z, _38.w);
        _75 = _31 * _75.xx;
        _34 = texture(_16, _75);
        _54 = (_68.z * 1.2000000476837158203125) + _34.w;
        _83 = max(_54, _83);
        vec3 _700 = _38.xyz * _34.xyz;
        _38 = vec4(_700.x, _700.y, _700.z, _38.w);
        _55 = _54;
        _80 = _110__m42;
    }
    else
    {
        _38.x = 0.0;
        _38.y = 0.0;
        _38.z = 0.0;
        _55 = 0.0;
        _80 = 0.0;
    }
    _76 = _83 + (-0.20000000298023223876953125);
    _98 += (-_76);
    _98 = max(_98, 0.0);
    _90 = _36.x + (-_76);
    _90 = max(_90, 0.0);
    _58 *= vec3(_90);
    _58 = (vec3(_98) * _56) + _58;
    _95 = _98 + _90;
    _106 = _60 + (-_76);
    _106 = max(_106, 0.0);
    _58 = (vec3(_106) * _64) + _58;
    _64.x = _106 + _95;
    _84 = _66 + (-_76);
    _84 = max(_84, 0.0);
    _58 = (vec3(_84) * _67) + _58;
    _64.x = _84 + _64.x;
    _94 = _69 + (-_76);
    _94 = max(_94, 0.0);
    _58 = (vec3(_94) * _78) + _58;
    _64.x = _94 + _64.x;
    _67.x = _71 + (-_76);
    _67.x = max(_67.x, 0.0);
    _58 = (_67.xxx * _79) + _58;
    _64.x += _67.x;
    _86 = _55 + (-_76);
    _86 = max(_86, 0.0);
    vec3 _822 = (vec3(_86) * _38.xyz) + _58;
    _38 = vec4(_822.x, _822.y, _822.z, _38.w);
    _58.x = _64.x + _86;
    _58.x += 0.001000000047497451305389404296875;
    vec3 _838 = _38.xyz / _58.xxx;
    _33 = vec4(_838.x, _838.y, _838.z, _33.w);
    _38.x = _100 * _90;
    _38.x = (_98 * _99) + _38.x;
    _38.x = (_106 * _101) + _38.x;
    _38.x = (_84 * _102) + _38.x;
    _38.x = (_94 * _104) + _38.x;
    _38.x = (_67.x * _105) + _38.x;
    _38.x = (_86 * _80) + _38.x;
    _33.w = _38.x / _58.x;
    _77.x = _33.w >= 0.00020080320246051996946334838867188;
    _81.x = Varying_1.z;
    _81.y = Varying_2.z;
    _81.z = Varying_3.z;
    _88.x = dot(_81, _81);
    _88.x = inversesqrt(_88.x);
    _88 = _88.xxx * _81;
    if (_77.x)
    {
        _81.x = Varying_1.w;
        _81.y = Varying_3.w;
        _96 = _81.xy * _110__m5.zz;
        vec2 _944 = _81.xy * _110__m6.xx;
        _81 = vec3(_944.x, _944.y, _81.z);
        _38.x = dot(_110__m4.xyz, _110__m4.xyz);
        _38.x = inversesqrt(_38.x);
        vec3 _965 = _38.xxx * _110__m4.xyz;
        _38 = vec4(_965.x, _965.y, _965.z, _38.w);
        _38.x = dot(_88, _38.xyz);
        _38.x = clamp(_38.x, 0.0, 1.0);
        _38.x += 9.9999997473787516355514526367188e-05;
        _38.x = log2(_38.x);
        _38.x *= _110__m4.w;
        _38.x = exp2(_38.x);
        _38.x = _33.w * _38.x;
        _97 = texture(_17, _96).x;
        _107 = texture(_17, _81.xy).x;
        _37 = texture(_18, _81.xy);
        _82.x = (_97 * _107) + 9.9999997473787516355514526367188e-05;
        _82.x = log2(_82.x);
        _82.x *= _110__m2.w;
        _82.x = exp2(_82.x);
        _58 = (-_110__m2.xyz) + _110__m3.xyz;
        _82 = (_82.xxx * _58) + _110__m2.xyz;
        _82 = (_37.xyz * _82) + (-_33.xyz);
        vec3 _1063 = (_38.xxx * _82) + _33.xyz;
        _33 = vec4(_1063.x, _1063.y, _1063.z, _33.w);
        _82.x = _37.w + (-0.300000011920928955078125);
        _36.w = (_38.x * _82.x) + 0.300000011920928955078125;
        _81.x = _33.w * 0.4979999959468841552734375;
        _81.y = 0.0;
        _38 = vec4(_81.xy.x, _81.xy.y, _38.z, _38.w);
        _58.x = 0.0;
        _33.w = 0.0;
    }
    else
    {
        _38.x = 0.039999999105930328369140625;
        _38.y = 0.039999999105930328369140625;
        _58.x = 0.039999999105930328369140625;
        _36.w = 0.300000011920928955078125;
    }
    vec3 _1100 = (_88 * vec3(0.5)) + vec3(0.5);
    _36 = vec4(_1100.x, _1100.y, _1100.z, _36.w);
    _38.w = _77.x ? 0.054901964962482452392578125 : 0.0;
    _77.x = any(notEqual(vec4(0.0), vec4(_110__m1)));
    _38.z = _77.x ? 0.0 : _58.x;
    Output_0 = _36;
    Output_1 = _33;
    Output_2 = _38;
}

void main()
{
    vec3 _1158 = vec3(0.0);
    vec3 _1202 = vec3(0.0);
    vec3 _1237 = vec3(0.0);
    _120();
    if (_2 != 0u)
    {
        _1151 = _1134[((uint(gl_FragCoord.x) & 1u) << 1u) | (uint(gl_FragCoord.y) & 1u)];
        _1155 = (_2 >> 0u) & 3u;
        switch (_1155)
        {
            case 1u:
            {
                _1158 = vec3(_1151 * 2.0);
                _1161 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _1158 = vec3(_1151);
                _1161 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _1158 = vec3(_1151, _1151 * 0.5, _1151);
                _1161 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _1187 = Output_0.xyz + _1158;
        Output_0 = vec4(_1187.x, _1187.y, _1187.z, Output_0.w);
        vec3 _1196 = round(Output_0.xyz * _1161) / _1161;
        Output_0 = vec4(_1196.x, _1196.y, _1196.z, Output_0.w);
        _1201 = (_2 >> 2u) & 3u;
        switch (_1201)
        {
            case 1u:
            {
                _1202 = vec3(_1151 * 2.0);
                _1203 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _1202 = vec3(_1151);
                _1203 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _1202 = vec3(_1151, _1151 * 0.5, _1151);
                _1203 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _1222 = Output_1.xyz + _1202;
        Output_1 = vec4(_1222.x, _1222.y, _1222.z, Output_1.w);
        vec3 _1231 = round(Output_1.xyz * _1203) / _1203;
        Output_1 = vec4(_1231.x, _1231.y, _1231.z, Output_1.w);
        _1236 = (_2 >> 4u) & 3u;
        switch (_1236)
        {
            case 1u:
            {
                _1237 = vec3(_1151 * 2.0);
                _1238 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _1237 = vec3(_1151);
                _1238 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _1237 = vec3(_1151, _1151 * 0.5, _1151);
                _1238 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _1257 = Output_2.xyz + _1237;
        Output_2 = vec4(_1257.x, _1257.y, _1257.z, Output_2.w);
        vec3 _1266 = round(Output_2.xyz * _1238) / _1238;
        Output_2 = vec4(_1266.x, _1266.y, _1266.z, Output_2.w);
    }
}


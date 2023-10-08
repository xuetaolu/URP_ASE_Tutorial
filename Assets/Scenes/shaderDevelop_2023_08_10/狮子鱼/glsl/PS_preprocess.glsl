#version 450

#define _32__m0 _32._m0
#define _32__m1 _32._m1
#define _32__m2 _32._m2
#define _32__m3 _32._m3
#define _32__m4 _32._m4
#define _32__m5 _32._m5
#define _32__m6 _32._m6
#define _32__m7 _32._m7
#define _32__m8 _32._m8
#define _32__m9 _32._m9
#define _32__m10 _32._m10
#define _32__m11 _32._m11


layout(early_fragment_tests) in;

layout(constant_id = 4) const uint _2 = 0u;

struct _36
{
    float _m0;
    float _m1;
    float _m2;
    float _m3;
};

const float _284[4] = float[](-0.01171875, 0.00390625, 0.01171875, -0.00390625);

layout(set = 1, binding = 1, std140) uniform _30_32
{
    vec4 _m0;
    vec3 _m1;
    vec4 _m2;
    vec4 _m3;
    vec4 _m4;
    vec4 _m5;
    vec4 _m6;
    vec4 _m7;
    vec4 _m8;
    float _m9;
    float _m10;
    float _m11;
} _32;

layout(set = 0, binding = 0, std140) uniform _37_39
{
    vec4 _m0;
    uint _m1;
    uint _m2;
    int _m3;
    int _m4;
    ivec4 _m5;
    uvec4 _m6;
    _36 _m7;
} _39;

layout(set = 2, binding = 0) uniform sampler2D _7;
layout(set = 2, binding = 1) uniform sampler2D _8;

layout(location = 0) in vec4 Varying_0;
layout(location = 1) in vec3 Varying_1;
layout(location = 2) in vec2 Varying_2;
layout(location = 3) in vec2 Varying_3;
layout(location = 0, component = 0) out vec4 _43[8];
vec4 _20;
vec3 _22;
vec3 _23;
vec3 _24;
vec3 _25;
vec2 _27;
float _29;
float _302;
uint _306;
vec3 _312 = vec3(255.0);
uint _356;
vec3 _358 = vec3(255.0);
uint _394;
vec3 _396 = vec3(255.0);
uint _432;
vec3 _434 = vec3(255.0);
uint _470;
vec3 _472 = vec3(255.0);
uint _508;
vec3 _510 = vec3(255.0);
uint _547;
vec3 _549 = vec3(255.0);
uint _586;
vec3 _588 = vec3(255.0);

void _46()
{
    vec3 _55 = (-Varying_0.xyz) + _32__m1;
    _20 = vec4(_55.x, _55.y, _55.z, _20.w);
    _29 = dot(_20.xyz, _20.xyz);
    _29 = inversesqrt(_29);
    _22.x = dot(_32__m2.xyz, _32__m2.xyz);
    _22.x = inversesqrt(_22.x);
    _22 = _22.xxx * _32__m2.xyz;
    vec3 _93 = (_20.xyz * vec3(_29)) + _22;
    _20 = vec4(_93.x, _93.y, _93.z, _20.w);
    _29 = dot(_20.xyz, _20.xyz);
    _29 = inversesqrt(_29);
    vec3 _107 = vec3(_29) * _20.xyz;
    _20 = vec4(_107.x, _107.y, _107.z, _20.w);
    _29 = dot(Varying_1, Varying_1);
    _29 = inversesqrt(_29);
    _23 = vec3(_29) * Varying_1;
    _20.x = dot(_20.xyz, _23);
    _20.y = dot(_23, _22);
    vec2 _131 = max(_20.xy, vec2(0.0));
    _20 = vec4(_131.x, _131.y, _20.z, _20.w);
    _20.x = log2(_20.x);
    _27.x = _32__m0.y + _32__m5.y;
    _27.x *= _32__m10;
    _27 = (_27.xx * vec2(0.0, 0.100000001490116119384765625)) + Varying_3;
    _27 = (_27 * _32__m8.xy) + _32__m8.zw;
    _27.x = texture(_7, _27).w;
    _27.y = _27.x * _32__m11;
    _27 *= vec2(_32__m9);
    _29 = (_27.y * 10.0) + 1.0;
    _29 = exp2(_29);
    _20.x *= _29;
    _20.x = exp2(_20.x);
    _23 = _20.xxx * _32__m4.xyz;
    vec3 _215 = _27.xxx * _23;
    _20 = vec4(_215.x, _20.y, _215.y, _215.z);
    vec2 _227 = (Varying_2 * _32__m7.xy) + _32__m7.zw;
    _23 = vec3(_227.x, _227.y, _23.z);
    _24 = texture(_8, _23.xy).xyz;
    _23 = _24 * _32__m6.xyz;
    _22 = _32__m3.xyz + _32__m3.xyz;
    _23 = _22 * _23;
    _25 = (_20.yyy * _32__m4.xyz) + _22;
    vec3 _265 = (_25 * _23) + _20.xzw;
    _43[0u] = vec4(_265.x, _265.y, _265.z, _43[0u].w);
    _43[0u].w = 1.0;
}

void main()
{
    vec3 _309 = vec3(0.0);
    vec3 _357 = vec3(0.0);
    vec3 _395 = vec3(0.0);
    vec3 _433 = vec3(0.0);
    vec3 _471 = vec3(0.0);
    vec3 _509 = vec3(0.0);
    vec3 _548 = vec3(0.0);
    vec3 _587 = vec3(0.0);
    _46();
    if (_2 != 0u)
    {
        _302 = _284[((uint(gl_FragCoord.x) & 1u) << 1u) | (uint(gl_FragCoord.y) & 1u)];
        _306 = (_2 >> 0u) & 3u;
        switch (_306)
        {
            case 1u:
            {
                _309 = vec3(_302 * 2.0);
                _312 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _309 = vec3(_302);
                _312 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _309 = vec3(_302, _302 * 0.5, _302);
                _312 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _340 = _43[0u].xyz + _309;
        _43[0u] = vec4(_340.x, _340.y, _340.z, _43[0u].w);
        vec3 _350 = round(_43[0u].xyz * _312) / _312;
        _43[0u] = vec4(_350.x, _350.y, _350.z, _43[0u].w);
        _356 = (_2 >> 2u) & 3u;
        switch (_356)
        {
            case 1u:
            {
                _357 = vec3(_302 * 2.0);
                _358 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _357 = vec3(_302);
                _358 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _357 = vec3(_302, _302 * 0.5, _302);
                _358 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _378 = _43[1u].xyz + _357;
        _43[1u] = vec4(_378.x, _378.y, _378.z, _43[1u].w);
        vec3 _388 = round(_43[1u].xyz * _358) / _358;
        _43[1u] = vec4(_388.x, _388.y, _388.z, _43[1u].w);
        _394 = (_2 >> 4u) & 3u;
        switch (_394)
        {
            case 1u:
            {
                _395 = vec3(_302 * 2.0);
                _396 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _395 = vec3(_302);
                _396 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _395 = vec3(_302, _302 * 0.5, _302);
                _396 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _416 = _43[2u].xyz + _395;
        _43[2u] = vec4(_416.x, _416.y, _416.z, _43[2u].w);
        vec3 _426 = round(_43[2u].xyz * _396) / _396;
        _43[2u] = vec4(_426.x, _426.y, _426.z, _43[2u].w);
        _432 = (_2 >> 6u) & 3u;
        switch (_432)
        {
            case 1u:
            {
                _433 = vec3(_302 * 2.0);
                _434 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _433 = vec3(_302);
                _434 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _433 = vec3(_302, _302 * 0.5, _302);
                _434 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _454 = _43[3u].xyz + _433;
        _43[3u] = vec4(_454.x, _454.y, _454.z, _43[3u].w);
        vec3 _464 = round(_43[3u].xyz * _434) / _434;
        _43[3u] = vec4(_464.x, _464.y, _464.z, _43[3u].w);
        _470 = (_2 >> 8u) & 3u;
        switch (_470)
        {
            case 1u:
            {
                _471 = vec3(_302 * 2.0);
                _472 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _471 = vec3(_302);
                _472 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _471 = vec3(_302, _302 * 0.5, _302);
                _472 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _492 = _43[4u].xyz + _471;
        _43[4u] = vec4(_492.x, _492.y, _492.z, _43[4u].w);
        vec3 _502 = round(_43[4u].xyz * _472) / _472;
        _43[4u] = vec4(_502.x, _502.y, _502.z, _43[4u].w);
        _508 = (_2 >> 10u) & 3u;
        switch (_508)
        {
            case 1u:
            {
                _509 = vec3(_302 * 2.0);
                _510 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _509 = vec3(_302);
                _510 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _509 = vec3(_302, _302 * 0.5, _302);
                _510 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _530 = _43[5u].xyz + _509;
        _43[5u] = vec4(_530.x, _530.y, _530.z, _43[5u].w);
        vec3 _540 = round(_43[5u].xyz * _510) / _510;
        _43[5u] = vec4(_540.x, _540.y, _540.z, _43[5u].w);
        _547 = (_2 >> 12u) & 3u;
        switch (_547)
        {
            case 1u:
            {
                _548 = vec3(_302 * 2.0);
                _549 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _548 = vec3(_302);
                _549 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _548 = vec3(_302, _302 * 0.5, _302);
                _549 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _569 = _43[6u].xyz + _548;
        _43[6u] = vec4(_569.x, _569.y, _569.z, _43[6u].w);
        vec3 _579 = round(_43[6u].xyz * _549) / _549;
        _43[6u] = vec4(_579.x, _579.y, _579.z, _43[6u].w);
        _586 = (_2 >> 14u) & 3u;
        switch (_586)
        {
            case 1u:
            {
                _587 = vec3(_302 * 2.0);
                _588 = vec3(15.0);
                break;
            }
            case 2u:
            {
                _587 = vec3(_302);
                _588 = vec3(31.0);
                break;
            }
            case 3u:
            {
                _587 = vec3(_302, _302 * 0.5, _302);
                _588 = vec3(31.0, 63.0, 31.0);
                break;
            }
        }
        vec3 _608 = _43[7u].xyz + _587;
        _43[7u] = vec4(_608.x, _608.y, _608.z, _43[7u].w);
        vec3 _618 = round(_43[7u].xyz * _588) / _588;
        _43[7u] = vec4(_618.x, _618.y, _618.z, _43[7u].w);
    }
}


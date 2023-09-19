#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
};
#ifndef GENSHIP_TERRAIN_V2f
#define GENSHIP_TERRAIN_V2f
struct v2f
{
    float4 vertex : SV_POSITION;
    float4 Varying_0 : TEXCOORD0;
    float4 Varying_1 : TEXCOORD1;
    float4 Varying_2 : TEXCOORD2;
    float4 Varying_3 : TEXCOORD3;
};
#endif
// #define _9__m0 _9._m0
#define _53__m0 float4(1.00, 0.25, 6000.00, 0.00017) // _53._m0
// static const matrix _53__m1 = {float4(0.85884, -0.83339, -0.69361, -0.69355      ),
//                                 float4(0.00033, 2.16144, -0.44611, -0.44607       ),
//                                 float4(1.05192, 0.67975, 0.56574, 0.56569         ),
//                                 float4(-25.82479, -555.03705, -13.64874, -13.14762)}; // _53._m1
#define _53__m1 transpose(UNITY_MATRIX_VP)
#define _53__m2 0 // _53._m2
static const matrix _53__m3 = {float4(1.00, 0.00, 0.00, 0.00            ),
                                float4(0.00, 1.00, 0.00, 0.00            ),
                                float4(0.00, 0.00, 1.00, 0.00            ),
                                float4(-910.36584, 0.00, -781.65784, 1.00)};// _53._m3
static const matrix _53__m4 = {float4(1.00, 0.00, 0.00, 0.00          ),
                                float4(0.00, 1.00, 0.00, 0.00          ),
                                float4(0.00, 0.00, 1.00, 0.00          ),
                                float4(910.36584, 0.00, 781.65784, 1.00)};//_53._m4
#define _53__m5 float4(513.00, 513.00, 2.00, 2.00) // _53._m5
#define _53__m6 float4(0.00, 0.00, 512.00, 512.00) // _53._m6

// #define _LocalTerrainOffset float4(416.00, 432.00, 1.00, 0.00)
// #define _LocalTerrainOffset float4(400.00, 432.00, 1.00, 0.00)
// #define _LocalTerrainOffset float4(384.00, 432.00, 1.00, 0.00)
float4 _LocalTerrainOffset;
#define _9__m0__3__m1 float4(0.00, 0.00, 0.00, 0.00)

UNITY_DECLARE_TEX2D(_VS_res13);
// float2 _VS_res13_range;
UNITY_DECLARE_TEX2D(_VS_res14);
// float _debug_value;

float4 GlslToDxClipPos(float4 clipPos) {
    clipPos.y = -clipPos.y;
    clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
    return clipPos;
}

v2f vert (appdata v)
{
    v2f o;
    float4 Vertex_Position = v.vertex;
    float4 Vertex_1 = float4(v.uv, v.uv2);
    
    float4 _23;
    // uint _25;
    float4 _27;
    bool4 _31;
    float3 _34;
    int _37;
    float4 _38;
    float4 _39;
    float4 _40;
    float3 _41;
    float3 _42;
    float2 _45;
    float2 _46;
    float _48;
    float4 _60;
    _27 = Vertex_1.zzzz * float4(0.5, 0.25, 0.125, 0.0625);
    _27 = frac(_27);
    _31 = _27 >= (0.5);
    _27 = lerp((0.0), (1.0), (_31));
    // _37 = gl_InstanceIndex + _53__m2;
    // _37 = _37 << 1;
    _27 *= _9__m0__3__m1;
    _27 *= (0.5);
    _27 = frac(_27);
    _31 = _27 >= (0.5);
    _31.x = _31.z || _31.x;
    _31.y = _31.w || _31.y;
    _31.x = _31.y || _31.x;
    _27.x = float(_31.x);
    float2 _149 = (Vertex_1.xy * _27.xx) + Vertex_Position.xz;
    _27 = float4(_149.x, _149.y, _27.z, _27.w);
    float2 _159 = _27.xy + _LocalTerrainOffset.xy;
    _27 = float4(_159.x, _159.y, _27.z, _27.w);
    _45 = (_27.xy * _LocalTerrainOffset.zz) + (-_53__m6.xy);
    float2 _183 = _27.xy * _LocalTerrainOffset.zz;
    _27 = float4(_183.x, _183.y, _27.z, _27.w);
    _45 += (0.5);
    _45 = float2(_45.x / _53__m6.z, _45.y / _53__m6.w);
    _34.x = UNITY_SAMPLE_TEX2D_LOD(_VS_res13, _45, 0.0).x;
    _45 = UNITY_SAMPLE_TEX2D_LOD(_VS_res14, _45, 0.0).xy;
    _45 = (_45 * (4.0)) + (-2.0);
    _38 = _34.xxxx * _53__m3[1u];
    _34 = _34.xxx * _53__m3[1u].xyz;
    float2 _239 = float2(_27.x * _53__m5.z, _27.y * _53__m5.w);
    _39 = float4(_239.x, _239.y, _39.z, _39.w);
    _38 = (_53__m3[0u] * _39.xxxx) + _38;
    _38 = (_53__m3[2u] * _39.yyyy) + _38;
    _38 += _53__m3[3u];
    // _38.y += _debug_value;
    _40 = _38.yyyy * _53__m1[1u];
    _40 = (_53__m1[0u] * _38.xxxx) + _40;
    _40 = (_53__m1[2u] * _38.zzzz) + _40;
    _38 = (_53__m1[3u] * _38.wwww) + _40;
    // gl_Position = _38;
    // o.vertex = GlslToDxClipPos(_38);
    o.vertex = _38;
    _46 = _53__m5.xy + (-1.0);
    float2 _297 = _27.xy / _46;
    o.Varying_0 = float4(_297.x, _297.y, o.Varying_0.z, o.Varying_0.w);
    o.Varying_0 = float4(o.Varying_0.x, o.Varying_0.y, (0.0), (0.0));
    _34 = (_53__m3[0u].xyz * _39.xxx) + _34;
    _34 = (_53__m3[2u].xyz * _39.yyy) + _34;
    _34 += _53__m3[3u].xyz;
    o.Varying_1.w = _34.x;
    _27.x = dot(_45, _45);
    float2 _338 = ((-_27.xx) * float2(0.25, 0.5)) + (1.0);
    _39 = float4(_39.x, _39.y, _338.x, _338.y);
    _27.x = sqrt(_39.z);
    float2 _348 = _27.xx * _45;
    _39 = float4(_348.x, _348.y, _39.z, _39.w);
    _27.y = dot(_39.xyw, _53__m4[0u].xyz);
    _27.z = dot(_39.xyw, _53__m4[1u].xyz);
    _27.x = dot(_39.xyw, _53__m4[2u].xyz);
    _48 = dot(_27.xyz, _27.xyz);
    _48 = rsqrt(_48);
    float3 _383 = (_48) * _27.xyz;
    _27 = float4(_383.x, _383.y, _383.z, _27.w);
    o.Varying_1.z = _27.y;
    _41 = _27.yzx * float3(1.0, 0.0, 0.0);
    _41 = (_27.xyz * float3(0.0, 0.0, 1.0)) + (-_41);
    _42 = _27.xyz * _41;
    _42 = (_27.zxy * _41.yzx) + (-_42);
    o.Varying_1.y = -_42.x;
    o.Varying_1.x = _41.z;
    o.Varying_2.x = _41.x;
    o.Varying_2.z = _27.z;
    o.Varying_3.z = _27.x;
    o.Varying_2.w = _34.y;
    o.Varying_3.w = _34.z;
    o.Varying_2.y = -_42.y;
    o.Varying_3.y = -_42.z;
    o.Varying_3.x = 0.0;
    _27.x = _38.y * _53__m0.x;
    _27.w = _27.x * 0.5;
    float2 _455 = _38.xw * (0.5);
    _27 = float4(_455.x, _27.y, _455.y, _27.w);
    _23 = float4(_23.x, _23.y, _38.zw.x, _38.zw.y);
    float2 _466 = _27.zz + _27.xw;
    _23 = float4(_466.x, _466.y, _23.z, _23.w);
    
    
    // o.vertex = UnityObjectToClipPos(v.vertex);
    return o;
}
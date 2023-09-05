#include "genship_water_common.hlsl"



// #define _11__m0 _11._m0

// _28 = 8,  _28 / 8 = 0
// #define _11__m0_instance__m0 _11__m0[_28 / 8]._m0
static const float4 _11__m0_instance__m0[4] = 
{
    float4(1.00, 0.00, 0.00, 0.00           ),
    float4(0.00, 1.00, 0.00, 0.00           ),
    float4(0.00, 0.00, 1.00, 0.00           ),
    float4(-63.74597, 196.06, 34.72217, 1.00)
};//_11__m0[_28 / 8]._m0

#define _38__m0 float3(-76.56767, 199.90689, 87.00145   ) //_38._m0
#define _38__m1 float4(1.00, 0.25, 6000.00, 0.00017     ) //_38._m1
#define _38__m2 float4(0.00, 0.12654, 0.00, 0.20238     ) //_38._m2
#define _38__m3 float4(0.00, 0.15191, 0.00, 0.28788     ) //_38._m3
#define _38__m4 float4(0.00, 0.19486, 0.00, 0.45829     ) //_38._m4
#define _38__m5 float4(-0.04413, -0.03476, -0.0106, 1.00) //_38._m5
static const float4 _38__m6[4] = 
{
    float4(0.63206, -0.34567, 0.69355, 0.00     ),
    float4(0.00, 0.895, 0.44607, 0.00           ),
    float4(0.77492, 0.28194, -0.56569, 0.00     ),
    float4(-19.02394, -229.91272, 13.14762, 1.00)
};//_38._m6
static const float4  _38__m7[4] = 
{
    float4(0.85884, -0.83339, -0.69361, -0.69355      ),
    float4(0.00033, 2.16144, -0.44611, -0.44607       ),
    float4(1.05192, 0.67975, 0.56574, 0.56569         ),
    float4(-25.82479, -555.03705, -13.64874, -13.14762)
}; //_38._m7
#define _38__m8 0    // _38._m8
#define _38__m9 0.00 // _38._m9
v2f vert (appdata v)
{
    v2f o;
    // o.vertex = UnityObjectToClipPos(v.vertex);
    
    float4 Vertex_Position = v.vertex;
    float4 Vertex_1 = v.color;  // xy w
    float4 Vertex_2 = float4(v.uv, 1, 1);  // only xy

    float4 _18;
    float3 _25;
    // int _28;
    float4 _30;
    float4 _31;
    float3 _32;
    float3 _33;
    float _35;
    // float4 _45;
    
    // #define _38__m8 0    // _38._m8
    // _28 = gl_InstanceIndex + _38__m8;
    // _28 = 0 + _38__m8;
    // _28 = _28 << 3;
    // _28 = 8,  _28 / 8 = 0
    _33 = Vertex_Position.yyy * _11__m0_instance__m0[1u].xyz;
    _33 = (_11__m0_instance__m0[0u].xyz * Vertex_Position.xxx) + _33;
    _33 = (_11__m0_instance__m0[2u].xyz * Vertex_Position.zzz) + _33;
    float3 _105 = _33 + _11__m0_instance__m0[3u].xyz;
    _30 = float4(_105.x, _105.y, _105.z, _30.w);
    _25 = (_11__m0_instance__m0[3u].xyz * Vertex_Position.www) + _33;
    _25 = (-_25) + _38__m0;
    _31 = _30.yyyy * _38__m7[1u];
    _31 = (_38__m7[0u] * _30.xxxx) + _31;
    _31 = (_38__m7[2u] * _30.zzzz) + _31;
    o.Varying_4 = float4(_30.xyz.x, _30.xyz.y, _30.xyz.z, o.Varying_4.w);
    _30 = _31 + _38__m7[3u];
    _30.z = (_38__m9 * _30.w) + _30.z;
    // gl_Position = _30;
    o.vertex = GlslToDxClipPos(_30);
    o.Varying_2 = float4(o.Varying_2.x, o.Varying_2.y, _30.zw.x, _30.zw.y);
    o.Varying_0 = Vertex_1;
    _18 = float4(Vertex_2.xy.x, Vertex_2.xy.y, _18.z, _18.w);
    _18 = float4(_18.x, _18.y, 0.0, 0.0);
    _32.x = dot(_38__m2.yw, 1.0);
    _32.y = dot(_38__m3.yw, 1.0);
    _32.z = dot(_38__m4.yw, 1.0);
    _32 += (-_38__m5.xyz);
    float3 Varying_24 = max(_32, 0.0);
    o.Varying_1 = float4(Varying_24.x, Varying_24.y, Varying_24.z, o.Varying_1.w);
    o.Varying_1.w = 0.0;
    _35 = _30.y * _38__m1.x;
    float2 Varying_40 = _30.xw * 0.5;
    _30 = float4(Varying_40.x, _30.y, Varying_40.y, _30.w);
    _30.w = _35 * 0.5;
    float2 _230 = _30.zz + _30.xw;
    o.Varying_2 = float4(_230.x, _230.y, o.Varying_2.z, o.Varying_2.w);
    _30.x = _38__m6[0u].z;
    _30.y = _38__m6[1u].z;
    _30.z = _38__m6[2u].z;
    o.Varying_3.w = dot(_30.xyz, _25);
    o.Varying_3 = float4(_25.x, _25.y, _25.z, o.Varying_3.w);
    o.Varying_4.w = 0.0;

    
    
    return o;
}
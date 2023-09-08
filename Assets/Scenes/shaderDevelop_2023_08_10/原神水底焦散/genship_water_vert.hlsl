#include "genship_water_common.hlsl"



// #define _11__m0 _11._m0

// _28 = 8,  _28 / 8 = 0
// #define UNITY_MATRIX_M _11__m0[_28 / 8]._m0
// static const matrix UNITY_MATRIX_M = 
// {
//     float4(1.00, 0.00, 0.00, 0.00           ),
//     float4(0.00, 1.00, 0.00, 0.00           ),
//     float4(0.00, 0.00, 1.00, 0.00           ),
//     float4(-63.74597, 196.06, 34.72217, 1.00)
// };//_11__m0[_28 / 8]._m0

#define _RolePos float3(-76.56767, 199.90689, 87.00145   ) //_38._m0
// #define _ProjectionParams float4(1.00, 0.25, 6000.00, 0.00017     ) //_38._m1
 static float4 _FoamParamsYW = float4(0.00, 0.12654, 0.00, 0.20238     ); //_38._m2
// #define _38__m3 float4(0.00, 0.15191, 0.00, 0.28788     ) //_38._m3
// #define _38__m4 float4(0.00, 0.19486, 0.00, 0.45829     ) //_38._m4
 static float4 _FoamColorNegate  = float4(-0.04413, -0.03476, -0.0106, 1.00); //_38._m5
// static const matrix UNITY_MATRIX_V_T = 
// {
//     float4(0.63206, -0.34567, 0.69355, 0.00     ),
//     float4(0.00, 0.895, 0.44607, 0.00           ),
//     float4(0.77492, 0.28194, -0.56569, 0.00     ),
//     float4(-19.02394, -229.91272, 13.14762, 1.00)
// };//_38._m6
// #define UNITY_MATRIX_V_T transpose(UNITY_MATRIX_V)
static const matrix  UNITY_MATRIX_VP_T = 
{
    float4(0.85884, -0.83339, -0.69361, -0.69355      ),
    float4(0.00033, 2.16144, -0.44611, -0.44607       ),
    float4(1.05192, 0.67975, 0.56574, 0.56569         ),
    float4(-25.82479, -555.03705, -13.64874, -13.14762)
}; //_38._m7
// #define _38__m8 0    // _38._m8
#define _ClipPosZ_01_Offset 0.00 // _38._m9

float4 GlslToDxClipPos(float4 clipPos) {
    clipPos.y = -clipPos.y;
    clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
    return clipPos;
}


v2f vert (appdata v)
{
    v2f o;
    
    float4 Vertex_Position = v.vertex;
    float4 Vertex_Color = v.color;  // xy w
    // float4 Vertex_uv = float4(v.uv, 1, 1);  // only xy


    float3 _worldPos = mul( UNITY_MATRIX_M, float4(Vertex_Position.xyz, 1.0) );
    float3 _worldPos2 = mul( UNITY_MATRIX_M, Vertex_Position ).xyz;

    
    float3 _relativeToRolePos = _RolePos - _worldPos2;
    
    o.Varying_4.xyz = _worldPos;
    o.Varying_4.w = 0.0;

    // float4 _clipPos = mul(transpose(UNITY_MATRIX_VP_T), float4(_worldPos, 1));
    float4 _clipPos = mul(UNITY_MATRIX_VP, float4(_worldPos, 1));
    
    _clipPos.z += (_ClipPosZ_01_Offset * _clipPos.w);

    // o.vertex = GlslToDxClipPos(_clipPos);
    o.vertex = _clipPos;
    
    o.Varying_ColorXYW = Vertex_Color;

    o.Varying_1.xyz = max(dot(_FoamParamsYW.yw, 1.0) - _FoamColorNegate.xyz, 0.0);
    o.Varying_1.w = 0.0;


    o.Varying_NonStereoScreenPos.xy = (_clipPos.w * 0.5) + float2( _clipPos.x * 0.5, _clipPos.y * _ProjectionParams.x * 0.5 );
    o.Varying_NonStereoScreenPos.zw = _clipPos.zw;

    o.Varying_3.w = dot(UNITY_MATRIX_V[2u].xyz, _relativeToRolePos);
    o.Varying_3.xyz = _relativeToRolePos;
    

    
    
    return o;
}
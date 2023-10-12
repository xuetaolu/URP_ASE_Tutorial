#include "genship_water_common_v2.hlsl"

float _FoamLightAddScalar = 0.32892; //_38._m2
float4 _FoamLightAddColor  = float4(0.04413, 0.03476, 0.0106, 1.00); //_38._m5

#define _ClipPosZ_01_Offset 0.00 // _38._m9

v2f vert (appdata v)
{
    v2f o = (v2f)0;
    // TRANSFER_SHADOW(o);
    
    float4 Vertex_Position = v.vertex;
    float4 Vertex_Color = v.color;  // xy w
    
    float3 _worldPos = mul( UNITY_MATRIX_M, float4(Vertex_Position.xyz, 1.0) );
    
    float3 _viewDir = _WorldSpaceCameraPos - _worldPos;
    
    o.Varying_WorldPosXYZ.xyz = _worldPos;
    o.Varying_WorldPosXYZ.w = 0.0;
    
    float4 _clipPos = mul(UNITY_MATRIX_VP, float4(_worldPos, 1));
    
    _clipPos.z += (_ClipPosZ_01_Offset * _clipPos.w);
    
    o.vertex = _clipPos;
    
    o.Varying_ColorXYW = Vertex_Color;
    
    o.Varying_FoamLightAdd.xyz = max(_FoamLightAddScalar + _FoamLightAddColor.xyz, 0.0);
    o.Varying_FoamLightAdd.w = 0.0;


    o.Varying_NonStereoScreenPos.xy = (_clipPos.w * 0.5) + float2( _clipPos.x * 0.5, _clipPos.y * _ProjectionParams.x * 0.5 );
    o.Varying_NonStereoScreenPos.zw = _clipPos.zw;

    const float3 _worldCameraBack = UNITY_MATRIX_V[2u].xyz;
    o.Varying_ViewDirXYZ_BackDotVW.w = dot(_worldCameraBack, _viewDir);
    o.Varying_ViewDirXYZ_BackDotVW.xyz = _viewDir;
    
    return o;
}
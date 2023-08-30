#ifndef GENSHIP_CLOUD_COMMON_INCLUDED
#define GENSHIP_CLOUD_COMMON_INCLUDED
#include "UnityCG.cginc"

float4 GlslToDxClipPos(float4 clipPos) {
    clipPos.y = -clipPos.y;
    clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
    return clipPos;
}

struct appdata
{
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
    float2 uv3 : TEXCOORD2;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 Varying_0 : TEXCOORD0;
    float4 Varying_RelativeToRoleDirXYZ_Angle1_n1 : TEXCOORD1;
    float4 Varying_2 : TEXCOORD2;
    float3 Varying_3 : TEXCOORD3;
    float3 Varying_4 : TEXCOORD4;
    float3 Varying_5 : TEXCOORD5;
    float3 Varying_6 : TEXCOORD6;
    float3 Varying_7 : TEXCOORD7;
};

#endif
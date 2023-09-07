#ifndef GENSHIP_CLOUD_COMMON_INCLUDED
#define GENSHIP_CLOUD_COMMON_INCLUDED
#include "UnityCG.cginc"

float4 GlslToDxClipPos(float4 clipPos) {
    clipPos.y = -clipPos.y;
    clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
    return clipPos;
}

float Remap(float In, float InMin, float InMax, float OutMin, float OutMax)
{
    return OutMin + (In - InMin) * (OutMax - OutMin) / (InMax - InMin);
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
    float4 Varying_MaskMapUvXY_DisturbanceNoiseUvZW : TEXCOORD0;
    float4 Varying_ViewDirAndAngle1_n1 : TEXCOORD1;
    float4 Varying_DesityRefW_ColorzwYZ_LDotDir01FixX : TEXCOORD2;
    float3 Varying_DayPartColor : TEXCOORD3;
    float3 Varying_ShineColor : TEXCOORD4;
    float3 Varying_TransmissionColor : TEXCOORD5;
    float3 Varying_CloudColor_Bright : TEXCOORD6;
    float3 Varying_CloudColor_Dark : TEXCOORD7;
};

#endif
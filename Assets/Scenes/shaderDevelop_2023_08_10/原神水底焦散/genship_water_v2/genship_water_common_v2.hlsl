#ifndef GENSHIP_WATER_COMMON_INCLUDED
#define GENSHIP_WATER_COMMON_INCLUDED

// #include "UnityCG.cginc"
// #include "AutoLight.cginc"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#include <HLSLSupport.cginc>

struct v2f
{
    float4 Varying_ColorXYW : TEXCOORD0;
    float4 Varying_FoamLightAdd : TEXCOORD1;
    float4 Varying_NonStereoScreenPos : TEXCOORD2;
    float4 Varying_ViewDirXYZ_BackDotVW : TEXCOORD3;
    float4 Varying_WorldPosXYZ : TEXCOORD4;
    float4 vertex : SV_POSITION;
    // SHADOW_COORDS(5)
    // float4 shadowCoord : TEXCOORD5;
};

struct appdata
{
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};


#endif
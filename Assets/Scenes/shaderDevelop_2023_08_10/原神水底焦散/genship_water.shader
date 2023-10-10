Shader "Unlit/genship_water"
{
    Properties
    {
//        _38__m5 ("_38__m5", Vector) = (-0.04413, -0.03476, -0.0106, 1.00)
//        _38__m2 ("_38__m2", Vector) = (0.00, 0.12654, 0.00, 0.20238     )
        
//        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_GlossColor ("_GlossColor", Color) = (2.92204, 1.56181, 0.57585, 1.62808)
        [HDR]_CausticColor ("_CausticColor", Color) = (0.60632, 0.5298, 0.44146, 1.00)
        _WorldPosXY_Offset ("_WorldPosXY_Offset", Vector) = (1934.36584, 0.00, -1266.34216, 0.00)
        _WorldPosXY_Offset_Negative ("_WorldPosXY_Offset_Negative", Vector) = (-1934.36584, 0.00, 1266.34216, 0.00)
        _WorldPosXY_Scale ("_WorldPosXY_Scale", Range(0, 1)) = 0.05
        
        _WorldPosXY_Speed1X ("_WorldPosXY_Speed1X", Range(-1, 1))  = -0.02
        _WorldPosXY_Speed1Y ("_WorldPosXY_Speed1Y", Range(-1, 1))  = -0.01
        _WorldPosXY_Speed2X ("_WorldPosXY_Speed2X", Range(-1, 1))  =  0.05
        _WorldPosXY_Speed2Y ("_WorldPosXY_Speed2Y", Range(-1, 1))  = -0.04
        
        _NormalScale1 ("_NormalScale1", Range(0, 1)) = 0.7
        
        _Color_Base ("_Color_Base", Color) = (0.05891, 0.20904, 0.43325, 0.90 )
        _Color_Height_Add ("_Color_Height_Add", Vector) = (0.27672, 0.01464, -0.23447, 0.00)
        [HDR]_151__m26 ("_151__m26", Color) = (0.00, 0.00, 0.00, 0.00)
        [HDR]_Color_Far_2 ("_Color_Far_2", Color) = (0.50353, 0.31069, 0.31797, 1.30 )
        [HDR]_Color_C ("_Color_C", Color) = (1.00, 1.00, 1.00, 0.07213 )
        [HDR]_Color_D ("_Color_D", Color) = (1.00, 1.00, 1.00, 16.00 )
        
        _GrabTextureFade ("_GrabTextureFade", Range(0, 1)) = 0.0
        _FixNDotH_Power ("_FixNDotH_Power", Range(0, 1000)) = 332.79999
        _GlossPosAdjust ("_GlossPosAdjust", Range(0, 10)) = 2.38
        _WaterSurfAlpha ("_WaterSurfAlpha", Range(0, 1)) = 0.4
        
        _GlossFactor ("_GlossFactor", Range(0, 10)) = 5
        _CausticSpeed ("_CausticSpeed", Range(0, 1)) = 0.131
        _CausticScale ("_CausticScale", Range(0, 1)) = 0.25
        _CausticNormalDisturbance ("_CausticNormalDisturbance", Range(0, 1)) = 0.096
        _CausticVisibleHeightFactor ("_CausticVisibleHeightFactor", Range(0, 10)) = 3.33333
        _CausticDistanceFade ("_CausticDistanceFade", Range(0, 1)) = 0.01667
        
        _CausticColorDisappearOfWaterDepth ("_CausticColorDisappearOfWaterDepth", Range(0, 10)) = 0.87
        _CausticColorDisappearPower ("_CausticColorDisappearPower", Range(0, 10)) = 2.49
        
        _WaterSurfAlphaPower ("_WaterSurfAlphaPower", Range(0, 10)) = 1.50
        
        _WaterBottomDarkPower ("_WaterBottomDarkPower", Range(0, 10)) = 1.00
        _WaterBottomDarkFactor ("_WaterBottomDarkFactor", Range(0, 10)) = 0.06667
        
        _WaterSurfColor ("_WaterSurfColor", Color) = (0.11131, 1.00, 0.9415, 0.00)
        _WaterSurfColorBlend ("_WaterSurfColorBlend", Color) = (1.00, 1.00, 1.00, 0.00)
        
                
        _WaterBottomDarkColor ("_WaterBottomDarkColor", Color) = (0.01694, 0.1433, 0.26481, 0.00)
        _WaterBottomDarkColorBlend ("_WaterBottomDarkColorBlend", Color) = (1.00, 1.00, 1.00, 0.00)
        
        
        _SurfNormalScale2 ("_SurfNormalScale2", Vector) = (0.10238, 0.09815, 0.59876, 0.00)
        
        _SSRNormalDisturbance1 ("_SSRNormalDisturbance1", Range(0, 10)) = 0.60
        _SSRNormalDisturbance2 ("_SSRNormalDisturbance2", Range(0, 10)) = 1.51515
        _SSRAlpha ("_SSRAlpha", Range(0, 1)) = 0.80
        
        _ReflectFactor ("_ReflectFactor", Range(0, 1)) = 0.50
        _ReflectWaterDepthFactor ("_ReflectWaterDepthFactor", Range(0, 1)) = 0.38462
        _ReflectWaterViewYDisappearFactor ("_ReflectWaterViewYDisappearFactor", Range(0, 1)) = 0.93
        
        
        _GlossColorAdd ("_GlossColorAdd", Color) = (0.04413, 0.03476, 0.0106, 1.00)
        _GlossColorAddScalar ("_GlossColorAddScalar", Range(0, 1)) = 0.32892
        
        _FoamLineAreaSize ("_FoamLineAreaSize", Range(0, 1)) = 0.30
        _FoamLineSinFrequency ("_FoamLineSinFrequency", Range(0, 30)) = 19
        _FoamLineSpeed ("_FoamLineSpeed", Range(-10, 10)) = -1.28
        
        _FoamLineFadeDistance ("_FoamLineFadeDistance", Range(0, 1000)) = 205.00
        
        _FoamColor ("_FoamColor", Color) = (1.00, 1.00, 1.00, 1.00)
        _FoamLineVisibleDistance ("_FoamLineVisibleDistance", Range(0, 100)) = 10.00
        _FoamLineFadeDiv ("_FoamLineFadeDiv", Range(0, 100)) = 20.00
        
        _WaterAlpha ("_WaterAlpha", Range(0, 10)) = 2.5641
        
        _DivRefScale ("_DivRefScale", Vector) = (0.045, 0.00214, 0.00, 0.00)
        _DivRefMax ("_DivRefMax", Vector) = (0.9716, -0.02881, 1.00, 0.00)
        
        [Toggle]_SSREnable ("_SSREnable", float) = 1.0
        
//        _151__m24 ("_151__m24", Vector) = (1.00, -1.00, 10000.00, 0.00)
        
        [NoScaleOffset]_DepthTexture ("_DepthTexture ", 2D) = "white" {}
//        [NoScaleOffset]_ScreenMaskMap   ("_ScreenMaskMap ", 2D) = "white" {}
//        [NoScaleOffset]unity_SpecCube0_("unity_SpecCube0_", Cube) = "white" {}
        [NoScaleOffset]_Noise2D_R  ("_Noise2D_R", 2D) = "white" {}
        _Noise2D_R_ScaleSpeed  ("_Noise2D_R_ScaleSpeed", Vector) = (0.20, 0.15, 0.01, 0.01)
        [NoScaleOffset]_NormalMap1  ("_NormalMap1", 2D) = "white" {}
        [NoScaleOffset]_NormalMap2  ("_NormalMap2", 2D) = "white" {}
        [NoScaleOffset]_Noise3DMap  ("_Noise3DMap", 3D) = "white" {}
        [NoScaleOffset]_20_sampler3D  ("_20_sampler3D", 3D) = "white" {}
        [NoScaleOffset]_21_sampler2D  ("_21_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_22_sampler2D  ("_22_sampler2D", 2D) = "white" {}
//        [NoScaleOffset]_GrabTexture  ("_GrabTexture", 2D) = "white" {} // _CameraOpaqueTexture
//        [NoScaleOffset]_ScreenReflectTexture  ("_ScreenReflectTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            // #pragma multi_compile_fwdbase_fullshadows
            #pragma vertex vert
            #pragma fragment frag

            #include "genship_water_vert.hlsl"
            #include "genship_water_frag.hlsl"
            
            ENDHLSL
        }
    }
}

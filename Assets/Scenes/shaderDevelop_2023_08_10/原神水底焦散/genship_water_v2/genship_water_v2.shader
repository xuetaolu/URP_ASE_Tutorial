Shader "Unlit/genship_water_v2"
{
    Properties
    {

//        [HDR]_LightColor ("_LightColor", Color) = (2.92204, 1.56181, 0.57585, 1.62808)
        _LightIntensity ("_LightIntensity", Range(0, 10)) = 5
        _NDotHPower ("_NDotHPower", Range(0, 333)) = 332.79999
        
        [HDR]_ColorFarFog ("_ColorFarFog", Color) = (0.50353, 0.31069, 0.31797, 1.30 )
        _ColorFarFogW ("_ColorFarFogW", Range(0, 2)) = 1.3

        [HDR]_CausticColor ("_CausticColor", Color) = (0.60632, 0.5298, 0.44146, 1.00)
        
        _WorldPosXY_Offset ("_WorldPosXY_Offset", Vector) = (1934.36584, 0.00, -1266.34216, 0.00)       
        _WorldPosXY_Scale ("_WorldPosXY_Scale", Range(0, 1)) = 0.05
        
        _WorldPosXY_Speed1X ("_WorldPosXY_Speed1X", Range(-1, 1))  = -0.02
        _WorldPosXY_Speed1Y ("_WorldPosXY_Speed1Y", Range(-1, 1))  = -0.01
        _WorldPosXY_Speed2X ("_WorldPosXY_Speed2X", Range(-1, 1))  =  0.05
        _WorldPosXY_Speed2Y ("_WorldPosXY_Speed2Y", Range(-1, 1))  = -0.04
        
        _NormalMapScale ("_NormalMapScale", Range(0, 1)) = 0.7
        
        _SurfNormalScale ("_SurfNormalScale", Range(0, 1)) = 0.15
        
        
        _ColorBase ("_ColorBase", Color) = (0.05891, 0.20904, 0.43325, 0.90 )
        _ColorHeightAdd ("_ColorHeightAdd", Vector) = (0.27672, 0.01464, -0.23447, 0.00)

        [HDR]_ColorFarExp ("_ColorFarExp", Color) = (1.00, 1.00, 1.00, 16.00 )
        
        _GrabTextureFade ("_GrabTextureFade", Range(0, 1)) = 0.0
        
        _GlossFactorLookAtHorizontallyAndLightHight ("_GlossFactorLookAtHorizontallyAndLightHight", Range(0, 10)) = 2.38
        _WaterSmoothness ("_WaterSmoothness", Range(0, 1)) = 0.4
        
        
        _CausticSpeed ("_CausticSpeed", Range(0, 1)) = 0.131
        _CausticScale ("_CausticScale", Range(0, 1)) = 0.25
        _CausticNormalDisturbance ("_CausticNormalDisturbance", Range(0, 1)) = 0.096
        
        [Header(__CausticVisibleParams__)]
        _CausticVisibleHeight ("_CausticVisibleHeight", Range(0, 10)) = 3.33333
        _CausticVisibleDistance ("_CausticVisibleDistance", Range(0, 1)) = 0.01667
        _CausticVisibleWaterDepth ("_CausticVisibleWaterDepth", Range(0, 10)) = 0.87
        _CausticVisiblePower ("_CausticVisiblePower", Range(0, 10)) = 2.49
        [Header(_)]
        _WaterReflectPower ("_WaterReflectPower", Range(0, 10)) = 1.50
        
        _WaterBottomDarkPower ("_WaterBottomDarkPower", Range(0, 10)) = 1.00
        _WaterBottomDarkFactor ("_WaterBottomDarkFactor", Range(0, 10)) = 0.06667
        
        _WaterSurfColor ("_WaterSurfColor", Color) = (0.11131, 1.00, 0.9415, 0.00)
        _WaterSurfColorBlend ("_WaterSurfColorBlend", Color) = (1.00, 1.00, 1.00, 0.00)
        
                
        _WaterBottomDarkColor ("_WaterBottomDarkColor", Color) = (0.01694, 0.1433, 0.26481, 0.00)
        _WaterBottomDarkColorBlend ("_WaterBottomDarkColorBlend", Color) = (1.00, 1.00, 1.00, 0.00)
        
        
        _SurfNormalReflectScale ("_SurfNormalReflectScale", Vector) = (0.10238, 0.09815, 0.59876, 0.00)
        
        _SSRNormalDisturbance ("_SSRNormalDisturbance", Range(0, 10)) = 0.60
        _SSRNormalDisturbanceWaterDepthRelevant ("_SSRNormalDisturbanceWaterDepthRelevant", Range(0, 10)) = 1.51515
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
        
        _ExpDampingScaleXYZ ("_ExpDampingScaleXYZ", Vector) = (0.045, 0.00214, 0.00, 0.00)
        _ExpDampingStartXZ ("_ExpDampingStartXZ", Vector) = (0.9716, -0.02881, 1.00, 0.00)
//        
        _ColorVeryFar ("_ColorVeryFar", Color) = (0.39681, 0.34829, 0.44667, 0.00017      )
        
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

            #include "genship_water_vert_v2.hlsl"
            #include "genship_water_frag_v2.hlsl"
            
            ENDHLSL
        }
    }
}

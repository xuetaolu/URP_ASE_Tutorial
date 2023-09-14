Shader "Unlit/genship_water"
{
    Properties
    {
//        _38__m5 ("_38__m5", Vector) = (-0.04413, -0.03476, -0.0106, 1.00)
//        _38__m2 ("_38__m2", Vector) = (0.00, 0.12654, 0.00, 0.20238     )
        
//        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_CausticColor ("_CausticColor", Color) = (2.92204, 1.56181, 0.57585, 1.62808)
        _WorldPosXY_Offset ("_WorldPosXY_Offset", Vector) = (1934.36584, 0.00, -1266.34216, 0.00)
        _WorldPosXY_Offset_Negative ("_WorldPosXY_Offset_Negative", Vector) = (-1934.36584, 0.00, 1266.34216, 0.00)
        _WorldPosXY_Scale ("_WorldPosXY_Scale", Range(0, 1)) = 0.05
        
        _WorldPosXY_Speed1X ("_WorldPosXY_Speed1X", Range(-1, 1))  = -0.02
        _WorldPosXY_Speed1Y ("_WorldPosXY_Speed1Y", Range(-1, 1))  = -0.01
        _WorldPosXY_Speed2X ("_WorldPosXY_Speed2X", Range(-1, 1))  =  0.05
        _WorldPosXY_Speed2Y ("_WorldPosXY_Speed2Y", Range(-1, 1))  = -0.04
        
        _NormalScale1 ("_NormalScale1", Range(0, 1)) = 0.7
        
        [HDR]_Color_Far ("_Color_Far", Color) = (0.05891, 0.20904, 0.43325, 0.90 )
        _151__m14 ("_151__m14", Vector) = (0.27672, 0.01464, -0.23447, 0.00)
        [HDR]_151__m26 ("_151__m26", Color) = (0.00, 0.00, 0.00, 0.00)
        [HDR]_Color_Far_2 ("_Color_Far_2", Color) = (0.50353, 0.31069, 0.31797, 1.30 )
        [HDR]_Color_C ("_Color_C", Color) = (1.00, 1.00, 1.00, 0.07213 )
        [HDR]_Color_D ("_Color_D", Color) = (1.00, 1.00, 1.00, 16.00 )
        
        _GrabTextureFade ("_GrabTextureFade", Range(0, 1)) = 0.0
        
//        _151__m24 ("_151__m24", Vector) = (1.00, -1.00, 10000.00, 0.00)
        
        [NoScaleOffset]_DepthTexture ("_DepthTexture ", 2D) = "white" {}
        [NoScaleOffset]_ScreenMaskMap   ("_ScreenMaskMap ", 2D) = "white" {}
        [NoScaleOffset]_12_samplerCUBE("_12_samplerCUBE", Cube) = "white" {}
        [NoScaleOffset]_13_sampler2D  ("_13_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_NormalMap1  ("_NormalMap1", 2D) = "white" {}
        [NoScaleOffset]_NormalMap2  ("_NormalMap2", 2D) = "white" {}
        [NoScaleOffset]_Noise3DMap  ("_Noise3DMap", 3D) = "white" {}
        [NoScaleOffset]_20_sampler3D  ("_20_sampler3D", 3D) = "white" {}
        [NoScaleOffset]_21_sampler2D  ("_21_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_22_sampler2D  ("_22_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_GrabTexture  ("_GrabTexture", 2D) = "white" {}
        [NoScaleOffset]_ScreenReflectTexture  ("_ScreenReflectTexture", 2D) = "white" {}
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
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "genship_water_vert.hlsl"
            #include "genship_water_frag.hlsl"
            
            ENDCG
        }
    }
}

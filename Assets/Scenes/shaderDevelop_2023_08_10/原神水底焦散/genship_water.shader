Shader "Unlit/genship_water"
{
    Properties
    {
//        _38__m5 ("_38__m5", Vector) = (-0.04413, -0.03476, -0.0106, 1.00)
//        _38__m2 ("_38__m2", Vector) = (0.00, 0.12654, 0.00, 0.20238     )
        
//        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_CausticColor ("_CausticColor", Color) = (2.92204, 1.56181, 0.57585, 1.62808)
        _WorldPosXY_Offset ("_WorldPosXY_Offset", Vector) = (1934.36584, 0.00, -1266.34216, 0.00)
        _WorldPosXY_Scale ("_WorldPosXY_Scale", Range(0, 1)) = 0.05
        
        _WorldPosXY_Speed1X ("_WorldPosXY_Speed1X", Range(-1, 1))  = -0.02
        _WorldPosXY_Speed1Y ("_WorldPosXY_Speed1Y", Range(-1, 1))  = -0.01
        _WorldPosXY_Speed2X ("_WorldPosXY_Speed2X", Range(-1, 1))  =  0.05
        _WorldPosXY_Speed2Y ("_WorldPosXY_Speed2Y", Range(-1, 1))  = -0.04
        
        _NormalScale1 ("_NormalScale1", Range(0, 1)) = 0.7
        
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

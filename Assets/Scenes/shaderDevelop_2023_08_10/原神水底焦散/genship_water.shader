Shader "Unlit/genship_water"
{
    Properties
    {
//        _38__m5 ("_38__m5", Vector) = (-0.04413, -0.03476, -0.0106, 1.00)
//        _38__m2 ("_38__m2", Vector) = (0.00, 0.12654, 0.00, 0.20238     )
        
//        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_DepthTexture ("_DepthTexture ", 2D) = "white" {}
        [NoScaleOffset]_8_sampler2D   ("_8_sampler2D ", 2D) = "white" {}
        [NoScaleOffset]_12_samplerCUBE("_12_samplerCUBE", Cube) = "white" {}
        [NoScaleOffset]_13_sampler2D  ("_13_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_14_sampler2D  ("_14_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_15_sampler2D  ("_15_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_19_sampler3D  ("_19_sampler3D", 3D) = "white" {}
        [NoScaleOffset]_20_sampler3D  ("_20_sampler3D", 3D) = "white" {}
        [NoScaleOffset]_21_sampler2D  ("_21_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_22_sampler2D  ("_22_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_23_sampler2D  ("_23_sampler2D", 2D) = "white" {}
        [NoScaleOffset]_24_sampler2D  ("_24_sampler2D", 2D) = "white" {}
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

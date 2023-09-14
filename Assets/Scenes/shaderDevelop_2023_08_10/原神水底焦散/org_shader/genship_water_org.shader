Shader "Unlit/genship_water_org"
{
    Properties
    {
//        _38__m5 ("_38__m5", Vector) = (-0.04413, -0.03476, -0.0106, 1.00)
//        _38__m2 ("_38__m2", Vector) = (0.00, 0.12654, 0.00, 0.20238     )
        
//        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_DepthTexture ("_DepthTexture ", 2D) = "white" {}
        [NoScaleOffset]_8 ("_8 ", 2D) = "white" {}
        [NoScaleOffset]_12("_12", Cube) = "white" {}
        [NoScaleOffset]_13("_13", 2D) = "white" {}
        [NoScaleOffset]_14("_14", 2D) = "white" {}
        [NoScaleOffset]_15("_15", 2D) = "white" {}
        [NoScaleOffset]_19("_19", 3D) = "white" {}
        [NoScaleOffset]_20("_20", 3D) = "white" {}
        [NoScaleOffset]_21("_21", 2D) = "white" {}
        [NoScaleOffset]_22("_22", 2D) = "white" {}
        [NoScaleOffset]_23("_23", 2D) = "white" {}
        [NoScaleOffset]_24("_24", 2D) = "white" {}
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

            #include "genship_water_vert_org.hlsl"
            #include "genship_water_frag_org.hlsl"
            
            ENDCG
        }
    }
}

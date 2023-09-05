Shader "Unlit/genship_water"
{
    Properties
    {
//        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_7 ("_7 ", 2D) = "white" {}
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

            #include "genship_water_vert.hlsl"
            #include "genship_water_frag.hlsl"
            
            ENDCG
        }
    }
}

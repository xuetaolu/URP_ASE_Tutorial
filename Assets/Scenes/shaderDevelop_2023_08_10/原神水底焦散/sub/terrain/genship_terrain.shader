Shader "Genship/Terrain"
{
    Properties
    {
        [NoScaleOffset]_VS_res13 ("_VS_res13", 2D) = "white" {}
        [NoScaleOffset]_VS_res14 ("_VS_res14", 2D) = "white" {}
        _LocalTerrainOffset ("_LocalTerrainOffset", Vector) = (400.00, 432.00, 1.00, 0.00)
        [NoScaleOffset]_7  ("_7 ", 2D) = "white" {}
        [NoScaleOffset]_8  ("_8 ", 2D) = "white" {}
        [NoScaleOffset]_9  ("_9 ", 2D) = "white" {}
        [NoScaleOffset]_10 ("_10", 2D) = "white" {}
        [NoScaleOffset]_11 ("_11", 2D) = "white" {}
        [NoScaleOffset]_12 ("_12", 2D) = "white" {}
        [NoScaleOffset]_13 ("_13", 2D) = "white" {}
        [NoScaleOffset]_14 ("_14", 2D) = "white" {}
        [NoScaleOffset]_15 ("_15", 2D) = "white" {}
        [NoScaleOffset]_16 ("_16", 2D) = "white" {}
        [NoScaleOffset]_17 ("_17", 2D) = "white" {}
        [NoScaleOffset]_18 ("_18", 2D) = "white" {}

    }
    

    
    SubShader
    {
        Tags { "RenderType"="Opaque" } 
        LOD 100
//        ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "./genship_terrain_vert.hlsl"
            #include "./genship_terrain_frag.hlsl"
            
            ENDHLSL
        }
        
        Pass
        {
            Tags { "LightMode"="DepthOnly" }
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "./genship_terrain_vert.hlsl"
            #include "./genship_terrain_frag.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Tags { "LightMode"="ShadowCaster" }
            ColorMask 0
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "./genship_terrain_vert.hlsl"
            #include "./genship_terrain_frag.hlsl"
            ENDHLSL
        }
    }
}

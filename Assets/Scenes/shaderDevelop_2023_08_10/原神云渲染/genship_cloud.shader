Shader "genship/genship_cloud"
{
    Properties
    {
//        _58__m35 ("_58__m35", Vector) = ( 2.00, 4.00, 0, 0)
        
        _IrradianceMap ("_IrradianceMap", 2D) = "white" {}
        _NoiseMapRGB ("_NoiseMapRGB", 2D) = "white" {}
        _MaskMapRGBA ("_MaskMapRGBA", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "genship_cloud_vert.hlsl"
            #include "genship_cloud_frag.hlsl"

            ENDCG
        }
    }
}

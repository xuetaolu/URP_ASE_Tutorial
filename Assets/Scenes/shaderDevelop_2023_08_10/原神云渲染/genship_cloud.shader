Shader "genship/genship_cloud"
{
    Properties
    {
        _IrradianceMap ("_IrradianceMap", 2D) = "white" {}
        _NoiseMapRGB ("_NoiseMapRGB", 2D) = "white" {}
        _MaskMapRGBA ("_MaskMapRGBA", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }

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

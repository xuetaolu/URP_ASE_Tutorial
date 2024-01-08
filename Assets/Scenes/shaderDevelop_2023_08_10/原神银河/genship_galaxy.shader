Shader "Unlit/genship_galaxy"
{
    Properties
    {
        
        [Header(__AreaMask__)]
        _ColorMask  ("_ColorMask ", 2D) = "white" {}
        _ColorMask_Disturb ("_ColorMask_Disturb", Range(0, 1)) = 0.23
        _ColorMaskDisturbNoise ("_ColorMaskDisturbNoise ", 2D) = "white" {}
        [HideInInspector]_ColorMaskDisturbNoise_ST ("_ColorMaskDisturbNoise_ST", Vector) = (3.50, 1.00, 0.00, 0.00)
        _ColorMaskDisturbNoise_Speed ("_ColorMaskDisturbNoise_Speed", Range(0, 0.1)) = 0.005
        
        [Header(__Center__)]
        _CenterColor ("_CenterColor", Color) = (1.00, 0.00, 0.00, 1.00   )
        _CenterColor_Intensity ("_CenterColor_Intensity", Range(0, 20)) = 18.00
        _CenterBandNoise ("_CenterBandNoise ", 2D) = "white" {}
        [HideInInspector]_CenterBandNoise_ST ("_CenterBandNoise_ST", Vector) = (4.35, 1.10, 0.00, 0.00 )
        _CenterBandNoise_Bias ("_CenterBandNoise_Bias", Range(-0.1, 0)) = -0.05 
        _CenterBandNoise_Scale ("_CenterBandNoise_Scale", Range(0, 2)) = 1.60
        
        [Header(__Beside__)]
        _BesideColor  ("_BesideColor ", Color) = (0.00, 0.74265, 0.03585, 1.00               )
        _BesideColor_Intensity ("_BesideColor_Intensity", Range(0, 20)) = 10.82
        _BesideColorAddition ("_BesideColorAddition", Color) = (0.14706, 0.40, 1.00, 1.00)
        _BesideColorAddition_Intensity ("_BesideColorAddition_Intensity", Range(0, 10)) = 5.00
        _BesideBandNoise ("_BesideBandNoise", 2D) = "white" {}
        [HideInInspector]_BesideBandNoise_ST ("_BesideBandNoise_ST", Vector) = (1.00, 0.40, 0.00, 0.00                     )
        _BesideBandNoise_Speed ("_BesideBandNoise_Speed", Range(-0.1, 0)) = -0.005
        _BesideBandNoise_Bias ("_BesideBandNoise_Bias", Range(0, 1)) = 0.38
        
        [Header(__BesideStar__)]
        _StarDotMap ("_StarDotMap", 2D) = "white" {}
        [HideInInspector]_StarDotMap_ST ("_StarDotMap_ST", Vector) = (8.00, 6.00, 0.00, 0.00   )
        _StarDotIntensity ("_StarDotIntensity", Range(0, 2000)) = 700.00
        
        [Header(__Misc__)]
        _GalaxyAlpha ("_GalaxyAlpha", Range(0, 1)) = 1.00
    }
    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend DstColor One, DstColor One
            BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"
            #include "Assets/Common/shaderlib/common.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 Varying_UV : TEXCOORD0;
            };
            
            #define _UV_ST float4(1.00, 1.00, 0.00, 0.00)           // _42._m9

            sampler2D _ColorMaskDisturbNoise ;
            sampler2D _ColorMask ;
            sampler2D _CenterBandNoise ;
            sampler2D _BesideBandNoise;
            float4 _BesideBandNoise_ST; // _34._m2
            sampler2D _StarDotMap;
            
            v2f vert (appdata v)
            {
                v2f o;

                float4 Vertex_Position;
                float4 Vertex_UV;
                Vertex_Position = v.vertex;
                Vertex_UV = float4(v.uv, 0, 0);

                float4 _worldPos = mul(UNITY_MATRIX_M, Vertex_Position);

                float4 _clipPos = mul(UNITY_MATRIX_VP, _worldPos);

                o.vertex = _clipPos;
                
                o.Varying_UV = (Vertex_UV.xy * _UV_ST.xy) + _UV_ST.zw;
                
                return o;
            }

            #define _Time float4(84.2623, 1685.24597, 3370.49194, 5055.73779)// _34._m0
            // #define _BesideColor float4(0.00, 0.74265, 0.03585, 1.00               )// _34._m1
            float4 _BesideColor;
            // #define _BesideBandNoise_ST float4(1.00, 0.40, 0.00, 0.00                     )// _34._m2
            // #define _BesideBandNoise_Speed (-0.005) //_34._m3
            float _BesideBandNoise_Speed; //_34._m3
            // #define _BesideBandNoise_Bias (0.38  ) //_34._m4
            float _BesideBandNoise_Bias; // (0.38  ) //_34._m4
            // #define _BesideColor_Intensity (10.82 ) //_34._m5
            float _BesideColor_Intensity; // (10.82 ) //_34._m5
            // #define _ColorMask_ST float4(1.00, 1.00, 0.00, 0.00) // _34._m6
            float4 _ColorMask_ST; // float4(1.00, 1.00, 0.00, 0.00) // _34._m6
            // #define _ColorMaskDisturbNoise_ST float4(3.50, 1.00, 0.00, 0.00) // _34._m7
            float4 _ColorMaskDisturbNoise_ST; // float4(3.50, 1.00, 0.00, 0.00) // _34._m7
            // #define _ColorMaskDisturbNoise_Speed (0.005) // _34._m8
            float _ColorMaskDisturbNoise_Speed; // (0.005) // _34._m8
            // #define _ColorMask_Disturb (0.23 ) // _34._m9
            float _ColorMask_Disturb; // (0.23 ) // _34._m9
            // #define _CenterBandNoise_ST float4(4.35, 1.10, 0.00, 0.00 ) // _34._m10
            float4 _CenterBandNoise_ST; // float4(4.35, 1.10, 0.00, 0.00 ) // _34._m10
            // #define _CenterBandNoise_Bias (-0.05 ) // _34._m11
            float _CenterBandNoise_Bias; // (-0.05 ) // _34._m11
            // #define _CenterBandNoise_Scale (1.60  ) // _34._m12
            float _CenterBandNoise_Scale; // (1.60  ) // _34._m12
            // #define _StarDotIntensity (700.00) // _34._m13
            float _StarDotIntensity; // (700.00) // _34._m13
            // #define _StarDotMap_ST float4(8.00, 6.00, 0.00, 0.00   ) // _34._m14
            float4 _StarDotMap_ST; // float4(8.00, 6.00, 0.00, 0.00   ) // _34._m14
            // #define _CenterColor float4(1.00, 0.00, 0.00, 1.00   ) // _34._m15
            float4 _CenterColor; // float4(1.00, 0.00, 0.00, 1.00   ) // _34._m15
            // #define _BesideColorAddition float4(0.14706, 0.40, 1.00, 1.00) // _34._m16
            float4 _BesideColorAddition; // float4(0.14706, 0.40, 1.00, 1.00) // _34._m16
            // #define _BesideColorAddition_Intensity (5.00 ) // _34._m17
            float _BesideColorAddition_Intensity; // (5.00 ) // _34._m17
            // #define _GalaxyAlpha (0.00 ) // _34._m18
            float _GalaxyAlpha; // (0.00 ) // _34._m18
            // #define _CenterColor_Intensity (18.00) // _34._m19
            float _CenterColor_Intensity; // (18.00) // _34._m19
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 1);
                float4 Output_0;

                float2 _noiseMap1UV = (_Time.yy * (_BesideBandNoise_Speed)) + (i.Varying_UV * _BesideBandNoise_ST.xy) + _BesideBandNoise_ST.zw;

                // _BesideBandNoise
                float _besideBandNoise = tex2D(_BesideBandNoise, _noiseMap1UV).x;

                float3 _besideColor = (_besideBandNoise + _BesideBandNoise_Bias) * _BesideColor.xyz * _BesideColor_Intensity;

                float2 _starDotMapUV = (i.Varying_UV * _StarDotMap_ST.xy) + _StarDotMap_ST.zw;
                float _starDotMapSample = tex2D(_StarDotMap, _starDotMapUV).x;
                
                float _starDot = _starDotMapSample * _StarDotIntensity;

                float2 _noiseMap2AUV = (_Time.yy * (_ColorMaskDisturbNoise_Speed)) + (i.Varying_UV * _ColorMaskDisturbNoise_ST.xy) + _ColorMaskDisturbNoise_ST.zw;
                // _ColorMaskDisturbNoise
                float _colorMaskDisturbNoise = tex2D(_ColorMaskDisturbNoise, _noiseMap2AUV).x;

                float2 _colorMaskUV = (_colorMaskDisturbNoise * _ColorMask_Disturb) + (i.Varying_UV * _ColorMask_ST.xy) + _ColorMask_ST.zw;
                
                float2 _colorMaskRG = tex2D(_ColorMask, _colorMaskUV).xy;
                
                float _starDotR = _starDot * _colorMaskRG.r;

                float2 _noiseMap2BUV = (i.Varying_UV * _CenterBandNoise_ST.xy) + _CenterBandNoise_ST.zw;
                // _CenterBandNoise
                float _centerBandNoise = tex2D(_CenterBandNoise, _noiseMap2BUV).x;

                float _centerBandArea_reverse = lerp(1.0, (_centerBandNoise + _CenterBandNoise_Bias) * _CenterBandNoise_Scale, _colorMaskRG.g);
                
                float _besideBandArea = _centerBandArea_reverse * _colorMaskRG.r;

                float _centerBandArea = 1.0 - _centerBandArea_reverse;

                float3 _besideColor_2 = (_besideColor * _besideBandArea) + (_starDotR);

                float3 _centerColor = _CenterColor.xyz * _CenterColor_Intensity * _centerBandArea;

                float3 _color2 = (_centerColor * _besideBandArea) + _besideColor_2;

                float3 _color3 = (_besideBandArea * _BesideColorAddition.xyz * _BesideColorAddition_Intensity) + _color2;

                // float3 _color4 = lerp(_color3, 0.0, _GalaxyAlpha); // 原神这里原本是 0 表示显示，1 表示隐藏
                float3 _color4 = _color3 * _GalaxyAlpha;
                Output_0.xyz = _color4;
                Output_0.w = 1.0;

                col = Output_0;

                return col;
            }
            ENDCG
        }
    }
}

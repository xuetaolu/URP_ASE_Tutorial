Shader "Unlit/genship_galaxy"
{
    Properties
    {
        _BesideColor  ("_BesideColor ", Color) = (0.00, 0.74265, 0.03585, 1.00               )
        _NoiseMap2A  ("_NoiseMap2A ", 2D) = "white" {}
        _ColorMask  ("_ColorMask ", 2D) = "white" {}
        _NoiseMap2B  ("_NoiseMap2B ", 2D) = "white" {}
        _NoiseMap1 ("_NoiseMap1", 2D) = "white" {}
        _StarDotMap ("_StarDotMap", 2D) = "white" {}
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
                // float3 normal : Normal;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 Varying_UV : TEXCOORD0;
            };

            // static matrix _11__m0__0 = {
            //     4849.17432, -1206.20605, -3321.23096, 0.00 ,
            //     -3361.22192, -3314.02588, -3703.97461, 0.00,
            //     -1089.81433, 4854.10254, -3354.10229, 0.00 ,
            //     -1.82556, 500.00, 3.505, 1.00              ,
            // }; // _StarDotMap._m0
            // static matrix _11__m0__1 = {
            //     0.00013, -0.00009, -0.00003, 0.00 ,
            //     -0.00003, -0.00009, 0.00013, 0.00 ,
            //     -0.00009, -0.0001, -0.00009, 0.00 ,
            //     0.01732, 0.04622, -0.06715, 1.00  ,
            // }; // _StarDotMap._m0


            // #define _42__m0 float4(0.00, 0.08546, 0.00, 0.20387      ) //_42._m0
            // #define _42__m1 float4(0.00, 0.18747, 0.00, 0.36436      ) //_42._m1
            // #define _42__m2 float4(0.00, 0.27045, 0.00, 0.60278      ) //_42._m2
            // #define _42__m3 float4(0.00, 0.00, -0.08331, 0.00        ) //_42._m3
            // #define _42__m4 float4(0.00, 0.00, -0.11274, 0.00        ) //_42._m4
            // #define _42__m5 float4(0.00, 0.00, -0.12677, 0.00        ) //_42._m5
            // #define _42__m6 float4(-0.08331, -0.11274, -0.12677, 1.00) //_42._m6
            // static matrix _42__m7 = {
            //     -1.32406, -0.0001, 0.22249, 0.22247  ,
            //     4.28463E-10, 2.41421, 0.00, 0.00     ,
            //     -0.30169, 0.00045, -0.97502, -0.97494,
            //     -1.35973, -1207.10852, 3.3236, 3.8233,
            // }; // _42._m7
            // #define _42__m8 0 //_42._m8
            #define _UV_ST float4(1.00, 1.00, 0.00, 0.00)           // _42._m9

            sampler2D _NoiseMap2A ;
            sampler2D _ColorMask ;
            sampler2D _NoiseMap2B ;
            sampler2D _NoiseMap1;
            sampler2D _StarDotMap;
            
            v2f vert (appdata v)
            {
                v2f o;
                // o.vertex = UnityObjectToClipPos(v.vertex);

                float4 Vertex_Position;
                // float3 Vertex_Normal;
                float4 Vertex_UV;

                Vertex_Position = v.vertex;
                // Vertex_Normal = v.normal.xyz;
                // Vertex_Normal *= 0;
                Vertex_UV = float4(v.uv, 0, 0);
                
                // float3 _22;
                // float3 _23;
                // float3 _24;
                // float4 _26;
                // uint _28;
                float4 _30;
                // int _33;
                // float4 _34;
                // float4 _35;
                // float4 _36;
                // float3 _38;
                // float3 _39;
                // float4 _49;
                
                // _33 = 0 + 0;
                // _33 = _33 << 3;
                float4 _worldPos; 
                    // _worldPos = Vertex_Position.yyyy * _11__m0__0[1u];
                    // _worldPos = (_11__m0__0[0u] * Vertex_Position.xxxx) + _worldPos;
                    // _worldPos = (_11__m0__0[2u] * Vertex_Position.zzzz) + _worldPos;
                    // _worldPos = _worldPos + _11__m0__0[3u];
                    _worldPos = mul(UNITY_MATRIX_M, Vertex_Position);
                // _36 = _worldPos;
                
                
                // _23 = (_11__m0__0[3u].xyz * Vertex_Position.www) + _34.xyz;
                // _34 = _worldPos.yyyy * _42__m7[1u];
                // _34 = (_42__m7[0u] * _worldPos.xxxx) + _34;
                // _34 = (_42__m7[2u] * _worldPos.zzzz) + _34;
                // float4 gl_Position = (_42__m7[3u] * _worldPos.wwww) + _34;
                float4 _clipPos = mul(UNITY_MATRIX_VP, _worldPos);
                // o.vertex = GlslToDxClipPos(gl_Position);
                o.vertex = _clipPos;
                
                o.Varying_UV = (Vertex_UV.xy * _UV_ST.xy) + _UV_ST.zw;
                // _34.x = dot(Vertex_Normal, _11__m0__1[0u].xyz);
                // _34.y = dot(Vertex_Normal, _11__m0__1[1u].xyz);
                // _34.z = dot(Vertex_Normal, _11__m0__1[2u].xyz);
                // float3 _worldNormal = _34.xyz;

                // // ShadeSH9
                // 但没用，没传到 frag
                // float _worldNormal_length_rcp = 1 / length(_worldNormal.xyz);
                // // _30.x = rsqrt(_30.x);
                // // _30.x = dot(_worldNormal.xyz, _worldNormal.xyz);
                // // _30.x = _worldNormal_length_rcp;
                // float3 _worldNormal_norm = normalize(_worldNormal.xyz);
                // float3 _196 = _worldNormal_norm;
                // _30.xyz = _worldNormal_norm;
                //
                // // _22 = _30.xyz;
                // _38.x = _30.y * _30.y;
                // _38.x = (_30.x * _30.x) + (-_38.x);
                // _35 = _30.yzzx * _30.xyzz;
                // _39.x = dot(_42__m3, _35);
                // _39.y = dot(_42__m4, _35);
                // _39.z = dot(_42__m5, _35);
                // _38 = (_42__m6.xyz * _38.xxx) + _39;
                // _30.w = 1.0;
                // _39.x = dot(_42__m0, _30);
                // _39.y = dot(_42__m1, _30);
                // _39.z = dot(_42__m2, _30);
                // // _24 = _38 + _39;
                // // _26 = (0.0);
                // // _28 = uint(0);
                
                return o;
            }

            #define _Time float4(84.2623, 1685.24597, 3370.49194, 5055.73779)// _34._m0
            // #define _BesideColor float4(0.00, 0.74265, 0.03585, 1.00               )// _34._m1
            float4 _BesideColor;
            #define _NoiseMap1_ST float4(1.00, 0.40, 0.00, 0.00                     )// _34._m2
            #define _NoiseMap1_Speed (-0.005) //_34._m3
            #define _NoiseMap1Sample_Bias (0.38  ) //_34._m4
            #define _BesideColor_Intensity (10.82 ) //_34._m5
            #define _ColorMask_ST float4(1.00, 1.00, 0.00, 0.00) // _34._m6
            #define _NoiseMap2A_ST float4(3.50, 1.00, 0.00, 0.00) // _34._m7
            #define _NoiseMap2A_Speed (0.005) // _34._m8
            #define _ColorMask_Disturb (0.23 ) // _34._m9
            #define _34__m10 float4(4.35, 1.10, 0.00, 0.00 ) // _34._m10
            #define _NoiseMap2B_Bias (-0.05 ) // _34._m11
            #define _NoiseMap2B_Scale (1.60  ) // _34._m12
            #define _StarDotIntensity (700.00) // _34._m13
            #define _StarDotMap_ST float4(8.00, 6.00, 0.00, 0.00   ) // _34._m14
            #define _CenterColor float4(1.00, 0.00, 0.00, 1.00   ) // _34._m15
            #define _BesideColorAddition float4(0.14706, 0.40, 1.00, 1.00) // _34._m16
            #define _BesideColorAddition_Intensity (5.00 ) // _34._m17
            #define _GalaxyHidden (0.00 ) // _34._m18
            #define _CenterColor_Intensity (18.00) // _34._m19
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 1);
                float4 Output_0;
                
                float3 _20;
                float _22;
                float3 _23;
                float2 _25;
                float3 _26;
                float3 _27;
                float2 _28;
                float _29;
                float _30;
                // float _starDotMapSample;
                // float _332;
                // uint _336;
                // float3 _342 = (255.0);
                
                // float2 _56 = (i.Varying_UV * _NoiseMap1_ST.xy) + _NoiseMap1_ST.zw;
                // _20.xy = (i.Varying_UV * _NoiseMap1_ST.xy) + _NoiseMap1_ST.zw;
                // float2 _71 = (_Time.yy * (_NoiseMap1_Speed)) + _20.xy;
                float2 _noiseMap1UV = (_Time.yy * (_NoiseMap1_Speed)) + (i.Varying_UV * _NoiseMap1_ST.xy) + _NoiseMap1_ST.zw;
                // _20 = float3(_71.x, _71.y, _20.z);
                // _20.xy = _noiseMap1UV;
                
                float _noiseMap1Sample = tex2D(_NoiseMap1, _noiseMap1UV).x;
                // _22 = _noiseMap1Sample;
                // _20.x = _noiseMap1Sample + _NoiseMap1Sample_Bias;
                float3 _besideColor = (_noiseMap1Sample + _NoiseMap1Sample_Bias) * _BesideColor.xyz * _BesideColor_Intensity;
                // _20 = _besideColor;
                // _20 *= _BesideColor_Intensity;
                
                // float2 _111 = (i.Varying_UV * _StarDotMap_ST.xy) + _StarDotMap_ST.zw;
                // _23 = float3(_111.x, _111.y, _23.z);
                float2 _starDotMapUV = (i.Varying_UV * _StarDotMap_ST.xy) + _StarDotMap_ST.zw;
                float _starDotMapSample = tex2D(_StarDotMap, _starDotMapUV).x;
                
                float _starDot = _starDotMapSample * _StarDotIntensity;
                // _30 = _starDot;
                // float2 _133 = (i.Varying_UV * _NoiseMap2A_ST.xy) + _NoiseMap2A_ST.zw;
                // _23 = float3(_133.x, _133.y, _23.z);
                // float2 _146 = (_Time.yy * (_NoiseMap2A_Speed)) + _23.xy;
                // _23 = float3(_146.x, _146.y, _23.z);
                float2 _noiseMap2AUV = (_Time.yy * (_NoiseMap2A_Speed)) + (i.Varying_UV * _NoiseMap2A_ST.xy) + _NoiseMap2A_ST.zw;
                float _noiseMap2ASample = tex2D(_NoiseMap2A, _noiseMap2AUV).x;
                // _25.x = _noiseMap2ASample;
                // float2 _164 = (i.Varying_UV * _ColorMask_ST.xy) + _ColorMask_ST.zw;
                // _27 = float3(_164.x, _164.y, _27.z);
                float2 _colorMaskUV = (_noiseMap2ASample * _ColorMask_Disturb) + (i.Varying_UV * _ColorMask_ST.xy) + _ColorMask_ST.zw;
                // _23 = float3(_178.x, _178.y, _23.z);
                
                float2 _colorMaskRG = tex2D(_ColorMask, _colorMaskUV).xy;
                // _25 = _colorMaskRG;
                
                float _starDotR = _starDot * _colorMaskRG.r;
                // _30 = _starDotR;
                
                // _28 = (i.Varying_UV * _34__m10.xy) + _34__m10.zw;
                float2 _noiseMap2BUV = (i.Varying_UV * _34__m10.xy) + _34__m10.zw;
                float _noiseMap2BSample = tex2D(_NoiseMap2B, _noiseMap2BUV).x;
                // _29 = _noiseMap2BSample;
                // _28.x = _noiseMap2BSample + _NoiseMap2B_Bias;
                // _28.x = ((_noiseMap2BSample + _NoiseMap2B_Bias) * _NoiseMap2B_Scale) + (-1.0);
                // _27.x = _colorMaskRG.y * (((_noiseMap2BSample + _NoiseMap2B_Bias) * _NoiseMap2B_Scale) - 1.0 ) + 1.0;

                float _centerBandArea_reverse = lerp(1.0, (_noiseMap2BSample + _NoiseMap2B_Bias) * _NoiseMap2B_Scale, _colorMaskRG.g);
                // _27.x = _centerBandArea_reverse;
                // _27.x = (_colorMaskRG.y * ( + (-1.0))) + 1.0;
                
                
                float _besideBandArea = _centerBandArea_reverse * _colorMaskRG.r;
                // _23.x = _besideBandArea;
                float _centerBandArea = 1.0 - _centerBandArea_reverse;
                // _27.x = _centerBandArea;
                float3 _besideColor_2 = (_besideColor * _besideBandArea) + (_starDotR);
                // _20 = _besideColor_2;
                // _26 = _CenterColor.xyz * _CenterColor_Intensity;
                // _27 = _centerBandArea * _26;
                float3 _centerColor = _CenterColor.xyz * _CenterColor_Intensity * _centerBandArea;
                // _27 = _centerColor;
                float3 _color2 = (_centerColor * _besideBandArea) + _besideColor_2;
                // _20 = _color2;
                // _23 = _besideBandArea * _BesideColorAddition.xyz;
                float3 _color3 = (_besideBandArea * _BesideColorAddition.xyz * _BesideColorAddition_Intensity) + _color2;
                // _20 = _color3;
                // _20 = (float3(_GalaxyHidden, _GalaxyHidden, _GalaxyHidden) * (-_color3)) + _color3;
                // _20 = (_GalaxyHidden * (0.0-_color3)) + _color3;
                float3 _color4 = lerp(_color3, 0.0, _GalaxyHidden);
                Output_0.xyz = _color4;
                Output_0.w = 1.0;
                // Output_0.rgb = LinearToGammaSpace(Output_0.rgb );
                col = Output_0;
                // col.rgb = _centerBandArea_reverse;
                // col.rgb = _besideBandArea;
                // col.rgb = _centerBandArea;
                return col;
            }
            ENDCG
        }
    }
}

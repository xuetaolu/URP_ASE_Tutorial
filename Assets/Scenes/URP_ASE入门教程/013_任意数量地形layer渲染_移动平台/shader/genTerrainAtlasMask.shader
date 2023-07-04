Shader "Unlit/genTerrainAtlasMask"
{
    Properties
    {
        _SplatAlpha1 ("Texture", 2D) = "black" {}
        _SplatAlpha2 ("Texture", 2D) = "black" {}
        _SplatAlpha3 ("Texture", 2D) = "black" {}
        _SplatAlpha4 ("Texture", 2D) = "black" {}
        _LayerCount ("Texture", Int) = 16
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZTest Always ZWrite Off
            
            CGPROGRAM
            

            #define WEIGHT_ARRAY_SIZE 16
            // #define MAIN_WEIGHT_COUNT 2
            
            sampler2D _SplatAlpha1;
            sampler2D _SplatAlpha2;
            sampler2D _SplatAlpha3;
            sampler2D _SplatAlpha4;
            int _LayerCount;
            
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            #define SetWeightsArrayFourAt(weights, startIndex, rgba) \
                weights[startIndex]   = rgba.r; \
                weights[startIndex+1] = rgba.g; \
                weights[startIndex+2] = rgba.b; \
                weights[startIndex+3] = rgba.a; \
            
            #define GetWeightsArrayFourAt(weights, startIndex) \
                float4(weights[startIndex], \
                weights[startIndex+1], \
                weights[startIndex+2], \
                weights[startIndex+3]);

            void Swap(inout float a, inout float b)
            {
                const float tmp = b;
                b = a;
                a = tmp;
            }

            float3 CalcMain2IndexAndRatio(const float weights[WEIGHT_ARRAY_SIZE])
            {
                float3 res = (float3)0;
                
                int maxIndex1 = -1;
                float maxWeight1 = 0.0;
                int maxIndex2 = -1;
                float maxWeight2 = 0.0;

                for (int i = 0; i < WEIGHT_ARRAY_SIZE; i++)
                {
                    const float w = weights[i];
                    if (maxIndex1 < 0 || maxWeight1 < w)
                    {
                        maxWeight1 = w;
                        maxIndex1 = i;
                    }
                }

                for (int i = 0; i < WEIGHT_ARRAY_SIZE; i++)
                {
                    const float w = weights[i];
                    if (i == maxIndex1)
                        continue;
                    if (maxIndex2 < 0 || maxWeight2 < w)
                    {
                        maxWeight2 = w;
                        maxIndex2 = i;
                    }
                }

                res.r = (float)maxIndex1 / (float)(_LayerCount-1);
                res.g = (float)maxIndex2 / (float)(_LayerCount-1);
                const float sumWeight = max( 0.00001, maxWeight1 + maxWeight2);
                res.b = maxWeight2 / sumWeight;
            
                return res;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;

                float weights[WEIGHT_ARRAY_SIZE];
                for (int _i = 0; _i < WEIGHT_ARRAY_SIZE; _i++)
                {
                    weights[_i] = 0;
                }
                
                SetWeightsArrayFourAt(weights,  0,  tex2D(_SplatAlpha1, i.uv));
                SetWeightsArrayFourAt(weights,  4,  tex2D(_SplatAlpha2, i.uv));
                SetWeightsArrayFourAt(weights,  8,  tex2D(_SplatAlpha3, i.uv));
                SetWeightsArrayFourAt(weights, 12,  tex2D(_SplatAlpha4, i.uv));
                
                col.rgb = CalcMain2IndexAndRatio(weights);
                
                return col;
            }
            ENDCG
        }
    }
}

Shader "genship/water_ssr_process"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {  }
        
        
        Pass
        {
            ZWrite On
            ZTest Always
            ColorMask 0
            name "GetFarthestDepth"
            
            HLSLPROGRAM
            sampler2D _CameraDepthTexture;
            float4 _CameraDepthTexture_TexelSize;
            
            #pragma vertex vert
            #pragma fragment frag
            // #pragma enable_d3d11_debug_symbols

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

            fixed frag (v2f i) : SV_Depth
            {
                float2 leftUpUV = i.uv - 0.5*_CameraDepthTexture_TexelSize.xy;
                
                
                #if UNITY_REVERSED_Z
                // DX 版本，depth buffer 最近是1，远处是0
                const float c_farestDepth = 0.0;
                #else
                // glsl 原神版本，depth buffer 最近是0，远处是0
                const float c_farestDepth = 1.0;      
                #endif

                float nearDepth = c_farestDepth;
                
                UNITY_UNROLL
                for (int x = 0; x <= 1; x++)
                {
                    UNITY_UNROLL
                    for (int y = 0; y <= 1; y++)
                    {
                        float currentDepthBuffer = tex2D(_CameraDepthTexture, leftUpUV + _CameraDepthTexture_TexelSize.xy * float2(x,y));
                        #if UNITY_REVERSED_Z
                        // DX 版本，取大的，更近的
                        nearDepth = currentDepthBuffer > nearDepth ? currentDepthBuffer : nearDepth;
                        #else
                        // glsl 原神版本，取小的，更近的
                        nearDepth = currentDepthBuffer < nearDepth ? currentDepthBuffer : nearDepth;
                        #endif
                    }
                }

                return nearDepth;
                // return Linear01Depth(farthestDepth);
            }
            ENDHLSL
        }        
        
        Pass
        {
            ZWrite Off
            ZTest Always
            ColorMask R
            name "Linear01Depth"
            
            HLSLPROGRAM
            sampler2D _CameraDepthTextureDownSize;
            
            #pragma vertex vert
            #pragma fragment frag
            // #pragma enable_d3d11_debug_symbols

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

            float frag (v2f i) : SV_Target
            {
                float farthestDepth = tex2D(_CameraDepthTextureDownSize, i.uv);

                return Linear01Depth(farthestDepth);
            }
            ENDHLSL
        }
        
        Pass
        {
            ZWrite Off
            ZTest Always
            ColorMask RGBA
            name "FillEmptyPixel"
            
            HLSLPROGRAM

            sampler2D _MainTex;
            
            #pragma vertex vert
            #pragma fragment frag
            // #pragma enable_d3d11_debug_symbols

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

            #define MAX_OFFSETS_COUNT 8
            static const float offsets[MAX_OFFSETS_COUNT] = {
                    0.03125, -0.03125,
                    0.0625, -0.0625,
                    0.09375, -0.09375,
                    0.125, -0.125,
                };
            
            float4 frag (v2f input) : SV_Target
            {
                float4 col = tex2D(_MainTex, input.uv);

                // col.r = 0;

                if (col.a >= 0.02)
                    return col;

                UNITY_UNROLL
                for (int i = 0; i < MAX_OFFSETS_COUNT; i++)
                {
                    float offset = offsets[i];
                    float4 tmp_col = tex2D(_MainTex, input.uv + float2(offset, 0)).rgba;
                    if (tmp_col.a >= 0.02)
                    {
                        // col.a = col.a == 0 ? tmp_col.a : col.a;
                        col.a = col.a == 0 ? tmp_col.a : 0;
                        col.rgb = tmp_col.rgb;
                        break;
                    }
                }

                return col;
            }
            ENDHLSL
        }
    }
}

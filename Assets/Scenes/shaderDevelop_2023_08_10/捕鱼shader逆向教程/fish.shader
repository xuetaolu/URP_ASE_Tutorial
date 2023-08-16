Shader "fish"
{
    Properties
    {
        _7 ("_7", 2D) = "white" {}
        _8 ("_8", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : Normal;
                float4 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD2;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 Varying_v4_A : TEXCOORD0;
                float3 Varying_v3_B : TEXCOORD1;
                float2 Varying_v2_C : TEXCOORD2;
                float2 Varying_v2_D : TEXCOORD3;
            };
            

            // static const float4x4 _29__m0 = {-0.99639, 0.0582, 0.06188, 0.00,
            //                         -0.00144, 0.71673, -0.69735, 0.00 ,
            //                         -0.08494, -0.69492, -0.71406, 0.00,
            //                         -6.70942, 4.90415, 144.929, 1.00 }; //_29._m0
            
            // static const float4x4 _29__m1 = {-0.99638, -0.00144, -0.08494, 0.00  , 
            //                         0.0582, 0.71673, -0.69492, 0.00     , 
            //                         0.06188, -0.69735, -0.71406, 0.00   , 
            //                         -15.93918, 97.54115, 106.32565, 1.00};//_29._m1
                                    
            // static const float4x4 _29__m2 = {0.05625, 0.00, 0.00, 0.00 , 
            //                         0.00, 0.10, 0.00, 0.00             , 
            //                         0.00, 0.00, 0.00667, 0.00          , 
            //                         0.00, 0.00, -0.9334, 1.00          };//_29._m2

            #define _29__m0 transpose(mul(UNITY_MATRIX_V, UNITY_MATRIX_M))
            #define _29__m1 transpose(unity_WorldToObject)
            #define _29__m2 transpose(UNITY_MATRIX_P)

            float4 GlslToDxClipPos(float4 clipPos) {
                clipPos.y = -clipPos.y;
                clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
                return clipPos;
            }
            
            v2f vert (appdata v)
            {
                float4 Vertex_Position = v.vertex;
                float3 Vertex_Normal = v.normal;
                float2 Vertex_UV = v.uv;
                float2 Vertex_UV2 = v.uv2;

                v2f o;
                
                float4 _20;
                float4 _21;
                float _23;
                float4 _37;
                
                _20 = Vertex_Position.yyyy * _29__m0[1u];
                _20 = (_29__m0[0u] * Vertex_Position.xxxx) + _20;
                _20 = (_29__m0[2u] * Vertex_Position.zzzz) + _20;
                _21 = _20 + _29__m0[3u];
                o.Varying_v4_A = (_29__m0[3u] * Vertex_Position.wwww) + _20;
                _20 = _21.yyyy * _29__m2[1u];
                _20 = (_29__m2[0u] * _21.xxxx) + _20;
                _20 = (_29__m2[2u] * _21.zzzz) + _20;
                // gl_Position = (_29__m2[3u] * _21.wwww) + _20;
                // o.vertex = GlslToDxClipPos((_29__m2[3u] * _21.wwww) + _20);
                o.vertex = ((_29__m2[3u] * _21.wwww) + _20);
                o.Varying_v2_C = Vertex_UV;
                o.Varying_v2_D = Vertex_UV2;
                _20.x = dot(Vertex_Normal, _29__m1[0u].xyz);
                _20.y = dot(Vertex_Normal, _29__m1[1u].xyz);
                _20.z = dot(Vertex_Normal, _29__m1[2u].xyz);
                _23 = dot(_20.xyz, _20.xyz);
                _23 = rsqrt(_23);
                o.Varying_v3_B = float3(_23, _23, _23) * _20.xyz;
                
                
                // o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            #define _32__m0  float4(3.01219, 60.2438, 120.48759, 180.73138) // _32._m0 
            #define _32__m1  float3(0.00, 0.00, -10.00)                     // _32._m1 
            #define _32__m2  float4(0.00, 0.00, 0.00, 0.00)               // _32._m2 
            #define _32__m3  float4(0.50, 0.50, 0.50, 0.50)               // _32._m3 
            #define _32__m4  float4(0.00, 0.00, 0.00, 0.00)               // _32._m4 
            #define _32__m5  float4(0.00, 0.00, 0.00, 0.00)               // _32._m5 
            #define _32__m6  float4(0.78676, 0.74176, 0.71734, 1.00)      // _32._m6 
            #define _32__m7  float4(1.00, 1.00, 0.00, 0.00         )      // _32._m7 
            #define _32__m8  float4(1.77, 1.00, -1.34, 0.00        )        // _32._m8 
            #define _32__m9  1.00                                   // _32._m9 
            #define _32__m10 1.92                                   // _32._m10
            #define _32__m11 0.52                                   // _32._m11

            sampler2D _7;
            sampler2D _8;
            
            fixed4 frag (v2f i) : SV_Target
            {
                float4 _20;
                float3 _22;
                float3 _23;
                float3 _24;
                float3 _25;
                float2 _27;
                float _29;
                
                float3 _55 = (-i.Varying_v4_A.xyz) + _32__m1;
                _20 = float4(_55.x, _55.y, _55.z, _20.w);
                _29 = dot(_20.xyz, _20.xyz);
                _29 = rsqrt(_29);
                _22.x = dot(_32__m2.xyz, _32__m2.xyz);
                _22.x = rsqrt(_22.x);
                _22 = _22.xxx * _32__m2.xyz;
                float3 _93 = (_20.xyz * _29.xxx) + _22;
                _20 = float4(_93.x, _93.y, _93.z, _20.w);
                _29 = dot(_20.xyz, _20.xyz);
                _29 = rsqrt(_29);
                float3 _107 = _29.xxx * _20.xyz;
                _20 = float4(_107.x, _107.y, _107.z, _20.w);
                _29 = dot(i.Varying_v3_B, i.Varying_v3_B);
                _29 = rsqrt(_29);
                _23 = _29.xxx * i.Varying_v3_B;
                _20.x = dot(_20.xyz, _23);
                _20.y = dot(_23, _22);
                float2 _131 = max(_20.xy, (0.0).xx);
                _20 = float4(_131.x, _131.y, _20.z, _20.w);
                _20.x = log2(_20.x);
                _27.x = _32__m0.y + _32__m5.y;
                _27.x *= _32__m10;
                _27 = (_27.xx * float2(0.0, 0.100000001490116119384765625)) + i.Varying_v2_D;
                _27 = (_27 * _32__m8.xy) + _32__m8.zw;
                _27.x = tex2D(_7, _27).w;
                _27.y = _27.x * _32__m11;
                _27 *= (_32__m9).xx;
                _29 = (_27.y * 10.0) + 1.0;
                _29 = exp2(_29);
                _20.x *= _29;
                _20.x = exp2(_20.x);
                _23 = _20.xxx * _32__m4.xyz;
                float3 _215 = _27.xxx * _23;
                _20 = float4(_215.x, _20.y, _215.y, _215.z);
                float2 _227 = (i.Varying_v2_C * _32__m7.xy) + _32__m7.zw;
                _23 = float3(_227.x, _227.y, _23.z);
                _24 = tex2D(_8, _23.xy).xyz;
                _23 = _24 * _32__m6.xyz;
                _22 = _32__m3.xyz + _32__m3.xyz;
                _23 = _22 * _23;
                _25 = (_20.yyy * _32__m4.xyz) + _22;
                float3 _265 = (_25 * _23) + _20.xzw;
                // _43[0u] = float4(_265.x, _265.y, _265.z, _43[0u].w);
                // _43[0u].w = 1.0;
                fixed4 col = fixed4(0,0,0,1);
                col.rgb = _265.xyz;
                col.a = 1.0;
                return col;
            }
            ENDCG
        }
    }
}

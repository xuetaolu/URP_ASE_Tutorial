Shader "Unlit/lion_fish"
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {

                float4 vertex : SV_POSITION;
                float4 Varying_worldPos : TEXCOORD0;
                float3 Varying_1 : TEXCOORD1;
                float2 Varying_UV : TEXCOORD2;
                float2 Varying_UV2 : TEXCOORD3;
            };

            float4 GlslToDxClipPos(float4 clipPos) {
                clipPos.y = -clipPos.y;
                clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
                return clipPos;
            }

            // #define _29__m0 _29._m0
            // #define _29__m1 _29._m1
            // #define _29__m2 _29._m2

            
            #define _32__m0  float4(3.01219, 60.2438, 120.48759, 180.73138) // _32._m0
            #define _32__m1  float3(0.00, 0.00, -10.00)                     // _32._m1
            #define _32__m2  float4(0.00, 0.00, 0.00, 0.00       )          // _32._m2
            #define _32__m3  float4(0.50, 0.50, 0.50, 0.50       )          // _32._m3
            #define _32__m4  float4(0.00, 0.00, 0.00, 0.00       )          // _32._m4
            #define _32__m5  float4(0.00, 0.00, 0.00, 0.00       )          // _32._m5
            #define _32__m6  float4(1.00, 0.86207, 0.85294, 1.00 )          // _32._m6
            #define _32__m7  float4(1.00, 1.00, 0.00, 0.00       )          // _32._m7
            #define _32__m8  float4(2.50, 2.50, 0.00, 0.00       )          // _32._m8
            #define _32__m9  1.00                                   // _32._m9
            #define _32__m10 1.46                                   // _32._m10
            #define _32__m11 0.35                                   // _32._m11

            sampler2D _7;
            sampler2D _8;
            
            float4 _20;
            float4 _21;
            float _23;
            float4 _37;
            // UNITY_MATRIX_M
            static matrix _29__m0 = {0.66436, -0.5285, -0.5285, 0.00,
                                     0.01304, 0.7152, -0.6988, 0.00 ,
                                     0.7473, 0.45736, 0.48205, 0.00 ,
                                     -5.80, -5.47501, 18.00, 1.00   ,};

            // UNITY_MATRIX_V
            static matrix _29__m1 = {0.66436, 0.01304, 0.7473, 0.00    ,
                                     -0.5285, 0.7152, 0.45736, 0.00    ,
                                     -0.5285, -0.6988, 0.48205, 0.00   ,
                                     10.47275, 16.56977, -1.83844, 1.00,};

            
            // UNITY_MATRIX_VP
            static matrix _29__m2 = {0.05625, 0.00, 0.00, 0.00,
                                     0.00, 0.10, 0.00, 0.00   ,
                                     0.00, 0.00, 0.00667, 0.00,
                                     0.00, 0.00, -0.9334, 1.00,};
            
            v2f vert (appdata v)
            {
                float4 Vertex_Position = v.vertex;
                float3 Vertex_Normal = v.normal;
                float2 Vertex_2 = v.uv;
                float2 Vertex_3 = v.uv2;
                
                v2f o;
                // o.vertex = UnityObjectToClipPos(v.vertex);
                _20 = Vertex_Position.yyyy * _29__m0[1u];
                _20 = (_29__m0[0u] * Vertex_Position.xxxx) + _20;
                _20 = (_29__m0[2u] * Vertex_Position.zzzz) + _20;
                float4 _worldPos = _29__m0[3u] * Vertex_Position.wwww + _20;
                // _21 = _worldPos;

                // o.Varying_worldPos = (_29__m0[3u] * Vertex_Position.wwww) + _20;
                o.Varying_worldPos = _worldPos;
                _20 = _worldPos.yyyy * _29__m2[1u];
                _20 = (_29__m2[0u] * _worldPos.xxxx) + _20;
                _20 = (_29__m2[2u] * _worldPos.zzzz) + _20;
                // gl_Position = (_29__m2[3u] * _worldPos.wwww) + _20;
                o.vertex = (_29__m2[3u] * _worldPos.wwww) + _20;
                o.vertex = GlslToDxClipPos(o.vertex);
                o.Varying_UV = Vertex_2;
                o.Varying_UV2 = Vertex_3;
                _20.x = dot(Vertex_Normal, _29__m1[0u].xyz);
                _20.y = dot(Vertex_Normal, _29__m1[1u].xyz);
                _20.z = dot(Vertex_Normal, _29__m1[2u].xyz);
                
                _23 = dot(_20.xyz, _20.xyz);
                _23 = rsqrt(_23);
                o.Varying_1 = (_23) * _20.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = half4(0, 0, 0, 1);
                
                float4 _20;
                float3 _22;
                float3 _23;
                float3 _24;
                float3 _25;
                float2 _27;
                float _29;

                float3 _55 = (-i.Varying_worldPos.xyz) + _32__m1;
                _20 = float4(_55.x, _55.y, _55.z, _20.w);
                _29 = dot(_20.xyz, _20.xyz);
                _29 = rsqrt(_29);
                _22.x = dot(_32__m2.xyz, _32__m2.xyz);
                _22.x = rsqrt(_22.x);
                _22 = _22.xxx * _32__m2.xyz;
                float3 _93 = (_20.xyz * (_29)) + _22;
                _20 = float4(_93.x, _93.y, _93.z, _20.w);
                _29 = dot(_20.xyz, _20.xyz);
                _29 = rsqrt(_29);
                float3 _107 = (_29) * _20.xyz;
                _20 = float4(_107.x, _107.y, _107.z, _20.w);
                _29 = dot(i.Varying_1, i.Varying_1);
                _29 = rsqrt(_29);
                _23 = (_29) * i.Varying_1;
                _20.x = dot(_20.xyz, _23);
                _20.y = dot(_23, _22);
                float2 _131 = max(_20.xy, (0.0));
                _20 = float4(_131.x, _131.y, _20.z, _20.w);
                _20.x = log2(_20.x);
                _27.x = _32__m0.y + _32__m5.y;
                _27.x *= _32__m10;
                _27 = (_27.xx * float2(0.0, 0.100000001490116119384765625)) + i.Varying_UV2;
                _27 = (_27 * _32__m8.xy) + _32__m8.zw;
                _27.x = tex2D(_7, _27).w;
                _27.y = _27.x * _32__m11;
                _27 *= (_32__m9);
                _29 = (_27.y * 10.0) + 1.0;
                _29 = exp2(_29);
                _20.x *= _29;
                _20.x = exp2(_20.x);
                _23 = _20.xxx * _32__m4.xyz;
                float3 _215 = _27.xxx * _23;
                _20 = float4(_215.x, _20.y, _215.y, _215.z);
                float2 _227 = (i.Varying_UV * _32__m7.xy) + _32__m7.zw;
                _23 = float3(_227.x, _227.y, _23.z);
                _24 = tex2D(_8, _23.xy).xyz;
                _23 = _24 * _32__m6.xyz;
                _22 = _32__m3.xyz + _32__m3.xyz;
                _23 = _22 * _23;
                _25 = (_20.yyy * _32__m4.xyz) + _22;
                float3 _265 = (_25 * _23) + _20.xzw;
                // _43[0u] = float4(_265.x, _265.y, _265.z, _43[0u].w);
                // _43[0u].w = 1.0;
                // col = float4(_265.x, _265.y, _265.z, col.w);
                col.rgb = _265.rgb;
                col.w = 1.0;
                
                return col;
            }
            ENDCG
        }
    }
}

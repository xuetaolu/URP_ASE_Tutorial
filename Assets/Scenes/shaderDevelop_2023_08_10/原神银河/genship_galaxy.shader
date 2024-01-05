Shader "Unlit/genship_galaxy"
{
    Properties
    {
        _7  ("_7 ", 2D) = "white" {}
        _8  ("_8 ", 2D) = "white" {}
        _9  ("_9 ", 2D) = "white" {}
        _10 ("_10", 2D) = "white" {}
        _11 ("_11", 2D) = "white" {}
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
                float3 normal : Normal;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 Varying_0 : TEXCOORD0;
            };

            static matrix _11__m0__0 = {
                4849.17432, -1206.20605, -3321.23096, 0.00 ,
                -3361.22192, -3314.02588, -3703.97461, 0.00,
                -1089.81433, 4854.10254, -3354.10229, 0.00 ,
                -1.82556, 500.00, 3.505, 1.00              ,
            }; // _11._m0
            static matrix _11__m0__1 = {
                0.00013, -0.00009, -0.00003, 0.00 ,
                -0.00003, -0.00009, 0.00013, 0.00 ,
                -0.00009, -0.0001, -0.00009, 0.00 ,
                0.01732, 0.04622, -0.06715, 1.00  ,
            }; // _11._m0


            #define _42__m0 float4(0.00, 0.08546, 0.00, 0.20387      ) //_42._m0
            #define _42__m1 float4(0.00, 0.18747, 0.00, 0.36436      ) //_42._m1
            #define _42__m2 float4(0.00, 0.27045, 0.00, 0.60278      ) //_42._m2
            #define _42__m3 float4(0.00, 0.00, -0.08331, 0.00        ) //_42._m3
            #define _42__m4 float4(0.00, 0.00, -0.11274, 0.00        ) //_42._m4
            #define _42__m5 float4(0.00, 0.00, -0.12677, 0.00        ) //_42._m5
            #define _42__m6 float4(-0.08331, -0.11274, -0.12677, 1.00) //_42._m6
            static matrix _42__m7 = {
                -1.32406, -0.0001, 0.22249, 0.22247  ,
                4.28463E-10, 2.41421, 0.00, 0.00     ,
                -0.30169, 0.00045, -0.97502, -0.97494,
                -1.35973, -1207.10852, 3.3236, 3.8233,
            }; // _42._m7
            #define _42__m8 0 //_42._m8
            #define _42__m9 float4(1.00, 1.00, 0.00, 0.00)           // _42._m9

            sampler2D _7 ;
            sampler2D _8 ;
            sampler2D _9 ;
            sampler2D _10;
            sampler2D _11;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 Vertex_Position;
                float3 Vertex_1;
                float4 Vertex_2;

                Vertex_Position = v.vertex;
                Vertex_1 = v.normal.xyz;
                Vertex_1 *= 0;
                Vertex_2 = float4(v.uv, 0, 0);
                
                float3 _22;
                float3 _23;
                float3 _24;
                float4 _26;
                // uint _28;
                float4 _30;
                int _33;
                float4 _34;
                float4 _35;
                float4 _36;
                float3 _38;
                float3 _39;
                // float4 _49;
                
                _33 = 0 + 0;
                _33 = _33 << 3;
                _34 = Vertex_Position.yyyy * _11__m0__0[1u];
                _34 = (_11__m0__0[0u] * Vertex_Position.xxxx) + _34;
                _34 = (_11__m0__0[2u] * Vertex_Position.zzzz) + _34;
                _36 = _34 + _11__m0__0[3u];
                _23 = (_11__m0__0[3u].xyz * Vertex_Position.www) + _34.xyz;
                _34 = _36.yyyy * _42__m7[1u];
                _34 = (_42__m7[0u] * _36.xxxx) + _34;
                _34 = (_42__m7[2u] * _36.zzzz) + _34;
                float4 gl_Position = (_42__m7[3u] * _36.wwww) + _34;
                o.vertex = GlslToDxClipPos(gl_Position);
                
                o.Varying_0 = (Vertex_2.xy * _42__m9.xy) + _42__m9.zw;
                _34.x = dot(Vertex_1, _11__m0__1[0u].xyz);
                _34.y = dot(Vertex_1, _11__m0__1[1u].xyz);
                _34.z = dot(Vertex_1, _11__m0__1[2u].xyz);
                _30.x = dot(_34.xyz, _34.xyz);
                _30.x = rsqrt(_30.x);
                float3 _196 = _30.xxx * _34.xyz;
                _30 = float4(_196.x, _196.y, _196.z, _30.w);
                _22 = _30.xyz;
                _38.x = _30.y * _30.y;
                _38.x = (_30.x * _30.x) + (-_38.x);
                _35 = _30.yzzx * _30.xyzz;
                _39.x = dot(_42__m3, _35);
                _39.y = dot(_42__m4, _35);
                _39.z = dot(_42__m5, _35);
                _38 = (_42__m6.xyz * _38.xxx) + _39;
                _30.w = 1.0;
                _39.x = dot(_42__m0, _30);
                _39.y = dot(_42__m1, _30);
                _39.z = dot(_42__m2, _30);
                _24 = _38 + _39;
                _26 = (0.0);
                // _28 = uint(0);
                
                return o;
            }

            #define _Time float4(84.2623, 1685.24597, 3370.49194, 5055.73779)// _34._m0
            #define _34__m1 float4(0.00, 0.74265, 0.03585, 1.00               )// _34._m1
            #define _34__m2 float4(1.00, 0.40, 0.00, 0.00                     )// _34._m2
            #define _34__m3 (-0.005) //_34._m3
            #define _34__m4 (0.38  ) //_34._m4
            #define _34__m5 (10.82 ) //_34._m5
            #define _34__m6 float4(1.00, 1.00, 0.00, 0.00) // _34._m6
            #define _34__m7 float4(3.50, 1.00, 0.00, 0.00) // _34._m7
            #define _34__m8 (0.005) // _34._m8
            #define _34__m9 (0.23 ) // _34._m9
            #define _34__m10 float4(4.35, 1.10, 0.00, 0.00 ) // _34._m10
            #define _34__m11 (-0.05 ) // _34._m11
            #define _34__m12 (1.60  ) // _34._m12
            #define _34__m13 (700.00) // _34._m13
            #define _34__m14 float4(8.00, 6.00, 0.00, 0.00   ) // _34._m14
            #define _34__m15 float4(1.00, 0.00, 0.00, 1.00   ) // _34._m15
            #define _34__m16 float4(0.14706, 0.40, 1.00, 1.00) // _34._m16
            #define _34__m17 (5.00 ) // _34._m17
            #define _34__m18 (0.00 ) // _34._m18
            #define _34__m19 (18.00) // _34._m19
            
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
                float _31;
                float _332;
                uint _336;
                float3 _342 = (255.0);
                
                float2 _56 = (i.Varying_0 * _34__m2.xy) + _34__m2.zw;
                _20 = float3(_56.x, _56.y, _20.z);
                float2 _71 = (_Time.yy * (_34__m3)) + _20.xy;
                _20 = float3(_71.x, _71.y, _20.z);
                _22 = tex2D(_10, _20.xy).x;
                _20.x = _22 + _34__m4;
                _20 = _20.xxx * _34__m1.xyz;
                _20 *= float3(_34__m5, _34__m5, _34__m5);
                float2 _111 = (i.Varying_0 * _34__m14.xy) + _34__m14.zw;
                _23 = float3(_111.x, _111.y, _23.z);
                _31 = tex2D(_11, _23.xy).x;
                _30 = _31 * _34__m13;
                float2 _133 = (i.Varying_0 * _34__m7.xy) + _34__m7.zw;
                _23 = float3(_133.x, _133.y, _23.z);
                float2 _146 = (_Time.yy * (_34__m8)) + _23.xy;
                _23 = float3(_146.x, _146.y, _23.z);
                _25.x = tex2D(_7, _23.xy).x;
                float2 _164 = (i.Varying_0 * _34__m6.xy) + _34__m6.zw;
                _27 = float3(_164.x, _164.y, _27.z);
                float2 _178 = (_25.xx * float2(_34__m9, _34__m9)) + _27.xy;
                _23 = float3(_178.x, _178.y, _23.z);
                _25 = tex2D(_8, _23.xy).xy;
                _30 *= _25.x;
                _28 = (i.Varying_0 * _34__m10.xy) + _34__m10.zw;
                _29 = tex2D(_9, _28).x;
                _28.x = _29 + _34__m11;
                _28.x = (_28.x * _34__m12) + (-1.0);
                _27.x = (_25.y * _28.x) + 1.0;
                _23.x = _27.x * _25.x;
                _27.x = (-_27.x) + 1.0;
                _20 = (_20 * _23.xxx) + (_30);
                _26 = _34__m15.xyz * float3(_34__m19, _34__m19, _34__m19);
                _27 = _27.xxx * _26;
                _20 = (_27 * _23.xxx) + _20;
                _23 = _23.xxx * _34__m16.xyz;
                _20 = (_23 * (_34__m17)) + _20;
                _20 = (float3(_34__m18, _34__m18, _34__m18) * (-_20)) + _20;
                Output_0 = float4(_20.x, _20.y, _20.z, Output_0.w);
                Output_0.w = 1.0;
                // Output_0.rgb = LinearToGammaSpace(Output_0.rgb );
                col = Output_0;
                return col;
            }
            ENDCG
        }
    }
}

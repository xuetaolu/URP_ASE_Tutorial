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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 Varying_0 : TEXCOORD0;
                float4 Varying_1 : TEXCOORD1;
                float4 Varying_2 : TEXCOORD2;
                float4 Varying_3 : TEXCOORD3;
            };
            // #define _9__m0 _9._m0
            #define _53__m0 float4(1.00, 0.25, 6000.00, 0.00017) // _53._m0
            // static const matrix _53__m1 = {float4(0.85884, -0.83339, -0.69361, -0.69355      ),
            //                                 float4(0.00033, 2.16144, -0.44611, -0.44607       ),
            //                                 float4(1.05192, 0.67975, 0.56574, 0.56569         ),
            //                                 float4(-25.82479, -555.03705, -13.64874, -13.14762)}; // _53._m1
            #define _53__m1 transpose(UNITY_MATRIX_VP)
            #define _53__m2 0 // _53._m2
            static const matrix _53__m3 = {float4(1.00, 0.00, 0.00, 0.00            ),
                                            float4(0.00, 1.00, 0.00, 0.00            ),
                                            float4(0.00, 0.00, 1.00, 0.00            ),
                                            float4(-910.36584, 0.00, -781.65784, 1.00)};// _53._m3
            static const matrix _53__m4 = {float4(1.00, 0.00, 0.00, 0.00          ),
                                            float4(0.00, 1.00, 0.00, 0.00          ),
                                            float4(0.00, 0.00, 1.00, 0.00          ),
                                            float4(910.36584, 0.00, 781.65784, 1.00)};//_53._m4
            #define _53__m5 float4(513.00, 513.00, 2.00, 2.00) // _53._m5
            #define _53__m6 float4(0.00, 0.00, 512.00, 512.00) // _53._m6
            
            // #define _LocalTerrainOffset float4(416.00, 432.00, 1.00, 0.00)
            // #define _LocalTerrainOffset float4(400.00, 432.00, 1.00, 0.00)
            // #define _LocalTerrainOffset float4(384.00, 432.00, 1.00, 0.00)
            float4 _LocalTerrainOffset;
            #define _9__m0__3__m1 float4(0.00, 0.00, 0.00, 0.00)

            UNITY_DECLARE_TEX2D(_VS_res13);
            // float2 _VS_res13_range;
            UNITY_DECLARE_TEX2D(_VS_res14);
            // float _debug_value;

            float4 GlslToDxClipPos(float4 clipPos) {
                clipPos.y = -clipPos.y;
                clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
                return clipPos;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                float4 Vertex_Position = v.vertex;
                float4 Vertex_1 = float4(v.uv, v.uv2);
                
                float4 _23;
                // uint _25;
                float4 _27;
                bool4 _31;
                float3 _34;
                int _37;
                float4 _38;
                float4 _39;
                float4 _40;
                float3 _41;
                float3 _42;
                float2 _45;
                float2 _46;
                float _48;
                float4 _60;
                _27 = Vertex_1.zzzz * float4(0.5, 0.25, 0.125, 0.0625);
                _27 = frac(_27);
                _31 = _27 >= (0.5);
                _27 = lerp((0.0), (1.0), (_31));
                // _37 = gl_InstanceIndex + _53__m2;
                // _37 = _37 << 1;
                _27 *= _9__m0__3__m1;
                _27 *= (0.5);
                _27 = frac(_27);
                _31 = _27 >= (0.5);
                _31.x = _31.z || _31.x;
                _31.y = _31.w || _31.y;
                _31.x = _31.y || _31.x;
                _27.x = float(_31.x);
                float2 _149 = (Vertex_1.xy * _27.xx) + Vertex_Position.xz;
                _27 = float4(_149.x, _149.y, _27.z, _27.w);
                float2 _159 = _27.xy + _LocalTerrainOffset.xy;
                _27 = float4(_159.x, _159.y, _27.z, _27.w);
                _45 = (_27.xy * _LocalTerrainOffset.zz) + (-_53__m6.xy);
                float2 _183 = _27.xy * _LocalTerrainOffset.zz;
                _27 = float4(_183.x, _183.y, _27.z, _27.w);
                _45 += (0.5);
                _45 = float2(_45.x / _53__m6.z, _45.y / _53__m6.w);
                _34.x = UNITY_SAMPLE_TEX2D_LOD(_VS_res13, _45, 0.0).x;
                _45 = UNITY_SAMPLE_TEX2D_LOD(_VS_res14, _45, 0.0).xy;
                _45 = (_45 * (4.0)) + (-2.0);
                _38 = _34.xxxx * _53__m3[1u];
                _34 = _34.xxx * _53__m3[1u].xyz;
                float2 _239 = float2(_27.x * _53__m5.z, _27.y * _53__m5.w);
                _39 = float4(_239.x, _239.y, _39.z, _39.w);
                _38 = (_53__m3[0u] * _39.xxxx) + _38;
                _38 = (_53__m3[2u] * _39.yyyy) + _38;
                _38 += _53__m3[3u];
                // _38.y += _debug_value;
                _40 = _38.yyyy * _53__m1[1u];
                _40 = (_53__m1[0u] * _38.xxxx) + _40;
                _40 = (_53__m1[2u] * _38.zzzz) + _40;
                _38 = (_53__m1[3u] * _38.wwww) + _40;
                // gl_Position = _38;
                // o.vertex = GlslToDxClipPos(_38);
                o.vertex = _38;
                _46 = _53__m5.xy + (-1.0);
                float2 _297 = _27.xy / _46;
                o.Varying_0 = float4(_297.x, _297.y, o.Varying_0.z, o.Varying_0.w);
                o.Varying_0 = float4(o.Varying_0.x, o.Varying_0.y, (0.0), (0.0));
                _34 = (_53__m3[0u].xyz * _39.xxx) + _34;
                _34 = (_53__m3[2u].xyz * _39.yyy) + _34;
                _34 += _53__m3[3u].xyz;
                o.Varying_1.w = _34.x;
                _27.x = dot(_45, _45);
                float2 _338 = ((-_27.xx) * float2(0.25, 0.5)) + (1.0);
                _39 = float4(_39.x, _39.y, _338.x, _338.y);
                _27.x = sqrt(_39.z);
                float2 _348 = _27.xx * _45;
                _39 = float4(_348.x, _348.y, _39.z, _39.w);
                _27.y = dot(_39.xyw, _53__m4[0u].xyz);
                _27.z = dot(_39.xyw, _53__m4[1u].xyz);
                _27.x = dot(_39.xyw, _53__m4[2u].xyz);
                _48 = dot(_27.xyz, _27.xyz);
                _48 = rsqrt(_48);
                float3 _383 = (_48) * _27.xyz;
                _27 = float4(_383.x, _383.y, _383.z, _27.w);
                o.Varying_1.z = _27.y;
                _41 = _27.yzx * float3(1.0, 0.0, 0.0);
                _41 = (_27.xyz * float3(0.0, 0.0, 1.0)) + (-_41);
                _42 = _27.xyz * _41;
                _42 = (_27.zxy * _41.yzx) + (-_42);
                o.Varying_1.y = -_42.x;
                o.Varying_1.x = _41.z;
                o.Varying_2.x = _41.x;
                o.Varying_2.z = _27.z;
                o.Varying_3.z = _27.x;
                o.Varying_2.w = _34.y;
                o.Varying_3.w = _34.z;
                o.Varying_2.y = -_42.y;
                o.Varying_3.y = -_42.z;
                o.Varying_3.x = 0.0;
                _27.x = _38.y * _53__m0.x;
                _27.w = _27.x * 0.5;
                float2 _455 = _38.xw * (0.5);
                _27 = float4(_455.x, _27.y, _455.y, _27.w);
                _23 = float4(_23.x, _23.y, _38.zw.x, _38.zw.y);
                float2 _466 = _27.zz + _27.xw;
                _23 = float4(_466.x, _466.y, _23.z, _23.w);
                
                
                // o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            #define _110__m0  float4(1934.36584, 0.00, -1266.34216, 0.00) // _110._m0
            #define _110__m1  0.00 // _110._m1
            #define _110__m2  float4(0.7098, 0.7098, 0.7098, 0.82745) // _110._m2
            #define _110__m3  float4(0.13149, 0.38199, 0.55882, 1.00) // _110._m3
            #define _110__m4  float4(0.00, 1.00, 0.00, 5.00         ) // _110._m4
            #define _110__m5  float4(10.40, 0.50, 0.02, 12.00       ) // _110._m5
            #define _110__m6  float4(0.10, 0.03333, 80.00, 0.50     ) // _110._m6
            #define _110__m7  8.00  // _110._m7
            #define _110__m8  8.00  // _110._m8
            #define _110__m9  14.00 // _110._m9
            #define _110__m10 6.00  // _110._m10
            #define _110__m11 15.00 // _110._m11
            #define _110__m12 15.00 // _110._m12
            #define _110__m13 15.00 // _110._m13
            #define _110__m14 float4(1.00, 1.00, 1.00, 1.00) // _110._m14
            #define _110__m15 float4(1.00, 1.00, 1.00, 1.00) // _110._m15
            #define _110__m16 float4(1.00, 1.00, 1.00, 1.00) // _110._m16
            #define _110__m17 float4(1.00, 1.00, 1.00, 1.00) // _110._m17
            #define _110__m18 float4(1.00, 1.00, 1.00, 1.00) // _110._m18
            #define _110__m19 float4(1.00, 1.00, 1.00, 1.00) // _110._m19
            #define _110__m20 float4(1.00, 1.00, 1.00, 1.00) // _110._m20
            #define _110__m21 1.00 // _110._m21
            #define _110__m22 1.00 // _110._m22
            #define _110__m23 1.00 // _110._m23
            #define _110__m24 1.00 // _110._m24
            #define _110__m25 0.00 // _110._m25
            #define _110__m26 0.00 // _110._m26
            #define _110__m27 0.00 // _110._m27
            #define _110__m28 0.00 // _110._m28
            #define _110__m29 1.00 // _110._m29
            #define _110__m30 1.00 // _110._m30
            #define _110__m31 1.00 // _110._m31
            #define _110__m32 1.00 // _110._m32
            #define _110__m33 1.00 // _110._m33
            #define _110__m34 1.00 // _110._m34
            #define _110__m35 1.00 // _110._m35
            #define _110__m36 0.00 // _110._m36
            #define _110__m37 0.00 // _110._m37
            #define _110__m38 0.00 // _110._m38
            #define _110__m39 0.00 // _110._m39
            #define _110__m40 1.00 // _110._m40
            #define _110__m41 0.00 // _110._m41
            #define _110__m42 0.00 // _110._m42

            UNITY_DECLARE_TEX2D(_7 );
            UNITY_DECLARE_TEX2D(_8 );
            UNITY_DECLARE_TEX2D(_9 );
            UNITY_DECLARE_TEX2D(_10);
            UNITY_DECLARE_TEX2D(_11);
            UNITY_DECLARE_TEX2D(_12);
            UNITY_DECLARE_TEX2D(_13);
            UNITY_DECLARE_TEX2D(_14);
            UNITY_DECLARE_TEX2D(_15);
            UNITY_DECLARE_TEX2D(_16);
            UNITY_DECLARE_TEX2D(_17);
            UNITY_DECLARE_TEX2D(_18);
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 _31;
                float4 _33;
                float4 _34;
                float4 _35;
                float4 _36;
                float4 _37;
                float4 _38;
                float2 _39;
                float4 _40;
                bool4 _44;
                float3 _47;
                float4 _48;
                float2 _49;
                float4 _50;
                bool _52;
                float _54;
                float _55;
                float3 _56;
                float3 _57;
                float3 _58;
                float3 _59;
                float _60;
                bool2 _63;
                float3 _64;
                float2 _65;
                float _66;
                float3 _67;
                float3 _68;
                float _69;
                float4 _70;
                float _71;
                bool3 _74;
                float2 _75;
                float _76;
                bool2 _77;
                float3 _78;
                float3 _79;
                float _80;
                float3 _81;
                float3 _82;
                float _83;
                float _84;
                bool _85;
                float _86;
                bool _87;
                float3 _88;
                bool _89;
                float _90;
                float _91;
                bool2 _92;
                bool _93;
                float _94;
                float _95;
                float2 _96;
                float _97;
                float _98;
                float _99;
                float _100;
                float _101;
                float _102;
                float _103;
                float _104;
                float _105;
                float _106;
                float _107;
                // float _1151;
                // uint _1155;
                // float3 _1161 = float3(255.0);
                // uint _1201;
                // float3 _1203 = float3(255.0);
                // uint _1236;
                // float3 _1238 = float3(255.0);
                                
                _31.x = i.Varying_1.w + _110__m0.x;
                _31.y = i.Varying_3.w + _110__m0.z;
                float3 _145 = UNITY_SAMPLE_TEX2D_LOD(_7, i.Varying_0.xy, 0.0).xyz;
                _37 = float4(_145.x, _145.y, _145.z, _37.w);
                float3 _152 = _37.xyz + (9.9999997473787516355514526367188e-05);
                _36 = float4(_152.x, _152.y, _152.z, _36.w);
                float3 _161 = _36.xyz * (_110__m21);
                _38 = float4(_161.x, _161.y, _161.z, _38.w);
                _35 = UNITY_SAMPLE_TEX2D_LOD(_8, i.Varying_0.xy, 0.0);
                _44 = ((0.001000000047497451305389404296875) < _35);
                _92 = ((-1.0) < float4(_110__m29.x, _110__m30.x, _110__m29.x, _110__m30.x)).xy;
                _92.x = _92.x && _44.x;
                _92.y = _92.y && _44.y;
                if (_92.x)
                {
                    _91 = 1.0 / _110__m7;
                    _44.x = any(((0.0) != (_110__m22)));
                    _47 = _38.xyz * _110__m14.xyz;
                    _47 = lerp(_110__m14.xyz, _47, bool(_44.x));
                    _39 = (_91) * _31;
                    _50 = UNITY_SAMPLE_TEX2D(_9, _39);
                    _91 = (_35.x * 1.2000000476837158203125) + _50.w;
                    _83 = max(_91, 0.0);
                    _56 = _47 * _50.xyz;
                    _98 = _91;
                    _99 = _110__m36;
                }
                else
                {
                    _98 = 0.0;
                    _83 = 0.0;
                    _56.x = 0.0;
                    _56.y = 0.0;
                    _56.z = 0.0;
                    _99 = 0.0;
                }
                if (_92.y)
                {
                    _35.x = 1.0 / _110__m8;
                    _52 = any(((0.0) != (_110__m23)));
                    _57 = _38.xyz * _110__m15.xyz;
                    _57 = lerp(_110__m15.xyz, _57, bool(_52));
                    _49 = _31 * _35.xx;
                    _48 = UNITY_SAMPLE_TEX2D(_10, _49);
                    _35.x = (_35.y * 1.2000000476837158203125) + _48.w;
                    _83 = max(_35.x, _83);
                    _58 = _57 * _48.xyz;
                    _36.x = _35.x;
                    _100 = _110__m37;
                }
                else
                {
                    _36.x = 0.0;
                    _58.x = 0.0;
                    _58.y = 0.0;
                    _58.z = 0.0;
                    _100 = 0.0;
                }
                _63 = (float4(-1.0, -1.0, 0.0, 0.0) < float4(_110__m31.x, _110__m32.x, _110__m31.x, _110__m31.x)).xy;
                _63.x = _44.z && _63.x;
                _63.y = _44.w && _63.y;
                if (_63.x)
                {
                    _59.x = 1.0 / _110__m9;
                    _93 = any(((0.0) != (_110__m24)));
                    _64 = _38.xyz * _110__m16.xyz;
                    _64 = lerp(_110__m16.xyz, _64, bool(_93));
                    float2 _382 = _31 * _59.xx;
                    _59 = float3(_382.x, _59.y, _382.y);
                    _40 = UNITY_SAMPLE_TEX2D(_11, _59.xz);
                    _59.x = (_35.z * 1.2000000476837158203125) + _40.w;
                    _83 = max(_83, _59.x);
                    _64 *= _40.xyz;
                    _60 = _59.x;
                    _101 = _110__m38;
                }
                else
                {
                    _60 = 0.0;
                    _64.x = 0.0;
                    _64.y = 0.0;
                    _64.z = 0.0;
                    _101 = 0.0;
                }
                if (_63.y)
                {
                    _65.x = 1.0 / _110__m10;
                    _85 = any(((0.0) != (_110__m25)));
                    _67 = _38.xyz * _110__m17.xyz;
                    _67 = lerp(_110__m17.xyz, _67, bool(_85));
                    _65 = _31 * _65.xx;
                    _40 = UNITY_SAMPLE_TEX2D(_12, _65);
                    _65.x = (_35.w * 1.2000000476837158203125) + _40.w;
                    _83 = max(_83, _65.x);
                    _67 *= _40.xyz;
                    _66 = _65.x;
                    _102 = _110__m39;
                }
                else
                {
                    _66 = 0.0;
                    _67.x = 0.0;
                    _67.y = 0.0;
                    _67.z = 0.0;
                    _102 = 0.0;
                }
                _68 = UNITY_SAMPLE_TEX2D_LOD(_13, i.Varying_0.xy, 0.0).xyz;
                _74 = (float4(0.001000000047497451305389404296875, 0.001000000047497451305389404296875, 0.001000000047497451305389404296875, 0.0) < _68.xyzx).xyz;
                _77 = (float4(-1.0, -1.0, 0.0, 0.0) < float4(_110__m33.x, _110__m34.x, _110__m33.x, _110__m33.x)).xy;
                _74.x = _74.x && _77.x;
                _74.y = _74.y && _77.y;
                if (_74.x)
                {
                    _103 = 1.0 / _110__m11;
                    _74.x = any(((0.0) != (_110__m26)));
                    _78 = _38.xyz * _110__m18.xyz;
                    _78 = lerp(_110__m18.xyz, _78, bool(_74.x));
                    float2 _541 = _31 * (_103);
                    _70 = float4(_541.x, _70.y, _70.z, _541.y);
                    _40 = UNITY_SAMPLE_TEX2D(_14, _70.xw);
                    _68.x = (_68.x * 1.2000000476837158203125) + _40.w;
                    _83 = max(_83, _68.x);
                    _78 *= _40.xyz;
                    _69 = _68.x;
                    _104 = _110__m40;
                }
                else
                {
                    _69 = 0.0;
                    _78.x = 0.0;
                    _78.y = 0.0;
                    _78.z = 0.0;
                    _104 = 0.0;
                }
                if (_74.y)
                {
                    _70.x = 1.0 / _110__m12;
                    _87 = any(((0.0) != (_110__m27)));
                    _79 = _38.xyz * _110__m19.xyz;
                    _79 = lerp(_110__m19.xyz, _79, bool(_87));
                    float2 _604 = _31 * _70.xx;
                    _70 = float4(_604.x, _604.y, _70.z, _70.w);
                    _40 = UNITY_SAMPLE_TEX2D(_15, _70.xy);
                    _70.x = (_68.y * 1.2000000476837158203125) + _40.w;
                    _83 = max(_83, _70.x);
                    _79 *= _40.xyz;
                    _71 = _70.x;
                    _105 = _110__m41;
                }
                else
                {
                    _71 = 0.0;
                    _79.x = 0.0;
                    _79.y = 0.0;
                    _79.z = 0.0;
                    _105 = 0.0;
                }
                _77.x = (-1.0) < _110__m35;
                _77.x = _74.z && _77.x;
                if (_77.x)
                {
                    _75.x = 1.0 / _110__m13;
                    _89 = any(((0.0) != (_110__m28)));
                    float3 _667 = _38.xyz * _110__m20.xyz;
                    _38 = float4(_667.x, _667.y, _667.z, _38.w);
                    float3 _677 = lerp(_110__m20.xyz, _38.xyz, bool(_89));
                    _38 = float4(_677.x, _677.y, _677.z, _38.w);
                    _75 = _31 * _75.xx;
                    _34 = UNITY_SAMPLE_TEX2D(_16, _75);
                    _54 = (_68.z * 1.2000000476837158203125) + _34.w;
                    _83 = max(_54, _83);
                    float3 _700 = _38.xyz * _34.xyz;
                    _38 = float4(_700.x, _700.y, _700.z, _38.w);
                    _55 = _54;
                    _80 = _110__m42;
                }
                else
                {
                    _38.x = 0.0;
                    _38.y = 0.0;
                    _38.z = 0.0;
                    _55 = 0.0;
                    _80 = 0.0;
                }
                _76 = _83 + (-0.20000000298023223876953125);
                _98 += (-_76);
                _98 = max(_98, 0.0);
                _90 = _36.x + (-_76);
                _90 = max(_90, 0.0);
                _58 *= (_90);
                _58 = ((_98) * _56) + _58;
                _95 = _98 + _90;
                _106 = _60 + (-_76);
                _106 = max(_106, 0.0);
                _58 = ((_106) * _64) + _58;
                _64.x = _106 + _95;
                _84 = _66 + (-_76);
                _84 = max(_84, 0.0);
                _58 = ((_84) * _67) + _58;
                _64.x = _84 + _64.x;
                _94 = _69 + (-_76);
                _94 = max(_94, 0.0);
                _58 = ((_94) * _78) + _58;
                _64.x = _94 + _64.x;
                _67.x = _71 + (-_76);
                _67.x = max(_67.x, 0.0);
                _58 = (_67.xxx * _79) + _58;
                _64.x += _67.x;
                _86 = _55 + (-_76);
                _86 = max(_86, 0.0);
                float3 _822 = ((_86) * _38.xyz) + _58;
                _38 = float4(_822.x, _822.y, _822.z, _38.w);
                _58.x = _64.x + _86;
                _58.x += 0.001000000047497451305389404296875;
                float3 _838 = _38.xyz / _58.xxx;
                _33 = float4(_838.x, _838.y, _838.z, _33.w);
                _38.x = _100 * _90;
                _38.x = (_98 * _99) + _38.x;
                _38.x = (_106 * _101) + _38.x;
                _38.x = (_84 * _102) + _38.x;
                _38.x = (_94 * _104) + _38.x;
                _38.x = (_67.x * _105) + _38.x;
                _38.x = (_86 * _80) + _38.x;
                _33.w = _38.x / _58.x;
                _77.x = _33.w >= 0.00020080320246051996946334838867188;
                _81.x = i.Varying_1.z;
                _81.y = i.Varying_2.z;
                _81.z = i.Varying_3.z;
                _88.x = dot(_81, _81);
                _88.x = rsqrt(_88.x);
                _88 = _88.xxx * _81;
                if (_77.x)
                {
                    _81.x = i.Varying_1.w;
                    _81.y = i.Varying_3.w;
                    _96 = _81.xy * _110__m5.zz;
                    float2 _944 = _81.xy * _110__m6.xx;
                    _81 = float3(_944.x, _944.y, _81.z);
                    _38.x = dot(_110__m4.xyz, _110__m4.xyz);
                    _38.x = rsqrt(_38.x);
                    float3 _965 = _38.xxx * _110__m4.xyz;
                    _38 = float4(_965.x, _965.y, _965.z, _38.w);
                    _38.x = dot(_88, _38.xyz);
                    _38.x = clamp(_38.x, 0.0, 1.0);
                    _38.x += 9.9999997473787516355514526367188e-05;
                    _38.x = log2(_38.x);
                    _38.x *= _110__m4.w;
                    _38.x = exp2(_38.x);
                    _38.x = _33.w * _38.x;
                    _97 = UNITY_SAMPLE_TEX2D(_17, _96).x;
                    _107 = UNITY_SAMPLE_TEX2D(_17, _81.xy).x;
                    _37 = UNITY_SAMPLE_TEX2D(_18, _81.xy);
                    _82.x = (_97 * _107) + 9.9999997473787516355514526367188e-05;
                    _82.x = log2(_82.x);
                    _82.x *= _110__m2.w;
                    _82.x = exp2(_82.x);
                    _58 = (-_110__m2.xyz) + _110__m3.xyz;
                    _82 = (_82.xxx * _58) + _110__m2.xyz;
                    _82 = (_37.xyz * _82) + (-_33.xyz);
                    float3 _1063 = (_38.xxx * _82) + _33.xyz;
                    _33 = float4(_1063.x, _1063.y, _1063.z, _33.w);
                    _82.x = _37.w + (-0.300000011920928955078125);
                    _36.w = (_38.x * _82.x) + 0.300000011920928955078125;
                    _81.x = _33.w * 0.4979999959468841552734375;
                    _81.y = 0.0;
                    _38 = float4(_81.xy.x, _81.xy.y, _38.z, _38.w);
                    _58.x = 0.0;
                    _33.w = 0.0;
                }
                else
                {
                    _38.x = 0.039999999105930328369140625;
                    _38.y = 0.039999999105930328369140625;
                    _58.x = 0.039999999105930328369140625;
                    _36.w = 0.300000011920928955078125;
                }
                float3 _1100 = (_88 * (0.5)) + (0.5);
                _36 = float4(_1100.x, _1100.y, _1100.z, _36.w);
                _38.w = _77.x ? 0.054901964962482452392578125 : 0.0;
                _77.x = any(((0.0) != (_110__m1)));
                _38.z = _77.x ? 0.0 : _58.x;
                float4 Output_0;
                float4 Output_1;
                float4 Output_2;
                Output_0 = _36;
                Output_1 = _33;
                Output_2 = _38;
                
                fixed4 col = fixed4(1, 0, 0, 1);
                col = Output_0;
                col.rgb = LinearToGammaSpace(Output_1.rgb);
                return col;
            }
            ENDCG
        }
    }
}

Shader "genship/postprocessfog"
{
    Properties
    {
        _7 ("_7", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Blend One SrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Common/shaderlib/common.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 Varying_FarPlaneConner : TEXCOORD0;
                float2 Varying_ScreenPos : TEXCOORD1;
            };

            // #define _ProjectionParams _ProjectionParams //_23._m0
            // static matrix _23__m1 = {
            //     1.00, 0.00, 0.00, 0.00,
            //     0.00, 1.00, 0.00, 0.00,
            //     0.00, 0.00, 1.00, 0.00,
            //     0.00, 0.00, 0.00, 1.00,
            // };//_23._m1
            static matrix _23__m2 = {
                2.00, 0.00, 0.00, 0.00    ,
                0.00, 2.00, 0.00, 0.00    ,
                0.00, 0.00, -0.00033, 0.00,
                -1.00, -1.00, -1.00, 1.00 ,
            }; //_23._m2
            
            v2f vert (appdata v)
            {
                v2f o;
                // o.vertex = UnityObjectToClipPos(v.vertex);
                float4 Vertex_Position = v.vertex;
                float3 Vertex_FarPlaneConner = float3(v.uv.xy, v.uv2.x);

                // float4 _14;
                // float4 _15;
                // float _17;
                // float4 _32;
                
                // _14 = Vertex_Position.yyyy * _23__m1[1u];
                // _14 = (_23__m1[0u] * Vertex_Position.xxxx) + _14;
                // _14 = (_23__m1[2u] * Vertex_Position.zzzz) + _14;
                // _14 += _23__m1[3u];

                // _14 = Vertex_Position;
                float4 _clipPos;
                    _clipPos = Vertex_Position.yyyy * _23__m2[1u];
                    _clipPos = (_23__m2[0u] * Vertex_Position.xxxx) + _clipPos;
                    _clipPos = (_23__m2[2u] * Vertex_Position.zzzz) + _clipPos;
                    _clipPos = (_23__m2[3u] * Vertex_Position.wwww) + _clipPos;
                // gl_Position = _14;
                o.vertex = GlslToDxClipPos(_clipPos);

                // x = 1 or -1 (-1 if projection is flipped)
                // y = near plane
                // z = far plane
                // w = 1/far plane
                // float4 _ProjectionParams;

                
                // _17 = _clipPos.y * _ProjectionParams.x;
                // float2 _108 = _clipPos.xw * (0.5);
                // _14 = float4(_clipPos.x*0.5, _clipPos.y, _clipPos.w*0.5, _clipPos.y * _ProjectionParams.x * 0.5);
                // _14.w = _17 * 0.5;
                float4 _screenPos = ComputeNonStereoScreenPos(_clipPos);
                o.Varying_ScreenPos = _screenPos.xy;
                o.Varying_FarPlaneConner = Vertex_FarPlaneConner;
                
                return o;
            }


            #define _64__m0  float3(4.72038, 196.40625, -8.97445)          // _64._m0
            #define _64__m1  float4(1.00, 0.25, 6000.00, 0.00017         ) //_64._m1
            #define _64__m2  float4(-23999.00, 24000.00, -3.99983, 4.00  ) //_64._m2
            #define _64__m3  float4(0.09966, 0.37807, 0.79386, 1.1879    ) //_64._m3
            #define _64__m4  float4(0.045, 0.00376, 0.00, 0.00           ) //_64._m4
            #define _64__m5  float4(0.00391, -0.0625, 1.00, 1.00         ) //_64._m5
            #define _64__m6  float4(0.00721, 0.1452, 0.38323, 0.90       ) //_64._m6
            #define _64__m7  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7
            #define _64__m8  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8
            #define _64__m9  float4(0.00208, 0.23016, 0.33588, 0.00017   ) //_64._m9
            #define _64__m10 float4(-0.001, 9.00, -0.001, 1.20191        ) //_64._m10
            #define _64__m11 float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
            #define _64__m12 float4(1.00, 0.00, -0.01, 2.50              ) //_64._m12
            #define _64__m13 float4(1.28117, 0.24777, 1.00, 0.00         ) //_64._m13
            #define _64__m14 float4(1.00, 0.90, 0.00, 0.00               ) //_64._m14
            #define _64__m15 float4(-1638.7793, 0.00, 2659.17578, 0.00   ) //_64._m15
            #define _64__m16 float4(1.00, 1.00, 1.00, 0.07213            ) //_64._m16
            #define _64__m17 float4(1.00, -1.00, 10000.00, 0.00          ) //_64._m17
            #define _64__m18 float4(1.00, 1.00, 1.00, -16.00             ) //_64._m18
            #define _64__m19 float4(0.00, 0.00, 0.00, 0.00               ) //_64._m19
            #define _64__m20 float4(0.00, 0.00, 0.00, 0.00               ) //_64._m20
            #define _64__m21 0.00                                          // _64._m21
            #define _64__m22 float3(0.00, 0.00, 0.00)                      //_64._m22
            #define _64__m23 float4(0.00, 0.00, 0.00, 0.00 )               // _64._m23
            #define _64__m24 float4(0.00, 0.00, 0.00, 0.00 )               // _64._m24
            #define _64__m25 0.00                                          // _64._m25

            sampler2D _7;
            fixed4 frag (v2f i) : SV_Target
            {

                fixed4 col = fixed4(0, 0, 0, 1);

                float4 Output_0;
                float4 Output_1;

                float _19;
                bool _22;
                float3 _24;
                float _25;
                bool _26;
                float3 _27;
                float _28;
                bool _29;
                float _30;
                float3 _31;
                float4 _33;
                float _34;
                bool3 _37;
                float3 _38;
                float2 _40;
                float3 _41;
                float3 _42;
                float3 _43;
                float2 _44;
                bool _45;
                float _46;
                float _47;
                float _48;
                bool _49;
                float _50;
                float _51;
                bool2 _54;
                float _55;
                float _56;
                bool _57;
                float _58;
                float _59;
                float _60;
                float _61;
                // float _939;
                // uint _943;
                // float3 _949 = (255.0);
                // uint _988;
                // float3 _990 = (255.0);
                
                _19 = tex2D(_7, i.Varying_ScreenPos).x;
                _19 = (_64__m2.x * _19) + _64__m2.y;
                _19 = 1.0 / _19;
                _43 = (_19) * i.Varying_FarPlaneConner;
                _24 = (i.Varying_FarPlaneConner * (_19)) + _64__m0;
                _19 *= _64__m1.z;
                _58 = dot(_43, _43);
                _58 = sqrt(_58);
                _27.x = _58 + (-_64__m11.w);
                _29 = _27.x < 0.0;
                if ((int(_29) * (-1)) != 0)
                {
                    discard;
                }
                _29 = 0.00999999977648258209228515625 < _64__m15.w;
                if (_29)
                {
                    _29 = _64__m20.y < 0.5;
                    if (_29)
                    {
                        _27 = _24 + (-_64__m15.xyz);
                        _24.x = dot(_27, _27);
                        _24.x = sqrt(_24.x);
                        _24.x = (_24.x * _64__m17.z) + _64__m17.w;
                        _24.x = clamp(_24.x, 0.0, 1.0);
                        _24.x = (-_24.x) + 1.0;
                        _31.x = _24.x * _24.x;
                    }
                    else
                    {
                        _24.x = _24.y + (-_64__m15.y);
                        _50 = 1.0 / _64__m15.w;
                        _24.x = _50 * _24.x;
                        _24.x = clamp(_24.x, 0.0, 1.0);
                        _50 = (_24.x * (-2.0)) + 3.0;
                        _24.x *= _24.x;
                        _30 = _24.x * _50;
                        _31.x = _30;
                    }
                    _26 = _64__m20.x >= 0.0500000007450580596923828125;
                    _24.x = float(_26);
                    _24.x *= _31.x;
                    _29 = 0.949999988079071044921875 >= _64__m20.x;
                    _27.x = float(_29);
                    _27.x *= _31.x;
                    _25 = _24.x;
                    _28 = _27.x;
                }
                else
                {
                    _25 = 0.0;
                    _28 = 0.0;
                }
                _33.x = (_58 * _64__m8.z) + _64__m8.w;
                _33.x = clamp(_33.x, 0.0, 1.0);
                _47 = (_58 * _64__m18.z) + _64__m18.w;
                _47 = clamp(_47, 0.0, 1.0);
                _31.x = (-_33.x) + _47;
                _31.x = (_25 * _31.x) + _33.x;
                _46 = (-_31.x) + 2.0;
                _31.x = _46 * _31.x;
                _43.x = dot(_43.xz, _43.xz);
                _43.x = sqrt(_43.x);
                _55 = (_43.x * _64__m10.x) + _64__m10.y;
                _55 = clamp(_55, 0.0, 1.0);
                _33.x = (_64__m0.y * _64__m10.z) + _64__m10.w;
                _33.x = clamp(_33.x, 0.0, 1.0);
                _47 = _64__m1.z * 0.99989998340606689453125;
                _22 = _19 >= _47;
                _47 = _31.x * _64__m7.w;
                _31.x = _22 ? _47 : _31.x;
                _46 = _22 ? _33.x : _55;
                _19 = (-_64__m3.w) + _64__m19.w;
                _19 = (_25 * _19) + _64__m3.w;
                _56 = _31.x + 9.9999997473787516355514526367188e-05;
                _56 = log2(_56);
                _19 = _56 * _19;
                _19 = exp2(_19);
                _55 = _64__m6.w * _64__m14.x;
                _19 = min(_55, _19);
                _19 = min(_19, 1.0);
                _55 = (_24.y * _64__m8.x) + _64__m8.y;
                _55 = clamp(_55, 0.0, 1.0);
                _31.x = (-_55) + 2.0;
                _31.x = _55 * _31.x;
                float3 _432 = (_31.xxx * _64__m7.xyz) + _64__m6.xyz;
                _33 = float4(_432.x, _432.y, _432.z, _33.w);
                _38 = (-_33.xyz) + _64__m19.xyz;
                float3 _448 = ((_25) * _38) + _33.xyz;
                _33 = float4(_448.x, _448.y, _448.z, _33.w);
                _55 = _58 + (-_64__m5.w);
                _55 *= _64__m9.w;
                _55 = clamp(_55, 0.0, 1.0);
                _38 = (-_33.xyz) + _64__m9.xyz;
                float3 _477 = ((_55) * _38) + _33.xyz;
                _33 = float4(_477.x, _477.y, _477.z, _33.w);
                _43.x = (_43.x * _64__m12.z) + _64__m12.w;
                _43.x = clamp(_43.x, 0.0, 1.0);
                _55 = (-_64__m4.y) + _64__m16.w;
                _55 = (_28 * _55) + _64__m4.y;
                float2 _513 = _43.yy * _64__m4.xz;
                _38 = float3(_513.x, _513.y, _38.z);
                _54 = ((0.00999999977648258209228515625) < abs(_38.xyxy)).xy;
                _40 = ((-_64__m4.xz) * _43.yy) + _64__m13.yw;
                _40 = min(_40, (80.0));
                _40 *= (1.44269502162933349609375);
                _40 = exp2(_40);
                _40 = (-_40) + _64__m13.xz;
                float2 _554 = _40 / _38.xy;
                _38 = float3(_554.x, _554.y, _38.z);
                _38.x = _54.x ? _38.x : _64__m13.x;
                _38.y = _54.y ? _38.y : _64__m13.z;
                _48 = _55 * _58;
                _48 *= (-_38.x);
                _48 = exp2(_48);
                _48 = (-_48) + 1.0;
                _48 = max(_48, 0.0);
                _55 = (_58 * _64__m5.x) + _64__m5.y;
                _55 = clamp(_55, 0.0, 1.0);
                _60 = (_58 * _64__m17.x) + _64__m17.y;
                _60 = clamp(_60, 0.0, 1.0);
                _31.x = (-_55) + _60;
                _31.x = (_28 * _31.x) + _55;
                _51 = (-_31.x) + 2.0;
                _55 = (-_64__m5.z) + _64__m18.x;
                _55 = (_28 * _55) + _64__m5.z;
                _61 = (_31.x * _51) + (-1.0);
                _55 = (_55 * _61) + 1.0;
                _31.x = _55 * _48;
                _48 = min(_31.x, _64__m6.w);
                _55 = _58 * _64__m4.w;
                _55 *= (-_38.y);
                _55 = exp2(_55);
                _55 = (-_55) + 1.0;
                _55 = max(_55, 0.0);
                _60 = (_58 * _64__m12.x) + _64__m12.y;
                _60 = clamp(_60, 0.0, 1.0);
                _31.x = (-_60) + 2.0;
                _31.x *= _60;
                _31.x = _55 * _31.x;
                _55 = min(_31.x, _64__m14.y);
                _31.x = _46 * _48;
                _31.y = _43.x * _55;
                _43 = (-_64__m3.xyz) + _64__m16.xyz;
                _43 = ((_28) * _43) + _64__m3.xyz;
                _41 = (_19) * _33.xyz;
                _42 = ((-_33.xyz) * (_19)) + _43;
                _41 = (_31.xxx * _42) + _41;
                _19 = (-_19) + 1.0;
                _44 = (-_31.xy) + (1.0);
                _19 = _44.x * _19;
                _31 = (_64__m11.xyz * _31.yyy) + _41;
                _19 = _44.y * _19;
                _45 = any(((0.0) != (_64__m21)));
                if (_45)
                {
                    _43.x = dot(-_64__m0, -_64__m0);
                    _43.x = sqrt(_43.x);
                    _49 = 10000.0 >= _43.x;
                    _37 = ((_64__m25) == float4(0.0, 2.0, 1.0, 0.0)).xyz;
                    _57 = _37.y && _37.x;
                    _34 = dot(_31, float3(0.2125000059604644775390625, 0.7153999805450439453125, 0.07209999859333038330078125));
                    float3 _822 = float3(_34 * _64__m22.x, _34 * _64__m22.y, _34 * _64__m22.z);
                    _33 = float4(_822.x, _822.y, _33.z, _822.z);
                    _41 = lerp(_31, _33.xyw, (_37.z));
                    _41 = lerp(_41, _31, (_57));
                    _33.x = (-_64__m23.z) + 1.0;
                    _43.x = ((-_33.x) * 10000.0) + _43.x;
                    _33.x = (_64__m23.z * 10000.0) + 9.9999997473787516355514526367188e-05;
                    _43.x /= _33.x;
                    _43.x = clamp(_43.x, 0.0, 1.0);
                    _59 = (_43.x * (-_64__m24.x)) + _64__m24.x;
                    _59 = clamp(_59, 0.0, 1.0);
                    _41 = (-_31) + _41;
                    _41 = ((_59) * _41) + _31;
                    _41 = lerp(_41, _31, (_57));
                    _31 = lerp(_31, _41, (_49));
                }
                Output_0 = float4(_31.x, _31.y, _31.z, Output_0.w);
                Output_0.w = _19;
                Output_1 = float4(_31.x, _31.y, _31.z, Output_1.w);
                Output_1.w = _19;

                col = Output_0;
                // col = Output_1;
                return col;
            }
            ENDCG
        }
    }
}

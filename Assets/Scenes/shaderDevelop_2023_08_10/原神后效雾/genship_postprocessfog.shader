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


            // #define _WorldSpaceCameraPos  float3(4.72038, 196.40625, -8.97445)          // _64._m0
            // #define _ProjectionParams  float4(1.00, 0.25, 6000.00, 0.00017         ) //_64._m1
            // #define _64__m2  _ZBufferParams// float4(-23999.00, 24000.00, -3.99983, 4.00  ) //_64._m2
            #define _ColorA1  float4(0.09966, 0.37807, 0.79386, 1.1879    ) //_64._m3
            #define _ExpDampingScaleXZ_AffectYW  float4(0.045, 0.00376, 0.00, 0.00           ) //_64._m4
            #define _FogGradientFactorZ_  float4(0.00391, -0.0625, 1.00, 1.00         ) //_64._m5
            #define _FogDistanceColor  float4(0.00721, 0.1452, 0.38323, 0.90       ) //_64._m6
            #define _SkyFogDistanceScaleW_  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7
            #define _TerrainYSO_XY_TerrainDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8
            #define _FogColorC  float4(0.00208, 0.23016, 0.33588, 0.00017   ) //_64._m9
            #define _64__m10 float4(-0.001, 9.00, -0.001, 1.20191        ) //_64._m10
            #define _FogColorXYZ_FogVisableDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
            #define _TerrainDistanceXYSO_ZW_DistanceSO_XY_ float4(1.00, 0.00, -0.01, 2.50              ) //_64._m12
            #define _ExpDampingStartXZ_ float4(1.28117, 0.24777, 1.00, 0.00         ) //_64._m13
            #define _FogDistanceLimitX_Y_ float4(1.00, 0.90, 0.00, 0.00               ) //_64._m14
            #define _MoonPos_maybe float4(-1638.7793, 0.00, 2659.17578, 0.00   ) //_64._m15
            #define _ColorA2 float4(1.00, 1.00, 1.00, 0.07213            ) //_64._m16
            #define _64__m17 float4(1.00, -1.00, 10000.00, 0.00          ) //_64._m17
            #define _64__m18 float4(1.00, 1.00, 1.00, -16.00             ) //_64._m18
            #define _FogColorB float4(0.00, 0.00, 0.00, 0.00               ) //_64._m19
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
                // bool _22;
                float3 _24;
                // float _25;
                bool _26;
                float3 _27;
                // float _28;
                // bool _29;
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
                // float _46;
                // float _47;
                float _48;
                bool _49;
                float _50;
                float _51;
                bool2 _54;
                float _55;
                // float _56;
                bool _57;
                // float _58;
                float _59;
                float _60;
                float _61;
                // float _939;
                // uint _943;
                // float3 _949 = (255.0);
                // uint _988;
                // float3 _990 = (255.0);
                // _19 = tex2D(_7, i.Varying_ScreenPos).x;
                float _rawDepth = tex2D(_7, i.Varying_ScreenPos).x;
                // _19 = (_64__m2.x * _rawDepth) + _64__m2.y;
                // _19 = 1.0 / _19;
                float _terrainLinear01Depth = Linear01Depth(_rawDepth);
                // 注：opengl 下
                //   _rawDepth 0 .. 1 ， 对应 _terrainLinear01Depth near/far .. 1，不会为 0
                // _19 = _terrainLinear01Depth;
                float3 _terrainWorldPos = (_terrainLinear01Depth * i.Varying_FarPlaneConner) + _WorldSpaceCameraPos;
                float3 _terrainWorldPos_relativeToCamera = (_terrainLinear01Depth) * i.Varying_FarPlaneConner;
                // _43 = _terrainWorldPos_relativeToCamera;
                // _24 = _terrainWorldPos;
                float _terrainEyeDepth = _terrainLinear01Depth * _ProjectionParams.z; // far plane
                // 注：opengl 下
                //   _terrainLinear01Depth near/far .. 1，对应 _terrainEyeDepth near .. far，不会为 0
                // _19 = _terrainEyeDepth; // far plane
                // _58 = dot(_terrainWorldPos_relativeToCamera, _terrainWorldPos_relativeToCamera);
                // _58 = sqrt(_58);
                float _terrainToCamera_length = length(_terrainWorldPos_relativeToCamera);
                // _58 = _terrainToCamera_length;

                // #define _FogColorXYZ_FogVisableDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
                // _27.x = _terrainToCamera_length + (-_FogColorXYZ_FogVisableDistanceW_.w);
                // _29 = _27.x < 0.0;
                if (_terrainToCamera_length < _FogColorXYZ_FogVisableDistanceW_.w)
                {
                    discard;
                }
                // _29 = 0.01 < _MoonPos_maybe.w;
                // #define _MoonPos_maybe float4(-1638.7793, 0.00, 2659.17578, 0.00   ) //_64._m15
                float _if_output1;
                float _if_output2;
                if (0.01 < _MoonPos_maybe.w)
                {
                    // _29 = _64__m20.y < 0.5;
                    // #define _64__m20 float4(0.00, 0.00, 0.00, 0.00               ) //_64._m20
                    if (_64__m20.y < 0.5)
                    {
                        _27 = _terrainWorldPos + (-_MoonPos_maybe.xyz);
                        _24.x = dot(_27, _27);
                        _24.x = sqrt(_24.x);
                        _24.x = (_24.x * _64__m17.z) + _64__m17.w;
                        _24.x = clamp(_24.x, 0.0, 1.0);
                        _24.x = (-_24.x) + 1.0;
                        _31.x = _24.x * _24.x;
                    }
                    else
                    {
                        _24.x = _terrainWorldPos.y + (-_MoonPos_maybe.y);
                        _50 = 1.0 / _MoonPos_maybe.w;
                        _24.x = _50 * _24.x;
                        _24.x = clamp(_24.x, 0.0, 1.0);
                        _50 = (_24.x * (-2.0)) + 3.0;
                        _24.x *= _24.x;
                        _30 = _24.x * _50;
                        _31.x = _30;
                    }
                    // #define _64__m20 float4(0.00, 0.00, 0.00, 0.00               ) //_64._m20
                    _26 = _64__m20.x >= 0.05;
                    _24.x = float(_26);
                    _24.x *= _31.x;
                    // _29 = 0.95 >= _64__m20.x;
                    _27.x = float(0.95 >= _64__m20.x);
                    _27.x *= _31.x;
                    _if_output1 = _24.x;
                    _if_output2 = _27.x;
                }
                else
                {
                    _if_output1 = 0.0;
                    _if_output2 = 0.0;
                }
                // _25 = _if_output1;
                // _28 = _if_output2;
                
                // _33.x = (_terrainToCamera_length * _TerrainYSO_XY_TerrainDistanceSO_ZW_.z) + _TerrainYSO_XY_TerrainDistanceSO_ZW_.w;
                // _33.x = clamp(_33.x, 0.0, 1.0);
                // #define _TerrainYSO_XY_TerrainDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8
                float _terrainToCamera_length_SO1 = saturate(_terrainToCamera_length * _TerrainYSO_XY_TerrainDistanceSO_ZW_.z + _TerrainYSO_XY_TerrainDistanceSO_ZW_.w);
                // _33.x = _terrainToCamera_length_SO1;
                // _47 = _terrainToCamera_length * _64__m18.z + _64__m18.w;
                // _47 = clamp(_47, 0.0, 1.0);
                float _terrainToCamera_length_SO2 = saturate(_terrainToCamera_length * _64__m18.z + _64__m18.w);
                // _47 = _terrainToCamera_length_SO2;
                // _31.x = _terrainToCamera_length_SO2 - _terrainToCamera_length_SO1;
                // _31.x = (_if_output1 * (_terrainToCamera_length_SO2 - _terrainToCamera_length_SO1)) + _terrainToCamera_length_SO1;
                float _terrainToCamera_length_SO = lerp( _terrainToCamera_length_SO1, _terrainToCamera_length_SO2, _if_output1 );
                // _31.x = _terrainToCamera_length_SO;
                // _46 = (-_terrainToCamera_length_SO) + 2.0;
                _31.x = (2.0-_terrainToCamera_length_SO) * _terrainToCamera_length_SO;
                // smooth 形式 0~1 快速上升后平缓到1
                float _terrainToCamera_length_SO_smooth01 = (2.0-_terrainToCamera_length_SO) * _terrainToCamera_length_SO;
                // _31.x = _terrainToCamera_length_SO_smooth01;
                // _43.x = dot(_terrainWorldPos_relativeToCamera.xz, _terrainWorldPos_relativeToCamera.xz);
                // _43.x = sqrt(_43.x);
                float _terrainToCameraXZ_length = length(_terrainWorldPos_relativeToCamera.xz);
                // _43.x = _terrainToCameraXZ_length;
                float _terrainToCameraXZ_length_SO1 = saturate(_terrainToCameraXZ_length * _64__m10.x + _64__m10.y);
                // _55 = clamp(_55, 0.0, 1.0);
                // _55 = _terrainToCameraXZ_length_SO1;
                // _33.x = (_WorldSpaceCameraPos.y * _64__m10.z) + _64__m10.w;
                // _33.x = clamp(_33.x, 0.0, 1.0);
                float _WorldSpaceCameraPosY_SO = saturate(_WorldSpaceCameraPos.y * _64__m10.z + _64__m10.w);
                // _33.x = _WorldSpaceCameraPosY_SO;
                // _47 = _ProjectionParams.z * 0.9999;
                bool _isSky = _terrainEyeDepth >= _ProjectionParams.z * 0.9999; // 没有深度了，太远了，是天空
                // _22 = _isSky;
                // _47 = _terrainToCamera_length_SO_smooth01 * _SkyFogDistanceScaleW_.w;
                // #define _SkyFogDistanceScaleW_  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7
                float _fogXZDistance = _isSky ? _terrainToCamera_length_SO_smooth01 * _SkyFogDistanceScaleW_.w : _terrainToCamera_length_SO_smooth01;
                // _31.x = _fogXZDistance;
                float _fogFactor = _isSky ? _WorldSpaceCameraPosY_SO : _terrainToCameraXZ_length_SO1;
                // _46 = _fogFactor;
                // #define _ColorA1  float4(0.09966, 0.37807, 0.79386, 1.1879    ) //_64._m3
                // #define _FogColorB float4(0.00, 0.00, 0.00, 0.00               ) //_64._m19
                // _19 = (-_ColorA1.w) + _FogColorB.w;
                // _19 = (_if_output1 * (_FogColorB.w - _ColorA1.w)) + _ColorA1.w;
                float _colorw = lerp( _ColorA1.w, _FogColorB.w, _if_output1 );
                // _19 = _colorw;
                // _56 = _fogXZDistance + 1e-04;
                // _56 = log2(_fogXZDistance + 1e-04);
                // _19 = log2(_fogXZDistance + 1e-04) * _colorw;
                // _19 = exp2(log2(_fogXZDistance + 1e-04) * _colorw);
                float _fogXZDistance_pow = pow(_fogXZDistance + 1e-04, _colorw);
                // _19 = _fogXZDistance_pow;
                // _55 = _FogDistanceColor.w * _FogDistanceLimitX_Y_.x;
                // _19 = min(_FogDistanceColor.w * _FogDistanceLimitX_Y_.x, _fogXZDistance_pow);
                float _fogXZDistance_pow_limit1 = min(_fogXZDistance_pow, min(_FogDistanceColor.w * _FogDistanceLimitX_Y_.x, 1.0));
                // _19 = _fogXZDistance_pow_limit1;
                // _55 = (_terrainWorldPos.y * _TerrainYSO_XY_TerrainDistanceSO_ZW_.x) + _TerrainYSO_XY_TerrainDistanceSO_ZW_.y;
                // _55 = clamp(_55, 0.0, 1.0);
                float _terrainWorldPosY_SO = saturate(_terrainWorldPos.y * _TerrainYSO_XY_TerrainDistanceSO_ZW_.x + _TerrainYSO_XY_TerrainDistanceSO_ZW_.y);
                float _terrainWorldPosY_SO_smooth01 = _terrainWorldPosY_SO * (2.0 - _terrainWorldPosY_SO);
                // _55 = _terrainWorldPosY_SO;
                // _31.x = (-_terrainWorldPosY_SO) + 2.0;
                // _31.x = _terrainWorldPosY_SO * (2.0 - _terrainWorldPosY_SO);
                // _31.x = _terrainWorldPosY_SO_smooth01;
                // float3 _432 = (_terrainWorldPosY_SO_smooth01 * _SkyFogDistanceScaleW_.xyz) + _FogDistanceColor.xyz;
                float3 _fogColor = (_terrainWorldPosY_SO_smooth01 * _SkyFogDistanceScaleW_.xyz) + _FogDistanceColor.xyz;
                
                // _33 = float4(_432.x, _432.y, _432.z, _33.w);
                // _33.xyz = _fogColor;
                // _38 = _FogColorB.xyz - _fogColor;
                // float3 _448 = ((_if_output1) * _38) + _fogColor;
                // _33 = float4(_448.x, _448.y, _448.z, _33.w);
                // _33.xyz = _if_output1 * (_FogColorB.xyz - _fogColor) + _fogColor;
                float3 _fogColor_2 = lerp(_fogColor, _FogColorB.xyz, _if_output1);
                // _33.xyz = _fogColor_2;
                // _55 = _terrainToCamera_length + (-_FogGradientFactorZ_.w);
                // _55 *= _FogColorC.w;
                float _terrainToCamera_length_OS = clamp((_terrainToCamera_length - _FogGradientFactorZ_.w) * _FogColorC.w, 0.0, 1.0);
                // _55 = _terrainToCamera_length_OS;
                // _38 = (-_fogColor_2) + _FogColorC.xyz;
                // float3 _477 = (_terrainToCamera_length_OS * ((-_fogColor_2) + _FogColorC.xyz)) + _fogColor_2;
                // float3 _477 = lerp(_fogColor_2, _FogColorC.xyz, _terrainToCamera_length_OS);
                // _33 = float4(_477.x, _477.y, _477.z, _33.w);
                float3 _fogColor_3 = lerp(_fogColor_2, _FogColorC.xyz, _terrainToCamera_length_OS);
                // _33.xyz = _fogColor_3;
                // _43.x = (_terrainToCameraXZ_length * _TerrainDistanceXYSO_ZW_DistanceSO_XY_.z) + _TerrainDistanceXYSO_ZW_DistanceSO_XY_.w;
                // _43.x = clamp(_43.x, 0.0, 1.0);
                float _terrainToCameraXZ_length_SO = saturate(_terrainToCameraXZ_length * _TerrainDistanceXYSO_ZW_DistanceSO_XY_.z + _TerrainDistanceXYSO_ZW_DistanceSO_XY_.w);
                // _43.x = _terrainToCameraXZ_length_SO;
                // _55 = (-_ExpDampingScaleXZ_AffectYW.y) + _ColorA2.w;
                // _55 = (_if_output2 * ((-_ExpDampingScaleXZ_AffectYW.y) + _ColorA2.w)) + _ExpDampingScaleXZ_AffectYW.y;
                float _lerp_55 = lerp(_ExpDampingScaleXZ_AffectYW.y, _ColorA2.w, _if_output2);
                // _55 = _lerp_55;
                // float2 _513 = _terrainWorldPos_relativeToCamera.yy * _ExpDampingScaleXZ_AffectYW.xz;
                // _38 = float3(_513.x, _513.y, _38.z);
                // _38.xy = _terrainWorldPos_relativeToCamera.yy * _ExpDampingScaleXZ_AffectYW.xz;
                // float _in_x_1 = _terrainWorldPos_relativeToCamera.y * _ExpDampingScaleXZ_AffectYW.x;
                // float _in_x_2 = _terrainWorldPos_relativeToCamera.y * _ExpDampingScaleXZ_AffectYW.z;
                // _38.xy = float2(_in_x_1, _in_x_2);
                // _54 = (0.01 < abs(_38.xy)).xy;
                // _54.x = 0.01 < abs(_38.x);
                // _54.y = 0.01 < abs(_38.y);
                // _40 = ((-_ExpDampingScaleXZ_AffectYW.xz) * _terrainWorldPos_relativeToCamera.yy) + _ExpDampingStartXZ_.yw;
    
                
                // _40 = min((-_ExpDampingScaleXZ_AffectYW.xz * _terrainWorldPos_relativeToCamera.yy) + _ExpDampingStartXZ_.yw, 80.0);
                // float _exponent_1 = min( _ExpDampingStartXZ_.y - _in_x_1, 80.0 );
                // float _exponent_2 = min( _ExpDampingStartXZ_.w - _in_x_2, 80.0 );
                // _40.xy = float2(_exponent_1, _exponent_2);
                // _40 = _40 * (1.44269502162933349609375);
                // _40 = exp2(_40 * 1.44269502162933349609375);
                // _40 = exp(_40);
                // _40.x = _ExpDampingStartXZ_.x - exp(_exponent_1);
                // _40.y = _ExpDampingStartXZ_.z - exp(_exponent_2);
                // float2 _554 = _40 / _38.xy;
                // _38 = float3(_554.x, _554.y, _38.z);
                // _38.x = _40.x / _in_x_1;
                // _38.y = _40.y / _in_x_2;
                
                // _38.x = 0.01 < abs(_in_x_1) ? _40.x / _in_x_1 : _ExpDampingStartXZ_.x;
                // _38.y = 0.01 < abs(_in_x_2) ? _40.y / _in_x_2 : _ExpDampingStartXZ_.z;

                // _ExpDampingStartXZ_.y = log(_ExpDampingStartXZ_.x)
                // in_pre_compute_a 是 log(_ExpDampingStartXZ_.x)
                //             也可以是 _ExpDampingStartXZ_.y
                // #define _ExpDampingScaleXZ_AffectYW  float4(0.045, 0.00376, 0.00, 0.00           ) //_64._m4
                // #define _ExpDampingStartXZ_ float4(1.28117, 0.24777, 1.00, 0.00         ) //_64._m13
                float _terrainHeightDiff_expDamping1 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _ExpDampingScaleXZ_AffectYW.x, _ExpDampingStartXZ_.x);
                float _terrainHeightDiff_expDamping2 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _ExpDampingScaleXZ_AffectYW.z, _ExpDampingStartXZ_.z);
                // _38.x = _terrainHeightDiff_expDamping1; 
                // _38.y = _terrainHeightDiff_expDamping2; 

                float _terrainDistanceAffectByDamping1;
                {
                    float _tmp_48;
                    // float _lerp_55 = lerp(_ExpDampingScaleXZ_AffectYW.y, _ColorA2.w, _if_output2)
                    _tmp_48 = _terrainToCamera_length * _lerp_55;
                    _tmp_48 = _tmp_48 * (-_terrainHeightDiff_expDamping1);
                    _tmp_48 = 1.0 - exp2(_tmp_48);
                    _tmp_48 = max(_tmp_48, 0.0);
                    _terrainDistanceAffectByDamping1 = _tmp_48;
                }
                // _48 = _terrainDistanceAffectByDamping1;

                
                // _55 = (_terrainToCamera_length * _FogGradientFactorZ_.x) + _FogGradientFactorZ_.y;
                // _55 = clamp(_55, 0.0, 1.0);
                float _terrainToCamera_length_SOB_1 = saturate(_terrainToCamera_length * _FogGradientFactorZ_.x + _FogGradientFactorZ_.y);
                // _55 = _terrainToCamera_length_SOB_1;
                // _60 = (_terrainToCamera_length * _64__m17.x) + _64__m17.y;
                // _60 = clamp(_60, 0.0, 1.0);
                float _terrainToCamera_length_SOB_2 = saturate(_terrainToCamera_length * _64__m17.x + _64__m17.y);
                // _60 = _terrainToCamera_length_SOB_2;
                // _31.x = (-_terrainToCamera_length_SOB_1) + _terrainToCamera_length_SOB_2;
                // _31.x = (_if_output2 * ((-_terrainToCamera_length_SOB_1) + _terrainToCamera_length_SOB_2)) + _terrainToCamera_length_SOB_1;
                float _terrainToCamera_length_SOB = lerp(_terrainToCamera_length_SOB_1, _terrainToCamera_length_SOB_2, _if_output2);
                // _31.x = _terrainToCamera_length_SOB;
                // _55 = (-_FogGradientFactorZ_.z) + _64__m18.x;
                // _55 = (_if_output2 * _55) + _FogGradientFactorZ_.z;
                float _fogGradientFactor = lerp(_FogGradientFactorZ_.z, _64__m18.x, _if_output2);
                // _55 = _fogGradientFactor;
                // _51 = (-_terrainToCamera_length_SOB) + 2.0;
                // _61 = (_terrainToCamera_length_SOB * (-_terrainToCamera_length_SOB + 2.0)) + (-1.0);
                float _terrainToCamera_length_SOB_smooth = _terrainToCamera_length_SOB * (-_terrainToCamera_length_SOB + 2.0);
                // _61 = _terrainToCamera_length_SOB_smooth;
                // _55 = _fogGradientFactor * (_terrainToCamera_length_SOB_smooth - 1.0) + 1.0;
                float _fogFactorB = lerp(1.0, _terrainToCamera_length_SOB_smooth, _fogGradientFactor);
                // _55 = _fogFactorB;
                float _fogFactorB_2 = _fogFactorB * _terrainDistanceAffectByDamping1;
                float _fogFactorB_3 = min(_fogFactorB_2, _FogDistanceColor.w);
                float _terrainDistanceAffectByDamping2;
                {
                    float _tmp_55;
                    _tmp_55 = _terrainToCamera_length * _ExpDampingScaleXZ_AffectYW.w;
                    _tmp_55 = _tmp_55 * (-_terrainHeightDiff_expDamping2);
                    _tmp_55 = 1.0 - exp2(_tmp_55);
                    _tmp_55 = max(_tmp_55, 0.0);
                    _terrainDistanceAffectByDamping2 = _tmp_55;
                }
                // _55 = _terrainDistanceAffectByDamping2;
                
                // _60 = (_terrainToCamera_length * _TerrainDistanceXYSO_ZW_DistanceSO_XY_.x) + _TerrainDistanceXYSO_ZW_DistanceSO_XY_.y;
                // _60 = clamp(_60, 0.0, 1.0);
                float _terrainToCamera_length_SOC = saturate(_terrainToCamera_length * _TerrainDistanceXYSO_ZW_DistanceSO_XY_.x + _TerrainDistanceXYSO_ZW_DistanceSO_XY_.y);
                // _60 = _terrainToCamera_length_SOC;
                // _31.x = (-_60) + 2.0;
                // _31.x *= _60;
                float _terrainToCamera_length_SOC_smooth = _terrainToCamera_length_SOC * (2.0 - _terrainToCamera_length_SOC);
                
                float _fogDistanceFactor = _terrainDistanceAffectByDamping2 * _terrainToCamera_length_SOC_smooth;
                // _31.x = _fogDistanceFactor;
                float _fogDistanceFactor_2 = min(_fogDistanceFactor, _FogDistanceLimitX_Y_.y);
                // _55 = _fogDistanceFactor_2;
                float _fogFactorC = _fogFactor * _fogFactorB_3;
                // _31.x = _fogFactorC;
                float _fogFactorD = _terrainToCameraXZ_length_SO * _fogDistanceFactor_2;
                // _31.y = _fogFactorD;
                // _43 = (-_ColorA1.xyz) + _ColorA2.xyz;
                // _43 = ((_if_output2) * _43) + _ColorA1.xyz;
                float3 _colorA = lerp(_ColorA1.xyz, _ColorA2.xyz, _if_output2);
                // _43 = _colorA;
                // _41 = (_fogXZDistance_pow_limit1) * _fogColor_3;
                float3 _outputColor1 = _fogXZDistance_pow_limit1 * _fogColor_3;
                // _41 = _outputColor1;
                float3 _outputColor2 = (-_fogColor_3 * _fogXZDistance_pow_limit1) + _colorA;
                // _42 = _outputColor2;
                float3 _outputColor3 = (_fogFactorC * _outputColor2) + _outputColor1;
                // _41 = _outputColor3;
                // _44 = (-_31.xy) + (1.0);
                _31 = (_FogColorXYZ_FogVisableDistanceW_.xyz * _fogFactorD) + _outputColor3;
                // _19 = (1.0 - _fogXZDistance_pow_limit1);
                // _19 = (1.0 - _fogFactorC) * _19;
                float _outputAlpha = (1.0 - _fogXZDistance_pow_limit1) * (1.0 - _fogFactorD) * (1.0 - _fogFactorC);
                // _45 = any(0.0 != _64__m21);
                if (_64__m21)
                {
                    _43.x = dot(-_WorldSpaceCameraPos, -_WorldSpaceCameraPos);
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
                Output_0.w = _outputAlpha;
                Output_1 = float4(_31.x, _31.y, _31.z, Output_1.w);
                Output_1.w = _outputAlpha;

                col = Output_0;
                // col = Output_1;
                return col;
            }
            ENDCG
        }
    }
}

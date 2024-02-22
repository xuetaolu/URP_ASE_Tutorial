Shader "genship/postprocessfog_box2"
{
    Properties
    {
        _7 ("_7", 2D) = "white" {}
        
        /*_m3 变xyz */ _FogMainColor ("_FogMainColor", Color) = (0.09966, 0.37807, 0.79386, 1.0) 
        /*_m3 变w   */ _FogDistancePow ("_FogDistancePow", Range(0, 2)) = 1.1879
        /*_m4 变y   */ _ExpDampingScaleXZ_AffectYW ("_ExpDampingScaleXZ_AffectYW", Vector) = (0.045, 0.00376, 0.00, 0.00) 
        _FogGradientFactorZ_ ("_FogGradientFactorZ_", Vector) = (0.00391, -0.0625, 1.00, 1.00)
        /*_m6 变xyz */ _FogDistanceColor ("_FogDistanceColor", Color) = (0.00721, 0.1452, 0.38323, 0.90)
        /*_m7 变xyz */ _SkyFogDistanceScaleW_ ("_SkyFogDistanceScaleW_", Vector) = (0.02258, 0.01951, -0.08341, 0.00)
        /*_m8 变xy  */ _TerrainYSO_XY_TerrainDistanceSO_ZW_ ("_TerrainYSO_XY_TerrainDistanceSO_ZW_", Vector) = (0.00393, -0.79396, 0.00042, -0.00671)
        /*_m9 变xyz */ _FogColorC ("_FogColorC", Color) = (0.00208, 0.23016, 0.33588, 0.00017)
        /*_m10变w   */ _64__m10 ("_64__m10", Vector) = (-0.001, 9.00, -0.001, 1.20191)
        _FogColorXYZ ("_FogColorXYZ", Color) = (1.00, 1.00, 1.00, 1.00)
        _FogVisibleDistanceW ("_FogVisibleDistanceW", Range(0, 32)) = 16
        _TerrainDistanceXYSO_ZW_DistanceSO_XY_ ("_TerrainDistanceXYSO_ZW_DistanceSO_XY_", Vector) = (1.00, 0.00, -0.01, 2.50)
        /*_m13变xy  */ _ExpDampingStartXZ_ ("_ExpDampingStartXZ_", Vector) = (1.28117, 0.24777, 1.00, 0.00)
        _FogDistanceLimitX_Y_ ("_FogDistanceLimitX_Y_", Vector) = (1.00, 0.90, 0.00, 0.00)
        
        _64__m20 ("_64__m20", Vector) = (0.00, 0.00, 0.00, 0.00)
        _ColorA2 ("_ColorA2", Color) = (1.00, 1.00, 1.00, 0.07213)
        _MoonPos_maybe_Pos ("_MoonPos_maybe_Pos", Vector) = (-1638.7793, 0.00, 2659.17578)
        _MoonPos_maybe_W ("_MoonPos_maybe_W", Range(0, 1)) = 0
        
        [Toggle]_64__m21 ("_64__m21", float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend One SrcAlpha
            Cull Front
            Cull Off
            ZWrite Off
            ZTest Always
            
            CGPROGRAM
            float3 _FogMainColor;
            float _FogDistancePow;
            float4 _ExpDampingScaleXZ_AffectYW;
            float4 _FogGradientFactorZ_;
            float4 _FogDistanceColor;
            float4 _SkyFogDistanceScaleW_;
            float4 _TerrainYSO_XY_TerrainDistanceSO_ZW_;
            float4 _FogColorC;
            float4 _64__m10;
            float3 _FogColorXYZ;
            float _FogVisibleDistanceW;
            float4 _TerrainDistanceXYSO_ZW_DistanceSO_XY_;
            float4 _ExpDampingStartXZ_;
            float4 _FogDistanceLimitX_Y_;
            float4 _ColorA2;

            float3 _MoonPos_maybe_Pos;
            float _MoonPos_maybe_W;


            float4 _64__m20;

            float _64__m21;
            #pragma vertex vert
            #pragma fragment frag

            // sampler2D _7;
            // #define _CameraDepthTexture _7

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
                // float3 Varying_FarPlaneConner : TEXCOORD0;
                float4 Varying_ScreenPos : TEXCOORD1;
            };
            
            // static matrix _23__m2 = {
            //     2.00, 0.00, 0.00, 0.00    ,
            //     0.00, 2.00, 0.00, 0.00    ,
            //     0.00, 0.00, -0.00033, 0.00,
            //     -1.00, -1.00, -1.00, 1.00 ,
            // }; //_23._m2
            
            v2f vert (appdata v)
            {
                v2f o;

                float4 Vertex_Position = v.vertex;
                // float3 Vertex_FarPlaneConner = float3(v.uv.xy, v.uv2.x);
                
                // float4 _clipPos;
                //     _clipPos = Vertex_Position.yyyy * _23__m2[1u];
                //     _clipPos = (_23__m2[0u] * Vertex_Position.xxxx) + _clipPos;
                //     _clipPos = (_23__m2[2u] * Vertex_Position.zzzz) + _clipPos;
                //     _clipPos = (_23__m2[3u] * Vertex_Position.wwww) + _clipPos;

                // o.vertex = GlslToDxClipPos(_clipPos);
                float4 _clipPos = UnityObjectToClipPos(v.vertex);
                
                float4 _screenPos = ComputeNonStereoScreenPos(_clipPos);

                o.Varying_ScreenPos = _screenPos.xyzw;
                // o.Varying_FarPlaneConner = Vertex_FarPlaneConner;
                o.vertex = _clipPos;
                
                return o;
            }


            // #define _WorldSpaceCameraPos  float3(4.72038, 196.40625, -8.97445)          // _64._m0
            // #define _ProjectionParams  float4(-1.00, 0.25, 6000.00, 0.00017         ) //_64._m1
            // #define _64__m2  _ZBufferParams// float4(-23999.00, 24000.00, -3.99983, 4.00  ) //_64._m2
            // #define _FogMainColorA  float4(0.09966, 0.37807, 0.79386, 1.1879    ) //_64._m3
            #define _FogMainColorA float4(_FogMainColor.xyz, _FogDistancePow.x) // _64._m3 // 变xyzw
            // #define _ExpDampingScaleXZ_AffectYW  float4(0.045, 0.00376, 0.00, 0.00           ) //_64._m4 // 变y
            // #define _FogGradientFactorZ_  float4(0.00391, -0.0625, 1.00, 1.00         ) //_64._m5
            // #define _FogDistanceColor  float4(0.00721, 0.1452, 0.38323, 0.90       ) //_64._m6 // 变xyz
            // #define _SkyFogDistanceScaleW_  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7 // 变xyz
            // #define _TerrainYSO_XY_TerrainDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8 // 变xy
            // #define _FogColorC  float4(0.00208, 0.23016, 0.33588, 0.00017   ) //_64._m9 // 变xyz
            // #define _64__m10 float4(-0.001, 9.00, -0.001, 1.20191        ) //_64._m10 // 变w
            // #define _FogColorXYZ_FogVisibleDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
            #define _FogColorXYZ_FogVisibleDistanceW_ float4(_FogColorXYZ.xyz, _FogVisibleDistanceW.x              ) //_64._m11
            // #define _TerrainDistanceXYSO_ZW_DistanceSO_XY_ float4(1.00, 0.00, -0.01, 2.50              ) //_64._m12
            // #define _ExpDampingStartXZ_ float4(1.28117, 0.24777, 1.00, 0.00         ) //_64._m13 变xy
            // #define _FogDistanceLimitX_Y_ float4(1.00, 0.90, 0.00, 0.00               ) //_64._m14
            #define _MoonPos_maybe float4(_MoonPos_maybe_Pos.xyz, _MoonPos_maybe_W.x   ) //_64._m15
            // #define _ColorA2 float4(1.00, 1.00, 1.00, 0.07213            ) //_64._m16
            #define _64__m17 float4(1.00, -1.00, 10000.00, 0.00          ) //_64._m17
            #define _64__m18 float4(1.00, 1.00, 1.00, -16.00             ) //_64._m18
            #define _FogColorB float4(0.00, 0.00, 0.00, 0.00               ) //_64._m19
            // #define _64__m20 float4(0.00, 0.00, 0.00, 0.00               ) //_64._m20
            // #define _64__m21 0.00                                          // _64._m21
            #define _64__m22 float3(0.00, 0.00, 0.00)                      //_64._m22
            #define _64__m23 float4(0.00, 0.00, 0.00, 0.00 )               // _64._m23
            #define _64__m24 float4(0.00, 0.00, 0.00, 0.00 )               // _64._m24
            #define _64__m25 0.00                                          // _64._m25

            sampler2D _CameraDepthTexture;
            fixed4 frag (v2f i) : SV_Target
            {

                fixed4 col = fixed4(0, 0, 0, 1);

                float4 Output_0;
                float4 Output_1;
                
                float3 _24;
                bool _26;
                float3 _27;
                float _30;
                float3 _31;
                float4 _33;
                float _34;
                bool3 _37;
                float3 _41;
                float3 _43;
                bool _49;
                float _50;
                bool _57;

                float _59;
                
                float2 _screenPos01 = i.Varying_ScreenPos.xy / i.Varying_ScreenPos.w;

                float _rawDepth = tex2D(_CameraDepthTexture, _screenPos01).x;

                float _terrainLinear01Depth = Linear01Depth(_rawDepth);
                // 注：opengl 下
                //   _rawDepth 0 .. 1 ， 对应 _terrainLinear01Depth near/far .. 1，不会为 0

                // float3 _terrainWorldPos = (_terrainLinear01Depth * i.Varying_FarPlaneConner) + _WorldSpaceCameraPos;
                // float3 _terrainWorldPos_relativeToCamera = (_terrainLinear01Depth) * i.Varying_FarPlaneConner;
                float4 _terrainWorldPos = ReconstructWorldPositionFromDepth( _screenPos01, _rawDepth );
                float3 _terrainWorldPos_relativeToCamera = _terrainWorldPos - _WorldSpaceCameraPos;

                float _terrainEyeDepth = _terrainLinear01Depth * _ProjectionParams.z; // far plane

                // 注：opengl 下
                //   _terrainLinear01Depth near/far .. 1，对应 _terrainEyeDepth near .. far，不会为 0
                float _terrainToCamera_length = length(_terrainWorldPos_relativeToCamera);

                // #define _FogColorXYZ_FogVisibleDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
                if (_terrainToCamera_length < _FogColorXYZ_FogVisibleDistanceW_.w)
                {
                    // discard;
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
                // _if_output1 = 0.5;
                // _if_output2 = 0.5;
                

                // #define _TerrainYSO_XY_TerrainDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8
                float _terrainToCamera_length_SO1 = saturate(_terrainToCamera_length * _TerrainYSO_XY_TerrainDistanceSO_ZW_.z + _TerrainYSO_XY_TerrainDistanceSO_ZW_.w);

                float _terrainToCamera_length_SO2 = saturate(_terrainToCamera_length * _64__m18.z + _64__m18.w);

                float _terrainToCamera_length_SO = lerp( _terrainToCamera_length_SO1, _terrainToCamera_length_SO2, _if_output1 );
                
                // smooth 形式 0~1 快速上升后平缓到1
                float _terrainToCamera_length_SO_smooth01 = (2.0-_terrainToCamera_length_SO) * _terrainToCamera_length_SO;

                float _terrainToCameraXZ_length = length(_terrainWorldPos_relativeToCamera.xz);

                float _terrainToCameraXZ_length_SO1 = saturate(_terrainToCameraXZ_length * _64__m10.x + _64__m10.y);

                float _WorldSpaceCameraPosY_SO = saturate(_WorldSpaceCameraPos.y * _64__m10.z + _64__m10.w);

                bool _isSky = _terrainEyeDepth >= _ProjectionParams.z * 0.9999; // 没有深度了，太远了，是天空

                // #define _SkyFogDistanceScaleW_  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7
                float _fogXZDistance = _isSky ? _terrainToCamera_length_SO_smooth01 * _SkyFogDistanceScaleW_.w : _terrainToCamera_length_SO_smooth01;

                float _fogFactor = _isSky ? _WorldSpaceCameraPosY_SO : _terrainToCameraXZ_length_SO1;

                // #define _FogMainColorA  float4(0.09966, 0.37807, 0.79386, 1.1879    ) //_64._m3
                // #define _FogColorB float4(0.00, 0.00, 0.00, 0.00               ) //_64._m19
                float _colorw = lerp( _FogMainColorA.w, _FogColorB.w, _if_output1 );

                float _fogXZDistance_pow = pow(_fogXZDistance + 1e-04, _colorw);

                float _fogXZDistance_pow_limit1 = min(_fogXZDistance_pow, min(_FogDistanceColor.w * _FogDistanceLimitX_Y_.x, 1.0));

                float _terrainWorldPosY_SO = saturate(_terrainWorldPos.y * _TerrainYSO_XY_TerrainDistanceSO_ZW_.x + _TerrainYSO_XY_TerrainDistanceSO_ZW_.y);
                float _terrainWorldPosY_SO_smooth01 = _terrainWorldPosY_SO * (2.0 - _terrainWorldPosY_SO);

                float3 _fogColor = (_terrainWorldPosY_SO_smooth01 * _SkyFogDistanceScaleW_.xyz) + _FogDistanceColor.xyz;

                float3 _fogColor_2 = lerp(_fogColor, _FogColorB.xyz, _if_output1);

                float _terrainToCamera_length_OS = clamp((_terrainToCamera_length - _FogGradientFactorZ_.w) * _FogColorC.w, 0.0, 1.0);

                float3 _fogColor_3 = lerp(_fogColor_2, _FogColorC.xyz, _terrainToCamera_length_OS);

                float _terrainToCameraXZ_length_SO = saturate(_terrainToCameraXZ_length * _TerrainDistanceXYSO_ZW_DistanceSO_XY_.z + _TerrainDistanceXYSO_ZW_DistanceSO_XY_.w);

                float _lerp_55 = lerp(_ExpDampingScaleXZ_AffectYW.y, _ColorA2.w, _if_output2);

                // _ExpDampingStartXZ_.y = log(_ExpDampingStartXZ_.x)
                // in_pre_compute_a 是 log(_ExpDampingStartXZ_.x)
                //             也可以是 _ExpDampingStartXZ_.y
                // #define _ExpDampingScaleXZ_AffectYW  float4(0.045, 0.00376, 0.00, 0.00           ) //_64._m4
                // #define _ExpDampingStartXZ_ float4(1.28117, 0.24777, 1.00, 0.00         ) //_64._m13
                float _terrainHeightDiff_expDamping1 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _ExpDampingScaleXZ_AffectYW.x, _ExpDampingStartXZ_.x);
                float _terrainHeightDiff_expDamping2 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _ExpDampingScaleXZ_AffectYW.z, _ExpDampingStartXZ_.z);

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
                
                float _terrainToCamera_length_SOB_1 = saturate(_terrainToCamera_length * _FogGradientFactorZ_.x + _FogGradientFactorZ_.y);

                float _terrainToCamera_length_SOB_2 = saturate(_terrainToCamera_length * _64__m17.x + _64__m17.y);

                float _terrainToCamera_length_SOB = lerp(_terrainToCamera_length_SOB_1, _terrainToCamera_length_SOB_2, _if_output2);

                float _fogGradientFactor = lerp(_FogGradientFactorZ_.z, _64__m18.x, _if_output2);

                float _terrainToCamera_length_SOB_smooth = _terrainToCamera_length_SOB * (-_terrainToCamera_length_SOB + 2.0);

                float _fogFactorB = lerp(1.0, _terrainToCamera_length_SOB_smooth, _fogGradientFactor);

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

                float _terrainToCamera_length_SOC = saturate(_terrainToCamera_length * _TerrainDistanceXYSO_ZW_DistanceSO_XY_.x + _TerrainDistanceXYSO_ZW_DistanceSO_XY_.y);

                float _terrainToCamera_length_SOC_smooth = _terrainToCamera_length_SOC * (2.0 - _terrainToCamera_length_SOC);
                
                float _fogDistanceFactor = _terrainDistanceAffectByDamping2 * _terrainToCamera_length_SOC_smooth;

                float _fogDistanceFactor_2 = min(_fogDistanceFactor, _FogDistanceLimitX_Y_.y);

                float _fogFactorC = _fogFactor * _fogFactorB_3;

                float _fogFactorD = _terrainToCameraXZ_length_SO * _fogDistanceFactor_2;

                float3 _colorA = lerp(_FogMainColorA.xyz, _ColorA2.xyz, _if_output2);

                float3 _outputColor1 = _fogXZDistance_pow_limit1 * _fogColor_3;

                float3 _outputColor2 = (-_fogColor_3 * _fogXZDistance_pow_limit1) + _colorA;

                float3 _outputColor3 = (_fogFactorC * _outputColor2) + _outputColor1;

                _31 = (_FogColorXYZ_FogVisibleDistanceW_.xyz * _fogFactorD) + _outputColor3;

                float _outputAlpha = (1.0 - _fogXZDistance_pow_limit1) * (1.0 - _fogFactorD) * (1.0 - _fogFactorC);

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

                // return float4(_screenPos01.xy, 0, 1);
                // return float4(_rawDepth, _rawDepth, _rawDepth, 1);
                float __Z = frac(_terrainWorldPos.x*1);
                // return float4(__Z, __Z, __Z, 1);
                
                float3 worldPos = _terrainWorldPos*0.01;
                // The following part creates the checkerboard effect.
                // Scale is the inverse size of the squares.
                uint scale = 100;
                // Scale, mirror and snap the coordinates.
                uint3 worldIntPos = uint3(abs(worldPos.xyz * scale));
                // Divide the surface into squares. Calculate the color ID value.
                bool white = (worldIntPos.x & 1) ^ (worldIntPos.z & 1) /*^ (worldIntPos.y & 1)*/;
                // Color the square based on the ID value (black or white).
                half4 color = white ? half4(1,1,1,1) : half4(0,0,0,1);
                // return float4(color.xyz, 1.0);
                // return float4(_terrainWorldPos.xyz, 1.0);
                // return 
                // return color;
                // return float4(_screenPos01.xy, 0.0, 1.0);
                // return float4(_terrainEyeDepth.xxx/16, 1.0);
                // return float4(_terrainToCamera_length.xxx/16, 1.0);
                // return float4(_terrainLinear01Depth.xxx*1, 1.0);
                
                // col = float4(1.0f, 0, 0, 0.5);
                // col = Output_1;
                return col;
            }
            ENDCG
        }
    }
}

Shader "genship/postprocessfog_box_v3"
{
    Properties
    {
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
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend One SrcAlpha
            Cull Front
//            Cull Off
            ZWrite Off
            ZTest Always
            
            CGPROGRAM
            
            float3 _FogMainColor;
            float _FogDistancePow;
            #define _FogMainColorA float4(_FogMainColor.xyz, _FogDistancePow.x) // _64._m3 // 变xyzw
            
            // #define _ExpDampingScaleXZ_AffectYW  float4(0.045, 0.00376, 0.00, 0.00           ) //_64._m4 // 变y
            float4 _ExpDampingScaleXZ_AffectYW;
            
            // #define _FogGradientFactorZ_  float4(0.00391, -0.0625, 1.00, 1.00         ) //_64._m5
            float4 _FogGradientFactorZ_;
            
            // #define _FogDistanceColor  float4(0.00721, 0.1452, 0.38323, 0.90       ) //_64._m6 // 变xyz
            float4 _FogDistanceColor;
            
            // #define _SkyFogDistanceScaleW_  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7 // 变xyz
            float4 _SkyFogDistanceScaleW_;
            
            // #define _TerrainYSO_XY_TerrainDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8 // 变xy
            float4 _TerrainYSO_XY_TerrainDistanceSO_ZW_;
            
            // #define _FogColorC  float4(0.00208, 0.23016, 0.33588, 0.00017   ) //_64._m9 // 变xyz
            float4 _FogColorC;
            
            // #define _64__m10 float4(-0.001, 9.00, -0.001, 1.20191        ) //_64._m10 // 变w
            float4 _64__m10;
            
            // #define _FogColorXYZ_FogVisibleDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
            float3 _FogColorXYZ;
            float _FogVisibleDistanceW;
            #define _FogColorXYZ_FogVisibleDistanceW_ float4(_FogColorXYZ.xyz, _FogVisibleDistanceW.x              ) //_64._m11
            
            // #define _TerrainDistanceXYSO_ZW_DistanceSO_XY_ float4(1.00, 0.00, -0.01, 2.50              ) //_64._m12
            float4 _TerrainDistanceXYSO_ZW_DistanceSO_XY_;
            
            // #define _ExpDampingStartXZ_ float4(1.28117, 0.24777, 1.00, 0.00         ) //_64._m13 变xy
            float4 _ExpDampingStartXZ_;
            
            // #define _FogDistanceLimitX_Y_ float4(1.00, 0.90, 0.00, 0.00               ) //_64._m14
            float4 _FogDistanceLimitX_Y_;

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Common/shaderlib/common.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;

                // 原本用于传相对摄像机的世界坐标偏移
                // float2 uv : TEXCOORD0;
                // float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 Varying_ScreenPos : TEXCOORD0;
                
                // 原本用于传相对摄像机的世界坐标偏移
                // float3 Varying_FarPlaneConner : TEXCOORD1;
            };


            // opengl Blit full screen triangle UNITY_MATRIX_VP
            // static matrix _23__m2 = {
            //     2.00, 0.00, 0.00, 0.00    ,
            //     0.00, 2.00, 0.00, 0.00    ,
            //     0.00, 0.00, -0.00033, 0.00,
            //     -1.00, -1.00, -1.00, 1.00 ,
            // }; //_23._m2
            
            v2f vert (appdata v)
            {
                v2f o;

                // float4 Vertex_Position = v.vertex;
                // float3 Vertex_FarPlaneConner = float3(v.uv.xy, v.uv2.x);
                
                float4 _clipPos = UnityObjectToClipPos(v.vertex);
                o.vertex = _clipPos;
                
                float4 _screenPos = ComputeNonStereoScreenPos(_clipPos);
                o.Varying_ScreenPos = _screenPos.xyzw;

                // o.Varying_FarPlaneConner = Vertex_FarPlaneConner;
                
                return o;
            }

            
            sampler2D _CameraDepthTexture;
            fixed4 frag (v2f i) : SV_Target
            {

                fixed4 col = fixed4(0, 0, 0, 1);
                float4 value = float4(0,0,0,1);
                
                float4 Output_0;
                
                float2 _screenPos01 = i.Varying_ScreenPos.xy / i.Varying_ScreenPos.w;

                float _rawDepth = tex2D(_CameraDepthTexture, _screenPos01).x;

                float _terrainLinear01Depth = Linear01Depth(_rawDepth);
                // 注：_rawDepth 0 .. 1 ， 对应 _terrainLinear01Depth near/far .. 1，不会为 0

                // 原本用相对摄像机的世界坐标偏移顶点插值计算的 _terrainWorldPos 和 _terrainWorldPos_relativeToCamera
                // float3 _terrainWorldPos = (_terrainLinear01Depth * i.Varying_FarPlaneConner) + _WorldSpaceCameraPos;
                // float3 _terrainWorldPos_relativeToCamera = (_terrainLinear01Depth) * i.Varying_FarPlaneConner;
                float4 _terrainWorldPos = ReconstructWorldPositionFromDepth( _screenPos01, _rawDepth );
                float3 _terrainWorldPos_relativeToCamera = _terrainWorldPos - _WorldSpaceCameraPos;

                float _terrainEyeDepth = _terrainLinear01Depth * _ProjectionParams.z; // far plane

                // 注：_terrainLinear01Depth near/far .. 1，对应 _terrainEyeDepth near .. far，不会为 0
                float _terrainToCamera_length = length(_terrainWorldPos_relativeToCamera);

                // #define _FogColorXYZ_FogVisibleDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
                if (_terrainToCamera_length < _FogColorXYZ_FogVisibleDistanceW_.w)
                {
                    discard;
                }

                // #define _TerrainYSO_XY_TerrainDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8
                float _terrainToCamera_length_SO = saturate(_terrainToCamera_length * _TerrainYSO_XY_TerrainDistanceSO_ZW_.z + _TerrainYSO_XY_TerrainDistanceSO_ZW_.w);
                
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
                float _colorw = _FogMainColorA.w;

                float _fogXZDistance_pow = pow(_fogXZDistance + 1e-04, _colorw);

                float _fogXZDistance_pow_limit1 = min(_fogXZDistance_pow, min(_FogDistanceColor.w * _FogDistanceLimitX_Y_.x, 1.0));

                float _terrainWorldPosY_SO = saturate(_terrainWorldPos.y * _TerrainYSO_XY_TerrainDistanceSO_ZW_.x + _TerrainYSO_XY_TerrainDistanceSO_ZW_.y);
                float _terrainWorldPosY_SO_smooth01 = _terrainWorldPosY_SO * (2.0 - _terrainWorldPosY_SO);

                float3 _fogColor = (_terrainWorldPosY_SO_smooth01 * _SkyFogDistanceScaleW_.xyz) + _FogDistanceColor.xyz;

                float3 _fogColor_2 = _fogColor;

                float _terrainToCamera_length_OS = clamp((_terrainToCamera_length - _FogGradientFactorZ_.w) * _FogColorC.w, 0.0, 1.0);

                float3 _fogColor_3 = lerp(_fogColor_2, _FogColorC.xyz, _terrainToCamera_length_OS);

                float _terrainToCameraXZ_length_SO = saturate(_terrainToCameraXZ_length * _TerrainDistanceXYSO_ZW_DistanceSO_XY_.z + _TerrainDistanceXYSO_ZW_DistanceSO_XY_.w);


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
                    _tmp_48 = _terrainToCamera_length * _ExpDampingScaleXZ_AffectYW.y;
                    _tmp_48 = _tmp_48 * (-_terrainHeightDiff_expDamping1);
                    _tmp_48 = 1.0 - exp2(_tmp_48);
                    _tmp_48 = max(_tmp_48, 0.0);
                    _terrainDistanceAffectByDamping1 = _tmp_48;
                }
                
                float _terrainToCamera_length_SOB = saturate(_terrainToCamera_length * _FogGradientFactorZ_.x + _FogGradientFactorZ_.y);

                float _fogGradientFactor = _FogGradientFactorZ_.z;

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

                float3 _colorA = _FogMainColorA.xyz;

                float3 _outputColor1 = _fogXZDistance_pow_limit1 * _fogColor_3;

                float3 _outputColor2 = (-_fogColor_3 * _fogXZDistance_pow_limit1) + _colorA;

                float3 _outputColor3 = (_fogFactorC * _outputColor2) + _outputColor1;

                float3 _outputColor = (_FogColorXYZ_FogVisibleDistanceW_.xyz * _fogFactorD) + _outputColor3;

                float _outputAlpha = (1.0 - _fogXZDistance_pow_limit1) * (1.0 - _fogFactorD) * (1.0 - _fogFactorC);

                Output_0.xyz = _outputColor;
                Output_0.w = _outputAlpha;

                col = Output_0;

                // Test WorldPos Grid
                // {
                //     float3 worldPos = _terrainWorldPos*0.01;
                //     // The following part creates the checkerboard effect.
                //     // Scale is the inverse size of the squares.
                //     uint scale = 1;
                //     // Scale, mirror and snap the coordinates.
                //     uint3 worldIntPos = uint3(abs(worldPos.xyz * scale));
                //     // Divide the surface into squares. Calculate the color ID value.
                //     bool white = (worldIntPos.x & 1) ^ (worldIntPos.z & 1) /*^ (worldIntPos.y & 1)*/;
                //     // Color the square based on the ID value (black or white).
                //     half4 color = white ? half4(1,1,1,1) : half4(0,0,0,1);
                //     return color;
                // }
                
                return col;
            }
            ENDCG
        }
    }
}

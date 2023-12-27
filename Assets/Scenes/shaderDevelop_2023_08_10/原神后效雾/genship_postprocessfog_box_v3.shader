Shader "genship/postprocessfog_box_v3"
{
    Properties
    {
        [Header(Basic)]
            _FogDistanceColor ("_FogDistanceColor", Color) = (0.00721, 0.1452, 0.38323, 0.90)
            _FogDistanceColorBlend ("_FogDistanceColorBlend", Color) = (0.00208, 0.23016, 0.33588, 0.00017)
            _SkyHeightSubColor ("_SkyHeightSubColor", Vector) = (0.02258, 0.01951, -0.08341, 0.00)
            
            _FogDistancePow ("_FogDistancePow", Range(0, 2)) = 1.1879
            
            _BasicPowDistanceSO ("_FogPowDistanceSO", Vector) = (0.00042, -0.0067, 0, 0)
            _BasicHeightSO ("_BasicHeightSO", Vector) = (0.00393, -0.79396, 0, 0)
        
        [Header(MainColor)]
            _FogMainColor ("_FogMainColor", Color) = (0.09966, 0.37807, 0.79386, 1.0) 
            _MainDistanceSO ("_MainDistanceSO", Vector) = (-0.001, 9.00, 0, 0)
            _MainHeightSO ("_MainHeightSO", Vector) = (-0.001, 1.20191, 0, 0)
        
            _FogHeightFactorA ("_FogHeightFactorA", Range(0.01, 3)) = 1.28117
            _FogHeightDampingScaleA ("_FogHeightDampingScaleA", Range(0, 0.1)) = 0.045 
            _FogDistanceDampingScaleA ("_FogDistanceDampingScaleA", Range(0, 0.01)) = 0.00376
        
        [Header(AdditionColor)]
            _FogColorAddition ("_FogColorAddition", Color) = (1.00, 1.00, 1.00, 1.00)
            _AdditionFogDistanceSO_1_2 ("_AdditionFogDistanceSO_1_2", Vector) = (1.00, 0.00, -0.01, 2.50)
            _FogHeightFactorB ("_FogHeightFactorB", Range(0.01, 3)) = 1
            _FogHeightDampingScaleB ("_FogHeightDampingScaleB", Range(0, 0.1)) = 0.00 
            _FogDistanceDampingScaleB ("_FogDistanceDampingScaleB", Range(0, 0.01)) = 0.00
        
        [Header(Misc)]
            _FogDiscardDistance ("_FogDiscardDistance", Range(0, 100)) = 16
        
//        /*_m13变xy  */ _ExpDampingStartXZ_ ("_ExpDampingStartXZ_", Vector) = (1.28117, 0.24777, 1.00, 0.00)
        
        
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

            // /* basic 基础的高度 距离偏移控制着色
            float4 _FogDistanceColor; // float4(0.00721, 0.1452, 0.38323, 0.90       ) //_64._m6 // 变xyz
            float4 _FogDistanceColorBlend; // float4(0.00208, 0.23016, 0.33588, 0.00017   ) //_64._m9 // 变xyz
            float4 _SkyHeightSubColor; // float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7 // 变xyz
            
            float _FogDistancePow; // _64._m3.w // 变
            
            float2 _BasicPowDistanceSO; // float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8.zw
            float2 _BasicHeightSO;  //_64._m8.xy // 变xy
            // -------------------------- */


            
            // /* main 主要的，会变的，指数衰减的雾效
            float3 _FogMainColor; // float4(_FogMainColor.xyz, _FogDistancePow.x) // _64._m3 // 变xyzw
            float2 _MainDistanceSO; // _64._m10.xy // float4(-0.001, 9.00, -0.001, 1.20191        )
            float2 _MainHeightSO;   // _64._m10.zw // 变w
            
            float _FogHeightFactorA; //_64._m13.x // float4(1.28117, 0.24777, 1.00, 0.00         ) // 变xy
            float _FogHeightDampingScaleA;  //_64._m4.x // float4(0.045, 0.00376, 0.00, 0.00           )
            float _FogDistanceDampingScaleA;//_64._m4.y // 变y
            // -------------------------- */

            

            // /* addition 次要的，原神抓帧没生效的
            float3 _FogColorAddition; //_64._m11.xyz // float4(1.00, 1.00, 1.00, 16.00              )
            float4 _AdditionFogDistanceSO_1_2; // float4(1.00, 0.00, -0.01, 2.50              ) //_64._m12
            float _FogHeightFactorB; //_64._m13.z
            float _FogHeightDampingScaleB;  //_64._m4.z
            float _FogDistanceDampingScaleB;//_64._m4.w
            // -------------------------- */
            


            // /* misc 其他
            float _FogDiscardDistance; //_64._m11.w // 16.00

            // #define _FogGradientFactorZ_  float4(0.00391, -0.0625, 1.00, 1.00         ) //_64._m5
            #define _ComplexCalcFogDistanceSO float2(0.00391, -0.0625) // x y
            #define _ComplexCalcFog (1.0) // z
            #define _FogGradientFactorZ_W (1.0) // w
            
            // #define _FogDistanceLimitX_Y_ float4(1.00, 0.90, 0.00, 0.00               ) //_64._m14
            #define _FogDistanceLimitX_ 1.0
            #define _FogDistanceLimitY_ 0.9
            // -------------------------- */


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

                bool _isSky = _terrainEyeDepth >= _ProjectionParams.z * 0.9999; // 没有深度了，太远了，是天空
                
                // 注：_terrainLinear01Depth near/far .. 1，对应 _terrainEyeDepth near .. far，不会为 0
                float _terrainToCamera_length = length(_terrainWorldPos_relativeToCamera);
                float _terrainToCameraXZ_length = length(_terrainWorldPos_relativeToCamera.xz);
                
                // #define _FogColorXYZ_FogVisibleDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
                if (_terrainToCamera_length < _FogDiscardDistance)
                {
                    discard;
                }

                // distance 从近到远 从小到大
                float _fogXZDistance_pow_limit1;
                {
                    // #define _TerrainYSO_XY_FogPowDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8
                    float _terrainToCamera_length_SO = saturate(_terrainToCamera_length * _BasicPowDistanceSO.x + _BasicPowDistanceSO.y);
                    // smooth 形式 0~1 快速上升后平缓到1
                    float _terrainToCamera_length_SO_smooth01 = (2.0-_terrainToCamera_length_SO) * _terrainToCamera_length_SO;
                    // #define _SkyHeightSubColor  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7
                    float _fogXZDistance = _isSky ? _terrainToCamera_length_SO_smooth01 * _SkyHeightSubColor.w : _terrainToCamera_length_SO_smooth01;
                    // #define _FogMainColorA  float4(0.09966, 0.37807, 0.79386, 1.1879    ) //_64._m3

                    float _fogXZDistance_pow = pow(_fogXZDistance + 1e-04, _FogDistancePow);
                    _fogXZDistance_pow_limit1 = min(_fogXZDistance_pow, min(_FogDistanceColor.w * _FogDistanceLimitX_, 1.0));
                }

                float3 _distanceColor_3;
                {
                    float _terrainWorldPosY_SO = saturate(_terrainWorldPos.y * _BasicHeightSO.x + _BasicHeightSO.y);
                    float _terrainWorldPosY_SO_smooth01 = _terrainWorldPosY_SO * (2.0 - _terrainWorldPosY_SO);

                    float3 _skyHeightColor = (_terrainWorldPosY_SO_smooth01 * _SkyHeightSubColor.xyz) + _FogDistanceColor.xyz;

                    float _terrainToCamera_length_OS = clamp((_terrainToCamera_length - _FogGradientFactorZ_W) * _FogDistanceColorBlend.w, 0.0, 1.0);

                    float3 _distanceColor_2 = lerp(_skyHeightColor, _FogDistanceColorBlend.xyz, _terrainToCamera_length_OS);
                    
                    _distanceColor_3 = _fogXZDistance_pow_limit1 * _distanceColor_2;
                }

                float _fogFactorMain;
                {
                    // 简易高度缩放和距离缩放
                    float _fogFactorSimple;
                    {
                        float _terrainToCameraXZ_length_SO1 = saturate(_terrainToCameraXZ_length * _MainDistanceSO.x + _MainDistanceSO.y);

                        float _WorldSpaceCameraPosY_SO = saturate(_WorldSpaceCameraPos.y * _MainHeightSO.x + _MainHeightSO.y);
                        
                        _fogFactorSimple = _isSky ? _WorldSpaceCameraPosY_SO : _terrainToCameraXZ_length_SO1;
                    }
                    
                    // 输出：_terrainDistanceAffectByDamping1
                    //   距离控制的雾强度，近 0，远接近 1，先快速上升后平缓到 1
                    // 变量：
                    //   1. _terrainHeightDiff_expDamping1:
                    //      因高度引起的最终 1 下调，高度越高，最终越不能接近于 1 (变成 0)
                    // 详见：
                    //   show_distance_affectbydamping.hip
                    float _terrainDistanceAffectByDamping1;
                    {
                        float _terrainHeightDiff_expDamping1 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _FogHeightDampingScaleA/*_ExpDampingScaleXZ_AffectYW.x*/, _FogHeightFactorA/*_ExpDampingStartXZ_.x*/);
                        float _tmp_48;
                        _tmp_48 = _terrainToCamera_length * _FogDistanceDampingScaleA/*_ExpDampingScaleXZ_AffectYW.y*/;
                        _tmp_48 = _tmp_48 * (-_terrainHeightDiff_expDamping1);
                        _tmp_48 = 1.0 - exp2(_tmp_48);
                        _tmp_48 = max(_tmp_48, 0.0);
                        _terrainDistanceAffectByDamping1 = _tmp_48;
                    }
                    
                    float _terrainToCamera_length_SOB = saturate(_terrainToCamera_length * _ComplexCalcFogDistanceSO.x + _ComplexCalcFogDistanceSO.y);
                    float _terrainToCamera_length_SOB_smooth = _terrainToCamera_length_SOB * (-_terrainToCamera_length_SOB + 2.0);

                    // 固定是 _terrainToCamera_length_SOB_smooth
                    float _terrainDistanceAffectByDamping1Factor = lerp(1.0, _terrainToCamera_length_SOB_smooth, _ComplexCalcFog);

                    float _terrainDistanceAffectByDamping1Fix = _terrainDistanceAffectByDamping1Factor * _terrainDistanceAffectByDamping1;
                    float _fogFactorComplex = min(_terrainDistanceAffectByDamping1Fix, _FogDistanceColor.w);
                    
                    _fogFactorMain = _fogFactorSimple * _fogFactorComplex;
                }
                
                float _fogFactorAddition;
                {
                    float _terrainDistanceAffectByDamping2;
                    {
                        float _terrainHeightDiff_expDamping2 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _FogHeightDampingScaleB/*_ExpDampingScaleXZ_AffectYW.z*/, _FogHeightFactorB/*_ExpDampingStartXZ_.z*/);
                        float _tmp_55;
                        _tmp_55 = _terrainToCamera_length * _FogDistanceDampingScaleB/*_ExpDampingScaleXZ_AffectYW.w*/;
                        _tmp_55 = _tmp_55 * (-_terrainHeightDiff_expDamping2);
                        _tmp_55 = 1.0 - exp2(_tmp_55);
                        _tmp_55 = max(_tmp_55, 0.0);
                        _terrainDistanceAffectByDamping2 = _tmp_55;
                    }

                    float _terrainToCamera_length_SOC = saturate(_terrainToCamera_length * _AdditionFogDistanceSO_1_2.x + _AdditionFogDistanceSO_1_2.y);

                    float _terrainToCamera_length_SOC_smooth = _terrainToCamera_length_SOC * (2.0 - _terrainToCamera_length_SOC);
                    
                    float _fogDistanceFactor = _terrainDistanceAffectByDamping2 * _terrainToCamera_length_SOC_smooth;

                    float _fogDistanceFactor_2 = min(_fogDistanceFactor, _FogDistanceLimitY_);

                    float _terrainToCameraXZ_length_SO = saturate(_terrainToCameraXZ_length * _AdditionFogDistanceSO_1_2.z + _AdditionFogDistanceSO_1_2.w);
                    
                    _fogFactorAddition = _terrainToCameraXZ_length_SO * _fogDistanceFactor_2;
                }

                float3 _mainColor = lerp(_distanceColor_3, _FogMainColor, _fogFactorMain);

                float3 _additionColor = _FogColorAddition * _fogFactorAddition;
                
                float3 _outputColor = _mainColor + _additionColor;

                float _outputAlpha = (1.0 - _fogXZDistance_pow_limit1) * (1.0 - _fogFactorAddition) * (1.0 - _fogFactorMain);

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

                // return value;
                return col;
            }
            ENDCG
        }
    }
}

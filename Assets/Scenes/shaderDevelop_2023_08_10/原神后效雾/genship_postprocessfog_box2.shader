Shader "genship/postprocessfog_box2"
{
    Properties
    {
        [Header(Basic)]
        /*_m6 变xyz */ _FogDistanceColor ("_FogDistanceColor", Color) = (0.05654, 0.13585, 0.35222, 0.92)
        /*_m9 变xyz */ _FogDistanceColorBlend ("_FogDistanceColorBlend", Color) = (0.46001, 0.42273, 0.46656, 0.00017)
        /*_m9.w     */ _FogDistanceColorBlendA ("_FogDistanceColorBlendA", Range(0, 0.001)) = 0.00017
        /*_m7 变xyz */ _SkyHeightSubColor ("_SkyHeightSubColor", Vector) = (-0.00127, 0.0048, -0.13633, 0.00)
        
        /*_m3 变w   */ _FogDistancePow ("_FogDistancePow", Range(0, 2)) = 1.08365
        
        /*_m8 变xy  */ //_TerrainYSO_XY_TerrainDistanceSO_ZW_ ("_TerrainYSO_XY_TerrainDistanceSO_ZW_", Vector) = (0.00147, -0.26394, 0.00047, -0.00707)
                       _BasicPowDistanceSO ("_FogPowDistanceSO", Vector) = (0.00047, -0.00707, 0, 0)
                       _BasicHeightSO ("_BasicHeightSO", Vector) = (0.00147, -0.26394, 0, 0)
        
        
        [Header(MainColor)]
        /*_m3 变xyz */ _FogMainColor ("_FogMainColor", Color) = (0.77914, 0.60906, 0.52263, 1.08365) 
        /*_m10变w   */ // _64__m10 ("_64__m10", Vector) = (-0.00033, 2.44722, -0.001, 1.17958)
                       _MainDistanceSO ("_MainDistanceSO", Vector) = (-0.00033, 2.44722, 0, 0)
                       _MainHeightSO ("_MainHeightSO", Vector) = (-0.001, 1.17958, 0, 0)
        
        /*_m13变xy  */ // _ExpDampingStartXZ_ ("_ExpDampingStartXZ_", Vector) = (0.12955, -2.04365, 0.03779, -3.27578)
        /*_m13.x    */ _FogHeightFactorA ("_FogHeightFactorA", Range(0.01, 3)) = 0.12955
        
        /*_m4 变y   */ // _ExpDampingScaleXZ_AffectYW ("_ExpDampingScaleXZ_AffectYW", Vector) = (0.09159, 0.02426, 0.04, 0.00) 
        /*_m4.x     */ _FogHeightDampingScaleA ("_FogHeightDampingScaleA", Range(0, 0.1)) = 0.09159
        /*_m4.y     */ _FogDistanceDampingScaleA ("_FogDistanceDampingScaleA", Range(0, 0.01)) = 0.02426
        
        
//        [Header(AdditionColor)]
//                       _FogColorAddition ("_FogColorAddition", Color) = (0.32624, 0.22609, 0.19079, 15.00)
//                       _AdditionFogDistanceSO_1_2 ("_AdditionFogDistanceSO_1_2", Vector) = (0.002, 0.00, -0.0005, 1.07681)
//        /*_m13.z    */ _FogHeightFactorB ("_FogHeightFactorB", Range(0.01, 3)) = 0.03779
//        /*_m4.z     */ _FogHeightDampingScaleB ("_FogHeightDampingScaleB", Range(0, 0.1)) = 0.04
//        /*_m4.w     */ _FogDistanceDampingScaleB ("_FogDistanceDampingScaleB", Range(0, 0.01)) = 0.00
        
        [Header(Misc)]
                       _FogDiscardDistance ("_FogDiscardDistance", Range(0, 32)) = 15.00

        /*_m5       */ //_FogGradientFactorZ_ ("_FogGradientFactorZ_", Vector) = (0.00839, -0.12581, 1.00, 1.00)
        /*_m5.xy    */ _ComplexCalcFogDistanceSO ("_ComplexCalcFogDistanceSO", Vector) = (0.00839, -0.12581, 0, 0)
        
        
        /*_m14      */ // _FogDistanceLimitX_Y_ ("_FogDistanceLimitX_Y_", Vector) = (1.00, 1.00, 0.00, 0.00)
        /*_m14.x    */ _FogDistanceLimitX_ ("_FogDistanceLimitX_", Range(0, 1)) = 1
        /*_m14.y    */ _FogDistanceLimitY_ ("_FogDistanceLimitY_", Range(0, 1)) = 1

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend One SrcAlpha
            Cull Front
            ZWrite Off
            ZTest Always
            
            CGPROGRAM
            // /* basic 基础的高度 距离偏移控制着色
            float4 _FogDistanceColor;
            float3 _FogDistanceColorBlend;
            float _FogDistanceColorBlendA;
            float4 _SkyHeightSubColor;
            
            float _FogDistancePow;
            
            // float4 _TerrainYSO_XY_TerrainDistanceSO_ZW_;
                float2 _BasicPowDistanceSO;
                float2 _BasicHeightSO;
            // -------------------------- */


            
            // /* main 主要的，会变的，指数衰减的雾效
            float3 _FogMainColor;
            float2 _MainDistanceSO;
            float2 _MainHeightSO;
            
            float _FogHeightFactorA;
            float _FogHeightDampingScaleA;
            float _FogDistanceDampingScaleA;
            // -------------------------- */

            

            // // /* addition 次要的
            // float3 _FogColorAddition;
            // float4 _AdditionFogDistanceSO_1_2;
            // float _FogHeightFactorB;
            // float _FogHeightDampingScaleB;
            // float _FogDistanceDampingScaleB;
            // // -------------------------- */



            // /* misc 其他
            float _FogDiscardDistance;

            // float4 _FogGradientFactorZ_;
                float2 _ComplexCalcFogDistanceSO; // x y
                #define _ComplexCalcFog (1.0) // z
                #define _FogGradientFactorZ_W (1.0) // w

            // float4 _FogDistanceLimitX_Y_;
                float _FogDistanceLimitX_;
                float _FogDistanceLimitY_;
            // // -------------------------- */
            
            sampler2D _CameraDepthTexture;
            
            
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
                // float3 Varying_FarPlaneConner : TEXCOORD0;
                float4 Varying_ScreenPos : TEXCOORD1;
            };
            
            
            v2f vert (appdata v)
            {
                v2f o;

                float4 Vertex_Position = v.vertex;
                
                float4 _clipPos = UnityObjectToClipPos(v.vertex);
                
                float4 _screenPos = ComputeNonStereoScreenPos(_clipPos);

                o.Varying_ScreenPos = _screenPos.xyzw;
                
                o.vertex = _clipPos;
                
                return o;
            }
            

            
            fixed4 frag (v2f i) : SV_Target
            {

                fixed4 col = fixed4(0, 0, 0, 1);

                float4 Output_0;
                
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

                bool _isSky = _terrainEyeDepth >= _ProjectionParams.z * 0.9999; // 没有深度了，太远了，是天空

                // 注：opengl 下
                //   _terrainLinear01Depth near/far .. 1，对应 _terrainEyeDepth near .. far，不会为 0
                float _terrainToCamera_length = length(_terrainWorldPos_relativeToCamera);
                float _terrainToCameraXZ_length = length(_terrainWorldPos_relativeToCamera.xz);

                // #define _FogColorXYZ_FogVisibleDistanceW_ float4(1.00, 1.00, 1.00, 16.00              ) //_64._m11
                if (_terrainToCamera_length < _FogDiscardDistance)
                {
                    discard;
                }
                
                float _fogXZDistance_pow_limit1;
                {
                    // #define _TerrainYSO_XY_TerrainDistanceSO_ZW_  float4(0.00393, -0.79396, 0.00042, -0.00671 ) //_64._m8
                    float _terrainToCamera_length_SO = saturate(_terrainToCamera_length * _BasicPowDistanceSO.x + _BasicPowDistanceSO.y);
                    // smooth 形式 0~1 快速上升后平缓到1
                    float _terrainToCamera_length_SO_smooth01 = (2.0-_terrainToCamera_length_SO) * _terrainToCamera_length_SO;
                    // #define _SkyHeightSubColor  float4(0.02258, 0.01951, -0.08341, 0.00     ) //_64._m7
                    float _fogXZDistance = _isSky ? _terrainToCamera_length_SO_smooth01 * _SkyHeightSubColor.w : _terrainToCamera_length_SO_smooth01;
                    float _fogXZDistance_pow = pow(_fogXZDistance + 1e-04, _FogDistancePow);

                    _fogXZDistance_pow_limit1 = min(_fogXZDistance_pow, min(_FogDistanceColor.w * _FogDistanceLimitX_, 1.0));
                }

                float3 _distanceColor_3;
                {
                    float _terrainWorldPosY_SO = saturate(_terrainWorldPos.y * _BasicHeightSO.x + _BasicHeightSO.y);
                    float _terrainWorldPosY_SO_smooth01 = _terrainWorldPosY_SO * (2.0 - _terrainWorldPosY_SO);

                    float3 _skyHeightColor = (_terrainWorldPosY_SO_smooth01 * _SkyHeightSubColor.xyz) + _FogDistanceColor.xyz;

                    float _terrainToCamera_length_OS = clamp((_terrainToCamera_length - _FogGradientFactorZ_W) * _FogDistanceColorBlendA, 0.0, 1.0);

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
                        // _ExpDampingStartXZ_.y = log(_ExpDampingStartXZ_.x)
                        // in_pre_compute_a 是 log(_ExpDampingStartXZ_.x)
                        //             也可以是 _ExpDampingStartXZ_.y
                        // #define _ExpDampingScaleXZ_AffectYW  float4(0.045, 0.00376, 0.00, 0.00           ) //_64._m4
                        // #define _ExpDampingStartXZ_ float4(1.28117, 0.24777, 1.00, 0.00         ) //_64._m13
                        float _terrainHeightDiff_expDamping1 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _FogHeightDampingScaleA, _FogHeightFactorA);
                        float _tmp_48;
                        _tmp_48 = _terrainToCamera_length * _FogDistanceDampingScaleA;
                        _tmp_48 = _tmp_48 * (-_terrainHeightDiff_expDamping1);
                        _tmp_48 = 1.0 - exp2(_tmp_48);
                        _tmp_48 = max(_tmp_48, 0.0);
                        _terrainDistanceAffectByDamping1 = _tmp_48;
                    }

                    float _terrainToCamera_length_SOB = saturate(_terrainToCamera_length * _ComplexCalcFogDistanceSO.x + _ComplexCalcFogDistanceSO.y);
                    float _terrainToCamera_length_SOB_smooth = _terrainToCamera_length_SOB * (-_terrainToCamera_length_SOB + 2.0);
                    
                    float _terrainDistanceAffectByDamping1Factor = lerp(1.0, _terrainToCamera_length_SOB_smooth, _ComplexCalcFog);

                    float _terrainDistanceAffectByDamping1Fix = _terrainDistanceAffectByDamping1Factor * _terrainDistanceAffectByDamping1;
                    
                    float _fogFactorComplex = min(_terrainDistanceAffectByDamping1Fix, _FogDistanceColor.w);
                    
                    _fogFactorMain = _fogFactorSimple * _fogFactorComplex;
                }

                // 原神抓帧昼夜实际这部分没有作用
                // float _fogFactorAddition;
                // {
                //     float _terrainDistanceAffectByDamping2;
                //     {
                //         float _terrainHeightDiff_expDamping2 = ExpDamping(_terrainWorldPos_relativeToCamera.y * _FogHeightDampingScaleB, _FogHeightFactorB);
                //         float _tmp_55;
                //         _tmp_55 = _terrainToCamera_length * _FogDistanceDampingScaleB;
                //         _tmp_55 = _tmp_55 * (-_terrainHeightDiff_expDamping2);
                //         _tmp_55 = 1.0 - exp2(_tmp_55);
                //         _tmp_55 = max(_tmp_55, 0.0);
                //         _terrainDistanceAffectByDamping2 = _tmp_55;
                //     }
                //
                //     float _terrainToCamera_length_SOC = saturate(_terrainToCamera_length * _AdditionFogDistanceSO_1_2.x + _AdditionFogDistanceSO_1_2.y);
                //
                //     float _terrainToCamera_length_SOC_smooth = _terrainToCamera_length_SOC * (2.0 - _terrainToCamera_length_SOC);
                //     
                //     float _fogDistanceFactor = _terrainDistanceAffectByDamping2 * _terrainToCamera_length_SOC_smooth;
                //
                //     float _fogDistanceFactor_2 = min(_fogDistanceFactor, _FogDistanceLimitY_);
                //
                //     float _terrainToCameraXZ_length_SO = saturate(_terrainToCameraXZ_length * _AdditionFogDistanceSO_1_2.z + _AdditionFogDistanceSO_1_2.w);
                //     
                //     _fogFactorAddition = _terrainToCameraXZ_length_SO * _fogDistanceFactor_2;
                // }
                
                float3 _mainColor = lerp(_distanceColor_3, _FogMainColor, _fogFactorMain);

                // float3 _additionColor = _FogColorAddition * _fogFactorAddition;
                
                float3 _outputColor = _mainColor /*+ _additionColor*/;

                float _outputAlpha = (1.0 - _fogXZDistance_pow_limit1) /** (1.0 - _fogFactorAddition)*/ * (1.0 - _fogFactorMain);
  
                Output_0.xyz = _outputColor;
                Output_0.w = _outputAlpha;

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


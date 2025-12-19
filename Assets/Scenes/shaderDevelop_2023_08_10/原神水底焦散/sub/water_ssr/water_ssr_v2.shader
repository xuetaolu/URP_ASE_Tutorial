Shader "genship/water_ssr_v2"
{
    Properties
    {
        _reflectInfiniteDistanceX_and_AlphaY("_reflectInfiniteDistanceX_and_AlphaY", Vector) = (129.00, 1.00, 0.00, 0.00)
        _InfiniteScreenPos_ST("_InfiniteScreenPos_ST", Vector) = (1.47305, 0.82843, -0.73652, -0.41421)
        _Linear_Tracing_AMOUNT_LENGTH("_Linear_Tracing_AMOUNT_LENGTH", Range(0, 17)) = 17
//        _DEBUG_VALUE_1("_DEBUG_VALUE_1", Range(0, 1)) = 0
        _DEBUG_VALUE_RAYCAST_BIAS("_DEBUG_VALUE_RAYCAST_BIAS", Range(0, 3.1875)) = 3.1875
        [Toggle(_ShowDebugValue)]_ShowDebugValue("_ShowDebugValue", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderQueue" = "Transparent" }
        LOD 100
        Blend One Zero
        ZWrite Off
        
//        GrabPass { "_CameraOpaqueTexture" }

        Pass
        {
//            Tags {"LightMode" = "UniversalForwardOnly"}
            HLSLPROGRAM
            float4 _reflectInfiniteDistanceX_and_AlphaY;
            float4 _InfiniteScreenPos_ST;
            float _Linear_Tracing_AMOUNT_LENGTH;
            float _DEBUG_VALUE_1;
            float _DEBUG_VALUE_RAYCAST_BIAS;

            sampler2D _CameraOpaqueTexture;
            sampler2D _CameraDepthTextureLinear01DownSize;
            
            // #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ _ShowDebugValue

            // #define REQUIRE_DEPTH_TEXTURE 1
            // #define REQUIRE_OPAQUE_TEXTURE 1

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 Varying_worldPos : TEXCOORD0;
                float3 Varying_WorldNormal : TEXCOORD1;
                float4 Varying_NotNormaizeScreenPosXYW : TEXCOORD2;
            };
            
            // float4 ComputeNonStereoScreenPos(float4 pos) {
            //     float4 o = pos * 0.5;
            //     o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w;
            //     o.zw = pos.zw;
            //     return o;
            //     // return ComputeScreenPos(pos);
            // }


            v2f vert (appdata v)
            {
                v2f o;

                // float3 _WorldPos = TransformObjectToWorld( v.vertex.xyz );
                float3 _WorldPos = mul(UNITY_MATRIX_M, float4( v.vertex.xyz, 1 ));
                o.Varying_worldPos.xyz = _WorldPos;
				// float4 _ClipPos = TransformWorldToHClip( _WorldPos );
                float4 _ClipPos = UnityWorldToClipPos(_WorldPos);
                o.vertex = _ClipPos;
                
                // o.Varying_WorldNormal = TransformObjectToWorldNormal(v.normal);
                o.Varying_WorldNormal = UnityObjectToWorldNormal(v.normal);
                
                o.Varying_NotNormaizeScreenPosXYW = ComputeNonStereoScreenPos(o.vertex);

                return o;
            }

            float2 NormalizeScreenPos(float3 xyw) {
                return xyw.xy / xyw.zz;
            }

            float MinTo01Edge2D( float2 in01_2D) {
                float2 oneMinus = 1.0 - in01_2D;
                float2 minPerXY = min(in01_2D, oneMinus);
                return min(minPerXY.y, minPerXY.x);
            }

            float CalcFadeToScreenEdge(float2 screenPos) {
                float minToScreenEdge = MinTo01Edge2D(screenPos);

                bool _bIsCloseToScreenEdgePixel;
                _bIsCloseToScreenEdgePixel = 0.15 < minToScreenEdge;
                
                float minToScreenEdge6_66 = clamp(minToScreenEdge * 6.666666666, 0.0, 1.0);

                return max(0.02, _bIsCloseToScreenEdgePixel ? 1.0 : minToScreenEdge6_66) ;
            }

            // 自适应正交/透视获取线性深度 01，从摄像机到 far plane 的
            float Linear01DepthAdaptive(float z)
            {
                float isOrtho = unity_OrthoParams.w;
                float isPers = 1.0 - unity_OrthoParams.w;
                z *= _ZBufferParams.x;
                return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);

                // _CameraDepthTextureLinear01DownSize01DownSize 已经 Linear01
                // return z;
            }

            float SampleSceneDepth(float2 uv)
            {
                return tex2D(_CameraDepthTextureLinear01DownSize, uv).r;
            }

            float3 SampleSceneColor(float2 uv)
            {
                return tex2D(_CameraOpaqueTexture, uv).rgb;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            #define Linear_Tracing_MAX_STEP 8
            #define Linear_Tracing_AMOUNT_LENGTH _Linear_Tracing_AMOUNT_LENGTH //17
            
            float4 frag (v2f input) : SV_Target
            {
                float4 value = float4(0, 0, 0, 1);
                float3 viewDir = (-input.Varying_worldPos.xyz) + _WorldSpaceCameraPos;

                float3 viewDir_normalize = normalize(viewDir);

                float3 viewInDir = -viewDir_normalize;
                
                float3 reflectDir = reflect(viewInDir, input.Varying_WorldNormal);
                
                reflectDir = normalize(reflectDir);

            // 1. raycast 的逻辑，尝试能不能 raycast 到，
                float3 _raycast_end = (reflectDir * Linear_Tracing_AMOUNT_LENGTH) + input.Varying_worldPos.xyz;

                float4 _raycast_end_clipPos = UnityWorldToClipPos(_raycast_end);

                float4 _raycast_end_NonStereoScreenPos = ComputeNonStereoScreenPos(_raycast_end_clipPos);

                float3 _raycast_step_NonStereoScreenPos_Delta = _raycast_end_NonStereoScreenPos.xyw - input.Varying_NotNormaizeScreenPosXYW.xyw;
                
                float _raycast_hit_at_step = 0.0;
                
                // 调试看看最终反射的屏幕坐标相对像素坐标是否异常，例如出现 w <0 会向下反射
                // value.rg = NormalizeScreenPos(_raycast_end_NonStereoScreenPos.xyw) - NormalizeScreenPos(input.Varying_NotNormaizeScreenPosXYW.xyw);
                
                // Linear Tracing Method 8 step,
                UNITY_UNROLL
                for (int i = 0; i < Linear_Tracing_MAX_STEP; i++) {
                    int currentStepInt = i+1;
                    // float currentStepInt = i+0.5;
                    float currentStep01 = float(currentStepInt)/float(Linear_Tracing_MAX_STEP);
                    
                    float3 currentNonStereoScreenPos = _raycast_step_NonStereoScreenPos_Delta * currentStep01 + input.Varying_NotNormaizeScreenPosXYW.xyw;
                    float2 currentScreenPos = NormalizeScreenPos( currentNonStereoScreenPos );
                    // currentScreenPos.y = 1-currentScreenPos.y;
                    // 等价于：
                    // 但原神 glsl 是用 currentNonStereoScreenPos.z (即clippos.w) 作为 surface eye depth 比较，编辑器上有误 ??
                    // float3 currentViewPos = UnityWorldToViewPos(reflectDir * Linear_Tracing_AMOUNT_LENGTH * currentStep01 + input.Varying_worldPos.xyz);
                    // float3 currentNonStereoScreenPos = ComputeNonStereoScreenPos(UnityViewToClipPos(currentViewPos)).xyw;
                    // float2 currentScreenPos = NormalizeScreenPos( currentNonStereoScreenPos );

                    // 注：原神的深度贴图已经预处理 Linear01DepthAdaptive，故其逆向源码没有 Linear01DepthAdaptive
                    float currentSampleEyeDepth = Linear01DepthAdaptive(SampleSceneDepth(currentScreenPos).x) * _ProjectionParams.z;
                    float _WDistanceToSampleDepth = currentNonStereoScreenPos.z - currentSampleEyeDepth;
                    // 准确的 surface eye depth 应该是 -currentViewPos.z ??，而不是 currentNonStereoScreenPos.z (即clippos.w) ??
                    // float _WDistanceToSampleDepth = -currentViewPos.z - currentSampleEyeDepth;
                    bool _bCloseToSampleDepth = abs(_WDistanceToSampleDepth) < _DEBUG_VALUE_RAYCAST_BIAS;

                    
                    // raycast 成功
                    if (_bCloseToSampleDepth) {
                        _raycast_hit_at_step = float(currentStepInt);
                        break;
                    }
                }
                

            bool _bAnyExistCloseToSampleDepth = _raycast_hit_at_step > 0.0;

            float3 _one_step_delta = _raycast_step_NonStereoScreenPos_Delta / Linear_Tracing_MAX_STEP;


            // 2. 预准备 没有 raycast 成功的数据
                float3 _reflectInfinitePos = (reflectDir * _reflectInfiniteDistanceX_and_AlphaY.x) + input.Varying_worldPos.xyz;

                float4 _InfiniteClipPos = UnityWorldToClipPos(_reflectInfinitePos);
                
                float2 _InfiniteScreenPos = NormalizeScreenPos(ComputeNonStereoScreenPos(_InfiniteClipPos).xyw) ;

                // 注：原神的深度贴图已经预处理 Linear01DepthAdaptive，故其逆向源码没有 Linear01DepthAdaptive
                float _InfiniteActualEyeDepth = Linear01DepthAdaptive(SampleSceneDepth(_InfiniteScreenPos).x) * _ProjectionParams.z;
                
                bool _bInfinitePosIsActualMoreDepthThanHere = _InfiniteActualEyeDepth > input.Varying_NotNormaizeScreenPosXYW.w;

                float _fadeToScreenEdge = CalcFadeToScreenEdge(_InfiniteScreenPos);

                // #define _InfiniteScreenPos_ST float4(1.47305, 0.82843, -0.73652, -0.41421)
                // scale small, 0~1  ->  -0.736 ~ 0.736  ,  -0.414 ~ 0.414
                // 1.47305:0.82843 == 16/9
                // 找离屏幕中心近一点的，来反射
                float2 _InfiniteAdjustClipPos_XY = (_InfiniteScreenPos * _InfiniteScreenPos_ST.xy) + _InfiniteScreenPos_ST.zw;

                // 原始反混淆代码这里 viewPos.z 是 _InfiniteActualEyeDepth，但实际 unity 编辑器下 viewSpace 前方是 -z，修改后效果正确
                // float3 _InfiniteAdjustViewPos = float3( _InfiniteActualEyeDepth * _InfiniteAdjustClipPos_XY, _InfiniteActualEyeDepth );
                float3 _InfiniteAdjustViewPos = float3( _InfiniteActualEyeDepth * _InfiniteAdjustClipPos_XY, -_InfiniteActualEyeDepth );

                float3 _InfiniteActualAdjustWorldPos = mul(UNITY_MATRIX_I_V, float4(_InfiniteAdjustViewPos, 1)).xyz;

                float3 _WorldSpaceHereToActualInfiniteVector = _InfiniteActualAdjustWorldPos - input.Varying_worldPos.xyz;
                
                // 这个影响不大？   
                // 避免反射过于接近像素、接近水面的内容
                // float _WorldNDotInfiniteDir = dot(input.Varying_WorldNormal, _WorldSpaceHereToActualInfiniteVector);
                // _WorldNDotInfiniteDir = clamp(_WorldNDotInfiniteDir + 1.0, 0.1, 1.0);

                // 实际会导致飞起来时，远处的 alpha 为 0
                const float _WorldNDotInfiniteDir = 1.0;
                
                
                // value.r = _WorldNDotInfiniteDir;
                // 无限远的地方如果深度比当前像素还近，说明不应该反射这更靠近摄像头的内容。
                float _fadeToScreenEdgeNoDepthMoreFade = ( _bInfinitePosIsActualMoreDepthThanHere ? 1.0 : 0.01 ) * _fadeToScreenEdge;


            // 3. 计算 _reflectColorRGBA
            float4 _reflectColorRGBA;
                if (_bAnyExistCloseToSampleDepth)
                {
                    float3 _raycast_hit_NotNormalizeScreenPosXYW = _raycast_hit_at_step * _one_step_delta + input.Varying_NotNormaizeScreenPosXYW.xyw;
                
                    float2 _raycast_hit_ScreenPos = NormalizeScreenPos(_raycast_hit_NotNormalizeScreenPosXYW);
                
                    _fadeToScreenEdge = CalcFadeToScreenEdge(_raycast_hit_ScreenPos);
                
                    _reflectColorRGBA.rgb = SampleSceneColor(_raycast_hit_ScreenPos).xyz ;
                
                    // 2023-07-19:
                    //   原神这里是 + 法，但重现时发现低头会多重影，改成 * 法 ??
                    //   需要特定角度才能看到多重影的情况，暂时还是用回 + 法，保证 raycast 成功时是能看清完整的倒影。
                    // 2023-07-20:
                    //   改为后续依据 VDotN 统一修改 alpha
                    _reflectColorRGBA.a = (_fadeToScreenEdgeNoDepthMoreFade * _WorldNDotInfiniteDir) + _fadeToScreenEdge;
                    
                    // _reflectColorRGBA.rgb = float3(1, 0, 0); // 调试红色表示 raycast 成功
                }
                else
                {
                    _reflectColorRGBA.rgb = SampleSceneColor(_InfiniteScreenPos).xyz;
                    _reflectColorRGBA.a = _WorldNDotInfiniteDir * _fadeToScreenEdgeNoDepthMoreFade;
                    
                    // _reflectColorRGBA.rgb = float3(0, 0, 0); // 调试黑色表示 raycast 失败
                }

                float4 Output_1;
                // float4 Output_2;

                float4 _reflectColorAdjust = _reflectColorRGBA * _reflectInfiniteDistanceX_and_AlphaY.y;
                Output_1.xyz = _reflectColorAdjust.xyz;

                bool _bReflectColorAlphaNearlyZero = _reflectColorAdjust.w < (0.02 - 1e-4); // Android 机器上，0.02 有时候会变成 0.01998
                bool _meshEyeDepthLess7 = input.Varying_NotNormaizeScreenPosXYW.w < 7.0;
                bool _bNearlyFadeoutFromScreen = _fadeToScreenEdge < 0.01;
                
                // 追加一个直视水面不要反射的操作，视线需要与水面法线夹角大于 60°
                const float PI = 3.14159265;
                const float limitAngleCos = cos(PI / 180.0 * 60.0);
                //_reflectColorAdjust.w *= smoothstep(0.0, 0.1, limitAngleCos - dot(viewDir_normalize, input.Varying_WorldNormal)); 
            
                // 屏幕边边，靠近摄像机且alpha接近0的，直接 alpha 设置为0
                bool _bNoReflect = _bNearlyFadeoutFromScreen || (_meshEyeDepthLess7 && _bReflectColorAlphaNearlyZero);
                Output_1.w = _bNoReflect ? 0.0 : _reflectColorAdjust.w;
                // Output_1.w = bNoReflectData ? 0.0: max(0.00, Output_1.w);

                // 实际测试不加这个效果没问题，因为屏幕两侧水后面都有地形。
                // 加上反而反射内容不正常.
                // Output_1.w = !_bInfinitePosIsActualMoreDepthThanHere ? 0.0: max(0.02, Output_1.w);
                
                Output_1.a = saturate(Output_1.a);

            // // 4. 计算 透射color，直接就是抓屏
            //     Output_2.xyz = SampleSceneColor(NormalizeScreenPos(input.Varying_NotNormaizeScreenPosXYW.xyw)).xyz;
            //     Output_2.w = 1.0;
                
                float4 col = 0;
                
                col = Output_1;
                // return value;
                #if _ShowDebugValue
                return value;
                #endif
                
                return col;
            }
            ENDHLSL
        }
    }
}

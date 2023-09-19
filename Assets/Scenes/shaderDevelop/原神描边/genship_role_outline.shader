Shader "genship/role_outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _OutlineColor ("_OutlineColor", Color) = (0.5, 0.5, 0.5, 1)
        
        // 0.0 隐藏， 1.0 正常，2.0 描边？ 目前还原到unity 只有 2 有含义
        [IntRange]_RenderMode ("_RenderMode", Range(0, 2)) = 2
        _eyeDepthInNewRangeMulti1 ("_eyeDepthInNewRangeMulti1", float) = 0.03
        _ViewSpaceSnapVertexDirScale ("_ViewSpaceSnapVertexDirScale", float) = 1.0
        _eyeDepthInNewRangeMulti_0_01 ("_eyeDepthInNewRangeMulti_0_01", float) = 0.01
        _Property_EyeDepthRemapOldRanges ("_Property_EyeDepthRemapOldRanges", Vector) = (0.01, 2.00, 6.00)
        _Property_EyeDepthRemapNewRanges ("_Property_EyeDepthRemapNewRanges", Vector) = (0.105, 0.245, 0.60)
        _AffectProjectionXYWhenLessThan_0_95 ("_AffectProjectionXYWhenLessThan_0_95", float) = 1.00
        _eyeDepthInNewRangeMulti2 ("_eyeDepthInNewRangeMulti2", float) = 1.82
        
        // WZ 调整最终描边屏幕偏移，需要 _AffectProjectionXYWhenLessThan_0_95 小于 0.95 才生效
        _OutlineScreenOffsetWZ ("_OutlineScreenOffsetWZ", Vector) = (0, 0, 0.00044, 0.00012)
        
        _normalizeViewNormalXYScale ("_normalizeViewNormalXYScale", Range(0, 100)) = 1
        
        [Enum(UnityEngine.Rendering.CullMode)]_BaseColorCullMode ("_BaseColorCullMode", float) = 2
        _Slope_ScaledBias ("_Slope_ScaledBias", Range(0, 4)) = 0
        _DepthBias ("_DepthBias", Range(0, 1)) = 0
        
//        _WorldSpaceCameraPos_maybe ("_WorldSpaceCameraPos_maybe", Vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "BaseColor"
            Cull [_BaseColorCullMode]
            
            tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                v.uv.y = 1-v.uv.y;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Outline"
            
            Cull Front
            ZTest Less
            Offset [_Slope_ScaledBias], [_DepthBias]
            
            Tags { "LightMode" = "UniversalForwardOnly" }
            
            HLSLPROGRAM
            float3 _OutlineColor;
            
            float _RenderMode;
            float _eyeDepthInNewRangeMulti1;
            float _ViewSpaceSnapVertexDirScale;
            float _eyeDepthInNewRangeMulti_0_01;
            float3 _Property_EyeDepthRemapOldRanges;
            float3 _Property_EyeDepthRemapNewRanges;
            float _AffectProjectionXYWhenLessThan_0_95;
            float _eyeDepthInNewRangeMulti2;
            float4 _OutlineScreenOffsetWZ;

            // float3 _WorldSpaceCameraPos_maybe;
            float _normalizeViewNormalXYScale;
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			//
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 value : TEXCOORD7;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float EyeDepthRemap(float In, float2 InMinMax, float2 OutMinMax)
            {
                // float _newRangeLength = _eyeDepthParams_newRange.y - _eyeDepthParams_newRange.x; // _newRangeLength 是 [ _eyeDepthParams_newRange.x, _eyeDepthParams_newRange.y ] 之间的距离
                
                // float _oldRangeLength = _eyeDepthParams_oldRange.y - _eyeDepthParams_oldRange.x; // _oldRangeLength = 1.99
                // _oldRangeLength = max(_oldRangeLength, 0.001);                           // _oldRangeLength = 1.99
                // float _01InOldRange = (_eyeDepth_ofSnapToCamera * _fov45AdaptScale) - _eyeDepthParams_oldRange.x;
                // _01InOldRange /= _oldRangeLength;                   // _01InOldRange 是 eyeDepth 在 [ _eyeDepthParams_oldRange.x, _eyeDepthParams_oldRange.y ] 之间的 0~1 比例
                // _01InOldRange = clamp(_01InOldRange, 0.0, 1.0);   // 
                
                // float _eyeDepthInNewRange = (_01InOldRange * _newRangeLength) + _eyeDepthParams_newRange.x;
                
                float Out = OutMinMax.x + saturate((In - InMinMax.x) / max(InMinMax.y - InMinMax.x, 0.001)) * (OutMinMax.y - OutMinMax.x);
                return Out;
            }
            
            v2f vert (appdata v)
            {
                // layout(location = 0) in vec4 Vertex_Position;
                float4 Vertex_Position = v.vertex;

                // layout(location = 1) in vec3 Vertex_Normal;
                float3 Vertex_Normal = v.normal;
                
                // Z 是前后偏移控制 0.5 表示中间，逆向模型的 Z 都是 0.5
                // W 是 eyeDepth 缩放控制，逆向模型大部分是 0.5，也有 0.4，0 的
                // layout(location = 2) in vec4 Vertex_FrontBackOffsetZ_DepthScaleW;
                // 暂时写死这个顶点数据，
                float4 Vertex_FrontBackOffsetZ_DepthScaleW = float4(0, 0, v.uv2.xy);
                // Vertex_FrontBackOffsetZ_DepthScaleW = float4(0, 0, 0.5, 0.5);

                // layout(location = 4) in vec4 Vertex_TANGENT;
                float4 Vertex_TANGENT = v.tangent;
                // float4 Vertex_TANGENT = float4(v.normal, 1.0);



                
                v2f o = (v2f)0;
                // o.vertex = UnityObjectToClipPos(v.vertex);
                // o.vertex = TransformObjectToHClip(v.vertex);
                v.uv.y = 1-v.uv.y;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                
        /// 真正有用的代码 ，描边挤出 计算 ClipPos ============ 
                
                float4 _fixVertexPos1;
                _fixVertexPos1 = Vertex_Position;
                // Varying_ColorRGBA.w = 1.0;

                // // 注意 Unity 本身和 mumu12 vulkan 下的 glsl 矩阵是转置的
                // // float3 _WorldViewLookAt = unity_ObjectToWorld[3u].xyz - _WorldSpaceCameraPos_maybe;
                // // float3 _WorldViewLookAt = unity_ObjectToWorld._14_24_34 - _WorldSpaceCameraPos_maybe;
                // // float3 _WorldViewLookAt = unity_ObjectToWorld[3u].xyz - _WorldSpaceCameraPos;
                // float3 _WorldViewLookAt = float3(unity_ObjectToWorld[0u].w, unity_ObjectToWorld[1u].w, unity_ObjectToWorld[2u].w) - _WorldSpaceCameraPos;
                //
                // // objectToWorld, 但 T 是 _WorldViewLookAt;
                // // 即 object 只生效旋转和缩放，然后位置是吸到摄像机上。
                // float4 _row_x;
                //     // _row_x.x = unity_ObjectToWorld[0u].x;
                //     // _row_x.y = unity_ObjectToWorld[1u].x;
                //     // _row_x.z = unity_ObjectToWorld[2u].x;
                //     // _row_x.w = _WorldViewLookAt.x;
                //     _row_x.xyzw = float4( unity_ObjectToWorld[0u].xyz, _WorldViewLookAt.x );
                //
                // float4 _row_y;
                //     // _row_y.x = unity_ObjectToWorld[0u].y;
                //     // _row_y.y = unity_ObjectToWorld[1u].y;
                //     // _row_y.z = unity_ObjectToWorld[2u].y;
                //     // _row_y.w = _WorldViewLookAt.y;
                //     _row_y.xyzw = float4( unity_ObjectToWorld[1u].xyz, _WorldViewLookAt.y );
                //
                // float4 _row_z;
                //     // _row_z.x = unity_ObjectToWorld[0u].z;
                //     // _row_z.y = unity_ObjectToWorld[1u].z;
                //     // _row_z.z = unity_ObjectToWorld[2u].z;
                //     // _row_z.w = _WorldViewLookAt.z;
                //     _row_z.xyzw = float4( unity_ObjectToWorld[2u].xyz, _WorldViewLookAt.z );
                //
                // float4 _row_w;
                //     // _row_w.x = unity_ObjectToWorld[0u].w;
                //     // _row_w.y = unity_ObjectToWorld[1u].w;
                //     // _row_w.z = unity_ObjectToWorld[2u].w;
                //     // _row_w.w = unity_ObjectToWorld[3u].w;
                //     _row_w.xyzw = unity_ObjectToWorld[3u].xyzw;
                //
                //
                // float4 _worldPosButSnapToCamera;
                //     _worldPosButSnapToCamera.x = dot(_row_x, _fixVertexPos1);
                //     _worldPosButSnapToCamera.y = dot(_row_y, _fixVertexPos1);
                //     _worldPosButSnapToCamera.z = dot(_row_z, _fixVertexPos1);
                //     _worldPosButSnapToCamera.w = dot(_row_w, _fixVertexPos1);

                float4 _worldPosButSnapToCamera = mul(unity_ObjectToWorld, _fixVertexPos1);
                _worldPosButSnapToCamera.xyz -= _WorldSpaceCameraPos;
        
        
                // float3 _rol2_x;
                //     // _rol2_x.x = unity_MatrixV[0u].x;
                //     // _rol2_x.y = unity_MatrixV[1u].x;
                //     // _rol2_x.z = unity_MatrixV[2u].x;
                //     _rol2_x.xyz = unity_MatrixV[0u];
                //
                // float3 _rol2_y;
                //     // _rol2_y.x = unity_MatrixV[0u].y;
                //     // _rol2_y.y = unity_MatrixV[1u].y;
                //     // _rol2_y.z = unity_MatrixV[2u].y;
                //     _rol2_y.xyz = unity_MatrixV[1u];
                //
                // float3 _rol2_z;
                //     // _rol2_z.x = unity_MatrixV[0u].z;
                //     // _rol2_z.y = unity_MatrixV[1u].z;
                //     // _rol2_z.z = unity_MatrixV[2u].z;
                //     _rol2_z.xyz = unity_MatrixV[2u];
                //
                // float4 _viewPos;
                //     _viewPos.x = dot(_rol2_x.xyz, _worldPosButSnapToCamera.xyz);
                //     _viewPos.y = dot(_rol2_y.xyz, _worldPosButSnapToCamera.xyz);
                //     _viewPos.z = dot(_rol2_z.xyz, _worldPosButSnapToCamera.xyz);

                float3 _viewPos = mul((float3x3)unity_MatrixV, _worldPosButSnapToCamera.xyz);
        
                float4 _negateWorldCameraPos;
                    // _negateWorldCameraPos.x = unity_MatrixV[0u].w;
                    // _negateWorldCameraPos.y = unity_MatrixV[1u].w;
                    // _negateWorldCameraPos.z = unity_MatrixV[2u].w;
                    // _negateWorldCameraPos.w = unity_MatrixV[3u].w;
                    _negateWorldCameraPos.xyzw = unity_MatrixV[3u];

                // _worldPos_snapToCameraDotNegateCameraPos
                //   为 0，   无限远, clipPos Z 变得无限远
                //   为 0.01，很远， clipPos Z 变得很远
                //   为 1，   正常， clipPos Z 很远
                //   为 >1,   变近，当住原本内容
                // 但 unity_MatrixV[3u] 恒为 float4(0, 0, 0, 1)
                //   _worldPosButSnapToCamera  恒为 float4(x,y,z,1)
                // 故 _worldPos_snapToCameraDotNegateCameraPos 恒为 1
                // float _worldPos_snapToCameraDotNegateCameraPos;
                //     _worldPos_snapToCameraDotNegateCameraPos = dot(_negateWorldCameraPos, _worldPosButSnapToCamera);
        
        
                // #define _RenderMode  2.0 。
                // 1.0 使用 Vertex_Normal 作为法线，否则用 Vertex_TANGENT 作为法线
                float3 _vertex_normal = _RenderMode == 1.0 ? Vertex_Normal : Vertex_TANGENT.xyz ;
        
        
                // float3 _tmp453 = _vertex_normal.yyy * unity_ObjectToWorld[1u].xyz;
                // _tmp453 = (unity_ObjectToWorld[0u].xyz * _vertex_normal.xxx) + _tmp453;
                // _tmp453 = (unity_ObjectToWorld[2u].xyz * _vertex_normal.zzz) + _tmp453;
                //
                // float3 _world_normal = _tmp453;

                float3 _world_normal = mul( (float3x3)unity_ObjectToWorld, _vertex_normal );
        
        
                // float3 _tmp483 = _world_normal.yyy * unity_MatrixV[1u].xyz;
                // _tmp483 = (unity_MatrixV[0u].xyz * _world_normal.xxx) + _tmp483;
                // _tmp483 = (unity_MatrixV[2u].xyz * _world_normal.zzz) + _tmp483;
                //
                // float3 _viewSpace_normal = _tmp483;

                float3 _viewSpace_normal = mul( (float3x3)unity_MatrixV, _world_normal );
                o.value.xyz = _world_normal;
                o.value.xyz = _viewSpace_normal;
         
                float3 _fixViewNormal;
                    _fixViewNormal.xy = _viewSpace_normal.xy;
                    _fixViewNormal.z = 0.01;
        
        
                float2 _normalizeViewNormalXY;
                    _normalizeViewNormalXY = normalize(_fixViewNormal).xy;

                _normalizeViewNormalXY *= _normalizeViewNormalXYScale;
        
        
        
                // cot( 0.5*45° ) = 2.414
                // UNITY_MATRIX_P[1u].y = cot( 0.5 * FOV )
                // FOV 越小，cot( 0.5 * FOV ) 越大，2.414 / UNITY_MATRIX_P[1u].y; 越小
                //   FOV 越小，人物越大，描边在3D空间上变小，最终屏幕上粗度相应保持不变
                float _fov45AdaptScale = 2.414 / (UNITY_MATRIX_P[1u].y*_ProjectionParams.x);
        
                float _eyeDepth_ofSnapToCamera = -_viewPos.z;
        
        
                // #define _Property_EyeDepthRemapOldRanges float3(0.01, 2.00, 6.00)
                // #define _Property_EyeDepthRemapNewRanges float3(0.105, 0.245, 0.60)
        
                bool _eyeDepth_is_small = _eyeDepth_ofSnapToCamera * _fov45AdaptScale < _Property_EyeDepthRemapOldRanges.y;
                float2 _eyeDepthParams_oldRange = _eyeDepth_is_small ? _Property_EyeDepthRemapOldRanges.xy : _Property_EyeDepthRemapOldRanges.yz;
                float2 _eyeDepthParams_newRange = _eyeDepth_is_small ? _Property_EyeDepthRemapNewRanges.xy : _Property_EyeDepthRemapNewRanges.yz;

                float _eyeDepthInNewRange = EyeDepthRemap(_eyeDepth_ofSnapToCamera * _fov45AdaptScale, _eyeDepthParams_oldRange.xy, _eyeDepthParams_newRange.xy);
        
        
                // #define _eyeDepthInNewRangeMulti1 0.03
                // #define _eyeDepthInNewRangeMulti2 1.82   // _eyeDepthInNewRangeMulti1 * _eyeDepthInNewRangeMulti2 == 0.0546
        
                // #define _eyeDepthInNewRangeMulti_0_01 0.01
        
                float _eyeDepthInNewRangeScale = _eyeDepthInNewRange;
                _eyeDepthInNewRangeScale *= _eyeDepthInNewRangeMulti1 * _eyeDepthInNewRangeMulti2;
                _eyeDepthInNewRangeScale *= 100.0 * _eyeDepthInNewRangeMulti_0_01;
                _eyeDepthInNewRangeScale *= 0.414250195026397705078125;
                _eyeDepthInNewRangeScale *= Vertex_FrontBackOffsetZ_DepthScaleW.w;
        
        
                float3 _viewPos_normalize = normalize(_viewPos);
        
        
                // #define _ViewSpaceSnapVertexDirScale 1.00
                float3 _viewSpaceDir_a_little;
                _viewSpaceDir_a_little = _viewPos_normalize * _ViewSpaceSnapVertexDirScale;
                _viewSpaceDir_a_little *= _eyeDepthInNewRangeMulti_0_01;
        
        
                float3 _viewPos_bias = (_viewSpaceDir_a_little * (Vertex_FrontBackOffsetZ_DepthScaleW.z - 0.5)) + _viewPos;
        
                float3 _normalBiasViewPos = _viewPos_bias;
                
                _normalBiasViewPos.xy += _normalizeViewNormalXY * _eyeDepthInNewRangeScale;
        
                #define UNITY_MATRIX_P_2 UNITY_MATRIX_P
                // float4 _tmp26;
                // _tmp26 = _normalBiasViewPos.yyyy * UNITY_MATRIX_P_2[1u];
                // _tmp26 = (UNITY_MATRIX_P_2[0u] * _normalBiasViewPos.xxxx) + _tmp26;
                // _tmp26 = (UNITY_MATRIX_P_2[2u] * _normalBiasViewPos.zzzz) + _tmp26;
                // _tmp26 = (UNITY_MATRIX_P_2[3u] * _worldPos_snapToCameraDotNegateCameraPos) + _tmp26;
                //
                //
                // float4 _clipPos = _tmp26;

                // _worldPos_snapToCameraDotNegateCameraPos 恒为 1
                // float4 _clipPos = mul(UNITY_MATRIX_P, float4(_normalBiasViewPos, _worldPos_snapToCameraDotNegateCameraPos));
                float4 _clipPos = mul(UNITY_MATRIX_P, float4(_normalBiasViewPos, 1.0));

                
                // #define _OutlineScreenOffsetWZ float4(0.00015, 0.00069, 0.00044, 0.00012)
                float2 _clipPosXYOffset;
                _clipPosXYOffset.x = _clipPos.w * _OutlineScreenOffsetWZ.z;
                _clipPosXYOffset.y = _clipPos.w * _OutlineScreenOffsetWZ.w * _ProjectionParams.x ;
                float2 _clipPosApplyXYOffset = (_clipPosXYOffset.xy * 2.0) + _clipPos.xy;
        
                // #define _AffectProjectionXYWhenLessThan_0_95 1.00
                _clipPos.xy = _AffectProjectionXYWhenLessThan_0_95 < 0.95 ? _clipPosApplyXYOffset : _clipPos.xy;
                // gl_Position = _clipPos;

        /// 真正有用的代码 ，描边挤出 计算 ClipPos ============ end
                o.vertex = _clipPos;
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 value = float4(0,0,0,1);
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);

                // value.r = unity_MatrixVP[1][0];
                

                // value.xyz = i.value;
                

                return float4(_OutlineColor.rgb, 1.0);
                // return value;
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
//            Name "DepthOnly"
            Cull [_BaseColorCullMode]
            ColorMask 0
            
            Tags { "LightMode" = "DepthOnly" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            void frag(v2f i)
            {
                return;
            }

            
            ENDHLSL
        }
        
//        Pass
//        {
//            Name "DepthOnlyOutline"
//            
//            Cull Front
//            ZTest Less
//            Offset [_Slope_ScaledBias], [_DepthBias]
//            
//            Tags { "LightMode" = "DepthOnly" }
//            
//            HLSLPROGRAM
//            float3 _OutlineColor;
//            
//            float _RenderMode;
//            float _eyeDepthInNewRangeMulti1;
//            float _ViewSpaceSnapVertexDirScale;
//            float _eyeDepthInNewRangeMulti_0_01;
//            float3 _Property_EyeDepthRemapOldRanges;
//            float3 _Property_EyeDepthRemapNewRanges;
//            float _AffectProjectionXYWhenLessThan_0_95;
//            float _eyeDepthInNewRangeMulti2;
//            float4 _OutlineScreenOffsetWZ;
//            
//            float _normalizeViewNormalXYScale;
//            
//            #pragma vertex vert
//            #pragma fragment frag
//
//            #include "UnityCG.cginc"
//
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float3 normal : NORMAL;
//                float4 tangent : TANGENT;
//                float2 uv : TEXCOORD0;
//                float2 uv2 : TEXCOORD1;
//                
//            };
//
//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//                float4 vertex : SV_POSITION;
//                float4 value : TEXCOORD7;
//            };
//
//            sampler2D _MainTex;
//            float4 _MainTex_ST;
//
//            float EyeDepthRemap(float In, float2 InMinMax, float2 OutMinMax)
//            {
//                float Out = OutMinMax.x + saturate((In - InMinMax.x) / max(InMinMax.y - InMinMax.x, 0.001)) * (OutMinMax.y - OutMinMax.x);
//                return Out;
//            }
//            
//            v2f vert (appdata v)
//            {
//                float4 Vertex_Position = v.vertex;
//                
//                float3 Vertex_Normal = v.normal;
//                
//                float4 Vertex_FrontBackOffsetZ_DepthScaleW = float4(0, 0, v.uv2.xy);
//                
//                float4 Vertex_TANGENT = v.tangent;
//                
//                
//                v2f o = (v2f)0;
//
//                v.uv.y = 1-v.uv.y;
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//
//                
//        /// 真正有用的代码 ，描边挤出 计算 ClipPos ============ 
//                
//                float4 _fixVertexPos1;
//                _fixVertexPos1 = Vertex_Position;
//     
//                float4 _worldPosButSnapToCamera = mul(unity_ObjectToWorld, _fixVertexPos1);
//                _worldPosButSnapToCamera.xyz -= _WorldSpaceCameraPos;
//
//                float3 _viewPos = mul((float3x3)unity_MatrixV, _worldPosButSnapToCamera.xyz);
//        
//        
//                // #define _RenderMode  2.0 。
//                // 1.0 使用 Vertex_Normal 作为法线，否则用 Vertex_TANGENT 作为法线
//                float3 _vertex_normal = _RenderMode == 1.0 ? Vertex_Normal : Vertex_TANGENT.xyz ;
//                
//
//                float3 _world_normal = mul( (float3x3)unity_ObjectToWorld, _vertex_normal );
//                
//
//                float3 _viewSpace_normal = mul( (float3x3)unity_MatrixV, _world_normal );
//                o.value.xyz = _world_normal;
//                o.value.xyz = _viewSpace_normal;
//         
//                float3 _fixViewNormal;
//                    _fixViewNormal.xy = _viewSpace_normal.xy;
//                    _fixViewNormal.z = 0.01;
//        
//        
//                float2 _normalizeViewNormalXY;
//                    _normalizeViewNormalXY = normalize(_fixViewNormal).xy;
//
//                _normalizeViewNormalXY *= _normalizeViewNormalXYScale;
//                
//                float _fov45AdaptScale = 2.414 / (UNITY_MATRIX_P[1u].y*_ProjectionParams.x);
//        
//                float _eyeDepth_ofSnapToCamera = -_viewPos.z;
//        
//        
//                // #define _Property_EyeDepthRemapOldRanges float3(0.01, 2.00, 6.00)
//                // #define _Property_EyeDepthRemapNewRanges float3(0.105, 0.245, 0.60)
//        
//                bool _eyeDepth_is_small = _eyeDepth_ofSnapToCamera * _fov45AdaptScale < _Property_EyeDepthRemapOldRanges.y;
//                float2 _eyeDepthParams_oldRange = _eyeDepth_is_small ? _Property_EyeDepthRemapOldRanges.xy : _Property_EyeDepthRemapOldRanges.yz;
//                float2 _eyeDepthParams_newRange = _eyeDepth_is_small ? _Property_EyeDepthRemapNewRanges.xy : _Property_EyeDepthRemapNewRanges.yz;
//
//                float _eyeDepthInNewRange = EyeDepthRemap(_eyeDepth_ofSnapToCamera * _fov45AdaptScale, _eyeDepthParams_oldRange.xy, _eyeDepthParams_newRange.xy);
//        
//        
//                // #define _eyeDepthInNewRangeMulti1 0.03
//                // #define _eyeDepthInNewRangeMulti2 1.82   // _eyeDepthInNewRangeMulti1 * _eyeDepthInNewRangeMulti2 == 0.0546
//        
//                // #define _eyeDepthInNewRangeMulti_0_01 0.01
//        
//                float _eyeDepthInNewRangeScale = _eyeDepthInNewRange;
//                _eyeDepthInNewRangeScale *= _eyeDepthInNewRangeMulti1 * _eyeDepthInNewRangeMulti2;
//                _eyeDepthInNewRangeScale *= 100.0 * _eyeDepthInNewRangeMulti_0_01;
//                _eyeDepthInNewRangeScale *= 0.414250195026397705078125;
//                _eyeDepthInNewRangeScale *= Vertex_FrontBackOffsetZ_DepthScaleW.w;
//        
//        
//                float3 _viewPos_normalize = normalize(_viewPos);
//        
//        
//                // #define _ViewSpaceSnapVertexDirScale 1.00
//                float3 _viewSpaceDir_a_little;
//                _viewSpaceDir_a_little = _viewPos_normalize * _ViewSpaceSnapVertexDirScale;
//                _viewSpaceDir_a_little *= _eyeDepthInNewRangeMulti_0_01;
//        
//        
//                float3 _viewPos_bias = (_viewSpaceDir_a_little * (Vertex_FrontBackOffsetZ_DepthScaleW.z - 0.5)) + _viewPos;
//        
//                float3 _normalBiasViewPos = _viewPos_bias;
//                
//                _normalBiasViewPos.xy += _normalizeViewNormalXY * _eyeDepthInNewRangeScale;
//        
//                #define UNITY_MATRIX_P_2 UNITY_MATRIX_P
//
//                
//                float4 _clipPos = mul(UNITY_MATRIX_P, float4(_normalBiasViewPos, 1.0));
//
//                
//                // #define _OutlineScreenOffsetWZ float4(0.00015, 0.00069, 0.00044, 0.00012)
//                float2 _clipPosXYOffset;
//                _clipPosXYOffset.x = _clipPos.w * _OutlineScreenOffsetWZ.z;
//                _clipPosXYOffset.y = _clipPos.w * _OutlineScreenOffsetWZ.w * _ProjectionParams.x ;
//                float2 _clipPosApplyXYOffset = (_clipPosXYOffset.xy * 2.0) + _clipPos.xy;
//        
//                // #define _AffectProjectionXYWhenLessThan_0_95 1.00
//                _clipPos.xy = _AffectProjectionXYWhenLessThan_0_95 < 0.95 ? _clipPosApplyXYOffset : _clipPos.xy;
//                // gl_Position = _clipPos;
//
//        /// 真正有用的代码 ，描边挤出 计算 ClipPos ============ end
//                o.vertex = _clipPos;
//                
//                return o;
//            }
//
//            void frag(v2f i)
//            {
//                return;
//            }
//            ENDHLSL
//            
//        }
    }
}

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonFire_ase"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_OutColor("OutColor", Color) = (0.95,0.1,0.2,1)
		_MidFireTop("MidFireTop", Color) = (0.9,0.3,0.2,1)
		_MidFireBottom("MidFireBottom", Color) = (0.9,0.6,0.2,1)
		[ASEEnd]_InnerColor("InnerColor", Color) = (0.9,0.8,0.2,1)

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull Back
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 3.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999

			
			// #pragma multi_compile _ LIGHTMAP_ON
			// #pragma multi_compile _ DIRLIGHTMAP_COMBINED
			// #pragma shader_feature _ _SAMPLE_GI
			// #pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			// #pragma multi_compile _ DEBUG_DISPLAY
			#define SHADERPASS SHADERPASS_UNLIT


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"


			#include "ToonFireInclude.hlsl"
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
				float fogFactor : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			UNITY_INSTANCING_BUFFER_START(ToonFire_ase)
				UNITY_DEFINE_INSTANCED_PROP(float4, _OutColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireTop)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireBottom)
				UNITY_DEFINE_INSTANCED_PROP(float4, _InnerColor)
			UNITY_INSTANCING_BUFFER_END(ToonFire_ase)


						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				#ifdef ASE_FOG
				o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif
				float4 _OutColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_OutColor);
				float3 appendResult142 = (float3(_OutColor_Instance.rgb));
				float4 _ST = float4(2,2,-1,-1);
				float2 appendResult14 = (float2(_ST.x , _ST.y));
				float2 appendResult15 = (float2(_ST.z , _ST.w));
				float2 uv24 = (IN.ase_texcoord3.xy*appendResult14 + appendResult15);
				float4 appendResult135 = (float4(appendResult142 , ( uv24.y + 0.8 )));
				float smoothstepResult75 = smoothstep( -0.3 , 1.0 , uv24.y);
				float2 appendResult83 = (float2(( (smoothstepResult75*1.5 + 1.5) * uv24.x ) , uv24.y));
				float smoothstepResult84 = smoothstep( 1.0 , 0.5 , length( appendResult83 ));
				float m86 = smoothstepResult84;
				float iTime36 = _TimeParameters.x;
				float2 appendResult31 = (float2(0.0 , ( 0.0 - iTime36 )));
				float2 p22 = ( uv24 + appendResult31 );
				float in_timeInSeconds22 = iTime36;
				float localworley22 = worley( p22 , in_timeInSeconds22 );
				float worley_layer158 = ( localworley22 * 0.5 );
				float2 appendResult49 = (float2(( sin( ( iTime36 * 2.0 ) ) * 0.5 ) , ( iTime36 * -6.0 )));
				float2 p51 = ( ( uv24 * float2( 2,2 ) ) + appendResult49 );
				float in_timeInSeconds51 = iTime36;
				float localworley51 = worley( p51 , in_timeInSeconds51 );
				float worley_layer261 = ( localworley51 * 0.5 );
				float y_mask68 = ( ( -0.3 - uv24.y ) * 0.6 );
				float c92 = ( worley_layer158 + worley_layer261 + y_mask68 );
				float smoothstepResult89 = smoothstep( 0.4 , 0.6 , ( 3.0 * m86 * c92 ));
				float c094 = smoothstepResult89;
				float4 lerpResult118 = lerp( float4( 0,0,0,0 ) , appendResult135 , c094);
				float4 OutColor128 = lerpResult118;
				float4 _MidFireTop_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireTop);
				float3 appendResult144 = (float3(_MidFireTop_Instance.rgb));
				float4 _MidFireBottom_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireBottom);
				float3 appendResult143 = (float3(_MidFireBottom_Instance.rgb));
				float3 lerpResult141 = lerp( appendResult144 , appendResult143 , ( 0.0 - uv24.y ));
				float4 appendResult154 = (float4(lerpResult141 , 1.0));
				float smoothstepResult99 = smoothstep( 0.5 , 0.52 , ( 2.0 * m86 * c92 ));
				float c1101 = smoothstepResult99;
				float4 lerpResult145 = lerp( OutColor128 , appendResult154 , c1101);
				float4 MidColor149 = lerpResult145;
				float4 _InnerColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_InnerColor);
				float4 appendResult161 = (float4(_InnerColor_Instance.rgb , 1.0));
				float smoothstepResult107 = smoothstep( 0.5 , 0.52 , ( 1.2 * m86 * c92 * ( 0.3 - uv24.y ) ));
				float c2109 = smoothstepResult107;
				float4 lerpResult157 = lerp( MidColor149 , appendResult161 , c2109);
				float4 InnerColor163 = lerpResult157;
				float4 FinalColor130 = InnerColor163;
				float3 appendResult131 = (float3(FinalColor130.xyz));
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = appendResult131;
				float Alpha = saturate( (FinalColor130).w );
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.clipPos, Color);
				#endif

				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif


				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999

			
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#include "ToonFireInclude.hlsl"
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			UNITY_INSTANCING_BUFFER_START(ToonFire_ase)
				UNITY_DEFINE_INSTANCED_PROP(float4, _OutColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireTop)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireBottom)
				UNITY_DEFINE_INSTANCED_PROP(float4, _InnerColor)
			UNITY_INSTANCING_BUFFER_END(ToonFire_ase)


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = TransformWorldToHClip( positionWS );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 _OutColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_OutColor);
				float3 appendResult142 = (float3(_OutColor_Instance.rgb));
				float4 _ST = float4(2,2,-1,-1);
				float2 appendResult14 = (float2(_ST.x , _ST.y));
				float2 appendResult15 = (float2(_ST.z , _ST.w));
				float2 uv24 = (IN.ase_texcoord2.xy*appendResult14 + appendResult15);
				float4 appendResult135 = (float4(appendResult142 , ( uv24.y + 0.8 )));
				float smoothstepResult75 = smoothstep( -0.3 , 1.0 , uv24.y);
				float2 appendResult83 = (float2(( (smoothstepResult75*1.5 + 1.5) * uv24.x ) , uv24.y));
				float smoothstepResult84 = smoothstep( 1.0 , 0.5 , length( appendResult83 ));
				float m86 = smoothstepResult84;
				float iTime36 = _TimeParameters.x;
				float2 appendResult31 = (float2(0.0 , ( 0.0 - iTime36 )));
				float2 p22 = ( uv24 + appendResult31 );
				float in_timeInSeconds22 = iTime36;
				float localworley22 = worley( p22 , in_timeInSeconds22 );
				float worley_layer158 = ( localworley22 * 0.5 );
				float2 appendResult49 = (float2(( sin( ( iTime36 * 2.0 ) ) * 0.5 ) , ( iTime36 * -6.0 )));
				float2 p51 = ( ( uv24 * float2( 2,2 ) ) + appendResult49 );
				float in_timeInSeconds51 = iTime36;
				float localworley51 = worley( p51 , in_timeInSeconds51 );
				float worley_layer261 = ( localworley51 * 0.5 );
				float y_mask68 = ( ( -0.3 - uv24.y ) * 0.6 );
				float c92 = ( worley_layer158 + worley_layer261 + y_mask68 );
				float smoothstepResult89 = smoothstep( 0.4 , 0.6 , ( 3.0 * m86 * c92 ));
				float c094 = smoothstepResult89;
				float4 lerpResult118 = lerp( float4( 0,0,0,0 ) , appendResult135 , c094);
				float4 OutColor128 = lerpResult118;
				float4 _MidFireTop_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireTop);
				float3 appendResult144 = (float3(_MidFireTop_Instance.rgb));
				float4 _MidFireBottom_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireBottom);
				float3 appendResult143 = (float3(_MidFireBottom_Instance.rgb));
				float3 lerpResult141 = lerp( appendResult144 , appendResult143 , ( 0.0 - uv24.y ));
				float4 appendResult154 = (float4(lerpResult141 , 1.0));
				float smoothstepResult99 = smoothstep( 0.5 , 0.52 , ( 2.0 * m86 * c92 ));
				float c1101 = smoothstepResult99;
				float4 lerpResult145 = lerp( OutColor128 , appendResult154 , c1101);
				float4 MidColor149 = lerpResult145;
				float4 _InnerColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_InnerColor);
				float4 appendResult161 = (float4(_InnerColor_Instance.rgb , 1.0));
				float smoothstepResult107 = smoothstep( 0.5 , 0.52 , ( 1.2 * m86 * c92 * ( 0.3 - uv24.y ) ));
				float c2109 = smoothstepResult107;
				float4 lerpResult157 = lerp( MidColor149 , appendResult161 , c2109);
				float4 InnerColor163 = lerpResult157;
				float4 FinalColor130 = InnerColor163;
				
				float Alpha = saturate( (FinalColor130).w );
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999

			
			// #pragma multi_compile _ LIGHTMAP_ON
			// #pragma multi_compile _ DIRLIGHTMAP_COMBINED
			// #pragma shader_feature _ _SAMPLE_GI
			// #pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			// #pragma multi_compile _ DEBUG_DISPLAY
			#define SHADERPASS SHADERPASS_UNLIT


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"


			#include "ToonFireInclude.hlsl"
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
				float fogFactor : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			UNITY_INSTANCING_BUFFER_START(ToonFire_ase)
				UNITY_DEFINE_INSTANCED_PROP(float4, _OutColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireTop)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireBottom)
				UNITY_DEFINE_INSTANCED_PROP(float4, _InnerColor)
			UNITY_INSTANCING_BUFFER_END(ToonFire_ase)


						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				#ifdef ASE_FOG
				o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif
				float4 _OutColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_OutColor);
				float3 appendResult142 = (float3(_OutColor_Instance.rgb));
				float4 _ST = float4(2,2,-1,-1);
				float2 appendResult14 = (float2(_ST.x , _ST.y));
				float2 appendResult15 = (float2(_ST.z , _ST.w));
				float2 uv24 = (IN.ase_texcoord3.xy*appendResult14 + appendResult15);
				float4 appendResult135 = (float4(appendResult142 , ( uv24.y + 0.8 )));
				float smoothstepResult75 = smoothstep( -0.3 , 1.0 , uv24.y);
				float2 appendResult83 = (float2(( (smoothstepResult75*1.5 + 1.5) * uv24.x ) , uv24.y));
				float smoothstepResult84 = smoothstep( 1.0 , 0.5 , length( appendResult83 ));
				float m86 = smoothstepResult84;
				float iTime36 = _TimeParameters.x;
				float2 appendResult31 = (float2(0.0 , ( 0.0 - iTime36 )));
				float2 p22 = ( uv24 + appendResult31 );
				float in_timeInSeconds22 = iTime36;
				float localworley22 = worley( p22 , in_timeInSeconds22 );
				float worley_layer158 = ( localworley22 * 0.5 );
				float2 appendResult49 = (float2(( sin( ( iTime36 * 2.0 ) ) * 0.5 ) , ( iTime36 * -6.0 )));
				float2 p51 = ( ( uv24 * float2( 2,2 ) ) + appendResult49 );
				float in_timeInSeconds51 = iTime36;
				float localworley51 = worley( p51 , in_timeInSeconds51 );
				float worley_layer261 = ( localworley51 * 0.5 );
				float y_mask68 = ( ( -0.3 - uv24.y ) * 0.6 );
				float c92 = ( worley_layer158 + worley_layer261 + y_mask68 );
				float smoothstepResult89 = smoothstep( 0.4 , 0.6 , ( 3.0 * m86 * c92 ));
				float c094 = smoothstepResult89;
				float4 lerpResult118 = lerp( float4( 0,0,0,0 ) , appendResult135 , c094);
				float4 OutColor128 = lerpResult118;
				float4 _MidFireTop_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireTop);
				float3 appendResult144 = (float3(_MidFireTop_Instance.rgb));
				float4 _MidFireBottom_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireBottom);
				float3 appendResult143 = (float3(_MidFireBottom_Instance.rgb));
				float3 lerpResult141 = lerp( appendResult144 , appendResult143 , ( 0.0 - uv24.y ));
				float4 appendResult154 = (float4(lerpResult141 , 1.0));
				float smoothstepResult99 = smoothstep( 0.5 , 0.52 , ( 2.0 * m86 * c92 ));
				float c1101 = smoothstepResult99;
				float4 lerpResult145 = lerp( OutColor128 , appendResult154 , c1101);
				float4 MidColor149 = lerpResult145;
				float4 _InnerColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_InnerColor);
				float4 appendResult161 = (float4(_InnerColor_Instance.rgb , 1.0));
				float smoothstepResult107 = smoothstep( 0.5 , 0.52 , ( 1.2 * m86 * c92 * ( 0.3 - uv24.y ) ));
				float c2109 = smoothstepResult107;
				float4 lerpResult157 = lerp( MidColor149 , appendResult161 , c2109);
				float4 InnerColor163 = lerpResult157;
				float4 FinalColor130 = InnerColor163;
				float3 appendResult131 = (float3(FinalColor130.xyz));
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = appendResult131;
				float Alpha = saturate( (FinalColor130).w );
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.clipPos, Color);
				#endif

				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif


				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}


		
        Pass
        {
			
            Name "SceneSelectionPass"
            Tags { "LightMode"="SceneSelectionPass" }
        
			Cull Off

			HLSLPROGRAM
        
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999

        
			#pragma only_renderers d3d11 glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#include "ToonFireInclude.hlsl"
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
						#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			UNITY_INSTANCING_BUFFER_START(ToonFire_ase)
				UNITY_DEFINE_INSTANCED_PROP(float4, _OutColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireTop)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireBottom)
				UNITY_DEFINE_INSTANCED_PROP(float4, _InnerColor)
			UNITY_INSTANCING_BUFFER_END(ToonFire_ase)


			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif
			
			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 _OutColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_OutColor);
				float3 appendResult142 = (float3(_OutColor_Instance.rgb));
				float4 _ST = float4(2,2,-1,-1);
				float2 appendResult14 = (float2(_ST.x , _ST.y));
				float2 appendResult15 = (float2(_ST.z , _ST.w));
				float2 uv24 = (IN.ase_texcoord.xy*appendResult14 + appendResult15);
				float4 appendResult135 = (float4(appendResult142 , ( uv24.y + 0.8 )));
				float smoothstepResult75 = smoothstep( -0.3 , 1.0 , uv24.y);
				float2 appendResult83 = (float2(( (smoothstepResult75*1.5 + 1.5) * uv24.x ) , uv24.y));
				float smoothstepResult84 = smoothstep( 1.0 , 0.5 , length( appendResult83 ));
				float m86 = smoothstepResult84;
				float iTime36 = _TimeParameters.x;
				float2 appendResult31 = (float2(0.0 , ( 0.0 - iTime36 )));
				float2 p22 = ( uv24 + appendResult31 );
				float in_timeInSeconds22 = iTime36;
				float localworley22 = worley( p22 , in_timeInSeconds22 );
				float worley_layer158 = ( localworley22 * 0.5 );
				float2 appendResult49 = (float2(( sin( ( iTime36 * 2.0 ) ) * 0.5 ) , ( iTime36 * -6.0 )));
				float2 p51 = ( ( uv24 * float2( 2,2 ) ) + appendResult49 );
				float in_timeInSeconds51 = iTime36;
				float localworley51 = worley( p51 , in_timeInSeconds51 );
				float worley_layer261 = ( localworley51 * 0.5 );
				float y_mask68 = ( ( -0.3 - uv24.y ) * 0.6 );
				float c92 = ( worley_layer158 + worley_layer261 + y_mask68 );
				float smoothstepResult89 = smoothstep( 0.4 , 0.6 , ( 3.0 * m86 * c92 ));
				float c094 = smoothstepResult89;
				float4 lerpResult118 = lerp( float4( 0,0,0,0 ) , appendResult135 , c094);
				float4 OutColor128 = lerpResult118;
				float4 _MidFireTop_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireTop);
				float3 appendResult144 = (float3(_MidFireTop_Instance.rgb));
				float4 _MidFireBottom_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireBottom);
				float3 appendResult143 = (float3(_MidFireBottom_Instance.rgb));
				float3 lerpResult141 = lerp( appendResult144 , appendResult143 , ( 0.0 - uv24.y ));
				float4 appendResult154 = (float4(lerpResult141 , 1.0));
				float smoothstepResult99 = smoothstep( 0.5 , 0.52 , ( 2.0 * m86 * c92 ));
				float c1101 = smoothstepResult99;
				float4 lerpResult145 = lerp( OutColor128 , appendResult154 , c1101);
				float4 MidColor149 = lerpResult145;
				float4 _InnerColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_InnerColor);
				float4 appendResult161 = (float4(_InnerColor_Instance.rgb , 1.0));
				float smoothstepResult107 = smoothstep( 0.5 , 0.52 , ( 1.2 * m86 * c92 * ( 0.3 - uv24.y ) ));
				float c2109 = smoothstepResult107;
				float4 lerpResult157 = lerp( MidColor149 , appendResult161 , c2109);
				float4 InnerColor163 = lerpResult157;
				float4 FinalColor130 = InnerColor163;
				
				surfaceDescription.Alpha = saturate( (FinalColor130).w );
				surfaceDescription.AlphaClipThreshold = 0.5;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}

			ENDHLSL
        }

		
        Pass
        {
			
            Name "ScenePickingPass"
            Tags { "LightMode"="Picking" }
        
			HLSLPROGRAM

			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999


			#pragma only_renderers d3d11 glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag

        
			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY
			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#include "ToonFireInclude.hlsl"
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
						#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			UNITY_INSTANCING_BUFFER_START(ToonFire_ase)
				UNITY_DEFINE_INSTANCED_PROP(float4, _OutColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireTop)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireBottom)
				UNITY_DEFINE_INSTANCED_PROP(float4, _InnerColor)
			UNITY_INSTANCING_BUFFER_END(ToonFire_ase)


			
        
			float4 _SelectionID;

        
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 _OutColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_OutColor);
				float3 appendResult142 = (float3(_OutColor_Instance.rgb));
				float4 _ST = float4(2,2,-1,-1);
				float2 appendResult14 = (float2(_ST.x , _ST.y));
				float2 appendResult15 = (float2(_ST.z , _ST.w));
				float2 uv24 = (IN.ase_texcoord.xy*appendResult14 + appendResult15);
				float4 appendResult135 = (float4(appendResult142 , ( uv24.y + 0.8 )));
				float smoothstepResult75 = smoothstep( -0.3 , 1.0 , uv24.y);
				float2 appendResult83 = (float2(( (smoothstepResult75*1.5 + 1.5) * uv24.x ) , uv24.y));
				float smoothstepResult84 = smoothstep( 1.0 , 0.5 , length( appendResult83 ));
				float m86 = smoothstepResult84;
				float iTime36 = _TimeParameters.x;
				float2 appendResult31 = (float2(0.0 , ( 0.0 - iTime36 )));
				float2 p22 = ( uv24 + appendResult31 );
				float in_timeInSeconds22 = iTime36;
				float localworley22 = worley( p22 , in_timeInSeconds22 );
				float worley_layer158 = ( localworley22 * 0.5 );
				float2 appendResult49 = (float2(( sin( ( iTime36 * 2.0 ) ) * 0.5 ) , ( iTime36 * -6.0 )));
				float2 p51 = ( ( uv24 * float2( 2,2 ) ) + appendResult49 );
				float in_timeInSeconds51 = iTime36;
				float localworley51 = worley( p51 , in_timeInSeconds51 );
				float worley_layer261 = ( localworley51 * 0.5 );
				float y_mask68 = ( ( -0.3 - uv24.y ) * 0.6 );
				float c92 = ( worley_layer158 + worley_layer261 + y_mask68 );
				float smoothstepResult89 = smoothstep( 0.4 , 0.6 , ( 3.0 * m86 * c92 ));
				float c094 = smoothstepResult89;
				float4 lerpResult118 = lerp( float4( 0,0,0,0 ) , appendResult135 , c094);
				float4 OutColor128 = lerpResult118;
				float4 _MidFireTop_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireTop);
				float3 appendResult144 = (float3(_MidFireTop_Instance.rgb));
				float4 _MidFireBottom_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireBottom);
				float3 appendResult143 = (float3(_MidFireBottom_Instance.rgb));
				float3 lerpResult141 = lerp( appendResult144 , appendResult143 , ( 0.0 - uv24.y ));
				float4 appendResult154 = (float4(lerpResult141 , 1.0));
				float smoothstepResult99 = smoothstep( 0.5 , 0.52 , ( 2.0 * m86 * c92 ));
				float c1101 = smoothstepResult99;
				float4 lerpResult145 = lerp( OutColor128 , appendResult154 , c1101);
				float4 MidColor149 = lerpResult145;
				float4 _InnerColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_InnerColor);
				float4 appendResult161 = (float4(_InnerColor_Instance.rgb , 1.0));
				float smoothstepResult107 = smoothstep( 0.5 , 0.52 , ( 1.2 * m86 * c92 * ( 0.3 - uv24.y ) ));
				float c2109 = smoothstepResult107;
				float4 lerpResult157 = lerp( MidColor149 , appendResult161 , c2109);
				float4 InnerColor163 = lerpResult157;
				float4 FinalColor130 = InnerColor163;
				
				surfaceDescription.Alpha = saturate( (FinalColor130).w );
				surfaceDescription.AlphaClipThreshold = 0.5;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;
				
				return outColor;
			}
        
			ENDHLSL
        }
		
		
        Pass
        {
			
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

        
			HLSLPROGRAM
			
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999

			
			#pragma only_renderers d3d11 glcore gles gles3 
			#pragma multi_compile_fog
			#pragma instancing_options renderinglayer
			#pragma vertex vert
			#pragma fragment frag

        
			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#include "ToonFireInclude.hlsl"
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
						#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			UNITY_INSTANCING_BUFFER_START(ToonFire_ase)
				UNITY_DEFINE_INSTANCED_PROP(float4, _OutColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireTop)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireBottom)
				UNITY_DEFINE_INSTANCED_PROP(float4, _InnerColor)
			UNITY_INSTANCING_BUFFER_END(ToonFire_ase)


			      
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal(v.ase_normal);

				o.clipPos = TransformWorldToHClip(positionWS);
				o.normalWS.xyz =  normalWS;

				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 _OutColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_OutColor);
				float3 appendResult142 = (float3(_OutColor_Instance.rgb));
				float4 _ST = float4(2,2,-1,-1);
				float2 appendResult14 = (float2(_ST.x , _ST.y));
				float2 appendResult15 = (float2(_ST.z , _ST.w));
				float2 uv24 = (IN.ase_texcoord1.xy*appendResult14 + appendResult15);
				float4 appendResult135 = (float4(appendResult142 , ( uv24.y + 0.8 )));
				float smoothstepResult75 = smoothstep( -0.3 , 1.0 , uv24.y);
				float2 appendResult83 = (float2(( (smoothstepResult75*1.5 + 1.5) * uv24.x ) , uv24.y));
				float smoothstepResult84 = smoothstep( 1.0 , 0.5 , length( appendResult83 ));
				float m86 = smoothstepResult84;
				float iTime36 = _TimeParameters.x;
				float2 appendResult31 = (float2(0.0 , ( 0.0 - iTime36 )));
				float2 p22 = ( uv24 + appendResult31 );
				float in_timeInSeconds22 = iTime36;
				float localworley22 = worley( p22 , in_timeInSeconds22 );
				float worley_layer158 = ( localworley22 * 0.5 );
				float2 appendResult49 = (float2(( sin( ( iTime36 * 2.0 ) ) * 0.5 ) , ( iTime36 * -6.0 )));
				float2 p51 = ( ( uv24 * float2( 2,2 ) ) + appendResult49 );
				float in_timeInSeconds51 = iTime36;
				float localworley51 = worley( p51 , in_timeInSeconds51 );
				float worley_layer261 = ( localworley51 * 0.5 );
				float y_mask68 = ( ( -0.3 - uv24.y ) * 0.6 );
				float c92 = ( worley_layer158 + worley_layer261 + y_mask68 );
				float smoothstepResult89 = smoothstep( 0.4 , 0.6 , ( 3.0 * m86 * c92 ));
				float c094 = smoothstepResult89;
				float4 lerpResult118 = lerp( float4( 0,0,0,0 ) , appendResult135 , c094);
				float4 OutColor128 = lerpResult118;
				float4 _MidFireTop_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireTop);
				float3 appendResult144 = (float3(_MidFireTop_Instance.rgb));
				float4 _MidFireBottom_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireBottom);
				float3 appendResult143 = (float3(_MidFireBottom_Instance.rgb));
				float3 lerpResult141 = lerp( appendResult144 , appendResult143 , ( 0.0 - uv24.y ));
				float4 appendResult154 = (float4(lerpResult141 , 1.0));
				float smoothstepResult99 = smoothstep( 0.5 , 0.52 , ( 2.0 * m86 * c92 ));
				float c1101 = smoothstepResult99;
				float4 lerpResult145 = lerp( OutColor128 , appendResult154 , c1101);
				float4 MidColor149 = lerpResult145;
				float4 _InnerColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_InnerColor);
				float4 appendResult161 = (float4(_InnerColor_Instance.rgb , 1.0));
				float smoothstepResult107 = smoothstep( 0.5 , 0.52 , ( 1.2 * m86 * c92 * ( 0.3 - uv24.y ) ));
				float c2109 = smoothstepResult107;
				float4 lerpResult157 = lerp( MidColor149 , appendResult161 , c2109);
				float4 InnerColor163 = lerpResult157;
				float4 FinalColor130 = InnerColor163;
				
				surfaceDescription.Alpha = saturate( (FinalColor130).w );
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				float3 normalWS = IN.normalWS;
				return half4(NormalizeNormalPerPixel(normalWS), 0.0);

			}
        
			ENDHLSL
        }

		
        Pass
        {
			
            Name "DepthNormalsOnly"
            Tags { "LightMode"="DepthNormalsOnly" }
        
			ZTest LEqual
			ZWrite On
        
        
			HLSLPROGRAM
        
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999

        
			#pragma exclude_renderers glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag
        
			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define ATTRIBUTES_NEED_TEXCOORD1
			#define VARYINGS_NEED_NORMAL_WS
			#define VARYINGS_NEED_TANGENT_WS
        
			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#include "ToonFireInclude.hlsl"
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
						#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			UNITY_INSTANCING_BUFFER_START(ToonFire_ase)
				UNITY_DEFINE_INSTANCED_PROP(float4, _OutColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireTop)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MidFireBottom)
				UNITY_DEFINE_INSTANCED_PROP(float4, _InnerColor)
			UNITY_INSTANCING_BUFFER_END(ToonFire_ase)


			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
      
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal(v.ase_normal);

				o.clipPos = TransformWorldToHClip(positionWS);
				o.normalWS.xyz =  normalWS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 _OutColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_OutColor);
				float3 appendResult142 = (float3(_OutColor_Instance.rgb));
				float4 _ST = float4(2,2,-1,-1);
				float2 appendResult14 = (float2(_ST.x , _ST.y));
				float2 appendResult15 = (float2(_ST.z , _ST.w));
				float2 uv24 = (IN.ase_texcoord1.xy*appendResult14 + appendResult15);
				float4 appendResult135 = (float4(appendResult142 , ( uv24.y + 0.8 )));
				float smoothstepResult75 = smoothstep( -0.3 , 1.0 , uv24.y);
				float2 appendResult83 = (float2(( (smoothstepResult75*1.5 + 1.5) * uv24.x ) , uv24.y));
				float smoothstepResult84 = smoothstep( 1.0 , 0.5 , length( appendResult83 ));
				float m86 = smoothstepResult84;
				float iTime36 = _TimeParameters.x;
				float2 appendResult31 = (float2(0.0 , ( 0.0 - iTime36 )));
				float2 p22 = ( uv24 + appendResult31 );
				float in_timeInSeconds22 = iTime36;
				float localworley22 = worley( p22 , in_timeInSeconds22 );
				float worley_layer158 = ( localworley22 * 0.5 );
				float2 appendResult49 = (float2(( sin( ( iTime36 * 2.0 ) ) * 0.5 ) , ( iTime36 * -6.0 )));
				float2 p51 = ( ( uv24 * float2( 2,2 ) ) + appendResult49 );
				float in_timeInSeconds51 = iTime36;
				float localworley51 = worley( p51 , in_timeInSeconds51 );
				float worley_layer261 = ( localworley51 * 0.5 );
				float y_mask68 = ( ( -0.3 - uv24.y ) * 0.6 );
				float c92 = ( worley_layer158 + worley_layer261 + y_mask68 );
				float smoothstepResult89 = smoothstep( 0.4 , 0.6 , ( 3.0 * m86 * c92 ));
				float c094 = smoothstepResult89;
				float4 lerpResult118 = lerp( float4( 0,0,0,0 ) , appendResult135 , c094);
				float4 OutColor128 = lerpResult118;
				float4 _MidFireTop_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireTop);
				float3 appendResult144 = (float3(_MidFireTop_Instance.rgb));
				float4 _MidFireBottom_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_MidFireBottom);
				float3 appendResult143 = (float3(_MidFireBottom_Instance.rgb));
				float3 lerpResult141 = lerp( appendResult144 , appendResult143 , ( 0.0 - uv24.y ));
				float4 appendResult154 = (float4(lerpResult141 , 1.0));
				float smoothstepResult99 = smoothstep( 0.5 , 0.52 , ( 2.0 * m86 * c92 ));
				float c1101 = smoothstepResult99;
				float4 lerpResult145 = lerp( OutColor128 , appendResult154 , c1101);
				float4 MidColor149 = lerpResult145;
				float4 _InnerColor_Instance = UNITY_ACCESS_INSTANCED_PROP(ToonFire_ase,_InnerColor);
				float4 appendResult161 = (float4(_InnerColor_Instance.rgb , 1.0));
				float smoothstepResult107 = smoothstep( 0.5 , 0.52 , ( 1.2 * m86 * c92 * ( 0.3 - uv24.y ) ));
				float c2109 = smoothstepResult107;
				float4 lerpResult157 = lerp( MidColor149 , appendResult161 , c2109);
				float4 InnerColor163 = lerpResult157;
				float4 FinalColor130 = InnerColor163;
				
				surfaceDescription.Alpha = saturate( (FinalColor130).w );
				surfaceDescription.AlphaClipThreshold = 0.5;
				
				#if _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				float3 normalWS = IN.normalWS;
				return half4(NormalizeNormalPerPixel(normalWS), 0.0);

			}

			ENDHLSL
        }
		
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18935
85;211;1089;734;749.7776;872.8323;2.001019;True;False
Node;AmplifyShaderEditor.CommentaryNode;37;-6440.579,-271.2202;Inherit;False;531.736;168.0724;Comment;2;35;36;iTime;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;25;-6474.821,-920.8976;Inherit;False;1047.54;470;Comment;6;15;13;11;14;10;24;uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;35;-6390.579,-214.1478;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;13;-6424.821,-685.8978;Inherit;False;Constant;_ST;ST;0;0;Create;True;0;0;0;False;0;False;2,2,-1,-1;2,2,-1,-1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;36;-6133.843,-220.2202;Inherit;False;iTime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;62;-4881.741,-409.1977;Inherit;False;1837.625;522.3146;Comment;14;53;33;34;42;43;44;45;46;47;49;50;51;52;61;worley_layer2;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-4831.741,-193.8827;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;14;-6170.822,-684.8978;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;10;-6254.821,-870.8977;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;15;-6165.822,-585.8977;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;56;-4885.234,-950.8563;Inherit;False;1621.331;417.4354;Comment;9;58;22;30;31;32;39;38;27;28;worley_layer1;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-4645.741,-191.8828;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;11;-5917.822,-869.8977;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SinOpNode;43;-4469.741,-194.8827;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-5651.281,-857.2483;Inherit;False;uv;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;87;-5256.841,594.2763;Inherit;False;2207.304;531.8889;Comment;13;73;75;72;77;78;79;80;81;82;83;85;84;86;m;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-4835.234,-738.9314;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-4474.741,-22.88315;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-5206.841,644.2764;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;33;-4502.564,-359.1978;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;30;-4623.93,-753.1686;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-4288.74,-178.8828;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;70;-4864.954,249.9076;Inherit;False;1077.278;192.0679;Comment;5;68;66;67;64;65;y_mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-4279.74,-21.88315;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-6;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-4814.954,306.3374;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;73;-5002.294,648.2087;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;27;-4487.636,-900.8562;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;31;-4453.932,-776.1685;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;49;-4114.741,-113.8832;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-4293.563,-316.1979;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-3943.287,-38.2471;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;50;-3910.742,-204.8827;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-4698.13,799.255;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;65;-4632.143,306.9754;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SmoothstepOpNode;75;-4779.862,673.0376;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-0.3;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-4321.694,-666.0252;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-4290.932,-812.1685;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CustomExpressionNode;51;-3723.703,-208.3779;Inherit;False; ;1;File;2;True;p;FLOAT2;0,0;In;;Inherit;False;True;in_timeInSeconds;FLOAT;0;In;;Inherit;False;worley;False;False;0;31435e59c03d4d9db49adfebc2a0d6e7;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;22;-4053.176,-730.3109;Inherit;False; ;1;File;2;True;p;FLOAT2;0,0;In;;Inherit;False;True;in_timeInSeconds;FLOAT;0;In;;Inherit;False;worley;False;False;0;31435e59c03d4d9db49adfebc2a0d6e7;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;66;-4458.736,303.1867;Inherit;False;2;0;FLOAT;-0.3;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;81;-4429.665,990.1309;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;77;-4552.26,673.038;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1.5;False;2;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;80;-4492.255,800.2897;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.BreakToComponentsNode;82;-4223.786,991.1649;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-3778.652,-735.2623;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-3437.089,-216.5337;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-4236.847,301.0007;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-4295.689,696.8325;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;83;-3965.662,789.9434;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-3573.819,-730.73;Inherit;False;worley_layer1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;202;-2416.67,-1937.494;Inherit;False;908.2805;470.0605;Comment;5;69;59;63;55;92;c;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;68;-4014.031,302.2584;Inherit;False;y_mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-3265.991,-226.7676;Inherit;False;worley_layer2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;-2366.67,-1887.494;Inherit;False;58;worley_layer1;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;-2362.417,-1747.15;Inherit;False;61;worley_layer2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-2334.962,-1583.434;Inherit;False;68;y_mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;85;-3702.884,801.3231;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;84;-3505.281,802.3571;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;55;-1908.917,-1732.171;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;86;-3273.539,802.3572;Inherit;True;m;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;103;-2372.617,-1341.069;Inherit;False;1051.356;278.7;out fire;5;90;94;95;89;93;c0;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;92;-1732.389,-1724.265;Inherit;False;c;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;155;-2347.571,1192.363;Inherit;False;1809.547;430.2169;Comment;9;121;122;116;123;142;135;119;118;128;OutColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-2322.617,-1280.882;Inherit;False;86;m;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-2308.261,-1186.069;Inherit;False;92;c;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-2056.261,-1291.069;Inherit;False;3;3;0;FLOAT;3;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-2297.571,1411.556;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;116;-2109.299,1242.363;Inherit;False;InstancedProperty;_OutColor;OutColor;1;0;Create;True;0;0;0;False;0;False;0.95,0.1,0.2,1;0.95,0.09999991,0.1999999,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;156;-2555.608,1695.619;Inherit;False;1776.619;545.4886;Comment;13;137;138;139;140;144;143;141;148;145;149;154;146;197;MidColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.SmoothstepOpNode;89;-1873.49,-1221.369;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.4;False;2;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;104;-2376.829,-976.1689;Inherit;False;1059.46;271;mid fire;5;97;100;99;98;101;c1;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;122;-2076.062,1416.32;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;139;-2513.435,2107.108;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;100;-2312.473,-821.1689;Inherit;False;92;c;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;94;-1545.263,-1220.069;Inherit;False;c0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;142;-1795.547,1270.68;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;115;-2351.917,-602.2329;Inherit;False;1384.77;395.8818;inner fire;8;113;110;111;108;106;107;109;105;c2 ;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;123;-1882.125,1439.138;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;-2326.829,-915.9819;Inherit;False;86;m;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;-2301.917,-356.8951;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;140;-2301.435,2120.108;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;135;-1605.462,1296.063;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;137;-2505.608,1809.019;Inherit;False;InstancedProperty;_MidFireTop;MidFireTop;2;0;Create;True;0;0;0;False;0;False;0.9,0.3,0.2,1;0.9,0.2999999,0.1999999,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;138;-2497.909,1984.619;Inherit;False;InstancedProperty;_MidFireBottom;MidFireBottom;3;0;Create;True;0;0;0;False;0;False;0.9,0.6,0.2,1;0.9,0.6,0.1999999,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;-2060.473,-926.1689;Inherit;False;3;3;0;FLOAT;2;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-1575.506,1506.579;Inherit;False;94;c0;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;144;-2190.252,1849.637;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;99;-1883.701,-913.4692;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.52;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;118;-1262.409,1377.332;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;111;-2095.696,-341.351;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleSubtractOpNode;197;-2158.5,2112.135;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;143;-2192.252,2023.637;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;105;-1976.607,-542.046;Inherit;False;86;m;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;-1962.251,-447.2329;Inherit;False;92;c;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;113;-1940.254,-342.3879;Inherit;False;2;0;FLOAT;0.3;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-1541.371,-920.9482;Inherit;False;c1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;141;-1994.435,1950.108;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;128;-762.0248,1371.893;Inherit;False;OutColor;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;146;-1677.489,1830.17;Inherit;False;128;OutColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;148;-1490.589,2057.17;Inherit;False;101;c1;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-1710.253,-552.233;Inherit;False;4;4;0;FLOAT;1.2;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;154;-1590.374,1947.489;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;164;-2247.19,2572.357;Inherit;False;1196.268;368.3623;Comment;6;162;157;158;159;161;163;InnerColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.SmoothstepOpNode;107;-1533.482,-539.534;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.52;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;145;-1303.388,1941.47;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;109;-1191.149,-547.013;Inherit;False;c2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;159;-2197.19,2685.202;Inherit;False;InstancedProperty;_InnerColor;InnerColor;4;0;Create;True;0;0;0;False;0;False;0.9,0.8,0.2,1;0.9,0.8,0.1999999,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-1002.99,1949.746;Inherit;False;MidColor;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;161;-1898.297,2698.097;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;-1932.397,2622.357;Inherit;False;149;MidColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;-1897.127,2824.719;Inherit;False;109;c2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;157;-1521.112,2624.789;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;163;-1274.923,2634.227;Inherit;False;InnerColor;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;165;583.904,-17.1929;Inherit;False;163;InnerColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;130;815.9035,-27.19038;Inherit;False;FinalColor;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;199;1282.563,-309.8116;Inherit;False;604.173;194.9278;Unity 2021.3.10f1 下材质属性 Color 会自动矫正到 linear 下，导致颜色本来就暗了，不用手动转;2;198;19;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;185;-2341.2,-56.90267;Inherit;False;2150.093;544.3777;Comment;16;178;166;167;181;183;184;180;176;172;177;174;175;179;170;169;171;sparkle;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;192;-2309.725,617.5665;Inherit;False;1111;330.2747;实际测试不明显，不要了;7;186;187;188;190;189;191;193;SparkleColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.ComponentMaskNode;136;1017.145,94.64559;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;181;-847.7808,93.48132;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.98;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;177;-1996.981,252.703;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-16;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;131;1050.903,-52.19038;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;132;1297.662,100.1209;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GammaToLinearNode;198;1336.563,-249.8116;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;179;-1805.524,167.2488;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CustomExpressionNode;167;-1398.36,93.48132;Inherit;False; ;1;File;2;True;p;FLOAT2;0,0;In;;Inherit;False;True;in_timeInSeconds;FLOAT;0;In;;Inherit;False;worley;False;False;0;31435e59c03d4d9db49adfebc2a0d6e7;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;-1812.012,3.914173;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;6,6;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;178;-1759.644,371.4753;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;187;-1635.725,727.5665;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;210;559.5369,-153.1867;Inherit;False;128;OutColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;189;-2258.519,667.5665;Inherit;False;InstancedProperty;_SparkleColor;SparkleColor;0;0;Create;True;0;0;0;False;0;False;1,0.4,0.2,1;1,0.3999999,0.1999999,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;172;-2291.2,118.5733;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;166;-1089.539,151.777;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;190;-2006.725,667.5665;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GammaToLinearNode;204;1919.461,-82.49741;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;186;-2007.296,828.8412;Inherit;False;180;sparkle;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;193;-1416.259,836.3028;Inherit;False;191;SparkleColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;19;1578.736,-254.8838;Inherit;False;Property;_Keyword0;Keyword 0;1;0;Create;True;0;0;0;False;0;False;0;0;0;False;UNITY_COLORSPACE_GAMMA;Toggle;2;Key0;Key1;Fetch;False;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;1619.486,-56.20314;Inherit;False;92;c;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;169;-2033.759,-6.90273;Inherit;False;24;uv;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;171;-1604.328,2.832447;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;191;-1422.725,720.5665;Inherit;False;SparkleColor;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;183;-610.6172,169.7748;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;184;-852.108,240.5911;Inherit;False;86;m;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;188;-1834.725,701.5665;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;208;2038.997,266.4914;Inherit;False;163;InnerColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;-2112.721,122.8999;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;207;2031.758,67.02942;Inherit;False;128;OutColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;180;-422.3139,138.8881;Inherit;False;sparkle;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;206;1793.346,-56.90439;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;209;2035.997,172.4914;Inherit;False;149;MidColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SinOpNode;174;-1980.755,122.9001;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;176;-2230.625,242.9677;Inherit;False;36;iTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;8;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormalsOnly;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;7;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;9;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormalsOnly;False;True;15;d3d9;d3d11_9x;d3d11;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;6;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;2303.244,24.44259;Float;False;True;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;16;ToonFire_ase;02c48f6cf12f440e8c22c1d6e9d72158;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForwardOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;22;Surface;1;638019337050034508;  Blend;0;0;Two Sided;1;0;Cast Shadows;0;638019337060660994;  Use Shadow Threshold;0;0;Receive Shadows;0;638019337063371276;GPU Instancing;0;638019337065623252;LOD CrossFade;0;0;Built-in Fog;0;0;DOTS Instancing;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;True;False;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1424.216,-47.94341;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;02c48f6cf12f440e8c22c1d6e9d72158;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.CommentaryNode;71;149.2112,-721.5138;Inherit;False;735.9407;100;https://www.shadertoy.com/view/cdlGzS;0;shadertoy 链接;1,1,1,1;0;0
WireConnection;36;0;35;0
WireConnection;14;0;13;1
WireConnection;14;1;13;2
WireConnection;15;0;13;3
WireConnection;15;1;13;4
WireConnection;44;0;42;0
WireConnection;11;0;10;0
WireConnection;11;1;14;0
WireConnection;11;2;15;0
WireConnection;43;0;44;0
WireConnection;24;0;11;0
WireConnection;30;1;38;0
WireConnection;45;0;43;0
WireConnection;47;0;46;0
WireConnection;73;0;72;0
WireConnection;31;1;30;0
WireConnection;49;0;45;0
WireConnection;49;1;47;0
WireConnection;34;0;33;0
WireConnection;50;0;34;0
WireConnection;50;1;49;0
WireConnection;65;0;64;0
WireConnection;75;0;73;1
WireConnection;28;0;27;0
WireConnection;28;1;31;0
WireConnection;51;0;50;0
WireConnection;51;1;52;0
WireConnection;22;0;28;0
WireConnection;22;1;39;0
WireConnection;66;1;65;1
WireConnection;77;0;75;0
WireConnection;80;0;79;0
WireConnection;82;0;81;0
WireConnection;32;0;22;0
WireConnection;53;0;51;0
WireConnection;67;0;66;0
WireConnection;78;0;77;0
WireConnection;78;1;80;0
WireConnection;83;0;78;0
WireConnection;83;1;82;1
WireConnection;58;0;32;0
WireConnection;68;0;67;0
WireConnection;61;0;53;0
WireConnection;85;0;83;0
WireConnection;84;0;85;0
WireConnection;55;0;59;0
WireConnection;55;1;63;0
WireConnection;55;2;69;0
WireConnection;86;0;84;0
WireConnection;92;0;55;0
WireConnection;95;1;90;0
WireConnection;95;2;93;0
WireConnection;89;0;95;0
WireConnection;122;0;121;0
WireConnection;94;0;89;0
WireConnection;142;0;116;0
WireConnection;123;0;122;1
WireConnection;140;0;139;0
WireConnection;135;0;142;0
WireConnection;135;3;123;0
WireConnection;98;1;97;0
WireConnection;98;2;100;0
WireConnection;144;0;137;0
WireConnection;99;0;98;0
WireConnection;118;1;135;0
WireConnection;118;2;119;0
WireConnection;111;0;110;0
WireConnection;197;1;140;1
WireConnection;143;0;138;0
WireConnection;113;1;111;1
WireConnection;101;0;99;0
WireConnection;141;0;144;0
WireConnection;141;1;143;0
WireConnection;141;2;197;0
WireConnection;128;0;118;0
WireConnection;108;1;105;0
WireConnection;108;2;106;0
WireConnection;108;3;113;0
WireConnection;154;0;141;0
WireConnection;107;0;108;0
WireConnection;145;0;146;0
WireConnection;145;1;154;0
WireConnection;145;2;148;0
WireConnection;109;0;107;0
WireConnection;149;0;145;0
WireConnection;161;0;159;0
WireConnection;157;0;162;0
WireConnection;157;1;161;0
WireConnection;157;2;158;0
WireConnection;163;0;157;0
WireConnection;130;0;165;0
WireConnection;136;0;130;0
WireConnection;181;0;166;0
WireConnection;177;0;176;0
WireConnection;131;0;130;0
WireConnection;132;0;136;0
WireConnection;198;0;131;0
WireConnection;179;0;174;0
WireConnection;179;1;177;0
WireConnection;167;0;171;0
WireConnection;167;1;178;0
WireConnection;170;0;169;0
WireConnection;187;0;188;0
WireConnection;187;1;186;0
WireConnection;166;0;167;0
WireConnection;190;0;189;0
WireConnection;204;0;206;0
WireConnection;19;1;198;0
WireConnection;19;0;131;0
WireConnection;171;0;170;0
WireConnection;171;1;179;0
WireConnection;191;0;187;0
WireConnection;183;0;181;0
WireConnection;183;1;184;0
WireConnection;188;0;190;0
WireConnection;175;0;172;0
WireConnection;180;0;183;0
WireConnection;206;0;203;0
WireConnection;174;0;175;0
WireConnection;1;2;131;0
WireConnection;1;3;132;0
ASEEND*/
//CHKSM=66E9B3ED73748AC005433308E138BA4CD3D9529F
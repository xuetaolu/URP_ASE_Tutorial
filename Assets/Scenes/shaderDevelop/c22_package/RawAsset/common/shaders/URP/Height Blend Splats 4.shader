// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom Shader/URP/Height Blend (4 Splats)"
{
	Properties
	{
		_Splat1 ("Splat 1 ( Red )", 2D) = "black" {}
		_Splat2 ("Splat 2 ( Green )", 2D) = "black" {}
		_Splat3 ("Splat 3 ( Blue )", 2D) = "black" {}
		_Splat4 ("Splat 4 ( Alpha )", 2D) = "black" {}
		_Normal1 ("Normal 1 ( Red )", 2D) = "bump" {}
		_Normal2 ("Normal 2 ( Green )", 2D) = "bump" {}
		_Normal3 ("Normal 3 ( Blue )", 2D) = "bump" {}
		_Normal4 ("Normal 4 ( Alpha )", 2D) = "bump" {}
		[Header(Meta Red__HeightMap)]
		_Meta1 ("Meta 1", 2D) = "black" {}
		_Meta2 ("Meta 2", 2D) = "black" {}
		_Meta3 ("Meta 3", 2D) = "black" {}
		_Meta4 ("Meta 4", 2D) = "black" {}
		[Header(Param X__HeightSharp Y__HeightScaler)]
		_Param1 ("Sharp 1", Vector) = (1,1,1,1)
		_Param2 ("Sharp 2", Vector) = (1,1,1,1)
		_Param3 ("Sharp 3", Vector) = (1,1,1,1)
		_Param4 ("Sharp 4", Vector) = (1,1,1,1)
		_BlendPinch ("Blend Pinch", Range(0.01, 1)) = 0.02

		[Toggle(_UnityFogEnable)] _UnityFogEnable("_UnityFogEnable (default = on)", Float) = 1
	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry-100" "DisableBatching" = "True" }
		LOD 0

		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL

		
		Pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend One Zero
			ZWrite On

			HLSLPROGRAM
			#pragma multi_compile_fog

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

			#pragma shader_feature_local _UnityFogEnable

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 control : COLOR;

				float4 texcoord : TEXCOORD0;
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;

				float4 fogFactorAndVertexLight : TEXCOORD7;

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
				float4 _Param1;
				float4 _Param2;
				float4 _Param3;
				float4 _Param4;
				float _BlendPinch;
			CBUFFER_END

			TEXTURE2D(_Splat1);
			TEXTURE2D(_Normal1);
			TEXTURE2D(_Meta1);
			SAMPLER(sampler_Splat1);
			SAMPLER(sampler_Meta1);
			float4 _Splat1_ST;
			float4 _Meta1_ST;

			TEXTURE2D(_Splat2);
			TEXTURE2D(_Normal2);
			TEXTURE2D(_Meta2);
			SAMPLER(sampler_Splat2);
			SAMPLER(sampler_Meta2);
			float4 _Splat2_ST;
			float4 _Meta2_ST;


			TEXTURE2D(_Splat3);
			TEXTURE2D(_Normal3);
			TEXTURE2D(_Meta3);
			SAMPLER(sampler_Splat3);
			SAMPLER(sampler_Meta3);
			float4 _Splat3_ST;
			float4 _Meta3_ST;

			TEXTURE2D(_Splat4);
			TEXTURE2D(_Normal4);
			TEXTURE2D(_Meta4);
			SAMPLER(sampler_Splat4);
			SAMPLER(sampler_Meta4);
			float4 _Splat4_ST;
			float4 _Meta4_ST;

			float4 blendLayers
			(
				float w1, float w2, float w3, float w4, 
				float4 color1, float4 color2, float4 color3, float4 color4, 
				float h1, float h2, float h3, float h4
			)
			{
				// float w1234 = w1 + w2 + w3 + w4;
				//float wt = max(w1234, 0.01);
				//half4 control = half4(w1 / wt, w2 / wt, w3 / wt, w4 / wt);

				float4 blend;
				blend.r = h1; // * control.r;
				blend.g = h2; // * control.g;
				blend.b = h3; // * control.b;
				blend.a = h4; // * control.a;

				float amount = max(blend.r, max(blend.g, max(blend.b, blend.a)));
				blend = max(blend - amount + _BlendPinch, 0); // * control;
				blend = blend / (blend.r + blend.g + blend.b + blend.a);

				return color1 * blend.r + color2 * blend.g + color3 * blend.b + color4 * blend.a;
			}

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.normal, v.tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				o.control = v.color;

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#if _UnityFogEnabsle
					half fogFactor = ComputeFogFactor(positionCS.z);
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				o.texcoord.xy = v.texcoord.xy;
				o.texcoord.zw = 0;
				o.clipPos = positionCS;

				return o;
			}

			half4 frag(VertexOutput IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				float3 WorldNormal = normalize( IN.tSpace0.xyz );
				float3 WorldTangent = IN.tSpace1.xyz;
				float3 WorldBiTangent = IN.tSpace2.xyz;
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				ShadowCoords = TransformWorldToShadowCoord( WorldPosition );

				float4 blendWeight = IN.control;
				float2 uvs = IN.texcoord;
				float4 lay1 = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat1, uvs.xy * _Splat1_ST.xy + _Splat1_ST.zw);
				float4 lay2 = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat2, uvs.xy * _Splat2_ST.xy + _Splat2_ST.zw);
				float4 lay3 = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat3, uvs.xy * _Splat3_ST.xy + _Splat3_ST.zw);
				float4 lay4 = SAMPLE_TEXTURE2D(_Splat4, sampler_Splat4, uvs.xy * _Splat4_ST.xy + _Splat4_ST.zw);
				float4 lay1n = SAMPLE_TEXTURE2D(_Normal1, sampler_Splat1, uvs.xy * _Splat1_ST.xy + _Splat1_ST.zw);
				float4 lay2n = SAMPLE_TEXTURE2D(_Normal2, sampler_Splat2, uvs.xy * _Splat2_ST.xy + _Splat2_ST.zw);
				float4 lay3n = SAMPLE_TEXTURE2D(_Normal3, sampler_Splat3, uvs.xy * _Splat3_ST.xy + _Splat3_ST.zw);
				float4 lay4n = SAMPLE_TEXTURE2D(_Normal4, sampler_Splat4, uvs.xy * _Splat4_ST.xy + _Splat4_ST.zw);
				float4 meta1 = SAMPLE_TEXTURE2D(_Meta1, sampler_Meta1, uvs.xy * _Meta1_ST.xy + _Meta1_ST.zw);
				float4 meta2 = SAMPLE_TEXTURE2D(_Meta2, sampler_Meta2, uvs.xy * _Meta2_ST.xy + _Meta2_ST.zw);
				float4 meta3 = SAMPLE_TEXTURE2D(_Meta3, sampler_Meta3, uvs.xy * _Meta3_ST.xy + _Meta3_ST.zw);
				float4 meta4 = SAMPLE_TEXTURE2D(_Meta4, sampler_Meta4, uvs.xy * _Meta4_ST.xy + _Meta4_ST.zw);

				float w1 = blendWeight.x;
				float w2 = blendWeight.y;
				float w3 = blendWeight.z;
				float w4 = blendWeight.w;

				float h1 = pow(meta1.r * _Param1.y, _Param1.x);
				float h2 = pow(meta2.r * _Param2.y, _Param2.x);
				float h3 = pow(meta3.r * _Param3.y, _Param3.x);
				float h4 = pow(meta4.r * _Param4.y, _Param4.x);

                float4 Diffuse = blendLayers(w1, w2, w3, w4, lay1, lay2, lay3, lay4, h1, h2, h3, h4);
                float3 Normal = UnpackNormal(blendLayers(w1, w2, w3, w4, lay1n, lay2n, lay3n, lay4n, h1, h2, h3, h4));
                half3 wNormal = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));

				wNormal = NormalizeNormalPerPixel(wNormal);

				float3 Albedo = Diffuse;
				float3 Emission = 0;
				float3 Specular = 0;
				float Metallic = 0;
				float Smoothness = 1;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				// float3 RefractionColor = 1;
				// float RefractionIndex = 1;
				// float3 Transmission = 1;
				// float3 Translucency = 1;

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;
				inputData.normalWS = wNormal;
				#if _UnityFogEnable
                	inputData.fogCoord = IN.fogFactorAndVertexLight.x;
                #endif
				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = BakedGI;
				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

				#ifdef _UnityFogEnable
						// color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
				#endif

				return color;
			}

			ENDHLSL
		}
	}
}



// simple LAMBERT.
//
//	 Light mainLight = GetMainLight( ShadowCoords );
//	 float3 Atten = mainLight.color * mainLight.shadowAttenuation;
//	 float3 Ambient = LightingLambert(mainLight.color, mainLight.direction, wNormal);
//	 float3 Lighting = Atten + Ambient;
//
//	 half4 color = Diffuse;
//	 color.xyz *= Lighting;
//
//	 #if _UnityFogEnable
//	     color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
//	 #endif
//	 return color;
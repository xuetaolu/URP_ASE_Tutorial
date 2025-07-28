/*
* Copyright (c) <2018> Side Effects Software Inc.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice,
*    this list of conditions and the following disclaimer.
*
* 2. The name of Side Effects Software may not be used to endorse or
*    promote products derived from this software without specific prior
*    written permission.
*
* THIS SOFTWARE IS PROVIDED BY SIDE EFFECTS SOFTWARE "AS IS" AND ANY EXPRESS
* OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
* OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN
* NO EVENT SHALL SIDE EFFECTS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
* INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
* LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
* OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
* NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
* EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

Shader "Houdini/HoudiniStandard" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) TransGloss (A)", 2D) = "white" {}

		_BumpMap ("Normal Map", 2D) = "bump" {}

		_EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
		_EmissionMap ("Emission Map", 2D) = "white" {}

		_Metallic ("Metallic", Range (0, 1)) = 0
		_MetallicMap ("Metallic Map", 2D) = "white" {}

		_Smoothness ("Smoothness", Range (0, 1)) = 0.5
		_SmoothnessMap ("Smoothness Map", 2D) = "white" {}

		_Occlusion ("Occlusion", Range (0, 1)) = 1
		_OcclusionMap ("Occlusion Map", 2D) = "white" {}
	}
	SubShader { 
		Tags { "RenderType"="Opaque" }
		LOD 400
	
		CGPROGRAM
			#pragma target 3.0
			#pragma surface surf Standard fullforwardshadows

			fixed4 _Color;
			sampler2D _MainTex;

			sampler2D _BumpMap;

			fixed4 _EmissionColor;
			sampler2D _EmissionMap;

			half _Metallic;
			sampler2D _MetallicMap;

			half _Smoothness;
			sampler2D _SmoothnessMap;

			half _Occlusion;
			sampler2D _OcclusionMap;

			struct Input {
				float2 uv_MainTex;
				float2 uv_BumpMap;
				float2 uv_EmissionMap;
				float2 uv_MetallicMap;
				float2 uv_SmoothnessMap;
				float2 uv_OcclusionMap;
				float4 color: Color;
			};

			// https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
			// struct SurfaceOutputStandard
			// {
			//     fixed3 Albedo;      // base (diffuse or specular) color
			//     fixed3 Normal;      // tangent space normal, if written
			//     half3 Emission;
			//     half Metallic;      // 0=non-metal, 1=metal
			//     half Smoothness;    // 0=rough, 1=smooth
			//     half Occlusion;     // occlusion (default 1)
			//     fixed Alpha;        // alpha for transparencies
			// };

			void surf ( Input IN, inout SurfaceOutputStandard o ) {
				fixed4 tex = tex2D( _MainTex, IN.uv_MainTex );
				o.Albedo = tex.rgb * _Color.rgb * IN.color.rgb;
				o.Normal = UnpackNormal( tex2D( _BumpMap, IN.uv_BumpMap ) );
				o.Emission = tex2D( _EmissionMap, IN.uv_EmissionMap ).rgb * _EmissionColor;

				fixed4 metallic = tex2D( _MetallicMap, IN.uv_MetallicMap );
				o.Metallic = metallic.rgb * _Metallic;
				o.Smoothness = tex2D(_SmoothnessMap, IN.uv_SmoothnessMap).r * _Smoothness;
				o.Occlusion = tex2D(_OcclusionMap, IN.uv_OcclusionMap).r * _Occlusion;
				o.Alpha = tex.a * _Color.a;
			}
		ENDCG
	}

	FallBack "Specular"
}

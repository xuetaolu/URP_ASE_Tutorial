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

Shader "Houdini/HoudiniSpecularAlpha" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) TransGloss (A)", 2D) = "white" {}

		_SpecColor ("Specular Color", Color) = (0.2, 0.2, 0.2, 1)
		_SpecMap ("Specular Map", 2D) = "white" {}

		_BumpMap ("Normal Map", 2D) = "bump" {}

		_EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
		_EmissionMap ("Emission Map", 2D) = "white" {}

		_Smoothness ("Smoothness", Range (0, 1)) = 0.5
		_SmoothnessMap ("Smoothness Map", 2D) = "white" {}

		_Occlusion ("Occlusion", Range (0, 1)) = 1
		_OcclusionMap ("Occlusion Map", 2D) = "white" {}

		_OpacityMap ("Opacity Map", 2D) = "white" {}
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 400
	
		CGPROGRAM
			#pragma target 3.5
			#pragma surface surf StandardSpecular fullforwardshadows alpha

			fixed4 _Color;
			sampler2D _MainTex;

			//fixed4 _SpecColor; // Defined in the includes
			sampler2D _SpecMap;

			sampler2D _BumpMap;

			fixed4 _EmissionColor;
			sampler2D _EmissionMap;

			half _Smoothness;
			sampler2D _SmoothnessMap;

			half _Occlusion;
			sampler2D _OcclusionMap;

			sampler2D _OpacityMap;

			struct Input {
				float2 uv_MainTex;
				float2 uv_SpecMap;
				float2 uv_EmissionMap;
				float2 uv_BumpMap;
				float2 uv_SmoothnessMap;
				float2 uv_OcclusionMap;
				float2 uv_OpacityMap;
				float4 color: Color;
			};

			// https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
			//struct SurfaceOutputStandardSpecular
			//{
			//    fixed3 Albedo;      // diffuse color
			//    fixed3 Specular;    // specular color
			//    fixed3 Normal;      // tangent space normal, if written
			//    half3 Emission;
			//    half Smoothness;    // 0=rough, 1=smooth
			//    half Occlusion;     // occlusion (default 1)
			//    fixed Alpha;        // alpha for transparencies
			//};

			void surf ( Input IN, inout SurfaceOutputStandardSpecular o ) {
				fixed4 tex = tex2D( _MainTex, IN.uv_MainTex );
				o.Albedo = tex.rgb * _Color.rgb * IN.color.rgb;
				o.Specular = tex2D( _SpecMap, IN.uv_SpecMap ).rgb * _SpecColor;
				o.Normal = UnpackNormal( tex2D( _BumpMap, IN.uv_BumpMap ) );
				o.Emission = tex2D( _EmissionMap, IN.uv_EmissionMap ).rgb * _EmissionColor;
				o.Smoothness = tex2D(_SmoothnessMap, IN.uv_SmoothnessMap).r * _Smoothness;
				o.Occlusion = tex2D(_OcclusionMap, IN.uv_OcclusionMap).r * _Occlusion;
				o.Alpha =  tex2D(_OpacityMap, IN.uv_OpacityMap).r * tex.a * _Color.a * IN.color.a;
			}
		ENDCG
	}

	Fallback "Transparent/Specular", 1
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "xue/xBRZ"
{
	Properties
	{
		decal("decal (RGB)", 2D) = "white" {} //you should also set this in scripting with material.SetTexture ("decal", yourDecalTexture)
//		texture_size("texture_size", Vector) = (256,224,0,0) // 直接用 decal_
		[KeywordEnum(x2,x4,x6)] _SCALE("SCALE", Float) = 1
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{	
			ZWrite Off
			ZTest Always
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma multi_compile_local _SCALE_X2 _SCALE_X4 _SCALE_X6

			#if defined(_SCALE_X2)
			#include "libretro-common-shaders/xbrz/shaders/2xbrz.cginc"
			#endif
			#if defined(_SCALE_X4)
			#include "libretro-common-shaders/xbrz/shaders/4xbrz.cginc"
			#endif
			#if defined(_SCALE_X6)
			#include "libretro-common-shaders/xbrz/shaders/6xbrz.cginc"
			#endif
			
			#pragma vertex main_vertex
			#pragma fragment main_fragment

			ENDCG
		} 
	}
}
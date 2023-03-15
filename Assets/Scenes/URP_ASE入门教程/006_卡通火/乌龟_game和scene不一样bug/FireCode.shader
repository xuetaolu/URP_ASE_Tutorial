Shader "FireCode"
{
	Properties
	{
		_GradientTex("GradientTex",2D) = "white"{}
		_NoiseTex("NoiseTex",2D) = "white"{}
		_NoiseSpeedX("NoiseSpeedX",float) = 0.1
		_NoiseSpeedY("NoiseSpeedX",float) = -2.0
		[Space]
		_Soft("Soft",Range(0,1)) = 0.4
		_NoiseIntensity("NoiseIntensity",Range(0,3)) = 0.2
		[Space]
		_InnerColor("Inner Color",Color) = (1,0.9,1,1)
		_OutColor("Out Color",Color) = (1,0.05,1,1)
		_InoutFireLerp("InoutFireLerp",Range(0,1)) = 0.4
		[Space]
		_Mask("Mask",2D) = "white"{}
	}
	
	SubShader
	{	
		Tags { "RenderType"="Transparent" }	
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			Tags { "LightMode"="UniversalForward" "RenderQueue" = "Transparent" }
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _GradientTex; float4 _GradientTex_ST;
			sampler2D _NoiseTex; float4 _NoiseTex_ST;
			float _NoiseSpeedX;
			float _NoiseSpeedY;
			
			float _Soft;
			float _NoiseIntensity;
			
			fixed4 _InnerColor;
			fixed4 _OutColor;
			float _InoutFireLerp;
			
			sampler2D _Mask; float4 _Mask_ST;

			
			v2f vert ( appdata v )
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 gradientTex = tex2D(_GradientTex,i.uv * _GradientTex_ST.xy + _GradientTex_ST.zw) ;
				
				float2 noisebias = float2(_NoiseSpeedX * _Time.y,_NoiseSpeedY * _Time.y);
				fixed4 noiseTex = tex2D(_NoiseTex,i.uv + noisebias);

				fixed3 color = lerp(_OutColor.rgb,_InnerColor.rgb,(gradientTex.r - _InoutFireLerp));

				float fireSmooth = smoothstep(noiseTex.r,(noiseTex.r + _Soft),gradientTex.r);

				fixed4 _MaskTex = tex2D(_Mask,i.uv + noiseTex * _NoiseIntensity * gradientTex);
				_MaskTex = _MaskTex * fireSmooth;

				fixed4 finalColor = fixed4(color,_MaskTex.r);
				return finalColor;
			}
			ENDCG
		}
	}
}

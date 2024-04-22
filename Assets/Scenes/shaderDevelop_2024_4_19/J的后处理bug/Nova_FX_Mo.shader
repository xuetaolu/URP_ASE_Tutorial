Shader "Nova/FX/Mo"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int) = 0

        _MainTex("MainTex", 2D) = "White"{}
        
        [HDR]_MainCol("MainCol", Color) = (1.0,1.0,1.0,1.0)
		

        _NormalTex ("NormalTex", 2D) = "bump" {}
        _BumpScale("BumpScale", Range(-10,10)) = 1





        _MatcapColor("MatCapCol", Color) = (1.0,1.0,1.0,1.0)

        _uvflow("HitIntensity",Float) = 1
        _HitSpread("HitSpread", Float) = 0
        [HDR]_HitColor("HitColor", Color) = (1.0,1.0,1.0,1.0)
        _RampTex2("HitRRampTex2", 2D) = "White"{}
        
        _MatCap("MatCapTexture", 2D) = "black"{}
        // [Toggle]_MatCapXReverse("Reverse X", float) = 1
        // [Toggle]_MatCapYReverse("Reverse Y", float) = 1
        _MatCapIntensity("MatCap Intensity", Range(-10,10)) = 1
        _MatCapContrast("MatCap Contrast", Range(-20,20)) = 1
	}
	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		Pass
		{
			Tags { "LightMode"="URPDistorted" }

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull [_Cull] 
			//Cull Back

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			
			struct VertexInput
			{
				float4 positionOS  : POSITION ;
				float4 uv          : TEXCOORD0;
                float3 normalOS    : NORMAL   ;
                float4 tangentOS   : TANGENT  ;
			};

			struct VertexOutput
			{
				float4 positionCS  : SV_POSITION;
                float4 uv          : TEXCOORD0;
				float3 positionWS  : TEXCOORD1;
                float4 tangentWS   : TEXCOORD2;
                float3 normalWS    : TEXCOORD3;
                float4 screenPos   : TEXCOORD4;
                float3 viewDirWS   : TEXCOORD5;
                // half2  matCapUV   : TEXCOORD10;
			};

			CBUFFER_START(UnityPerMaterial)
				// float4 _DistortedMap_ST;
                float4 _DistortedMap2_ST;
				float _uvflow;
                half _BumpScale;

                half4 _MainCol;
                float4 _NormalTex_ST, _MainTex_ST;
                half4 _MatcapColor;
 
      

                half4 _HitColor;
                half _HitSpread;
                uniform float AffectorAmount;
                uniform float4 HitPosition[20];
                uniform float HitSize[20];
                half _MatCapIntensity;
                half _MatCapContrast;
                // half _MatCapXReverse, _MatCapYReverse;
                
			CBUFFER_END

            TEXTURE2D (_NormalTex);SAMPLER(sampler_NormalTex);



			sampler2D _SourceTexDownSample;
            sampler2D _RampTex2;
			TEXTURE2D (_DistortionTexture);SAMPLER(sampler_DistortionTexture);
            TEXTURE2D (_MatCap);SAMPLER(sampler_MatCap);
            TEXTURE2D (_MainTex);SAMPLER(sampler_MainTex);


			inline float4 ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
				#else
					float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = (pos.y- o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			
			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;


                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                real sign = v.tangentOS.w * GetOddNegativeScale();
                o.positionWS = vertexInput.positionWS;
                o.normalWS = normalInput.normalWS;
                o.tangentWS = float4(normalInput.tangentWS.xyz, sign);

                float3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
                o.viewDirWS = SafeNormalize(viewDirWS);

                
                // half3 normalWS = normalInput.normalWS;
                // half3 normalVS = TransformWorldToViewDir(normalWS);
                // o.matCapUV = normalVS.xy * 0.5 + 0.5;
				o.screenPos =  ComputeScreenPos(TransformWorldToHClip(vertexInput.positionWS));
				o.uv.xy = v.uv.xy;
	
				o.positionCS = vertexInput.positionCS;

				return o;
			}

			half4 frag ( VertexOutput i) : SV_Target
			{ 
                //distord
				float4 grabScreenPos = ComputeGrabScreenPos( i.screenPos );
				float4 grabScreenPosNorm = grabScreenPos / grabScreenPos.w;
				// float2 uv1_NoiseMap = (i.uv.xy) * _DistortedMap_ST.xy + _DistortedMap_ST.zw;
				// float3 unpack13 =  tex2D(_DistortedMap, uv1_NoiseMap + float2( _uvflow.x* _Time.y ,_uvflow.y* _Time.y) ) * _uvflow.z ;

            
                half hit_result;

                for(int j = 0; j< AffectorAmount; j++)
                {
                    float distance_mask = distance(HitPosition[j], i.positionWS);
                    float hit_range = -clamp((distance_mask - HitSize[j])/ _HitSpread, -1, 0);
                    float2 ramp_uv = float2(hit_range, 0.5);
                    half hit_wave = tex2D(_RampTex2, ramp_uv).r;
                    hit_result = hit_result + hit_wave * (1 - HitSize[j]);
                }

                //return float4(hit_result,hit_result,hit_result,1);
                half hit_result2 = hit_result * _uvflow;//


                // float3 unpack132 = tex2D(_DistortedMap2, i.uv.xy)*_uvflow.w;
                
                
				

                float4 normalSample = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, (i.uv.xy*_NormalTex_ST.xy +_NormalTex_ST.zw ) + hit_result2);
      

                
                //计算Ndir
                float3 normalTS = UnpackNormalScale(normalSample, _BumpScale + (cos((_Time.y+1)*6)*0.01));
                float sign = i.tangentWS.w * GetOddNegativeScale();
                float3 tangentWS = normalize(i.tangentWS.xyz);
                float3 bitangentWS = sign * cross(i.normalWS.xyz, i.tangentWS.xyz);

                float3 nDirWS = SafeNormalize(TransformTangentToWorld(normalTS, float3x3(i.tangentWS.xyz, bitangentWS.xyz, i.normalWS.xyz)));
                
                // float3 vDirWS = normalize(i.viewDirWS);
                // float nDotv    = saturate(dot(nDirWS,vDirWS));

                half4 MainTexture = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy*_MainTex_ST.xy + _MainTex_ST.zw + half2(normalTS.x, normalTS.y) *0.1+ float4(hit_result2.xxx,0).xy*0.2);
                half4 Col = SAMPLE_TEXTURE2D(_DistortionTexture, sampler_DistortionTexture, (grabScreenPosNorm + half2(normalTS.x, normalTS.y) *0.03 + float4(hit_result2.xxx,0)).xy) ;


                //MatCap


                half3 normalVS = TransformWorldToViewDir(nDirWS);
                half2 matCapUV = normalVS.xy * 0.5 + 0.5;
                half3 matCap = SAMPLE_TEXTURE2D_LOD(_MatCap, sampler_MatCap, float2(matCapUV.x,matCapUV.y), 0) *_MatCapIntensity ;
                matCap = pow(matCap.rgb,_MatCapContrast);



                //高光
                // Light mainLight = GetMainLight();
                // float3 lightDirectionWS = normalize(mainLight.direction);
                // float3 floatDir = normalize(lightDirectionWS + vDirWS);
   
   

                //输出
                //return float4( Col.rgb - ((MainTexture*_MainCol) )   + hit_result*_HitColor.rgb+ fresnel*_MatcapColor + matCap ,1);
                return half4( Col.rgb  ,1);

			}
			ENDHLSL
		}
	}
}

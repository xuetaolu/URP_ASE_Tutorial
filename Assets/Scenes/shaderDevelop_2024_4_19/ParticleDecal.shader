Shader "Unlit/ParticleDecal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            ZTest Greater
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "ScreenDecalLib.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv_centerxy : TEXCOORD0;
                float4 centerZ_sizeXYZ : TEXCOORD1;
                float3 rotation3D : TEXCOORD2;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 viewRay : TEXCOORD0;
                float3 viewRayStartPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                float3 _centerPos = float3(v.uv_centerxy.zw, v.centerZ_sizeXYZ.x);
                float3 _size = v.centerZ_sizeXYZ.yzw;
                float3 _rotation = v.rotation3D;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.color = v.color;

                VertexViewRayParams(v.vertex.xyz, _centerPos, _size, _rotation, o.viewRay, o.viewRayStartPos);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;
	            float rawDepth = SampleSceneDepth(screenSpaceUV).r;
                float _linearEyeDepth = LinearEyeDepthPerspOrOrtho(rawDepth);
                float3 decalSpaceScenePos = i.viewRayStartPos.xyz + i.viewRay.xyz * _linearEyeDepth / i.viewRay.w;
                clip(0.5f - abs(decalSpaceScenePos));
                float2 uv = decalSpaceScenePos.xz + 0.5;
                float4 col = tex2D(_MainTex, TRANSFORM_TEX(uv, _MainTex));
                col *= i.color;
                return col;
            }
            ENDHLSL
        }
    }
}

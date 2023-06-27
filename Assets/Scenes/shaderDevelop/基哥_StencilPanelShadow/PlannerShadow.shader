//URP
Shader "custom/PlannerShadow"
{
    Properties
    {
        _Color ("投影颜色", color) = (0, 0, 0, 0.5)
        _Plane ("平面方程", vector) = (0, 1, -0.01, 0)
        //_LDir ("光照方向", vector) = (1.72, 2.47, 2.57, 0)
        _ShadowFalloff("ShadowFalloff",Range(0,1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        //投影
        Pass
        {
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
           /* Stencil {
                Ref 1
                Comp NotEqual
                Pass Replace
                Fail Keep
            }*/

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _Plane;
                float _ShadowFalloff;
            CBUFFER_END

            //任意平面求交点
            half3 GetCrossPos(half3 posIN, half3 lDir)
            {
                half t = -(_Plane.w + dot(posIN, _Plane.xyz)) / dot(lDir, _Plane.xyz);
                return t * lDir + posIN;
            }

            struct a2v
            {
                half4 posOS    : POSITION;
            };

            struct v2f
            {
                half4 posCS    : SV_POSITION;
                half4 color  : Color;
            };

            v2f vert (a2v i)
            {
                v2f o;
                Light light = GetMainLight();
                half3 posWS = TransformObjectToWorld(i.posOS.xyz);
                half3 shadowPosWS = GetCrossPos(posWS, light.direction); 
                o.posCS = TransformWorldToHClip(shadowPosWS);

                //得到中心点世界坐标
                float3 center = float3(unity_ObjectToWorld[0].w, _Plane.y, unity_ObjectToWorld[2].w);
                //计算阴影衰减
                float falloff = 1 - saturate(distance(shadowPosWS, center) * _ShadowFalloff);

                //阴影颜色
                o.color = _Color;
                o.color.a *= falloff;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return i.color;
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
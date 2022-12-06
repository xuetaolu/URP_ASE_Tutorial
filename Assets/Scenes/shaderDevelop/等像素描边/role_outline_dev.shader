// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "role_outline_dev"
{
    Properties {
            _OutlineParam("_OutlineParam", Vector) = (2, 10, 30, 0)
            _OutlineColor("_OutlineColor", Color) = (1,0,0,1)
        
            [Toggle(_DEBUG_FADE_EFFECT)]_DEBUG_FADE_EFFECT("DEBUG_FADE_EFFECT", float) = 0
        }

    SubShader
    {
        // 描边
        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Front
            ZTest Less
            ZWrite On
            Blend Off

            // Offset <factor>, <units>
            // factor	
            //   浮点数，范围 –1 到 1。	
            //   缩放最大 Z 斜率，也称为深度斜率，以生成每个多边形的可变深度偏移。
            //   不平行于近剪裁平面和远剪裁平面的多边形具有 Z 斜率。调整此值以避免此类多边形上出现视觉瑕疵。
            // units
            //   浮点数，范围 –1 到 1。
            //   缩放最小可分辨深度缓冲区值，以产生恒定的深度偏移。最小可分辨深度缓冲区值（一个 _unit_）因设备而异。
            //   负值意味着 GPU 将多边形绘制得更靠近摄像机。正值意味着 GPU 将多边形绘制得更远离摄像机。
            Offset 1,1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma shader_feature _ _DEBUG_FADE_EFFECT
            #include "UnityCG.cginc"

            struct vdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : POSITION;
                #ifdef _DEBUG_FADE_EFFECT
                float verttofrag : TEXCOORD0;
                #endif
            };
            
            half4 _OutlineColor;
            float4 _OutlineParam; // 描边宽度, 描边变细的最近/最远距离

            //
            v2f vert(vdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // 网上的标准算法
                float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, normalize(v.normal));
                float2 offset = TransformViewToProjection(norm.xy);
                // o.pos.xy += offset * o.pos.z;

                // 偏移 pixel_offset 个像素
                float2 width = (_ScreenParams.zw - 1.0) * _OutlineParam.x;
                float3 vpos = UnityObjectToViewPos(v.vertex);
                width *= smoothstep(_OutlineParam.z, _OutlineParam.y, abs(vpos.z));
                float2 pixel_offset = width * sign(offset);

                

                // 添加到 pos.xy, 乘以 w 是因为 pos 中的 xyz 都是被放大了 w 倍存放的
                o.pos.xy += pixel_offset * o.pos.w; //min(o.pos.w, 4);
                #ifdef _DEBUG_FADE_EFFECT
                o.verttofrag = float4( smoothstep(_OutlineParam.z, _OutlineParam.y, abs(vpos.z)), 0,0,0 );
                #endif
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
                //return float4(0.15, 0.04, 0.03, 1); // 28,22,7
                half factor = 1;
                #ifdef _DEBUG_FADE_EFFECT
                factor = i.verttofrag.r;
                #endif
                return _OutlineColor * factor;
            }
            ENDCG
        }
    }
}
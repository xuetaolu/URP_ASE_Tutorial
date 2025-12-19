Shader "Custom/2DPokerFlipv2"
{
    Properties
    {
        _MainTex ("Back (背面 - 红色)", 2D) = "white" {}
        _FrontTex ("Front (正面 - 黑桃A)", 2D) = "white" {}
        _Flip ("Flip Progress (0-1)", Range(0, 1)) = 0
        _ShadowIntensity ("Shadow (折痕阴影)", Range(0, 1)) = 0.2
        _ShadowDistance  ("Shadow (折痕阴影范围)", Range(0, 1)) = 0.2
        _StartFlipUVPos ("开始翻折的位置UV", Vector) = (0, 0, 0, 0)
        _EndFlipUVPos ("最终翻折的位置UV", Vector) = (0.5, 0.5, 0, 0)
        _CardWidth ("卡片宽度", Range(0.01, 10)) = 1
        _CardHeight ("卡片高度", Range(0.01, 10)) = 1
        [Toggle]_DebugFrontUV ("Debug Front UV", Float) = 0
        [Toggle]_DebugBackUV ("Debug Back UV", Float) = 0
        [Toggle]_FlipFrontUVX ("Flip Front UVX", Float) = 0
        [Toggle]_FlipFrontUVY ("Flip Front UVY", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _FrontTex;
            float _Flip;
            float _ShadowIntensity;
            float _ShadowDistance;
            float _DebugFrontUV;
            float _DebugBackUV;
            float _FlipFrontUVX;
            float _FlipFrontUVY;
            float2 _StartFlipUVPos;
            float2 _EndFlipUVPos;
            float _CardWidth;
            float _CardHeight;


            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // uv (0,0)~(1,1) uv.x 是卡片宽度方向 对应 _CardWidth, uv.y 是卡片高度方向 对应 _CardHeight
                float2 uv = i.uv.xy;

                float2 _cardSize = float2(_CardWidth, _CardHeight);
                // uv to obj ; objPos = uv * _cardSize;
                // obj to uv ; __uv = objPos / _cardSize;
                float2 objPos = uv * _cardSize;
                

                float2 _startFilpObjPos = _StartFlipUVPos * _cardSize;
                float2 _endFlipObjPos = _EndFlipUVPos * _cardSize;

                // 翻转锚点
                float2 rollPovit = lerp(_startFilpObjPos, _endFlipObjPos, _Flip);

                // 获取翻转方向 (rollX)
                float2 rollX = normalize(_endFlipObjPos - _startFilpObjPos);

                // 获取垂直方向 (rollY) - 逆时针旋转 90 度
                float2 rollY = float2(-rollX.y, rollX.x);

                // rollX 和 rollY 是你之前计算出的 float2
                float2x2 R = float2x2(
                    rollX.x, rollY.x,  // 
                    rollX.y, rollY.y   // 
                );
                // 验证：
                // float2 resultX = mul(R, float2(1, 0)); // 结果等于 rollX
                // float2 resultY = mul(R, float2(0, 1)); // 结果等于 rollY

                float2x2 invR = transpose(R);

                // 1. 计算背面可见性
                float backAlpha = 1;
                {
                    float2 tmpObjPos = objPos;
                    tmpObjPos -= rollPovit;
                    tmpObjPos = mul(invR, tmpObjPos); // 让折痕上方向对齐 objUp
                    backAlpha = tmpObjPos.x >= 0;
                }


                float4 _backColor = tex2D(_MainTex, i.uv);
                _backColor.a *= backAlpha;


                // 2. 计算翻转过来的正面可见性
                float frontAlpha = 0;
                float2 frontRefObjPos;  // 假设正面可见，那么他对于 objPos 空间下是什么位置呢
                float frontDist; // 正面可见离折痕距离
                {
                    float2 tmpObjPos = objPos;
                    tmpObjPos -= rollPovit;
                    tmpObjPos = mul(invR, tmpObjPos); // 让折痕上方向对齐 objUp
                    {
                        frontDist = max(0, tmpObjPos.x);     // 正面可见离折痕距离
                    }
                    tmpObjPos.x = -tmpObjPos.x;       // 翻折
                    tmpObjPos = mul(R, tmpObjPos);    // 折痕朝向恢复
                    tmpObjPos += rollPovit;
                    frontRefObjPos = tmpObjPos;
                }
                float2 frontRefUVPos = frontRefObjPos / _cardSize;
                if (_FlipFrontUVX)
                    frontRefUVPos.x = 1 - frontRefUVPos.x;
                if (_FlipFrontUVY)
                    frontRefUVPos.y = 1 - frontRefUVPos.y;
                float4 _frontColor = tex2D(_FrontTex, frontRefUVPos);
                _frontColor.a *= backAlpha;

                // 增加折痕阴影：越靠近折痕 (dist 接近 f) 越暗
                float atten = smoothstep(0, _ShadowDistance, frontDist);
                _frontColor.rgb *= lerp(1.0 - _ShadowIntensity, 1.0, atten);
                
                if (_DebugBackUV)
                    _backColor.rgb = float3(i.uv.xy, 0);
                if (_DebugFrontUV)
                    _frontColor.rgb = float3(frontRefUVPos.xy, 0);

                fixed4 finalCol = fixed4(0,0,0,0);
                // alpha blend 混合 _backColor 和 _frontColor
                finalCol.rgb = lerp(_backColor.rgb, _frontColor.rgb, _frontColor.a);
                finalCol.a = _frontColor.a + _backColor.a * (1 - _frontColor.a);

                return finalCol;
            }
            ENDCG
        }
    }
}
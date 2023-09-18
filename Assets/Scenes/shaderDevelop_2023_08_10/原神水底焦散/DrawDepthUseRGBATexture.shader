Shader "Hidden/DrawDepthUseRGBATexture"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        [KeywordEnum(Opengl,Direct)] _InputDepthMode("InputDepthMode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        ZTest Always
        ZWrite On

        Pass
        {
            CGPROGRAM
            
            sampler2D _MainTex;
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local_fragment _INPUTDEPTHMODE_OPENGL _INPUTDEPTHMODE_DIRECT
            #pragma multi_compile_local_fragment _ _KEEP_RAW_DEPTH

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 GlslToDxClipPos(float4 clipPos) {
                clipPos.y = -clipPos.y;
                clipPos.z = -0.5*clipPos.z + 0.5*clipPos.w;
                return clipPos;
            }

            float4 DxToGlslClipPos(float4 clipPos) {
                clipPos.y = -clipPos.y;
                clipPos.z = -2*clipPos.z + clipPos.w;
                return clipPos;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            struct FragOutput {
                // float4 color : SV_Target;
                fixed depth : SV_Depth;
            };

            FragOutput frag (v2f i)// : SV_Target
            {
                FragOutput o = (FragOutput)0;
                
                float4 depthSample = tex2D(_MainTex, i.uv);

                const float depthIntMax = (256*256*256-1);
                float rawDepth = (depthSample.r * 256.0*256.0*255.0 + depthSample.g*256.0*255.0 + depthSample.b*255.0 ) / depthIntMax;
                float depthn11 = rawDepth;
                #if defined(_INPUTDEPTHMODE_OPENGL)
                    // 0~1 ”≥…‰≥… -1~1
                    depthn11 = rawDepth*2-1;
                #elif defined(_INPUTDEPTHMODE_DIRECT)
                    // 1~0 ”≥…‰≥… -1~1
                    depthn11 = (1-rawDepth)*2-1;
                #endif

                // opengl like platform, near is -1, far is 1
                float outputDepth = depthn11;
                
                // DX like platform, near is 1, far is 0
                #if UNITY_REVERSED_Z
                    outputDepth = GlslToDxClipPos(float4(0,0,outputDepth,1)).z;
                #endif
                
                #ifdef _KEEP_RAW_DEPTH
                o.depth = rawDepth;
                #else
                o.depth = outputDepth;
                #endif
                
                return o;
            }
            ENDCG
        }
    }
}

Shader "Hidden/DrawR32UseRGBATexture"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
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
            

            // void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            // {
            //     Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            // }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            

            float frag (v2f i) : SV_Target
            {
                float4 sample = tex2D(_MainTex, i.uv);

                float4 divs;
                divs.x = 256.0;
                divs.y = 1.0;
                divs.z = 1.0/256.0;
                divs.w = 1.0/256.0/256.0;
                
                
                return dot(sample*255, divs);
            }
            ENDCG
        }
    }
}

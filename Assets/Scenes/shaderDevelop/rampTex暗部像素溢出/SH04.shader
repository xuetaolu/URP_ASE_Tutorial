//  Unlit/SH0501 是unity里选择 的sheder的路径
Shader "Unlit/SH04"
{
    Properties
    {
        _LightCol ("光照颜色",color) = (1.0,1.0,1.0,1.0)
        _NormalTex ("RGB:法线贴图", 2D) ="bump"{}

        _RampTex ("颜色映射",  2D) ="white"{}
        
        [Toggle(USE_RAMP_MAP)] _USE_RAMP_MAP("USE_RAMP_MAP", float) = 1
        //_aa01("aa01",   Range(0,5)) = 0.5
        //_aa02("aa02",   Range(0,5)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "FORWARD"
            // Tags {"LightMode" = "ForwardBase"}
            Tags {"LightMode" = "UniversalForward"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile_fwdbase_fullshadows
            #pragma tagget 3.0

            #pragma multi_compile _ USE_RAMP_MAP

            #include "UnityCG.cginc"
            
            //  接受阴影01
            //      引入 AutoLight.cginc 文件  在 Editor\Data\CGIncludes\AutoLight.cginc unity
            #include "AutoLight.cginc"
            //      引入 Lighting.cginc 文件 用于引入 _LightColor0 其实这是在 Lighting.cginc里 引入的 UnityLightingCommon.cginc中
            #include "Lighting.cginc"
            
            // #include "../pxfCG/MyCGinc.cginc"

            //  光照颜色
            float3 _LightCol;
            sampler2D _NormalTex;
            sampler2D _RampTex;



            
            //  输入结构
            struct VertexInput
            {
                float4 vertex   : POSITION;   //  顶点数据
                float3 normal   : NORMAL;     //  (模型)法向
                float4 tangent  : TANGENT;    //  切线空间    法线贴图相关
                float2 uv0      : TEXCOORD0;  //  uv2 在环境遮蔽贴图 的时候 用到uv

            };

            //  输出结构
            struct VertexOutput
            {
                float4 pos    : SV_POSITION;    //  (剪裁空间)顶点数据
                float2 uv0    : TEXCOORD0;  //  在环境遮蔽贴图 的时候 用到uv 也用于法线贴图uv采样
                float4 posWS  : TEXCOORD1;  //  (模型)顶点空间坐标
                float3 nDirWS : TEXCOORD2;  //  (模型)法向
                float3 tDirWS : TEXCOORD3;  //  (模型)切线空间向量       //  法线贴图相关
                float3 bDirWS : TEXCOORD4;  //  (模型)副切线空间朝向     //  法线贴图相关
                

                //  接受阴影02
                UNITY_LIGHTING_COORDS(5, 6)

            };

            //  VertexOutput vert() 顶点函数
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                //  变换为(剪裁)顶点数据
                o.pos = UnityObjectToClipPos(v.vertex);               
                o.uv0 = v.uv0;
                //  变换为(模型)顶点空间坐标
                o.posWS = mul(unity_ObjectToWorld, v.vertex);              
                //  变换为(世界)法向
                o.nDirWS = UnityObjectToWorldNormal(v.normal);
                //  法线贴图相关
                o.tDirWS = normalize(mul(unity_ObjectToWorld,float4(v.tangent.xyz, 0.0)));
                o.bDirWS = normalize(cross(o.nDirWS, o.tDirWS) * v.tangent.w); 

                

                //  接受阴影03-------------------------------------------------------------
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                //  --------接受阴影03-----------------------------------------------------

                return o;
            }


            //  fixed4 frag()片段函数
            //      VertexOutput i(VertexOutput结构体 的形参i) 输入 vert()返回的(VertexOutput o)
            float4 frag (VertexOutput i) : COLOR
            {

                float3 nDir = i.nDirWS;
                //  法线贴图------------------------------------------------------------------------
                float3 nDirTS = UnpackNormal(tex2D(_NormalTex, i.uv0)).rgb;
                float3x3 TBN = float3x3(i.tDirWS, i.bDirWS, i.nDirWS);
                float3 nDirWS = normalize(mul(nDirTS, TBN));
                //  --------法线贴图----------------------------------------------------------------
                //  光照方向 平行光
                float3 lDirWS = _WorldSpaceLightPos0;
                
                //  漫反射
                float ndotl = dot(nDirWS,lDirWS);
                //  兰伯特反射  去掉负值
                float lambert = max(0.0,ndotl);

                //  接受阴影04-------------------------------------------------------------
                float shadow = LIGHT_ATTENUATION(i);
                //  --------接受阴影04-----------------------------------------------------
                
                // RampTex
                //float lambert02 = lambert * 0.5 + 0.5;
                float RampTexV = 0.2;
                float2 RampTexUV = float2(lambert, RampTexV);
                float3 rampTex = tex2D(_RampTex, RampTexUV);

                rampTex *= smoothstep(0.0, 0.01, lambert);
                
                float3 finalRGB01 = rampTex * shadow;
                float3 finalRGB02 = lambert * shadow;
                //  注意 导入aaa的渐变纹 在unity里把图片 Wrap Mode 调成Clamp 模式 否则亮部有白点/暗点
                //                      在unity里把图片 Compression 调成None 模式 不压缩图片

                #ifdef USE_RAMP_MAP
                    return float4 (finalRGB01, 1.0);
                #else
                    return float4 (finalRGB02, 1.0);
                #endif
                
                
            }
            ENDCG
        }
    }
    //  利用回滚"Diffuse" 来让编写的shader 投射平行光阴影 + 点光源阴影
    FallBack "Diffuse"
}

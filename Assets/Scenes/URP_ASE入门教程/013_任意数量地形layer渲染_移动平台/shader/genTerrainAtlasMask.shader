Shader "Unlit/genTerrainAtlasMask"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "black" {}
        _SplatAlpha1 ("Texture", 2D) = "black" {}
        _SplatAlpha2 ("Texture", 2D) = "black" {}
        _SplatAlpha3 ("Texture", 2D) = "black" {}
        _SplatAlpha4 ("Texture", 2D) = "black" {}
        _LayerCount ("Texture", Int) = 16
        
        _DangerZone1 ("Texture", 2D) = "black" {}
        _DangerZone2 ("Texture", 2D) = "black" {}
        _DangerZone3 ("Texture", 2D) = "black" {}
        _DangerZone4 ("Texture", 2D) = "black" {}
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "CalcMaxIdAndFactor"
            ZTest Always ZWrite Off
            
            CGPROGRAM
            #include "TerrainAltasLib.hlsl"
            
            sampler2D _SplatAlpha1;
            sampler2D _SplatAlpha2;
            sampler2D _SplatAlpha3;
            sampler2D _SplatAlpha4;
            sampler2D _DangerZone1;
            sampler2D _DangerZone2;
            sampler2D _DangerZone3;
            sampler2D _DangerZone4;
            int _LayerCount;
            
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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // TODO: 外部传入贴图 rgba 决定，
            // At Id1 则读取此 uv 对应的 col.r 的值
            // At Id2 则读取此 uv 对应的 col.g 的值，如此类推
            // note: 需要提前判断当前位置是 danger
            bool DeterminePixelMainLayerAtId1(int id, float2 uv, const float isDangers[MAX_LAYER_COUNT])
            {
                // 例如 id 0 第一个地块最大，0为基础地块，镶嵌随机地块
                // return id == 0;
                for (int i=0; i<MAX_LAYER_COUNT; i++)
                {
                    if (isDangers[i] > 0)
                    {
                        return i == id;
                    }
                }
                return false;
                // return isDangers[id] > 0;
            }

            float4 CalcMain2IndexAndRatio(
                const float weights[MAX_LAYER_COUNT],
                const float isDangers[MAX_LAYER_COUNT],
                const float dangerCount,
                const float dangerEdgeCount,
                float2 pixelUV
                )
            {
                float4 res = (float4)0;
                
                int mainIndex1 = -1;
                float mainWeight1 = 0.0;
                int mainIndex2 = -1;
                float mainWeight2 = 0.0;

                // 临时判断层级优先级，这里取的是同一连续像素块中，面积最大的
                // const int MAIN_INDEX1 = DeterminePixelMainLayerAtId1(pixelUV, isDangers);
                
                // 过渡中的像素，有2层以上的像素
                bool centerDangerPixel = dangerCount > 0.8;
                
                // 过渡边缘的像素，唯一1层，但要特殊处理
                bool dangerEdgePixel = dangerEdgeCount;

                // 唯一1层
                bool sameIdPixel = dangerCount <= 0.01;
                
                // 唯一1层, id 唯一的像素
                if(sameIdPixel)
                {
                    for (int i = 0; i < MAX_LAYER_COUNT; i++)
                    {
                        const float w = weights[i];
                        if (w > 0)
                        {
                            mainIndex1 = i;
                            mainIndex2 = i;
                        }
                    }

                    // 唯一1层, 但可能是边缘，要处理权重
                    if (dangerEdgePixel)
                    {
                        // 最优先记录权重的地块，是 1
                        // 表示最优先的地块在这个边缘开始，准备减弱
                        // if (mainIndex1 == 0)
                        if (DeterminePixelMainLayerAtId1(mainIndex1, pixelUV, isDangers))
                        {
                            mainWeight1 = 1.0;
                            mainWeight2 = 1.0-mainWeight1;
                        }
                        // 否则是 0，
                        // 表示最优先的地块在这个边缘开始消失
                        else
                        {
                            mainWeight1 = 0.0;
                            mainWeight2 = 1.0-mainWeight1;
                        }
                    }
                }
                // 若干层过渡
                else
                {
                    for (int i = 0; i < MAX_LAYER_COUNT; i++)
                    {
                        const float w = weights[i];
                        
                        // MAIN_INDEX1 优先占坑
                        if (DeterminePixelMainLayerAtId1(i, pixelUV, isDangers))
                        {
                            mainWeight1 = w;
                            mainIndex1 = i;
                            break;
                        }
                        
                        if (mainWeight1 < w)
                        {
                            mainWeight1 = w;
                            mainIndex1 = i;
                        }
                    }
                    
                    for (int i = 0; i < MAX_LAYER_COUNT; i++)
                    {
                        const float w = weights[i];
                        if (i == mainIndex1)
                            continue;
                        if (mainWeight2 < w)
                        {
                            mainWeight2 = w;
                            mainIndex2 = i;
                        }
                    }
                    
                    // 如果没有次要层，但这里会有过渡，说明次要层是另一个过渡层
                    if (mainIndex2 < 0)
                    {
                        for (int i = 0; i < MAX_LAYER_COUNT; i++)
                        {
                            const bool danger = isDangers[i] > 0.8;
                            if (danger && i != mainIndex1)
                            {
                                mainIndex2 = i;
                                break;
                            }
                        }
                    }


                    

                }
                

                res.r = (float)mainIndex1 / (float)(_LayerCount-1);
                res.g = (float)mainIndex2 / (float)(_LayerCount-1);
                const float sumWeight = max( 0.00001, mainWeight1 + mainWeight2);
                res.b = mainWeight1 / sumWeight;
                res.a = mainWeight2 / sumWeight;
            
                return res;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;

                float weights[MAX_LAYER_COUNT];
                for (int _i = 0; _i < MAX_LAYER_COUNT; _i++)
                {
                    weights[_i] = 0;
                }
                
                SetArrayFourAt(weights,  0,  tex2D(_SplatAlpha1, i.uv));
                SetArrayFourAt(weights,  4,  tex2D(_SplatAlpha2, i.uv));
                SetArrayFourAt(weights,  8,  tex2D(_SplatAlpha3, i.uv));
                SetArrayFourAt(weights, 12,  tex2D(_SplatAlpha4, i.uv));

                float sumWeight = 0.0;
                for (int _i = 0; _i < MAX_LAYER_COUNT; _i++)
                {
                    sumWeight += weights[_i];
                }
                for (int _i = 0; _i < MAX_LAYER_COUNT; _i++)
                {
                    weights[_i] /= max(0.0001, sumWeight);
                }

                float dangerCount=0;
                float dangerEdgeCount=0;
                float isDangers[MAX_LAYER_COUNT];
                for (int _i = 0; _i < MAX_LAYER_COUNT; _i++)
                {
                    isDangers[_i] = 0;
                }
                SetArrayFourAt(isDangers,  0,  tex2D(_DangerZone1, i.uv));
                SetArrayFourAt(isDangers,  4,  tex2D(_DangerZone2, i.uv));
                SetArrayFourAt(isDangers,  8,  tex2D(_DangerZone3, i.uv));
                SetArrayFourAt(isDangers, 12,  tex2D(_DangerZone4, i.uv));
                for (int _i = 0; _i < MAX_LAYER_COUNT; _i++)
                {
                    // 2层以上的
                    dangerCount += isDangers[_i] >= 1.0;
                    
                    // 1层的，但隔壁是2层的，要特殊处理
                    dangerEdgeCount += 1.0 > isDangers[_i] > 0;
                }
                
                col.rgba = CalcMain2IndexAndRatio(weights, isDangers, dangerCount, dangerEdgeCount, i.uv);
                
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            Name "FixFirst"
            ZTest Always ZWrite Off
            
            CGPROGRAM
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _LayerCount;
            
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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            bool isApproximatly(float a, float b)
            {
                float d = a-b;
                return d*d < 0.0001;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;

                col = tex2D(_MainTex, i.uv);


                for(int du = -1; du <= 1; du+=2)
                {
                    for (int dv = -1; dv <=1; dv+=2)
                    {
                        float2 uv = i.uv + _MainTex_TexelSize.xy * float2(du, dv);
                        fixed4 neighbor = tex2D(_MainTex, uv);
                        if (!isApproximatly(neighbor.r, col.r))
                        {
                            float db = col.b - 0.5;
                            float s = sign(db);
                            col.b = 0.5 + pow(abs(db), 2.2) * s;
                        }
                    }
                }
                
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            Name "DangerZoneDetect"
            ZTest Always ZWrite Off
            
            CGPROGRAM
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _LayerCount;
            
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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            

            float4 isDifference(float4 col1, float4 col2)
            {
                float4 d = col1-col2;
                float4 d_square = d*d;
                float4 d_LargerThanLittle = step(0.0001, d_square);
                return d_LargerThanLittle;
            }

            float4 DetermineIsTransition(float2 uv)
            {
                float4 res = 0;
                
                // 如果像素本身不是 完整的0，也不是完整的1
                // 那就是过渡区
                float4 rgba = tex2D(_MainTex, uv);

                float4 largerThan0 = step(0.0001, rgba);
                float4 smallerThen1 = step(rgba, 0.9999);
                float4 largerThan0AndSmallerThen1 = min(largerThan0, smallerThen1);
                
                res = max(res, largerThan0AndSmallerThen1);
                


                // 如果像素隔壁与自己不一样，那自己这个像素也属于过渡区
                for (int du=-1; du <= 1; du++)
                {
                    for(int dv=-1; dv<=1; dv++)
                    {
                        if (du==0 && dv == 0)
                            continue;
                        float2 neighbor_uv = uv + _MainTex_TexelSize.xy * float2(du, dv);
                        float4 neighborRGBA = tex2D(_MainTex, neighbor_uv);
                        float4 diffWithNeighbor = isDifference(rgba, neighborRGBA);
                        
                        res = max(res, diffWithNeighbor);

                    }
                }

                // 是完整的 0 或完整的 1，和隔壁接近，那就是固定的安全区
                return res;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;

                col = DetermineIsTransition(i.uv);
                
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            Name "DangerZoneDetectSpread"
            ZTest Always ZWrite Off
            
            CGPROGRAM
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _LayerCount;
            
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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 isDifference(float4 col1, float4 col2)
            {
                float4 d = col1-col2;
                float4 d_square = d*d;
                float4 d_LargerThanLittle = step(0.0001, d_square);
                return d_LargerThanLittle ;
            }

            float4 SpreadFromNeighbor(float2 uv)
            {
                float4 res = 0;

                float4 rgba = tex2D(_MainTex, uv);
                
                // 如果像素隔壁与自己不一样，那自己这个像素也属于过渡区
                for (int du=-1; du <= 1; du++)
                {
                    for(int dv=-1; dv<=1; dv++)
                    {
                        if (du==0 && dv == 0)
                            continue;
                        float2 neighbor_uv = uv + _MainTex_TexelSize.xy * float2(du, dv);
                        float4 neighborRGBA = tex2D(_MainTex, neighbor_uv);
                        float4 diffWithNeighbor = isDifference(rgba, neighborRGBA);
                        
                        res = max(res, diffWithNeighbor*0.5);

                    }
                }

                // 是完整的 0 或完整的 1，和隔壁接近，那就是固定的安全区
                return res;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;
                col = tex2D(_MainTex, i.uv);
                col = max(col, SpreadFromNeighbor(i.uv));
                
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            Name "FixAlphaMap"
            ZTest Always ZWrite Off
            
            CGPROGRAM
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            void Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax, out float4 Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            
            float4 FixPerChannel(float2 uv)
            {
                float4 rgba = tex2D(_MainTex, uv);
                // 目前实际没有生效修改 alpha map
                // 取消注释下面这一行会修改 alpha map，把接近1，接近0的去掉，锐化一下
                // Unity_Remap_float4(rgba, float2(0.2, 0.8), float2(0, 1),  rgba);
                rgba = saturate(rgba);
                return rgba;
            }
            
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;

                col = FixPerChannel(i.uv);
                
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            // 仅可视化，渲染数据没有实际使用
            Name "DangerZoneCount"
            ZTest Always ZWrite Off
            
            CGPROGRAM
            sampler2D _DangerZone1;
            sampler2D _DangerZone2;
            sampler2D _DangerZone3;
            sampler2D _DangerZone4;
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "TerrainAltasLib.hlsl"

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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;

                float layerDangers[MAX_LAYER_COUNT];
                for (int _i=0; _i<MAX_LAYER_COUNT; _i++)
                {
                    layerDangers[_i] = 0;
                }

                SetArrayFourAt(layerDangers,  0,  tex2D(_DangerZone1, i.uv));
                SetArrayFourAt(layerDangers,  4,  tex2D(_DangerZone2, i.uv));
                SetArrayFourAt(layerDangers,  8,  tex2D(_DangerZone3, i.uv));
                SetArrayFourAt(layerDangers, 12,  tex2D(_DangerZone4, i.uv));

                float dangerSum = 0;
                for (int _i=0; _i<MAX_LAYER_COUNT; _i++)
                {
                    dangerSum += layerDangers[_i];
                }

                float dangerSum01 = (dangerSum-1) / max(1, MAX_LAYER_COUNT - 1);

                // float displaySum01 = remap(1, 16, 0, 2, dangerSum);
                const int SAFE_MAX_COUNT = 2;
                if (dangerSum <= SAFE_MAX_COUNT)
                {
                    col.rgb = dangerSum/2 * 0.5;
                }
                else
                {
                    col.rgb = lerp(float(0.5).xxx, float3(1, 0, 0), float(dangerSum-SAFE_MAX_COUNT) / float(6-SAFE_MAX_COUNT));
                }
                // col.rgb = IQPalette(dangerSum01, float3(0.500,0.500,0.000), float3(0.500,0.500,0.000), float3(0.500,0.500,0.000), float3(0.500,0.000,0.000));
                // col.rgb = dangerSum/4;
                return col;
            }
            ENDCG
        }
    }
}

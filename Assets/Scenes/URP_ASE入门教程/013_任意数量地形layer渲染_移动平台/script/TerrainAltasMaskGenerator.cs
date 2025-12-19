#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace terrain_altas
{
    [ExecuteAlways]
    public class TerrainAltasMaskGenerator : MonoBehaviour
    {
        #region 生成 mask 的shader / 材质

        public Shader genMaskShader => Shader.Find("Unlit/genTerrainAtlasMask");

        private Material _genMaskMaterial;

        public Material genMaskMaterial
        {
            get
            {
                if (_genMaskMaterial == null)
                {
                    _genMaskMaterial = new Material(genMaskShader);
                    _genMaskMaterial.hideFlags = HideFlags.HideAndDontSave;
                }

                return _genMaskMaterial;
            }
        }

        #endregion


        #region 显示数据

        public TerrainData m_TerrainData;

        public Material m_terrainMaterialAsset;

        // public Material m_generateMaskMaterial;

        [SerializeField] private RenderTexture m_generateMaskRT;

        public RenderTexture generateMaskRT => m_generateMaskRT;

        #endregion

        #region 临时数据

        private Material _terrainTemplateMaterial;

        #endregion


        // Update is called once per frame
        void Update()
        {
            TryFetchTerrainData();
            // TryFetchCustomTerrainAltasMat();
            CheckInitMask();
            InitMaterialRelevant();
            GenTerrainMask();
        }

        private void InitMaterialRelevant()
        {
            if (m_terrainMaterialAsset == null)
                return;

            // 同步需要的数据到 material asset，会保存的
            if (m_TerrainData != null)
            {
                m_terrainMaterialAsset.SetInt("_MainTexArrayCount", m_TerrainData.terrainLayers.Length);

                // tiles 单独缩放会导致 UV 不连续，mipmap level 计算跳变
                float[] tiles = new float[16];
                TerrainLayer[] terrainLayers = m_TerrainData.terrainLayers;
                int layerCount = Mathf.Min(tiles.Length, terrainLayers.Length);
                for (int i = 0; i < layerCount; i++)
                {
                    float scale = terrainLayers[i].tileSize.x;
                    tiles[i] = scale;
                }
                
                for (int i = 0; i < 4; i++)
                {
                    Vector4 v = new Vector4(
                        tiles[i * 4],
                        tiles[i * 4 + 1],
                        tiles[i * 4 + 2],
                        tiles[i * 4 + 3]
                        );
                    m_terrainMaterialAsset.SetVector($"_LayerTiles{i+1}", v);
                }
            }


            // 同步 material asset，属性到地形显示
            if (_terrainTemplateMaterial == null)
            {
                _terrainTemplateMaterial = new Material(m_terrainMaterialAsset);
                _terrainTemplateMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            else
            {
                _terrainTemplateMaterial.CopyPropertiesFromMaterial(m_terrainMaterialAsset);
            }

            _terrainTemplateMaterial.SetTexture("_MaskMap", m_generateMaskRT);

            // 地形预览使用自定义临时材质
            Terrain terrain = GetComponent<Terrain>();
            if (terrain != null)
            {
                terrain.materialTemplate = _terrainTemplateMaterial;
            }
        }

        private void CheckInitMask()
        {
            // 需要有 terrainData，地形数据
            // 需要地形数据有 alphaMap
            TerrainData terrainData = m_TerrainData;
            if (terrainData == null)
                return;
            if (terrainData.alphamapTextures.Length <= 0)
                return;
            (int w, int h) = (
                terrainData.alphamapTextures[0].width,
                terrainData.alphamapTextures[0].height);

            // 实时预览用的 rt 分配
            RenderTexture rt = m_generateMaskRT;
            if (rt == null || rt.width != w || rt.height != h)
            {
                if (rt != null)
                {
                    rt.Release();
                }

                rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
                rt.name = $"{w}x{h}_{nameof(m_generateMaskRT)}";
                m_generateMaskRT = rt;
            }
        }


        private void TryFetchTerrainData()
        {
            if (m_TerrainData != null)
                return;

            Terrain terrain = GetComponent<Terrain>();
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                if (terrainData != null)
                    m_TerrainData = terrainData;
            }
        }

        private void GetTemporaryRTs(RenderTexture[] rts, ref RenderTextureDescriptor desc)
        {
            for (int i = 0; i < rts.Length; i++)
            {
                rts[i] = RenderTexture.GetTemporary(desc);
            }
        }

        private void ReleaseTemporaryRTs(RenderTexture[] rts)
        {
            for (int i = 0; i < rts.Length; i++)
            {
                if (rts[i] != null)
                {
                    RenderTexture.ReleaseTemporary(rts[i]);
                    rts[i] = null;
                }
            }
        }

        private bool GenTerrainMask()
        {
            TerrainData terrainData = m_TerrainData;
            Material genMat = genMaskMaterial;
            if (terrainData == null || genMat == null || m_generateMaskRT == null)
                return false;

            Texture2D[] alphamapTextures = terrainData.alphamapTextures;
            
            RenderTexture targetRT = m_generateMaskRT;
            RenderTextureDescriptor desc = targetRT.descriptor;
            RenderTexture rt1 = RenderTexture.GetTemporary(desc);

            // remap 一下每个 channel，
            // 注：目前没有实际生效，shader 中真正 remap 的注释掉了
            RenderTexture[] fixAlphamaps = new RenderTexture[alphamapTextures.Length];
            {
                GetTemporaryRTs(fixAlphamaps, ref desc);

                for (int i = 0; i < fixAlphamaps.Length; i++)
                {
                    RenderTexture rt = fixAlphamaps[i];
                    Texture src = alphamapTextures[i];

                    // 目前没有真正修改 AlphaMap 的内容，
                    Graphics.Blit(src, rt, genMat, genMat.FindPass("FixAlphaMap"));
                }
            }
            

            // 过渡区域标记，以及过渡区域边缘标记
            // 共 4 张，16 channel，每 channel 记录一个 layer 的过渡区域
            //   .output == 1，表示这个区域是 0 < c < 1，或者隔壁不一样
            //   .output == 0.5，表示是边缘，需要在生成权重时特殊处理
            RenderTexture[] dangerZones = new RenderTexture[4];
            {
                GetTemporaryRTs(dangerZones, ref desc);
                for (int i = 0; i < dangerZones.Length; i++)
                {
                    RenderTexture tmpDangerRT = RenderTexture.GetTemporary(desc);
                    RenderTexture rt = dangerZones[i];
                    Texture src = null;
                    if (fixAlphamaps.Length > i)
                    {
                        src = fixAlphamaps[i];
                    }

                    Graphics.Blit(src, tmpDangerRT, genMat, genMat.FindPass("DangerZoneDetect"));
                    Graphics.Blit(tmpDangerRT, rt, genMat, genMat.FindPass("DangerZoneDetectSpread"));
                    RenderTexture.ReleaseTemporary(tmpDangerRT);
                }
            }

            // 过渡区域重叠可视化，需要在 FrameBuffer 查看
            RenderTexture dangerZoneCount = RenderTexture.GetTemporary(desc);
            {
                for (int i = 0; i < dangerZones.Length; i++)
                {
                    RenderTexture rt = dangerZones[i];
                    genMat.SetTexture($"_DangerZone{i + 1}", rt);
                }
                Graphics.Blit(null, dangerZoneCount, genMat, genMat.FindPass("DangerZoneCount"));
            }
            

            
            // 真正渲染 mask
            {
                // 设置 SplatAlpha
                for (int i = 0; i < 4; i++)
                {
                    Texture tex = null;
                    if (i < fixAlphamaps.Length)
                    {
                        tex = fixAlphamaps[i];
                    }

                    genMat.SetTexture($"_SplatAlpha{i + 1}", tex);
                }
                // 设置一共多少 layer
                int layerCount = terrainData.terrainLayers.Length;
                genMat.SetInt("_LayerCount", layerCount);

                // 真正渲染的地方
                Graphics.Blit(null, targetRT, genMat, genMat.FindPass("CalcMaxIdAndFactor"));

            }
            
            
            RenderTexture.ReleaseTemporary(rt1);
            ReleaseTemporaryRTs(dangerZones);
            ReleaseTemporaryRTs(fixAlphamaps);
            RenderTexture.ReleaseTemporary(dangerZoneCount);


            return true;
        }

        #region 销毁相关

        public static void ReleaseRT(ref RenderTexture rt)
        {
            if (rt != null)
            {
                rt.Release();
                rt = null;
            }
        }

        public static void DestoryMaterial(ref Material m)
        {
            if (m != null)
            {
                objcleaner.Destroy(m);
                m = null;
            }
        }

        private void OnDestroy()
        {
            DestoryMaterial(ref _genMaskMaterial);
            DestoryMaterial(ref _terrainTemplateMaterial);

            ReleaseRT(ref m_generateMaskRT);
        }

        #endregion

        #region Unuse
        
        // [ContextMenu("TestTmp")]
        // public void TestTmp()
        // {
        //     GenTerrainMask();
        // }

        // private void TryFetchCustomTerrainAltasMat()
        // {
        //     Terrain terrain = GetComponent<Terrain>();
        //     if (terrain != null)
        //     {
        //         Material mat = terrain.materialTemplate;
        //         if (mat != m_terrainMaterialAsset && mat.shader.name == "TerrainAtlas")
        //         {
        //             m_terrainMaterialAsset = mat;
        //         }
        //     }
        // }

        // private void TrySyncMaskTexToCustomMat()
        // {
        //     if (m_terrainMaterialAsset == null || m_generateMask == null)
        //         return;
        //     
        //     m_terrainMaterialAsset.SetTexture("_MaskMap", m_generateMask);
        //     
        // }

        #endregion
    }
}
#endif
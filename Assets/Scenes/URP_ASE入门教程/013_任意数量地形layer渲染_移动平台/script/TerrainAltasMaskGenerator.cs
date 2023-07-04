#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

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

    [ContextMenu("TestTmp")]
    public void TestTmp()
    {
        GenTerrainMask();
    }

    private bool GenTerrainMask()
    {
        TerrainData terrainData = m_TerrainData;
        Material genMat = genMaskMaterial;
        if (terrainData == null || genMat == null || m_generateMaskRT == null)
            return false;

        // 设置生成材质
        {
            // 设置 SplatAlpha
            Texture[] alphamapTextures = terrainData.alphamapTextures;
            for (int i = 0; i < 4; i++)
            {
                Texture tex = null;
                if (i < alphamapTextures.Length)
                {
                    tex = alphamapTextures[i];
                }

                genMat.SetTexture($"_SplatAlpha{i + 1}", tex);
            }

            // 设置一共多少 layer
            int layerCount = terrainData.terrainLayers.Length;
            genMat.SetInt("_LayerCount", layerCount);


            // 生成 mask
            {
                RenderTexture targetRT = m_generateMaskRT;
                Graphics.Blit(null, targetRT, genMat);
            }
            
            
        }

        return true;
    }

    #region 销毁相关

    private static void ReleaseRT(ref RenderTexture rt)
    {
        if (rt != null)
        {
            rt.Release();
            rt = null;
        }
    }
    private void DestoryMaterial(ref Material m)
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

    // private void TryFetchCustomTerrainAltasMat()
    // {
    //     Terrain terrain = GetComponent<Terrain>();
    //     if (terrain != null)
    //     {
    //         Material mat = terrain.materialTemplate;
    //         if (mat != m_terrainMaterialAsset && mat.shader.name == "flower_scene/TerrainAtlas")
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

#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrainFailProductions.PolyFew
{

    public class CombiningInformation
    {

        #region DATA_STRUCTURES

        public enum DiffuseColorSpace
        {
            NON_LINEAR,
            LINEAR
        }

        public enum CompressionType
        {
            UNCOMPRESSED,
            DXT1,
            ETC2_RGB,
            PVRTC_RGB4,
            ASTC_RGB,
            //DXT1_CRUNCHED,
        }

        public enum CompressionQuality
        {
            LOW,
            MEDIUM,
            HIGH
        }

        [System.Serializable]
        public struct Resolution
        {
            public int width;
            public int height;
        }

        [System.Serializable]
        public class TextureArrayUserSettings
        {
            public Resolution resolution;
            public FilterMode filteringMode;
            public CompressionType compressionType;
            public CompressionQuality compressionQuality;
            public int anisotropicFilteringLevel;

            #region BATCH FEW INSPECTOR VARS
            public int choiceResolutionW = 4;
            public int choiceResolutionH = 4;
            public int choiceFilteringMode = 0;
            public int choiceCompressionQuality = 1;
            public int choiceCompressionType = 0;
            #endregion BATCH FEW INSPECTOR VARS

            public TextureArrayUserSettings(Resolution resolution, FilterMode filteringMode, CompressionType compressionType, CompressionQuality compressionQuality = CompressionQuality.MEDIUM, int anisotropicFilteringLevel = 1)
            {
                this.resolution = resolution;
                this.filteringMode = filteringMode;
                this.compressionType = compressionType;
                this.compressionQuality = compressionQuality;
                this.anisotropicFilteringLevel = anisotropicFilteringLevel;
            }
        }

        [System.Serializable]
        public class TextureArrayGroup
        {
            public TextureArrayUserSettings diffuseArraySettings;
            public TextureArrayUserSettings metallicArraySettings;
            public TextureArrayUserSettings specularArraySettings;
            public TextureArrayUserSettings normalArraySettings;
            public TextureArrayUserSettings heightArraySettings;
            public TextureArrayUserSettings occlusionArraySettings;
            public TextureArrayUserSettings emissiveArraySettings;
            public TextureArrayUserSettings detailMaskArraySettings;
            public TextureArrayUserSettings detailAlbedoArraySettings;
            public TextureArrayUserSettings detailNormalArraySettings;

            public void InitializeDefaultArraySettings(Resolution resolution, FilterMode filteringMode, CompressionType compressionType, CompressionQuality compressionQuality = CompressionQuality.MEDIUM, int anisotropicFilteringLevel = 1)
            {
                diffuseArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                metallicArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                specularArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                normalArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                heightArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                occlusionArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                emissiveArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                detailMaskArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                detailAlbedoArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                detailNormalArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
            }
        }

        [System.Serializable]
        public class MaterialProperties
        {
            // BATCHFEW UI
            public bool foldOut = true;
            public int texArrIndex;
            public int matIndex;

            // BATCHFEW UI

            public string materialName;
            public Material originalMaterial;
            public Color albedoTint;
            public Vector4 uvTileOffset = new Vector4(1, 1, 0, 0);
            public float normalIntensity = 1;
            public float occlusionIntensity = 1;
            public float smoothnessIntensity = 1;
            public float glossMapScale = 1;
            public float metalIntensity = 1;
            public Color emissionColor = Color.black;
            public Vector4 detailUVTileOffset = new Vector4(1, 1, 0, 0);
            public float alphaCutoff = 0.5f;
            public Color specularColor = Color.black;
            public float detailNormalScale = 1;
            public float heightIntensity = 0.05f;
            public float uvSec = 0;
            public int alphaMode = 0;
            public bool specularWorkflow = false;

            #region PUBLIC_METHODS

            public bool IsSameAs(MaterialProperties toCompare)
            {
                if (originalMaterial == toCompare.originalMaterial) { return true; }

                if (toCompare.albedoTint != albedoTint) { return false; }

                if (toCompare.normalIntensity != normalIntensity) { return false; }

                if (toCompare.occlusionIntensity != occlusionIntensity) { return false; }

                if (toCompare.smoothnessIntensity != smoothnessIntensity) { return false; }

                if (toCompare.glossMapScale != glossMapScale) { return false; }

                if (toCompare.uvTileOffset != uvTileOffset) { return false; }

                if (toCompare.metalIntensity != metalIntensity) { return false; }

                if (toCompare.emissionColor != emissionColor) { return false; }

                if (toCompare.detailUVTileOffset != detailUVTileOffset) { return false; }

                if (toCompare.alphaCutoff != alphaCutoff) { return false; }

                if (toCompare.specularColor != specularColor) { return false; }

                if (toCompare.detailNormalScale != detailNormalScale) { return false; }

                if (toCompare.heightIntensity != heightIntensity) { return false; }

                if (toCompare.uvSec != uvSec) { return false; }

                if (toCompare.alphaMode != alphaMode) { return false; }


                return true;

            }

            public static Texture2D NewTexture()
            {
                Texture2D tex = new Texture2D(8, 4, TextureFormat.RGBAHalf, false, true);

                for (int x = 0; x < 8; ++x)
                {
                    for (int y = 0; y < 4; ++y)
                    {
                        tex.SetPixel(x, y, Color.black);
                    }
                }

                tex.Apply();

                return tex;
            }

            public void BurnAttrToImg(ref Texture2D burnOn, int index, int textureArrayIndex)
            {
                if (index >= burnOn.height)
                {
                    var t = new Texture2D(burnOn.width, index + 1, TextureFormat.RGBAHalf, false, true);
                    Color[] colors = burnOn.GetPixels();
                    t.SetPixels(0, 0, burnOn.width, burnOn.height, colors);
                    burnOn = t;
                }

                if (burnOn.width < 8)
                {
                    var t = new Texture2D(8, burnOn.height, TextureFormat.RGBAHalf, false, true);

                    Color[] colors = burnOn.GetPixels();
                    t.SetPixels(0, 0, burnOn.width, burnOn.height, colors);
                    burnOn = t;
                }

                burnOn.SetPixel(0, index, (new Color(uvTileOffset.x - 1, uvTileOffset.y - 1, uvTileOffset.z, uvTileOffset.w)));
                burnOn.SetPixel(1, index, (new Color(normalIntensity, occlusionIntensity, smoothnessIntensity, metalIntensity)));
                burnOn.SetPixel(2, index, albedoTint);
                burnOn.SetPixel(3, index, emissionColor);
                burnOn.SetPixel(4, index, new Color(specularColor.r, specularColor.g, specularColor.b, glossMapScale));
                burnOn.SetPixel(5, index, (new Color(detailUVTileOffset.x, detailUVTileOffset.y, detailUVTileOffset.z, detailUVTileOffset.w)));
                burnOn.SetPixel(6, index, (new Color(alphaCutoff, detailNormalScale, heightIntensity, uvSec)));
                burnOn.SetPixel(7, index, (new Color(textureArrayIndex, textureArrayIndex, textureArrayIndex, textureArrayIndex)));

                burnOn.Apply();
            }

            public void FillPropertiesFromMaterial(Material material, CombiningInformation combineInfo)
            {
                materialName = material.name;
                originalMaterial = material;
                normalIntensity = 1;
                occlusionIntensity = 1;
                smoothnessIntensity = 1;
                albedoTint = Color.white;
                metalIntensity = 1;
                uvTileOffset = new Vector4(1, 1, 0, 0);
                detailUVTileOffset = new Vector4(1, 1, 0, 0);
                emissionColor = Color.black;
                alphaCutoff = 0.5f;
                specularColor = Color.black;
                detailNormalScale = 1;
                heightIntensity = 0.05f;
                alphaMode = 0;
                glossMapScale = 0;

                if (material.shader.name.ToLower() == "standard (specular setup)")
                {
                    specularWorkflow = true;
                }

                if (material.HasProperty("_Color"))
                {
                    albedoTint = material.GetColor("_Color");
                }

                if (material.HasProperty("_MainTex") && material.HasProperty("_MainTex_ST"))
                {
                    uvTileOffset = material.GetVector("_MainTex_ST");
                }

                if (material.HasProperty("_GlossMapScale"))
                {
                    glossMapScale = material.GetFloat("_GlossMapScale");
                }

                if (material.HasProperty("_Glossiness"))
                {
                    smoothnessIntensity = material.GetFloat("_Glossiness");
                }

                if (material.HasProperty("_Smoothness"))
                {
                    smoothnessIntensity = material.GetFloat("_Smoothness");
                }

                if (material.HasProperty("_MetallicGlossMap") && material.GetTexture("_MetallicGlossMap") != null)
                {
                    smoothnessIntensity = glossMapScale;
                }

                if (material.HasProperty("_SpecColor"))
                {
                    specularColor = material.GetColor("_SpecColor");
                }

                if (material.HasProperty("_Metallic"))
                {
                    metalIntensity = material.GetFloat("_Metallic");
                }

                if (material.HasProperty("_OcclusionStrength"))
                {
                    occlusionIntensity = material.GetFloat("_OcclusionStrength") + 1f;
                }

                if (material.HasProperty("_BumpScale"))
                {
                    normalIntensity = material.GetFloat("_BumpScale");
                }

                if (material.HasProperty("_DetailNormalMapScale"))
                {
                    detailNormalScale = material.GetFloat("_DetailNormalMapScale");
                }

                if (material.HasProperty("_EmissionColor") && material.HasProperty("_EmissionMap") && combineInfo.ShouldGenerateEmissionArray())
                {
                    emissionColor = Color.black;
                }

                else if (material.HasProperty("_EmissionColor"))
                {
                    emissionColor = material.GetColor("_EmissionColor");
                }

                if (material.HasProperty("_Parallax"))
                {
                    heightIntensity = material.GetFloat("_Parallax");
                }

                if (material.HasProperty("_UVSec"))
                {
                    uvSec = material.GetFloat("_UVSec");
                }

                if (material.HasProperty("_DetailAlbedoMap") && material.HasProperty("_DetailAlbedoMap_ST"))
                {
                    detailUVTileOffset = material.GetVector("_DetailAlbedoMap_ST");
                }

                if (material.HasProperty("_Mode"))
                {
                    alphaMode = (int)material.GetFloat("_Mode");
                }
            }

            #endregion PUBLIC_METHODS

        }

        [System.Serializable]
        public class MeshData
        {
            public List<MeshFilter> meshFilters;
            public List<MeshRenderer> meshRenderers;
            public List<SkinnedMeshRenderer> skinnedMeshRenderers;
            public Material[] originalMaterials;
            public Mesh[] outputMeshes;
            public Matrix4x4[] outputMatrices;
        }

        [System.Serializable]
        public class CombineMetaData
        {
            public Material material;
            public MaterialProperties materialProperties;

            //For BatchFew UI
            public MaterialProperties tempMaterialProperties;
            //For BatchFew UI

            public List<MeshData> meshesData = new List<MeshData>();
        }

        [System.Serializable]
        public class MaterialEntity
        {
            public List<CombineMetaData> combinedMats = new List<CombineMetaData>();

            // FOR BATCHFEW UI
            public int textArrIndex;
            // FOR BATCHFEW UI


            #region DATA_STRUCTURES
            public Texture2D diffuseMap;
            public Texture2D metallicMap;
            public Texture2D specularMap;
            public Texture2D normalMap;
            public Texture2D heightMap;
            public Texture2D occlusionMap;
            public Texture2D emissionMap;
            public Texture2D detailMaskMap;
            public Texture2D detailAlbedoMap;
            public Texture2D detailNormalMap;
            #endregion DATA_STRUCTURES

            #region PUBLIC_METHODS

            public bool HasAnyTextures()
            {
                return
                    (
                    diffuseMap != null ||
                    heightMap != null ||
                    normalMap != null ||
                    metallicMap != null ||
                    detailAlbedoMap != null ||
                    detailNormalMap != null ||
                    detailMaskMap != null ||
                    emissionMap != null ||
                    specularMap != null ||
                    occlusionMap != null
                    );
            }

            #endregion PUBLIC_METHODS

        }

        #endregion DATA_STRUCTURES


        #region PUBLIC_METHODS

        public bool ShouldGenerateMetallicArray()
        {
            foreach (var s in materialEntities)
            {
                if (s.metallicMap != null)
                    return true;
            }
            return false;
        }

        public bool ShouldGenerateSpecularArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.specularMap != null) { return true; }
            }
            return false;
        }

        public bool ShouldGenerateNormalArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.normalMap != null) { return true; }
            }
            return false;
        }

        public bool ShouldGenerateHeightArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.heightMap != null) { return true; }
            }
            return false;
        }

        public bool ShouldGenerateOcclusionArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.occlusionMap != null) { return true; }
            }
            return false;
        }

        public bool ShouldGenerateEmissionArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.emissionMap != null) { return true; }
            }
            return false;
        }

        public bool ShouldGenerateDetailMaskArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.detailMaskMap != null) { return true; }
            }
            return false;
        }

        public bool ShouldGenerateDetailAlbedoArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.detailAlbedoMap != null) { return true; }
            }
            return false;
        }

        public bool ShouldGenerateDetailNormalArray()
        {
            foreach (MaterialEntity texture in materialEntities)
            {
                if (texture.detailNormalMap != null) { return true; }
            }
            return false;
        }

        #endregion PUBLIC_METHODS


        public List<MaterialEntity> materialEntities = new List<MaterialEntity>();

        public TextureArrayGroup textureArraysSettings = new TextureArrayGroup();

        public DiffuseColorSpace diffuseColorSpace;

        public Material[] combinedMaterials;

    }

}
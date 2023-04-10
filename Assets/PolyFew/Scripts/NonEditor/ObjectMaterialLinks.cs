using System.Collections.Generic;
using UnityEngine;
using static BrainFailProductions.PolyFewRuntime.PolyfewRuntime;

namespace BrainFailProductions.PolyFew
{

    [ExecuteInEditMode]
    public class ObjectMaterialLinks : MonoBehaviour
    {
        [SerializeField]
        private List<CombiningInformation.MaterialEntity> linkedEntities;

        public List<CombiningInformation.MaterialEntity> linkedMaterialEntities
        {
            get
            {
                return linkedEntities;
            }
            set
            {
                linkedEntities = value;

                if (value == null) { return; }

                this.materialsProperties = new List<MaterialProperties>();

                for (int a = 0; a < value.Count; a++)
                {

                    var enitity = value[a];

                    if (enitity == null) { continue; }

                    for (int b = 0; b < enitity.combinedMats.Count; b++)
                    {
                        var combinedMat = enitity.combinedMats[b];
                        var origProps = combinedMat.materialProperties;
                        var matProps = new MaterialProperties
                        (
                            origProps.texArrIndex,
                            origProps.matIndex,
                            origProps.materialName,
                            origProps.originalMaterial,
                            origProps.albedoTint,
                            origProps.uvTileOffset,
                            origProps.normalIntensity,
                            origProps.occlusionIntensity,
                            origProps.smoothnessIntensity,
                            origProps.glossMapScale,
                            origProps.metalIntensity,
                            origProps.emissionColor,
                            origProps.detailUVTileOffset,
                            origProps.alphaCutoff,
                            origProps.specularColor,
                            origProps.detailNormalScale,
                            origProps.heightIntensity,
                            origProps.uvSec
                        );

                        materialsProperties.Add(matProps);
                    }
                }
            }
        }

        public List<MaterialProperties> materialsProperties;

        public Texture2D linkedAttrImg;

        void Start()
        {
            
            var mr = GetComponent<MeshRenderer>();
            var smr = GetComponent<SkinnedMeshRenderer>();
            Material[] materials;

            if (mr != null)
            {
                materials = mr.sharedMaterials;

                if (materials != null && materials.Length > 0)
                {
                    bool isFeasible = false;

                    foreach (var mat in materials)
                    {
                        if (mat == null) { continue; }

                        string shaderName = mat.shader.name.ToLower();

                        if (shaderName == "batchfewstandard" || shaderName == "batchfewstandardspecular")
                        {
                            isFeasible = true;
                            break;
                        }
                    }

                    if (!isFeasible)
                    {
                        DestroyImmediate(this);
                    }
                }

                else
                {
                    DestroyImmediate(this);
                }
            }

            else if (smr != null)
            {
                materials = smr.sharedMaterials;

                if (materials != null && materials.Length > 0)
                {
                    bool isFeasible = false;

                    foreach (var mat in materials)
                    {
                        if (mat == null) { continue; }

                        string shaderName = mat.shader.name.ToLower();

                        if (shaderName == "batchfewstandard" || shaderName == "batchfewstandardspecular")
                        {
                            isFeasible = true;
                            break;
                        }
                    }

                    if (!isFeasible)
                    {
                        DestroyImmediate(this);
                    }
                }

                else
                {
                    DestroyImmediate(this);
                }
            }

            else
            {
                DestroyImmediate(this);
            }
        }
    }

}

using UnityEngine;

namespace BrainFailProductions.PolyFew.AsImpL
{
    /// <summary>
    /// Material data
    /// </summary>
    /// <remarks>This should be completed and extended to support more formats.</remarks>
    /// TODO: fields to store more information should be defined.
    /// TODO: texture mapping data should be defined a separare structure and stored here in a list.
    public class MaterialData
    {
        public string materialName;
        public Color ambientColor;
        public Color diffuseColor;
        public Color specularColor;
        public Color emissiveColor;
        public float shininess = 0.0f;
        public float overallAlpha = 1.0f;
        public int illumType = 0;
        public bool hasReflectionTex = false;
        public string diffuseTexPath;
        public Texture2D diffuseTex;
        public string bumpTexPath;
        public Texture2D bumpTex;
        public string specularTexPath;
        public Texture2D specularTex;
        public string opacityTexPath;
        public Texture2D opacityTex;
    }
}

using UnityEngine;

namespace BrainFailProductions.PolyFew.AsImpL
{
    [System.Serializable]
    /// <summary>
    /// Options to define how the model will be loaded and imported.
    /// </summary>
    public class ImportOptions
    {
        /// <summary>Load the OBJ file assuming its vertical axis is Z instead of Y. </summary>
        [Tooltip("load the OBJ file assuming its vertical axis is Z instead of Y")]
        public bool zUp = true;

        /// <summary>Consider diffuse map as already lit (disable lighting) if no other texture is present. </summary>
        [Tooltip("Consider diffuse map as already lit (disable lighting) if no other texture is present")]
        public bool litDiffuse = false;

        /// <summary>Consider to double-sided (duplicate and flip faces and normals. </summary>
        [Tooltip("Consider to double-sided (duplicate and flip faces and normals")]
        public bool convertToDoubleSided = false;

        /// <summary>Rescaling for the model (1 = no rescaling). </summary>
        [Tooltip("Rescaling for the model (1 = no rescaling)")]
        public float modelScaling = 1f;

        /// <summary>Reuse a model in memory if already loaded. </summary>
        [Tooltip("Reuse a model in memory if already loaded")]
        public bool reuseLoaded = false;

        /// <summary>Inherit parent layer. </summary>
        [Tooltip("Inherit parent layer")]
        public bool inheritLayer = false;

        /// <summary>Generate mesh colliders. </summary>
        [Tooltip("Generate mesh colliders")]
        public bool buildColliders = false;

        /// <summary>Generate convex mesh colliders (only active if buildColliders = true)\nNote: it could not work for meshes with too many smooth surface regions. </summary>
        [Tooltip("Generate convex mesh colliders (only active if buildColliders = true)\nNote: it could not work for meshes with too many smooth surface regions.")]
        public bool colliderConvex = false;

        /// <summary>Mesh colliders as trigger (only active if colliderConvex = true). </summary>
        [Tooltip("Mesh colliders as trigger (only active if colliderConvex = true)")]
        public bool colliderTrigger = false;

#if !UNITY_2018_3_OR_NEWER
        /// <summary>Mesh colliders inflated (only active if colliderConvex = true). </summary>
        [Tooltip("Mesh colliders inflated (only active if colliderConvex = true)")]
        public bool colliderInflate = false;

        /// <summary>Mesh colliders inflation amount (only active if colliderInflate = true). </summary>
        [Tooltip("Mesh colliders inflation amount (only active if colliderInflate = true)")]
        public float colliderSkinWidth = 0.01f;
#endif

#if UNITY_2017_3_OR_NEWER
        /// <summary>Use 32 bit indices when needed, if available. </summary>
        [Tooltip("Use 32 bit indices when needed, if available")]
        public bool use32bitIndices = true;
#endif

        /// <summary>Hide the loaded object during the loading process. </summary>
        [Tooltip("Hide the loaded object during the loading process")]
        public bool hideWhileLoading = false;

        /// <summary>Position of the object. </summary>
        [Header("Local Transform for the imported game object")]
        [Tooltip("Position of the object")]
        public Vector3 localPosition = Vector3.zero;

        /// <summary>Rotation of the object. </summary>
        [Tooltip("Rotation of the object\n(Euler angles)")]
        public Vector3 localEulerAngles = Vector3.zero;

        /// <summary>Scaling of the object\n([1,1,1] = no rescaling). </summary>
        [Tooltip("Scaling of the object\n([1,1,1] = no rescaling)")]
        public Vector3 localScale = Vector3.one;
    }
}

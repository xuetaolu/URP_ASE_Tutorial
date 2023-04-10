//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace BrainFailProductions.PolyFewRuntime
{
    
    [AddComponentMenu("")]
    public class PolyfewRuntime : MonoBehaviour
    {
        

        #region DATA_STRUCTURES


        private const int MAX_LOD_COUNT = 8;
        //public const int MAX_CONCURRENT_THREADS = 16;
//#pragma warning disable
        //private static int maxConcurrentThreads = SystemInfo.processorCount * 2;


        /// <summary>
        /// A Dictionary that holds a GameObject as key and the associated MeshRendererPair as value
        /// </summary>
        [System.Serializable]
        public class ObjectMeshPairs : Dictionary<GameObject, MeshRendererPair> { }


        /// <summary>
        /// An enum that is used to specify what kind of meshes to combine
        /// </summary>
        public enum MeshCombineTarget
        {
            SkinnedAndStatic,
            StaticOnly,
            SkinnedOnly
        }


        /// <summary>
        /// This class represents a simple data structure that holds reference to a mesh and whether that mesh is part of a MeshRenderer (Attached to MeshFilter) or SkinnedMeshRenderer. This structure is used thoroughly in various mesh simplification operations. 
        /// </summary>
        [System.Serializable]
        public class MeshRendererPair
        {
            /// <summary>
            /// Whether mesh is part of a MeshRenderer (Attached to MeshFilter) or SkinnedMeshRenderer.
            /// </summary>
            public bool attachedToMeshFilter;
            /// <summary>
            ///  A reference to a mesh
            /// </summary>
            public Mesh mesh;

            public MeshRendererPair(bool attachedToMeshFilter, Mesh mesh)
            {
                this.attachedToMeshFilter = attachedToMeshFilter;
                this.mesh = mesh;
            }

            public void Destruct()
            {
                if (mesh != null)
                {
                    DestroyImmediate(mesh);
                }
            }
        }


        /// <summary>
        /// This class represents a custom data structure that holds reference to a MeshRendererPair, the GameObject from which the MeshRendererPair was constructed and an Action object used to execute some code. 
        /// </summary>
        [System.Serializable]
        public class CustomMeshActionStructure
        {
            /// <summary>
            /// The MeshRendererPair constructed for the referenced GameObject. This contains the mesh associated with the GameObject if any and some other info about the mesh.
            /// </summary>
            public MeshRendererPair meshRendererPair;
            /// <summary>
            /// The GameObject with which this data structure is associated with.
            /// </summary>
            public GameObject gameObject;
            /// <summary>
            /// An action object that can hold some custom code to execute.
            /// </summary>
            public Action action;

            public CustomMeshActionStructure(MeshRendererPair meshRendererPair, GameObject gameObject, Action action)
            {
                this.meshRendererPair = meshRendererPair;
                this.gameObject = gameObject;
                this.action = action;
            }
        }



        /// <summary>
        /// This class holds all the available options for mesh simplification. An object of this class is needed by many of the Mesh Simplification methods for controlling the mesh simplification process.
        /// </summary>
        [System.Serializable]
        public class SimplificationOptions
        {

            /// <summary> The strength with which to reduce the polygons by. Greater strength results in fewer polygons but lower quality. The acceptable values are between [0-100] inclusive. </summary>
            public float simplificationStrength;

            /// <summary> If set to true the mesh is simplified without loosing too much quality. Please note that simplify lossless cannot guarantee optimal triangle count after simplification. It's best that you specify the simplificationStrength manually and leave this to false. Also in case if this is true then the "simplificationStrength" attribute will be disregarded.  </summary>
            public bool simplifyMeshLossless = false;

            /// <summary> Smart linking links vertices that are very close to each other. This helps in the mesh simplification process where holes or other serious issues could arise. Disabling this (where not needed) can cause a minor performance gain.</summary>
            public bool enableSmartlinking = true;

            /// <summary> Recalculate mesh normals after simplification. Use this option if you see incorrect lighting or dark regions on the simplified mesh(es). This also recalculates the tangents afterwards.</summary>
            public bool recalculateNormals = false;

            /// <summary> This option (if set to true) preserves the mesh areas where the UV seams are made. These are the areas where different UV islands are formed (usually the shallow polygon conjested areas). </summary>
            public bool preserveUVSeamEdges = false;

            /// <summary> This option (if set to true)  preserves UV foldover areas. Usually these are the areas where sharp edges, corners or dents are formed in the mesh or simply the areas where the mesh folds over. </summary>
            public bool preserveUVFoldoverEdges = false;

            /// <summary> This option (if set to true)  preserves border edges of the mesh. Border edges are the edges that are unconnected and open. Preserving border edges might lead to lesser polygon reduction but can be helpful where you see serious mesh and texture distortions. </summary>
            public bool preserveBorderEdges = false;

            /// <summary> This option (if set to true) will take into account the preservation spheres (If specified in the SimplificationOptions). A preservation sphere retains the original quality of the mesh area enclosed within it while simplifying all other areas of the mesh. Please note that mesh simplification with preservation spheres might get slow.</summary>
            public bool regardPreservationSpheres = false;

            /// <summary> The list of preservation spheres that dictate which areas of the mesh to preserve during simplification. This list will only be regarded if "regardPreservationSphere" option is set to true. </summary>
            public List<PreservationSphere> preservationSpheres = new List<PreservationSphere>();

            /// <summary> This option (if set to true) will take into account the discrete curvature of mesh surface during simplification. Taking surface curvature into account can result in very good quality mesh simplification, but it can slow the simplification process significantly.</summary>
            public bool regardCurvature = false;

            /// <summary> The maximum passes the reduction algorithm does. Higher number is more expensive but can bring you closer to your target quality. 100 is the lowest allowed value. The default value of 100 works best for most of the meshes and should not be changed. </summary>
            public int maxIterations = 100;

            /// <summary> The agressiveness of the reduction algorithm to use for this LOD level. Higher number equals higher quality, but more expensive to run. Lowest value is 7. The default value of 7 works best for most of the meshes and should not be changed. </summary>
            public float aggressiveness = 7;

            /// <summary> Using edge sort can result in very good quality mesh simplification in some cases but can be a little slow to run. </summary>
            public bool useEdgeSort = false;


            public SimplificationOptions() { }


            public SimplificationOptions(float simplificationStrength, bool simplifyOptimal, bool enableSmartlink, bool recalculateNormals, bool preserveUVSeamEdges, bool preserveUVFoldoverEdges, bool preserveBorderEdges, bool regardToleranceSphere, List<PreservationSphere> preservationSpheres, bool regardCurvature, int maxIterations, float aggressiveness, bool useEdgeSort)
            {
                this.simplificationStrength = simplificationStrength;
                this.simplifyMeshLossless = simplifyOptimal;
                this.enableSmartlinking = enableSmartlink;
                this.recalculateNormals = recalculateNormals;
                this.preserveUVSeamEdges = preserveUVSeamEdges;
                this.preserveUVFoldoverEdges = preserveUVFoldoverEdges;
                this.preserveBorderEdges = preserveBorderEdges;
                this.regardPreservationSpheres = regardToleranceSphere;
                this.preservationSpheres = preservationSpheres;
                this.regardCurvature = regardCurvature;
                this.maxIterations = maxIterations;
                this.aggressiveness = aggressiveness;
                this.useEdgeSort = useEdgeSort;
            }


        }




        /// <summary>
        /// This class is used to represent a preservation sphere. A preservation sphere retains the original quality of the mesh area enclosed within it while simplifying all other areas of the mesh. Please note that mesh simplification with preservation spheres might get slow.
        /// </summary>
        [System.Serializable]
        public class PreservationSphere
        {
            /// <summary>
            /// The position of this preservation sphere in world coordinates. Please note that this position should accurately represent the center point of the sphere.
            /// </summary>
            public Vector3 worldPosition;

            /// <summary>
            /// The diameter of this preservation sphere.
            /// </summary>
            public float diameter;


            /// <summary> 
            /// The percentage of triangles to preserve in the region enclosed by this preservation sphere.
            /// </summary>
            public float preservationStrength = 100;


            public PreservationSphere(Vector3 worldPosition, float diameter, float preservationStrength)
            {
                this.worldPosition = worldPosition;
                this.diameter = diameter;
                this.preservationStrength = preservationStrength;
            }

        }




        /// <summary>
        /// Options that define how the model will be loaded and imported.
        /// </summary>
        [System.Serializable]
        public class OBJImportOptions : PolyFew.AsImpL.ImportOptions
        {

        }


        /// <summary>
        /// Options that define how the a GameObject will be exported to wavefront OBJ.
        /// </summary>
        [System.Serializable]
        public class OBJExportOptions
        {


            /// <summary>
            /// When checked, the position of models will be taken into account on export.
            /// </summary>
            public readonly bool applyPosition = true;
            /// <summary>
            /// When checked, the rotation of models will be taken into account on export.
            /// </summary>
            public readonly bool applyRotation = true;
            /// <summary>
            /// When checked, the scale of models will be taken into account on export.
            /// </summary>
            public readonly bool applyScale = true;
            /// <summary>
            /// Should the materials associated with the GameObject to export also be exported as .MTL files.
            /// </summary>
            public readonly bool generateMaterials = true;
            /// <summary>
            /// Should the textures associated with the materials also be exported.
            /// </summary>
            public readonly bool exportTextures = true;


            public OBJExportOptions(bool applyPosition, bool applyRotation, bool applyScale, bool generateMaterials, bool exportTextures)
            {
                this.applyPosition = applyPosition;
                this.applyRotation = applyRotation;
                this.applyScale = applyScale;
                this.generateMaterials = generateMaterials;
                this.exportTextures = exportTextures;
            }
        }



        /// <summary>
        /// A wrapper class that holds a primitive numeric type and fakes them to act as reference types.
        /// </summary>
        /// <typeparam name="T"> Any primitive numeric type. Int, float, double, byte etc</typeparam>
        public class ReferencedNumeric<T> where T : struct,
        IComparable,
        IComparable<T>,
        IConvertible,
        IEquatable<T>,
        IFormattable
        {
            private T val;
            public T Value { get { return val; } set { val = value; } }

            public ReferencedNumeric(T value)
            {
                val = value;
            }
        }


        /// <summary> This class represents a merged material properties. A merged material is a material that is combined by the Batch Few material combiner.</summary>
        [System.Serializable]
        public class MaterialProperties
        {

            /// <summary> The index in the texture array this material points to</summary>
            public readonly int texArrIndex;
            /// <summary> The index in the materials array this material points to</summary>
            public readonly int matIndex;
            /// <summary> The name of the original material which was merged</summary>
            public readonly string materialName;
            /// <summary> A reference to the original material which was combined</summary>
            public readonly Material originalMaterial;
            /// <summary> The albedo tint color. Please note that the alpha value means nothing and can't be changed</summary>
            public Color albedoTint;
            /// <summary> The UV tiling</summary>
            public Vector4 uvTileOffset = new Vector4(1, 1, 0, 0);
            /// <summary> The normal intensity</summary>
            public float normalIntensity = 1;
            /// <summary> The occlusion intensity</summary>
            public float occlusionIntensity = 1;
            /// <summary> The smoothness intensity</summary>
            public float smoothnessIntensity = 1;
            /// <summary> The scale of the specular/gloss map</summary>
            public float glossMapScale = 1;
            /// <summary> The metal intensity</summary>
            public float metalIntensity = 1;
            /// <summary> The color of the emissive channel</summary>
            public Color emissionColor = Color.black;
            /// <summary> The uv tiling for detailed maps</summary>
            public Vector4 detailUVTileOffset = new Vector4(1, 1, 0, 0);
            /// <summary> The alpha cutoff value</summary>
            public float alphaCutoff = 0.5f;
            /// <summary> The specular channel color</summary>
            public Color specularColor = Color.black;
            /// <summary> The scale of the detailed normal map</summary>
            public float detailNormalScale = 1;
            /// <summary> The height intensity</summary>
            public float heightIntensity = 0.05f;
            /// <summary> Leave this alone</summary>
            public readonly float uvSec = 0;


            public MaterialProperties(int texArrIndex, int matIndex, string materialName, Material originalMaterial, Color albedoTint, Vector4 uvTileOffset, float normalIntensity, float occlusionIntensity, float smoothnessIntensity, float glossMapScale, float metalIntensity, Color emissionColor, Vector4 detailUVTileOffset, float alphaCutoff, Color specularColor, float detailNormalScale, float heightIntensity, float uvSec)
            {
                this.texArrIndex = texArrIndex;
                this.matIndex = matIndex;
                this.materialName = materialName;
                this.originalMaterial = originalMaterial;
                this.albedoTint = albedoTint;
                this.uvTileOffset = uvTileOffset;
                this.normalIntensity = normalIntensity;
                this.occlusionIntensity = occlusionIntensity;
                this.smoothnessIntensity = smoothnessIntensity;
                this.glossMapScale = glossMapScale;
                this.metalIntensity = metalIntensity;
                this.emissionColor = emissionColor;
                this.detailUVTileOffset = detailUVTileOffset;
                this.alphaCutoff = alphaCutoff;
                this.specularColor = specularColor;
                this.detailNormalScale = detailNormalScale;
                this.heightIntensity = heightIntensity;
                this.uvSec = uvSec;
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

        }


        #endregion DATA_STRUCTURES




        #region PUBLIC_METHODS


        /// <summary>
        /// Simplifies the provided gameobject include the full nested children hierarchy with the settings provided. Any errors are thrown as exceptions with relevant information. Please note that the method won't simplify the object if the simplification strength provided in the SimplificationOptions is close to 0.
        /// </summary>
        /// <param name="toSimplify"> The gameobject to simplify.</param>
        /// <param name="simplificationOptions"> Provide a SimplificationOptions object which contains different parameters and rules for simplifying the meshes. </param>
        /// <param name="OnEachMeshSimplified"> This method will be called when a mesh is simplified. The method will be passed a gameobject whose mesh is simplified and some information about the original unsimplified mesh. If you donot want to receive this callback then you can pass null as an argument here.</param>
        /// <returns> The total number of triangles after simplifying the provided gameobject inlcuding the nested children hierarchies. Please note that the method returns -1 if the method doesn't simplify the object. </returns>

        public static int SimplifyObjectDeep(GameObject toSimplify, SimplificationOptions simplificationOptions, Action<GameObject, MeshRendererPair> OnEachMeshSimplified)
        {

            if (simplificationOptions == null)
            {
                throw new ArgumentNullException("simplificationOptions", "You must provide a SimplificationOptions object.");
            }

            int totalTriangles = 0;
            float simplificationStrength = simplificationOptions.simplificationStrength;


            if (toSimplify == null)
            {
                throw new ArgumentNullException("toSimplify", "You must provide a gameobject to simplify.");
            }

            if (!simplificationOptions.simplifyMeshLossless)
            {
                if (!(simplificationStrength >= 0 && simplificationStrength <= 100))
                {
                    throw new ArgumentOutOfRangeException("simplificationStrength", "The allowed values for simplification strength are between [0-100] inclusive.");
                }

                if (Mathf.Approximately(simplificationStrength, 0)) { return -1; }
            }



            if (simplificationOptions.regardPreservationSpheres)
            {
                if (simplificationOptions.preservationSpheres == null || simplificationOptions.preservationSpheres.Count == 0)
                {
                    simplificationOptions.preservationSpheres = new List<PreservationSphere>();
                    simplificationOptions.regardPreservationSpheres = false;
                }
            }


            ObjectMeshPairs objectMeshPairs = GetObjectMeshPairs(toSimplify, true);

            if (!AreAnyFeasibleMeshes(objectMeshPairs))
            {
                throw new InvalidOperationException("No mesh/meshes found nested under the provided gameobject to simplify.");
            }


            bool runOnThreads = false;

            int trianglesCount = CountTriangles(objectMeshPairs);

            if (trianglesCount >= 2000 && objectMeshPairs.Count >= 2)
            {
                runOnThreads = true;
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                runOnThreads = false;
            }


            float quality = 1f - (simplificationStrength / 100f);
            int totalMeshCount = objectMeshPairs.Count;
            int meshesHandled = 0;
            int threadsRunning = 0;
            bool isError = false;
#pragma warning disable
            string error = "";

            object threadLock1 = new object();
            object threadLock2 = new object();
            object threadLock3 = new object();


            if (runOnThreads)
            {

                List<CustomMeshActionStructure> meshAssignments = new List<CustomMeshActionStructure>();
                List<CustomMeshActionStructure> callbackFlusher = new List<CustomMeshActionStructure>();

                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                //watch.Start();

                foreach (var kvp in objectMeshPairs)
                {

                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { meshesHandled++; continue; }

                    MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { meshesHandled++; continue; }

                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);

                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[simplificationOptions.preservationSpheres.Count];


                    if (!meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {
                            Vector3 ignoreSphereCenterLocal = gameObject.transform.InverseTransformPoint(sphere.worldPosition);

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }

                    else if (meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }


                    meshSimplifier.Initialize(meshRendererPair.mesh, simplificationOptions.regardPreservationSpheres);



                    //while (threadsRunning == maxConcurrentThreads) { } // Don't create another thread if the max limit is reached wait for existing threads to clear

                    threadsRunning++;


                    while (callbackFlusher.Count > 0)
                    {
                        var meshInfo = callbackFlusher[0];

                        callbackFlusher.RemoveAt(0);

                        if (meshInfo == null) { continue; }

                        OnEachMeshSimplified?.Invoke(meshInfo.gameObject, meshInfo.meshRendererPair);
                    }



                    Task.Factory.StartNew(() =>
                    {

                        CustomMeshActionStructure structure = new CustomMeshActionStructure
                        (

                            meshRendererPair,

                            gameObject,

                            () =>
                            {
                                var reducedMesh = meshSimplifier.ToMesh();

                                AssignReducedMesh(gameObject, meshRendererPair.mesh, reducedMesh, meshRendererPair.attachedToMeshFilter, true);

                                if (meshSimplifier.RecalculateNormals)
                                {
                                    reducedMesh.RecalculateNormals();
                                    reducedMesh.RecalculateTangents();
                                }

                                totalTriangles += reducedMesh.triangles.Length / 3;
                            }

                        );


                        try
                        {
                            if (!simplificationOptions.simplifyMeshLossless)
                            {
                                meshSimplifier.SimplifyMesh(quality);
                            }

                            else
                            {             
                                meshSimplifier.SimplifyMeshLossless();
                            }



                            // Create cannot be called from a background thread
                            lock (threadLock1)
                            {
                                meshAssignments.Add(structure);
                                /*
                                meshAssignments.Add(() =>
                                {
                                    //Debug.Log("reduced for  " + gameObject.name);

                                    var reducedMesh = meshSimplifier.ToMesh();

                                    UtilityServices.AssignReducedMesh(gameObject, meshRendererPair.mesh, reducedMesh, meshRendererPair.attachedToMeshFilter, true);

                                    triangleCount += reducedMesh.triangles.Length / 3;
                                });
                                */

                                threadsRunning--;
                                meshesHandled++;
                            }


                            lock (threadLock3)
                            {
                                CustomMeshActionStructure callbackFlush = new CustomMeshActionStructure
                                (
                                    meshRendererPair,
                                    gameObject,
                                    () => { }
                                );

                                callbackFlusher.Add(callbackFlush);
                            }

                        }
#pragma warning disable
                        catch (Exception ex)
                        {
                            lock (threadLock2)
                            {
                                threadsRunning--;
                                meshesHandled++;
                                isError = true;
                                error = ex.ToString();
                                //structure?.action();
                                //OnEachSimplificationError?.Invoke(error, structure?.gameObject, structure?.meshRendererPair);
                            }
                        }

                    }, CancellationToken.None, TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current);

                }

                //Wait for all threads to complete
                //Not reliable sometimes gets stuck
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

                while (callbackFlusher.Count > 0)
                {
                    var meshInfo = callbackFlusher[0];

                    callbackFlusher.RemoveAt(0);

                    if (meshInfo == null) { continue; }

                    OnEachMeshSimplified?.Invoke(meshInfo.gameObject, meshInfo.meshRendererPair);
                }


                while (meshesHandled < totalMeshCount && !isError)
                {
                    while (callbackFlusher.Count > 0)
                    {
                        var meshInfo = callbackFlusher[0];

                        callbackFlusher.RemoveAt(0);

                        if (meshInfo == null) { continue; }

                        OnEachMeshSimplified?.Invoke(meshInfo.gameObject, meshInfo.meshRendererPair);
                    }
                }


                if (!isError)
                {
                    foreach (CustomMeshActionStructure structure in meshAssignments)
                    {
                        structure?.action();
                    }
                }

                else
                {
                    //OnError?.Invoke(error);
                }

                //watch.Stop();
                //Debug.Log("Elapsed Time   " + watch.Elapsed.TotalSeconds + "  isPreservationActive?  " +isPreservationActive + "  reductionStrength   " + reductionStrength);
                //Debug.Log("MESHESHANDLED  " + meshesHandled + "  Threads Allowed?  " + maxConcurrentThreads + "   Elapsed Time   "  +watch.Elapsed.TotalSeconds);

            }

            else
            {
                foreach (var kvp in objectMeshPairs)
                {

                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { continue; }

                    MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { continue; }


                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);



                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[simplificationOptions.preservationSpheres.Count];


                    if (!meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }

                    else if (meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }




                    meshSimplifier.Initialize(meshRendererPair.mesh, simplificationOptions.regardPreservationSpheres);



                    if (!simplificationOptions.simplifyMeshLossless)
                    {
                        meshSimplifier.SimplifyMesh(quality);
                    }

                    else
                    {
                        meshSimplifier.SimplifyMeshLossless();
                    }

                    OnEachMeshSimplified?.Invoke(gameObject, meshRendererPair);

                    var reducedMesh = meshSimplifier.ToMesh();
                    reducedMesh.bindposes = meshRendererPair.mesh.bindposes;   
                    reducedMesh.name = meshRendererPair.mesh.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";

                    if (meshSimplifier.RecalculateNormals)
                    {
                        reducedMesh.RecalculateNormals();
                        reducedMesh.RecalculateTangents();
                    }

                    if (meshRendererPair.attachedToMeshFilter)
                    {
                        MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                        if (filter != null)
                        {
                            filter.sharedMesh = reducedMesh;
                        }
                    }

                    else
                    {
                        SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                        if (sRenderer != null)
                        {
                            sRenderer.sharedMesh = reducedMesh;
                        }
                    }

                    totalTriangles += reducedMesh.triangles.Length / 3;
                }
            }

            return totalTriangles;

        }



        /// <summary>
        /// Simplifies the meshes nested under the given gameobject(including itself) including the full nested children hierarchy with the settings provided. Retuns back a specialized data structure with the simplified meshes. Any errors are thrown as exceptions with relevant information. Please note that the method won't simplify the object if the simplification strength provided in the SimplificationOptions is close to 0.
        /// </summary>
        /// <param name="toSimplify"> The gameobject to simplify.</param>
        /// <param name="simplificationOptions"> Provide a SimplificationOptions object which contains different parameters and rules for simplifying the meshes. </param>
        /// <param name="OnEachMeshSimplified"> This method will be called when a mesh is simplified. The method will be passed a gameobject whose mesh is simplified and some information about the original unsimplified mesh.</param>
        /// <returns> A specialized data structure that holds information about all the simplified meshes and their information and the GameObjects with which they are associated. Please note that in case the simplificationStrength was near 0 the method doesn't simplify any meshes and returns null. </returns>

        public static ObjectMeshPairs SimplifyObjectDeep(GameObject toSimplify, SimplificationOptions simplificationOptions)
        {

            if (simplificationOptions == null)
            {
                throw new ArgumentNullException("simplificationOptions", "You must provide a SimplificationOptions object.");
            }

            float simplificationStrength = simplificationOptions.simplificationStrength;
            ObjectMeshPairs toReturn = new ObjectMeshPairs();


            if (toSimplify == null)
            {
                throw new ArgumentNullException("toSimplify", "You must provide a gameobject to simplify.");
            }

            if (!simplificationOptions.simplifyMeshLossless)
            {
                if (!(simplificationStrength >= 0 && simplificationStrength <= 100))
                {
                    throw new ArgumentOutOfRangeException("simplificationStrength", "The allowed values for simplification strength are between [0-100] inclusive.");
                }

                if (Mathf.Approximately(simplificationStrength, 0)) { return null; }
            }


            if (simplificationOptions.regardPreservationSpheres)
            {
                if (simplificationOptions.preservationSpheres == null || simplificationOptions.preservationSpheres.Count == 0)
                {
                    simplificationOptions.preservationSpheres = new List<PreservationSphere>();
                    simplificationOptions.regardPreservationSpheres = false;
                }
            }


            ObjectMeshPairs objectMeshPairs = GetObjectMeshPairs(toSimplify, true);

            if (!AreAnyFeasibleMeshes(objectMeshPairs))
            {
                throw new InvalidOperationException("No mesh/meshes found nested under the provided gameobject to simplify.");
            }


            bool runOnThreads = false;

            int trianglesCount = CountTriangles(objectMeshPairs);

            if (trianglesCount >= 2000 && objectMeshPairs.Count >= 2)
            {
                runOnThreads = true;
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                runOnThreads = false;
            }


            float quality = 1f - (simplificationStrength / 100f);
            int totalMeshCount = objectMeshPairs.Count;
            int meshesHandled = 0;
            int threadsRunning = 0;
            bool isError = false;
#pragma warning disable
            string error = "";

            object threadLock1 = new object();
            object threadLock2 = new object();


            if (runOnThreads)
            {

                List<CustomMeshActionStructure> meshAssignments = new List<CustomMeshActionStructure>();

                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                //watch.Start();


                foreach (var kvp in objectMeshPairs)
                {

                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { meshesHandled++; continue; }

                    MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { meshesHandled++; continue; }

                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);



                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[simplificationOptions.preservationSpheres.Count];


                    if (!meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }

                    else if (meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }


                    meshSimplifier.Initialize(meshRendererPair.mesh, simplificationOptions.regardPreservationSpheres);


                    //while (threadsRunning == maxConcurrentThreads) { } // Don't create another thread if the max limit is reached wait for existing threads to clear

                    threadsRunning++;


                    Task.Factory.StartNew(() =>
                    {

                        CustomMeshActionStructure structure = new CustomMeshActionStructure
                        (

                            meshRendererPair,

                            gameObject,

                            () =>
                            {
                                var reducedMesh = meshSimplifier.ToMesh();
                                reducedMesh.bindposes = meshRendererPair.mesh.bindposes;
                                reducedMesh.name = meshRendererPair.mesh.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";

                                if (meshSimplifier.RecalculateNormals)
                                {
                                    reducedMesh.RecalculateNormals();
                                    reducedMesh.RecalculateTangents();
                                }

                                MeshRendererPair redMesh = new MeshRendererPair(meshRendererPair.attachedToMeshFilter, reducedMesh);

                                toReturn.Add(gameObject, redMesh);
                            }

                        );


                        try
                        {
                            if (!simplificationOptions.simplifyMeshLossless)
                            {
                                meshSimplifier.SimplifyMesh(quality);
                            }

                            else
                            {
                                meshSimplifier.SimplifyMeshLossless();
                            }



                            // Create cannot be called from a background thread
                            lock (threadLock1)
                            {
                                meshAssignments.Add(structure);
                            
                                threadsRunning--;
                                meshesHandled++;
                            }


                        }
#pragma warning disable
                        catch (Exception ex)
                        {
                            lock (threadLock2)
                            {
                                threadsRunning--;
                                meshesHandled++;
                                isError = true;
                                error = ex.ToString();
                                //structure?.action();
                                //OnEachSimplificationError?.Invoke(error, structure?.gameObject, structure?.meshRendererPair);
                            }
                        }

                    }, CancellationToken.None, TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current);

                }

                //Wait for all threads to complete
                //Not reliable sometimes gets stuck
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();


                while (meshesHandled < totalMeshCount && !isError)
                {

                }


                if (!isError)
                {
                    foreach (CustomMeshActionStructure structure in meshAssignments)
                    {
                        structure?.action();
                    }
                }

                else
                {
                    //OnError?.Invoke(error);
                }

                //watch.Stop();
                //Debug.Log("Elapsed Time   " + watch.Elapsed.TotalSeconds + "  isPreservationActive?  " +isPreservationActive + "  reductionStrength   " + reductionStrength);
                //Debug.Log("MESHESHANDLED  " + meshesHandled + "  Threads Allowed?  " + maxConcurrentThreads + "   Elapsed Time   "  +watch.Elapsed.TotalSeconds);

            }

            else
            {
                foreach (var kvp in objectMeshPairs)
                {

                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { continue; }

                    MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { continue; }


                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);


                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[simplificationOptions.preservationSpheres.Count];


                    if (!meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }

                    else if (meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }


                    meshSimplifier.Initialize(meshRendererPair.mesh, simplificationOptions.regardPreservationSpheres);



                    if (!simplificationOptions.simplifyMeshLossless)
                    {
                        meshSimplifier.SimplifyMesh(quality);
                    }

                    else
                    {
                        meshSimplifier.SimplifyMeshLossless();       
                    }


                    var reducedMesh = meshSimplifier.ToMesh();
                    reducedMesh.bindposes = meshRendererPair.mesh.bindposes;   // Might cause issues
                    reducedMesh.name = meshRendererPair.mesh.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";

                    if (meshSimplifier.RecalculateNormals)
                    {
                        reducedMesh.RecalculateNormals();
                        reducedMesh.RecalculateTangents();
                    }

                    if (meshRendererPair.attachedToMeshFilter)
                    {
                        MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                        MeshRendererPair redMesh = new MeshRendererPair(true, reducedMesh);
                        toReturn.Add(gameObject, redMesh);


                        if (filter != null)
                        {
                            filter.sharedMesh = reducedMesh;
                        }


                    }

                    else
                    {
                        SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                        MeshRendererPair redMesh = new MeshRendererPair(false, reducedMesh);
                        toReturn.Add(gameObject, redMesh);

                        if (sRenderer != null)
                        {
                            sRenderer.sharedMesh = reducedMesh;
                        }
                    }

                }
            }

            return toReturn;

        }



        /// <summary>
        /// Simplifies the meshes provided in the "objectMeshPairs" argument and assigns the simplified meshes to the corresponding objects. Any errors are thrown as exceptions with relevant information. Please note that the method won't simplify the object if the simplification strength provided in the SimplificationOptions is close to 0.
        /// </summary>
        /// <param name="objectMeshPairs"> The ObjectMeshPairs data structure which holds relationship between objects and the corresponding meshes which will be simplified. You can get this structure by calling "GetObjectMeshPairs(GameObject forObject, bool includeInactive)" method.</param>
        /// <param name="simplificationOptions"> Provide a SimplificationOptions object which contains different parameters and rules for simplifying the meshes. </param>
        /// <param name="OnEachMeshSimplified"> This method will be called when a mesh is simplified. The method will be passed a gameobject whose mesh is simplified and some information about the original unsimplified mesh.  If you donot want to receive this callback then you can pass null as an argument here.</param>
        /// <returns> The total number of triangles after simplifying the provided gameobject inlcuding the nested children hierarchies. Please note that the method returns -1 is the method doesn't simplify the object. </returns>

        public static int SimplifyObjectDeep(ObjectMeshPairs objectMeshPairs, SimplificationOptions simplificationOptions, Action<GameObject, MeshRendererPair> OnEachMeshSimplified)
        {
            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            if (simplificationOptions == null)
            {
                throw new ArgumentNullException("simplificationOptions", "You must provide a SimplificationOptions object.");
            }

            int totalTriangles = 0;
            float simplificationStrength = simplificationOptions.simplificationStrength;


            if (objectMeshPairs == null)
            {
                throw new ArgumentNullException("objectMeshPairs", "You must provide the objectMeshPairs structure to simplify.");
            }

            if (!simplificationOptions.simplifyMeshLossless)
            {
                if (!(simplificationStrength >= 0 && simplificationStrength <= 100))
                {
                    throw new ArgumentOutOfRangeException("simplificationStrength", "The allowed values for simplification strength are between [0-100] inclusive.");
                }

                if (Mathf.Approximately(simplificationStrength, 0)) { return -1; }
            }


            if (!AreAnyFeasibleMeshes(objectMeshPairs))
            {
                throw new InvalidOperationException("No mesh/meshes found nested under the provided gameobject to simplify.");
            }


            if (simplificationOptions.regardPreservationSpheres)
            {
                if (simplificationOptions.preservationSpheres == null || simplificationOptions.preservationSpheres.Count == 0)
                {
                    simplificationOptions.preservationSpheres = new List<PreservationSphere>();
                    simplificationOptions.regardPreservationSpheres = false;
                }
            }


            bool runOnThreads = false;

            int trianglesCount = CountTriangles(objectMeshPairs);

            if (trianglesCount >= 2000 && objectMeshPairs.Count >= 2)
            {
                runOnThreads = true;
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                runOnThreads = false;
            }


            float quality = 1f - (simplificationStrength / 100f);
            int totalMeshCount = objectMeshPairs.Count;
            int meshesHandled = 0;
            int threadsRunning = 0;
            bool isError = false;
#pragma warning disable
            string error = "";

            object threadLock1 = new object();
            object threadLock2 = new object();
            object threadLock3 = new object();


            if (runOnThreads)
            {

                List<CustomMeshActionStructure> meshAssignments = new List<CustomMeshActionStructure>();
                List<CustomMeshActionStructure> callbackFlusher = new List<CustomMeshActionStructure>();

                foreach (var kvp in objectMeshPairs)
                {

                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { meshesHandled++; continue; }

                    MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { meshesHandled++; continue; }

                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);



                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[simplificationOptions.preservationSpheres.Count];


                    if (!meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }

                    else if (meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }


                    meshSimplifier.Initialize(meshRendererPair.mesh, simplificationOptions.regardPreservationSpheres);


                    //while (threadsRunning == maxConcurrentThreads) { } // Don't create another thread if the max limit is reached wait for existing threads to clear

                    threadsRunning++;



                    while (callbackFlusher.Count > 0)
                    {
                        var meshInfo = callbackFlusher[0];

                        callbackFlusher.RemoveAt(0);

                        if (meshInfo == null) { continue; }

                        OnEachMeshSimplified?.Invoke(meshInfo.gameObject, meshInfo.meshRendererPair);
                    }



                    Task.Factory.StartNew(() =>
                    {

                        CustomMeshActionStructure structure = new CustomMeshActionStructure
                        (

                            meshRendererPair,

                            gameObject,

                            () =>
                            {
                                var reducedMesh = meshSimplifier.ToMesh();

                                AssignReducedMesh(gameObject, meshRendererPair.mesh, reducedMesh, meshRendererPair.attachedToMeshFilter, true);

                                if(meshSimplifier.RecalculateNormals)
                                {
                                    reducedMesh.RecalculateNormals();
                                    reducedMesh.RecalculateTangents();
                                }

                                totalTriangles += reducedMesh.triangles.Length / 3;
                            }

                        );


                        try
                        {
                            if (!simplificationOptions.simplifyMeshLossless)
                            {                    
                                meshSimplifier.SimplifyMesh(quality);
                            }

                            else
                            {
                                meshSimplifier.SimplifyMeshLossless();
                            }



                            // Create cannot be called from a background thread
                            lock (threadLock1)
                            {
                                meshAssignments.Add(structure);

                                threadsRunning--;
                                meshesHandled++;
                            }


                            lock (threadLock3)
                            {
                                CustomMeshActionStructure callbackFlush = new CustomMeshActionStructure
                                (
                                    meshRendererPair,
                                    gameObject,
                                    () => { }
                                );

                                callbackFlusher.Add(callbackFlush);
                            }

                        }
#pragma warning disable
                        catch (Exception ex)
                        {
                            lock (threadLock2)
                            {
                                threadsRunning--;
                                meshesHandled++;
                                isError = true;
                                error = ex.ToString();
                                //structure?.action();
                                //OnEachSimplificationError?.Invoke(error, structure?.gameObject, structure?.meshRendererPair);
                            }
                        }
                    }, CancellationToken.None, TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current);

                }



                //Wait for all threads to complete
                //Not reliable sometimes gets stuck
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();


                while (callbackFlusher.Count > 0)
                {
                    var meshInfo = callbackFlusher[0];

                    callbackFlusher.RemoveAt(0);

                    if (meshInfo == null) { continue; }

                    OnEachMeshSimplified?.Invoke(meshInfo.gameObject, meshInfo.meshRendererPair);
                }

                while (meshesHandled < totalMeshCount && !isError)
                {
                    while (callbackFlusher.Count > 0)
                    {
                        var meshInfo = callbackFlusher[0];

                        callbackFlusher.RemoveAt(0);

                        if (meshInfo == null) { continue; }

                        OnEachMeshSimplified?.Invoke(meshInfo.gameObject, meshInfo.meshRendererPair);
                    }
                }


                //watch.Stop();
                //Debug.Log("Elapsed Time   " + watch.ElapsedMilliseconds);

                if (!isError)
                {
                    foreach (CustomMeshActionStructure structure in meshAssignments)
                    {
                        structure?.action();
                    }
                }

                else
                {
                    //OnError?.Invoke(error);
                }

                //watch.Stop();
                //Debug.Log("Elapsed Time   " + watch.ElapsedMilliseconds );
                //Debug.Log("MESHESHANDLED  " + meshesHandled + "  Threads Allowed?  " + maxConcurrentThreads + "   Elapsed Time   "  +watch.Elapsed.TotalSeconds);

            }

            else
            {

                foreach (var kvp in objectMeshPairs)
                {

                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { continue; }

                    MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { continue; }


                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);


                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[simplificationOptions.preservationSpheres.Count];


                    if (!meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {

                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };
                            
                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }

                    else if (meshRendererPair.attachedToMeshFilter && simplificationOptions.regardPreservationSpheres)
                    {
                        int a = 0;

                        foreach (var sphere in simplificationOptions.preservationSpheres)
                        {
                            UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                            {
                                diameter = sphere.diameter,
                                localToWorldMatrix = gameObject.transform.localToWorldMatrix,
                                worldPosition = sphere.worldPosition,
                                targetObject = gameObject,
                                preservationStrength = sphere.preservationStrength
                            };

                            tSpheres[a] = toleranceSphere;
                            a++;
                        }

                        meshSimplifier.toleranceSpheres = tSpheres;
                    }


                    meshSimplifier.Initialize(meshRendererPair.mesh, simplificationOptions.regardPreservationSpheres);
                    

                    if (!simplificationOptions.simplifyMeshLossless)
                    {
                        meshSimplifier.SimplifyMesh(quality);
                    }

                    else
                    {
                        meshSimplifier.SimplifyMeshLossless();
                    }

                    OnEachMeshSimplified?.Invoke(gameObject, meshRendererPair);

                    var reducedMesh = meshSimplifier.ToMesh();
                    reducedMesh.bindposes = meshRendererPair.mesh.bindposes;   // Might cause issues
                    reducedMesh.name = meshRendererPair.mesh.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";


                    if (meshSimplifier.RecalculateNormals)
                    {
                        reducedMesh.RecalculateNormals();
                        reducedMesh.RecalculateTangents();
                    }

                    if (meshRendererPair.attachedToMeshFilter)
                    {
                        MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                        if (filter != null)
                        {
                            filter.sharedMesh = reducedMesh;
                        }
                    }

                    else
                    {
                        SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                        if (sRenderer != null)
                        {
                            sRenderer.sharedMesh = reducedMesh;
                        }
                    }

                    totalTriangles += reducedMesh.triangles.Length / 3;
                }

            }

            return totalTriangles;

        }



        /// <summary>
        /// Simplifies the meshes provided in the "meshesToSimplify" argument and returns the simplified meshes in a new list. Any errors are thrown as exceptions with relevant information.Please note that the returned list of simplified meshes doesn't guarantee the same order of meshes as supplied in the "meshesToSimplify" list. Please note that preservation spheres don't work with this method. 
        /// </summary>
        /// <param name="meshesToSimplify"> The list of meshes to simplify.</param>
        /// <param name="simplificationOptions"> Provide a SimplificationOptions object which contains different parameters and rules for simplifying the meshes. Please note that preservationSphere won't work for this method. </param>
        /// <param name="OnEachMeshSimplified"> This method will be called when a mesh is simplified. The method will be passed the original mesh that was simplified. </param>
        /// <returns> The list of simplified meshes. </returns>

        public static List<Mesh> SimplifyMeshes(List<Mesh> meshesToSimplify, SimplificationOptions simplificationOptions, Action<Mesh> OnEachMeshSimplified)
        {

            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            List<Mesh> simplifiedMeshes = new List<Mesh>();

            if (simplificationOptions == null)
            {
                throw new ArgumentNullException("simplificationOptions", "You must provide a SimplificationOptions object.");
            }

            int totalTriangles = 0;
            float simplificationStrength = simplificationOptions.simplificationStrength;


            if (meshesToSimplify == null)
            {
                throw new ArgumentNullException("meshesToSimplify", "You must provide a meshes list to simplify.");
            }

            if (meshesToSimplify.Count == 0)
            {
                throw new InvalidOperationException("You must provide a non-empty list of meshes to simplify.");
            }

            if (!simplificationOptions.simplifyMeshLossless)
            {
                if (!(simplificationStrength >= 0 && simplificationStrength <= 100))
                {
                    throw new ArgumentOutOfRangeException("simplificationStrength", "The allowed values for simplification strength are between [0-100] inclusive.");
                }

                if (Mathf.Approximately(simplificationStrength, 0)) { return null; }
            }


            bool runOnThreads = false;

            int trianglesCount = CountTriangles(meshesToSimplify);

            if (trianglesCount >= 2000 && meshesToSimplify.Count >= 2)
            {
                runOnThreads = true;
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                runOnThreads = false;
            }


            float quality = 1f - (simplificationStrength / 100f);
            int totalMeshCount = meshesToSimplify.Count;
            int meshesHandled = 0;
            int threadsRunning = 0;
            bool isError = false;
#pragma warning disable
            string error = "";

            object threadLock1 = new object();
            object threadLock2 = new object();
            object threadLock3 = new object();
            runOnThreads = true;

            if (runOnThreads)
            {


                List<CustomMeshActionStructure> meshAssignments = new List<CustomMeshActionStructure>();
                List<CustomMeshActionStructure> callbackFlusher = new List<CustomMeshActionStructure>();

                foreach (var meshToSimplify in meshesToSimplify)
                {

                    if (meshToSimplify == null) { meshesHandled++; continue; }


                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);

                    meshSimplifier.Initialize(meshToSimplify, false);


                    //while (threadsRunning == maxConcurrentThreads) { } // Don't create another thread if the max limit is reached wait for existing threads to clear

                    threadsRunning++;


                    while (callbackFlusher.Count > 0)
                    {
                        var meshInfo = callbackFlusher[0];
                        callbackFlusher.RemoveAt(0);

                        OnEachMeshSimplified?.Invoke(meshInfo.meshRendererPair.mesh);
                    }


                    Task.Factory.StartNew(() =>
                    {

                        CustomMeshActionStructure structure = new CustomMeshActionStructure
                        (

                            null,

                            null,

                            () =>
                            {
                                var reducedMesh = meshSimplifier.ToMesh();
                                reducedMesh.bindposes = meshToSimplify.bindposes;
                                reducedMesh.name = meshToSimplify.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";

                                if (meshSimplifier.RecalculateNormals)
                                {
                                    reducedMesh.RecalculateNormals();
                                    reducedMesh.RecalculateTangents();
                                }

                                simplifiedMeshes.Add(reducedMesh);
                            }

                        );


                        try
                        {
                            if (!simplificationOptions.simplifyMeshLossless)
                            {
                                meshSimplifier.SimplifyMesh(quality);
                            }

                            else
                            {
                                meshSimplifier.SimplifyMeshLossless();
                            }


                            // Create cannot be called from a background thread
                            lock (threadLock1)
                            {
                                meshAssignments.Add(structure);

                                threadsRunning--;
                                meshesHandled++;
                            }


                            lock (threadLock3)
                            {
                                MeshRendererPair mRendererPair = new MeshRendererPair(true, meshToSimplify);

                                CustomMeshActionStructure callbackFlush = new CustomMeshActionStructure
                                (
                                    mRendererPair,
                                    null,
                                    () => { }
                                );

                                callbackFlusher.Add(callbackFlush);
                            }

                        }
#pragma warning disable
                        catch (Exception ex)
                        {
                            lock (threadLock2)
                            {
                                threadsRunning--;
                                meshesHandled++;
                                isError = true;
                                error = ex.ToString();
                                //structure?.action();
                                //OnEachSimplificationError?.Invoke(error, structure?.gameObject, structure?.meshRendererPair);
                            }
                        }
                    }, CancellationToken.None, TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current);

                }



                //Wait for all threads to complete
                //Not reliable sometimes gets stuck
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();


                while (callbackFlusher.Count > 0)
                {
                    var meshInfo = callbackFlusher[0];
                    callbackFlusher.RemoveAt(0);

                    OnEachMeshSimplified?.Invoke(meshInfo.meshRendererPair.mesh);
                }


                while (meshesHandled < totalMeshCount && !isError)
                {
                    while (callbackFlusher.Count > 0)
                    {
                        var meshInfo = callbackFlusher[0];
                        callbackFlusher.RemoveAt(0);

                        OnEachMeshSimplified?.Invoke(meshInfo.meshRendererPair.mesh);
                    }
                }


                //watch.Stop();
                //Debug.Log("Elapsed Time   " + watch.ElapsedMilliseconds);

                if (!isError)
                {
                    foreach (CustomMeshActionStructure structure in meshAssignments)
                    {
                        structure?.action();
                    }
                }

                else
                {
                    //OnError?.Invoke(error);
                }

                //watch.Stop();
                //Debug.Log("Elapsed Time   " + watch.ElapsedMilliseconds );
                //Debug.Log("MESHESHANDLED  " + meshesHandled + "  Threads Allowed?  " + maxConcurrentThreads + "   Elapsed Time   "  +watch.Elapsed.TotalSeconds);

            }

            else
            {

                foreach (var meshToSimplify in meshesToSimplify)
                {

                    if (meshToSimplify == null) { continue; }


                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(simplificationOptions, meshSimplifier);

                    //meshSimplifier.VertexLinkDistance = meshSimplifier.VertexLinkDistance / 10f;
                    meshSimplifier.Initialize(meshToSimplify, false);


                    if (!simplificationOptions.simplifyMeshLossless)
                    {
                        meshSimplifier.SimplifyMesh(quality);
                    }

                    else
                    {
                        meshSimplifier.SimplifyMeshLossless();
                    }

                    OnEachMeshSimplified?.Invoke(meshToSimplify);

                    var reducedMesh = meshSimplifier.ToMesh();
                    reducedMesh.bindposes = meshToSimplify.bindposes;
                    reducedMesh.name = meshToSimplify.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";

                    if (meshSimplifier.RecalculateNormals)
                    {
                        reducedMesh.RecalculateNormals();
                        reducedMesh.RecalculateTangents();
                    }

                    simplifiedMeshes.Add(reducedMesh);

                }

            }

            return simplifiedMeshes;
        }



        /// <summary>
        /// This method returns a specialized DataStructure for the provided object. The key is a reference to a GameObject and the value is a MeshRendererPair which contains a reference to the mesh attached to the GameObject (key) and the type of mesh (Skinned or static).
        /// </summary>
        /// <param name="forObject"> The object for which the ObjectMeshPairs is constructed.</param>
        /// <param name="includeInactive"> If this is true then the method also considers the nested inactive children of the GameObject provided, otherwise it only considers the active nested children.</param>
        /// <returns> A specialized data structure that contains information about all the meshes nested under the provided GameObject. </returns>

        public static ObjectMeshPairs GetObjectMeshPairs(GameObject forObject, bool includeInactive)
        {

            if (forObject == null)
            {
                throw new ArgumentNullException("forObject", "You must provide a gameobject to get the ObjectMeshPairs for.");
            }

            ObjectMeshPairs objectMeshPairs = new ObjectMeshPairs();


            MeshFilter[] meshFilters = forObject.GetComponentsInChildren<MeshFilter>(includeInactive);


            if (meshFilters != null && meshFilters.Length != 0)
            {
                foreach (var filter in meshFilters)
                {
                    if (filter.sharedMesh)
                    {
                        //Debug.Log("Adding From Mesh Filter   "+ filter.sharedMesh.name + "  for gameobject  "+ filter.gameObject.name);
                        MeshRendererPair meshRendererPair = new MeshRendererPair(true, filter.sharedMesh);
                        objectMeshPairs.Add(filter.gameObject, meshRendererPair);
                    }
                }
            }


            SkinnedMeshRenderer[] sMeshRenderers = forObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive);

            if (sMeshRenderers != null && sMeshRenderers.Length != 0)
            {
                foreach (var renderer in sMeshRenderers)
                {
                    if (renderer.sharedMesh)
                    {
                        MeshRendererPair meshRendererPair = new MeshRendererPair(false, renderer.sharedMesh);
                        objectMeshPairs.Add(renderer.gameObject, meshRendererPair);
                    }
                }

            }


            return objectMeshPairs;

        }



        /// <summary>
        /// Tries to combine meshes nested under the provided GameObject. Please note that the method modifies the provided gameobject and it's children hierarchy.
        /// </summary>
        /// <param name="forObject"> The object under which all the Static and Skinned meshes will be merged.</param>
        /// <param name="skipInactiveRenderers"> Whether the child renderers of the provided objects be skipped if they are inactive.</param>
        /// <param name="OnError">The method to invoke when an error occurs. The method is passed the error title and the description of the error.</param>
        /// <param name="combineTarget">Indicates what kind of meshes to combine.</param>

        public static void CombineMeshesInGameObject(GameObject forObject, bool skipInactiveRenderers, Action<string, string> OnError, MeshCombineTarget combineTarget = MeshCombineTarget.SkinnedAndStatic)
        {

            #region Pre Checks


            if (forObject == null)
            {
                OnError?.Invoke("Argument Null Exception", "You must provide a gameobject whose meshes will be combined.");
                return;
            }


            // Collect all enabled renderers under the game object
            Renderer[] allRenderers = UtilityServicesRuntime.GetChildRenderersForCombining(forObject, skipInactiveRenderers);


            if (allRenderers == null || allRenderers.Length == 0)
            {
                OnError?.Invoke("Operation Failed", "No feasible renderers found under the provided object to combine.");
                return;
            }

            #endregion Pre Checks




            MeshRenderer[] originalMeshRenderers = null;
            HashSet<Transform> originalMeshRenderersTransforms = new HashSet<Transform>();
            SkinnedMeshRenderer[] originalSkinnedMeshRenderers = null;
            HashSet<Transform> originalSkinnedRenderersTransforms = new HashSet<Transform>();

            MeshCombiner.StaticRenderer[] staticRenderers = null;
            MeshCombiner.SkinnedRenderer[] skinnedRenderers = null;

            if (skipInactiveRenderers)
            {
                originalMeshRenderers =
                    (from renderer in allRenderers
                     where renderer.enabled && renderer.gameObject.activeInHierarchy && renderer as MeshRenderer != null
                     && renderer.transform.GetComponent<MeshFilter>() != null
                     && renderer.transform.GetComponent<MeshFilter>().sharedMesh != null
                     select renderer as MeshRenderer).ToArray();

                originalSkinnedMeshRenderers =
                    (from renderer in allRenderers
                     where renderer.enabled && renderer.gameObject.activeInHierarchy && renderer as SkinnedMeshRenderer != null
                     && renderer.transform.GetComponent<SkinnedMeshRenderer>().sharedMesh != null
                     select renderer as SkinnedMeshRenderer).ToArray();
            }
            else
            {
                originalMeshRenderers =
                    (from renderer in allRenderers
                     where renderer as MeshRenderer != null
                     && renderer.transform.GetComponent<MeshFilter>() != null
                     && renderer.transform.GetComponent<MeshFilter>().sharedMesh != null
                     select renderer as MeshRenderer).ToArray();

                originalSkinnedMeshRenderers =
                    (from renderer in allRenderers
                     where renderer as SkinnedMeshRenderer != null
                     && renderer.transform.GetComponent<SkinnedMeshRenderer>().sharedMesh != null
                     select renderer as SkinnedMeshRenderer).ToArray();
            }



            if (originalMeshRenderers != null)
            {
                foreach (var item in originalMeshRenderers) { originalMeshRenderersTransforms.Add(item.transform); }
            }
            if (originalSkinnedMeshRenderers != null)
            {
                foreach (var item in originalSkinnedMeshRenderers) { originalSkinnedRenderersTransforms.Add(item.transform); }
            }


            if (combineTarget == MeshCombineTarget.StaticOnly)
            {
                originalSkinnedMeshRenderers = new SkinnedMeshRenderer[0];
            }

            else if (combineTarget == MeshCombineTarget.SkinnedOnly)
            {
                originalMeshRenderers = new MeshRenderer[0];
            }



            staticRenderers = MeshCombiner.GetStaticRenderers(originalMeshRenderers);
            skinnedRenderers = MeshCombiner.GetSkinnedRenderers(originalSkinnedMeshRenderers);

            int totalSkinnedMeshesToCombine = (from renderer in originalSkinnedMeshRenderers
                                               where renderer.sharedMesh != null // && renderer.sharedMesh.blendShapeCount == 0 baw did
                                               select renderer).Count();

            int totalStaticMeshes = staticRenderers == null ? 0 : staticRenderers.Length;
            int totalSkinnedMeshes = skinnedRenderers == null ? 0 : skinnedRenderers.Length;

            if ((totalStaticMeshes == 0 || totalStaticMeshes == 1) && (totalSkinnedMeshesToCombine == 0 || totalSkinnedMeshesToCombine == 1))
            {
                string error = $"Nothing combined in GameObject \"{forObject.name}\". Not enough feasible renderers/meshes to combine.";

                if (combineTarget == MeshCombineTarget.StaticOnly)
                {
                    error = $"Nothing combined in GameObject \"{forObject.name}\". Not enough feasible static meshes to combine. Consider selecting the option of combining both skinned and static meshes.";
                }

                else if (combineTarget == MeshCombineTarget.SkinnedOnly)
                {
                    error = $"Nothing combined in GameObject \"{forObject.name}\". Not enough feasible skinned meshes to combine. Consider selecting the option of combining both skinned and static meshes.";
                }

                OnError?.Invoke("Operation Failed", error);
                return;
            }

            #region Combining Meshes


#if !UNITY_2017_3_OR_NEWER


            // Check if we are in older versions of Unity with max vertex limit <= 65534 

            if (originalMeshRenderers.Length != 0)
            {
                int totalVertsCount = 0;

                foreach (var renderer in originalMeshRenderers)
                {
                    if (renderer == null) { continue; }

                    MeshFilter mf = renderer.gameObject.GetComponent<MeshFilter>();

                    if (mf == null) { continue; }

                    Mesh m = mf.sharedMesh;

                    if (m == null) { continue; }

                    totalVertsCount += m.vertexCount;
                }

                // Don't combine meshes
                if (totalVertsCount > 65534)
                {
                    string error = $"No meshes under GameObject \"{forObject.name}\" will be combined. The combined mesh exceeds the maximum vertex count of 65534. This is a limitation in older versions of Unity (2017.2 and Below).";
                    OnError?.Invoke("Operation Failed", error);
                    return;
                }
            }


            var combineRenderers = (from renderer in originalSkinnedMeshRenderers
                                    where renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount == 0
                                    select renderer).ToArray();

            if (combineRenderers.Length != 0)
            {
                int totalVertsCount = 0;

                foreach (var renderer in combineRenderers)
                {
                    if (renderer == null) { continue; }

                    Mesh m = renderer.sharedMesh;

                    if (m == null) { continue; }

                    totalVertsCount += m.vertexCount;
                }

                // Don't combine meshes
                if (totalVertsCount > 65534)
                {
                    string error = $"No meshes under GameObject \"{forObject.name}\" will be combined. The combined mesh exceeds the maximum vertex count of 65534. This is a limitation in older versions of Unity (2017.2 and Below).";
                    OnError?.Invoke("Operation Failed", error);
                    return;
                }
            }

#endif


            SkinnedMeshRenderer[] skinnedRenderersActuallyCombined = null;

            var staticCombinedRenderers = MeshCombiner.CombineStaticMeshes(forObject.transform, -1, originalMeshRenderers, false);
            var skinnedCombinedRenderers = MeshCombiner.CombineSkinnedMeshes(forObject.transform, -1, originalSkinnedMeshRenderers, ref skinnedRenderersActuallyCombined, false);

            if (skinnedRenderersActuallyCombined != null)
            {
                foreach (var smr in skinnedRenderersActuallyCombined) { smr.enabled = false; }
            }
            if (originalMeshRenderers != null)
            {
                foreach (var mr in originalMeshRenderers) { mr.enabled = false; }
            }

            #endregion Combining meshes


            int totalCombinedStaticMeshes = staticCombinedRenderers == null ? 0 : staticCombinedRenderers.Length;
            int totalCombinedSkinnedMeshes = skinnedCombinedRenderers == null ? 0 : skinnedCombinedRenderers.Length;

            int totalMeshesToSave = totalCombinedStaticMeshes + totalCombinedSkinnedMeshes;
            int meshesHandled = 0;

            GameObject parentObject = forObject;
            Transform combinedTransform = parentObject.transform;

            HashSet<Transform> combinedStaticObjects = new HashSet<Transform>();
            HashSet<Transform> combinedSkinnedObjects = new HashSet<Transform>();



            for (int rendererIndex = 0; rendererIndex < totalCombinedStaticMeshes; rendererIndex++)
            {

                MeshCombiner.StaticRenderer staticCombinedRenderer = staticCombinedRenderers[rendererIndex];
                Mesh combinedMesh = staticCombinedRenderer.mesh;

                string rendererName = string.Format("{0}_combined_static", staticCombinedRenderer.name.Replace("_combined", ""));
                var levelRenderer = UtilityServicesRuntime.CreateStaticLevelRenderer(rendererName, combinedTransform, staticCombinedRenderer.transform, combinedMesh, staticCombinedRenderer.materials);

                combinedStaticObjects.Add(levelRenderer.transform);

                // Make this combined MeshRenderer object a direct child of the Main Object
                levelRenderer.transform.parent = forObject.transform;
            }


            for (int rendererIndex = 0; rendererIndex < totalCombinedSkinnedMeshes; rendererIndex++)
            {

                MeshCombiner.SkinnedRenderer skinnedCombinedRenderer = skinnedCombinedRenderers[rendererIndex];
                Mesh combinedMesh = skinnedCombinedRenderer.mesh;

                string rendererName = string.Format("{0}_combined_skinned", skinnedCombinedRenderer.name.Replace("_combined", ""));
                var levelRenderer = UtilityServicesRuntime.CreateSkinnedLevelRenderer(rendererName, combinedTransform, skinnedCombinedRenderer.transform, combinedMesh, skinnedCombinedRenderer.materials, skinnedCombinedRenderer.rootBone, skinnedCombinedRenderer.bones);

                combinedSkinnedObjects.Add(levelRenderer.transform);

                // Make this combined SkinnedMeshRenderer object a direct child of the Main Object
                levelRenderer.transform.parent = forObject.transform;
            }



            GameObject bonesHiererachyHolder = new GameObject(forObject.name + "_bonesHiererachy");
            bonesHiererachyHolder.transform.parent = forObject.transform;
            bonesHiererachyHolder.transform.localPosition = Vector3.zero;
            bonesHiererachyHolder.transform.localRotation = Quaternion.identity;
            bonesHiererachyHolder.transform.localScale = Vector3.one;

            Transform[] children = new Transform[forObject.transform.childCount];

            for (int a = 0; a < forObject.transform.childCount; a++) { children[a] = forObject.transform.GetChild(a); }

            for (int a = 0; a < children.Length; a++)
            {
                var child = children[a];

                if (combineTarget == MeshCombineTarget.SkinnedAndStatic)
                {
                    if (!combinedSkinnedObjects.Contains(child) && !combinedStaticObjects.Contains(child))
                    {
                        child.parent = bonesHiererachyHolder.transform;
                    }
                }

                else if (combineTarget == MeshCombineTarget.StaticOnly)
                {
                    if (!combinedStaticObjects.Contains(child) && !originalSkinnedRenderersTransforms.Contains(child))
                    {
                        child.parent = bonesHiererachyHolder.transform;
                    }
                }

                else
                {
                    if (!combinedSkinnedObjects.Contains(child) && !originalMeshRenderersTransforms.Contains(child))
                    {
                        child.parent = bonesHiererachyHolder.transform;
                    }
                }

            }

            // Set references to all combined meshes in the original object to null
            // So that if we run combine meshes operation again then those meshes are not 
            // combined again with the new combined meshes
            if (skinnedRenderersActuallyCombined != null)
            {
                foreach (var sRenderer in skinnedRenderersActuallyCombined)
                {
                    if (sRenderer == null) { continue; }

                    sRenderer.sharedMesh = null;
                }
            }

            if (originalMeshRenderers != null)
            {
                foreach (var meshRenderer in originalMeshRenderers)
                {
                    if (meshRenderer == null) { continue; }

                    var mFilter = meshRenderer.GetComponent<MeshFilter>();

                    if (mFilter == null) { continue; }

                    mFilter.sharedMesh = null;
                }
            }

        }



        /// <summary>
        /// Tries to combine the static and skinned meshes provided in the arguments.
        /// </summary>
        /// <param name="rootTransform">The root transform to create the combined meshes based from, essentially the origin of the new mesh.</param>
        /// <param name="originalMeshRenderers"> The list of MeshRenderer components whose corresponding meshes to combine. </param>
        /// <param name="OnError">The method to invoke when an error occurs. The method is passed the error title and the description of the error.</param>
        /// <param name="originalSkinnedMeshRenderers"> The list of SkinnedMeshRenderer components whose corresponding meshes to combine.</param>
        /// <returns> A new GameObject with the combined meshes, or returns null in case of any problem. </returns>

        public static GameObject CombineMeshesFromRenderers(Transform rootTransform, MeshRenderer[] originalMeshRenderers, SkinnedMeshRenderer[] originalSkinnedMeshRenderers, Action<string, string> OnError)
        {


            if (rootTransform == null)
            {
                OnError?.Invoke("Argument Null Exception", "You must provide a root transform to create the combined meshes based from.");
                return null;
            }


            if (originalMeshRenderers == null || originalMeshRenderers.Length == 0)
            {
                if (originalSkinnedMeshRenderers == null || originalSkinnedMeshRenderers.Length == 0)
                {
                    OnError?.Invoke("Operation Failed", "Both the Static and Skinned renderers list is empty. Atleast one of them must be non empty.");
                    return null;
                }
            }


            if (originalMeshRenderers == null) { originalMeshRenderers = new MeshRenderer[0]; }
            if (originalSkinnedMeshRenderers == null) { originalSkinnedMeshRenderers = new SkinnedMeshRenderer[0]; }


            originalMeshRenderers =
                (from renderer in originalMeshRenderers
                 where renderer.transform.GetComponent<MeshFilter>() != null
                 && renderer.transform.GetComponent<MeshFilter>().sharedMesh != null
                 select renderer as MeshRenderer).ToArray();

            originalSkinnedMeshRenderers =
                (from renderer in originalSkinnedMeshRenderers
                 where renderer.transform.GetComponent<SkinnedMeshRenderer>().sharedMesh != null
                 select renderer as SkinnedMeshRenderer).ToArray();



            if (originalMeshRenderers == null || originalMeshRenderers.Length == 0)
            {
                if (originalSkinnedMeshRenderers == null || originalSkinnedMeshRenderers.Length == 0)
                {
                    OnError?.Invoke("Operation Failed", "Couldn't combine any meshes. Couldn't find any feasible renderers in the provided lists to combine.");
                    return null;
                }
            }

            SkinnedMeshRenderer[] skinnedRenderersActuallyCombined = null;

            var staticCombinedRenderers = MeshCombiner.CombineStaticMeshes(rootTransform, -1, originalMeshRenderers, false);
            var skinnedCombinedRenderers = MeshCombiner.CombineSkinnedMeshes(rootTransform, -1, originalSkinnedMeshRenderers, ref skinnedRenderersActuallyCombined, false);


            if (staticCombinedRenderers == null || staticCombinedRenderers.Length == 0)
            {
                if (skinnedCombinedRenderers == null || skinnedCombinedRenderers.Length == 0)
                {
                    OnError?.Invoke("Operation Failed", "Couldn't combine any meshes due to unknown reasons.");
                    return null;
                }
            }

            var combinedGameObject = new GameObject(rootTransform.name + "_Combined_Meshes");
            var combinedTransform = combinedGameObject.transform;


            if (staticCombinedRenderers != null)
            {

                for (int rendererIndex = 0; rendererIndex < staticCombinedRenderers.Length; rendererIndex++)
                {
                    MeshCombiner.StaticRenderer staticCombinedRenderer = staticCombinedRenderers[rendererIndex];
                    Mesh combinedMesh = staticCombinedRenderer.mesh;

                    string rendererName = string.Format("{0}_combined_static", staticCombinedRenderer.name.Replace("_combined", ""));
                    var levelRenderer = UtilityServicesRuntime.CreateStaticLevelRenderer(rendererName, combinedTransform, staticCombinedRenderer.transform, combinedMesh, staticCombinedRenderer.materials);
                }
            }

            if (skinnedCombinedRenderers != null)
            {

                for (int rendererIndex = 0; rendererIndex < skinnedCombinedRenderers.Length; rendererIndex++)
                {
                    var skinnedCombinedRenderer = skinnedCombinedRenderers[rendererIndex];
                    Mesh combinedMesh = skinnedCombinedRenderer.mesh;

                    string rendererName = string.Format("{0}_combined_skinned", skinnedCombinedRenderer.name.Replace("_combined", ""));
                    var levelRenderer = UtilityServicesRuntime.CreateSkinnedLevelRenderer(rendererName, combinedTransform, skinnedCombinedRenderer.transform, combinedMesh, skinnedCombinedRenderer.materials, skinnedCombinedRenderer.rootBone, skinnedCombinedRenderer.bones);
                }

            }

            return combinedGameObject;
        }



        /// <summary>
        /// Converts all skinned meshes in the provided GameObject to non skinned/static meshes and also changes the corresponding renderer components. Please note that this method modifies the original GameObject and it's child hierarchy.
        /// </summary>
        /// <param name="forObject">The game object under which all SkinnedMeshRenderers will be converted.</param>
        /// <param name="skipInactiveRenderers"> Whether the child renderers of the provided objects be skipped if they are inactive.</param>
        /// <param name="OnError">The method to invoke when an error occurs. The method is passed the error title and the description of the error.</param>

        public static void ConvertSkinnedMeshesInGameObject(GameObject forObject, bool skipInactiveRenderers, Action<string, string> OnError)
        {


            #region Pre Checks

            if (forObject == null)
            {
                OnError?.Invoke("Argument Null Exception", "You must provide a gameobject whose meshes will be converted.");
                return;
            }


            SkinnedMeshRenderer[] renderersToConvert = null;

            renderersToConvert = forObject.GetComponentsInChildren<SkinnedMeshRenderer>(!skipInactiveRenderers);

            if (skipInactiveRenderers)
            {
                renderersToConvert =
                    (from renderer in renderersToConvert
                     where renderer.enabled && renderer.gameObject.activeInHierarchy && renderer.sharedMesh != null
                     select renderer).ToArray();
            }
            else
            {
                renderersToConvert =
                    (from renderer in renderersToConvert
                     where renderer.sharedMesh != null
                     select renderer).ToArray();
            }

            if (renderersToConvert == null || renderersToConvert.Length == 0)
            {
                OnError?.Invoke("Operation Failed", $"Failed to convert skinned meshes for the provided GameObject. No feasible skinned mesh renderer found in the GameObject or any of the nested children to convert.");
                return;
            }


            #endregion Pre Checks


            int count = 0;
            Mesh[] convertedMeshes = new Mesh[renderersToConvert.Length];
            List<GameObject> bonesToDelete = new List<GameObject>();

            foreach (var smr in renderersToConvert)
            {

                Mesh convertedMesh = new Mesh();
                convertedMesh.name = smr.sharedMesh.name + "-Skinned_Converted_Mesh";
                smr.BakeMesh(convertedMesh);

                Vector3[] vertices = convertedMesh.vertices;
                Vector3[] normals = convertedMesh.normals;

                float scaleX = smr.transform.lossyScale.x;
                float scaleY = smr.transform.lossyScale.y;
                float scaleZ = smr.transform.lossyScale.z;

                for (int a = 0; a < vertices.Length; a++)
                {
                    vertices[a] = new Vector3(vertices[a].x / scaleX, vertices[a].y / scaleY, vertices[a].z / scaleZ);
                    normals[a] = new Vector3(normals[a].x / scaleX, normals[a].y / scaleY, normals[a].z / scaleZ);
                }

                convertedMesh.vertices = vertices;
                convertedMesh.normals = normals;

                convertedMesh.RecalculateBounds();

                Material[] materials = smr.sharedMaterials;
                Transform rootBone = smr.rootBone;

                if (rootBone != null && rootBone.parent != null)
                {
                    if (rootBone.parent.gameObject.GetHashCode() != smr.gameObject.GetHashCode())
                    {
                        bonesToDelete.Add(rootBone.parent.gameObject);
                    }
                }


                GameObject gameObject = smr.gameObject;
                DestroyImmediate(smr);

                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = convertedMesh;
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = materials;

                convertedMeshes[count - 1] = convertedMesh;
            }


            foreach (var bone in bonesToDelete) { DestroyImmediate(bone); }

        }



        /// <summary>
        /// Converts all Skinned Mesh Renderers in the provided list to non simple Mesh Renderers and also changes the corresponding meshes. Pleaes note that this method doesn't modify the actualy GameObject(s) from which the provided SkinnedMeshRenderes are extracted.
        /// </summary>
        /// <param name="renderersToConvert">The list of SkinnedMeshRenderers to convert.</param>
        /// <param name="OnError">The method to invoke when an error occurs. The method is passed the error title and the description of the error.</param>
        /// <returns> An array of tuples that contain the original SkinnedMeshRenderer, the corresponding converted MeshRenderer and the Mesh. Returns null if the operation failed.</returns>

        public static Tuple<SkinnedMeshRenderer, MeshRenderer, Mesh>[] ConvertSkinnedMeshesFromRenderers(SkinnedMeshRenderer[] renderersToConvert, Action<string, string> OnError)
        {

            #region Pre Checks

            if (renderersToConvert == null)
            {
                OnError?.Invoke("Argument Null Exception", "You must provide a List of Skinned Mesh Renders to convert.");
                return null;
            }


            if (renderersToConvert.Length == 0)
            {
                OnError?.Invoke("Operation Failed", "The list of Skinned Mesh Renders to convert must not be empty.");
                return null;
            }


            renderersToConvert = (from renderer in renderersToConvert
                                  where renderer.sharedMesh != null
                                  select renderer).ToArray();


            if (renderersToConvert == null || renderersToConvert.Length == 0)
            {
                OnError?.Invoke("Operation Failed", $"Failed to convert skinned meshes. No feasible skinned mesh renderer found in the provided list to convert.");
                return null;
            }

            #endregion Pre Checks


            Tuple<SkinnedMeshRenderer, MeshRenderer, Mesh>[] converted = new Tuple<SkinnedMeshRenderer, MeshRenderer, Mesh>[renderersToConvert.Length];

            int a = 0;
            GameObject temp = new GameObject();

            foreach (var smr in renderersToConvert)
            {

                Mesh convertedMesh = new Mesh();
                convertedMesh.name = smr.sharedMesh.name + (smr.sharedMesh.name.EndsWith("-") ? "Skinned_Converted_Mesh" : "-Skinned_Converted_Mesh");
                smr.BakeMesh(convertedMesh);

                Vector3[] vertices = convertedMesh.vertices;
                Vector3[] normals = convertedMesh.normals;

                float scaleX = smr.transform.lossyScale.x;
                float scaleY = smr.transform.lossyScale.y;
                float scaleZ = smr.transform.lossyScale.z;

                for (int b = 0; b < vertices.Length; b++)
                {
                    vertices[b] = new Vector3(vertices[b].x / scaleX, vertices[b].y / scaleY, vertices[b].z / scaleZ);
                    normals[b] = new Vector3(normals[b].x / scaleX, normals[b].y / scaleY, normals[b].z / scaleZ);
                }

                convertedMesh.vertices = vertices;
                convertedMesh.normals = normals;

                convertedMesh.RecalculateBounds();

                MeshRenderer mr = temp.AddComponent<MeshRenderer>();
                mr.sharedMaterials = smr.sharedMaterials;

                converted[a] = Tuple.Create(smr, mr, convertedMesh);

                a++;
            }

            DestroyImmediate(temp);

            return converted;

        }




        /// <summary>
        /// Imports a wavefront obj file provided by the absolute path. Please note that this method doesn't work on WebGL builds and will safely return.
        /// </summary>
        /// <param name="objAbsolutePath"> The absolute path to the obj file.</param>
        /// <param name="texturesFolderPath"> The absolute path to the folder containing the texture files associated with the model to load. If you don't want to load the associated textures or there are none then you can pass an empty or null to this argument.</param>
        /// <param name="materialsFolderPath"> The absolute path to the folder containing the material files assoicated with the model to load.  If you don't want to load the associated material or there is none then you can pass an empty or null to this argument.</param>
        /// <param name="OnSuccess"> The callback method that will be invoked when the import was successful. The method is passed in the imported GameObject as the argument.</param>
        /// <param name="OnError"> The callback method that will be invoked when the import was not successful. The method is passed in an exception that made the task unsuccessful.</param>
        /// <param name="importOptions"> Specify additional import options for custom importing.</param>

        public static async void ImportOBJFromFileSystem(string objAbsolutePath, string texturesFolderPath, string materialsFolderPath, Action<GameObject> OnSuccess, Action<Exception> OnError, OBJImportOptions importOptions = null)
        {

            UtilityServicesRuntime.OBJExporterImporter importerExporter = new UtilityServicesRuntime.OBJExporterImporter();
            bool isWorking = true;

            try
            {
                await importerExporter.ImportFromLocalFileSystem(objAbsolutePath, texturesFolderPath, materialsFolderPath, (GameObject importedObject) =>
                {
                    isWorking = false;
                    OnSuccess(importedObject);
                }, importOptions);
            }

            catch(Exception ex)
            {
                isWorking = false;
                OnError(ex);
            }

            
            while(isWorking)
            {
                await Task.Delay(1);
            }
        }



        /// <summary>
        /// Downloads a wavefront obj file from the direct URl passed and imports it. You can also specify the URL for different textures associated with the model and also the URL to the linked material file. This function also works on WebGL builds.
        /// </summary>
        /// <param name="objURL"> The direct URL to the obj file.</param>
        /// <param name="objName"> The name for the GameObject that will represent the imported obj.</param>
        /// <param name="diffuseTexURL"> The absolute URL to the associated Diffuse texture (Main texture). If the model has no diffuse texture on the material then you can pass in null or empty string to this parameter.</param>
        /// <param name="bumpTexURL"> The absolute URL to the associated Bump texture (Bump map). If the model has no bump map then you can pass in null or empty string to this parameter.</param>
        /// <param name="specularTexURL">The absolute URL to the associated Specular texture (Reflection map). If the model has no reflection map then you can pass in null or empty string to this parameter.</param>
        /// <param name="opacityTexURL"> The absolute URL to the associated Opacity texture (Transparency map). If the model has no transparency map then you can pass in null or empty string to this parameter.</param>
        /// <param name="materialURL"> If the model has an associated material file (.mtl) then pass in the absolute URL to that otherwise pass a null or empty string.</param>
        /// <param name="downloadProgress"> The object of type ReferencedNumeric of type float that is updated with the download progress percentage.</param>
        /// <param name="OnSuccess"> The callback method that will be invoked when the import was successful. The method is passed in the imported GameObject as the argument..</param>
        /// <param name="OnError"> The callback method that will be invoked when the import was not successful. The method is passed in an exception that made the task unsuccessful.</param>
        /// <param name="importOptions"> Specify additional import options for custom importing.</param>

        public static async void ImportOBJFromNetwork(string objURL, string objName, string diffuseTexURL, string bumpTexURL, string specularTexURL, string opacityTexURL, string materialURL, ReferencedNumeric<float> downloadProgress, Action<GameObject> OnSuccess, Action<Exception> OnError, OBJImportOptions importOptions = null)
        {

            UtilityServicesRuntime.OBJExporterImporter importerExporter = new UtilityServicesRuntime.OBJExporterImporter();
            bool isWorking = true;

#if !UNITY_WEBGL

            importerExporter.ImportFromNetwork(objURL, objName, diffuseTexURL, bumpTexURL, specularTexURL, opacityTexURL, materialURL, downloadProgress, (GameObject importedObject) =>
            {
                isWorking = false;
                OnSuccess(importedObject);
            }, 
            (Exception ex) => 
            {
                isWorking = false;
                OnError(ex);

            } , importOptions);

            
            while(isWorking)
            {      
                await Task.Delay(1);
            }

#else

            importerExporter.ImportFromNetworkWebGL(objURL, objName, diffuseTexURL, bumpTexURL, specularTexURL, opacityTexURL, materialURL, downloadProgress, (GameObject importedObject) =>
            {
                isWorking = false;
                OnSuccess(importedObject);
            },
            (Exception ex) =>
            {
                isWorking = false;
                OnError(ex);

            }, importOptions);


            //while (isWorking)
            //{
                // Some how wait without using threads.
            //}
#endif
        }



        /// <summary>
        /// Exports the provided GameObject to wavefront OBJ format with support for saving textures and materials. Please note that the method won't work on WebGL builds and will safely return.
        /// </summary>
        /// <param name="toExport"> The GameObject that will be exported.</param>
        /// <param name="exportPath"> The path to the folder where the file will be written.</param>
        /// <param name="exportOptions"> Some additional export options for customizing the export. </param>
        /// <param name="OnSuccess">The callback to be invoked on successful export. </param>
        /// <param name="OnError"> The callback method that will be invoked when the import was not successful. The method is passed in an exception that made the task unsuccessful.</param>

        public static async void ExportGameObjectToOBJ(GameObject toExport, string exportPath, Action OnSuccess, Action<Exception> OnError, OBJExportOptions exportOptions = null)
        {
            UtilityServicesRuntime.OBJExporterImporter importerExporter = new UtilityServicesRuntime.OBJExporterImporter();
            bool isWorking = true;

            try
            {
                importerExporter.ExportGameObjectToOBJ(toExport, exportPath, exportOptions, ()=> 
                {
                    isWorking = false;
                    OnSuccess();
                });
            }

            catch (Exception ex)
            {
                isWorking = false;
                OnError(ex);
            }


            while(isWorking)
            {
                await Task.Delay(1);
            }
        }



        /// <summary>
        /// Counts the number of triangles in the provided GameObject. If "countDeep" is true then the method counts all the triangles considering all the nested meshes in the children hierarchies of the given GameObject.
        /// </summary>
        /// <param name="countDeep"> If true the method also counts and considers the triangles of the nested children hierarchies for the given GameObject. </param>
        /// <param name="forObject"> The GameObject for which to count the triangles.</param>
        /// <returns> The total traingles summing the triangles count of all the meshes nested under the provided GameObject.</returns>

        public static int CountTriangles(bool countDeep, GameObject forObject)
        {
            int triangleCount = 0;

            if (forObject == null) { return 0; }


            if (countDeep)
            {
                MeshFilter[] meshFilters = forObject.GetComponentsInChildren<MeshFilter>(true);


                if (meshFilters != null && meshFilters.Length != 0)
                {
                    foreach (var filter in meshFilters)
                    {
                        if (filter.sharedMesh)
                        {
                            triangleCount += (filter.sharedMesh.triangles.Length) / 3;
                        }
                    }
                }


                SkinnedMeshRenderer[] sMeshRenderers = forObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

                if (sMeshRenderers != null && sMeshRenderers.Length != 0)
                {
                    foreach (var renderer in sMeshRenderers)
                    {
                        if (renderer.sharedMesh)
                        {
                            triangleCount += (renderer.sharedMesh.triangles.Length) / 3;
                        }
                    }
                }
            }

            else
            {
                MeshFilter mFilter = forObject.GetComponent<MeshFilter>();
                SkinnedMeshRenderer sRenderer = forObject.GetComponent<SkinnedMeshRenderer>();

                if (mFilter && mFilter.sharedMesh)
                {
                    triangleCount = (mFilter.sharedMesh.triangles.Length) / 3;
                }

                else if (sRenderer && sRenderer.sharedMesh)
                {
                    triangleCount = (sRenderer.sharedMesh.triangles.Length) / 3;
                }
            }


            return triangleCount;
        }



        /// <summary>
        /// Counts the number of triangles in the provided meshes list.
        /// </summary>
        /// <param name="toCount"> The list of meshes whose triangles will be counted. </param>
        /// <returns> The total triangles summing the triangles count of all the meshes in the provided list. WIll return 0 if there are no meshes in the list</returns>

        public static int CountTriangles(List<Mesh> toCount)
        {
            int triangleCount = 0;

            if (toCount == null || toCount.Count == 0) { return 0; }

            foreach (var mesh in toCount)
            {
                if (mesh != null)
                {
                    triangleCount += (mesh.triangles.Length) / 3;
                }
            }

            return triangleCount;
        }



        /// <summary>
        /// Get a list of MaterialProperties objects associated with this GameObject. These objects can be used to change the properties of individual materials on this GameObject that were combined with Batch Few
        /// </summary>
        /// <param name="forObject"> The GameObject whose material properties objects to fetch</param>
        /// <returns>A list of MaterialProperties objects associated with this GameObject</returns>
         
        public static List<MaterialProperties> GetMaterialsProperties(GameObject forObject)
        {

            #region Pre Checks

            if (forObject == null)
            {
                throw new ArgumentNullException("Argument Null Exception", "You must provide a GameObject whose material properties you want to change");
            }

            var matLinks = forObject.GetComponent<PolyFew.ObjectMaterialLinks>();

            if (matLinks == null)
            {
                throw new InvalidOperationException("The object whose material properties you're trying to combine doesn't have any materials combined with Batch Few");
            }

            var attrImg = matLinks.linkedAttrImg;

            if (matLinks == null)
            {
                throw new InvalidOperationException("There is no attributes image associated with the given object");
            }

            #endregion


            var originalMaterialsProperties = matLinks.materialsProperties;
            List<MaterialProperties> toReturn = new List<MaterialProperties>();

            foreach (var origProps in originalMaterialsProperties)
            {
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

                toReturn.Add(matProps);
            }

            return toReturn;

        }



        /// <summary>
        /// Change the properties of a merged material associated with this GameObject
        /// </summary>
        /// <param name="changeTo"> The new material properties</param>
        /// <param name="forObject"> The GameObject whose merged material to change properties for</param>
        
        public static void ChangeMaterialProperties(MaterialProperties changeTo, GameObject forObject)
        {
            if (forObject == null) { return; }
            if (changeTo == null) { return; }

            var attrImage = forObject.GetComponent<PolyFew.ObjectMaterialLinks>().linkedAttrImg;

            if (attrImage == null) { return; }

            int texArrIndex = changeTo.texArrIndex;
            int matIndex = changeTo.matIndex;

            changeTo.BurnAttrToImg(ref attrImage, matIndex, texArrIndex);
        }



        #endregion PUBLIC_METHODS




        #region PRIVATE_METHODS


        private static void SetParametersForSimplifier(SimplificationOptions simplificationOptions, UnityMeshSimplifier.MeshSimplifier meshSimplifier)
        {
            meshSimplifier.RecalculateNormals = simplificationOptions.recalculateNormals;
            meshSimplifier.EnableSmartLink = simplificationOptions.enableSmartlinking;
            meshSimplifier.PreserveUVSeamEdges = simplificationOptions.preserveUVSeamEdges;
            meshSimplifier.PreserveUVFoldoverEdges = simplificationOptions.preserveUVFoldoverEdges;
            meshSimplifier.PreserveBorderEdges = simplificationOptions.preserveBorderEdges;
            meshSimplifier.MaxIterationCount = simplificationOptions.maxIterations;
            meshSimplifier.Aggressiveness = simplificationOptions.aggressiveness;
            meshSimplifier.RegardCurvature = simplificationOptions.regardCurvature;
            meshSimplifier.UseSortedEdgeMethod = simplificationOptions.useEdgeSort;
        }


        private static bool AreAnyFeasibleMeshes(ObjectMeshPairs objectMeshPairs)
        {

            if (objectMeshPairs == null || objectMeshPairs.Count == 0) { return false; }


            foreach (KeyValuePair<GameObject, MeshRendererPair> item in objectMeshPairs)
            {

                MeshRendererPair meshRendererPair = item.Value;
                GameObject gameObject = item.Key;

                if (gameObject == null || meshRendererPair == null) { continue; }

                if (meshRendererPair.attachedToMeshFilter)
                {
                    MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                    if (filter == null || meshRendererPair.mesh == null) { continue; }

                    return true;
                }

                else if (!meshRendererPair.attachedToMeshFilter)
                {
                    SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                    if (sRenderer == null || meshRendererPair.mesh == null) { continue; }

                    return true;
                }
            }

            return false;
        }


        private static void AssignReducedMesh(GameObject gameObject, Mesh originalMesh, Mesh reducedMesh, bool attachedToMeshfilter, bool assignBindposes)
        {
            if (assignBindposes)
            {
                reducedMesh.bindposes = originalMesh.bindposes;
            }

            reducedMesh.name = originalMesh.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";

            if (attachedToMeshfilter)
            {
                MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                if (filter != null)
                {
                    filter.sharedMesh = reducedMesh;
                }
            }

            else
            {
                SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                if (sRenderer != null)
                {
                    sRenderer.sharedMesh = reducedMesh;
                }
            }
        }


        private static int CountTriangles(ObjectMeshPairs objectMeshPairs)
        {
            int triangleCount = 0;

            if (objectMeshPairs == null) { return 0; }

            foreach (var item in objectMeshPairs)
            {
                if (item.Key == null || item.Value == null || item.Value.mesh == null)
                { continue; }

                triangleCount += (item.Value.mesh.triangles.Length) / 3;
            }

            return triangleCount;
        }


#endregion PRIVATE_METHODS


    }

}




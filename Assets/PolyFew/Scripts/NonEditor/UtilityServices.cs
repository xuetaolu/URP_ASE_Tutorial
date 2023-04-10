//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////


#if UNITY_EDITOR


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using static BrainFailProductions.PolyFew.DataContainer;



namespace BrainFailProductions.PolyFew
{


    public class UtilityServices : EditorWindow
    {

        public static GameObject dummyStatic;
        public static GameObject dummySkinned;
        private static Material dummyStaticMat;
        private static Material dummySkinnedMat;

        public const int MAX_LOD_COUNT = 8;

#pragma warning disable
        public static int maxConcurrentThreads;
        //GetProcessorCount is not allowed to be called from a ScriptableObject constructor (or instance field initializer), call it in OnEnable instead.

        public const string LOD_PARENT_OBJECT_NAME = "(POLY FEW)_LODS-DON'T-DELETE-MANUALLY";
        public const string LOD_ASSETS_DEFAULT_SAVE_PATH = "Assets/POLYFEW_LODs";   
        public const string BATCHFEW_ASSETS_DEFAULT_SAVE_PATH = "Assets/BATCHFEW_COMBINED_ASSETS";   
        public static DataContainer dataContainer;
        public static string AutoLODSavePath { get { return dataContainer.autoLODSavePath; } set { dataContainer.autoLODSavePath = value; } }
        public static string BatchFewSavePath { get { return dataContainer.batchFewSavePath; } set { dataContainer.batchFewSavePath = value; } }

        public enum HandleOrientation
        {
            localAligned,
            globalAligned
        }


        public enum MeshCombineTarget
        {
            SkinnedAndStatic,
            StaticOnly,
            SkinnedOnly
        }


        [System.Serializable]
        public struct ChildStateTuple
        {
            public Transform transform;
            public Vector3 position;
            public Quaternion rotation;

            public ChildStateTuple(Transform transform, Vector3 position, Quaternion rotation)
            {
                this.transform = transform;
                this.position = position;
                this.rotation = rotation;
            }
        }


        [System.Serializable]
        public struct ColliderState
        {
            public ColliderType type;
            public Vector3 center;
            public Quaternion rotation;
        }



        public static int SimplifyObjectDeep(ObjectMeshPair objectMeshPairs, List<ToleranceSphere> toleranceSpheres, bool runOnThreads, bool isToleranceActive, float quality, Action<string> OnError, Action<string, GameObject, MeshRendererPair> OnEachSimplificationError = null, Action<GameObject, MeshRendererPair> OnEachMeshSimplified = null)
        {
            // PRESERVATION SPHERE CAUSING SLOW BEHAVIOUR ON REDUCTION
            int totalMeshCount = objectMeshPairs.Count;
            int meshesHandled = 0;
            int threadsRunning = 0;
            bool isError = false;
            string error = "";
            int triangleCount = 0;

            object threadLock1 = new object();
            object threadLock2 = new object();

            //if(applyForReduceDeep)
            //Debug.Log("reduce deep was checked so executing like slider changed val");

            if (runOnThreads)
            {
                List<CustomMeshActionStructure> meshAssignments = new List<CustomMeshActionStructure>();
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                //watch.Start();


                foreach (var kvp in objectMeshPairs)
                {

                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { meshesHandled++; continue; }

                    DataContainer.MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { meshesHandled++; continue; }

                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(meshSimplifier);



                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[toleranceSpheres.Count];

                    if (!meshRendererPair.attachedToMeshFilter && isToleranceActive)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in toleranceSpheres)
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

                    else if(meshRendererPair.attachedToMeshFilter && isToleranceActive)
                    {
                        int a = 0;

                        foreach (var sphere in toleranceSpheres)
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


                    meshSimplifier.Initialize(meshRendererPair.mesh, isToleranceActive);


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
                                 
                                AssignReducedMesh(gameObject, meshRendererPair.mesh, reducedMesh, meshRendererPair.attachedToMeshFilter, true);

                                if(meshSimplifier.RecalculateNormals)
                                {
                                    reducedMesh.RecalculateNormals();
                                    reducedMesh.RecalculateTangents();
                                }

                                triangleCount += reducedMesh.triangles.Length / 3;
                            }


                        );


                        try
                        {

                            if (isToleranceActive)
                            {
                                meshSimplifier.SimplifyMesh(quality);
                            }

                            else
                            {
                                meshSimplifier.SimplifyMesh(quality);
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
                        }

                        catch (Exception ex)
                        {
                            lock (threadLock2)
                            {
                                threadsRunning--;
                                meshesHandled++;
                                isError = true;
                                error = ex.ToString();
                                //structure?.action();
                                OnEachSimplificationError?.Invoke(error, structure?.gameObject, structure?.meshRendererPair);
                            }
                        }

                    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current);


                }

                //Wait for all threads to complete
                //Not reliable sometimes gets stuck
                //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

                while (meshesHandled < totalMeshCount && !isError) { }


                if (!isError)
                {
                    foreach (CustomMeshActionStructure structure in meshAssignments)
                    {
                        structure?.action();
                        OnEachMeshSimplified?.Invoke(structure?.gameObject, structure?.meshRendererPair);
                    }
                }

                else
                {
                    OnError?.Invoke(error);
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

                    DataContainer.MeshRendererPair meshRendererPair = kvp.Value;

                    if (meshRendererPair.mesh == null) { continue; }


                    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

                    SetParametersForSimplifier(meshSimplifier);

                    UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[toleranceSpheres.Count];


                    if (!meshRendererPair.attachedToMeshFilter && isToleranceActive)
                    {
                        meshSimplifier.isSkinned = true;
                        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
                        meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                        meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                        meshSimplifier.bonesOriginal = smr.bones;
                        int a = 0;

                        foreach (var sphere in toleranceSpheres)
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

                    else if (meshRendererPair.attachedToMeshFilter && isToleranceActive)
                    {
                        int a = 0;

                        foreach (var sphere in toleranceSpheres)
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

                    //meshSimplifier.VertexLinkDistance = meshSimplifier.VertexLinkDistance / 10f;
                    meshSimplifier.Initialize(meshRendererPair.mesh, isToleranceActive);

                    meshSimplifier.SimplifyMesh(quality);

                    var reducedMesh = meshSimplifier.ToMesh();

                    AssignReducedMesh(gameObject, meshRendererPair.mesh, reducedMesh, meshRendererPair.attachedToMeshFilter, true);

                    if (meshSimplifier.RecalculateNormals)
                    {
                        reducedMesh.RecalculateNormals();
                        reducedMesh.RecalculateTangents();
                    }
     
                    triangleCount += reducedMesh.triangles.Length / 3;
                }
            }

            return triangleCount;

        }




        public static int SimplifyObjectShallow(MeshRendererPair meshRendererPair, List<ToleranceSphere> toleranceSpheres, GameObject selectedObject, bool isPreservationActive, float quality)
        {

            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();

            UtilityServices.SetParametersForSimplifier(meshSimplifier);

            UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[toleranceSpheres.Count];


            if (!meshRendererPair.attachedToMeshFilter && isPreservationActive)
            {
                meshSimplifier.isSkinned = true;
                var smr = selectedObject.GetComponent<SkinnedMeshRenderer>();
                meshSimplifier.boneWeightsOriginal = meshRendererPair.mesh.boneWeights;
                meshSimplifier.bindPosesOriginal = meshRendererPair.mesh.bindposes;
                meshSimplifier.bonesOriginal = smr.bones;

                int a = 0;

                foreach (var sphere in toleranceSpheres)
                {

                    UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                    {
                        diameter = sphere.diameter,
                        localToWorldMatrix = selectedObject.transform.localToWorldMatrix,
                        worldPosition = sphere.worldPosition,
                        targetObject = selectedObject,
                        preservationStrength = sphere.preservationStrength
                    };

                    tSpheres[a] = toleranceSphere;

                    a++;
                }

                meshSimplifier.toleranceSpheres = tSpheres;
            }

            else if (meshRendererPair.attachedToMeshFilter && isPreservationActive)
            {
                int a = 0;

                foreach (var sphere in toleranceSpheres)
                {

                    UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                    {
                        diameter = sphere.diameter,
                        localToWorldMatrix = selectedObject.transform.localToWorldMatrix,
                        worldPosition = sphere.worldPosition,
                        targetObject = selectedObject,
                        preservationStrength = sphere.preservationStrength
                    };

                    tSpheres[a] = toleranceSphere;
                    a++;

                }

                meshSimplifier.toleranceSpheres = tSpheres; 

            }


            meshSimplifier.Initialize(meshRendererPair.mesh, isPreservationActive);



            if (isPreservationActive)
            {
                meshSimplifier.SimplifyMesh(quality);
            }

            else
            {
                meshSimplifier.SimplifyMesh(quality);
            }

            var reducedMesh = meshSimplifier.ToMesh();

            reducedMesh.bindposes = meshRendererPair.mesh.bindposes;   

            reducedMesh.name = meshRendererPair.mesh.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";
            reducedMesh.name = reducedMesh.name.Replace("-BLENDSHAPES_SIMPLIFIED", "");

            if (meshSimplifier.RecalculateNormals)
            {
                reducedMesh.RecalculateNormals();
                reducedMesh.RecalculateTangents();
            }


            if (meshRendererPair.attachedToMeshFilter)
            {
                MeshFilter filter = selectedObject.GetComponent<MeshFilter>();

                if (filter != null)
                {
                    filter.sharedMesh = reducedMesh;
                }
            }

            else
            {
                SkinnedMeshRenderer sRenderer = selectedObject.GetComponent<SkinnedMeshRenderer>();

                if (sRenderer != null)
                {
                    sRenderer.sharedMesh = reducedMesh;
                }
            }

            return reducedMesh.triangles.Length / 3;

        }



        // Returns Tuple<StaticReducedMesh[], SkinnedReducedMesh[], The associated MeshFilters, The assoicated SkinnedMeshRenderer >[] 
        //-- Each Tuple corresponds to a LOD level in order
        public static Tuple<Mesh[], Mesh[], MeshFilter[], SkinnedMeshRenderer[]>[] GetReducedMeshes(ref LODGenerator.StaticRenderer[] staticRenderers, ref LODGenerator.SkinnedRenderer[] skinnedRenderers, GameObject forObject, LODLevelSettings[] lodSettings, List<ToleranceSphere> toleranceSpheres)
        {

            if ((staticRenderers == null || staticRenderers.Length == 0) && (skinnedRenderers == null || skinnedRenderers.Length == 0)) { return null; }

            if (lodSettings == null || lodSettings.Length == 0) { return null; }



            int totalStaticMeshes = staticRenderers == null ? 0 : staticRenderers.Length;
            int totalSkinnedMeshes = skinnedRenderers == null ? 0 : skinnedRenderers.Length;

            int lodLevelsToReduce = 0;



            foreach (var settings in lodSettings)
            {
                if (!Mathf.Approximately(settings.reductionStrength, 0))
                {
                    lodLevelsToReduce++;
                }
            }


            // The total meshes to reduce doesn't include the meshes for which the LOD level settings say quality = 1 (means don't reduce).

            int totalMeshesToReduce = lodLevelsToReduce * (totalStaticMeshes + totalSkinnedMeshes);
            int meshesHandled = 0;
            int threadsRunning = 0;

#pragma warning disable

            string error = "";

            object threadLock1 = new object();
            object threadLock2 = new object();

            //if(applyForReduceDeep)
            //Debug.Log("reduce deep was checked so executing like slider changed val");


            Tuple<Mesh[], Mesh[], MeshFilter[], SkinnedMeshRenderer[]>[] allLevelsReducedMeshes;
            allLevelsReducedMeshes = new Tuple<Mesh[], Mesh[], MeshFilter[], SkinnedMeshRenderer[]>[lodSettings.Length];

            List<Action> meshActions = new List<Action>(totalMeshesToReduce);


            for (int a = 0; a < lodSettings.Length; a++)
            {
                Tuple<Mesh[], Mesh[], MeshFilter[], SkinnedMeshRenderer[]> lodLevelMeshes;

                lodLevelMeshes = Tuple.Create(new Mesh[totalStaticMeshes],
                                              new Mesh[totalSkinnedMeshes],
                                              new MeshFilter[totalStaticMeshes],
                                              new SkinnedMeshRenderer[totalSkinnedMeshes]
                                              );
                allLevelsReducedMeshes[a] = lodLevelMeshes;
            }



            for (int a = 0; a < lodSettings.Length; a++)
            {

                var lodLevelSettings = lodSettings[a];


                Tuple<Mesh[], Mesh[], MeshFilter[], SkinnedMeshRenderer[]> lodLevelMeshes;
                lodLevelMeshes = allLevelsReducedMeshes[a];


                if (staticRenderers != null && staticRenderers.Length > 0)
                {

                    Mesh[] reducedStaticMeshes = lodLevelMeshes.Item1;
                    MeshFilter[] associatedMeshFilters = lodLevelMeshes.Item3;

                    for (int b = 0; b < staticRenderers.Length; b++)
                    {

                        var renderer = staticRenderers[b];
                        var meshToReduce = renderer.mesh;
                        float quality = (1f - (lodLevelSettings.reductionStrength / 100f));

                        // Simplify the mesh if necessary
                        if (!Mathf.Approximately(quality, 1))
                        {

                            MeshSimplifier meshSimplifier = new MeshSimplifier();
                            SetParametersForSimplifier(meshSimplifier, lodLevelSettings);



                            //while (threadsRunning == maxConcurrentThreads) { } // Don't create another thread if the max limit is reached wait for existing threads to clear

                            threadsRunning++;

                            if(lodLevelSettings.regardTolerance)
                            {

                                UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[toleranceSpheres.Count];
                                int index = 0;

                                foreach (var sphere in toleranceSpheres)
                                {

                                    UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                                    {
                                        diameter = sphere.diameter,
                                        localToWorldMatrix = renderer.transform.localToWorldMatrix,
                                        worldPosition = sphere.worldPosition,
                                        targetObject = renderer.transform.gameObject,
                                        preservationStrength = lodLevelSettings.sphereIntensities[index]
                                    };

                                    tSpheres[index] = toleranceSphere;

                                    index++;
                                }

                                meshSimplifier.toleranceSpheres = tSpheres;
                            }

                            bool isToleranceActive = false;

                            if (toleranceSpheres == null || toleranceSpheres.Count == 0)
                            {
                                isToleranceActive = false;
                            }
                            else if (lodLevelSettings.regardTolerance)
                            {
                                isToleranceActive = true;
                            }

                            meshSimplifier.Initialize(meshToReduce, isToleranceActive);



                            MeshFilter assoicatedMFilter = renderer.transform.GetComponent<MeshFilter>();
#pragma warning disable

                            int meshToReduceIndex = b;
                            int levelSettings = a;


                            Task.Factory.StartNew(() =>
                            {

                                try
                                {

                                    meshSimplifier.SimplifyMesh(quality);
          
                                    // Create cannot be called from a background thread
                                    lock (threadLock1)
                                    {

                                        meshActions.Add(() =>
                                        {
                                            Mesh reducedMesh = meshSimplifier.ToMesh();
                                            reducedMesh.bindposes = meshToReduce.bindposes;
                                            
                                            reducedMesh.name = meshToReduce.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";
                                            reducedMesh.name = reducedMesh.name.Replace("-BLENDSHAPES_SIMPLIFIED", "");

                                            if (meshSimplifier.RecalculateNormals)
                                            {
                                                reducedMesh.RecalculateNormals();
                                                reducedMesh.RecalculateTangents();
                                            }

                                            reducedStaticMeshes[meshToReduceIndex] = reducedMesh;
                                            associatedMeshFilters[meshToReduceIndex] = assoicatedMFilter;
                                        });

                                        threadsRunning--;
                                        meshesHandled++;
                                    }

                                }

                                catch (Exception ex)
                                {
                                    lock (threadLock2)
                                    {
                                        threadsRunning--;
                                        meshesHandled++;
                                        error = ex.ToString();
                                    }
                                }

                            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current);


                        }

                    }
                }


                if (skinnedRenderers != null && skinnedRenderers.Length > 0)
                {

                    Mesh[] reducedSkinnedMeshes = lodLevelMeshes.Item2;
                    SkinnedMeshRenderer[] associatedSkinRenderers = lodLevelMeshes.Item4;

                    for (int b = 0; b < skinnedRenderers.Length; b++)
                    {

                        var renderer = skinnedRenderers[b];
                        var meshToReduce = renderer.mesh;
                        float quality = (1f - (lodLevelSettings.reductionStrength / 100f));

                        // Simplify the mesh if necessary
                        if (!Mathf.Approximately(quality, 1))
                        {

                            MeshSimplifier meshSimplifier = new MeshSimplifier();
                            SetParametersForSimplifier(meshSimplifier, lodLevelSettings);
                            bool initializedAlready = false;
                            UnityMeshSimplifier.ToleranceSphere[] tSpheres = new UnityMeshSimplifier.ToleranceSphere[toleranceSpheres.Count];


                            if (lodLevelSettings.regardTolerance)
                            {
                                meshSimplifier.isSkinned = true;

                                meshSimplifier.isSkinned = true;
                                var smr = renderer.transform.GetComponent<SkinnedMeshRenderer>();
                                meshSimplifier.boneWeightsOriginal = meshToReduce.boneWeights;
                                meshSimplifier.bindPosesOriginal = meshToReduce.bindposes;
                                meshSimplifier.bonesOriginal = smr.bones;
                                initializedAlready = true;
                                int index = 0;
                                
                                foreach (var sphere in toleranceSpheres)
                                {

                                    UnityMeshSimplifier.ToleranceSphere toleranceSphere = new UnityMeshSimplifier.ToleranceSphere()
                                    {
                                        diameter = sphere.diameter,
                                        localToWorldMatrix = renderer.transform.localToWorldMatrix,
                                        worldPosition = sphere.worldPosition,
                                        targetObject = renderer.transform.gameObject,
                                        preservationStrength = lodLevelSettings.sphereIntensities[index]
                                    };

                                    tSpheres[index] = toleranceSphere;
                                    index++;
                                }

                                meshSimplifier.toleranceSpheres = tSpheres;
                            }


                            bool isToleranceActive = false;

                            if (toleranceSpheres == null || toleranceSpheres.Count == 0)
                            {
                                isToleranceActive = false;
                            }
                            else if (lodLevelSettings.regardTolerance)
                            {
                                isToleranceActive = true;
                            }


                            meshSimplifier.Initialize(meshToReduce, isToleranceActive);


                            //while (threadsRunning == maxConcurrentThreads) { } // Don't create another thread if the max limit is reached wait for existing threads to clear

                            threadsRunning++;


                            SkinnedMeshRenderer assoicatedSRenderer = renderer.transform.GetComponent<SkinnedMeshRenderer>();


                            int meshToReduceIndex = b;


                            Task.Factory.StartNew(() =>
                            {

                                try
                                {

                                    meshSimplifier.SimplifyMesh(quality);

                                    // Create cannot be called from a background thread
                                    lock (threadLock1)
                                    {

                                        meshActions.Add(() =>
                                        {
                                            Mesh reducedMesh = meshSimplifier.ToMesh();
                                            reducedMesh.bindposes = meshToReduce.bindposes;

                                            reducedMesh.name = meshToReduce.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";
                                            reducedMesh.name = reducedMesh.name.Replace("-BLENDSHAPES_SIMPLIFIED", "");

                                            if (meshSimplifier.RecalculateNormals)
                                            {
                                                reducedMesh.RecalculateNormals();
                                                reducedMesh.RecalculateTangents();
                                            }

                                            reducedSkinnedMeshes[meshToReduceIndex] = reducedMesh;
                                            associatedSkinRenderers[meshToReduceIndex] = assoicatedSRenderer;
                                            
                                        });

                                        threadsRunning--;
                                        meshesHandled++;
                                    }
                                }

                                catch (Exception ex)
                                {
                                    lock (threadLock2)
                                    {
                                        threadsRunning--;
                                        meshesHandled++;
                                        error = ex.ToString();
                                    }
                                }

                            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current);


                        }

                    }

                }


            }




            //Wait for all threads to complete
            //Not reliable sometimes gets stuck

            while (meshesHandled < totalMeshesToReduce)
            {
                EditorUtility.DisplayProgressBar("Generating LODs", $"Reducing Meshes {meshesHandled + 1}/{totalMeshesToReduce}", (float)meshesHandled / totalMeshesToReduce);
            }

            EditorUtility.ClearProgressBar();


            foreach (Action meshAction in meshActions)
            {
                meshAction?.Invoke();
            }



            return allLevelsReducedMeshes;

        }





        /// <summary>
        /// Generates the LODs and sets up a LOD Group for the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object to set up.</param>
        /// <param name="toleranceSpheres">The List of tolerance spheres.</param>
        /// <param name="lodLevelSettings">The LOD levels to set up.</param>
        /// <param name="saveAssetsPath">The path to where the generated assets should be saved. Can be null or empty to use the default path.</param>
        /// <param name="OnError">The method to invoke when an error occurs. The method is passed the description of the error.</param>
        /// <param name="displayErrorBox">Should an error dialog be shown when an error occurs?.</param>
        /// <param name="generateUV2">Should we generate secondary uv set for each mesh that will be combined.</param>
        /// <returns>True if the operation was successful. False otherwise.</returns>

        public static bool GenerateLODS(GameObject forObject, List<ToleranceSphere> toleranceSpheres, List<LODLevelSettings> lodLevelSettings, string saveAssetsPath, Action<string> OnError, bool displayErrorBox)
        {

            #region Pre Checks


            if (forObject == null)
            {
                string error = new System.ArgumentNullException(nameof(forObject)).Message;

                OnError?.Invoke(error);

                if(displayErrorBox)
                {
                    EditorUtility.DisplayDialog("Failed to generate LODs", error, "Ok");
                }

                return false;
            }

            else if (lodLevelSettings == null)
            {
                string error = new System.ArgumentNullException(nameof(lodLevelSettings)).Message;

                OnError?.Invoke(error);

                if (displayErrorBox)
                {
                    EditorUtility.DisplayDialog("Failed to generate LODs", error, "Ok");
                }

                return false;
            }

            var transform = forObject.transform;

            var existingLodParent = transform.Find(LOD_PARENT_OBJECT_NAME);

            if (existingLodParent != null)
            {
                string error = "The game object already appears to have LODs. Please remove them first.";

                OnError?.Invoke(error);

                if (displayErrorBox)
                {
                    EditorUtility.DisplayDialog("Failed to generate LODs", error, "Ok");
                }

                return false;
            }

            var existingLodGroup = forObject.GetComponent<LODGroup>();

            if (existingLodGroup != null)
            {
                string error = "The game object already appears to have an LOD Group. Please remove it first.";

                OnError?.Invoke(error);

                if (displayErrorBox)
                {
                    EditorUtility.DisplayDialog("Failed to generate LODs", error, "Ok");
                }

                return false;
            }

            // Collect all enabled renderers under the game object
            Renderer[] allRenderers = GetChildRenderersForLOD(forObject);

            if (allRenderers == null || allRenderers.Length == 0)
            {
                string error = "No valid renderers found under this object.";

                OnError?.Invoke(error);

                if (displayErrorBox)
                {
                    EditorUtility.DisplayDialog("Failed to generate LODs", error, "Ok");
                }

                return false;
            }

            #endregion Pre Checks


            var lodParentGameObject = new GameObject(LOD_PARENT_OBJECT_NAME);
            var lodParent = lodParentGameObject.transform;
            ParentAndResetTransform(lodParent, transform);

            var lodGroup = forObject.AddComponent<LODGroup>();

            //var renderersToDisable = new List<Renderer>(allRenderers.Length);
            var lodLevels = new LOD[lodLevelSettings.Count];

            string rootPath;
            string uniqueParentPath;

            if (!string.IsNullOrWhiteSpace(saveAssetsPath))
            {

                if (saveAssetsPath.EndsWith("/")) { saveAssetsPath.Remove(saveAssetsPath.Length - 1, 1); }

                if (AssetDatabase.IsValidFolder(saveAssetsPath))
                {
                    rootPath = saveAssetsPath + "/" + forObject.name + "_LOD_Meshes"; 
                }

                else
                {
                    rootPath = LOD_ASSETS_DEFAULT_SAVE_PATH + "/" + forObject.name + "_LOD_Meshes";
                    Debug.LogWarning($"The save path: \"{AutoLODSavePath}\" is not valid or does not exist. A default path \"{rootPath}\" will be used to save the LOD mesh assets.");
                }

            }

            else
            {
                rootPath = LOD_ASSETS_DEFAULT_SAVE_PATH + "/" + forObject.name + "_LOD_Meshes";
                Debug.LogWarning($"The save path: \"{AutoLODSavePath}\" is not valid or does not exist. A default path \"{rootPath}\" will be used to save the LOD mesh assets.");
            }


            uniqueParentPath = AssetDatabase.GenerateUniqueAssetPath(rootPath);

            if (!String.IsNullOrWhiteSpace(uniqueParentPath))
            {
                rootPath = uniqueParentPath;
            }


            MeshRenderer[] originalMeshRenderers = null;
            SkinnedMeshRenderer[] originalSkinnedMeshRenderers = null;
            LODGenerator.StaticRenderer[] staticRenderers = null;
            LODGenerator.SkinnedRenderer[] skinnedRenderers = null;


            originalMeshRenderers =
                (from renderer in allRenderers
                 where renderer.enabled && renderer as MeshRenderer != null
                 && renderer.transform.GetComponent<MeshFilter>() != null
                 && renderer.transform.GetComponent<MeshFilter>().sharedMesh != null
                 select renderer as MeshRenderer).ToArray();

            originalSkinnedMeshRenderers =
                (from renderer in allRenderers
                 where renderer.enabled && renderer as SkinnedMeshRenderer != null
                 && renderer.transform.GetComponent<SkinnedMeshRenderer>().sharedMesh != null
                 select renderer as SkinnedMeshRenderer).ToArray();



            staticRenderers = LODGenerator.GetStaticRenderers(originalMeshRenderers);
            skinnedRenderers = LODGenerator.GetSkinnedRenderers(originalSkinnedMeshRenderers);


            int totalStaticMeshes = staticRenderers == null ? 0 : staticRenderers.Length;
            int totalSkinnedMeshes = skinnedRenderers == null ? 0 : skinnedRenderers.Length;


            int lodLevelsToReduce = 0;
            int totalCombinedLevels = 0;

            List<LODLevelSettings> meshCombinedLevels = new List<LODLevelSettings>();

            foreach (var settings in lodLevelSettings)
            {
                if (!Mathf.Approximately(settings.reductionStrength, 0))
                {
                    lodLevelsToReduce++;
                }
                else if (settings.combineMeshes)
                {
                    totalCombinedLevels--;
                }

                if (settings.combineMeshes)
                {
                    meshCombinedLevels.Add(settings);
                    totalCombinedLevels++;
                }
            }



            
            Tuple<Mesh[], Mesh[], MeshFilter[], SkinnedMeshRenderer[]>[] allLevelsReducedMeshes;
            allLevelsReducedMeshes = GetReducedMeshes(ref staticRenderers, ref skinnedRenderers, forObject, lodLevelSettings.ToArray(), toleranceSpheres);

            List<PolyFewRuntime.MeshCombiner.StaticRenderer[]> levelsStaticCombinedRenderers = null;
            List<PolyFewRuntime.MeshCombiner.SkinnedRenderer[]> levelsSkinnedCombinedRenderers = null;

            levelsStaticCombinedRenderers  = new List<PolyFewRuntime.MeshCombiner.StaticRenderer[]>();
            levelsSkinnedCombinedRenderers = new List<PolyFewRuntime.MeshCombiner.SkinnedRenderer[]>();



            #region Combining Meshes

            if (meshCombinedLevels.Count > 0)
            {

                List<Mesh> meshesChangedToReadible = new List<Mesh>();

                foreach(var renderer in originalMeshRenderers)
                {
                    var meshFilter = renderer.GetComponent<MeshFilter>();

                    if (meshFilter == null || meshFilter.sharedMesh == null)
                    {
                        continue;
                    }

                    if (!meshFilter.sharedMesh.isReadable)
                    {
                        
                        ChangeMeshReadability(meshFilter.sharedMesh, true, false);

                        if (meshFilter.sharedMesh.isReadable)
                        {
                            meshesChangedToReadible.Add(meshFilter.sharedMesh);
                        }
                    }
                }
                

                foreach (var renderer in originalSkinnedMeshRenderers)
                {
                    if (renderer == null || renderer.sharedMesh == null)
                    {
                        continue;
                    }

                    if(!renderer.sharedMesh.isReadable)
                    {
                        ChangeMeshReadability(renderer.sharedMesh, true, false);

                        if (renderer.sharedMesh.isReadable)
                        {
                            meshesChangedToReadible.Add(renderer.sharedMesh);
                        }
                    }
                }
                

                int a = 0;
                bool ranOnce = false;

                foreach (var settings in lodLevelSettings)
                {
                    // iterate only over the meshes with combined levels
                    if(!settings.combineMeshes) { a++; continue; }

#if !UNITY_2017_3_OR_NEWER
                    // Check if we are in older versions of Unity with max vertex limit <= 65534 
                    if (!ranOnce)
                    {
                        ranOnce = true;

                        if (originalMeshRenderers.Length != 0)
                        {
                            int totalVertsCount = 0;

                            foreach(var renderer in originalMeshRenderers)
                            {
                                if(renderer == null) { continue; }

                                MeshFilter mf = renderer.gameObject.GetComponent<MeshFilter>();

                                if(mf == null) { continue; }

                                Mesh m = mf.sharedMesh;

                                if(m == null) { continue; }

                                totalVertsCount += m.vertexCount;
                            }

                            // Don't combine meshes in any LOD levels
                            if(totalVertsCount > 65534)
                            {
                                foreach (var levels in lodLevelSettings)
                                {
                                    levels.combineMeshes = false;
                                }

                                Debug.LogWarning($"No meshes under GameObject \"{forObject.name}\" will be combined. The combined mesh exceeds the maximum vertex count of 65534. This is a limitation in older versions of Unity (2017.2 and Below).");

                                break;
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

                            // Don't combine meshes in any LOD levels
                            if (totalVertsCount > 65534)
                            {
                                foreach (var levels in lodLevelSettings)
                                {
                                    levels.combineMeshes = false;
                                }

                                Debug.LogWarning($"No meshes under GameObject \"{forObject.name}\" will be combined. The combined mesh exceeds the maximum vertex count of 65534. This is a limitation in older versions of Unity (2017.2 and Below).");

                                break;
                            }
                        }
                    }
#endif

                    Tuple<Mesh[], Mesh[], MeshFilter[], SkinnedMeshRenderer[]> levelReducedMeshes = allLevelsReducedMeshes[a];

                    Mesh[] reducedStaticMeshes = levelReducedMeshes.Item1;
                    MeshFilter[] associatedMeshFilters = levelReducedMeshes.Item3;
                    Mesh[] reducedSkinnedMeshes = levelReducedMeshes.Item2;
                    SkinnedMeshRenderer[] associatedSkinnedRenderers = levelReducedMeshes.Item4;


                    for (int b = 0; b < associatedMeshFilters.Length; b++)
                    {
                        Mesh reducedMesh = reducedStaticMeshes[b];
                        MeshFilter associatedFilter = associatedMeshFilters[b];

                        if(associatedFilter != null)
                        {
                            associatedFilter.sharedMesh = reducedMesh;
                        }

                    }


                    for (int b = 0; b < associatedSkinnedRenderers.Length; b++)
                    {
                        Mesh reducedMesh = reducedSkinnedMeshes[b];
                        SkinnedMeshRenderer associatedSkinRenderer = associatedSkinnedRenderers[b];

                        if (associatedSkinRenderer != null)
                        {
                            associatedSkinRenderer.sharedMesh = reducedMesh;
                        }

                    }

                    SkinnedMeshRenderer[] skinnedRenderersActuallyCombined = null;

                    var staticCombinedRenderer  = PolyFewRuntime.MeshCombiner.CombineStaticMeshes(forObject.transform, -1, originalMeshRenderers, false);
                    var skinnedCombinedRenderer = PolyFewRuntime.MeshCombiner.CombineSkinnedMeshes(forObject.transform, -1, originalSkinnedMeshRenderers, ref skinnedRenderersActuallyCombined, false);

                    if (staticCombinedRenderer != null)
                    {
                        levelsStaticCombinedRenderers.Add(staticCombinedRenderer);
                    }

                    if(skinnedCombinedRenderer != null)
                    {
                        levelsSkinnedCombinedRenderers.Add(skinnedCombinedRenderer);                      
                    }


                    // With autoname = false the level index passed doesn't matter

                    a++;
                }

                // Change back the meshes readibility State
                foreach(var mesh in meshesChangedToReadible)
                {
                    Debug.LogWarning($"Mesh \"{mesh.name}\" was not readible so we marked it readible for the mesh combining process to complete and changed it back to non-readible after completion. This process can slow down LOD generation. You may want to mark this mesh Read/Write enabled in the model import settings, so that next time LOD generation on this model can be faster.");
                    ChangeMeshReadability(mesh, false, false);
                }

                RestoreMeshesFromPairs(dataContainer.objectMeshPairs);

            }

#endregion Combining meshes



            if(levelsStaticCombinedRenderers.Count == 0)  { levelsStaticCombinedRenderers = null; }
            if(levelsSkinnedCombinedRenderers.Count == 0) { levelsSkinnedCombinedRenderers = null; }

            
            int totalCombinedStaticMeshes = levelsStaticCombinedRenderers == null ? 0 : levelsStaticCombinedRenderers[0].Length;
            int totalCombinedSkinnedMeshes = levelsSkinnedCombinedRenderers == null ? 0 : levelsSkinnedCombinedRenderers[0].Length;

            // The total meshes to reduce doesn't include the meshes for which the LOD level settings say quality = 1 (means don't reduce)
            int totalMeshesToReduce = ((lodLevelsToReduce - totalCombinedLevels) * (totalStaticMeshes + totalSkinnedMeshes));
            totalMeshesToReduce += (totalCombinedLevels * (totalCombinedStaticMeshes + totalCombinedSkinnedMeshes));
            int totalMeshesToSave = 0;
            int meshesHandled = 0;
            int combinedLvlIndex = 0;


            #region Counting meshes to save

            for (int levelIndex = 0; levelIndex < lodLevelSettings.Count; levelIndex++)
            {

                var levelSettings = lodLevelSettings[levelIndex];

                var levelRenderers = new List<Renderer>((allRenderers != null ? allRenderers.Length : 0));
          

                if (levelSettings.combineMeshes)
                {

                    float quality = (1f - (levelSettings.reductionStrength / 100f));

                    if (levelsStaticCombinedRenderers != null)
                    {
                        var staticCombinedRenderers = levelsStaticCombinedRenderers[combinedLvlIndex];

                        for (int rendererIndex = 0; rendererIndex < staticCombinedRenderers.Length; rendererIndex++)
                        {
                            PolyFewRuntime.MeshCombiner.StaticRenderer staticCombinedRenderer = staticCombinedRenderers[rendererIndex];
                            Mesh reducedMesh = staticCombinedRenderer.mesh;

                            if (!Mathf.Approximately(quality, 1) || !AssetDatabase.Contains(reducedMesh))
                            {
                                totalMeshesToSave++;
                            }

                        }
                    }

                    if (levelsSkinnedCombinedRenderers != null)
                    {
                        var skinnedCombinedRenderers = levelsSkinnedCombinedRenderers[combinedLvlIndex];

                        for (int rendererIndex = 0; rendererIndex < skinnedCombinedRenderers.Length; rendererIndex++)
                        {
                            var skinnedCombinedRenderer = skinnedCombinedRenderers[rendererIndex];
                            Mesh reducedMesh = skinnedCombinedRenderer.mesh;

                            if (!Mathf.Approximately(quality, 1) || !AssetDatabase.Contains(reducedMesh))
                            {
                                totalMeshesToSave++;
                            }
                        }
                    }

                    combinedLvlIndex++;

                }

                else
                {
                    Mesh[] reducedStaticMeshes = allLevelsReducedMeshes[levelIndex].Item1;
                    Mesh[] reducedSkinnedMeshes = allLevelsReducedMeshes[levelIndex].Item2;

                    float quality = (1f - (levelSettings.reductionStrength / 100f));


                    if (staticRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < staticRenderers.Length; rendererIndex++)
                        {

                            LODGenerator.StaticRenderer renderer = staticRenderers[rendererIndex];
                            Mesh mesh = renderer.mesh;
                            Mesh reducedMesh = reducedStaticMeshes[rendererIndex];

                            if (!Mathf.Approximately(quality, 1))
                            {
                                totalMeshesToSave++;
                            }
                        }
                    }

                    if (skinnedRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
                        {
                            var renderer = skinnedRenderers[rendererIndex];
                            var mesh = renderer.mesh;
                            Mesh reducedMesh = reducedSkinnedMeshes[rendererIndex];

                            // Simplify the mesh if necessary
                            if (!Mathf.Approximately(quality, 1))
                            {
                                mesh = reducedMesh;
                                totalMeshesToSave++;   
                            }
                        }
                    }

                }

            }

            #endregion Counting meshes to save


            combinedLvlIndex = 0;


            #region DEALING_WITH_BASE_LEVEL
            //For level 0 we don't create new gameobject to become the lod level
            //instead the original gameobject will be used to represent level 0
            var baseLevelRenderers = new List<Renderer>();

            if (staticRenderers != null)
            {
                for (int rendererIndex = 0; rendererIndex < staticRenderers.Length; rendererIndex++)
                {
                    baseLevelRenderers.Add(staticRenderers[rendererIndex].originalRenderer);
                }
            }

            if(skinnedRenderers != null)
            {
                for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
                {
                    baseLevelRenderers.Add(skinnedRenderers[rendererIndex].originalRenderer);
                }
            }

            lodLevels[0] = new LOD(lodLevelSettings[0].transitionHeight, baseLevelRenderers.ToArray());

            #endregion DEALING_WITH_BASE_LEVEL


            for (int levelIndex = 1; levelIndex < lodLevelSettings.Count; levelIndex++)
            {

                var levelSettings = lodLevelSettings[levelIndex];
                var levelGameObject = new GameObject(string.Format("Level{0:00}", levelIndex + 1));  // Making levels start from 1 not 0(index based)
                var levelTransform = levelGameObject.transform;
                var levelRenderers = new List<Renderer>((allRenderers != null ? allRenderers.Length : 0));


                #region Setting Up and Saving LOD Assets


                if (levelSettings.combineMeshes)
                {
                    #region DEPRECATED_MOVED_TO_BATCHFEW
                    ParentAndResetTransform(levelTransform, lodParent);

                    float quality = (1f - (levelSettings.reductionStrength / 100f));

                    if (levelsStaticCombinedRenderers != null)
                    {
                        var staticCombinedRenderers  = levelsStaticCombinedRenderers[combinedLvlIndex];

                        for (int rendererIndex = 0; rendererIndex < staticCombinedRenderers.Length; rendererIndex++)
                        {
                            PolyFewRuntime.MeshCombiner.StaticRenderer staticCombinedRenderer = staticCombinedRenderers[rendererIndex];
                            Mesh reducedMesh = staticCombinedRenderer.mesh;


                            // Simplify the mesh if necessary
                            //if (!Mathf.Approximately(quality, 1))
                            //{

                            // In case of combined levels with reduction strength 0 some meshes might not be combined and will not be reduced
                            // and hence will just refer to the original meshes of the model, so we don't save them as they already exist in the asset database and will throw error if tried to save
                            if (!Mathf.Approximately(quality, 1) || !AssetDatabase.Contains(reducedMesh))
                            {

                                bool lightMapGenerated = false;
#if UNITY_EDITOR
                                if (levelSettings.generateUV2)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Generating UV2 And Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);

                                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(reducedMesh);
                                    lightMapGenerated = true;
                                }

#endif

                                if (!lightMapGenerated)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);      
                                }

                                SaveLODMeshAsset(reducedMesh, forObject.name, staticCombinedRenderer.name, levelIndex, staticCombinedRenderer.mesh.name, rootPath);

                                if (staticCombinedRenderer.isNewMesh)
                                {
                                    //DestroyImmediate(renderer.mesh);
                                    //renderer.mesh = null;
                                }
                            }
                            //}

                            string rendererName = string.Format("{0:000}_static_combined_{1}", rendererIndex, staticCombinedRenderer.name);
                            var levelRenderer = CreateStaticLevelRenderer(rendererName, levelTransform, staticCombinedRenderer.transform, reducedMesh, staticCombinedRenderer.materials);
                            levelRenderers.Add(levelRenderer);
                        }
                    }

                    if (levelsSkinnedCombinedRenderers != null)
                    {

                        var skinnedCombinedRenderers = levelsSkinnedCombinedRenderers[combinedLvlIndex];

                        for (int rendererIndex = 0; rendererIndex < skinnedCombinedRenderers.Length; rendererIndex++)
                        {
                            var skinnedCombinedRenderer = skinnedCombinedRenderers[rendererIndex];
                            Mesh reducedMesh = skinnedCombinedRenderer.mesh;

                            // Simplify the mesh if necessary
                            //if (!Mathf.Approximately(quality, 1))
                            //{
                            
                            // In case of combined levels with reduction strength 0 some meshes might not be combined and will not be reduced
                            // as well and hence will just refer to the original meshes of the model, so we don't save them as they already exist in the asset database and will throw error if tried to save
                            if(!Mathf.Approximately(quality, 1) || !AssetDatabase.Contains(reducedMesh))
                            {

                                bool lightMapGenerated = false;
#if UNITY_EDITOR
                                if (levelSettings.generateUV2)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Generating UV2 And Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);

                                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(reducedMesh);
                                    lightMapGenerated = true;
                                }

#endif

                                if (!lightMapGenerated)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);
                                }

                                SaveLODMeshAsset(reducedMesh, forObject.name, skinnedCombinedRenderer.name, levelIndex, skinnedCombinedRenderer.mesh.name, rootPath);

                                if (skinnedCombinedRenderer.isNewMesh)
                                {
                                    //DestroyObject(renderer.mesh);
                                    //renderer.mesh = null;
                                }
                            }
       
                            //}

                            string rendererName = string.Format("{0:000}_skinned_{1}", rendererIndex, skinnedCombinedRenderer.name);
                            var levelRenderer = CreateSkinnedLevelRenderer(rendererName, levelTransform, skinnedCombinedRenderer.transform, reducedMesh, skinnedCombinedRenderer.materials, skinnedCombinedRenderer.rootBone, skinnedCombinedRenderer.bones);
                            levelRenderers.Add(levelRenderer);
                        }
                    }

                    combinedLvlIndex++;
                    #endregion DEPRECATED_MOVED_TO_BATCHFEW
                }

                else
                {
                    Mesh[] reducedStaticMeshes = allLevelsReducedMeshes[levelIndex].Item1;
                    Mesh[] reducedSkinnedMeshes = allLevelsReducedMeshes[levelIndex].Item2;

                    ParentAndResetTransform(levelTransform, lodParent);

                    float quality = (1f - (levelSettings.reductionStrength / 100f));

                    
                    if (staticRenderers != null)
                    {

                        for (int rendererIndex = 0; rendererIndex < staticRenderers.Length; rendererIndex++)
                        {

                            LODGenerator.StaticRenderer renderer = staticRenderers[rendererIndex];
                            Mesh mesh = renderer.mesh;
                            Mesh reducedMesh = reducedStaticMeshes[rendererIndex];
                            // In case of lodlevels with reduction strength 0 some meshes were not reduced
                            // and hence will just refer to the original meshes of the model, so we don't bother saving them                            
                            if (!Mathf.Approximately(quality, 1))
                            {

                                //reducedMesh.bindposes = mesh.bindposes;
                                mesh = reducedMesh;

                                bool lightMapGenerated = false;
#if UNITY_EDITOR
                                if (levelSettings.generateUV2)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Generating UV2 And Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);

                                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
                                    lightMapGenerated = true;
                                }

#endif

                                if (!lightMapGenerated)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);
                                }


                                SaveLODMeshAsset(mesh, forObject.name, renderer.name, levelIndex, renderer.mesh.name, rootPath);

                                if (renderer.isNewMesh)
                                {
                                    DestroyObject(renderer.mesh);
                                    renderer.mesh = null;
                                }
                            }

                            string rendererName = string.Format("{0:000}_static_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateStaticLevelRenderer(rendererName, levelTransform, renderer.transform, mesh, renderer.materials);
                            levelRenderers.Add(levelRenderer);           

                        }
                    }

                    if (skinnedRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
                        {
                            var renderer = skinnedRenderers[rendererIndex];
                            var mesh = renderer.mesh;
                            Mesh reducedMesh = reducedSkinnedMeshes[rendererIndex];

                            // Simplify the mesh if necessary
                            if (!Mathf.Approximately(quality, 1))
                            {

                                //reducedMesh.bindposes = mesh.bindposes;
                                mesh = reducedMesh;

                                bool lightMapGenerated = false;
#if UNITY_EDITOR
                                if (levelSettings.generateUV2)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Generating UV2 And Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);

                                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
                                    lightMapGenerated = true;
                                }

#endif

                                if (!lightMapGenerated)
                                {
                                    EditorUtility.DisplayProgressBar("Generating LODs", $"Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);
                                }


                                SaveLODMeshAsset(mesh, forObject.name, renderer.name, levelIndex, renderer.mesh.name, rootPath);
                                
                                if (renderer.isNewMesh)
                                {
                                    DestroyObject(renderer.mesh);
                                    renderer.mesh = null;
                                }
                            }


                            string rendererName = string.Format("{0:000}_skinned_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateSkinnedLevelRenderer(rendererName, levelTransform, renderer.transform, mesh, renderer.materials, renderer.rootBone, renderer.bones);
                            levelRenderers.Add(levelRenderer);
                            
                        }
                    }

                }


                #endregion Setting Up and Saving LOD Assets

                lodLevels[levelIndex] = new LOD(levelSettings.transitionHeight, levelRenderers.ToArray());

            }


            CreateBackup(forObject, lodParent.gameObject, null);


            lodGroup.animateCrossFading = false;
            lodGroup.SetLODs(lodLevels);


            EditorUtility.ClearProgressBar();

            return true;
        }



        public static bool HasLODs(GameObject checkFor)
        {
            if (checkFor == null) { return false; }

            GameObject existingLodParent = checkFor.transform.Find(LOD_PARENT_OBJECT_NAME)?.gameObject;
            DataContainer dataContainer = checkFor.GetComponent<PolyFew>().dataContainer;

            if (existingLodParent == null)
            {
                existingLodParent = dataContainer.lodBackup?.lodParentObject;
            }

            var existingLodGroup = checkFor.transform.GetComponent<LODGroup>();

            return (existingLodParent != null || existingLodGroup != null);
        }




        /// <summary>
        /// Combine all meshes nested under the provided GameObject.
        /// </summary>
        /// <param name="forObject">The game object under which all renderers/meshes will be combined.</param>
        /// <param name="combinedBaseName">The base name from which the full name of the combined meshes will be derived.</param>
        /// <param name="saveAssetsPath">The path to where the generated assets should be saved. Can be null or empty to use the default path.</param>
        /// <param name="OnError">The method to invoke when an error occurs. The method is passed the error title and the description of the error.</param>
        /// <param name="combineTarget">Indicates what kind of meshes to combine.</param>
        /// <param name="generateUV2">Should we generate secondary uv set for each mesh that will be combined.</param>

        public static void CombineMeshes(GameObject forObject, string combinedBaseName, string saveAssetsPath, Action<string, string> OnError, MeshCombineTarget combineTarget = MeshCombineTarget.SkinnedAndStatic, bool generateUV2 = false)
        {


            #region Pre Checks

            if (forObject == null)
            {
                string error = $"Failed to combine meshes for \"{forObject.name}\". " + new System.ArgumentNullException(nameof(forObject)).Message;
                OnError?.Invoke("Operation Failed", error);
                return;
            }


            Renderer[] allRenderers = forObject.GetComponentsInChildren<Renderer>(true);


            if (allRenderers == null || allRenderers.Length == 0)
            {
                string error = $"Failed to combine meshes for \"{forObject.name}\". No feasible renderers found in the GameObject or any of the nested children to combine.";
                OnError?.Invoke("Operation Failed", error);
                return;
            }

            #endregion Pre Checks


            string rootPath;
            string uniqueParentPath;
            combinedBaseName = MakeNameSafe(combinedBaseName);


            if (!string.IsNullOrWhiteSpace(saveAssetsPath))
            {

                if (saveAssetsPath.EndsWith("/")) { saveAssetsPath.Remove(saveAssetsPath.Length - 1, 1); }

                if (AssetDatabase.IsValidFolder(saveAssetsPath))
                {
                    rootPath = saveAssetsPath + "/" + combinedBaseName + (combinedBaseName.EndsWith("_") ? "Combined_Meshes" : "_Combined_Meshes");
                }

                else
                {
                    rootPath = BATCHFEW_ASSETS_DEFAULT_SAVE_PATH + "/" + combinedBaseName + (combinedBaseName.EndsWith("_") ? "Combined_Meshes" : "_Combined_Meshes");
                    Debug.LogWarning($"The save path: \"{BatchFewSavePath}\" is not valid or does not exist. A default path \"{rootPath}\" will be used to save the combined mesh assets.");
                }

            }

            else
            {
                rootPath = BATCHFEW_ASSETS_DEFAULT_SAVE_PATH + "/" + combinedBaseName + (combinedBaseName.EndsWith("_") ? "Combined_Meshes" : "_Combined_Meshes");
                Debug.LogWarning($"The save path: \"{BatchFewSavePath}\" is not valid or does not exist. A default path \"{rootPath}\" will be used to save the combined mesh assets.");
            }


            uniqueParentPath = AssetDatabase.GenerateUniqueAssetPath(rootPath);

            if (!String.IsNullOrWhiteSpace(uniqueParentPath))
            {
                rootPath = uniqueParentPath;
            }


            MeshRenderer[] originalMeshRenderers = null;
            HashSet<Transform> originalMeshRenderersTransforms = new HashSet<Transform>();
            SkinnedMeshRenderer[] originalSkinnedMeshRenderers = null;
            HashSet<Transform> originalSkinnedRenderersTransforms = new HashSet<Transform>();

            PolyFewRuntime.MeshCombiner.StaticRenderer[] staticRenderers = null;
            PolyFewRuntime.MeshCombiner.SkinnedRenderer[] skinnedRenderers = null;





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

            if (originalMeshRenderers != null)
            {
                foreach (var item in originalMeshRenderers) { originalMeshRenderersTransforms.Add(item.transform); }
            }
            if (originalSkinnedMeshRenderers != null)
            {
                foreach (var item in originalSkinnedMeshRenderers) { originalSkinnedRenderersTransforms.Add(item.transform); }
            }


            if(originalMeshRenderers.Length == 1)
            {
                // if it has submeshes
                if (originalMeshRenderers[0].sharedMaterials.Length > 1)
                {
                    bool isFeasible = false;
                    HashSet<int> materials = new HashSet<int>();

                    //if all the submeshes share the same material or at least 2 do
                    foreach(var mat in originalMeshRenderers[0].sharedMaterials)
                    {
                        if(mat == null) { continue; }
                        int hash = mat.GetHashCode();
                        if (materials.Contains(hash))
                        {
                            //one match found no need to go further
                            isFeasible = true;
                            break;
                        }

                        materials.Add(hash);
                    }
                    
                    if(isFeasible)
                    {
                        dummyStatic = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        MeshRenderer mr = dummyStatic.GetComponent<MeshRenderer>();
                        dummyStaticMat = mr.sharedMaterials[0];
                        dummyStaticMat.name = "dummy-material";
                        MeshFilter f = dummyStatic.GetComponent<MeshFilter>();
                        f.sharedMesh = new Mesh();
                        dummyStatic.name = "";
                        dummyStatic.hideFlags = HideFlags.HideAndDontSave;
                        var list = originalMeshRenderers.ToList();
                        list.Add(mr);
                        originalMeshRenderers = list.ToArray();
                    }

                }
            }

            if (originalSkinnedMeshRenderers.Length == 1)
            {
                // if it has submeshes
                if (originalSkinnedMeshRenderers[0].sharedMaterials.Length > 1)
                {
                    bool isFeasible = false;
                    HashSet<int> materials = new HashSet<int>();

                    //if all the submeshes share the same material or at least 2 do
                    foreach (var mat in originalSkinnedMeshRenderers[0].sharedMaterials)
                    {
                        if (mat == null) { continue; }
                        int hash = mat.GetHashCode();
                        if (materials.Contains(hash))
                        {
                            //one match found no need to go further
                            isFeasible = true;
                            break;
                        }

                        materials.Add(hash);
                    }

                    if (isFeasible)
                    {
                        dummySkinned = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        MeshRenderer mr = dummySkinned.GetComponent<MeshRenderer>();
                        dummySkinnedMat = mr.sharedMaterials[0];
                        DestroyImmediate(mr);
                        DestroyImmediate(dummySkinned.GetComponent<MeshFilter>());
                        SkinnedMeshRenderer smr = dummySkinned.AddComponent<SkinnedMeshRenderer>();
                        smr.sharedMesh = new Mesh();
                        smr.sharedMaterials = new Material[1] { dummySkinnedMat };
                        
                        dummySkinnedMat.name = "dummy-material";
                        dummySkinned.name = "";
                        dummySkinned.hideFlags = HideFlags.HideAndDontSave;
                        var list = originalSkinnedMeshRenderers.ToList();
                        list.Add(smr);
                        originalSkinnedMeshRenderers = list.ToArray();
                    }

                }
            }

            #region Pre Checks

            foreach (var mr in originalMeshRenderers)
            {
                var materials = mr.sharedMaterials;
                var mf = mr.GetComponent<MeshFilter>();
                var mesh = mf?.sharedMesh;

                if (mesh == null)
                {
                    string error = $"The MeshFilter on GameObject \"{mr.name}\" has no mesh. Either attach a valid mesh or remove this GameObject from the GameObjects to be combined, so that the mesh combining process can proceed.";
                    OnError?.Invoke("Operation Failed", error);
                    return;
                }

                else
                {
                    int subMeshCount = mesh.subMeshCount;

                    if (materials != null)
                    {
                        if (materials.Length != subMeshCount)
                        {
                            string error = $"The materials set in the MeshRenderer for GameObject \"{mr.name}\" doesn't match the submesh count of the mesh \"{mesh.name}\":  ({materials.Length} != {subMeshCount}). Please ensure that the materials set in each MeshRenderer match the number of submeshes in the corresponding mesh.";
                            OnError?.Invoke("Operation Failed", error);
                            return;
                        }

                        int a = 0;

                        foreach (var mat in materials)
                        {
                            if (mat == null)
                            {
                                string error = $"The material at index {a} set on the MeshRenderer for GameObject \"{mr.name}\" is null. Please ensure that the materials list contains no null entry.";
                                OnError?.Invoke("Operation Failed", error);
                                return;
                            }
                            a++;
                        }
                    }

                }
            }


            foreach (var smr in originalSkinnedMeshRenderers)
            {
                var materials = smr.sharedMaterials;
                var mesh = smr.sharedMesh;

                if (mesh == null)
                {
                    string error = $"The SkinnedMeshRenderer on GameObject \"{smr.name}\" has no mesh. Either attach a valid mesh or remove this GameObject from the GameObjects to be combined, so that the mesh combining process can proceed.";
                    OnError?.Invoke("Operation Failed", error);
                    return;
                }

                else
                {
                    int subMeshCount = mesh.subMeshCount;

                    if (materials != null)
                    {
                        if (materials.Length != subMeshCount)
                        {
                            string error = $"The materials set in the SkinnedMeshRenderer for GameObject \"{smr.name}\" doesn't match the submesh count of the mesh \"{mesh.name}\":  ({materials.Length} != {subMeshCount}). Please ensure that the materials set in each SkinnedMeshRenderer match the number of submeshes in the corresponding mesh.";
                            OnError?.Invoke("Operation Failed", error);
                            return;
                        }

                        int a = 0;

                        foreach (var mat in materials)
                        {
                            if (mat == null)
                            {
                                string error = $"The material at index {a} set in the SkinnedMeshRenderer for GameObject \"{smr.name}\" is null. Please ensure that the materials list contains no null entry.";
                                OnError?.Invoke("Operation Failed", error);
                                return;
                            }
                            a++;
                        }
                    }

                }
            }

            #endregion Pre Checks



            if (combineTarget == MeshCombineTarget.StaticOnly)
            {
                originalSkinnedMeshRenderers = new SkinnedMeshRenderer[0];
            }

            else if (combineTarget == MeshCombineTarget.SkinnedOnly)
            {
                originalMeshRenderers = new MeshRenderer[0];
            }



            staticRenderers = PolyFewRuntime.MeshCombiner.GetStaticRenderers(originalMeshRenderers);
            skinnedRenderers = PolyFewRuntime.MeshCombiner.GetSkinnedRenderers(originalSkinnedMeshRenderers);

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

            List<Mesh> meshesChangedToReadible = new List<Mesh>();

            #region Marking meshes readible
            foreach (var renderer in originalMeshRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();

                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                if (!meshFilter.sharedMesh.isReadable)
                {

                    ChangeMeshReadability(meshFilter.sharedMesh, true, false);

                    if (meshFilter.sharedMesh.isReadable)
                    {
                        meshesChangedToReadible.Add(meshFilter.sharedMesh);
                    }
                }
            }


            foreach (var renderer in originalSkinnedMeshRenderers)
            {
                if (renderer == null || renderer.sharedMesh == null)
                {
                    continue;
                }

                if (!renderer.sharedMesh.isReadable)
                {
                    ChangeMeshReadability(renderer.sharedMesh, true, false);

                    if (renderer.sharedMesh.isReadable)
                    {
                        meshesChangedToReadible.Add(renderer.sharedMesh);
                    }
                }
            }
            #endregion Marking meshes readible


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
            PolyFewRuntime.MeshCombiner.generateUV2 = generateUV2;

            EditorUtility.DisplayProgressBar("Combining Meshes", $"Combining Static Meshes and MeshRenderers", 0);
            PolyFewRuntime.MeshCombiner.StaticRenderer[] staticCombinedRenderers = PolyFewRuntime.MeshCombiner.CombineStaticMeshes(forObject.transform, -1, originalMeshRenderers, false, combinedBaseName);
            EditorUtility.DisplayProgressBar("Combining Meshes", $"Combining Skinned Meshes and SkinnedMeshRenderers", 0);
            PolyFewRuntime.MeshCombiner.SkinnedRenderer[] skinnedCombinedRenderers = PolyFewRuntime.MeshCombiner.CombineSkinnedMeshes(forObject.transform, -1, originalSkinnedMeshRenderers, ref skinnedRenderersActuallyCombined, false, combinedBaseName);

            

            if (skinnedRenderersActuallyCombined != null)
            {
                foreach (var smr in skinnedRenderersActuallyCombined) { smr.enabled = false; }
            }
            if (originalMeshRenderers != null)
            {
                foreach (var mr in originalMeshRenderers) { mr.enabled = false; }
            }


            // Change back the meshes readibility State
            foreach (var mesh in meshesChangedToReadible)
            {
                Debug.LogWarning($"Mesh \"{mesh.name}\" was not readible so we marked it readible for the mesh combining process to complete and changed it back to non-readible after completion. This process can slow down mesh combining. You may want to mark this mesh Read/Write enabled in the model import settings, so that next time mesh combining on this model can be faster.");
                ChangeMeshReadability(mesh, false, false);
            }


            #endregion Combining meshes


            int totalCombinedStaticMeshes = staticCombinedRenderers == null ? 0 : staticCombinedRenderers.Length;
            int totalCombinedSkinnedMeshes = skinnedCombinedRenderers == null ? 0 : skinnedCombinedRenderers.Length;

            int totalMeshesToSave = totalCombinedStaticMeshes + totalCombinedSkinnedMeshes;
            int meshesHandled = 0;

            GameObject parentObject = forObject;

            HashSet<Transform> combinedStaticObjects = new HashSet<Transform>();
            HashSet<Transform> combinedSkinnedObjects = new HashSet<Transform>();


            for (int rendererIndex = 0; rendererIndex < totalCombinedStaticMeshes; rendererIndex++)
            {

                PolyFewRuntime.MeshCombiner.StaticRenderer staticCombinedRenderer = staticCombinedRenderers[rendererIndex];
                Mesh combinedMesh = staticCombinedRenderer.mesh;

                EditorUtility.DisplayProgressBar("Combining Renderers", $"Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);

                if (!AssetDatabase.Contains(combinedMesh))
                {
                    SaveCombinedMeshAsset(combinedMesh, staticCombinedRenderer.name, rootPath);
                }

                string rendererName = $"{staticCombinedRenderer.name}";
                var levelRenderer = CreateStaticLevelRenderer(rendererName, parentObject.transform, staticCombinedRenderer.transform, combinedMesh, staticCombinedRenderer.materials);

                combinedStaticObjects.Add(levelRenderer.transform);

                // Make this combined MeshRenderer object a direct child of the Main Object
                levelRenderer.transform.parent = forObject.transform;
            }

            for (int rendererIndex = 0; rendererIndex < totalCombinedSkinnedMeshes; rendererIndex++)
            {

                PolyFewRuntime.MeshCombiner.SkinnedRenderer skinnedCombinedRenderer = skinnedCombinedRenderers[rendererIndex];
                Mesh combinedMesh = skinnedCombinedRenderer.mesh;

                EditorUtility.DisplayProgressBar("Combining Renderers", $"Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);


                if (!AssetDatabase.Contains(combinedMesh))
                {
                    SaveCombinedMeshAsset(combinedMesh, skinnedCombinedRenderer.name, rootPath);
                }


                string rendererName = $"{skinnedCombinedRenderer.name}";
                var levelRenderer = CreateSkinnedLevelRenderer(rendererName, parentObject.transform, skinnedCombinedRenderer.transform, combinedMesh, skinnedCombinedRenderer.materials, skinnedCombinedRenderer.rootBone, skinnedCombinedRenderer.bones);

                combinedSkinnedObjects.Add(levelRenderer.transform);



                // Make this combined SkinnedMeshRenderer object a direct child of the Main Object
                levelRenderer.transform.parent = forObject.transform;
            }



            GameObject bonesHiererachyHolder = new GameObject(forObject.name + "_bonesHierarchy");
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



            #region CLEANING SUBMESH COLLAPSE DATA

            MeshRenderer mrend = null;
            Material[] staticMats = null;

            if(combinedStaticObjects != null && combinedStaticObjects.Count > 0)
            {
                mrend = combinedStaticObjects.ToArray()[0].GetComponent<MeshRenderer>();
                staticMats = mrend?.sharedMaterials;
            }

            if (staticMats != null && dummyStaticMat != null && dummyStatic != null)
            {
                Material[] sharedMats = new Material[staticMats.Length];

                for(int a = 0; a < staticMats.Length - 1; a++)
                {
                    if(staticMats[a].GetHashCode().Equals(dummyStaticMat.GetHashCode()))
                    {
                        a--;
                        continue;     
                    }

                    sharedMats[a] = staticMats[a];    
                }

                mrend.sharedMaterials = sharedMats;
                dummyStaticMat = null;
            }

            SkinnedMeshRenderer smrend = null;
            Material[] skinnedMats = null;

            if (combinedSkinnedObjects != null && combinedSkinnedObjects.Count > 0)
            {
                smrend = combinedSkinnedObjects.ToArray()[0].GetComponent<SkinnedMeshRenderer>();
                skinnedMats = smrend?.sharedMaterials;
            }

            if (skinnedMats != null && dummySkinnedMat != null && dummySkinned != null)
            {
                Material[] sharedMats = new Material[skinnedMats.Length];

                for (int a = 0; a < skinnedMats.Length - 1; a++)
                {
                    if (skinnedMats[a].GetHashCode().Equals(dummySkinnedMat.GetHashCode()))
                    {
                        a--;
                        continue;
                    }

                    sharedMats[a] = skinnedMats[a];
                }

                smrend.sharedMaterials = sharedMats;
                dummySkinnedMat = null;
            }

            #endregion CLEANING SUBMESH COLLAPSE DATA


            EditorUtility.ClearProgressBar();
        }




        /// <summary>
        /// Converts all skinned meshes in the provided GameObject to non skinned/static meshes and also changes the corresponding renderer components.
        /// </summary>
        /// <param name="forObject">The game object under which all renderers/meshes will be combined.</param>
        /// <param name="folderBaseName">The base name from which the full name of the new folder will be derived.</param>
        /// <param name="saveAssetsPath">The path to where the generated assets should be saved. Can be null or empty to use the default path.</param>
        /// <param name="OnError">The method to invoke when an error occurs. The method is passed the error title and the description of the error.</param>
        /// <param name="regardChildren">If true then all the deep nested skinned meshes under this object will also be converted, otherwise only the skinned mesh renderer if any attached to this particular object is considered.</param>
        /// <param name="generateUV2">Should we generate secondary uv set for each mesh that will be combined.</param>
        public static void ConvertSkinnedMeshes(GameObject forObject, string folderBaseName, string saveAssetsPath, Action<string, string> OnError, bool regardChildren, bool generateUV2 = false)
        {


            #region Pre Checks

            if (forObject == null)
            {
                string error = $"Failed to convert skinned meshes for \"{forObject.name}\". " + new System.ArgumentNullException(nameof(forObject)).Message;
                OnError?.Invoke("Operation Failed", error);
                return;
            }


            SkinnedMeshRenderer[] renderersToConvert = null;

            if (regardChildren) { renderersToConvert = forObject.GetComponentsInChildren<SkinnedMeshRenderer>(true); }
            else
            {
                var smr = forObject.GetComponent<SkinnedMeshRenderer>();
                if(smr != null) { renderersToConvert = new SkinnedMeshRenderer[] { smr }; }   
            }

            
            if (renderersToConvert == null || renderersToConvert.Length == 0)
            {
                string error = "";

                if (regardChildren)
                {
                    error = $"Failed to convert skinned meshes for \"{forObject.name}\". No feasible skinned mesh renderer found in the GameObject or any of the nested children to convert.";
                }
                else
                {
                    error = $"Failed to convert skinned mesh for \"{forObject.name}\". No Skinned mesh renderer found on the GameObject to convert.";
                }
                OnError?.Invoke("Operation Failed", error);
                return;
            }


            renderersToConvert = ( from renderer in renderersToConvert
                                   where renderer.sharedMesh != null
                                   select renderer ).ToArray();


            if (renderersToConvert == null || renderersToConvert.Length == 0)
            {
                string error = "";

                if (regardChildren)
                {
                    error = $"Failed to convert skinned meshes for \"{forObject.name}\". No feasible skinned mesh renderer found in the GameObject or any of the nested children to convert.";
                }
                else
                {
                    error = $"Failed to convert skinned mesh for \"{forObject.name}\". No Skinned mesh renderer found on the GameObject to convert.";
                }
                OnError?.Invoke("Operation Failed", error);
                return;
            }

            #endregion Pre Checks



            string rootPath;
            string uniqueParentPath;
            folderBaseName = MakeNameSafe(folderBaseName);


            if (!string.IsNullOrWhiteSpace(saveAssetsPath))
            {

                if (saveAssetsPath.EndsWith("/")) { saveAssetsPath.Remove(saveAssetsPath.Length - 1, 1); }

                if (AssetDatabase.IsValidFolder(saveAssetsPath))
                {
                    rootPath = saveAssetsPath + "/" + folderBaseName + "_Skinned_Converted_Meshes";
                }

                else
                {
                    rootPath = BATCHFEW_ASSETS_DEFAULT_SAVE_PATH + "/" + folderBaseName + "_Skinned_Converted_Meshes";
                    Debug.LogWarning($"The save path: \"{BatchFewSavePath}\" is not valid or does not exist. A default path \"{rootPath}\" will be used to save the converted mesh assets.");
                }

            }

            else
            {
                rootPath = BATCHFEW_ASSETS_DEFAULT_SAVE_PATH + "/" + folderBaseName + "_Skinned_Converted_Meshes";
                Debug.LogWarning($"The save path: \"{BatchFewSavePath}\" is not valid or does not exist. A default path \"{rootPath}\" will be used to save the converted mesh assets.");
            }


            uniqueParentPath = AssetDatabase.GenerateUniqueAssetPath(rootPath);

            if (!String.IsNullOrWhiteSpace(uniqueParentPath))
            {
                rootPath = uniqueParentPath;
            }


            int count = 0;
            Mesh[] convertedMeshes = new Mesh[renderersToConvert.Length];
            List<GameObject> bonesToDelete = new List<GameObject>();
            
            foreach (var smr in renderersToConvert)
            {

                EditorUtility.DisplayProgressBar("Converting Skinned Meshes", $"Converting Skinned Meshes {++count}/{renderersToConvert.Length}",  (float)count / renderersToConvert.Length);

                Mesh convertedMesh = new Mesh();
                convertedMesh.name = smr.sharedMesh.name + "-Skinned_Converted_Mesh";
                smr.BakeMesh(convertedMesh);
                //Remove blendshapes as they are useless now
                convertedMesh.ClearBlendShapes();

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
                    if(rootBone.parent.gameObject.GetHashCode() != smr.gameObject.GetHashCode())
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

                UnityEditor.MeshUtility.Optimize(convertedMesh);

                if (generateUV2)
                {
                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(convertedMesh);
                }

                convertedMeshes[count - 1] = convertedMesh;
            }


            foreach (var bone in bonesToDelete) { DestroyImmediate(bone); }


            int totalMeshesToSave = convertedMeshes.Length;
            int meshesHandled = 0;

        
            foreach(var convertedMesh in convertedMeshes)
            {

                EditorUtility.DisplayProgressBar("Converting Skinned Meshes", $"Saving Mesh Assets {++meshesHandled}/{totalMeshesToSave}", (float)meshesHandled / totalMeshesToSave);

                if (!AssetDatabase.Contains(convertedMesh))
                {
                    SaveCombinedMeshAsset(convertedMesh, convertedMesh.name, rootPath);
                }
            }

            EditorUtility.ClearProgressBar();
        }




        #region From MeshSimplifier


        public static bool DestroyLODs(GameObject gameObject)
        {
            if (gameObject == null)
            {
                string error = new System.ArgumentNullException(nameof(gameObject)).Message;
                EditorUtility.DisplayDialog("Failed to delete LODs", error, "Ok");
                return false;
            }

            RestoreBackup(gameObject);

            Transform transform = gameObject.transform;
            Transform lodParent = transform.Find(LOD_PARENT_OBJECT_NAME);
            DataContainer dataContainer = gameObject.GetComponent<PolyFew>().dataContainer;

            if (lodParent == null)
            {
                lodParent = dataContainer.lodBackup?.lodParentObject?.transform;

                if (lodParent == null)
                {
                    EditorUtility.DisplayDialog("Failed to delete LODs", $"Found no LOD parent nested under this Game Object. Did you modify in any way the child object named  \"{LOD_PARENT_OBJECT_NAME}\". If so then you must delete the LODs manually.", "Ok");
                    return false;
                }
            }

            dataContainer.lodBackup = null;

            try
            {
                // Destroy LOD assets
                DestroyLODAssets(lodParent);
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
            }


            // Destroy the LOD parent
            DestroyImmediate(lodParent.gameObject);

            // Destroy the LOD Group if there is one
            var lodGroup = gameObject.GetComponent<LODGroup>();

            if (lodGroup != null)
            {
                DestroyImmediate(lodGroup);
            }

            return true;
        }

        private static Renderer[] GetChildRenderersForLOD(GameObject forObject)
        {
            var resultRenderers = new List<Renderer>();
            CollectChildRenderersForLOD(forObject.transform, resultRenderers);
            return resultRenderers.ToArray();
        }

        private static void CollectChildRenderersForLOD(Transform transform, List<Renderer> resultRenderers)
        {

            // Collect the renderers of this transform
            var childRenderers = transform.GetComponents<Renderer>();

            resultRenderers.AddRange(childRenderers);

            int childCount = transform.childCount;

            for (int a = 0; a < childCount; a++)
            {

                // Skip children that are not active
                var childTransform = transform.GetChild(a);

                if (!childTransform.gameObject.activeSelf)
                {
                    continue;
                }


                // If the transform have the identical name as to our LOD Parent GO name, then we also skip it
                if (string.Equals(childTransform.name, LOD_PARENT_OBJECT_NAME))
                {
                    continue;
                }


                // Skip children that has a LOD Group or a LOD Generator Helper component
                if (childTransform.GetComponent<LODGroup>() != null)
                {
                    continue;
                }

                /*
                else if (childTransform.GetComponent<LODGeneratorHelper>() != null)
                {
                    continue;
                }
                */

                // Skip the preservation sphere object
                if (childTransform.hideFlags == HideFlags.HideAndDontSave && childTransform.name == "4bbe6110e6faf2b499fcb86cd896c082")
                {
                    continue;
                }

                // Continue recursively through the children of this transform
                CollectChildRenderersForLOD(childTransform, resultRenderers);
            }
        }

        private static void ParentAndResetTransform(Transform transform, Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private static void SaveLODMeshAsset(UnityEngine.Object asset, string gameObjectName, string rendererName, int levelIndex, string meshName, string rootFolderPath)
        {
            gameObjectName = MakeNameSafe(gameObjectName);
            rendererName = MakeNameSafe(rendererName);
            string oldName = meshName;
            meshName = MakeNameSafe(meshName);
            meshName = string.Format("{0:00}_{1}", levelIndex + 1, meshName);   // Level indices are 0 based

            string path;

            //path = $"{rootFolderPath}/LEVEL_{levelIndex + 1}/{rendererName}/{meshName}.mesh";  // Creates folders for each individual mesh
            path = $"{rootFolderPath}/LEVEL_{levelIndex + 1}/{meshName}.mesh";    // No folders for individual meshes
            SaveAsset(asset, path);
        }


        private static void SaveCombinedMeshAsset(UnityEngine.Object asset, string meshName, string rootFolderPath)
        {
            string oldName = meshName;
            meshName = MakeNameSafe(meshName);
            string path;
            path = $"{rootFolderPath}/{meshName}.mesh";  
            SaveAsset(asset, path);
        }


        private static void CreateBackup(GameObject gameObject, GameObject lodParentObject, Renderer[] originalRenderers)
        {
            DataContainer dataContainer = gameObject.GetComponent<PolyFew>().dataContainer;
            dataContainer.lodBackup = new LODBackup();
            dataContainer.lodBackup.OriginalRenderers = originalRenderers;
            dataContainer.lodBackup.lodParentObject = lodParentObject;
        }

        private static void SaveAsset(UnityEngine.Object asset, string path)
        {
#if UNITY_EDITOR
            CreateParentDirectory(path);

            string oldPath = path;
            // Make sure that there is no asset with the same path already
            path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
            UnityEditor.AssetDatabase.CreateAsset(asset, path);
#endif
        }

        private static void CreateParentDirectory(string path)
        {

#if UNITY_EDITOR
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex != -1)
            {

                string parentPath = path.Substring(0, lastSlashIndex);
                if (!UnityEditor.AssetDatabase.IsValidFolder(parentPath))
                {
                    lastSlashIndex = parentPath.LastIndexOf('/');
                    if (lastSlashIndex != -1)
                    {
                        string folderName = parentPath.Substring(lastSlashIndex + 1);
                        string folderParentPath = parentPath.Substring(0, lastSlashIndex);
                        CreateParentDirectory(parentPath);

                        UnityEditor.AssetDatabase.CreateFolder(folderParentPath, folderName);
                    }
                    else
                    {
                        UnityEditor.AssetDatabase.CreateFolder(string.Empty, parentPath);
                    }
                }
            }
#endif
        }

        public static string MakeNameSafe(string name)
        {
            var sb = new System.Text.StringBuilder(name.Length);
            bool lastWasSeparator = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    lastWasSeparator = false;
                    sb.Append(c);
                }
                else if (c == '_' || c == '-')
                {
                    if (!lastWasSeparator)
                    {
                        lastWasSeparator = true;
                        sb.Append(c);
                    }
                }
                else
                {
                    if (!lastWasSeparator)
                    {
                        lastWasSeparator = true;
                        sb.Append('_');
                    }
                }
            }
            return sb.ToString();
        }

        private static string ValidateSaveAssetsPath(string saveAssetsPath)
        {
            if (string.IsNullOrEmpty(saveAssetsPath))
                return null;

            saveAssetsPath = saveAssetsPath.Replace('\\', '/');
            saveAssetsPath = saveAssetsPath.Trim('/');

            if (System.IO.Path.IsPathRooted(saveAssetsPath))
                throw new System.InvalidOperationException("The save assets path cannot be rooted.");
            else if (saveAssetsPath.Length > 100)
                throw new System.InvalidOperationException("The save assets path cannot be more than 100 characters long to avoid I/O issues.");

            // Make the path safe
            var pathParts = saveAssetsPath.Split('/');
            for (int i = 0; i < pathParts.Length; i++)
            {
                pathParts[i] = MakeNameSafe(pathParts[i]);
            }
            saveAssetsPath = string.Join("/", pathParts);

            return saveAssetsPath;
        }

        private static bool DeleteEmptyDirectory(string path)
        {
#if UNITY_EDITOR
            bool deletedAllSubFolders = true;
            var subFolders = UnityEditor.AssetDatabase.GetSubFolders(path);
            for (int i = 0; i < subFolders.Length; i++)
            {
                if (!DeleteEmptyDirectory(subFolders[i]))
                {
                    deletedAllSubFolders = false;
                }
            }

            if (!deletedAllSubFolders)
                return false;

            string[] assetGuids = UnityEditor.AssetDatabase.FindAssets(string.Empty, new string[] { path });
            if (assetGuids.Length > 0)
                return false;

            return UnityEditor.AssetDatabase.DeleteAsset(path);
#else
            return false;
#endif
        }

        private static MeshRenderer CreateStaticLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials)
        {
            var levelGameObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            var levelTransform = levelGameObject.transform;
            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var meshFilter = levelGameObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = levelGameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            //SetupLevelRenderer(meshRenderer, ref level);
            return meshRenderer;
        }

        private static SkinnedMeshRenderer CreateSkinnedLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials, Transform rootBone, Transform[] bones)
        {
            var levelGameObject = new GameObject(name, typeof(SkinnedMeshRenderer));
            var levelTransform = levelGameObject.transform;

            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var skinnedMeshRenderer = levelGameObject.GetComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMesh = mesh;
            skinnedMeshRenderer.sharedMaterials = materials;
            skinnedMeshRenderer.rootBone = rootBone;
            skinnedMeshRenderer.bones = bones;

            return skinnedMeshRenderer;
        }

        private static void DestroyLODAssets(Transform transform)
        {
#if UNITY_EDITOR
            var renderers = transform.GetComponentsInChildren<Renderer>(true);

            if (renderers == null || renderers.Length == 0) { return; }
            int a = 0;

            foreach (var renderer in renderers)
            {

                var meshRenderer = renderer as MeshRenderer;
                var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;

                //Debug.Log($"Deleting LOD Asset {a + 1}/{renderers.Length}   Progress   " + (float)a / renderers.Length);
                EditorUtility.DisplayProgressBar("Destroying LODS", $"Deleting LOD Asset {a + 1}/{renderers.Length}", (float)a / renderers.Length);
                a++;

                if (meshRenderer != null)
                {
                    var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        DestroyLODAsset(meshFilter.sharedMesh);
                    }
                }
                else if (skinnedMeshRenderer != null)
                {
                    DestroyLODAsset(skinnedMeshRenderer.sharedMesh);
                }

                foreach (var material in renderer.sharedMaterials)
                {
                    DestroyLODMaterialAsset(material);
                }

            }

            EditorUtility.DisplayProgressBar("Destroying LODS", $"Deleting LOD Asset {a}/{renderers.Length}", (float)a / renderers.Length);
            EditorUtility.ClearProgressBar();


            // Delete any empty LOD asset directories
            //DeleteEmptyDirectory(LODAssetParentPath.TrimEnd('/'));
#endif
        }

        private static void DestroyLODMaterialAsset(Material material)
        {
            if (material == null)
                return;

#if UNITY_EDITOR
            var shader = material.shader;
            if (shader == null)
                return;

            // We find all texture properties of materials and delete those assets also
            int propertyCount = UnityEditor.ShaderUtil.GetPropertyCount(shader);
            for (int propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                var propertyType = UnityEditor.ShaderUtil.GetPropertyType(shader, propertyIndex);
                if (propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, propertyIndex);
                    var texture = material.GetTexture(propertyName);
                    DestroyLODAsset(texture);
                }
            }

            DestroyLODAsset(material);
#endif
        }

        private static void DestroyLODAsset(UnityEngine.Object asset)
        {
            if (asset == null)
                return;

#if UNITY_EDITOR
            // We only delete assets that we have automatically generated
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (assetPath.StartsWith(LOD_ASSETS_DEFAULT_SAVE_PATH))
            {
                UnityEditor.AssetDatabase.DeleteAsset(assetPath);
            }
#endif
        }

        private static void RestoreBackup(GameObject gameObject)
        {
            LODBackup lodBackup = gameObject.GetComponent<PolyFew>().dataContainer.lodBackup;

            if (lodBackup == null) { return; }

            var originalRenderers = lodBackup.OriginalRenderers;

            if (originalRenderers != null)
            {
                foreach (var renderer in originalRenderers)
                {
                    if (renderer == null) { continue; }

                    renderer.enabled = true;
                }
            }
        }

#endregion From MeshSimplifier



        public static void ParentAndOffsetTransform(Transform transform, Transform parentTransform, Transform originalTransform)
        {
            transform.position = originalTransform.position;
            transform.rotation = originalTransform.rotation;
            transform.localScale = originalTransform.lossyScale;
            transform.SetParent(parentTransform, true);
        }




        public static void SetParametersForSimplifier(UnityMeshSimplifier.MeshSimplifier meshSimplifier)
        {
            meshSimplifier.RecalculateNormals = dataContainer.recalculateNormals;
            meshSimplifier.EnableSmartLink = true;
            meshSimplifier.PreserveUVSeamEdges = dataContainer.preserveUVSeams;
            meshSimplifier.PreserveUVFoldoverEdges = dataContainer.preserveUVFoldover;
            meshSimplifier.PreserveBorderEdges = dataContainer.preserveBorders;
            meshSimplifier.Aggressiveness = dataContainer.aggressiveness;
            meshSimplifier.MaxIterationCount = dataContainer.maxIterations;
            meshSimplifier.RegardCurvature = dataContainer.regardCurvature;
            meshSimplifier.UseSortedEdgeMethod = dataContainer.useEdgeSort;
            meshSimplifier.ClearBlendshapesComplete = dataContainer.clearBlendshapesComplete;
        }



        public static void SetParametersForSimplifier(MeshSimplifier meshSimplifier, LODLevelSettings levelSettings)
        {
            meshSimplifier.RecalculateNormals = levelSettings.recalculateNormals;
            meshSimplifier.EnableSmartLink = true;
            meshSimplifier.PreserveUVSeamEdges = levelSettings.preserveUVSeams;
            meshSimplifier.PreserveUVFoldoverEdges = levelSettings.preserveUVFoldover;
            meshSimplifier.PreserveBorderEdges = levelSettings.preserveBorders;
            meshSimplifier.Aggressiveness = levelSettings.aggressiveness;
            meshSimplifier.MaxIterationCount = levelSettings.maxIterations;
            meshSimplifier.RegardCurvature = levelSettings.regardCurvature;
            meshSimplifier.UseSortedEdgeMethod = levelSettings.useEdgeSort;
            meshSimplifier.ClearBlendshapesComplete = levelSettings.clearBlendshapesComplete;
        }



        public static void DrawHorizontalLine(Color color, int thickness = 2, int verticalPadding = 10, int widthAdder = 20, int xOffset = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(verticalPadding + thickness));
            r.height = thickness;
            r.y += verticalPadding;
            r.x -= xOffset;
            r.width += widthAdder;
            EditorGUI.DrawRect(r, color);
        }


        public static void DrawVerticalLine(Color color, int thickness = 2, int horizontalPadding = 10, int yOffset = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(horizontalPadding + thickness));

            r.width = thickness;
            r.x += horizontalPadding;
            r.y -= yOffset;
            r.height += 20;
            EditorGUI.DrawRect(r, color);
        }



        public static Texture2D MakeColoredTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }



        public static Texture2D CreateTexture2DCopy(Texture2D original)
        {

            Texture2D result = new Texture2D(original.width, original.height);
            result.SetPixels(original.GetPixels());
            result.Apply();
            return result;

        }




        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);

        }







        public static bool IsMeshless(Transform transform)
        {
            if (transform == null) { return true; }

            MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
            SkinnedMeshRenderer sRenderer = transform.GetComponent<SkinnedMeshRenderer>();


            if (meshFilter)
            {
                if (meshFilter.sharedMesh != null) { return false; }
            }

            if (sRenderer && sRenderer.enabled)
            {
                if (sRenderer.sharedMesh != null) { return false; }
            }

            return true;
        }




        public static bool CheckIfFeasible(Transform transform)
        {
            if (transform == null || !transform.gameObject.activeInHierarchy) { return false; }

            MeshRenderer[] meshrenderers = transform.GetComponentsInChildren<MeshRenderer>(true);
            SkinnedMeshRenderer[] sMeshRenderers = null;

            if (meshrenderers == null || meshrenderers.Length == 0)
            {
                sMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);

                if (sMeshRenderers == null || sMeshRenderers.Length == 0)
                { return false; }
            }

            MeshFilter[] filters = transform.GetComponentsInChildren<MeshFilter>(true);

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (filter.sharedMesh != null) { return true; }
                }
            }

            if (sMeshRenderers == null)
            {
                sMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            }

            foreach (var skinnedRenderer in sMeshRenderers)
            {
                if (skinnedRenderer.sharedMesh != null) { return true; }
            }


            return false;

        }



        public static bool IsInbuiltAsset(UnityEngine.Object asset)
        {
            return (AssetDatabase.Contains(asset) && (!AssetDatabase.IsSubAsset(asset) && AssetDatabase.GetAssetPath(asset).ToLower().StartsWith("library")));
        }



        public static List<Mesh> GetAllStaticMeshesUnderObject(GameObject go, bool includeInactive, bool includeInbuilt)
        {
            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>(includeInactive);
            List<Mesh> meshes = new List<Mesh>();

            if (meshFilters == null || meshFilters.Length == 0) { return null; }


            foreach (var filter in meshFilters)
            {
                // Skip the sphere object
                if (filter.gameObject.hideFlags == HideFlags.HideAndDontSave && filter.gameObject.name == "4bbe6110e6faf2b499fcb86cd896c082")
                {
                    continue;
                }

                if (filter.sharedMesh)
                {
                    if (includeInbuilt)
                    {
                        meshes.Add(filter.sharedMesh);
                    }

                    else if (!IsInbuiltAsset(filter.sharedMesh))
                    {
                        meshes.Add(filter.sharedMesh);
                    }
                }

            }

            if (meshes.Count == 0) { return null; }

            return meshes;
        }



        public static List<Mesh> GetAllSkinnedMeshesUnderObject(GameObject go, bool includeInactive, bool includeInbuilt)
        {
            SkinnedMeshRenderer[] sMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive);
            List<Mesh> meshes = new List<Mesh>();

            if (sMeshRenderers == null || sMeshRenderers.Length == 0) { return null; }


            foreach (var renderer in sMeshRenderers)
            {

                // Skip the sphere object
                if (renderer.gameObject.hideFlags == HideFlags.HideAndDontSave && renderer.gameObject.name == "4bbe6110e6faf2b499fcb86cd896c082")
                {
                    continue;
                }

                if (renderer.sharedMesh)
                {
                    if (includeInbuilt)
                    {
                        meshes.Add(renderer.sharedMesh);
                    }

                    else if (!IsInbuiltAsset(renderer.sharedMesh))
                    {
                        meshes.Add(renderer.sharedMesh);
                    }
                }
            }

            if (meshes.Count == 0) { return null; }

            return meshes;
        }



        public static bool SaveAllMeshesUnderObject(GameObject go, Action<Mesh> SavedMeshCallback, string folderPath, bool includeInactive, bool includeInbuilt, bool optimizeMeshes)
        {
            // This excludes the Tolerance sphere
            List<Mesh> staticMeshes = GetAllStaticMeshesUnderObject(go, includeInactive, includeInbuilt);
            List<Mesh> skinnedMeshes = GetAllSkinnedMeshesUnderObject(go, includeInactive, includeInbuilt);

#pragma warning disable

            string actual = folderPath;
            folderPath = FileUtil.GetProjectRelativePath(folderPath);
            if (!folderPath.EndsWith("/")) { folderPath += "/"; }

            int totalIterations = 0;
            int currentIteration = 0;
            string filePath = "";

            if (staticMeshes != null) { totalIterations = staticMeshes.Count; }
            if (skinnedMeshes != null) { totalIterations += skinnedMeshes.Count; }



            if (staticMeshes != null)
            {
                foreach (var mesh in staticMeshes)
                {

                    EditorUtility.DisplayProgressBar("Saving Object", $"Writing static mesh assets to disk {currentIteration + 1}/{totalIterations}", (float)currentIteration / totalIterations);

                    currentIteration++;

                    bool createdAsset = false;

                    try
                    {
                        if (!IsMeshSavedAsAsset(mesh))
                        {
                            if (optimizeMeshes) { UnityEditor.MeshUtility.Optimize(mesh); }

                            //filePath = folderPath + mesh.name + ".asset"; //baw did
                            filePath = folderPath + mesh.name + ".mesh";
                            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
                            AssetDatabase.CreateAsset(mesh, filePath);

                            createdAsset = true;
                        }

                        else { createdAsset = false; }
                    }

                    catch (Exception ex)
                    {
                        EditorUtility.ClearProgressBar();
                        Debug.LogError(ex);
                        return false;
                    }

                    EditorUtility.DisplayProgressBar("Saving Object", $"Writing static mesh assets to disk {currentIteration}/{totalIterations}", (float)(currentIteration) / totalIterations);

                    if (createdAsset) { SavedMeshCallback(mesh); }

                }

            }

            if (skinnedMeshes != null)
            {
                foreach (var mesh in skinnedMeshes)
                {

                    EditorUtility.DisplayProgressBar("Saving Object", $"Writing skinned mesh assets to disk {currentIteration + 1}/{totalIterations}", (float)currentIteration / totalIterations);

                    currentIteration++;

                    bool createdAsset = false;

                    try
                    {
                        if (!IsMeshSavedAsAsset(mesh))
                        {
                            if (optimizeMeshes) { UnityEditor.MeshUtility.Optimize(mesh); }

                            //filePath = folderPath + "static_REDUCED.asset";  //baw did
                            filePath = folderPath + "static_REDUCED.mesh";
                            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
                            AssetDatabase.CreateAsset(mesh, filePath);
                            createdAsset = true;
                        }

                        else { createdAsset = false; }
                    }

                    catch (Exception ex)
                    {
                        EditorUtility.ClearProgressBar();
                        Debug.LogError(ex);
                        return false;
                    }

                    if (createdAsset) { SavedMeshCallback(mesh); }

                }

                EditorUtility.DisplayProgressBar("Saving Object", $"Writing skinned mesh assets to disk {currentIteration}/{totalIterations}", (float)(currentIteration) / totalIterations);

            }

            EditorUtility.ClearProgressBar();

            if (staticMeshes == null && skinnedMeshes == null) { return false; }
            else { return true; }

        }





        public static bool SaveAllMeshes(List<Mesh> meshes, string defaultSavePath, bool optimizeMeshes, bool generateUV2, Action<string> ErrorCallback)
        {

            string folderPath = EditorUtility.OpenFolderPanel("Save the mesh assets", defaultSavePath, "");


            if (String.IsNullOrWhiteSpace(folderPath))
            {
                ErrorCallback("Failed to save mesh because no path was chosen.");
                return false;
            }

            if (!UtilityServices.IsPathInAssetsDir(folderPath))
            {
                ErrorCallback("Failed to save mesh because the chosen path is not valid.The path must point to a directory that exists in the project's Assets folder.");
                return false;
            }

#pragma warning disable

            string actual = folderPath;
            folderPath = FileUtil.GetProjectRelativePath(folderPath);


            if (!folderPath.EndsWith("/")) { folderPath += "/"; }


            if (meshes == null)
            {
                ErrorCallback("Failed to save meshes because the provided list is empty.");
                return false;
            }


            string filePath = "";
            int totalIterations = meshes.Count;
            int currentIteration = 0;




            foreach (var mesh in meshes)
            {
                currentIteration++;

                if (mesh == null) { continue; }

                if(generateUV2)
                {
                    EditorUtility.DisplayProgressBar("Saving Object", $"Generating UV2 and writing mesh assets to disk {currentIteration}/{meshes.Count}", (float)(currentIteration - 1) / totalIterations);
                }
                else
                {
                    EditorUtility.DisplayProgressBar("Saving Object", $"Writing mesh assets to disk {currentIteration}/{meshes.Count}", (float)(currentIteration - 1) / totalIterations);
                }

                try
                {
                    if (!IsMeshSavedAsAsset(mesh))
                    {
                        if (optimizeMeshes) { UnityEditor.MeshUtility.Optimize(mesh); }

                        if (generateUV2)
                        {
                            if (UtilityServices.HasUV2(mesh))
                            {
                                Debug.LogWarning($"Mesh \"{mesh.name}\" already had a secondary uv set so we didn't generate a new one. For performance reasons you should disable \"Generate UV2\" option for meshes that already contain the secondary uv set.");
                            }
                            else
                            {
                                UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
                            }
                        }

                        string meshName = UtilityServices.MakeNameSafe(mesh.name) + ".mesh";
                        filePath = folderPath + meshName;
                        filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
                        AssetDatabase.CreateAsset(mesh, filePath);
                    }

                    else { }

                }

                catch (Exception ex)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError(ex);
                    return false;
                }

            }

            if (generateUV2)
            {
                EditorUtility.DisplayProgressBar("Saving Object", $"Generating UV2 and writing mesh assets to disk {currentIteration}/{meshes.Count}", (float)(currentIteration) / totalIterations);
            }
            else
            {
                EditorUtility.DisplayProgressBar("Saving Object", $"Writing mesh assets to disk {currentIteration}/{meshes.Count}", (float)(currentIteration) / totalIterations);
            }


            EditorUtility.ClearProgressBar();

            return true;

        }



        public static bool SaveMesh(Mesh mesh, string defaultSavePath, bool optimizeMesh, Action<string> ErrorCallback)
        {

            string folderPath = EditorUtility.OpenFolderPanel("Save the mesh asset", defaultSavePath, "");


            if (String.IsNullOrWhiteSpace(folderPath))
            {
                ErrorCallback("Failed to save mesh because no path was chosen.");
                return false;
            }

            if (!UtilityServices.IsPathInAssetsDir(folderPath))
            {
                ErrorCallback("Failed to save mesh because the chosen path is not valid.The path must point to a directory that exists in the project's Assets folder.");
                return false;
            }


            string actual = folderPath;
            folderPath = FileUtil.GetProjectRelativePath(folderPath);


            if (!folderPath.EndsWith("/")) { folderPath += "/"; }

            string filePath = "";

            if (mesh == null)
            {
                ErrorCallback("Failed to save mesh because the mesh specified is null.");
                return false;
            }

            EditorUtility.DisplayProgressBar("Saving Object", "Writing mesh asset to disk", 0);

            try
            {
                if (!IsMeshSavedAsAsset(mesh))
                {
                    if (optimizeMesh) { UnityEditor.MeshUtility.Optimize(mesh); }

                    //filePath = folderPath + mesh.name + ".asset"; //baw did
                    string meshName = UtilityServices.MakeNameSafe(mesh.name) + ".mesh";
                    filePath = folderPath + meshName;
                    filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
                    AssetDatabase.CreateAsset(mesh, filePath);
                }
            }

            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                ErrorCallback("Failed to save mesh: " + ex.ToString());
                return false;
            }

            EditorUtility.DisplayProgressBar("Saving Object", "Writing mesh asset to disk", 1);


            EditorUtility.ClearProgressBar();

            return true;

        }



        public static bool OverwriteAssetWith(UnityEngine.Object toOverwrite, UnityEngine.Object overwriteWith, bool refreshAssetdb)
        {
            try
            {
                string toOverwritePath = AssetDatabase.GetAssetPath(toOverwrite);
                string toOverwriteName = toOverwrite.name;
                string overwithName = overwriteWith.name;
                string guid = GUID.Generate().ToString();
                string tempFolderPath = Path.GetDirectoryName(toOverwritePath) + Path.DirectorySeparatorChar + guid + Path.DirectorySeparatorChar;

                AssetDatabase.CreateFolder(Path.GetDirectoryName(toOverwritePath), guid);
                AssetDatabase.CreateAsset(overwriteWith, tempFolderPath + toOverwriteName + ".mesh");
                var bytes = File.ReadAllBytes(AssetDatabase.GetAssetPath(overwriteWith));
                string metaPath = Path.GetDirectoryName(tempFolderPath) + ".meta";
                FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(tempFolderPath));
                FileUtil.DeleteFileOrDirectory(metaPath);

                File.WriteAllBytes(toOverwritePath, bytes);
                if (refreshAssetdb) { AssetDatabase.Refresh(); }

                return true;
            }

            catch (Exception ex)
            {
                Debug.LogError($"Error  {ex}");
                return false;
            }
        }


        public static bool OverwriteAssetWith(string toOverwritePath, UnityEngine.Object overwriteWith, bool refreshAssetdb)
        {
            try
            {
                string toOverwriteName = Path.GetFileNameWithoutExtension(toOverwritePath);
                string overwriteWithName = overwriteWith.name;
                string guid = GUID.Generate().ToString();
                string tempPath = Path.GetDirectoryName(toOverwritePath) + Path.DirectorySeparatorChar + guid + Path.DirectorySeparatorChar + "temp.asset";

                AssetDatabase.CreateFolder(Path.GetDirectoryName(toOverwritePath), guid);
                AssetDatabase.CreateAsset(overwriteWith, tempPath);
                var bytes = File.ReadAllBytes(AssetDatabase.GetAssetPath(overwriteWith));
                string metaPath = Path.GetDirectoryName(tempPath) + Path.DirectorySeparatorChar + "temp.meta";
                FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(tempPath));
                FileUtil.DeleteFileOrDirectory(metaPath);

                if (refreshAssetdb) { AssetDatabase.Refresh(); }
                File.WriteAllBytes(toOverwritePath, bytes);
                AssetDatabase.RenameAsset(toOverwritePath, toOverwriteName);
                if (refreshAssetdb) { AssetDatabase.Refresh(); }

                return true;
            }

            catch (Exception ex)
            {
                Debug.LogError($"Error  {ex}");
                return false;
            }
        }


        public static bool OverwriteAssetWith(string toOverwritePath, string overwriteWithPath, bool refreshAssetdb)
        {
            try
            {
                string toOverwriteName = Path.GetFileNameWithoutExtension(toOverwritePath);
                string overwriteWithName = Path.GetFileNameWithoutExtension(overwriteWithPath);
 

                if (refreshAssetdb) { AssetDatabase.Refresh(); }
                File.WriteAllBytes(toOverwritePath, File.ReadAllBytes(overwriteWithPath));
                AssetDatabase.RenameAsset(toOverwritePath, toOverwriteName);
                if (refreshAssetdb) { AssetDatabase.Refresh(); }

                return true;
            }

            catch (Exception ex)
            {
                Debug.LogError($"Error  {ex}");
                return false;
            }
        }


        public static bool AreAllMeshesSavedAsAssets(GameObject go, bool includeInactive)
        {

            // Theses exclude the tolerance sphere
            List<Mesh> staticMeshes = GetAllStaticMeshesUnderObject(go, includeInactive, true);
            List<Mesh> skinnedMeshes = GetAllSkinnedMeshesUnderObject(go, includeInactive, true);


            if (staticMeshes != null)
            {
                foreach (var mesh in staticMeshes)
                {
                    //Debug.Log("Static Mesh Path  " + AssetDatabase.GetAssetPath(mesh));

                    if (!IsMeshSavedAsAsset(mesh)) { return false; }
                }
            }

            if (skinnedMeshes != null)
            {
                foreach (var mesh in skinnedMeshes)
                {
                    //Debug.Log("Skinned Mesh Path  " + AssetDatabase.GetAssetPath(mesh));

                    if (!IsMeshSavedAsAsset(mesh)) { return false; }
                }
            }


            return true;
        }




        public static bool AreMeshesSavedAsAssets(List<Mesh> meshes)
        {

            if (meshes == null || meshes.Count == 0) { return false; }

            foreach (var mesh in meshes)
            {
                if (!IsMeshSavedAsAsset(mesh)) { return false; }
            }

            return true;
        }




        public static bool AreMeshesSavedAsAssets(DataContainer.ObjectMeshPair objMeshPairs)
        {

            if (objMeshPairs == null || objMeshPairs.Count == 0) { return false; }

            foreach (var kvp in objMeshPairs)
            {
                if (kvp.Key == null || kvp.Value.mesh == null) { continue; }

                if (!IsMeshSavedAsAsset(kvp.Value.mesh)) { return false; }
            }

            return true;
        }




        public static bool IsMeshSavedAsAsset(GameObject go)
        {
            Mesh staticMesh = go.GetComponent<MeshFilter>().sharedMesh;
            Mesh skinnedMesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;


            if (staticMesh == null && skinnedMesh == null) { return true; }

            if (staticMesh != null)
            {
                if (!IsMeshSavedAsAsset(staticMesh)) { return false; }
            }

            if (skinnedMesh != null)
            {
                if (!IsMeshSavedAsAsset(skinnedMesh)) { return false; }
            }


            return true;
        }





        public static bool IsMeshSavedAsAsset(Mesh mesh)
        {
            if (mesh == null) { return true; }

            string path = AssetDatabase.GetAssetPath(mesh);
            //return (AssetDatabase.Contains(mesh) && path.ToLower().EndsWith(".asset")); baw did
            return (AssetDatabase.Contains(mesh) && path.ToLower().EndsWith(".mesh"));
        }



        public static Mesh GetReducedMesh(GameObject gameObject, DataContainer.MeshRendererPair mRendererPair)
        {

            if (gameObject == null) { return null; }
            Mesh originalMesh = null;
            Mesh reducedMesh = null;

            if (mRendererPair.attachedToMeshFilter)
            {
                MeshFilter filter = gameObject.GetComponent<MeshFilter>();
                reducedMesh  = filter?.sharedMesh;
                originalMesh = mRendererPair.mesh;

                if (originalMesh != null && reducedMesh != null)
                {
                    if (!reducedMesh.GetHashCode().Equals(originalMesh.GetHashCode()))
                    {
                        return reducedMesh;
                    }
                    else
                    {
                        return null;
                    }
                }

                else { return null; }

            }

            else
            {
                SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                reducedMesh = sRenderer?.sharedMesh;
                originalMesh = mRendererPair.mesh;

                if (originalMesh != null && reducedMesh != null)
                {
                    if (!reducedMesh.GetHashCode().Equals(originalMesh.GetHashCode()))
                    {
                        return reducedMesh;
                    }
                    else
                    {
                        return null;
                    }
                }

                else { return null; }
            }

        }




        public static List<Mesh> GetAllReducedMeshes(DataContainer.ObjectMeshPair objMeshPairs)
        {

            if (objMeshPairs == null || objMeshPairs.Count == 0) { return null; }

            List<Mesh> reducedMeshes = new List<Mesh>();


            foreach (var kvp in objMeshPairs)
            {

                if (kvp.Key == null || kvp.Value.mesh == null) { continue; }

                DataContainer.MeshRendererPair meshRendererPair = kvp.Value;
                GameObject gameObject = kvp.Key;

                if (meshRendererPair.attachedToMeshFilter)
                {
                    MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                    if (filter != null)
                    {
                        Mesh mesh = filter.sharedMesh;
                        if (mesh != null) { reducedMeshes.Add(mesh); }
                    }
                }

                else
                {
                    SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                    if (sRenderer != null)
                    {
                        Mesh mesh = sRenderer.sharedMesh;
                        if (mesh != null) { reducedMeshes.Add(mesh); }
                    }
                }
            }


            if (reducedMeshes.Count == 0) { return null; }

            return reducedMeshes;
        }





        public static List<Mesh> GetMeshesFromPairs(DataContainer.ObjectMeshPair objMeshPairs)
        {

            if (objMeshPairs == null || objMeshPairs.Count == 0) { return null; }


            List<Mesh> originalMeshes = new List<Mesh>();


            foreach (var kvp in objMeshPairs)
            {

                if (kvp.Key == null || kvp.Value.mesh == null) { continue; }

                DataContainer.MeshRendererPair meshRendererPair = kvp.Value;
                GameObject gameObject = kvp.Key;


                if (kvp.Value.mesh != null)
                {
                    originalMeshes.Add(kvp.Value.mesh);
                }
            }


            return originalMeshes;
        }




        public static Tuple<HashSet<Mesh>, DataContainer.ObjectMeshPair> GetUnsavedReducedMeshes(DataContainer.ObjectMeshPair objMeshPairs)
        {

            if (objMeshPairs == null || objMeshPairs.Count == 0) { return null; }


            HashSet<Mesh> unsavedReducedMeshes = new HashSet<Mesh>();
            DataContainer.ObjectMeshPair reducedObjectMeshPairs = new ObjectMeshPair();


            foreach (var kvp in objMeshPairs)
            {

                if (kvp.Key == null || kvp.Value.mesh == null) { continue; }

                DataContainer.MeshRendererPair meshRendererPair = kvp.Value;
                GameObject gameObject = kvp.Key;

                if (meshRendererPair.attachedToMeshFilter)
                {
                    MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                    if (filter != null)
                    {
                        Mesh reducedMesh = filter.sharedMesh;
                        Mesh originalMesh = kvp.Value.mesh;

                        if(reducedMesh != null && originalMesh != null)
                        {
                            if(!reducedMesh.GetHashCode().Equals(originalMesh.GetHashCode()))
                            {
                                if (!IsMeshSavedAsAsset(originalMesh))
                                {
                                    unsavedReducedMeshes.Add(reducedMesh);
                                    reducedObjectMeshPairs.Add(gameObject, new MeshRendererPair(meshRendererPair.attachedToMeshFilter, reducedMesh));
                                }
                            }
                        }
                    }
                }

                else
                {
                    SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                    if (sRenderer != null)
                    {
                        Mesh reducedMesh = sRenderer.sharedMesh;
                        Mesh originalMesh = kvp.Value.mesh;

                        if (reducedMesh != null && originalMesh != null)
                        {
                            if (!reducedMesh.GetHashCode().Equals(originalMesh.GetHashCode()))
                            {
                                if (!IsMeshSavedAsAsset(originalMesh))
                                {
                                    unsavedReducedMeshes.Add(reducedMesh);
                                    reducedObjectMeshPairs.Add(gameObject, new MeshRendererPair(meshRendererPair.attachedToMeshFilter, reducedMesh));
                                }
                            }
                        }
                    }
                }
            }


            if (unsavedReducedMeshes == null || unsavedReducedMeshes.Count == 0) { return null; }

            return Tuple.Create<HashSet<Mesh>, DataContainer.ObjectMeshPair>(unsavedReducedMeshes, reducedObjectMeshPairs);

        }




        public static void GetAllReducedAndOriginalMeshes(DataContainer.ObjectMeshPair objMeshPairs, Action<List<Mesh>, List<Mesh>> MeshesCallback)
        {

            if (objMeshPairs == null || objMeshPairs.Count == 0) { MeshesCallback(null, null); return; }


            List<Mesh> originalMeshes = new List<Mesh>();
            List<Mesh> reducedMeshes = new List<Mesh>();


            foreach (var kvp in objMeshPairs)
            {

                if (kvp.Key == null || kvp.Value.mesh == null) { continue; }

                DataContainer.MeshRendererPair meshRendererPair = kvp.Value;
                GameObject gameObject = kvp.Key;

                if (meshRendererPair.attachedToMeshFilter)
                {
                    MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                    if (filter != null)
                    {
                        Mesh reducedMesh = filter.sharedMesh;
                        Mesh originalMesh = kvp.Value.mesh;

                        if (reducedMesh != null)
                        {
                            reducedMeshes.Add(reducedMesh);
                        }

                        if (originalMesh != null)
                        {
                            originalMeshes.Add(originalMesh);
                        }
                    }
                }

                else
                {
                    SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                    if (sRenderer != null)
                    {
                        Mesh reducedMesh = sRenderer.sharedMesh;
                        Mesh originalMesh = kvp.Value.mesh;

                        if (reducedMesh != null)
                        {
                            reducedMeshes.Add(reducedMesh);
                        }

                        if (originalMesh != null)
                        {
                            originalMeshes.Add(originalMesh);
                        }
                    }
                }
            }


            MeshesCallback(originalMeshes, reducedMeshes);
        }




        public static DataContainer.ObjectMeshPair GetObjectMeshPairs(GameObject forObject, bool includeInactive, bool includeInbuilt)
        {

            DataContainer.ObjectMeshPair objectMeshPairs = new DataContainer.ObjectMeshPair();

            if(forObject == null) { return null; }

            MeshFilter[] meshFilters = forObject.GetComponentsInChildren<MeshFilter>(includeInactive);

            if (meshFilters != null && meshFilters.Length != 0)
            {
                foreach (var filter in meshFilters)
                {
                    if (filter.gameObject.hideFlags == HideFlags.HideAndDontSave && filter.gameObject.name == "4bbe6110e6faf2b499fcb86cd896c082")
                    { continue; }  // Don't save for the tolerance sphere

                    if (filter.sharedMesh)
                    {
                        if (includeInbuilt)
                        {
                            //Debug.Log("Adding From Mesh Filter   "+ filter.sharedMesh.name + "  for gameobject  "+ filter.gameObject.name);
                            DataContainer.MeshRendererPair meshRendererPair = new DataContainer.MeshRendererPair(true, filter.sharedMesh);
                            objectMeshPairs.Add(filter.gameObject, meshRendererPair);
                        }

                        else if (!IsInbuiltAsset(filter.sharedMesh))
                        {
                            DataContainer.MeshRendererPair meshRendererPair = new DataContainer.MeshRendererPair(true, filter.sharedMesh);
                            objectMeshPairs.Add(filter.gameObject, meshRendererPair);
                        }
                    }
                }
            }


            SkinnedMeshRenderer[] sMeshRenderers = forObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive);

            if (sMeshRenderers != null && sMeshRenderers.Length != 0)
            {
                foreach (var renderer in sMeshRenderers)
                {
                    // Don't save for the tolerance sphere
                    if (renderer.gameObject.hideFlags == HideFlags.HideAndDontSave && renderer.gameObject.name == "4bbe6110e6faf2b499fcb86cd896c082")
                    { continue; }

                    if (renderer.sharedMesh)
                    {
                        if (includeInbuilt)
                        {
                            DataContainer.MeshRendererPair meshRendererPair = new DataContainer.MeshRendererPair(false, renderer.sharedMesh);
                            objectMeshPairs.Add(renderer.gameObject, meshRendererPair);
                        }

                        else if (!IsInbuiltAsset(renderer.sharedMesh))
                        {
                            DataContainer.MeshRendererPair meshRendererPair = new DataContainer.MeshRendererPair(false, renderer.sharedMesh);
                            objectMeshPairs.Add(renderer.gameObject, meshRendererPair);
                        }
                    }
                }

            }


            return objectMeshPairs;

        }




        public static HashSet<GameObject> GetPrefabsUnderObject(GameObject forObject, bool includeInactive, bool includeInbuilt)
        {

            MeshFilter[] meshFilters = forObject.GetComponentsInChildren<MeshFilter>(includeInactive);
            HashSet<GameObject> prefabs = new HashSet<GameObject>();


            if (meshFilters != null && meshFilters.Length != 0)
            {
                foreach (var filter in meshFilters)
                {
                    if (filter.gameObject.hideFlags == HideFlags.HideAndDontSave && filter.gameObject.name == "4bbe6110e6faf2b499fcb86cd896c082")
                    { continue; }  // Don't save for the tolerance sphere

                    if (filter.sharedMesh)
                    {
                        PrefabType prefabType = PrefabUtility.GetPrefabType(filter.gameObject);

                        if (includeInbuilt)
                        {
                            if (prefabType != PrefabType.None && prefabType != PrefabType.DisconnectedModelPrefabInstance && prefabType != PrefabType.DisconnectedPrefabInstance && prefabType != PrefabType.MissingPrefabInstance)
                            {
                                prefabs.Add(filter.gameObject);
                            }
                        }

                        else if (!IsInbuiltAsset(filter.sharedMesh))
                        {
                            if (prefabType != PrefabType.None && prefabType != PrefabType.DisconnectedModelPrefabInstance && prefabType != PrefabType.DisconnectedPrefabInstance && prefabType != PrefabType.MissingPrefabInstance)
                            {
                                prefabs.Add(filter.gameObject);
                            }
                        }
                    }
                }
            }


            SkinnedMeshRenderer[] sMeshRenderers = forObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive);

            if (sMeshRenderers != null && sMeshRenderers.Length != 0)
            {
                foreach (var renderer in sMeshRenderers)
                {
                    // Don't save for the tolerance sphere
                    if (renderer.gameObject.hideFlags == HideFlags.HideAndDontSave && renderer.gameObject.name == "4bbe6110e6faf2b499fcb86cd896c082")
                    { continue; }

                    if (renderer.sharedMesh)
                    {
                        PrefabType prefabType = PrefabUtility.GetPrefabType(renderer.gameObject);

                        if (includeInbuilt)
                        {
                            if (prefabType != PrefabType.None && prefabType != PrefabType.DisconnectedModelPrefabInstance && prefabType != PrefabType.DisconnectedPrefabInstance && prefabType != PrefabType.MissingPrefabInstance)
                            {
                                prefabs.Add(renderer.gameObject);
                            }
                        }

                        else if (!IsInbuiltAsset(renderer.sharedMesh))
                        {
                            if (prefabType != PrefabType.None && prefabType != PrefabType.DisconnectedModelPrefabInstance && prefabType != PrefabType.DisconnectedPrefabInstance && prefabType != PrefabType.MissingPrefabInstance)
                            {
                                prefabs.Add(renderer.gameObject);
                            }
                        }
                    }
                }

            }

            return prefabs;

        }


        public static void RestorePolyFewGameObjects(GameObject[] torestore)
        {
            if(torestore == null)
            {
                return;
            }

            foreach (var gameObject in torestore)
            {
                PolyFew[] polyfews = gameObject.GetComponentsInChildren<PolyFew>(true);

                if (polyfews != null && polyfews.Length > 0)
                {
                    foreach (PolyFew polyfew in polyfews)
                    {
                        /*
                        Get Object mesh pairs and restore them if reduction is pending 
                        and reduction strength > 0
                        (IF TEMP MESH ASSIGNED) 
                        check if mesh saved
                        */
                        DataContainer dataContainer = polyfew.dataContainer;
                        GameObject polyfewGameObject = polyfew.gameObject;

                        if (dataContainer != null)
                        {
                            if (dataContainer.reductionPending)
                            {
                                RestoreMeshesFromPairs(dataContainer.objectMeshPairs);
                                dataContainer.triangleCount = CountTriangles(dataContainer.considerChildren, dataContainer.objectMeshPairs, polyfewGameObject);
                                dataContainer.reductionPending = false;
                                dataContainer.reductionStrength = 0;
                            }
                        }
                    }
                }
            }

        }


        public static void RestoreMeshesFromPairs(DataContainer.ObjectMeshPair objectMeshPairs)
        {
            if (objectMeshPairs != null)
            {
                foreach (GameObject gameObject in objectMeshPairs.Keys)
                {
                    if (gameObject != null)
                    {
                        DataContainer.MeshRendererPair meshRendererPair = objectMeshPairs[gameObject];

                        if (meshRendererPair.mesh == null) { continue; }

                        if (meshRendererPair.attachedToMeshFilter)
                        {
                            MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                            if (filter == null) { continue; }

                            //Debug.Log("Is attached to meshfilter  GAMOBJECT:   " + gameObject.name + "  CurrentMesh name:  " + filter.sharedMesh.name + "  set sharedMesh to  " + meshRendererPair.mesh.name);

                            filter.sharedMesh = meshRendererPair.mesh;
                        }

                        else if (!meshRendererPair.attachedToMeshFilter)
                        {
                            SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                            if (sRenderer == null) { continue; }

                            //Debug.Log("Is attached to SkinnedMeshRendere  GAMOBJECT:   " + gameObject.name + "  CurrentMesh name:  " + sRenderer.sharedMesh.name + "  set sharedMesh to  " + meshRendererPair.mesh.name);

                            sRenderer.sharedMesh = meshRendererPair.mesh;
                        }
                    }
                    else
                    {
                        Debug.Log("Invalid reference to gameobnejct");
                    }

                }
            }

            else { Debug.LogError("Object mesh pairs is null"); }

        }




        public static Mesh GetObjectMesh(GameObject go)
        {

            if (go == null) { return null; }

            Mesh mesh = null;

            if (go.GetComponent<MeshFilter>())
            {
                mesh = go.GetComponent<MeshFilter>().sharedMesh;
            }

            else if (go.GetComponent<SkinnedMeshRenderer>())
            {
                mesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            return mesh;
        }




        public static void AssignReducedMesh(GameObject gameObject, Mesh originalMesh, Mesh reducedMesh, bool attachedToMeshfilter, bool assignBindposes)
        {
            if (assignBindposes)
            {
                reducedMesh.bindposes = originalMesh.bindposes;   // Might cause issues
            }

            reducedMesh.name = originalMesh.name.Replace("-POLY_REDUCED", "") + "-POLY_REDUCED";
            reducedMesh.name = reducedMesh.name.Replace("-BLENDSHAPES_SIMPLIFIED", "");

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


        public enum BlendShapeClearType
        {
            CLEAR_ALL_DATA,
            CLEAR_NORMALS,
            CLEAR_TANGENTS
        }

        public static bool SimplifyBlendShapes(ref Mesh forMesh, int? rendererHashCode, BlendShapeClearType clearType)
        {
            if (forMesh == null) { return false; }
            if(forMesh.blendShapeCount == 0) { return false; }
            if(rendererHashCode == null) { return false; }

            Dictionary<String, UnityMeshSimplifier.BlendShapeFrame> blendShapes = new Dictionary<string, UnityMeshSimplifier.BlendShapeFrame>();
            bool alreadyClearedNormals = true;
            bool alreadyClearedTangents = true;
            bool simplifiedSomething = false;

            if (clearType != BlendShapeClearType.CLEAR_ALL_DATA)
            {
                for (int shapeIndex = 0; shapeIndex < forMesh.blendShapeCount; shapeIndex++)
                {
                    for (int frameIndex = 0; frameIndex < forMesh.GetBlendShapeFrameCount(shapeIndex); frameIndex++)
                    {
                        Vector3[] deltaVertices = new Vector3[forMesh.vertexCount];
                        Vector3[] deltaNormals = new Vector3[forMesh.vertexCount];
                        Vector3[] deltaTangents = new Vector3[forMesh.vertexCount];

                        if (!blendShapes.ContainsKey(forMesh.GetBlendShapeName(shapeIndex) + rendererHashCode))
                        {
                            forMesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);
                            blendShapes.Add(forMesh.GetBlendShapeName(shapeIndex) + rendererHashCode, new UnityMeshSimplifier.BlendShapeFrame(forMesh.GetBlendShapeName(shapeIndex) + rendererHashCode, forMesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex), deltaVertices, deltaNormals, deltaTangents, 0));
                            if (deltaNormals != null && deltaNormals.Length > 0) {  alreadyClearedNormals = false; }
                            if (deltaTangents != null && deltaTangents.Length > 0) { alreadyClearedTangents = false; }
                        }
                    }
                }
            }

            if (clearType == BlendShapeClearType.CLEAR_ALL_DATA)
            {
                forMesh.ClearBlendShapes();
                simplifiedSomething = true;
            }

            else if (clearType == BlendShapeClearType.CLEAR_NORMALS)
            {
                if (!alreadyClearedNormals)
                {
                    forMesh.ClearBlendShapes();

                    foreach (UnityMeshSimplifier.BlendShapeFrame blendShape in blendShapes.Values)
                    {
                        Vector3[] deltaVertices = new Vector3[blendShape.deltaVertices.Length];
                        Vector3[] deltaNormals = null;
                        Vector3[] deltaTangents = new Vector3[blendShape.deltaTangents.Length];
                        
                        for (int p = 0; p < blendShape.deltaVertices.Length; p++)
                        {
                            deltaVertices.SetValue(blendShape.deltaVertices[p], p + blendShape.vertexOffset);
                            deltaTangents.SetValue(blendShape.deltaTangents[p], p + blendShape.vertexOffset);
                        }
                        
                        simplifiedSomething = true;
                        forMesh.AddBlendShapeFrame(blendShape.shapeName, blendShape.frameWeight, deltaVertices, deltaNormals, deltaTangents);
                        deltaNormals = new Vector3[blendShape.deltaTangents.Length];
                    }
                }
            }

            else
            {
                if (!alreadyClearedTangents)
                {
                    forMesh.ClearBlendShapes();

                    foreach (UnityMeshSimplifier.BlendShapeFrame blendShape in blendShapes.Values)
                    {

                        Vector3[] deltaVertices = new Vector3[blendShape.deltaVertices.Length];
                        Vector3[] deltaNormals = new Vector3[blendShape.deltaNormals.Length];
                        Vector3[] deltaTangents = null;

                        for (int p = 0; p < blendShape.deltaVertices.Length; p++)
                        {
                            deltaVertices.SetValue(blendShape.deltaVertices[p], p + blendShape.vertexOffset);
                            deltaNormals.SetValue(blendShape.deltaNormals[p], p + blendShape.vertexOffset);
                        }

                        simplifiedSomething = true;
                        forMesh.AddBlendShapeFrame(blendShape.shapeName, blendShape.frameWeight, deltaVertices, deltaNormals, deltaTangents);
                    }
                }
            }



            return simplifiedSomething;
        }


        public static bool IsNonuniformScale(Vector3 prevScale, Vector3 currScale)
        {
            float s1 = Mathf.Abs(prevScale.x - currScale.x);
            float s2 = Mathf.Abs(prevScale.y - currScale.y);
            float s3 = Mathf.Abs(prevScale.z - currScale.z);


            if (Mathf.Approximately(s1, s2) && Mathf.Approximately(s2, s3)) { return false; }

            return true;
        }


        public static int CountTriangles(bool countDeep, DataContainer.ObjectMeshPair objectMeshPairs, GameObject forObject)
        {
            int triangleCount = 0;

            if (objectMeshPairs == null) { return 0; }

            if (countDeep)
            {
                foreach (var item in objectMeshPairs)
                {
                    if (item.Key == null || item.Value == null || item.Value.mesh == null)
                    { continue; }

                    triangleCount += (item.Value.mesh.triangles.Length) / 3;
                }
            }
            else if (forObject != null)
            {
                if (objectMeshPairs.ContainsKey(forObject))
                {
                    MeshRendererPair mRendererPair = objectMeshPairs[forObject];

                    if (mRendererPair == null || mRendererPair.mesh == null)
                    {
                        return 0;
                    }

                    triangleCount = (mRendererPair.mesh.triangles.Length / 3);
                }
            }

            return triangleCount;
        }


        public static Vector3 GetClosestVertex(Vector3 point, Mesh mesh, Transform obj)
        {

            if (mesh == null) { return Vector3.zero; }


            Vector3 closestVertex = Vector3.zero;
            float minDist = Mathf.Infinity;
            point = obj.InverseTransformPoint(point);

            for (int a = 0; a < mesh.vertexCount; a++)
            {
                Vector3 vertexPos = mesh.vertices[a];
                float distance = Vector3.Distance(vertexPos, point);

                if (distance < minDist)
                {
                    minDist = distance;
                    closestVertex = vertexPos;
                }
            }

            return obj.TransformPoint(closestVertex);

        }




        public static PointState GetPointSphereRelation(Vector3 sphereCenter, float sphereRadius, Vector3 point)
        {
            int x1 = (int)Math.Pow((point.x - sphereCenter.x), 2);
            int y1 = (int)Math.Pow((point.y - sphereCenter.y), 2);
            int z1 = (int)Math.Pow((point.z - sphereCenter.z), 2);

            float dist = (x1 + y1 + z1);


            // distance btw centre 
            // and point is less  
            // than radius 

            if (dist < (sphereRadius * sphereRadius)) { return PointState.LIES_INSIDE; }

            // distance btw centre 
            // and point is  
            // equal to radius 
            else if (dist == (sphereRadius * sphereRadius)) { return PointState.LIES_OVER; }

            // distance btw center  
            // and point is greater 
            // than radius 
            else { return PointState.LIES_OUTSIDE; }

        }



        public enum PointState
        {
            LIES_OUTSIDE,
            LIES_INSIDE,
            LIES_OVER
        }



        public static Vector3 GetSnapPoint(Vector3 position, Quaternion rotation, Vector3 snapVector, Vector3 dragDirection, HandleOrientation handlesOrientation)
        {

            var selectedControl = HandleControlsUtility.handleControls.GetCurrentSelectedControl();
            Vector3 result = Vector3.zero;

            if (handlesOrientation == HandleOrientation.globalAligned)
            {
                rotation = Quaternion.identity;
            }


            if (selectedControl == HandleControlsUtility.HandleControls.xAxisMoveHandle)
            {
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.yAxisMoveHandle)
            {
                result = GetYSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.zAxisMoveHandle)
            {
                result = GetZSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.xyAxisMoveHandle)
            {
                Vector3 localAxisDir = GetXaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.xAxisMoveHandle;
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);


                localAxisDir = GetYaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.yAxisMoveHandle;
                result = GetYSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.yzAxisMoveHandle)
            {
                Vector3 localAxisDir = GetYaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.yAxisMoveHandle;
                result = GetYSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);

                localAxisDir = GetZaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.zAxisMoveHandle;
                result = GetZSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.xzAxisMoveHandle)
            {
                Vector3 localAxisDir = GetXaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.xAxisMoveHandle;
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);

                localAxisDir = GetZaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.zAxisMoveHandle;
                result = GetZSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.allAxisMoveHandle)
            {
                Vector3 localAxisDir = GetXaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.xAxisMoveHandle;
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);

                localAxisDir = GetYaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.yAxisMoveHandle;
                result = GetXSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);

                localAxisDir = GetZaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.zAxisMoveHandle;
                result = GetXSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            return (result);

        }



        private static Vector3 GetXSnappedPos(Vector3 position, Quaternion rotation, Vector3 dragDirection, Vector3 snapVector, HandleControlsUtility.HandleControls selectedControl)
        {


            Vector3 result = Vector3.zero;
            Vector3 localAxisDir = GetXaxisinWorld(rotation);
            float dot = Vector3.Dot(localAxisDir, dragDirection);
            float angle = Vector3.Angle(dragDirection, localAxisDir);
            if (dot < 0) { localAxisDir *= -1; }

            result = position + (snapVector.x * localAxisDir);

            if (dot >= 0 && dot <= 1) { result = position; }

            return result;
        }


        private static Vector3 GetYSnappedPos(Vector3 position, Quaternion rotation, Vector3 dragDirection, Vector3 snapVector, HandleControlsUtility.HandleControls selectedControl)
        {
            Vector3 result = Vector3.zero;
            Vector3 localAxisDir = GetYaxisinWorld(rotation);
            float dot = Vector3.Dot(localAxisDir, dragDirection);

            if (dot < 0) { localAxisDir *= -1; }

            result = position + (snapVector.y * localAxisDir);

            if (dot >= 0 && dot <= 1f) { result = position; }

            return result;
        }


        private static Vector3 GetZSnappedPos(Vector3 position, Quaternion rotation, Vector3 dragDirection, Vector3 snapVector, HandleControlsUtility.HandleControls selectedControl)
        {
            Vector3 result = Vector3.zero;
            Vector3 localAxisDir = GetZaxisinWorld(rotation);
            float dot = Vector3.Dot(localAxisDir, dragDirection);

            if (dot < 0) { localAxisDir *= -1; }

            result = position + (snapVector.z * localAxisDir);

            if (dot >= 0 && dot <= 1f) { result = position; }

            return result;
        }




        public static Vector3? CorrectHandleValues(Vector3 pointToCorrect, Vector3 oldValueOfPoint)
        {

            if (HandleControlsUtility.handleControls == null) { return null; }


            Vector3 corrected = Vector3.zero;

            using (HandleControlsUtility handleControls = HandleControlsUtility.handleControls)
            {
                switch (handleControls.GetCurrentSelectedControl())
                {
                    case HandleControlsUtility.HandleControls.xAxisMoveHandle:
                        corrected = new Vector3(pointToCorrect.x, oldValueOfPoint.y, oldValueOfPoint.z);
                        break;

                    case HandleControlsUtility.HandleControls.yAxisMoveHandle:
                        corrected = new Vector3(oldValueOfPoint.x, pointToCorrect.y, oldValueOfPoint.z);
                        break;

                    case HandleControlsUtility.HandleControls.zAxisMoveHandle:
                        corrected = new Vector3(oldValueOfPoint.x, oldValueOfPoint.y, pointToCorrect.z);
                        break;

                    case HandleControlsUtility.HandleControls.xyAxisMoveHandle:
                        corrected = new Vector3(pointToCorrect.x, pointToCorrect.y, oldValueOfPoint.z);
                        break;

                    case HandleControlsUtility.HandleControls.xzAxisMoveHandle:
                        corrected = new Vector3(pointToCorrect.x, oldValueOfPoint.y, pointToCorrect.z);
                        break;

                    case HandleControlsUtility.HandleControls.yzAxisMoveHandle:
                        corrected = new Vector3(oldValueOfPoint.x, pointToCorrect.y, pointToCorrect.z);
                        break;
                }
            }

            return corrected;

        }




        public void RunAfter(Action command, YieldInstruction yieldInstruction)
        {
            this.StartCoroutine(CommandEnumerator(command, yieldInstruction));
        }



        public static Vector3 GetXaxisinWorld(Quaternion rotation)
        {
            return rotation * Vector3.right;
        }


        public static Vector3 GetYaxisinWorld(Quaternion rotation)
        {
            return rotation * Vector3.up;
        }


        public static Vector3 GetZaxisinWorld(Quaternion rotation)
        {
            return rotation * Vector3.forward;
        }



        public static float CalcEulerSafeAngle(float angle)
        {
            if (angle >= -90 && angle <= 90)
                return angle;
            angle = angle % 180;
            if (angle > 0)
                angle -= 180;
            else
                angle += 180;
            return angle;
        }





        private static IEnumerator CommandEnumerator(Action command, YieldInstruction yieldInstruction)
        {
            yield return yieldInstruction;
            command();
        }



        public static string GetValidFolderPath(string folderPath)
        {
            string path = "";

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                //return "Assets/";
                return "";
            }

            path = FileUtil.GetProjectRelativePath(folderPath);


            if (!AssetDatabase.IsValidFolder(path))
            {
                //return "Assets/";
                return "";
            }

            return path;
        }




        public static bool IsPathInAssetsDir(string folderPath)
        {

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return false;
            }
            
            folderPath = FileUtil.GetProjectRelativePath(folderPath);

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return false;
            }

            return true;
        }



        public static GameObject DuplicateGameObject(GameObject toDuplicate, string newName, bool duplicateFromRoot, bool duplicateChildren)
        {
            if (toDuplicate == null) { return null; }

            GameObject selectedObject = Selection.activeGameObject;
            GameObject duplicate = null;


            if (!selectedObject.GetHashCode().Equals(toDuplicate.GetHashCode()))
            {
                Selection.activeGameObject = toDuplicate;
            }


            GameObject rootParent = (GameObject)PrefabUtility.GetPrefabParent(toDuplicate);
            if (duplicateFromRoot && rootParent) { Selection.activeGameObject = rootParent; }


            SceneView.lastActiveSceneView.Focus();
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

            duplicate = Selection.activeGameObject;
            Selection.activeGameObject.name = newName;
            Selection.activeGameObject = selectedObject;

            if (!duplicateChildren)
            {

                while (duplicate.transform.childCount > 0)
                {
                    DestroyImmediate(duplicate.transform.GetChild(0).gameObject);
                }

            }

            duplicate.transform.parent = null;

            return duplicate;
        }



        public static Tuple<GameObject[], GameObject[]> DuplicateMultipleSelected()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length <= 1) { return null; }

            GameObject[] oldSelected = Selection.gameObjects.ToArray();
            GameObject[] duplicated = null;

            SceneView.lastActiveSceneView.Focus();
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

            duplicated = Selection.gameObjects;
          
            return Tuple.Create<GameObject[], GameObject[]>(oldSelected, duplicated);
        }



        public static ChildStateTuple[] SaveChildrenStates(GameObject forObject)
        {

            var children = GetTopLevelChildren(forObject.transform);

            ChildStateTuple[] childrenStates = new ChildStateTuple[children.Length];

            for (int a = 0; a < children.Length; a++)
            {
                childrenStates[a].transform = children[a];
                childrenStates[a].position = children[a].position;
                childrenStates[a].rotation = children[a].rotation;
            }

            return childrenStates;

        }




        /// <summary> Restores the children states to the ones before pivot modification. </summary>

        public static void RestoreChildrenStates(ChildStateTuple[] childStates)
        {

            if (childStates == null) { return; }

            for (int a = 0; a < childStates.Length; a++)
            {
                if (childStates[a].transform == null) { continue; }

                childStates[a].transform.position = childStates[a].position;
                childStates[a].transform.rotation = childStates[a].rotation;
            }

        }




        public static ColliderState SaveColliderState(GameObject forObject)
        {

            Collider selectedObjectCollider = forObject.GetComponent<Collider>();
            Transform selectedTransform = forObject.transform;
            ColliderState colliderState = new ColliderState();

            if (selectedObjectCollider)
            {
                if (selectedObjectCollider is BoxCollider)
                {
                    colliderState.center = selectedTransform.TransformPoint(((BoxCollider)selectedObjectCollider).center);
                    colliderState.type = ColliderType.BoxCollider;
                }
                else if (selectedObjectCollider is CapsuleCollider)
                {
                    colliderState.center = selectedTransform.TransformPoint(((CapsuleCollider)selectedObjectCollider).center);
                    colliderState.type = ColliderType.CapsuleCollider;
                }
                else if (selectedObjectCollider is SphereCollider)
                {
                    colliderState.center = selectedTransform.TransformPoint(((SphereCollider)selectedObjectCollider).center);
                    colliderState.type = ColliderType.SphereCollider;
                }
                else if (selectedObjectCollider is MeshCollider)
                {
                    colliderState.type = ColliderType.MeshCollider;
                    //colliderState.center = selectedTransform.TransformPoint(((MeshCollider)selectedObjectCollider).bounds.center);
                }
            }

            return colliderState;

        }





        /// <summary> Restore the collider orientation.</summary>
        public static void RestoreColliderState(GameObject forObject, ColliderState colliderState)
        {


            Collider selectedObjectCollider = forObject.GetComponent<Collider>();
            Transform selectedTransform = forObject.transform;


            if (selectedObjectCollider)
            {
                if (selectedObjectCollider is BoxCollider)
                {
                    if (colliderState.type == ColliderType.BoxCollider)
                    {
                        ((BoxCollider)selectedObjectCollider).center = selectedTransform.InverseTransformPoint(colliderState.center);
                    }
                }
                else if (selectedObjectCollider is CapsuleCollider)
                {
                    if (colliderState.type == ColliderType.CapsuleCollider)
                    {
                        ((CapsuleCollider)selectedObjectCollider).center = selectedTransform.InverseTransformPoint(colliderState.center);
                    }
                }
                else if (selectedObjectCollider is SphereCollider)
                {
                    if (colliderState.type == ColliderType.SphereCollider)
                    {
                        ((SphereCollider)selectedObjectCollider).center = selectedTransform.InverseTransformPoint(colliderState.center);
                    }
                }
                else if (selectedObjectCollider is MeshCollider)
                {

                    /*
                    MeshCollider meshColl = (MeshCollider)selectedObjectCollider;

                    bool isConvex = meshColl.convex;

                    meshColl.convex = false;

                    meshColl.sharedMesh = selectedObjectMesh;

                    if (isConvex)
                    {

                        if (selectedObjectMesh.vertexCount >= 2000)
                        {

                            Debug.Log("<b><i><color=#008000ff> PLEASE WAIT... while the convex property on the mesh collider does some calculations.The editor won't be usable until the MeshCollider finishes its calculations.</color></i></b>");
                            new UtilityServices().RunAfter(() => { meshColl.convex = true; }, new WaitForSeconds(0.2f));
                        }

                        else { meshColl.convex = true; } 

                    }
                    */
                }
            }

        }





        public static Transform[] GetTopLevelChildren(Transform Parent)
        {
            Transform[] Children = new Transform[Parent.childCount];
            for (int a = 0; a < Parent.childCount; a++)
            {
                Children[a] = Parent.GetChild(a);
            }
            return Children;
        }




        public static GameObject CreateTestObj(PrimitiveType type, Vector3 position, Vector3 scale, string name = "")
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.localScale = scale;
            go.transform.position = position;

            if (name != "") { go.name = name; }

            return go;
        }






        private static Vector3 SubtractAngles(Vector3 rotation1, Vector3 rotation2)
        {

            float xDif = 0;
            float yDif = 0;
            float zDif = 0;

            if (AreAnglesSame(rotation1.x, rotation2.x)) { xDif = 0; }
            else { xDif = rotation1.x - rotation2.x; }

            if (AreAnglesSame(rotation1.y, rotation2.y)) { yDif = 0; }
            else { yDif = rotation1.y - rotation2.y; }

            if (AreAnglesSame(rotation1.z, rotation2.z)) { zDif = 0; }
            else { zDif = rotation1.z - rotation2.z; }

            return new Vector3(xDif, yDif, zDif);
        }



        private static bool AreAnglesSame(float angle1, float angle2)
        {

            if (Mathf.Approximately((Mathf.Cos(angle1) * Mathf.Deg2Rad), (Mathf.Cos(angle2) * Mathf.Deg2Rad)))
            {
                if (Mathf.Approximately((Mathf.Sin(angle1) * Mathf.Deg2Rad), (Mathf.Sin(angle2) * Mathf.Deg2Rad)))
                {
                    //Debug.Log("equal");
                    return true;
                }
            }

            return false;
        }


        public static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            return angles;
        }


        static float NormalizeAngle(float angle)
        {
            while (angle > 180)
                angle -= 360;

            return angle;
        }


        public static Vector3 Absolute(Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }



        public static float SetAndReturnFloatPref(string name, float val)
        {
            EditorPrefs.SetFloat(name, val);
            return val;
        }


        public static int SetAndReturnIntPref(string name, int val)
        {
            EditorPrefs.SetInt(name, val);
            return val;
        }


        public static bool SetAndReturnBoolPref(string name, bool val)
        {
            EditorPrefs.SetBool(name, val);
            return val;
        }


        public static string SetAndReturnStringPref(string name, string val)
        {
            EditorPrefs.SetString(name, val);
            return val;
        }


        public static Vector3 SetAndReturnVectorPref(string nameX, string nameY, string nameZ, Vector3 value)
        {
            EditorPrefs.SetFloat(nameX, value.x);
            EditorPrefs.SetFloat(nameY, value.y);
            EditorPrefs.SetFloat(nameZ, value.z);

            return value;
        }




        public enum ColliderType
        {
            BoxCollider,
            SphereCollider,
            CapsuleCollider,
            MeshCollider,
            None
        }




        public static float Average(params float[] list)
        {

            if (list == null || list.Length == 0) { return 0; }

            float sum = 0;
            float count = list.Length;

            foreach (var num in list) { sum += num; }

            return sum / count;
        }




        public static GameObject FindObjectFromTags(string goName, string tag)
        {
            var list = GameObject.FindGameObjectsWithTag(tag);

            foreach (var item in list)
            {
                if (item.name.Equals(goName)) { return item; }
            }

            return null;
        }



        public static int GetFirstNDigits(int number, int N)
        {
            // this is for handling negative numbers, we are only insterested in postitve number 
            number = Math.Abs(number);

            // special case for 0 as Log of 0 would be infinity 
            if (number == 0) { return number; }

            int numberOfDigits = (int)Math.Floor(Math.Log10(number) + 1);

            if (numberOfDigits >= N)
            {
                return (int)Math.Truncate((number / Math.Pow(10, numberOfDigits - N)));
            }

            else { return number; }

        }


        /*
        public static GameObject DuplicateGameObject(string newName, bool duplicateFromRoot, bool duplicateChildren)
        {
            if (Selection.activeGameObject == null) { return null; }

            GameObject selectedObject = Selection.activeGameObject;
            GameObject duplicate = null;

            string name = Selection.activeGameObject.name;

            GameObject rootParent = (GameObject)PrefabUtility.GetPrefabParent(Selection.activeGameObject);
            if (duplicateFromRoot) { Selection.activeGameObject = rootParent; }


            SceneView.lastActiveSceneView.Focus();
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

            duplicate = Selection.activeGameObject;
            Selection.activeGameObject.name = newName;
            Selection.activeGameObject = selectedObject;

            if (!duplicateChildren)
            {
                foreach (Transform child in duplicate.transform) { DestroyImmediate(child.gameObject); }
            }

            return duplicate;
        }
        */



        public static Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary
            (
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear
            );

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }



        public static void CombineSkinnedMeshes(Transform transform)
        {
            SkinnedMeshRenderer[] smRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            List<Transform> bones = new List<Transform>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            List<Texture2D> textures = new List<Texture2D>();
            int numSubs = 0;

            foreach (SkinnedMeshRenderer smr in smRenderers)
                numSubs += smr.sharedMesh.subMeshCount;

            int[] meshIndex = new int[numSubs];
            int boneOffset = 0;
            for (int s = 0; s < smRenderers.Length; s++)
            {
                SkinnedMeshRenderer smr = smRenderers[s];

                BoneWeight[] meshBoneweight = smr.sharedMesh.boneWeights;

                // May want to modify this if the renderer shares bones as unnecessary bones will get added.
                foreach (BoneWeight bw in meshBoneweight)
                {
                    BoneWeight bWeight = bw;

                    bWeight.boneIndex0 += boneOffset;
                    bWeight.boneIndex1 += boneOffset;
                    bWeight.boneIndex2 += boneOffset;
                    bWeight.boneIndex3 += boneOffset;

                    boneWeights.Add(bWeight);
                }
                boneOffset += smr.bones.Length;

                Transform[] meshBones = smr.bones;
                foreach (Transform bone in meshBones)
                    bones.Add(bone);

                if (smr.sharedMaterial.mainTexture != null)
                    textures.Add(smr.sharedMaterial.mainTexture as Texture2D);

                CombineInstance ci = new CombineInstance();
                ci.mesh = smr.sharedMesh;
                meshIndex[s] = ci.mesh.vertexCount;
                ci.transform = smr.transform.localToWorldMatrix;
                combineInstances.Add(ci);

                UnityEngine.Object.DestroyImmediate(smr.gameObject);
            }

            List<Matrix4x4> bindposes = new List<Matrix4x4>();

            for (int b = 0; b < bones.Count; b++)
            {
                if(bones[b] == null) { continue; }
                bindposes.Add(bones[b].worldToLocalMatrix * transform.worldToLocalMatrix);
            }

            SkinnedMeshRenderer r = transform.gameObject.AddComponent<SkinnedMeshRenderer>();
            r.sharedMesh = new Mesh();
            r.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
            
            Texture2D skinnedMeshAtlas = new Texture2D(128, 128);
            Rect[] packingResult = skinnedMeshAtlas.PackTextures(textures.ToArray(), 0);
            Vector2[] originalUVs = r.sharedMesh.uv;
            Vector2[] atlasUVs = new Vector2[originalUVs.Length];

            int rectIndex = 0;
            int vertTracker = 0;
            for (int i = 0; i < atlasUVs.Length; i++)
            {
                atlasUVs[i].x = Mathf.Lerp(packingResult[rectIndex].xMin, packingResult[rectIndex].xMax, originalUVs[i].x);
                atlasUVs[i].y = Mathf.Lerp(packingResult[rectIndex].yMin, packingResult[rectIndex].yMax, originalUVs[i].y);

                if (i >= meshIndex[rectIndex] + vertTracker)
                {
                    vertTracker += meshIndex[rectIndex];
                    rectIndex++;
                }
            }

            Material combinedMat = new Material(Shader.Find("Diffuse"));
            combinedMat.mainTexture = skinnedMeshAtlas;
            r.sharedMesh.uv = atlasUVs;
            r.sharedMaterial = combinedMat;

            r.bones = bones.ToArray();
            r.sharedMesh.boneWeights = boneWeights.ToArray();
            r.sharedMesh.bindposes = bindposes.ToArray();
            r.sharedMesh.RecalculateBounds();

        }




        public static void ChangeMeshReadability(Mesh mesh, bool markReadible, bool makePersistent)
        {
            if (mesh == null) { return; }

            if (!AssetDatabase.Contains(mesh))
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(mesh);

            ModelImporter importerForAsset = ModelImporter.GetAtPath(assetPath) as ModelImporter;

            if (importerForAsset == null)
            {
                return;
            }

            bool prevReadibilityState = mesh.isReadable;

            importerForAsset.isReadable = markReadible;

            bool newReadibilityState = mesh.isReadable;


            if(prevReadibilityState == newReadibilityState)
            {
                importerForAsset.SaveAndReimport();
            }

            if (makePersistent)
            {
                EditorUtility.SetDirty(mesh);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }


        }


        public static Transform GetTopMostParent(Transform forObject)
        {
            Transform topLevelParent = forObject;

            while (topLevelParent.parent != null) { topLevelParent = topLevelParent.parent; }

            return topLevelParent;
        }


        public static Transform GetTopLevelParent(Transform forObject)
        {
            Transform topLevelParent = forObject;

            if(forObject == null) { return null; }

            while (topLevelParent.parent != null)
            {
                if(topLevelParent.parent.parent == null) { return topLevelParent; }

                topLevelParent = topLevelParent.parent;
            }

            return topLevelParent;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Transform GetTopLevelSelectedParent(Transform forObject, HashSet<Transform> allSelectedObjects)
        {

            Transform topLevelParent = forObject;
            Transform topSelectedParent = forObject;

            while (topLevelParent.parent != null)
            {
                topLevelParent = topLevelParent.parent;
                if(allSelectedObjects.Contains(topLevelParent)) { topSelectedParent = topLevelParent; }
            }

            return topSelectedParent;
        }



        public static void FillTextureWithColor(ref Texture2D toFill, Color color)
        {
            if(toFill == null) { return; }

            var fillColorArray = toFill.GetPixels();

            for (var a = 0; a < fillColorArray.Length; ++a)
            {
                fillColorArray[a] = color;
            }

            toFill.SetPixels(fillColorArray);

            toFill.Apply();
        }


        public static bool HasUV2(Mesh mesh)
        {
            bool hasUV2 = false;

            if(mesh == null) { return false; }

#if UNITY_2019_3_OR_NEWER
            if(mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1))
            {
                hasUV2 = true;
            }
#else
            List<Vector2> uv2 = new List<Vector2>();
            mesh.GetUVs(1, uv2);

            if (uv2 != null && uv2.Count > 0)
            {
                hasUV2 = true;
            }
#endif

            return hasUV2;
        }


        #region OBJ_EXPORT_IMPORT


        /*
         ======================================================================================
         |	    Special thanks to aaro4130 for the Unity3D Scene OBJ Exporter
         |      This section would not have been made possible or would have been partial 
         |      without his works.
         |
         |      Do check out: 
         |      https://assetstore.unity.com/packages/tools/utilities/scene-obj-exporter-22250
         |  
         ======================================================================================
        */


        public class OBJExporterImporter
        {

#region OBJ_EXPORT

            private bool applyPosition = true;
            private bool applyRotation = true;
            private bool applyScale = true;
            private bool generateMaterials = true;
            private bool exportTextures = true;
            private string exportPath;
            private MeshFilter meshFilter;

            private Mesh meshToExport;
            private MeshRenderer meshRenderer;



            public OBJExporterImporter() { }



            public class OBJExportOptions
            {

                public readonly bool applyPosition = true;
                public readonly bool applyRotation = true;
                public readonly bool applyScale = true;
                public readonly bool generateMaterials = true;
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



            private void InitializeExporter(GameObject toExport, string exportPath, OBJExportOptions exportOptions)
            {
                this.exportPath = exportPath;


                if (string.IsNullOrWhiteSpace(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }

                else
                {
                    exportPath = Path.GetFullPath(exportPath);
                    if (exportPath[exportPath.Length - 1] == '\\') { exportPath = exportPath.Remove(exportPath.Length - 1); }
                    else if (exportPath[exportPath.Length - 1] == '/') { exportPath = exportPath.Remove(exportPath.Length - 1); }
                }

                if (!System.IO.Directory.Exists(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }

                if (toExport == null)
                {
                    throw new ArgumentNullException("toExport", "Please provide a GameObject to export as OBJ file.");
                }


                meshRenderer = toExport.GetComponent<MeshRenderer>();
                meshFilter = toExport.GetComponent<MeshFilter>();

                if (meshRenderer == null)
                {

                }

                else
                {
                    if (meshRenderer.isPartOfStaticBatch)
                    {
                        throw new InvalidOperationException("The provided object is static batched. Static batched object cannot be exported. Please disable it before trying to export the object.");
                    }
                }

                if (meshFilter == null)
                {
                    throw new InvalidOperationException("There is no MeshFilter attached to the provided GameObject.");
                }

                else
                {
                    meshToExport = meshFilter.sharedMesh;

                    if (meshToExport == null || meshToExport.triangles == null || meshToExport.triangles.Length == 0)
                    {
                        throw new InvalidOperationException("The MeshFilter on the provided GameObject has invalid or no mesh at all.");
                    }
                }


                if (exportOptions != null)
                {
                    applyPosition = exportOptions.applyPosition;
                    applyRotation = exportOptions.applyRotation;
                    applyScale = exportOptions.applyScale;
                    generateMaterials = exportOptions.generateMaterials;
                    exportTextures = exportOptions.exportTextures;
                }

            }


            private void InitializeExporter(Mesh toExport, string exportPath)
            {
                this.exportPath = exportPath;

                if (string.IsNullOrWhiteSpace(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }


                if (!System.IO.Directory.Exists(exportPath))
                {
                    throw new DirectoryNotFoundException("The path provided is non-existant.");
                }


                if (toExport == null)
                {
                    throw new ArgumentNullException("toExport", "Please provide a Mesh to export as OBJ file.");
                }


                meshToExport = toExport;


                if (meshToExport == null || meshToExport.triangles == null || meshToExport.triangles.Length == 0)
                {
                    throw new InvalidOperationException("The MeshFilter on the provided GameObject has invalid or no mesh at all.");
                }

            }



            Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
            {
                return angle * (point - pivot) + pivot;
            }

            Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
            {
                return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
            }



            public void ExportGameObjectToOBJ(GameObject toExport, string exportPath, OBJExportOptions exportOptions = null, Action OnSuccess = null)
            {
                //init stuff
                Dictionary<string, bool> materialCache = new Dictionary<string, bool>();

                //Debug.Log("Exporting OBJ. Please wait.. Starting to export.");


                InitializeExporter(toExport, exportPath, exportOptions);


                //get list of required export things


                string objectName = toExport.gameObject.name;


                //work on export
                StringBuilder sb = new StringBuilder();
                StringBuilder sbMaterials = new StringBuilder();


                if (generateMaterials)
                {
                    sb.AppendLine("mtllib " + objectName + ".mtl");
                }

                int lastIndex = 0;


                if (meshRenderer != null && generateMaterials)
                {
                    Material[] mats = meshRenderer.sharedMaterials;
                    for (int j = 0; j < mats.Length; j++)
                    {
                        Material m = mats[j];
                        if (!materialCache.ContainsKey(m.name))
                        {
                            materialCache[m.name] = true;
                            sbMaterials.Append(MaterialToString(m));
                            sbMaterials.AppendLine();
                        }
                    }
                }

                //export the meshhh :3

                int faceOrder = (int)Mathf.Clamp((toExport.gameObject.transform.lossyScale.x * toExport.gameObject.transform.lossyScale.z), -1, 1);

                //export vector data (FUN :D)!
                foreach (Vector3 vx in meshToExport.vertices)
                {
                    Vector3 v = vx;
                    if (applyScale)
                    {
                        v = MultiplyVec3s(v, toExport.gameObject.transform.lossyScale);
                    }

                    if (applyRotation)
                    {
                        v = RotateAroundPoint(v, Vector3.zero, toExport.gameObject.transform.rotation);
                    }

                    if (applyPosition)
                    {
                        v += toExport.gameObject.transform.position;
                    }

                    v.x *= -1;
                    sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector3 vx in meshToExport.normals)
                {
                    Vector3 v = vx;

                    if (applyScale)
                    {
                        v = MultiplyVec3s(v, toExport.gameObject.transform.lossyScale.normalized);
                    }
                    if (applyRotation)
                    {
                        v = RotateAroundPoint(v, Vector3.zero, toExport.gameObject.transform.rotation);
                    }

                    v.x *= -1;
                    sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector2 v in meshToExport.uv)
                {
                    sb.AppendLine("vt " + v.x + " " + v.y);
                }

                for (int j = 0; j < meshToExport.subMeshCount; j++)
                {
                    if (meshRenderer != null && j < meshRenderer.sharedMaterials.Length)
                    {
                        string matName = meshRenderer.sharedMaterials[j].name;
                        sb.AppendLine("usemtl " + matName);
                    }
                    else
                    {
                        sb.AppendLine("usemtl " + objectName + "_sm" + j);
                    }

                    int[] tris = meshToExport.GetTriangles(j);

                    for (int t = 0; t < tris.Length; t += 3)
                    {
                        int idx2 = tris[t] + 1 + lastIndex;
                        int idx1 = tris[t + 1] + 1 + lastIndex;
                        int idx0 = tris[t + 2] + 1 + lastIndex;

                        if (faceOrder < 0)
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                        }
                        else
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                        }

                    }
                }

                lastIndex += meshToExport.vertices.Length;


                //write to disk

                string writePath = System.IO.Path.Combine(exportPath, objectName + ".obj");

                System.IO.File.WriteAllText(writePath, sb.ToString());

                if (generateMaterials)
                {
                    writePath = System.IO.Path.Combine(exportPath, objectName + ".mtl");
                    System.IO.File.WriteAllText(writePath, sbMaterials.ToString());
                }

                //export complete, close progress dialog
                OnSuccess?.Invoke();
            }



            public void ExportMeshToOBJ(Mesh mesh, string exportPath)
            {

                InitializeExporter(mesh, exportPath);

                string objectName = meshToExport.name;
                StringBuilder sb = new StringBuilder();
                int lastIndex = 0;
                int faceOrder = 1;

                //export vector data (FUN :D)!
                foreach (Vector3 vx in meshToExport.vertices)
                {
                    Vector3 v = vx;

                    v.x *= -1;

                    sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector3 vx in meshToExport.normals)
                {
                    Vector3 v = vx;

                    v.x *= -1;
                    sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

                }

                foreach (Vector2 v in meshToExport.uv)
                {
                    sb.AppendLine("vt " + v.x + " " + v.y);
                }

                for (int j = 0; j < meshToExport.subMeshCount; j++)
                {

                    sb.AppendLine("usemtl " + objectName + "_sm" + j);

                    int[] tris = meshToExport.GetTriangles(j);

                    for (int t = 0; t < tris.Length; t += 3)
                    {
                        int idx2 = tris[t] + 1 + lastIndex;
                        int idx1 = tris[t + 1] + 1 + lastIndex;
                        int idx0 = tris[t + 2] + 1 + lastIndex;

                        if (faceOrder < 0)
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                        }
                        else
                        {
                            sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                        }

                    }
                }

                lastIndex += meshToExport.vertices.Length;


                //write to disk

                string writePath = System.IO.Path.Combine(exportPath, objectName + ".obj");

                System.IO.File.WriteAllText(writePath, sb.ToString());

            }



            string TryExportTexture(string propertyName, Material m, string exportPath)
            {
                if (m.HasProperty(propertyName))
                {
                    Texture t = m.GetTexture(propertyName);

                    if (t != null)
                    {
                        return ExportTexture((Texture2D)t, exportPath);
                    }
                }

                return "false";
            }


            string ExportTexture(Texture2D t, string exportPath)
            {
                //Debug.Log($"Exporting texture:  {t.name} to path: {exportPath}");

                string textureName = t.name;

                try
                {
                    Color32[] pixels32 = null;

                    try
                    {
                        pixels32 = t.GetPixels32();
                    }

                    catch (UnityException ex)
                    {
                        t = UtilityServices.DuplicateTexture(t);
                        pixels32 = t.GetPixels32();
                    }

                    string qualifiedPath = System.IO.Path.Combine(exportPath, textureName + ".png");
                    Texture2D exTexture = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false);
                    exTexture.SetPixels32(pixels32);

                    System.IO.File.WriteAllBytes(qualifiedPath, exTexture.EncodeToPNG());

                    return qualifiedPath;
                }

                catch (System.Exception ex)
                {
                    Debug.LogWarning("Could not export texture : " + t.name + ". is it readable?");
                    return "null";
                }

            }


            private string ConstructOBJString(int index)
            {
                string idxString = index.ToString();
                return idxString + "/" + idxString + "/" + idxString;
            }


            string MaterialToString(Material m)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("newmtl " + m.name);


                //add properties
                if (m.HasProperty("_Color"))
                {
                    sb.AppendLine("Kd " + m.color.r.ToString() + " " + m.color.g.ToString() + " " + m.color.b.ToString());
                    if (m.color.a < 1.0f)
                    {
                        //use both implementations of OBJ transparency
                        sb.AppendLine("Tr " + (1f - m.color.a).ToString());
                        sb.AppendLine("d " + m.color.a.ToString());
                    }
                }
                if (m.HasProperty("_SpecColor"))
                {
                    Color sc = m.GetColor("_SpecColor");
                    sb.AppendLine("Ks " + sc.r.ToString() + " " + sc.g.ToString() + " " + sc.b.ToString());
                }
                if (exportTextures)
                {
                    //diffuse
                    string exResult = TryExportTexture("_MainTex", m, exportPath);
                    if (exResult != "false")
                    {
                        sb.AppendLine("map_Kd " + exResult);
                    }
                    //spec map
                    exResult = TryExportTexture("_SpecMap", m, exportPath);
                    if (exResult != "false")
                    {
                        sb.AppendLine("map_Ks " + exResult);
                    }
                    //bump map
                    exResult = TryExportTexture("_BumpMap", m, exportPath);
                    if (exResult != "false")
                    {
                        sb.AppendLine("map_Bump " + exResult);
                    }

                }
                sb.AppendLine("illum 2");
                return sb.ToString();
            }



#endregion OBJ_IMPORT


        }


#endregion OBJ_EXPORT_IMPORT

}

}


#endif
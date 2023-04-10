//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static BrainFailProductions.PolyFew.CombiningInformation;

namespace BrainFailProductions.PolyFew
{

    [System.Serializable]
    public class DataContainer 
    {

        [System.Serializable]
        public class TempGameObjectWrapper
        {
            [HideInInspector]
            [SerializeField]
            private GameObject gameObject;

            // This is the unique Id for the current GameObject and it's children hierarchy
            [HideInInspector]
            [SerializeField]
            private int uniqueId;

            private TempGameObjectWrapper(GameObject gameObject)
            {
                if(gameObject == null) { throw new ArgumentNullException(nameof(gameObject)); }
                this.gameObject = gameObject;
                uniqueId = gameObject.GetInstanceID();
            }

            public static implicit operator GameObject(TempGameObjectWrapper gameObjectWrapper) => gameObjectWrapper.gameObject;

            public static explicit operator TempGameObjectWrapper(GameObject gameObject) => new TempGameObjectWrapper(gameObject);

            public override int GetHashCode()
            {
                return uniqueId;
            }


            public override bool Equals(object obj)
            {
                if ((obj == null) || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    return (GetHashCode() == obj.GetHashCode());
                }
            }
        }


        [System.Serializable]
        public class ObjectMeshPair : SerializableDictionary<GameObject, MeshRendererPair> { }


        [System.Serializable]
        public class MeshRendererPair
        {
            public bool attachedToMeshFilter;
            public Mesh mesh;

            public MeshRendererPair(bool attachedToMeshFilter, Mesh mesh)
            {
                this.attachedToMeshFilter = attachedToMeshFilter;
                this.mesh = mesh;
            }

            public void Destruct()
            {
                if(mesh != null)
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            }
        }


        [System.Serializable]
        public class CustomMeshActionStructure
        {
            public MeshRendererPair meshRendererPair;
            public GameObject gameObject;
            public Action action;

            public CustomMeshActionStructure(MeshRendererPair meshRendererPair, GameObject gameObject, Action action)
            {
                this.meshRendererPair = meshRendererPair;
                this.gameObject = gameObject;
                this.action = action;
            }
        }


        [System.Serializable]
        public class ObjectHistory
        {
            public bool isReduceDeep;
            public ObjectMeshPair objectMeshPairs;

            public ObjectHistory(bool isReduceDeep, ObjectMeshPair objectMeshPairs)
            {
                this.isReduceDeep = isReduceDeep;
                this.objectMeshPairs = objectMeshPairs;
            }

            public void Destruct()
            {

                if (objectMeshPairs == null || objectMeshPairs.Count == 0)
                {
                    return;
                }

                foreach (var item in objectMeshPairs)
                {
                    item.Value.Destruct();
                }

                objectMeshPairs = null;
            }
        }


        [System.Serializable]
        public class UndoRedoOps
        {
            private const int OBJECT_HISTORY_LIMIT = 5;
            public GameObject gameObject;
            public List<ObjectHistory> undoOperations;
            public List<ObjectHistory> redoOperations;

            public UndoRedoOps(GameObject gameObject, List<ObjectHistory> undoOperations, List<ObjectHistory> redoOperations)
            {
                this.gameObject = gameObject;
                this.undoOperations = undoOperations;
                this.redoOperations = redoOperations; 
            }


            public void SaveRecord(bool isReduceDeep, bool isUndo, ObjectMeshPair originalMeshesClones)
            {

                if (undoOperations == null)
                {
                    undoOperations = new List<ObjectHistory>();
                }

                if (redoOperations == null)
                {
                    redoOperations = new List<ObjectHistory>();
                }

                if (isUndo)
                {
                    if (undoOperations.Count == OBJECT_HISTORY_LIMIT)
                    {
                        undoOperations[0].Destruct();
                        undoOperations[0] = null;
                        undoOperations.RemoveAt(0);
                    }

                    ObjectHistory undoOperation = new ObjectHistory(isReduceDeep, originalMeshesClones);

                    undoOperations.Add(undoOperation);

                }

                else
                {
                    if (redoOperations.Count == OBJECT_HISTORY_LIMIT)
                    {
                        redoOperations[0].Destruct();
                        redoOperations[0] = null;
                        redoOperations.RemoveAt(0);
                    }

                    ObjectHistory redoOperation = new ObjectHistory(isReduceDeep, originalMeshesClones);

                    redoOperations.Add(redoOperation);
                }
            }


            public void ApplyUndoRedoOperation(bool isUndo)
            {

                if (isUndo)
                {
                    if (undoOperations == null || undoOperations.Count == 0)
                    {
                        return;
                    }
                }

                else
                {
                    if (redoOperations == null || redoOperations.Count == 0)
                    {
                        return;
                    }
                }



                List<ObjectHistory> operations = isUndo ? undoOperations : redoOperations;
                ObjectHistory lastOp = operations[operations.Count - 1];


                if (lastOp.isReduceDeep)
                {
                    if (isUndo)
                    {
                        //Debug.Log("Last undo operation was reduce deep   ObjectMeshPair count  " + lastOp.objectMeshPairs.Count);
                    }
                    else
                    {
                        //Debug.Log("Last redo operation was reduce deep   ObjectMeshPair count  " + lastOp.objectMeshPairs.Count);
                    }
                }

                else
                {
                    if (isUndo)
                    {
                        //Debug.Log("Last undo operation was NOT reduce deep   ObjectMeshPair count  " + lastOp.objectMeshPairs.Count);
                    }
                    else
                    {
                        //Debug.Log("Last redo operation was NOT reduce deep   ObjectMeshPair count  " + lastOp.objectMeshPairs.Count);
                    }
                }



                ObjectMeshPair originalMeshesClones = new ObjectMeshPair();
                int totalOverwrites = lastOp.objectMeshPairs.Count;
                int done = 0;

                foreach (var kvp in lastOp.objectMeshPairs)
                {

                    EditorUtility.DisplayProgressBar("Performing Undo/Redo", $"Reverting mesh changes to existing files {++done}/{totalOverwrites}", ((float)done / totalOverwrites));

                    MeshRendererPair meshRendererPair = kvp.Value;
                    GameObject gameObject = kvp.Key;

                    if (gameObject == null) { continue; }
                    if (meshRendererPair == null) { continue; }
                    if (meshRendererPair.mesh == null) { continue; }


                    if (meshRendererPair.attachedToMeshFilter)
                    {
                        MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                        if (filter != null)
                        {
                            Mesh origMesh = UnityEngine.Object.Instantiate(filter.sharedMesh);
                            MeshRendererPair originalPair = new MeshRendererPair(true, origMesh);
                            originalMeshesClones.Add(gameObject, originalPair);

                            // Overwrites the mesh assets and keeps references intact
                            if (UtilityServices.OverwriteAssetWith(filter.sharedMesh, meshRendererPair.mesh, true)) { }
                            else
                            {
                                filter.sharedMesh.Clear();
                                EditorUtility.CopySerialized(meshRendererPair.mesh, filter.sharedMesh);
                                filter.sharedMesh.vertices = meshRendererPair.mesh.vertices;
                            }

                            //meshRendererPair.Destruct();
                        }
                    }

                    else
                    {
                        SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                        if (sRenderer != null)
                        {
                            Mesh origMesh = UnityEngine.Object.Instantiate(sRenderer.sharedMesh);
                            MeshRendererPair originalPair = new MeshRendererPair(false, origMesh);
                            originalMeshesClones.Add(gameObject, originalPair);

                            //sRenderer.sharedMesh.MakeSimilarToOtherMesh(meshRendererPair.mesh);

                            //sRenderer.sharedMesh.Clear();
                            //EditorUtility.CopySerialized(meshRendererPair.mesh, sRenderer.sharedMesh);

                            //Overwrites the mesh assets and keeps references intact
                            if (UtilityServices.OverwriteAssetWith(sRenderer.sharedMesh, meshRendererPair.mesh, true)) { }
                            else
                            {
                                sRenderer.sharedMesh.Clear();
                                EditorUtility.CopySerialized(meshRendererPair.mesh, sRenderer.sharedMesh);
                                sRenderer.sharedMesh.vertices = meshRendererPair.mesh.vertices;
                            }


                            //meshRendererPair.Destruct();
                        }
                    }
                }


                SaveRecord(lastOp.isReduceDeep, !isUndo, originalMeshesClones);

                lastOp.objectMeshPairs = null;
                lastOp = null;
                operations.RemoveAt(operations.Count - 1);

                EditorUtility.ClearProgressBar();
            }


            public void Destruct()
            {
                if (undoOperations != null && undoOperations.Count > 0)
                {
                    foreach (var operation in undoOperations)
                    {
                        operation.Destruct();
                    }
                }

                if (redoOperations != null && redoOperations.Count > 0)
                {
                    foreach (var operation in redoOperations)
                    {
                        operation.Destruct();
                    }
                }

            }
        }


        [System.Serializable]
        public class LODLevelSettings
        {
            public float reductionStrength;
            public float transitionHeight;
            public bool preserveUVFoldover;
            public bool preserveUVSeams;
            public bool preserveBorders;
            public bool useEdgeSort;
            public bool recalculateNormals;
            public float aggressiveness;
            public int maxIterations;
            public bool regardCurvature;
            public bool regardTolerance;
            public bool combineMeshes;
            public bool simplificationOptionsFoldout;
            public bool intensityFoldout;
            public bool clearBlendshapesComplete;
            public bool generateUV2;
            public List<float> sphereIntensities;


            public LODLevelSettings(float reductionStrength, float transitionHeight, bool preserveUVFoldover, bool preserveUVSeams, bool preserveBorders, bool smartLinking, bool recalculateNormals, float aggressiveness, int maxIterations, bool regardTolerance, bool regardCurvature, bool combineMeshes, bool clearBlendshapesComplete, bool generateUV2, List<float> sphereIntensities)
            {
                this.reductionStrength = reductionStrength;
                this.transitionHeight = transitionHeight;
                this.preserveUVFoldover = preserveUVFoldover;
                this.preserveUVSeams = preserveUVSeams;
                this.preserveBorders = preserveBorders;
                this.useEdgeSort = smartLinking;
                this.recalculateNormals = recalculateNormals;
                this.aggressiveness = aggressiveness;
                this.maxIterations = maxIterations;
                this.regardTolerance = regardTolerance;
                this.regardCurvature = regardCurvature;
                this.combineMeshes = combineMeshes;
                this.clearBlendshapesComplete = clearBlendshapesComplete;
                this.generateUV2 = generateUV2;
                this.sphereIntensities = sphereIntensities;
            }
        }


        [System.Serializable]
        public class LODBackup
        {
            [SerializeField]
            private Renderer[] originalRenderers = null;
            [SerializeField]
            public GameObject lodParentObject;


            public Renderer[] OriginalRenderers
            {
                get { return originalRenderers; }
                set { originalRenderers = value; }
            }
        }

#if UNITY_2019_3_OR_NEWER
        [SerializeReference]
#else
        [SerializeField]
#endif
        public UndoRedoOps objectsHistory;

#if UNITY_2019_3_OR_NEWER
        [SerializeReference]
#else
        [SerializeField]
#endif
        public ObjectMeshPair objectMeshPairs;
        
        public List<LODLevelSettings> currentLodLevelSettings;

        public List<ToleranceSphere> toleranceSpheres;

#if UNITY_2019_3_OR_NEWER
        [SerializeReference]
#else
        [SerializeField]
#endif
        public LODBackup lodBackup;


        #region BATCH FEW DATA

#if UNITY_2019_3_OR_NEWER
        [SerializeReference]
#else
        [SerializeField]
#endif
        public TextureArrayGroup textureArraysSettings;
        public List<MaterialProperties> materialsToRestore;
#if UNITY_2019_3_OR_NEWER
        [SerializeReference]
#else
        [SerializeField]
#endif
        public ObjectMaterialLinks lastObjMaterialLinks; // BeforeSerialization
        public bool relocateMaterialLinks;
        public bool reInitializeTempMatProps;

        public int choiceTextureMap = 0;
        public int choiceDiffuseColorSpace = 0;
     
        public readonly string[] textureMapsChoices = new[] { "Albedo", "Metallic", "Specular", "Normal", "Height", "Occlusion", "Emission", "Detail Mask", "Detail Albedo", "Detail Normal" };
        public readonly string[] compressionTypesChoices = new[] { "Uncompressed", "DXT1", "ETC2_RGB", "PVRTC_RGB4", "ASTC_RGB"};
        public readonly string[] resolutionsChoices = new[] { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        public readonly string[] filteringModesChoices = new[] { "Point (no filter)", "Bilinear", "Trilinear" };
        public readonly string[] compressionQualitiesChoices = new[] { "Low", "Medium", "High" };
        public readonly string[] colorSpaceChoices = new[] { "Non_Linear", "Linear" };
        public string batchFewSavePath = "";

#endregion BATCH FEW DATA


#region ALTER TEXTURE ARRAYS

        public List<Texture2DArray> existingTextureArrays = new List<Texture2DArray>();
        public bool existingTextureArraysFoldout;
        public int existingTextureArraysSize;
        public bool textureArraysPropsFoldout;
#if UNITY_2019_3_OR_NEWER
        [SerializeReference]
#else
        [SerializeField]
#endif
        public TextureArrayUserSettings existingArraysProps;
        public int choiceColorSpace = 0; //non linear

#endregion ALTER TEXTURE ARRAYS


#region INSPECTOR DRAWER VARS

        public bool preserveBorders;
        public bool preserveUVSeams;
        public bool preserveUVFoldover;
        public bool useEdgeSort = false;
        public bool recalculateNormals;
        public int maxIterations = 100;
        public float aggressiveness = 7;
        public bool regardCurvature = false;
        public bool considerChildren;
        public bool isPreservationActive;
        public float sphereDiameter = 0.5f;
        public Vector3 oldSphereScale;
        public float reductionStrength;
        public bool reductionPending;
        public GameObject prevFeasibleTarget;
        public bool runOnThreads;
        public int triangleCount;
        [SerializeField]
        public UnityEngine.Object lastDrawer;
        public bool foldoutAutoLOD;
        public bool foldoutBatchFew;
        public bool foldoutAutoLODMultiple;
        public Vector3 objPositionPrevFrame; 
        public Vector3 objScalePrevFrame;
        public bool considerChildrenBatchFew = true;
        public string autoLODSavePath = "";

        public bool foldoutAdditionalOpts;
        public bool generateUV2;
        public bool copyParentStaticFlags;
        public bool copyParentTag;
        public bool copyParentLayer;
        public bool createAsChildren;
        public bool removeLODBackupComponent;
        public bool generateUV2batchfew;
        public bool removeMaterialLinksComponent;
        public bool clearBlendshapesComplete;
        public bool clearBlendshapesNormals;
        public bool clearBlendshapesTangents;

        public bool isPlainSkin = false;

#endregion INSPECTOR DRAWER VARS

    }
}

#endif
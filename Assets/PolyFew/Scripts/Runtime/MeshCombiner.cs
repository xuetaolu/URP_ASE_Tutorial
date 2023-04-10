//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////


#if UNITY_2018_2_OR_NEWER
#define UNITY_8UV_SUPPORT
#endif

#if UNITY_2017_3_OR_NEWER
#define UNITY_MESH_INDEXFORMAT_SUPPORT
#endif

#if UNITY_MESH_INDEXFORMAT_SUPPORT
using UnityEngine.Rendering;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrainFailProductions.PolyFewRuntime
{

    public static class MeshCombiner
    {

        #region DATA_STRUCTURES

        private static MeshRenderer[] unityCombinedMeshRenderers = null;
        private static Material[] unityCombinedMeshesMats = null;
        private static bool didUseUnityCombine = false;
        public static bool generateUV2 = false;

        public struct StaticRenderer
        {
            public string name;
            public bool isNewMesh;
            public Transform transform;
            public Mesh mesh;
            public Material[] materials;
        }

        public struct SkinnedRenderer
        {
            public bool hasBlendShapes;
            public string name;
            public bool isNewMesh;
            public Transform transform;
            public Mesh mesh;
            public Material[] materials;
            public Transform rootBone;
            public Transform[] bones;
        }

        [Serializable]
        public struct BlendShape
        {
            /// <summary>
            /// The name of the blend shape.
            /// </summary>
            public string ShapeName;
            /// <summary>
            /// The blend shape frames.
            /// </summary>
            public BlendShapeFrame[] Frames;

            /// <summary>
            /// Creates a new blend shape.
            /// </summary>
            /// <param name="shapeName">The name of the blend shape.</param>
            /// <param name="frames">The blend shape frames.</param>
            public BlendShape(string shapeName, BlendShapeFrame[] frames)
            {
                this.ShapeName = shapeName;
                this.Frames = frames;
            }
        }


        [Serializable]
        public struct BlendShapeFrame
        {
            /// <summary>
            /// The name of the blend shape this frame is associated with.
            /// </summary>
            public string shapeName;
            /// <summary>
            /// The weight of the blend shape frame.
            /// </summary>
            public float frameWeight;
            /// <summary>
            /// The delta vertices of the blend shape frame.
            /// </summary>
            public Vector3[] deltaVertices;
            /// <summary>
            /// The delta normals of the blend shape frame.
            /// </summary>
            public Vector3[] deltaNormals;
            /// <summary>
            /// The delta tangents of the blend shape frame.
            /// </summary>
            public Vector3[] deltaTangents;
            /// <summary>
            /// The vertex offset to be used in the combined mesh vertex array.
            /// </summary>
            public int vertexOffset;


            /// <summary>
            /// Creates a new blend shape frame.
            /// </summary>
            /// <param name="frameWeight">The weight of the blend shape frame.</param>
            /// <param name="deltaVertices">The delta vertices of the blend shape frame.</param>
            /// <param name="deltaNormals">The delta normals of the blend shape frame.</param>
            /// <param name="deltaTangents">The delta tangents of the blend shape frame.</param>
            public BlendShapeFrame(float frameWeight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
            {
                this.frameWeight = frameWeight;
                this.deltaVertices = deltaVertices;
                this.deltaNormals = deltaNormals;
                this.deltaTangents = deltaTangents;
                this.shapeName = "";
                this.vertexOffset = -1;
            }


            public BlendShapeFrame(string shapeName, float frameWeight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents, int vertexOffset)
            {
                this.shapeName = shapeName;
                this.frameWeight = frameWeight;
                this.deltaVertices = deltaVertices;
                this.deltaNormals = deltaNormals;
                this.deltaTangents = deltaTangents;
                this.vertexOffset = vertexOffset;
            }
        }


        public static class MeshUtils
        {
            #region Consts
            /// <summary>
            /// The count of supported UV channels.
            /// </summary>
#if UNITY_8UV_SUPPORT
            public const int UVChannelCount = 8;
#else
            public const int UVChannelCount = 4;
#endif
            #endregion

            #region Public Methods
            /// <summary>
            /// Creates a new mesh.
            /// </summary>
            /// <param name="vertices">The mesh vertices.</param>
            /// <param name="indices">The mesh sub-mesh indices.</param>
            /// <param name="normals">The mesh normals.</param>
            /// <param name="tangents">The mesh tangents.</param>
            /// <param name="colors">The mesh colors.</param>
            /// <param name="boneWeights">The mesh bone-weights.</param>
            /// <param name="uvs">The mesh 4D UV sets.</param>
            /// <param name="bindposes">The mesh bindposes.</param>
            /// <returns>The created mesh.</returns>
            public static Mesh CreateMesh(Vector3[] vertices, int[][] indices, Vector3[] normals, Vector4[] tangents, Color[] colors, BoneWeight[] boneWeights, List<Vector2>[] uvs, Matrix4x4[] bindposes, BlendShape[] blendShapes)
            {
                return CreateMesh(vertices, indices, normals, tangents, colors, boneWeights, uvs, null, null, bindposes, blendShapes);
            }

            /// <summary>
            /// Creates a new mesh.
            /// </summary>
            /// <param name="vertices">The mesh vertices.</param>
            /// <param name="indices">The mesh sub-mesh indices.</param>
            /// <param name="normals">The mesh normals.</param>
            /// <param name="tangents">The mesh tangents.</param>
            /// <param name="colors">The mesh colors.</param>
            /// <param name="boneWeights">The mesh bone-weights.</param>
            /// <param name="uvs">The mesh 4D UV sets.</param>
            /// <param name="bindposes">The mesh bindposes.</param>
            /// <returns>The created mesh.</returns>
            public static Mesh CreateMesh(Vector3[] vertices, int[][] indices, Vector3[] normals, Vector4[] tangents, Color[] colors, BoneWeight[] boneWeights, List<Vector4>[] uvs, Matrix4x4[] bindposes, BlendShape[] blendShapes)
            {
                return CreateMesh(vertices, indices, normals, tangents, colors, boneWeights, null, null, uvs, bindposes, blendShapes);
            }

            /// <summary>
            /// Creates a new mesh.
            /// </summary>
            /// <param name="vertices">The mesh vertices.</param>
            /// <param name="indices">The mesh sub-mesh indices.</param>
            /// <param name="normals">The mesh normals.</param>
            /// <param name="tangents">The mesh tangents.</param>
            /// <param name="colors">The mesh colors.</param>
            /// <param name="boneWeights">The mesh bone-weights.</param>
            /// <param name="uvs2D">The mesh 2D UV sets.</param>
            /// <param name="uvs3D">The mesh 3D UV sets.</param>
            /// <param name="uvs4D">The mesh 4D UV sets.</param>
            /// <param name="bindposes">The mesh bindposes.</param>
            /// <returns>The created mesh.</returns>
            public static Mesh CreateMesh(Vector3[] vertices, int[][] indices, Vector3[] normals, Vector4[] tangents, Color[] colors, BoneWeight[] boneWeights, List<Vector2>[] uvs2D, List<Vector3>[] uvs3D, List<Vector4>[] uvs4D, Matrix4x4[] bindposes, BlendShape[] blendShapes)
            {
                var newMesh = new Mesh();
                int subMeshCount = indices.Length;

#if UNITY_MESH_INDEXFORMAT_SUPPORT
                IndexFormat indexFormat;
                var indexMinMax = MeshUtils.GetSubMeshIndexMinMax(indices, out indexFormat);
                newMesh.indexFormat = indexFormat;
#endif

                if (bindposes != null && bindposes.Length > 0)
                {
                    newMesh.bindposes = bindposes;
                }

                newMesh.subMeshCount = subMeshCount;
                newMesh.vertices = vertices;

                // If after assigning normals blendshapes are assigned, then blendshapes do not work correctly
                // In URP and HDRP configurations, so we add blendshapes first and then assign normals
                if (blendShapes != null)
                {
                    MeshUtils.ApplyMeshBlendShapes(newMesh, blendShapes);
                }

                if (normals != null && normals.Length > 0)
                {
                    newMesh.normals = normals;
                }
                if (tangents != null && tangents.Length > 0)
                {
                    newMesh.tangents = tangents;
                }
                if (colors != null && colors.Length > 0)
                {
                    newMesh.colors = colors;
                }
                if (boneWeights != null && boneWeights.Length > 0)
                {
                    newMesh.boneWeights = boneWeights;
                }

                if (uvs2D != null)
                {
                    for (int uvChannel = 0; uvChannel < uvs2D.Length; uvChannel++)
                    {
                        if (uvs2D[uvChannel] != null && uvs2D[uvChannel].Count > 0)
                        {
                            newMesh.SetUVs(uvChannel, uvs2D[uvChannel]);
                        }
                    }
                }

                if (uvs3D != null)
                {
                    for (int uvChannel = 0; uvChannel < uvs3D.Length; uvChannel++)
                    {
                        if (uvs3D[uvChannel] != null && uvs3D[uvChannel].Count > 0)
                        {
                            newMesh.SetUVs(uvChannel, uvs3D[uvChannel]);
                        }
                    }
                }

                if (uvs4D != null)
                {
                    for (int uvChannel = 0; uvChannel < uvs4D.Length; uvChannel++)
                    {
                        if (uvs4D[uvChannel] != null && uvs4D[uvChannel].Count > 0)
                        {
                            newMesh.SetUVs(uvChannel, uvs4D[uvChannel]);
                        }
                    }
                }


                //if (blendShapes != null)
                //{
                //    MeshUtils.ApplyMeshBlendShapes(newMesh, blendShapes);  //baw did
                //}


                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    var subMeshTriangles = indices[subMeshIndex];
#if UNITY_MESH_INDEXFORMAT_SUPPORT
                    var minMax = indexMinMax[subMeshIndex];
                    if (indexFormat == UnityEngine.Rendering.IndexFormat.UInt16 && minMax.y > ushort.MaxValue)
                    {
                        int baseVertex = minMax.x;
                        for (int index = 0; index < subMeshTriangles.Length; index++)
                        {
                            subMeshTriangles[index] -= baseVertex;
                        }
                        newMesh.SetTriangles(subMeshTriangles, subMeshIndex, false, baseVertex);
                    }
                    else
                    {
                        newMesh.SetTriangles(subMeshTriangles, subMeshIndex, false, 0);
                    }
#else
                    newMesh.SetTriangles(subMeshTriangles, subMeshIndex, false);
#endif
                }

                newMesh.RecalculateBounds();
                return newMesh;
            }

            /// <summary>
            /// Returns the blend shapes of a mesh.
            /// </summary>
            /// <param name="mesh">The mesh.</param>
            /// <returns>The mesh blend shapes.</returns>
            public static BlendShape[] GetMeshBlendShapes(Mesh mesh)
            {
                if (mesh == null)
                    throw new ArgumentNullException(nameof(mesh));

                int vertexCount = mesh.vertexCount;
                int blendShapeCount = mesh.blendShapeCount;
                if (blendShapeCount == 0)
                    return null;

                var blendShapes = new BlendShape[blendShapeCount];

                for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                {
                    string shapeName = mesh.GetBlendShapeName(blendShapeIndex);
                    int frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
                    var frames = new BlendShapeFrame[frameCount];

                    for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                    {
                        float frameWeight = mesh.GetBlendShapeFrameWeight(blendShapeIndex, frameIndex);

                        var deltaVertices = new Vector3[vertexCount];
                        var deltaNormals = new Vector3[vertexCount];
                        var deltaTangents = new Vector3[vertexCount];
                        mesh.GetBlendShapeFrameVertices(blendShapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                        frames[frameIndex] = new BlendShapeFrame(frameWeight, deltaVertices, deltaNormals, deltaTangents);
                    }

                    blendShapes[blendShapeIndex] = new BlendShape(shapeName, frames);
                }

                return blendShapes;
            }

            /// <summary>
            /// Applies and overrides the specified blend shapes on the specified mesh.
            /// </summary>
            /// <param name="mesh">The mesh.</param>
            /// <param name="blendShapes">The mesh blend shapes.</param>
            public static void ApplyMeshBlendShapes(Mesh mesh, BlendShape[] blendShapes)
            {
                if (mesh == null)
                    throw new ArgumentNullException(nameof(mesh));

                mesh.ClearBlendShapes();
                if (blendShapes == null || blendShapes.Length == 0)
                    return;

                for (int blendShapeIndex = 0; blendShapeIndex < blendShapes.Length; blendShapeIndex++)
                {
                    string shapeName = blendShapes[blendShapeIndex].ShapeName;
                    var frames = blendShapes[blendShapeIndex].Frames;

                    if (frames != null)
                    {
                        for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
                        {
                            mesh.AddBlendShapeFrame(shapeName, frames[frameIndex].frameWeight, frames[frameIndex].deltaVertices, frames[frameIndex].deltaNormals, frames[frameIndex].deltaTangents);
                        }
                    }
                }
            }




            /// <summary>
            /// Returns the UV sets for a specific mesh.
            /// </summary>
            /// <param name="mesh">The mesh.</param>
            /// <returns>The UV sets.</returns>
            public static List<Vector4>[] GetMeshUVs(Mesh mesh)
            {
                if (mesh == null)
                    throw new ArgumentNullException(nameof(mesh));

                var uvs = new List<Vector4>[UVChannelCount];
                for (int channel = 0; channel < UVChannelCount; channel++)
                {
                    uvs[channel] = GetMeshUVs(mesh, channel);
                }
                return uvs;
            }

            /// <summary>
            /// Returns the UV list for a specific mesh and UV channel.
            /// </summary>
            /// <param name="mesh">The mesh.</param>
            /// <param name="channel">The UV channel.</param>
            /// <returns>The UV list.</returns>
            public static List<Vector4> GetMeshUVs(Mesh mesh, int channel)
            {
                if (mesh == null)
                    throw new ArgumentNullException(nameof(mesh));
                else if (channel < 0 || channel >= UVChannelCount)
                    throw new ArgumentOutOfRangeException(nameof(channel));

                var uvList = new List<Vector4>(mesh.vertexCount);
                mesh.GetUVs(channel, uvList);
                return uvList;
            }

            /// <summary>
            /// Returns the number of used UV components in a UV set.
            /// </summary>
            /// <param name="uvs">The UV set.</param>
            /// <returns>The number of used UV components.</returns>
            public static int GetUsedUVComponents(List<Vector4> uvs)
            {

                if (uvs == null || uvs.Count == 0)
                    return 0;

                int usedComponents = 0;

                foreach (var uv in uvs)
                {
                    if (usedComponents < 1 && uv.x != 0f)
                    {
                        usedComponents = 1;
                    }
                    if (usedComponents < 2 && uv.y != 0f)
                    {
                        usedComponents = 2;
                    }
                    if (usedComponents < 3 && uv.z != 0f)
                    {
                        usedComponents = 3;
                    }
                    if (usedComponents < 4 && uv.w != 0f)
                    {
                        usedComponents = 4;
                        break;
                    }
                }

                return usedComponents;
            }

            /// <summary>
            /// Converts a list of 4D UVs into 2D.
            /// </summary>
            /// <param name="uvs">The list of UVs.</param>
            /// <returns>The array of 2D UVs.</returns>
            public static Vector2[] ConvertUVsTo2D(List<Vector4> uvs)
            {
                if (uvs == null)
                    return null;

                var uv2D = new Vector2[uvs.Count];
                for (int i = 0; i < uv2D.Length; i++)
                {
                    var uv = uvs[i];
                    uv2D[i] = new Vector2(uv.x, uv.y);
                }
                return uv2D;
            }

            /// <summary>
            /// Converts a list of 4D UVs into 3D.
            /// </summary>
            /// <param name="uvs">The list of UVs.</param>
            /// <returns>The array of 3D UVs.</returns>
            public static Vector3[] ConvertUVsTo3D(List<Vector4> uvs)
            {
                if (uvs == null)
                    return null;

                var uv3D = new Vector3[uvs.Count];
                for (int i = 0; i < uv3D.Length; i++)
                {
                    var uv = uvs[i];
                    uv3D[i] = new Vector3(uv.x, uv.y, uv.z);
                }
                return uv3D;
            }

#if UNITY_MESH_INDEXFORMAT_SUPPORT
            /// <summary>
            /// Returns the minimum and maximum indices for each submesh along with the needed index format.
            /// </summary>
            /// <param name="indices">The indices for the submeshes.</param>
            /// <param name="indexFormat">The output index format.</param>
            /// <returns>The minimum and maximum indices for each submesh.</returns>
            public static Vector2Int[] GetSubMeshIndexMinMax(int[][] indices, out IndexFormat indexFormat)
            {
                if (indices == null)
                    throw new ArgumentNullException(nameof(indices));

                var result = new Vector2Int[indices.Length];
                indexFormat = IndexFormat.UInt16;
                for (int subMeshIndex = 0; subMeshIndex < indices.Length; subMeshIndex++)
                {
                    int minIndex, maxIndex;
                    GetIndexMinMax(indices[subMeshIndex], out minIndex, out maxIndex);
                    result[subMeshIndex] = new Vector2Int(minIndex, maxIndex);

                    int indexRange = (maxIndex - minIndex);
                    if (indexRange > ushort.MaxValue)
                    {
                        indexFormat = IndexFormat.UInt32;
                    }
                }
                return result;
            }
#endif
            #endregion

            #region Private Methods
            private static void GetIndexMinMax(int[] indices, out int minIndex, out int maxIndex)
            {
                if (indices == null || indices.Length == 0)
                {
                    minIndex = maxIndex = 0;
                    return;
                }

                minIndex = int.MaxValue;
                maxIndex = int.MinValue;

                for (int i = 0; i < indices.Length; i++)
                {
                    if (indices[i] < minIndex)
                    {
                        minIndex = indices[i];
                    }
                    if (indices[i] > maxIndex)
                    {
                        maxIndex = indices[i];
                    }
                }
            }
            #endregion
        }


        #endregion DATA_STRUCTURES



        #region PUBLIC_METHODS


        public static StaticRenderer[] GetStaticRenderers(MeshRenderer[] renderers)
        {
            var newRenderers = new List<StaticRenderer>(renderers.Length);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh filter and was ignored.", renderer);
                    continue;
                }

                var mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
                    continue;
                }

                newRenderers.Add(new StaticRenderer()
                {
                    name = renderer.name,
                    isNewMesh = false,
                    transform = renderer.transform,
                    mesh = mesh,
                    materials = renderer.sharedMaterials
                });
            }
            return newRenderers.ToArray();
        }


        public static SkinnedRenderer[] GetSkinnedRenderers(SkinnedMeshRenderer[] renderers)
        {
            var newRenderers = new List<SkinnedRenderer>(renderers.Length);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];

                var mesh = renderer.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
                    continue;
                }

                newRenderers.Add(new SkinnedRenderer()
                {
                    name = renderer.name,
                    isNewMesh = false,
                    transform = renderer.transform,
                    mesh = mesh,
                    materials = renderer.sharedMaterials,
                    rootBone = renderer.rootBone,
                    bones = renderer.bones
                });
            }
            return newRenderers.ToArray();
        }


        public static StaticRenderer[] CombineStaticMeshes(Transform transform, int levelIndex, MeshRenderer[] renderers, bool autoName = true, string combinedBaseName = "")
        {
            if (renderers.Length == 0)
                return null;

            var newRenderers = new List<StaticRenderer>(renderers.Length);

            if (renderers.Length > 1)
            {
                var staticMeshes = (from renderer in renderers
                                    where renderer.GetComponent<MeshFilter>() != null && renderer.GetComponent<MeshFilter>().sharedMesh != null
                                    select renderer.GetComponent<MeshFilter>()).ToArray();

                CombineMeshesUnity(transform, staticMeshes);
                didUseUnityCombine = true;
            }

            Material[] combinedMaterials;
            Mesh combinedMesh;

            if (unityCombinedMeshRenderers == null)
            {
                combinedMesh = CombineMeshes(transform, renderers, out combinedMaterials);
            }
            else
            {
                if (unityCombinedMeshRenderers.Length == 1)
                {
                    combinedMaterials = unityCombinedMeshesMats.ToArray();
                    combinedMesh = unityCombinedMeshRenderers[0].GetComponent<MeshFilter>().sharedMesh;
                }

                else if (unityCombinedMeshRenderers.Length == 0)
                {
                    combinedMesh = CombineMeshes(transform, renderers, out combinedMaterials);
                }

                else
                {
                    combinedMesh = CombineMeshes(transform, unityCombinedMeshRenderers, out combinedMaterials);
                }
            }



            if (unityCombinedMeshRenderers != null)
            {
                foreach (var item in unityCombinedMeshRenderers) { UnityEngine.GameObject.DestroyImmediate(item.gameObject); }
            }

            unityCombinedMeshRenderers = null;
            unityCombinedMeshesMats = null;

            string baseName = string.IsNullOrWhiteSpace(combinedBaseName) ? transform.name : combinedBaseName;


            string rendererName = string.Format("{0}_combined_static", baseName);

            if (autoName)
            {
                if (transform != null)
                {
                    combinedMesh.name = string.Format("{0}_static{1:00}", transform.name, levelIndex);
                }
            }

            newRenderers.Add(new StaticRenderer()
            {
                name = rendererName,
                isNewMesh = true,
                transform = null,
                mesh = combinedMesh,
                materials = combinedMaterials
            });


#if UNITY_EDITOR

            //UnityEditor.MeshUtility.Optimize(combinedMesh);
            // Optimizing screws up the combined mesh sometimes by streching it into wierd shapes

            if (generateUV2)
            {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(combinedMesh);
            }
#endif

            didUseUnityCombine = false;

            return newRenderers.ToArray();
        }


        public static SkinnedRenderer[] CombineSkinnedMeshes(Transform transform, int levelIndex, SkinnedMeshRenderer[] renderers, ref SkinnedMeshRenderer[] renderersActuallyCombined, bool autoName = true, string combinedBaseName = "")
        {
            if (renderers.Length == 0)
                return null;

            // TODO: Support to merge sub-meshes and atlas textures

            var newRenderers = new List<SkinnedRenderer>(renderers.Length);
            //var blendShapeRenderers = (from renderer in renderers
            //                           where renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0
            //                           select renderer); //baw did

            var renderersWithoutMesh = (from renderer in renderers
                                        where renderer.sharedMesh == null
                                        select renderer);
            var combineRenderers = (from renderer in renderers
                                    where renderer.sharedMesh != null // && renderer.sharedMesh.blendShapeCount == 0 baw did
                                    select renderer).ToArray();


            renderersActuallyCombined = combineRenderers;

            // Warn about renderers without a mesh
            foreach (var renderer in renderersWithoutMesh)
            {
                Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
            }


            //Don't combine meshes with blend shapes
            //foreach (var renderer in blendShapeRenderers) 
            //{
            //    newRenderers.Add(new SkinnedRenderer()
            //    {
            //        name = renderer.name,
            //        isNewMesh = false,
            //        transform = renderer.transform,
            //        mesh = renderer.sharedMesh,
            //        materials = renderer.sharedMaterials,
            //        rootBone = renderer.rootBone,
            //        bones = renderer.bones,
            //        hasBlendShapes = true
            //    });
            //}


            if (combineRenderers.Length > 0)
            {
                Material[] combinedMaterials;
                Transform[] combinedBones;
                var combinedMesh = CombineMeshes(transform, combineRenderers, out combinedMaterials, out combinedBones);
                string baseName = string.IsNullOrWhiteSpace(combinedBaseName) ? transform.name : combinedBaseName;
                string rendererName = string.Format("{0}_combined_skinned", baseName);

                if (autoName)
                {
                    combinedMesh.name = string.Format("{0}_skinned{1:00}", transform.name, levelIndex);
                }

                var rootBone = FindBestRootBone(transform, combineRenderers);

                newRenderers.Add(new SkinnedRenderer()
                {
                    name = rendererName,
                    isNewMesh = false,
                    transform = null,
                    mesh = combinedMesh,
                    materials = combinedMaterials,
                    rootBone = rootBone,
                    bones = combinedBones
                });


#if UNITY_EDITOR

                //UnityEditor.MeshUtility.Optimize(combinedMesh);
                // Optimizing screws up the combined mesh sometimes by streching it into wierd shapes

                if (generateUV2)
                {
                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(combinedMesh);
                }
#endif

            }


            return newRenderers.ToArray();
        }


        public static Mesh CombineMeshes(Transform rootTransform, MeshRenderer[] renderers, out Material[] resultMaterials, Dictionary<Transform, Transform> topLevelParents = null, Dictionary<string, BlendShapeFrame> blendShapes = null)
        {
            bool hasUnknownRootTransform = false;

            if (rootTransform == null)
                hasUnknownRootTransform = true;
            //throw new System.ArgumentNullException(nameof(rootTransform));

            if (renderers == null)
                throw new System.ArgumentNullException(nameof(renderers));



            var meshes = new Mesh[renderers.Length];

            var transforms = new Matrix4x4[renderers.Length];

            Tuple<Matrix4x4, bool>[] normalsTransforms = new Tuple<Matrix4x4, bool>[renderers.Length];
            var materials = new Material[renderers.Length][];



            for (int i = 0; i < renderers.Length; i++)
            {

                var renderer = renderers[i];

                if (renderer == null)

                    throw new System.ArgumentException(string.Format("The renderer at index {0} is null.", i), nameof(renderers));



                var rendererTransform = renderer.transform;

                var meshFilter = renderer.GetComponent<MeshFilter>();

                if (meshFilter == null)

                    throw new System.ArgumentException(string.Format("The renderer at index {0} has no mesh filter.", i), nameof(renderers));

                else if (meshFilter.sharedMesh == null)

                    throw new System.ArgumentException(string.Format("The mesh filter for renderer at index {0} has no mesh.", i), nameof(renderers));

                else if (!meshFilter.sharedMesh.isReadable)

                    throw new System.ArgumentException(string.Format("The mesh in the mesh filter for renderer at index {0} is not readable.", i), nameof(renderers));



                meshes[i] = meshFilter.sharedMesh;

                if (hasUnknownRootTransform)
                {
                    rootTransform = topLevelParents[rendererTransform];
                }

                if (didUseUnityCombine) { transforms[i] = rendererTransform.localToWorldMatrix; }
                else { transforms[i] = rootTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix; }

                Vector3 lossyScale = rendererTransform.transform.lossyScale;
                bool isUniformScale = Mathf.Approximately(lossyScale.x, lossyScale.y) && Mathf.Approximately(lossyScale.y, lossyScale.z);

                if (!isUniformScale)
                {
                    Debug.LogWarning($"The GameObject \"{rendererTransform.name}\" has non uniform scaling applied. This might cause the combined mesh normals to be incorrectly calculated resulting in slight variation in lighting.");
                }

                normalsTransforms[i] = Tuple.Create(rootTransform.localToWorldMatrix * rendererTransform.localToWorldMatrix, !isUniformScale); //baw did
                materials[i] = renderer.sharedMaterials;
            }


            return CombineMeshes(meshes, transforms, normalsTransforms, materials, out resultMaterials, blendShapes);

        }


        public static Mesh CombineMeshes(Transform rootTransform, SkinnedMeshRenderer[] renderers, out Material[] resultMaterials, out Transform[] resultBones)
        {

            //if (rootTransform == null)

            //throw new System.ArgumentNullException(nameof(rootTransform));

            if (renderers == null)

                throw new System.ArgumentNullException(nameof(renderers));



            var meshes = new Mesh[renderers.Length];

            var transforms = new Matrix4x4[renderers.Length];

            Tuple<Matrix4x4, bool>[] normalsTransforms = new Tuple<Matrix4x4, bool>[renderers.Length];

            var materials = new Material[renderers.Length][];

            var bones = new Transform[renderers.Length][];

            Dictionary<string, BlendShapeFrame> blendShapes = new Dictionary<string, BlendShapeFrame>();

            int vertexOffset = 0;

            for (int i = 0; i < renderers.Length; i++)
            {

                var renderer = renderers[i];

                if (renderer == null)

                    throw new System.ArgumentException(string.Format("The renderer at index {0} is null.", i), nameof(renderers));

                else if (renderer.sharedMesh == null)

                    throw new System.ArgumentException(string.Format("The renderer at index {0} has no mesh.", i), nameof(renderers));

                else if (!renderer.sharedMesh.isReadable)

                    throw new System.ArgumentException(string.Format("The mesh in the renderer at index {0} is not readable.", i), nameof(renderers));



                var rendererTransform = renderer.transform;

                meshes[i] = renderer.sharedMesh;

                // IF NO BONES
                //transforms[i] = rootTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                //transforms[i] = rendererTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                // IF ANY BONES


                Vector3 lossyScale = rendererTransform.transform.lossyScale;
                bool isUniformScale = Mathf.Approximately(lossyScale.x, lossyScale.y) && Mathf.Approximately(lossyScale.y, lossyScale.z);


                // baw did
                if (renderer.bones == null || renderer.bones.Length == 0)
                {
                    transforms[i] = rootTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                    normalsTransforms[i] = Tuple.Create(rootTransform.localToWorldMatrix * rendererTransform.localToWorldMatrix, !isUniformScale); //baw did
                }
                else
                {
                    transforms[i] = rendererTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                    normalsTransforms[i] = Tuple.Create(rendererTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix, !isUniformScale);
                }

                if (!isUniformScale)
                {
                    Debug.LogWarning($"The GameObject \"{rendererTransform.name}\" has non uniform scaling applied. This might cause the combined mesh normals to be incorrectly calculated resulting in slight variation in lighting.");
                }



                materials[i] = renderer.sharedMaterials;

                bones[i] = renderer.bones;

                for (int a = 0; a < bones[i].Length; a++)
                {
                    Transform t = bones[i][a];
                    MeshFilter mf = t == null ? null : t.GetComponent<MeshFilter>();
                    Mesh m = mf == null ? null : mf.sharedMesh;

                    if (m != null)
                    {
                        Debug.LogWarning($"You have a static mesh attached to the bone:\"{t.name}\". The mesh combination logic will not deal with this properly, since that would require it to modify the original game object hierarchy. You might get erroneous results on mesh combination.");
                    }
                }


                Mesh mesh = renderer.sharedMesh;
                int rendererId = renderer.GetHashCode();

                if (mesh.blendShapeCount > 0)
                {
                    for (int s = 0; s < mesh.blendShapeCount; s++)
                    {
                        for (int f = 0; f < mesh.GetBlendShapeFrameCount(s); f++)
                        {
                            Vector3[] deltaVertices = new Vector3[mesh.vertexCount];
                            Vector3[] deltaNormals = new Vector3[mesh.vertexCount];
                            Vector3[] deltaTangents = new Vector3[mesh.vertexCount];

                            if (!blendShapes.ContainsKey(mesh.GetBlendShapeName(s) + rendererId))
                            {
                                mesh.GetBlendShapeFrameVertices(s, f, deltaVertices, deltaNormals, deltaTangents);
                                blendShapes.Add(mesh.GetBlendShapeName(s) + rendererId, new BlendShapeFrame(mesh.GetBlendShapeName(s) + rendererId, mesh.GetBlendShapeFrameWeight(s, f), deltaVertices, deltaNormals, deltaTangents, vertexOffset));
                            }
                        }
                    }

                }

                vertexOffset += mesh.vertexCount;

            }



            return CombineMeshes(meshes, transforms, normalsTransforms, materials, bones, out resultMaterials, out resultBones, blendShapes);

        }


        public static Mesh CombineMeshes(Mesh[] meshes, Matrix4x4[] transforms, Tuple<Matrix4x4, bool>[] normalsTransforms, Material[][] materials, out Material[] resultMaterials, Dictionary<string, BlendShapeFrame> blendShapes = null)
        {

            if (meshes == null)

                throw new System.ArgumentNullException(nameof(meshes));

            else if (transforms == null)

                throw new System.ArgumentNullException(nameof(transforms));

            else if (materials == null)

                throw new System.ArgumentNullException(nameof(materials));



            Transform[] resultBones;

            return CombineMeshes(meshes, transforms, normalsTransforms, materials, null, out resultMaterials, out resultBones, blendShapes);

        }


        public static Mesh CombineMeshes(Mesh[] meshes, Matrix4x4[] transforms, Tuple<Matrix4x4, bool>[] normalsTransforms, Material[][] materials, Transform[][] bones, out Material[] resultMaterials, out Transform[] resultBones, Dictionary<string, BlendShapeFrame> blendShapes = null)
        {

            if (meshes == null)

                throw new System.ArgumentNullException(nameof(meshes));

            else if (transforms == null)

                throw new System.ArgumentNullException(nameof(transforms));

            else if (materials == null)

                throw new System.ArgumentNullException(nameof(materials));

            else if (transforms.Length != meshes.Length)

                throw new System.ArgumentException("The array of transforms doesn't have the same length as the array of meshes.", nameof(transforms));

            else if (materials.Length != meshes.Length)

                throw new System.ArgumentException("The array of materials doesn't have the same length as the array of meshes.", nameof(materials));

            else if (bones != null && bones.Length != meshes.Length)

                throw new System.ArgumentException("The array of bones doesn't have the same length as the array of meshes.", nameof(bones));



            int totalVertexCount = 0;

            int totalSubMeshCount = 0;

            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {

                var mesh = meshes[meshIndex];

                if (mesh == null)

                    throw new System.ArgumentException(string.Format("The mesh at index {0} is null.", meshIndex), nameof(meshes));

                else if (!mesh.isReadable)

                    throw new System.ArgumentException(string.Format("The mesh at index {0} is not readable.", meshIndex), nameof(meshes));



                totalVertexCount += mesh.vertexCount;

                totalSubMeshCount += mesh.subMeshCount;



                // Validate the mesh materials

                var meshMaterials = materials[meshIndex];

                if (meshMaterials == null)

                    throw new System.ArgumentException(string.Format("The materials for mesh at index {0} is null.", meshIndex), nameof(materials));

                else if (meshMaterials.Length != mesh.subMeshCount)

                    throw new System.ArgumentException(string.Format("The materials for mesh at index {0} doesn't match the submesh count ({1} != {2}).", meshIndex, meshMaterials.Length, mesh.subMeshCount), nameof(materials));



                for (int materialIndex = 0; materialIndex < meshMaterials.Length; materialIndex++)
                {

                    if (meshMaterials[materialIndex] == null)
                        throw new System.ArgumentException(string.Format("The material at index {0} for mesh {1} is null.", materialIndex, mesh.name), nameof(materials));

                }



                // Validate the mesh bones

                if (bones != null)
                {

                    var meshBones = bones[meshIndex];

                    if (meshBones == null)

                        throw new System.ArgumentException(string.Format("The bones for mesh at index {0} is null.", meshIndex), nameof(meshBones));



                    for (int boneIndex = 0; boneIndex < meshBones.Length; boneIndex++)

                    {

                        if (meshBones[boneIndex] == null)

                            throw new System.ArgumentException(string.Format("The bone at index {0} for mesh at index {1} is null.", boneIndex, meshIndex), nameof(meshBones));

                    }

                }

            }



            var combinedVertices = new List<Vector3>(totalVertexCount);

            var combinedIndices = new List<int[]>(totalSubMeshCount);

            List<Vector3> combinedNormals = null;

            List<Vector4> combinedTangents = null;

            List<Color> combinedColors = null;

            List<BoneWeight> combinedBoneWeights = null;

            var combinedUVs = new List<Vector4>[MeshUtils.UVChannelCount];



            List<Matrix4x4> usedBindposes = null;

            List<Transform> usedBones = null;

            var usedMaterials = new List<Material>(totalSubMeshCount);

            var materialMap = new Dictionary<Material, int>(totalSubMeshCount);



            int currentVertexCount = 0;

            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {

                var mesh = meshes[meshIndex];

                var meshTransform = transforms[meshIndex];

                var normalsTransform = normalsTransforms[meshIndex];

                var meshMaterials = materials[meshIndex];

                var meshBones = (bones != null ? bones[meshIndex] : null);



                int subMeshCount = mesh.subMeshCount;

                int meshVertexCount = mesh.vertexCount;

                var meshVertices = mesh.vertices;

                var meshNormals = mesh.normals;

                var meshTangents = mesh.tangents;

                var meshUVs = MeshUtils.GetMeshUVs(mesh);

                var meshColors = mesh.colors;

                var meshBoneWeights = mesh.boneWeights;

                var meshBindposes = mesh.bindposes;



                // Transform vertices with bones to keep only one bindpose

                if (meshBones != null && meshBoneWeights != null && meshBoneWeights.Length > 0 && meshBindposes != null && meshBindposes.Length > 0 && meshBones.Length == meshBindposes.Length)
                {

                    if (usedBindposes == null)
                    {

                        usedBindposes = new List<Matrix4x4>(meshBindposes);

                        usedBones = new List<Transform>(meshBones);

                    }



                    int[] boneIndices = new int[meshBones.Length];

                    for (int i = 0; i < meshBones.Length; i++)
                    {

                        int usedBoneIndex = usedBones.IndexOf(meshBones[i]);

                        if (usedBoneIndex == -1 || meshBindposes[i] != usedBindposes[usedBoneIndex])

                        {

                            usedBoneIndex = usedBones.Count;

                            usedBones.Add(meshBones[i]);

                            usedBindposes.Add(meshBindposes[i]);

                        }

                        boneIndices[i] = usedBoneIndex;

                    }



                    // Then we remap the bones

                    RemapBones(meshBoneWeights, boneIndices);

                }



                // Transforms the vertices, normals and tangents using the mesh transform

                TransformVertices(meshVertices, ref meshTransform);

                TransformNormals(meshNormals, ref normalsTransform);

                TransformTangents(meshTangents, ref normalsTransform);

                // Copy vertex positions & attributes

                CopyVertexPositions(combinedVertices, meshVertices);

                CopyVertexAttributes(ref combinedNormals, meshNormals, currentVertexCount, meshVertexCount, totalVertexCount, new Vector3(1f, 0f, 0f));

                CopyVertexAttributes(ref combinedTangents, meshTangents, currentVertexCount, meshVertexCount, totalVertexCount, new Vector4(0f, 0f, 1f, 1f));

                CopyVertexAttributes(ref combinedColors, meshColors, currentVertexCount, meshVertexCount, totalVertexCount, new Color(1f, 1f, 1f, 1f));

                CopyVertexAttributes(ref combinedBoneWeights, meshBoneWeights, currentVertexCount, meshVertexCount, totalVertexCount, new BoneWeight());



                for (int channel = 0; channel < meshUVs.Length; channel++)

                {

                    CopyVertexAttributes(ref combinedUVs[channel], meshUVs[channel], currentVertexCount, meshVertexCount, totalVertexCount, new Vector4(0f, 0f, 0f, 0f));

                }



                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)

                {

                    var subMeshMaterial = meshMaterials[subMeshIndex];

#if UNITY_MESH_INDEXFORMAT_SUPPORT

                    var subMeshIndices = mesh.GetTriangles(subMeshIndex, true);

#else

                    var subMeshIndices = mesh.GetTriangles(subMeshIndex);

#endif



                    if (currentVertexCount > 0)

                    {

                        for (int index = 0; index < subMeshIndices.Length; index++)

                        {

                            subMeshIndices[index] += currentVertexCount;

                        }

                    }


                    int existingSubMeshIndex;

                    if (materialMap.TryGetValue(subMeshMaterial, out existingSubMeshIndex))
                    {

                        combinedIndices[existingSubMeshIndex] = MergeArrays(combinedIndices[existingSubMeshIndex], subMeshIndices);

                    }

                    else
                    {

                        int materialIndex = combinedIndices.Count;

                        materialMap.Add(subMeshMaterial, materialIndex);

                        usedMaterials.Add(subMeshMaterial);

                        combinedIndices.Add(subMeshIndices);

                    }

                }



                currentVertexCount += meshVertexCount;

            }



            var resultVertices = combinedVertices.ToArray();

            var resultIndices = combinedIndices.ToArray();

            var resultNormals = (combinedNormals != null ? combinedNormals.ToArray() : null);

            var resultTangents = (combinedTangents != null ? combinedTangents.ToArray() : null);

            var resultColors = (combinedColors != null ? combinedColors.ToArray() : null);

            var resultBoneWeights = (combinedBoneWeights != null ? combinedBoneWeights.ToArray() : null);

            var resultUVs = combinedUVs.ToArray();

            var resultBindposes = (usedBindposes != null ? usedBindposes.ToArray() : null);

            resultMaterials = usedMaterials.ToArray();

            resultBones = (usedBones != null ? usedBones.ToArray() : null);


            Mesh combinedMesh = MeshUtils.CreateMesh(resultVertices, resultIndices, resultNormals, resultTangents, resultColors, resultBoneWeights, resultUVs, resultBindposes, null);


            if (blendShapes != null && blendShapes.Count > 0)
            {
                foreach (BlendShapeFrame blendShape in blendShapes.Values)
                {
                    Vector3[] deltaVertices = new Vector3[combinedMesh.vertexCount];
                    Vector3[] deltaNormals = new Vector3[combinedMesh.vertexCount];
                    Vector3[] deltaTangents = new Vector3[combinedMesh.vertexCount];

                    for (int p = 0; p < blendShape.deltaVertices.Length; p++)
                    {
                        deltaVertices.SetValue(blendShape.deltaVertices[p], p + blendShape.vertexOffset);
                        deltaNormals.SetValue(blendShape.deltaNormals[p], p + blendShape.vertexOffset);
                        deltaTangents.SetValue(blendShape.deltaTangents[p], p + blendShape.vertexOffset);
                    }

                    combinedMesh.AddBlendShapeFrame(blendShape.shapeName, blendShape.frameWeight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            // If after assigning normals blendshapes are assigned, then blendshapes do not work correctly
            // In URP and HDRP configurations, so we add blendshapes first and then assign normals
            combinedMesh.normals = resultNormals;
            combinedMesh.tangents = resultTangents;
            combinedMesh.RecalculateBounds();

            return combinedMesh;

        }


        #endregion PUBLIC_METHODS



        #region PRIVATE_METHODS

        private static void ParentAndResetTransform(Transform transform, Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }


        private static void ParentAndOffsetTransform(Transform transform, Transform parentTransform, Transform originalTransform)
        {
            transform.position = originalTransform.position;
            transform.rotation = originalTransform.rotation;
            transform.localScale = originalTransform.lossyScale;
            transform.SetParent(parentTransform, true);
        }


        private static Transform FindBestRootBone(Transform transform, SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                return null;

            Transform bestBone = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i] == null || skinnedMeshRenderers[i].rootBone == null)
                    continue;

                var rootBone = skinnedMeshRenderers[i].rootBone;
                var distance = (rootBone.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestBone = rootBone;
                    bestDistance = distance;
                }
            }

            return bestBone;
        }


        private static Transform FindBestRootBone(Dictionary<Transform, Transform> topLevelParents, SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                return null;

            Transform bestBone = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i] == null || skinnedMeshRenderers[i].rootBone == null)
                    continue;

                Transform topParent = topLevelParents[skinnedMeshRenderers[i].transform];
                var rootBone = skinnedMeshRenderers[i].rootBone;
                var distance = (rootBone.position - topParent.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestBone = rootBone;
                    bestDistance = distance;
                }
            }

            return bestBone;
        }


        private static Transform GetTopLevelParent(Transform forObject)
        {
            Transform topLevelParent = forObject;

            while (topLevelParent.parent != null) { topLevelParent = topLevelParent.parent; }

            return topLevelParent;
        }


        private static void CopyVertexPositions(List<Vector3> list, Vector3[] arr)
        {

            if (arr == null || arr.Length == 0)

                return;



            for (int i = 0; i < arr.Length; i++)

            {

                list.Add(arr[i]);

            }

        }


        private static void CopyVertexAttributes<T>(ref List<T> dest, IEnumerable<T> src, int previousVertexCount, int meshVertexCount, int totalVertexCount, T defaultValue)
        {

            if (src == null || src.Count() == 0)

            {

                if (dest != null)

                {

                    for (int i = 0; i < meshVertexCount; i++)

                    {

                        dest.Add(defaultValue);

                    }

                }

                return;

            }



            if (dest == null)

            {

                dest = new List<T>(totalVertexCount);

                for (int i = 0; i < previousVertexCount; i++)

                {

                    dest.Add(defaultValue);

                }

            }



            dest.AddRange(src);

        }


        private static T[] MergeArrays<T>(T[] arr1, T[] arr2)
        {

            var newArr = new T[arr1.Length + arr2.Length];

            System.Array.Copy(arr1, 0, newArr, 0, arr1.Length);

            System.Array.Copy(arr2, 0, newArr, arr1.Length, arr2.Length);

            return newArr;

        }


        private static void TransformVertices(Vector3[] vertices, ref Matrix4x4 transform)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transform.MultiplyPoint3x4(vertices[i]);
            }
        }


        private static void TransformNormals(Vector3[] normals, ref Tuple<Matrix4x4, bool> transform)
        {

            if (normals == null)
                return;

            for (int i = 0; i < normals.Length; i++)
            {
                // Non-UniformScaling
                if (transform.Item2 == true)
                {
                    Quaternion rotation = Quaternion.LookRotation(transform.Item1.GetColumn(2), transform.Item1.GetColumn(1));
                    normals[i] = rotation * normals[i]; //baw did
                }
                else
                {
                    normals[i] = transform.Item1.MultiplyVector(normals[i]);
                }
            }

        }


        private static void TransformTangents(Vector4[] tangents, ref Tuple<Matrix4x4, bool> transform)
        {

            if (tangents == null)

                return;



            Vector3 tengentDir;

            for (int i = 0; i < tangents.Length; i++)

            {

                tengentDir = transform.Item1.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z));

                tangents[i] = new Vector4(tengentDir.x, tengentDir.y, tengentDir.z, tangents[i].w);

            }

        }


        private static void RemapBones(BoneWeight[] boneWeights, int[] boneIndices)
        {

            for (int i = 0; i < boneWeights.Length; i++)

            {

                if (boneWeights[i].weight0 > 0)

                {

                    boneWeights[i].boneIndex0 = boneIndices[boneWeights[i].boneIndex0];

                }

                if (boneWeights[i].weight1 > 0)

                {

                    boneWeights[i].boneIndex1 = boneIndices[boneWeights[i].boneIndex1];

                }

                if (boneWeights[i].weight2 > 0)

                {

                    boneWeights[i].boneIndex2 = boneIndices[boneWeights[i].boneIndex2];

                }

                if (boneWeights[i].weight3 > 0)

                {

                    boneWeights[i].boneIndex3 = boneIndices[boneWeights[i].boneIndex3];

                }

            }

        }


        private static Matrix4x4 ScaleMatrix(ref Matrix4x4 matrix, float scale)
        {

            return new Matrix4x4()

            {

                m00 = matrix.m00 * scale,

                m01 = matrix.m01 * scale,

                m02 = matrix.m02 * scale,

                m03 = matrix.m03 * scale,



                m10 = matrix.m10 * scale,

                m11 = matrix.m11 * scale,

                m12 = matrix.m12 * scale,

                m13 = matrix.m13 * scale,



                m20 = matrix.m20 * scale,

                m21 = matrix.m21 * scale,

                m22 = matrix.m22 * scale,

                m23 = matrix.m23 * scale,



                m30 = matrix.m30 * scale,

                m31 = matrix.m31 * scale,

                m32 = matrix.m32 * scale,

                m33 = matrix.m33 * scale

            };

        }


        private static void CombineMeshesUnity(Transform parentTransform, MeshFilter[] meshFilters)
        {

            var combineMeshInstanceDictionary = new Dictionary<Material, List<CombineInstance>>();
            int totalVertsCount = 0;

            foreach (var meshFilter in meshFilters)
            {
                // Check if we are in older versions of Unity with max vertex limit <= 65534 

                if (meshFilter == null) { continue; }

                Mesh m = meshFilter.sharedMesh;

                if (m == null) { continue; }

                totalVertsCount += m.vertexCount;
            }


            foreach (var meshFilter in meshFilters)
            {
                var mesh = meshFilter.sharedMesh;
                //var vertices = new List<Vector3>(); //uncomment when manual mesh duplication is uncommented
                //mesh.GetVertices(vertices); //uncomment when manual mesh duplication is uncommented
                var materials = meshFilter.GetComponent<Renderer>().sharedMaterials;
                var subMeshCount = meshFilter.sharedMesh.subMeshCount;


                if (materials == null)
                {
                    throw new System.ArgumentException(string.Format("The materials for GameObject are null.", meshFilter.transform.name), nameof(materials));
                }

                else if (materials.Length != mesh.subMeshCount)
                {
                    throw new System.ArgumentException(string.Format("The materials for mesh {0} on GameObject {1} doesn't match the submesh count ({2} != {3}).", mesh.name, meshFilter.transform.name, materials.Length, mesh.subMeshCount), nameof(materials));
                }


                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] == null)
                    {
                        throw new System.ArgumentException(string.Format("The material at index {0} for mesh {1} on GameObject {2} is null.", materialIndex, mesh.name, meshFilter.transform.name), nameof(materials));
                    }
                }


                for (var i = 0; i < subMeshCount; i++)
                {
                    var material = materials[i];
                    var triangles = new List<int>();
                    mesh.GetTriangles(triangles, i);

                    //manual mesh duplication

                    //var newMesh = new Mesh
                    //{
                    //    vertices = vertices.ToArray(),
                    //    triangles = triangles.ToArray(),
                    //    uv = mesh.uv,
                    //    normals = mesh.normals,
                    //    colors = mesh.colors
                    //};

                    var newMesh = UnityEngine.Object.Instantiate(mesh);
                    newMesh.triangles = triangles.ToArray();


                    if (!combineMeshInstanceDictionary.ContainsKey(material))
                    {
                        combineMeshInstanceDictionary.Add(material, new List<CombineInstance>());
                    }


                    //var combineInstance = new CombineInstance
                    //{ transform = meshFilter.transform.localToWorldMatrix, mesh = newMesh };
                    var combineInstance = new CombineInstance
                    { transform = (parentTransform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix) , mesh = newMesh };

                    combineMeshInstanceDictionary[material].Add(combineInstance);
                }
            }


            unityCombinedMeshRenderers = new MeshRenderer[combineMeshInstanceDictionary.Count];
            unityCombinedMeshesMats = new Material[combineMeshInstanceDictionary.Count];

            int index = 0;

            foreach (var kvp in combineMeshInstanceDictionary)
            {
                var newObject = new GameObject(kvp.Key.name);

                var meshRenderer = newObject.AddComponent<MeshRenderer>();
                var meshFilter = newObject.AddComponent<MeshFilter>();

                meshRenderer.material = kvp.Key;
                var combinedMesh = new Mesh();

#if UNITY_MESH_INDEXFORMAT_SUPPORT
                if (totalVertsCount > 65534) { combinedMesh.indexFormat = IndexFormat.UInt32; }
#endif

                combinedMesh.CombineMeshes(kvp.Value.ToArray());

                meshFilter.sharedMesh = combinedMesh;

//#if UNITY_EDITOR
//                UnityEditor.MeshUtility.Optimize(meshFilter.sharedMesh);

//                if (generateUV2)
//                {
//                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
//                }
//#endif

                newObject.transform.parent = parentTransform.parent;

                unityCombinedMeshesMats[index] = kvp.Key;
                unityCombinedMeshRenderers[index] = meshRenderer;

                index++;
            }

        }


        #endregion PRIVATE_METHODS

    }
}

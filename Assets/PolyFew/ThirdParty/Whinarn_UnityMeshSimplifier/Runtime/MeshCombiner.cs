#region License

/*

MIT License



Copyright(c) 2019 Mattias Edlund



Permission is hereby granted, free of charge, to any person obtaining a copy

of this software and associated documentation files (the "Software"), to deal

in the Software without restriction, including without limitation the rights

to use, copy, modify, merge, publish, distribute, sublicense, and/or sell

copies of the Software, and to permit persons to whom the Software is

furnished to do so, subject to the following conditions:



The above copyright notice and this permission notice shall be included in all

copies or substantial portions of the Software.



THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR

IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,

FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE

AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER

LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,

OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE

SOFTWARE.

*/

#endregion



#if UNITY_2017_3 || UNITY_2017_4 || UNITY_2018 || UNITY_2019

#define UNITY_MESH_INDEXFORMAT_SUPPORT

#endif



using System;
using System.Collections.Generic;

using System.Linq;

using UnityEngine;



namespace UnityMeshSimplifier

{

    /// <summary>

    /// Contains methods for combining meshes.

    /// </summary>

    public static class MeshCombiner
    {


        #region Public Methods

        /// <summary>

        /// Combines an array of mesh renderers into one single mesh.

        /// </summary>

        /// <param name="rootTransform">The root transform to create the combine mesh based from, essentially the origin of the new mesh.</param>

        /// <param name="renderers">The array of mesh renderers to combine.</param>

        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>

        /// <returns>The combined mesh.</returns>

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

                if(hasUnknownRootTransform)
                {
                    rootTransform = topLevelParents[rendererTransform];              
                }

                transforms[i] = rootTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;

                Vector3 lossyScale = rendererTransform.transform.lossyScale;
                bool isUniformScale = Mathf.Approximately(lossyScale.x, lossyScale.y) && Mathf.Approximately(lossyScale.y, lossyScale.z);

                if (!isUniformScale)
                {
                    Debug.LogWarning($"The GameObject \"{rendererTransform.name}\" has non uniform scaling applied. This will cause the combined mesh normals to be incorrectly calculated resulting in slight variation in lighting.");
                }

                normalsTransforms[i] = Tuple.Create(rootTransform.localToWorldMatrix * rendererTransform.localToWorldMatrix, !isUniformScale);

                materials[i] = renderer.sharedMaterials;
            }


            return CombineMeshes(meshes, transforms, normalsTransforms, materials, out resultMaterials, blendShapes);

        }


        /// <summary>

        /// Combines an array of skinned mesh renderers into one single skinned mesh.

        /// </summary>

        /// <param name="rootTransform">The root transform to create the combine mesh based from, essentially the origin of the new mesh.</param>

        /// <param name="renderers">The array of skinned mesh renderers to combine.</param>

        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>

        /// <param name="resultBones">The resulting bones for the combined mesh.</param>

        /// <returns>The combined mesh.</returns>

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

                // baw did
                if (renderer.bones == null || renderer.bones.Length == 0)
                {
                    transforms[i] = rootTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                }
                else
                {
                    transforms[i] = rendererTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                }

                Vector3 lossyScale = rendererTransform.transform.lossyScale;
                bool isUniformScale = Mathf.Approximately(lossyScale.x, lossyScale.y) && Mathf.Approximately(lossyScale.y, lossyScale.z);

                if (!isUniformScale)
                {
                    Debug.LogWarning($"The GameObject \"{rendererTransform.name}\" has non uniform scaling applied. This will cause the combined mesh normals to be incorrectly calculated resulting in slight variation in lighting.");
                }

                normalsTransforms[i] = Tuple.Create(rendererTransform.worldToLocalMatrix * rendererTransform.localToWorldMatrix, !isUniformScale);

                materials[i] = renderer.sharedMaterials;

                bones[i] = renderer.bones;

                for (int a = 0; a < bones[i].Length; a++)
                {
                    Transform t = bones[i][a];
                    MeshFilter mf = t == null ? null : t.GetComponent<MeshFilter>();
                    Mesh m = mf == null ? null : mf.sharedMesh;

                    if(m != null)
                    {
                        Debug.LogWarning($"You have a static mesh attached to the bone:\"{t.name}\". The mesh combination logic will not deal with this properly, since that would require it to modify the original game object hierarchy. You might get erroneous results on mesh combination.");
                    }             
                }


                Mesh mesh = renderer.sharedMesh;

                if (mesh.blendShapeCount > 0)
                {
                    for (int s = 0; s < mesh.blendShapeCount; s++)
                    {
                        for (int f = 0; f < mesh.GetBlendShapeFrameCount(s); f++)
                        {
                            Vector3[] deltaVertices = new Vector3[mesh.vertexCount];
                            Vector3[] deltaNormals = new Vector3[mesh.vertexCount];
                            Vector3[] deltaTangents = new Vector3[mesh.vertexCount];
                            int rendererId = renderer.GetHashCode();

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


        /// <summary>

        /// Combines an array of meshes into a single mesh.

        /// </summary>

        /// <param name="meshes">The array of meshes to combine.</param>

        /// <param name="transforms">The array of transforms for the meshes.</param>

        /// <param name="materials">The array of materials for each mesh to combine.</param>

        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>

        /// <returns>The combined mesh.</returns>

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



        /// <summary>

        /// Combines an array of meshes into a single mesh.

        /// </summary>

        /// <param name="meshes">The array of meshes to combine.</param>

        /// <param name="transforms">The array of transforms for the meshes.</param>

        /// <param name="materials">The array of materials for each mesh to combine.</param>

        /// <param name="bones">The array of bones for each mesh to combine.</param>

        /// <param name="resultMaterials">The resulting materials for the combined mesh.</param>

        /// <param name="resultBones">The resulting bones for the combined mesh.</param>

        /// <returns>The combined mesh.</returns>

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

                        throw new System.ArgumentException(string.Format("The material at index {0} for mesh at index {1} is null.", materialIndex, meshIndex), nameof(materials));

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

            if(blendShapes != null && blendShapes.Count > 0)
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



            return combinedMesh;

        }


        #endregion



        #region Private Methods


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

        #endregion

    }

}
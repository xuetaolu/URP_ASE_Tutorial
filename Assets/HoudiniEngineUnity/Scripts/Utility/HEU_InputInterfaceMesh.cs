/*
 * Copyright (c) <2020> Side Effects Software Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *
 * 2. The name of Side Effects Software may not be used to endorse or
 *    promote products derived from this software without specific prior
 *    written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY SIDE EFFECTS SOFTWARE "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN
 * NO EVENT SHALL SIDE EFFECTS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;

    [System.Serializable]
    public class HEU_InputInterfaceMeshSettings
    {
        public bool ExportColliders
        {
            get => _exportColliders;
            set => _exportColliders = value;
        }

        [SerializeField] private bool _exportColliders = false;
    };


    /// <summary>
    /// This class provides functionality for uploading Unity mesh data from gameobjects
    /// into Houdini through an input node.
    /// It derives from the HEU_InputInterface and registers with HEU_InputUtility so that it
    /// can be used automatically when uploading mesh data.
    /// </summary>
    public class HEU_InputInterfaceMesh : HEU_InputInterface
    {
#if UNITY_EDITOR
        /// <summary>
        /// Registers this input inteface for Unity meshes on
        /// the callback after scripts are reloaded in Unity.
        /// </summary>
        [InitializeOnLoadMethod]
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            HEU_InputInterfaceMesh inputInterface = new HEU_InputInterfaceMesh();
            HEU_InputUtility.RegisterInputInterface(inputInterface);
        }
#endif

        private HEU_InputInterfaceMeshSettings settings;

        private HEU_InputInterfaceMesh() : base(priority: DEFAULT_PRIORITY)
        {
        }

        public void Initialize(HEU_InputInterfaceMeshSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Creates a mesh input node and uploads the mesh data from inputObject.
        /// </summary>
        /// <param name="session">Session that connectNodeID exists in</param>
        /// <param name="connectNodeID">The node to connect the network to. Most likely a SOP/merge node</param>
        /// <param name="inputObject">The gameobject containing the mesh components</param>
        /// <param name="inputNodeID">The created input node ID</param>
        /// <returns>True if created network and uploaded mesh data.</returns>
        public override bool CreateInputNodeWithDataUpload(HEU_SessionBase session, HAPI_NodeId connectNodeID,
            GameObject inputObject, out HAPI_NodeId inputNodeID)
        {
            inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            // Create input node, cook it, then upload the geometry data

            if (!HEU_HAPIUtility.IsNodeValidInHoudini(session, connectNodeID))
            {
                HEU_Logger.LogError("Connection node is invalid.");
                return false;
            }

            bool bExportColliders = settings != null && settings.ExportColliders == true;

            // Get upload meshes from input object
            HEU_InputDataMeshes inputMeshes = GenerateMeshDatasFromGameObject(inputObject, bExportColliders);
            if (inputMeshes == null || inputMeshes._inputMeshes == null || inputMeshes._inputMeshes.Count == 0)
            {
                HEU_Logger.LogError("No valid meshes found on input objects.");
                return false;
            }

            string inputName = null;
            HAPI_NodeId newNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            session.CreateInputNode(out newNodeID, inputName);
            if (newNodeID == HEU_Defines.HEU_INVALID_NODE_ID ||
                !HEU_HAPIUtility.IsNodeValidInHoudini(session, newNodeID))
            {
                HEU_Logger.LogError("Failed to create new input node in Houdini session!");
                return false;
            }

            inputNodeID = newNodeID;

            if (!UploadData(session, inputNodeID, inputMeshes))
            {
                if (!session.CookNode(inputNodeID, false))
                {
                    HEU_Logger.LogError("New input node failed to cook!");
                    return false;
                }

                return false;
            }

            bool createMergeNode = false;
            HAPI_NodeId mergeNodeId = HEU_Defines.HEU_INVALID_NODE_ID;

            if (bExportColliders)
            {
                createMergeNode = true;
            }

            if (!createMergeNode)
            {
                return true;
            }

            HAPI_NodeId parentId = HEU_HAPIUtility.GetParentNodeID(session, newNodeID);

            if (!session.CreateNode(parentId, "merge", null, false, out mergeNodeId))
            {
                HEU_Logger.LogErrorFormat("Unable to create merge SOP node for connecting input assets.");
                return false;
            }

            if (!session.ConnectNodeInput(mergeNodeId, 0, newNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to connect to input node!");
                return false;
            }

            if (!session.SetNodeDisplay(mergeNodeId, 1))
            {
                HEU_Logger.LogWarningFormat("Unable to set display flag!");
            }

            inputNodeID = mergeNodeId;

            if (bExportColliders)
            {
                if (!UploadColliderData(session, mergeNodeId, inputMeshes, parentId))
                {
                    return false;
                }
            }

            if (!session.CookNode(inputNodeID, false))
            {
                HEU_Logger.LogError("New input node failed to cook!");
                return false;
            }

            return true;
        }

        public override bool IsThisInputObjectSupported(GameObject inputObject)
        {
            if (inputObject != null)
            {
                if (inputObject.GetComponent<LODGroup>() != null)
                {
                    return true;
                }
                else if (inputObject.GetComponentInChildren<MeshFilter>(true) != null)
                {
                    return true;
                }
                else if (inputObject.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static void GetUVsFromMesh(Mesh mesh, Vector2[] srcUVs, List<Vector3> destUVs, int index)
        {
            destUVs.Clear();
            if (srcUVs != null && srcUVs.Length > 0)
            {
                mesh.GetUVs(index, destUVs);
            }
        }

        /// <summary>
        /// Upload the inputData (mesh geometry) into the input node with inputNodeID.
        /// </summary>
        /// <param name="session">Session that the input node exists in</param>
        /// <param name="inputNodeID">ID of the input node</param>
        /// <param name="inputData">Container of the mesh geometry</param>
        /// <returns>True if successfully uploaded data</returns>
        public bool UploadData(HEU_SessionBase session, HAPI_NodeId inputNodeID, HEU_InputData inputData)
        {
            HEU_InputDataMeshes inputDataMeshes = inputData as HEU_InputDataMeshes;
            if (inputDataMeshes == null)
            {
                HEU_Logger.LogError("Expected HEU_InputDataMeshes type for inputData, but received unsupported type.");
                return false;
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Color> colors = new List<Color>();

#if UNITY_2018_2_OR_NEWER
            const int NumUVSets = 8;
#else
	    const int NumUVSets = 4;
#endif
            List<Vector3>[] uvs = new List<Vector3>[NumUVSets];
            for (int u = 0; u < NumUVSets; ++u)
            {
                uvs[u] = new List<Vector3>();
            }

            // Use tempUVs to help with reindexing
            List<Vector3>[] tempUVs = new List<Vector3>[NumUVSets];
            for (int u = 0; u < NumUVSets; ++u)
            {
                tempUVs[u] = new List<Vector3>();
            }

            List<int> pointIndexList = new List<int>();
            List<int> vertIndexList = new List<int>();

            int numMaterials = 0;

            int numMeshes = inputDataMeshes._inputMeshes.Count;

            // Get the parent's world transform, so when there are multiple child meshes,
            // can merge and apply their local transform after subtracting their parent's world transform
            Matrix4x4 rootInvertTransformMatrix = Matrix4x4.identity;
            if (numMeshes > 1)
            {
                rootInvertTransformMatrix = inputDataMeshes._inputObject.transform.worldToLocalMatrix;
            }

            // Always using the first submesh topology. This doesn't support mixed topology (triangles and quads).
            MeshTopology meshTopology = inputDataMeshes._inputMeshes[0]._mesh.GetTopology(0);

            int numVertsPerFace = 3;
            if (meshTopology == MeshTopology.Quads)
            {
                numVertsPerFace = 4;
            }

            // For all meshes:
            // Accumulate vertices, normals, uvs, colors, and indices.
            // Keep track of indices start and count for each mesh for later when uploading material assignments and groups.
            // Find shared vertices, and use unique set of vertices to use as point positions.
            // Need to reindex indices for both unique vertices, as well as vertex attributes.
            for (int i = 0; i < numMeshes; ++i)
            {
                Vector3[] meshVertices = inputDataMeshes._inputMeshes[i]._mesh.vertices;
                Matrix4x4 localToWorld = rootInvertTransformMatrix *
                                         inputDataMeshes._inputMeshes[i]._transform.localToWorldMatrix;

                List<Vector3> uniqueVertices = new List<Vector3>();

                // Keep track of old vertex positions (old vertex slot points to new unique vertex slot)
                int[] reindexVertices = new int[meshVertices.Length];
                Dictionary<Vector3, int> reindexMap = new Dictionary<Vector3, int>();

                // For each vertex, check against subsequent vertices for shared positions.
                for (int a = 0; a < meshVertices.Length; ++a)
                {
                    Vector3 va = meshVertices[a];

                    if (!reindexMap.ContainsKey(va))
                    {
                        if (numMeshes > 1 && !inputDataMeshes._hasLOD)
                        {
                            // For multiple meshes that are not LODs, apply local transform on vertices to get the merged mesh.
                            uniqueVertices.Add(localToWorld.MultiplyPoint(va));
                        }
                        else
                        {
                            uniqueVertices.Add(va);
                        }

                        // Reindex to point to unique vertex slot
                        reindexVertices[a] = uniqueVertices.Count - 1;
                        reindexMap[va] = uniqueVertices.Count - 1;
                    }
                    else
                    {
                        reindexVertices[a] = reindexMap[va];
                    }
                }

                int vertexOffset = vertices.Count;
                vertices.AddRange(uniqueVertices);

                Vector3[] meshNormals = inputDataMeshes._inputMeshes[i]._mesh.normals;
                Color[] meshColors = inputDataMeshes._inputMeshes[i]._mesh.colors;

                // This is really silly. mesh.GetUVs gives uvs regardless if they exist or not (makes duplicates of
                // first uv if they don't exist), but mesh.uv* gives correct UVs, but in Vector2 format.
                // Since we need to convert to Vector3 later, this checks mesh.uv*, then uses mesh.GetUVs to get in Vector3.
                // Note skipping uv1 as its internally used (i.e. the 2nd uv set is uv2)
                int uindex = 0;
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv,
                    tempUVs[0], uindex++);
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv2,
                    tempUVs[1], uindex++);
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv3,
                    tempUVs[2], uindex++);
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv4,
                    tempUVs[3], uindex++);
#if UNITY_2018_2_OR_NEWER
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv5,
                    tempUVs[4], uindex++);
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv6,
                    tempUVs[5], uindex++);
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv7,
                    tempUVs[6], uindex++);
                GetUVsFromMesh(inputDataMeshes._inputMeshes[i]._mesh, inputDataMeshes._inputMeshes[i]._mesh.uv8,
                    tempUVs[7], uindex++);
#endif

                inputDataMeshes._inputMeshes[i]._indexStart = new uint[inputDataMeshes._inputMeshes[i]._numSubMeshes];
                inputDataMeshes._inputMeshes[i]._indexCount = new uint[inputDataMeshes._inputMeshes[i]._numSubMeshes];

                // For each submesh:
                // Generate face to point index -> pointIndexList
                // Generate face to vertex attribute index -> vertIndexList
                for (int j = 0; j < inputDataMeshes._inputMeshes[i]._numSubMeshes; ++j)
                {
                    int indexStart = pointIndexList.Count;
                    int vertIndexStart = vertIndexList.Count;

                    // Indices have to be re-indexed with our own offset 
                    // (using GetIndices to generalize triangles and quad indices)
                    int[] meshIndices = inputDataMeshes._inputMeshes[i]._mesh.GetIndices(j);
                    int numIndices = meshIndices.Length;
                    for (int k = 0; k < numIndices; ++k)
                    {
                        int originalIndex = meshIndices[k];
                        meshIndices[k] = reindexVertices[originalIndex];

                        pointIndexList.Add(vertexOffset + meshIndices[k]);
                        vertIndexList.Add(vertIndexStart + k);

                        if (meshNormals != null && (originalIndex < meshNormals.Length))
                        {
                            normals.Add(meshNormals[originalIndex]);
                        }

                        for (int u = 0; u < NumUVSets; ++u)
                        {
                            if (tempUVs[u].Count > 0)
                            {
                                uvs[u].Add(tempUVs[u][originalIndex]);
                            }
                        }

                        if (meshColors != null && (originalIndex < meshColors.Length))
                        {
                            colors.Add(meshColors[originalIndex]);
                        }
                    }

                    inputDataMeshes._inputMeshes[i]._indexStart[j] = (uint)indexStart;
                    inputDataMeshes._inputMeshes[i]._indexCount[j] = (uint)(pointIndexList.Count) -
                                                                     inputDataMeshes._inputMeshes[i]._indexStart[j];
                }

                numMaterials += inputDataMeshes._inputMeshes[i]._materials != null
                    ? inputDataMeshes._inputMeshes[i]._materials.Length
                    : 0;
            }

            // It is possible for some meshes to not have normals/uvs/colors while others do.
            // In the case where an attribute is missing on some meshes, we clear out those attributes so we don't upload
            // partial attribute data.
            int totalAllVertexCount = vertIndexList.Count;
            if (normals.Count != totalAllVertexCount)
            {
                normals = null;
            }

            if (colors.Count != totalAllVertexCount)
            {
                colors = null;
            }

            HAPI_PartInfo partInfo = new HAPI_PartInfo();
            partInfo.faceCount = vertIndexList.Count / numVertsPerFace;
            partInfo.vertexCount = vertIndexList.Count;
            partInfo.pointCount = vertices.Count;
            partInfo.pointAttributeCount = 1;
            partInfo.vertexAttributeCount = 0;
            partInfo.primitiveAttributeCount = 0;
            partInfo.detailAttributeCount = 0;

            //HEU_Logger.LogFormat("Faces: {0}; Vertices: {1}; Verts/Face: {2}", partInfo.faceCount, partInfo.vertexCount, numVertsPerFace);

            if (normals != null && normals.Count > 0)
            {
                partInfo.vertexAttributeCount++;
            }

            for (int u = 0; u < NumUVSets; ++u)
            {
                if (uvs[u].Count > 0 && uvs[u].Count == totalAllVertexCount)
                {
                    partInfo.vertexAttributeCount++;
                }
                else
                {
                    uvs[u].Clear();
                }
            }

            if (colors != null && colors.Count > 0)
            {
                partInfo.vertexAttributeCount++;
            }

            if (numMaterials > 0)
            {
                partInfo.primitiveAttributeCount++;
            }

            if (numMeshes > 0)
            {
                partInfo.primitiveAttributeCount++;
            }

            if (inputDataMeshes._hasLOD)
            {
                partInfo.primitiveAttributeCount++;
                partInfo.detailAttributeCount++;
            }

            HAPI_GeoInfo displayGeoInfo = new HAPI_GeoInfo();
            if (!session.GetDisplayGeoInfo(inputNodeID, ref displayGeoInfo))
            {
                return false;
            }

            HAPI_NodeId displayNodeID = displayGeoInfo.nodeId;

            if (!session.SetPartInfo(displayNodeID, 0, ref partInfo))
            {
                HEU_Logger.LogError("Failed to set input part info. ");
                return false;
            }

            int[] faceCounts = new int[partInfo.faceCount];
            for (int i = 0; i < partInfo.faceCount; ++i)
            {
                faceCounts[i] = numVertsPerFace;
            }

            int[] faceIndices = pointIndexList.ToArray();

            if (!HEU_GeneralUtility.SetArray2Arg(displayNodeID, 0, session.SetFaceCount, faceCounts, 0,
                    partInfo.faceCount))
            {
                HEU_Logger.LogError("Failed to set input geometry face counts.");
                return false;
            }

            if (!HEU_GeneralUtility.SetArray2Arg(displayNodeID, 0, session.SetVertexList, faceIndices, 0,
                    partInfo.vertexCount))
            {
                HEU_Logger.LogError("Failed to set input geometry indices.");
                return false;
            }

            if (!HEU_InputMeshUtility.SetMeshPointAttribute(session, displayNodeID, 0,
                    HEU_HAPIConstants.HAPI_ATTRIB_POSITION, 3, vertices.ToArray(), ref partInfo, true))
            {
                HEU_Logger.LogError("Failed to set input geometry position.");
                return false;
            }

            int[] vertIndices = vertIndexList.ToArray();

            //if(normals != null && !SetMeshPointAttribute(session, displayNodeID, 0, HEU_Defines.HAPI_ATTRIB_NORMAL, 3, normals.ToArray(), ref partInfo, true))
            if (normals != null && !HEU_InputMeshUtility.SetMeshVertexAttribute(session, displayNodeID, 0,
                    HEU_HAPIConstants.HAPI_ATTRIB_NORMAL, 3, normals.ToArray(), vertIndices, ref partInfo, true))
            {
                HEU_Logger.LogError("Failed to set input geometry normals.");
                return false;
            }

            for (int u = 0; u < NumUVSets; ++u)
            {
                if (uvs[u].Count > 0)
                {
                    // Skip uv1 as its used internally. So it goes: uv, uv2, ..., uv8
                    string uvName = u == 0
                        ? HEU_HAPIConstants.HAPI_ATTRIB_UV
                        : string.Format("{0}{1}", HEU_HAPIConstants.HAPI_ATTRIB_UV, u + 1);
                    if (!HEU_InputMeshUtility.SetMeshVertexAttribute(session, displayNodeID, 0, uvName, 3,
                            uvs[u].ToArray(), vertIndices, ref partInfo, false))
                    {
                        HEU_Logger.LogError("Failed to set input geometry UV" + u);
                        return false;
                    }
                }
            }

            if (colors != null && colors.Count > 0)
            {
                Vector3[] rgb = new Vector3[colors.Count];
                float[] alpha = new float[colors.Count];
                for (int i = 0; i < colors.Count; ++i)
                {
                    rgb[i][0] = colors[i].r;
                    rgb[i][1] = colors[i].g;
                    rgb[i][2] = colors[i].b;

                    alpha[i] = colors[i].a;
                }

                //if(!SetMeshPointAttribute(session, displayNodeID, 0, HEU_Defines.HAPI_ATTRIB_COLOR, 3, rgb, ref partInfo, false))
                if (!HEU_InputMeshUtility.SetMeshVertexAttribute(session, displayNodeID, 0,
                        HEU_HAPIConstants.HAPI_ATTRIB_COLOR, 3, rgb, vertIndices, ref partInfo, false))
                {
                    HEU_Logger.LogError("Failed to set input geometry colors.");
                    return false;
                }

                //if(!SetMeshPointAttribute(session, displayNodeID, 0, HEU_Defines.HAPI_ATTRIB_ALPHA, 1, alpha, ref partInfo, false))
                if (!HEU_InputMeshUtility.SetMeshVertexFloatAttribute(session, displayNodeID, 0,
                        HEU_Defines.HAPI_ATTRIB_ALPHA, 1, alpha, vertIndices, ref partInfo))
                {
                    HEU_Logger.LogError("Failed to set input geometry color alpha.");
                    return false;
                }
            }

            // Set material names for round-trip perservation of material assignment
            // Each HEU_UploadMeshData might have a list of submeshes and materials
            // These are all combined into a single mesh, with group names
            if (numMaterials > 0)
            {
                bool bFoundAtleastOneValidMaterial = false;

                string[] materialIDs = new string[partInfo.faceCount];
                for (int g = 0; g < inputDataMeshes._inputMeshes.Count; ++g)
                {
                    if (inputDataMeshes._inputMeshes[g]._numSubMeshes !=
                        inputDataMeshes._inputMeshes[g]._materials.Length)
                    {
                        // Number of submeshes should equal number of materials since materials determine submeshes
                        continue;
                    }

                    for (int i = 0; i < inputDataMeshes._inputMeshes[g]._materials.Length; ++i)
                    {
                        string materialName =
                            HEU_AssetDatabase.GetAssetPathWithSubAssetSupport(inputDataMeshes._inputMeshes[g]
                                ._materials[i]);
                        if (materialName == null)
                        {
                            materialName = "";
                        }
                        else if (materialName.StartsWith(HEU_Defines.DEFAULT_UNITY_BUILTIN_RESOURCES))
                        {
                            materialName =
                                HEU_AssetDatabase.GetUniqueAssetPathForUnityAsset(inputDataMeshes._inputMeshes[g]
                                    ._materials[i]);
                        }

                        bFoundAtleastOneValidMaterial |= !string.IsNullOrEmpty(materialName);

                        int faceStart = (int)inputDataMeshes._inputMeshes[g]._indexStart[i] / numVertsPerFace;
                        int faceEnd = faceStart +
                                      ((int)inputDataMeshes._inputMeshes[g]._indexCount[i] / numVertsPerFace);
                        for (int m = faceStart; m < faceEnd; ++m)
                        {
                            materialIDs[m] = materialName;
                        }
                    }
                }

                if (bFoundAtleastOneValidMaterial)
                {
                    HAPI_AttributeInfo materialIDAttrInfo = new HAPI_AttributeInfo();
                    materialIDAttrInfo.exists = true;
                    materialIDAttrInfo.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM;
                    materialIDAttrInfo.storage = HAPI_StorageType.HAPI_STORAGETYPE_STRING;
                    materialIDAttrInfo.count = partInfo.faceCount;
                    materialIDAttrInfo.tupleSize = 1;
                    materialIDAttrInfo.originalOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;

                    if (!session.AddAttribute(displayNodeID, 0, HEU_PluginSettings.UnityMaterialAttribName,
                            ref materialIDAttrInfo))
                    {
                        HEU_Logger.LogError("Failed to add input geometry unity material name attribute.");
                        return false;
                    }

                    if (!HEU_GeneralUtility.SetAttributeArray(displayNodeID, 0,
                            HEU_PluginSettings.UnityMaterialAttribName, ref materialIDAttrInfo, materialIDs,
                            session.SetAttributeStringData, partInfo.faceCount))
                    {
                        HEU_Logger.LogError("Failed to set input geometry unity material name.");
                        return false;
                    }
                }
            }

            // Set mesh name attribute
            HAPI_AttributeInfo attrInfo = new HAPI_AttributeInfo();
            attrInfo.exists = true;
            attrInfo.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM;
            attrInfo.storage = HAPI_StorageType.HAPI_STORAGETYPE_STRING;
            attrInfo.count = partInfo.faceCount;
            attrInfo.tupleSize = 1;
            attrInfo.originalOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;

            if (session.AddAttribute(displayNodeID, 0, HEU_PluginSettings.UnityInputMeshAttr, ref attrInfo))
            {
                string[] primitiveNameAttr = new string[partInfo.faceCount];

                for (int g = 0; g < inputDataMeshes._inputMeshes.Count; ++g)
                {
                    for (int i = 0; i < inputDataMeshes._inputMeshes[g]._numSubMeshes; ++i)
                    {
                        int faceStart = (int)inputDataMeshes._inputMeshes[g]._indexStart[i] / numVertsPerFace;
                        int faceEnd = faceStart +
                                      ((int)inputDataMeshes._inputMeshes[g]._indexCount[i] / numVertsPerFace);
                        for (int m = faceStart; m < faceEnd; ++m)
                        {
                            primitiveNameAttr[m] = inputDataMeshes._inputMeshes[g]._meshPath;
                        }
                    }
                }

                if (!HEU_GeneralUtility.SetAttributeArray(displayNodeID, 0, HEU_PluginSettings.UnityInputMeshAttr,
                        ref attrInfo, primitiveNameAttr, session.SetAttributeStringData, partInfo.faceCount))
                {
                    HEU_Logger.LogError("Failed to set input geometry unity mesh name.");
                    return false;
                }
            }
            else
            {
                return false;
            }

            // Set LOD group membership
            if (inputDataMeshes._hasLOD)
            {
                int[] membership = new int[partInfo.faceCount];

                for (int g = 0; g < inputDataMeshes._inputMeshes.Count; ++g)
                {
                    if (g > 0)
                    {
                        // Clear array
                        for (int m = 0; m < partInfo.faceCount; ++m)
                        {
                            membership[m] = 0;
                        }
                    }

                    // Set 1 for faces belonging to this group
                    for (int s = 0; s < inputDataMeshes._inputMeshes[g]._numSubMeshes; ++s)
                    {
                        int faceStart = (int)inputDataMeshes._inputMeshes[g]._indexStart[s] / numVertsPerFace;
                        int faceEnd = faceStart +
                                      ((int)inputDataMeshes._inputMeshes[g]._indexCount[s] / numVertsPerFace);
                        for (int m = faceStart; m < faceEnd; ++m)
                        {
                            membership[m] = 1;
                        }
                    }

                    string groupName = inputDataMeshes._inputMeshes[g]._meshName;
                    if (!groupName.StartsWith(HEU_Defines.HEU_DEFAULT_LOD_NAME))
                    {
                        groupName = HEU_Defines.HEU_DEFAULT_LOD_NAME + g + "_" + groupName;
                    }

                    groupName = HEU_HAPIUtility.ToHapiVariableName(groupName);

                    if (!session.AddGroup(displayNodeID, 0, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, groupName))
                    {
                        HEU_Logger.LogError("Failed to add input geometry LOD group name.");
                        return false;
                    }

                    if (!session.SetGroupMembership(displayNodeID, 0, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, groupName,
                            membership, 0, partInfo.faceCount))
                    {
                        HEU_Logger.LogError("Failed to set input geometry LOD group name.");
                        return false;
                    }
                }
            }

            return session.CommitGeo(displayNodeID);
        }

        internal bool UploadColliderData(HEU_SessionBase session, HAPI_NodeId mergeNodeID,
            HEU_InputDataMeshes inputData, HAPI_NodeId parentNodeId)
        {
            // The input to put on
            int inputIndex = 1;

            foreach (HEU_InputDataMesh inputMesh in inputData._inputMeshes)
            {
                if (inputMesh == null || inputMesh._colliders == null)
                {
                    continue;
                }

                foreach (HEU_InputDataCollider colliderData in inputMesh._colliders)
                {
                    if (colliderData == null || colliderData._collider == null ||
                        colliderData._colliderType == HEU_InputColliderType.NONE)
                    {
                        continue;
                    }

                    HAPI_NodeId newInputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

                    switch (colliderData._colliderType)
                    {
                        case HEU_InputColliderType.BOX:
                            BoxCollider boxCollider = colliderData._collider as BoxCollider;
                            if (!boxCollider || !UploadBoxColliderData(session, boxCollider, inputIndex, parentNodeId,
                                    out newInputNodeID))
                            {
                                HEU_Logger.LogWarning("Invalid collider input!");
                                continue;
                            }

                            break;
                        case HEU_InputColliderType.SPHERE:
                            SphereCollider sphereCollider = colliderData._collider as SphereCollider;
                            if (!sphereCollider || !UploadSphereColliderData(session, sphereCollider, inputIndex,
                                    parentNodeId, out newInputNodeID))
                            {
                                HEU_Logger.LogWarning("Invalid collider input!");
                                continue;
                            }

                            break;
                        case HEU_InputColliderType.CAPSULE:
                            CapsuleCollider capsuleCollider = colliderData._collider as CapsuleCollider;
                            if (!capsuleCollider || !UploadCapsuleColliderData(session, capsuleCollider, inputIndex,
                                    parentNodeId, out newInputNodeID))
                            {
                                HEU_Logger.LogWarning("Invalid collider input!");
                                return false;
                            }

                            break;
                        case HEU_InputColliderType.MESH:
                            MeshCollider meshCollider = colliderData._collider as MeshCollider;
                            if (!meshCollider || !UploadMeshColliderData(session, meshCollider, inputIndex,
                                    parentNodeId, out newInputNodeID))
                            {
                                HEU_Logger.LogWarning("Invalid collider input!");
                                return false;
                            }

                            break;
                        default:
                            HEU_Logger.LogWarning("Invalid collider type!");
                            return false;
                    }

                    if (newInputNodeID == HEU_Defines.HEU_INVALID_NODE_ID) continue;

                    if (!session.ConnectNodeInput(mergeNodeID, inputIndex, newInputNodeID))
                    {
                        HEU_Logger.LogErrorFormat("Unable to connect to input node!");
                        return false;
                    }

                    inputIndex++;
                }
            }

            return true;
        }

        internal bool UploadBoxColliderData(HEU_SessionBase session, BoxCollider collider, int inputIndex,
            HAPI_NodeId parentNodeID, out HAPI_NodeId inputNodeID)
        {
            inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!collider) return false;

            HAPI_NodeId boxNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            string name = string.Format("Box{0}", inputIndex);

            Vector3 center = HEU_HAPIUtility.ConvertPositionUnityToHoudini(collider.center);
            Vector3 size = HEU_HAPIUtility.ConvertScaleUnityToHoudini(collider.size);

            if (!session.CreateNode(parentNodeID, "box", null, false, out boxNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to create merge box node for connecting input assets.");
                return false;
            }

            string sizeParamName = "size";
            if (!session.SetParamFloatValue(boxNodeID, sizeParamName, 0, size.x))
                return false;
            if (!session.SetParamFloatValue(boxNodeID, sizeParamName, 1, size.y))
                return false;
            if (!session.SetParamFloatValue(boxNodeID, sizeParamName, 2, size.z))
                return false;

            string transformParamName = "t";
            if (!session.SetParamFloatValue(boxNodeID, transformParamName, 0, center.x))
                return false;
            if (!session.SetParamFloatValue(boxNodeID, transformParamName, 1, center.y))
                return false;
            if (!session.SetParamFloatValue(boxNodeID, transformParamName, 2, center.z))
                return false;

            if (!session.CookNode(boxNodeID, false))
                return false;

            HAPI_NodeId groupNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            string groupName = string.Format("group{0}", inputIndex);

            if (!session.CreateNode(parentNodeID, "groupcreate", groupName, false, out groupNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to create group SOP node for connecting input assets.");
                return false;
            }

            HAPI_NodeId groupParmID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!session.GetParmIDFromName(groupNodeID, "groupname", out groupParmID) ||
                groupParmID == HEU_Defines.HEU_INVALID_NODE_ID) return false;

            string baseGroupName = GetColliderGroupBaseName(collider, bIsConvex: false, bIsSimple: true);
            string groupNameStr = string.Format("{0}_box{1}", baseGroupName, inputIndex);

            if (!session.SetParamStringValue(groupNodeID, groupNameStr, groupParmID, 0))
                return false;

            if (!session.ConnectNodeInput(groupNodeID, 0, boxNodeID))
                return false;

            inputNodeID = groupNodeID;

            return true;
        }

        internal bool UploadSphereColliderData(HEU_SessionBase session, SphereCollider collider, int inputIndex,
            HAPI_NodeId parentNodeID, out HAPI_NodeId inputNodeID)
        {
            inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!collider) return false;

            Vector3 center = HEU_HAPIUtility.ConvertPositionUnityToHoudini(collider.center);
            float radius = collider.radius;

            HAPI_NodeId sphereNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            string name = string.Format("Sphere{0}", inputIndex);

            if (!session.CreateNode(parentNodeID, "sphere", null, false, out sphereNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to create merge box node for connecting input assets.");
                return false;
            }

            string radParamName = "rad";
            if (!session.SetParamFloatValue(sphereNodeID, radParamName, 0, radius))
                return false;
            if (!session.SetParamFloatValue(sphereNodeID, radParamName, 1, radius))
                return false;
            if (!session.SetParamFloatValue(sphereNodeID, radParamName, 2, radius))
                return false;

            string transformParamName = "t";
            if (!session.SetParamFloatValue(sphereNodeID, transformParamName, 0, center.x))
                return false;
            if (!session.SetParamFloatValue(sphereNodeID, transformParamName, 1, center.y))
                return false;
            if (!session.SetParamFloatValue(sphereNodeID, transformParamName, 2, center.z))
                return false;

            string typeParamName = "type";
            if (!session.SetParamIntValue(sphereNodeID, typeParamName, 0, 1))
                return false;

            if (!session.CookNode(sphereNodeID, false))
                return false;

            HAPI_NodeId groupNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            string groupName = string.Format("group{0}", inputIndex);

            if (!session.CreateNode(parentNodeID, "groupcreate", groupName, false, out groupNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to create group SOP node for connecting input assets.");
                return false;
            }

            HAPI_NodeId groupParmID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!session.GetParmIDFromName(groupNodeID, "groupname", out groupParmID) ||
                groupParmID == HEU_Defines.HEU_INVALID_NODE_ID) return false;

            string baseGroupName = GetColliderGroupBaseName(collider, bIsConvex: false, bIsSimple: true);
            string groupNameStr = string.Format("{0}_sphere{1}", baseGroupName, inputIndex);
            if (!session.SetParamStringValue(groupNodeID, groupNameStr, groupParmID, 0))
                return false;

            if (!session.ConnectNodeInput(groupNodeID, 0, sphereNodeID))
                return false;

            inputNodeID = groupNodeID;

            return true;
        }

        internal bool UploadCapsuleColliderData(HEU_SessionBase session, CapsuleCollider collider, int inputIndex,
            HAPI_NodeId parentNodeID, out HAPI_NodeId inputNodeID)
        {
            inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!collider) return false;

            // Copied from Unreal FKSphylElem::GetElemSolid because exact Unity capsule source code is not available
            Vector3 sphereCenter = collider.center;
            float sphereRadius = collider.radius;
            float sphereLength = collider.height;
            // Height in Unreal is only the line segment. Height in Unity is the total length, so to get the line length, subtract 2 * rad
            sphereLength = Mathf.Max(sphereLength - 2 * sphereRadius, 0);

            int direction = collider.direction; // 0 = X, 1 = Y, 2 = Z. Default is Y

            // Unreal Y -> Unity X, Unreal Z -> Unity Y
            int numSides = 6;
            int numRings = (numSides / 2) + 1;

            int numVerts = (numSides + 1) * (numRings + 1);

            // Calculate the vertices for one arc
            Vector3[] arcVertices = new Vector3[numRings + 1];
            for (int ringIdx = 0; ringIdx < numRings + 1; ringIdx++)
            {
                float angle;
                float zOffset;
                if (ringIdx <= numSides / 4)
                {
                    angle = ((float)ringIdx / (numRings - 1)) * Mathf.PI;
                    zOffset = 0.5f * sphereLength;
                }
                else
                {
                    angle = ((float)(ringIdx - 1) / (numRings - 1)) * Mathf.PI;
                    zOffset = -0.5f * sphereLength;
                }

                // Note- unit sphere, so position always has mag of one. We can just use it for normal!
                Vector3 spherePos = new Vector3();
                spherePos.x = sphereRadius * Mathf.Sin(angle);
                spherePos.y = sphereRadius * Mathf.Cos(angle);
                spherePos.z = 0;

                arcVertices[ringIdx] = spherePos + new Vector3(0, zOffset, 0);
            }

            Vector3 directionRotationEuler = Vector3.zero;
            if (direction == 1)
            {
                // Y axis - This is the default after Unity unit conversion
                directionRotationEuler = Vector3.zero;
            }
            else if (direction == 0)
            {
                // X axis - Rotate around Z
                directionRotationEuler = new Vector3(0, 0, 90);
            }
            else if (direction == 2)
            {
                // Z axis - Rotate around X
                directionRotationEuler = new Vector3(90, 0, 0);
            }

            Quaternion directionRotation = Quaternion.Euler(directionRotationEuler);

            // Get the transform matrix for the rotation
            // Get the capsule vertices by rotating the arc NumSides+1 times

            float[] vertices = new float[numVerts * 3];
            for (int sideIdx = 0; sideIdx < numSides + 1; sideIdx++)
            {
                Vector3 arcEuler = new Vector3(0, 360.0f * ((float)sideIdx / (float)numSides), 0);
                Quaternion arcRot = Quaternion.Euler(arcEuler);

                for (int vertIdx = 0; vertIdx < numRings + 1; vertIdx++)
                {
                    int vIx = (numRings + 1) * sideIdx + vertIdx;
                    Vector3 arcVertex = arcRot * arcVertices[vertIdx];
                    arcVertex = directionRotation * arcVertex;

                    Vector3 curPosition = sphereCenter + arcVertex;
                    HEU_HAPIUtility.ConvertPositionUnityToHoudini(curPosition, out vertices[vIx * 3 + 0],
                        out vertices[vIx * 3 + 1], out vertices[vIx * 3 + 2]);
                }
            }

            int numIndices = numSides * numRings * 6;
            int[] indices = new int[numIndices];
            int curIndex = 0;

            for (int sideIdx = 0; sideIdx < numSides; sideIdx++)
            {
                int a0start = (sideIdx + 0) * (numRings + 1);
                int a1start = (sideIdx + 1) * (numRings + 1);
                for (int ringIdx = 0; ringIdx < numRings; ringIdx++)
                {
                    // First tri (reverse winding)
                    indices[curIndex + 0] = a0start + ringIdx + 0;
                    indices[curIndex + 2] = a1start + ringIdx + 0;
                    indices[curIndex + 1] = a0start + ringIdx + 1;
                    curIndex += 3;

                    // Second Tri (reverse winding)
                    indices[curIndex + 0] = a1start + ringIdx + 0;
                    indices[curIndex + 2] = a1start + ringIdx + 1;
                    indices[curIndex + 1] = a0start + ringIdx + 1;
                    curIndex += 3;
                }
            }

            HAPI_NodeId sphereNodeID = -1;
            string sphereName = string.Format("Sphyl{0}", inputIndex);

            if (!CreateInputNodeForCollider(session, out sphereNodeID, parentNodeID, inputIndex, sphereName, vertices,
                    indices))
                return false;

            if (!session.CookNode(sphereNodeID, false)) return false;

            HAPI_NodeId groupNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            string groupName = string.Format("group{0}", inputIndex);

            if (!session.CreateNode(parentNodeID, "groupcreate", groupName, false, out groupNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to create group SOP node for connecting input assets.");
                return false;
            }

            HAPI_NodeId groupParmID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!session.GetParmIDFromName(groupNodeID, "groupname", out groupParmID) ||
                groupParmID == HEU_Defines.HEU_INVALID_NODE_ID)
                return false;

            string baseGroupName = GetColliderGroupBaseName(collider, bIsConvex: false, bIsSimple: true);
            string groupNameStr = string.Format("{0}_capsule{1}", baseGroupName, inputIndex);

            if (!session.SetParamStringValue(groupNodeID, groupNameStr, groupParmID, 0))
                return false;

            if (!session.ConnectNodeInput(groupNodeID, 0, sphereNodeID))
                return false;

            inputNodeID = groupNodeID;

            return true;
        }

        internal bool UploadMeshColliderData(HEU_SessionBase session, MeshCollider collider, int inputIndex,
            HAPI_NodeId parentNodeID, out HAPI_NodeId inputNodeID)
        {
            inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!collider) return false;


            Mesh mesh = collider.sharedMesh;
            Vector3[] vertices = mesh.vertices;

            int numSubmeshes = mesh.subMeshCount;
            List<int> indices = new List<int>();
            for (int i = 0; i < numSubmeshes; i++)
            {
                int[] indicesForSubmesh = mesh.GetIndices(i);
                indices.AddRange(indicesForSubmesh);
            }

            int[] indicesArr = indices.ToArray();

            float[] verticesArr = new float[vertices.Length * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                HEU_HAPIUtility.ConvertPositionUnityToHoudini(vertices[i], out verticesArr[i * 3 + 0],
                    out verticesArr[i * 3 + 1], out verticesArr[i * 3 + 2]);
            }

            HAPI_NodeId meshNodeID = -1;
            string meshName = string.Format("MeshCollider{0}", inputIndex);

            if (!CreateInputNodeForCollider(session, out meshNodeID, parentNodeID, inputIndex, meshName, verticesArr,
                    indicesArr))
                return false;

            if (!session.CookNode(meshNodeID, false)) return false;

            HAPI_NodeId groupNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            string groupName = string.Format("group{0}", inputIndex);

            if (!session.CreateNode(parentNodeID, "groupcreate", groupName, false, out groupNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to create group SOP node for connecting input assets.");
                return false;
            }

            HAPI_NodeId groupParmID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!session.GetParmIDFromName(groupNodeID, "groupname", out groupParmID) ||
                groupParmID == HEU_Defines.HEU_INVALID_NODE_ID)
                return false;

            bool isConvex = collider.convex;
            string baseGroupName = GetColliderGroupBaseName(collider, bIsConvex: isConvex, bIsSimple: false);

            string groupNameStr = string.Format("{0}_mesh{1}", baseGroupName, inputIndex);
            if (!session.SetParamStringValue(groupNodeID, groupNameStr, groupParmID, 0))
                return false;

            if (!session.ConnectNodeInput(groupNodeID, 0, meshNodeID))
                return false;

            inputNodeID = groupNodeID;

            return true;
        }

        internal string GetColliderGroupBaseName(Collider collider, bool bIsConvex = false, bool bIsSimple = false,
            bool bIsRendered = false)
        {
            bool isTrigger = collider.isTrigger;
            string baseGroupName = "collision_geo";
            if (bIsConvex)
            {
                baseGroupName = "convex_" + baseGroupName;
            }

            if (bIsRendered)
            {
                baseGroupName = "rendered_" + baseGroupName;
            }

            if (bIsSimple)
            {
                baseGroupName = baseGroupName + "_simple";
            }

            if (isTrigger)
            {
                baseGroupName = baseGroupName + "_trigger";
            }

            return baseGroupName;
        }

        internal bool CreateInputNodeForCollider(HEU_SessionBase session, out HAPI_NodeId outNodeID,
            HAPI_NodeId parentNodeId, int colliderIndex, string colliderName, float[] colliderVertices,
            int[] colliderIndices)
        {
            outNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            HAPI_NodeId colliderNodeId = HEU_Defines.HEU_INVALID_NODE_ID;

            if (!session.CreateNode(parentNodeId, "null", colliderName, false, out colliderNodeId))
                return false;

            HAPI_PartInfo partInfo = new HAPI_PartInfo();
            partInfo.init();
            partInfo.id = 0;
            partInfo.nameSH = 0;
            partInfo.attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_POINT] = 0;
            partInfo.attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM] = 0;
            partInfo.attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX] = 0;
            partInfo.attributeCounts[(int)HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL] = 0;
            partInfo.vertexCount = colliderIndices.Length;
            partInfo.faceCount = colliderIndices.Length / 3;
            partInfo.pointCount = colliderVertices.Length / 3;
            partInfo.type = HAPI_PartType.HAPI_PARTTYPE_MESH;

            if (!session.SetPartInfo(colliderNodeId, 0, ref partInfo)) return false;

            HAPI_AttributeInfo attributeInfoPoint = new HAPI_AttributeInfo();
            attributeInfoPoint.count = colliderVertices.Length / 3;
            attributeInfoPoint.tupleSize = 3;
            attributeInfoPoint.exists = true;
            attributeInfoPoint.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
            attributeInfoPoint.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
            attributeInfoPoint.originalOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;

            if (!session.AddAttribute(colliderNodeId, 0, HEU_HAPIConstants.HAPI_ATTRIB_POSITION,
                    ref attributeInfoPoint))
                return false;

            if (!session.SetAttributeFloatData(colliderNodeId, 0, HEU_HAPIConstants.HAPI_ATTRIB_POSITION,
                    ref attributeInfoPoint, colliderVertices, 0, attributeInfoPoint.count))
                return false;

            if (!session.SetVertexList(colliderNodeId, 0, colliderIndices, 0, colliderIndices.Length))
                return false;

            int[] faceCounts = new int[partInfo.faceCount];
            for (int i = 0; i < faceCounts.Length; i++)
            {
                faceCounts[i] = 3;
            }

            if (!session.SetFaceCount(colliderNodeId, 0, faceCounts, 0, faceCounts.Length))
                return false;

            if (!session.CommitGeo(colliderNodeId))
                return false;

            outNodeID = colliderNodeId;

            return true;
        }

        /// <summary>
        /// Contains input geometry for multiple meshes.
        /// </summary>
        public class HEU_InputDataMeshes : HEU_InputData
        {
            public List<HEU_InputDataMesh> _inputMeshes = new List<HEU_InputDataMesh>();

            public bool _hasLOD;
        }

        public enum HEU_InputColliderType
        {
            NONE,
            BOX,
            SPHERE,
            CAPSULE,
            MESH
        }

        public class HEU_InputDataCollider
        {
            public Collider _collider;
            public HEU_InputColliderType _colliderType;
        }

        /// <summary>
        /// Contains input geometry for a single mesh.
        /// </summary>
        public class HEU_InputDataMesh
        {
            public Mesh _mesh;
            public Material[] _materials;

            public string _meshPath;
            public string _meshName;

            public int _numVertices;
            public int _numSubMeshes;

            // This keeps track of indices start and length for each submesh
            public uint[] _indexStart;
            public uint[] _indexCount;

            public float _LODScreenTransition;

            public Transform _transform;

            public List<HEU_InputDataCollider> _colliders;
        }

        /// <summary>
        /// Return an input data structure containing mesh data that needs to be
        /// uploaded from the given inputObject.
        /// Supports child gameobjects with meshes from the given inputObject.
        /// </summary>
        /// <param name="inputObject">GameObject containing mesh components</param>
        /// <returns>A valid input data strcuture containing mesh data</returns>
        public HEU_InputDataMeshes GenerateMeshDatasFromGameObject(GameObject inputObject,
            bool bExportColliders = false)
        {
            HEU_InputDataMeshes inputMeshes = new HEU_InputDataMeshes();
            inputMeshes._inputObject = inputObject;

            LODGroup lodGroup = inputObject.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                inputMeshes._hasLOD = true;

                LOD[] lods = lodGroup.GetLODs();
                for (int i = 0; i < lods.Length; ++i)
                {
                    if (lods[i].renderers != null && lods[i].renderers.Length > 0)
                    {
                        GameObject childGO = lods[i].renderers[0].gameObject;
                        HEU_InputDataMesh meshData = CreateSingleMeshData(childGO, bExportColliders);
                        if (meshData != null)
                        {
                            meshData._LODScreenTransition = lods[i].screenRelativeTransitionHeight;
                            inputMeshes._inputMeshes.Add(meshData);
                        }
                    }
                }
            }
            else
            {
                inputMeshes._hasLOD = false;

                // Create a HEU_InputDataMesh for each gameobject with a MeshFilter (including children)
                MeshFilter[] meshFilters = inputObject.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter filter in meshFilters)
                {
                    HEU_InputDataMesh meshData = CreateSingleMeshData(filter.gameObject, bExportColliders);
                    if (meshData != null)
                    {
                        inputMeshes._inputMeshes.Add(meshData);
                    }
                }


                SkinnedMeshRenderer[] skinnedMeshRenderers = inputObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer skinnedMeshRend in skinnedMeshRenderers)
                {
                    HEU_InputDataMesh meshData = CreateSingleMeshData(skinnedMeshRend.gameObject, bExportColliders);
                    if (meshData != null)
                    {
                        inputMeshes._inputMeshes.Add(meshData);
                    }
                }
            }

            return inputMeshes;
        }

        /// <summary>
        /// Returns HEU_UploadMeshData with mesh data found on meshGameObject.
        /// </summary>
        /// <param name="meshGameObject">The GameObject to query mesh data from</param>
        /// <returns>A valid HEU_UploadMeshData if mesh data found or null</returns>
        public static HEU_InputDataMesh CreateSingleMeshData(GameObject meshGameObject, bool bExportColliders)
        {
            HEU_InputDataMesh meshData = new HEU_InputDataMesh();

            if (meshGameObject == null)
            {
                return null;
            }

            Mesh sharedMesh = GetMeshFromObject(meshGameObject);

            if (sharedMesh == null)
            {
                return null;
            }

            meshData._mesh = sharedMesh;
            meshData._numVertices = meshData._mesh.vertexCount;
            meshData._numSubMeshes = meshData._mesh.subMeshCount;

            meshData._meshName = meshGameObject.name;

            // Use project path is not saved in scene, otherwise just use name
            if (HEU_GeneralUtility.IsGameObjectInProject(meshGameObject))
            {
                meshData._meshPath = HEU_AssetDatabase.GetAssetOrScenePath(meshGameObject);
                if (string.IsNullOrEmpty(meshData._meshPath))
                {
                    meshData._meshPath = meshGameObject.name;
                }
            }
            else
            {
                meshData._meshPath = meshGameObject.name;
            }
            //HEU_Logger.Log("Mesh Path: " + meshData._meshPath);

            MeshRenderer meshRenderer = meshGameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshData._materials = meshRenderer.sharedMaterials;
            }

            meshData._transform = meshGameObject.transform;

            if (bExportColliders && meshGameObject != null)
            {
                meshData._colliders = new List<HEU_InputDataCollider>();

                Collider[] colliders = meshGameObject.GetComponents<Collider>();
                if (colliders != null)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Collider collider = colliders[i];

                        if (collider == null) continue;

                        HEU_InputDataCollider newCollider = new HEU_InputDataCollider();
                        newCollider._collider = collider;

                        if (collider.GetType() == typeof(BoxCollider))
                        {
                            newCollider._colliderType = HEU_InputColliderType.BOX;
                        }
                        else if (collider.GetType() == typeof(SphereCollider))
                        {
                            newCollider._colliderType = HEU_InputColliderType.SPHERE;
                        }
                        else if (collider.GetType() == typeof(CapsuleCollider))
                        {
                            newCollider._colliderType = HEU_InputColliderType.CAPSULE;
                        }
                        else if (collider.GetType() == typeof(MeshCollider))
                        {
                            newCollider._colliderType = HEU_InputColliderType.MESH;
                        }
                        else
                        {
                            HEU_Logger.LogWarningFormat("Collider type not supported: {0}", meshGameObject.name);
                            newCollider._collider = null;
                            newCollider._colliderType = HEU_InputColliderType.NONE;
                        }

                        if (newCollider._colliderType != HEU_InputColliderType.NONE)
                        {
                            meshData._colliders.Add(newCollider);
                        }
                    }
                }
            }


            return meshData;
        }

        private static Mesh GetMeshFromObject(GameObject meshGameObject)
        {
            if (meshGameObject == null)
            {
                return null;
            }

            MeshFilter filter = meshGameObject.GetComponent<MeshFilter>();
            if (filter != null)
            {
                return filter.sharedMesh;
            }


            SkinnedMeshRenderer skinnedMesh = meshGameObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMesh != null)
            {
                return skinnedMesh.sharedMesh;
            }

            return null;
        }
    }
} // HoudiniEngineUnity
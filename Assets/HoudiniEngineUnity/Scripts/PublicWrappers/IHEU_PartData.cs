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

using UnityEngine;
using System.Collections.Generic;

// Expose internal classes/functions
#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HoudiniEngineUnityEditor")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityEditorTests")]
[assembly: InternalsVisibleTo("HoudiniEngineUnityPlayModeTests")]
#endif


namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;
    using HAPI_AssetLibraryId = System.Int32;
    using HAPI_StringHandle = System.Int32;
    using HAPI_ErrorCodeBits = System.Int32;
    using HAPI_NodeTypeBits = System.Int32;
    using HAPI_NodeFlagsBits = System.Int32;
    using HAPI_ParmId = System.Int32;
    using HAPI_PartId = System.Int32;


    /// <summary>
    /// Represents a Part object containing mesh / geometry/ attribute data.
    /// </summary>
    public interface IHEU_PartData
    {
        /// <summary>The part id of the HDA</summary>
        HAPI_PartId PartID { get; }

        /// <summary>The part name of the HDA</summary>
        string PartName { get; }

        /// <summary>The geo id of the HDA</summary>
        HAPI_NodeId GeoID { get; }

        /// <summary>The part type of the HDA</summary>
        HAPI_PartType PartType { get; }

        /// <summary>The parent geo node of the HDA</summary>
        HEU_GeoNode ParentGeoNode { get; }

        /// <summary>The object instance infos of the HDA</summary>
        List<HEU_ObjectInstanceInfo> ObjectInstanceInfos { get; }

        /// <summary>The curve of the HDA, if it contains one</summary>
        HEU_Curve Curve { get; }

        /// <summary>The mesh vertex count of the generated output</summary>
        int MeshVertexCount { get; }

        /// <summary>The generated output</summary>
        HEU_GeneratedOutput GeneratedOutput { get; }

        /// <summary>The output gameobject</summary>
        GameObject OutputGameObject { get; }

        /// <summary>Whether or not this is a part instancer</summary>
        bool IsPartInstancer();

        /// <summary>Whether or not this is an attribute instancer</summary>
        bool IsAttribInstancer();

        /// <summary>Whether or not this is an instancer of any type</summary>
        bool IsInstancerAnyType();

        /// <summary>Whether or not this part is instanced</summary>
        bool IsPartInstanced();

        /// <summary>The part point count</summary>
        int GetPartPointCount();

        /// <summary>Whether or not this is an object instancer</summary>
        bool IsObjectInstancer();

        /// <summary>Whether or not this is a volume part</summary>
        bool IsPartVolume();

        /// <summary>Whether or not this is a part curve</summary>
        bool IsPartCurve();

        /// <summary>Whether or not this is a mesh part</summary>
        bool IsPartMesh();

        /// <summary>Whether or not this part is editable</summary>
        bool IsPartEditable();

        /// <summary>Whether or not this part's instances have been generated</summary>
        bool HaveInstancesBeenGenerated();

        /// <summary>Sets the gameObject name</summary>
        void SetGameObjectName(string partName);

        /// <summary>Sets the gameObject of this part</summary>
        void SetGameObject(GameObject gameObject);

        /// <summary>Sets the volume layer name</summary>
        void SetVolumeLayerName(string name);

        /// <summary>Gets the volume layer name</summary>
        string GetVolumeLayerName();

        /// <summary>
        /// Destroy all generated data.
        /// </summary>
        /// <param name="bIsRebuild">Whether or not this is a rebuild (retains some data)</param>
        void DestroyAllData(bool bIsRebuild = false);


        /// <summary>
        /// Returns true if this part's mesh is using the given material.
        /// </summary>
        /// <param name="materialData">Material data containing the material to check</param>
        /// <returns>True if this part is using the given material</returns>
        bool IsUsingMaterial(HEU_MaterialData materialData);

        /// <summary>
        /// Adds gameobjects that were output from this part.
        /// </summary>
        /// <param name="outputObjects">List to add to</param>
        void GetOutputGameObjects(List<GameObject> outputObjects);

        /// <summary>
        /// Adds this Part's HEU_GeneratedOutput to given output list.
        /// </summary>
        /// <param name="output">List to add to</param>
        void GetOutput(List<HEU_GeneratedOutput> outputs);

        /// <summary>
        /// Returns self if it has the given output gameobject.
        /// </summary>
        /// <param name="inGameObject">The output gameobject to check</param>
        /// <returns>Valid HEU_PartData or null if no match</returns>
        HEU_PartData GetHDAPartWithGameObject(GameObject inGameObject);

        /// <summary>
        /// Clear out existing instances for this part.
        /// </summary>
        void ClearInstances();

        /// <summary>
        /// Gets the curve on this part, if it has one
        /// </summary>
        /// <param name="bEditableOnly">Only return if the curve part is editable</param>
        /// <returns>Returns the HEU_Curve component, if it has one</returns>
        HEU_Curve GetCurve(bool bEditableOnly);


        /// <summary>
        /// Set visibility on this part's gameobject.
        /// </summary>
        /// <param name="bVisibility">True if visible.</param>
        void SetVisiblity(bool bVisibility);

        /// <summary>
        /// Sets whether or not the collider state is visible
        /// </summary>
        /// <param name="bEnabled">True if set collider state is enabled.</param>
        void SetColliderState(bool bEnabled);

        /// <summary>
        /// Returns HEU_ObjectInstanceInfo with matching _instancedObjectPath.
        /// </summary>
        /// <param name="path">The path to match with _instancedObjectPath</param>
        /// <returns>HEU_ObjectInstanceInfo with matching _instancedObjectPath or null if none found</returns>
        HEU_ObjectInstanceInfo GetObjectInstanceInfoWithObjectPath(string path);

        /// <summary>
        /// Returns HEU_ObjectInstanceInfo with matching objNodeID
        /// </summary>
        /// <param name="objNodeID">The Houdini Engine node ID to match</param>
        /// <returns>HEU_ObjectInstanceInfo with matching objNodeID or null if none found</returns>
        HEU_ObjectInstanceInfo GetObjectInstanceInfoWithObjectID(HAPI_NodeId objNodeID);


        /// <summary>
        /// Sets the terrain offset position
        /// </summary>
        /// <param name="offsetPosition">The position to set the terrain at</param>
        void SetTerrainOffsetPosition(Vector3 offsetPosition);

        /// <summary>
        /// Saves the given terrainData to the AssetDatabase for this part.
        /// Adds to existing saved asset file or creates this as the root asset.
        /// </summary>
        /// <param name="terrainData">The TerrainData object to save</param>
        /// <param name="exportPathRelative">The relative export path</param>
        /// <param name="exportPathUser">The user exported path</param>
        void SetTerrainData(TerrainData terrainData, string exportPathRelative, string exportPathUser);
    }
} // HoudiniEngineUnity
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
    /// Represents the Houdini Object node.
    /// Holds and manages geo nodes.
    /// </summary>
    public interface IHEU_ObjectNode
    {
        /// <summary>The object id of the HDA</summary>
        HAPI_NodeId ObjectID { get; }

        /// <summary>The object name of the HDA</summary>
        string ObjectName { get; }

        /// <summary>The object info of the HDA</summary>
        HAPI_ObjectInfo ObjectInfo { get; }

        /// <summary>The geo nodes of the HDA</summary>
        List<HEU_GeoNode> GeoNodes { get; }

        /// <summary>The object transform of the HDA</summary>
        HAPI_Transform ObjectTransform { get; }

        /// <summary>Whether or not the object node is instanced</summary>
        bool IsInstanced();

        /// <summary>Whether or not the object node is visible</summary>
        bool IsVisible();

        /// <summary>
        /// Returns true if this object is using the given material.
        /// </summary>
        /// <param name="materialData">Material data containing the material to check</param>
        /// <returns>True if this object is using the given material</returns>
        bool IsUsingMaterial(HEU_MaterialData materialData);

        /// <summary>
        /// Adds gameobjects that were output from this object.
        /// </summary>
        /// <param name="outputObjects">List to add to</param>
        void GetOutputGameObjects(List<GameObject> outputObjects);

        /// <summary>
        /// Adds this node's HEU_GeneratedOutput to given outputs list.
        /// </summary>
        /// <param name="outputs">List to add to</param>
        void GetOutput(List<HEU_GeneratedOutput> outputs);

        /// <summary>
        /// Returns the HEU_PartData with the given output gameobject.
        /// </summary>
        /// <param name="outputGameObject">The output gameobject to check</param>
        /// <returns>Valid HEU_PartData or null if no match</returns>
        HEU_PartData GetHDAPartWithGameObject(GameObject outputGameObject);

        /// <summary>
        /// Returns the HEU_GeoNode with the given name.
        /// </summary>
        /// <param name="geoName">The name to check</param>
        /// <returns>Valid HEU_GeoNode or null if no match</returns>
        HEU_GeoNode GetGeoNode(string geoName);

        /// <summary>
        /// Gets the curves under this object node
        /// </summary>
        /// <param name="curves">List of curves to add. </param>
        /// <param name="bEditableOnly">Whether to filter editable nodes or not</param>
        void GetCurves(List<HEU_Curve> curves, bool bEditableOnly);

        /// <summary>
        /// Gets the output geo nodes
        /// </summary>
        /// <param name="outGeoNodes">List of output geo nodes</param>
        void GetOutputGeoNodes(List<HEU_GeoNode> outGeoNodes);

        /// <summary>
        /// Hide all geometry contained within
        /// </summary>
        void HideAllGeometry();

        /// <summary>
        /// Disables all clliders in this object node
        /// </summary>
        void DisableAllColliders();

        /// <summary>
        /// Returns true if this is an object instancer, or if it has point (attribute) instancer parts.
        /// </summary>
        /// <returns></returns>
        bool IsInstancer();
    }
} // HoudiniEngineUnity
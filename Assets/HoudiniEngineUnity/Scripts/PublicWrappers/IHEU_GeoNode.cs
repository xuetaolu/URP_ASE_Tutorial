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
    /// Represents a Geometry (SOP) node.
    /// </summary>
    public interface IHEU_GeoNode
    {
        /// <summary>The GeoID of the node in Houdini </summary>
        HAPI_NodeId GeoID { get; }

        /// <summary>The GeoInfo of the node in Houdini </summary>
        HAPI_GeoInfo GeoInfo { get; }

        /// <summary>The GeoName of the node in Houdini </summary>
        string GeoName { get; }

        /// <summary>The GeoType of the node in Houdini </summary>
        HAPI_GeoType GeoType { get; }

        /// <summary>Whether or not this node is editable</summary>
        bool Editable { get; }

        /// <summary>Whether or not this node is displayable</summary>
        bool Displayable { get; }

        /// <summary>The parts of this node </summary>
        List<HEU_PartData> Parts { get; }

        /// <summary>The object node.</summary>
        HEU_ObjectNode ObjectNode { get; }

        /// <summary>The input node, only valid if this is a geo input node</summary>
        HEU_InputNode InputNode { get; }

        /// <summary>The curve node, only valid if this is a geo curve</summary>
        HEU_Curve GeoCurve { get; }

        /// <summary>The volume caches, only valid if it contains a volume</summary>
        List<HEU_VolumeCache> VolumeCaches { get; }

        /// <summary>Whether or not this geo node is visible</summary>
        bool IsVisible();

        /// <summary>Whether or not this geo node is an intermediate node</summary>
        bool IsIntermediate();

        /// <summary>Whether or not this geo node is an intermediate node or editable</summary>
        bool IsIntermediateOrEditable();

        /// <summary>Whether or not this is a geo input type</summary>
        bool IsGeoInputType();

        /// <summary>Whether or not this is a curve type</summary>
        bool IsGeoCurveType();


        /// <summary>
        /// Destroy all generated data.
        /// </summary>
        /// <param name="bIsRebuild">Whether or not this is a rebuild (retains some data)</param>
        void DestroyAllData(bool bIsRebuild = false);

        /// <summary>
        /// Destroy a part in the geo node.
        /// </summary>
        /// <param name="part">The part to remove and destroy</param>
        void RemoveAndDestroyPart(HEU_PartData part);

        /// <summary>
        /// Adds gameobjects that were output from this geo node.
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
        /// Returns contained part with specified partID
        /// </summary>
        /// <param name="partID">The node ID to match</param>
        /// <returns>The part with partID</returns>
        HEU_PartData GetPartFromPartID(HAPI_NodeId partID);

        /// <summary>
        /// Gets all curves in the geoNode
        /// </summary>
        /// <param name="curves">List of curves to return to</param>
        /// <param name="bEditableOnly">Whether or not to only return editable curves</param>
        void GetCurves(List<HEU_Curve> curves, bool bEditableOnly);

        /// <summary>
        /// Get the parts
        /// </summary>
        /// <returns>The part with partID</returns>
        List<HEU_PartData> GetParts();

        /// <summary>
        /// Hide all geometry contained within
        /// </summary>
        void HideAllGeometry();

        /// <summary>
        /// Disable all colliders
        /// </summary>
        void DisableAllColliders();

        /// <summary>
        /// Gets the volume cache by the tile index
        /// </summary>
        /// <param name="tileIndex">The tile index of the volume</param>
        /// <returns>The volume cache at the index</returns>
        HEU_VolumeCache GetVolumeCacheByTileIndex(int tileIndex);
    }
} // HoudiniEngineUnity
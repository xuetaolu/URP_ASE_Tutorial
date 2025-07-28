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

    // The type of input node based on how it was specified in the HDA
    public enum HEU_InputNodeTypeWrapper
    {
        CONNECTION, // As an asset connection
        NODE, // Pure input asset node
        PARAMETER // As an input parameter
    };

    // The type of input data set by user
    public enum HEU_InputObjectTypeWrapper
    {
        HDA,
        UNITY_MESH,
        CURVE,
#if UNITY_2022_1_OR_NEWER
        SPLINE,
#endif
        TERRAIN,
        BOUNDING_BOX,
        TILEMAP
    };

    /// <summary>
    /// Holds all parameter data for an asset.
    /// </summary>
    public interface IHEU_InputNode
    {
        /// <summary>
        /// Enabling Keep World Transform by default to keep consistent with other plugins
        /// If true, sets the SOP/merge (object merge) node to use INTO_THIS_OBJECT transform type. Otherwise NONE.
        /// </summary>
        bool KeepWorldTransform { get; set; }

        /// <summary>Acts same as SOP/merge (object merge) Pack Geometry Before Merging parameter value.</summary>
        bool PackGeometryBeforeMerging { get; set; }

        /// <summary>Input node type</summary>
        HEU_InputNodeTypeWrapper NodeType { get; }

        /// <summary>Input node object type (HDA, Mesh, etc)</summary>
        HEU_InputObjectTypeWrapper ObjectType { get; }

        /// <summary>The inputted object type</summary>
        HEU_InputObjectTypeWrapper PendingObjectType { get; }

        /// <summary>Input node ID</summary>
        HAPI_NodeId InputNodeID { get; }

        /// <summary>Input node name</summary>
        string InputName { get; }

        /// <summary>Input node label</summary>
        string LabelName { get; }

        /// <summary>Input node parameter</summary>
        string ParamName { get; }

        /// <summary>Mesh settings</summary>
        HEU_InputInterfaceMeshSettings MeshSettings { get; }

        /// <summary>Tilemap settings</summary>
        HEU_InputInterfaceTilemapSettings TilemapSettings { get; }

        /// <summary>Spline settings</summary>
        HEU_InputInterfaceSplineSettings SplineSettings { get; }

        /// <summary>
        /// Whether or not this is an asset input
        /// </summary>
        /// <returns>Is an asset input?</returns>
        bool IsAssetInput();

        /// <summary>
        /// Get the number of input entries
        /// </summary>
        /// <returns>Number of input entries</returns>
        int NumInputEntries();

        /// <summary>
        /// Get the input entry gameobject at a index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The gameobject</returns>
        GameObject GetInputEntryGameObject(int index);

        /// <summary>
        /// Get the input entry gameobjects
        /// </summary>
        /// <returns>The gameobjects</returns>
        GameObject[] GetInputEntryGameObjects();

        /// <summary>
        /// Sets the input entry gameObject at a index
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="newInputGameObject">The gameObject to set</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void SetInputEntry(int index, GameObject newInputGameObject, bool bRecookAsset = false);

        /// <summary>
        /// Inserts the input entry gameObject at a index
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="newInputGameObject">The gameObject to set</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void InsertInputEntry(int index, GameObject newInputGameObject, bool bRecookAsset = false);

        /// <summary>
        /// Add an input entry at the end
        /// </summary>
        /// <param name="newEntryGameObject">The gameObject to add</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void AddInputEntryAtEnd(GameObject newEntryGameObject, bool bRecookAsset = false);

        /// <summary>
        /// Resets the input node
        /// </summary>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void ResetInputNode(bool bRecookAsset = false);

        /// <summary>
        /// Changes the input type
        /// </summary>
        /// <param name="newType">The new input type to change the input node to</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void ChangeInputType(HEU_InputObjectTypeWrapper newType, bool bRecookAsset = false);

        /// <summary>
        /// Remove input entry at index
        /// </summary>
        /// <param name="index">The input entry to remove</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void RemoveInputEntry(int index, bool bRecookAsset = false);

        /// <summary>
        /// Removes all input entries
        /// </summary>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void RemoveAllInputEntries(bool bRecookAsset = false);

        /// <summary>
        /// Sets the input entry object use transform flag.
        /// </summary>
        /// <param name="index">Index to set it at</param>
        /// <param name="value">The value to set it at</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void SetInputEntryObjectUseTransformOffset(int index, bool value, bool bRecookAsset = false);

        /// <summary>
        /// Sets the input entry object translation offset. Only valid if use transform offset is true.
        /// </summary>
        /// <param name="index">Index to set it at</param>
        /// <param name="translateOffset">The offset to set it at</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void SetInputEntryObjectTransformTranslateOffset(int index, Vector3 translateOffset, bool bRecookAsset = false);

        /// <summary>
        /// Sets the input entry object rotate offset. Only valid if use transform offset is true.
        /// </summary>
        /// <param name="index">Index to set it at</param>
        /// <param name="rotateOffset">The offset to set it at</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void SetInputEntryObjectTransformRotateOffset(int index, Vector3 rotateOffset, bool bRecookAsset = false);

        /// <summary>
        /// Sets the input entry object scale offset. Only valid if use transform offset is true.
        /// </summary>
        /// <param name="index">Index to set it at</param>
        /// <param name="scaleOffset">The offset to set it at</param>
        /// <param name="bRecookAsset">Whether or not to recook the asset afterwards</param>
        void SetInputEntryObjectTransformScaleOffset(int index, Vector3 scaleOffset, bool bRecookAsset = false);

        /// <summary>
        /// Gets whether or not an input HDAs are connected.
        /// </summary>
        /// <returns>Whether or not an input HDAs are connected.</returns>
        bool AreAnyInputHDAsConnected();

        /// <summary>
        /// Gets connected input count
        /// </summary>
        /// <returns>The number of connected input counts.</returns>
        int GetConnectedInputCount();

        /// <summary>
        /// Gets the connected input node ID at index
        /// </summary>
        /// <param name="index">Index to set it at</param>
        /// <returns>Gets connected node ID.</returns>
        HAPI_NodeId GetConnectedNodeID(int index);

        /// <summary>
        /// Loads the specified input preset
        /// </summary>
        /// <param name="inputPreset">The input preset</param>
        void LoadPreset(HEU_InputPreset inputPreset);

        /// <summary>
        /// Populates the specified inputPreset with this HEU_InputNode's data
        /// </summary>
        /// <param name="inputPreset">The input preset</param>
        void PopulateInputPreset(HEU_InputPreset inputPreset, bool sceneRelativeGameObjects);
    }
} // HoudiniEngineUnity
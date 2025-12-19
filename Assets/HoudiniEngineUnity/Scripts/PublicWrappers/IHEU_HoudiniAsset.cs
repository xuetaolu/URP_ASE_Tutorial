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
    /// A wrapper around the asset cook status
    /// </summary>
    public enum HEU_AssetCookStatusWrapper
    {
        NONE,
        COOKING,
        POSTCOOK,
        LOADING,
        POSTLOAD,
        PRELOAD,
        SELECT_SUBASSET
    };


    public enum HEU_AssetCookResultWrapper
    {
        NONE,
        SUCCESS,
        ERRORED
    };

    /// <summary>
    /// A wrapper around the curve draw collision
    /// </summary>
    public enum HEU_CurveDrawCollisionWrapper
    {
        INVALID = 0,
        COLLIDERS,
        LAYERMASK
    };


    /// <summary>
    /// A wrapper around the asset type
    /// </summary>
    public enum HEU_AssetTypeWrapper
    {
        TYPE_INVALID = 0,
        TYPE_HDA,
        TYPE_CURVE,
        TYPE_INPUT
    };

    /// <summary>
    /// Represents a Houdini Digital Asset in Unity.
    /// Contains object nodes, geo nodes, and parts for an HDA.
    /// Contains HDA's parameters.
    /// Load, (re)cook, and bake out asset.
    /// Can (and should) be excluded from builds & runtime.
    /// </summary>
    public interface IHEU_HoudiniAsset
    {
        // Get/Set options
        // If true, this asset file will be loaded into memory first
        // in Unity, then HARS will load it from memory buffer.

        /// <summary>If true, this asset file will be loaded into memory first in Unity, then HARS will load it from memory buffer. </summary>
        bool LoadAssetFromMemory { get; set; }

        /// <summary>If true, always overwrite the existing HDA file in a call to LoadAssetLibraryFromFile (Without showing dialog) </summary>
        bool AlwaysOverwriteOnLoad { get; set; }

        /// <summary>Whether or not to generate UVs on the output mesh</summary>
        bool GenerateUVs { get; set; }

        /// <summary>Whether or not to generate tangents on the output mesh</summary>
        bool GenerateTangents { get; set; }

        /// <summary>Whether or not to generate normals on the output mesh</summary>
        bool GenerateNormals { get; set; }

        /// <summary>Whether or not to push the unity transform to Houdini</summary>
        bool PushTransformToHoudini { get; set; }

        /// <summary>Whether or not transform changes triggers cooks</summary>
        bool TransformChangeTriggersCooks { get; set; }

        /// <summary>Whether or not cooking this asset triggers downstream cooks</summary>
        bool CookingTriggersDownCooks { get; set; }

        /// <summary>Whether or not changing parameters causes a recook</summary>
        bool AutoCookOnParameterChange { get; set; }

        /// <summary>Whether or not to ignore non-display nodes</summary>
        bool IgnoreNonDisplayNodes { get; set; }

        /// <summary>Whether or not to use output nodes</summary>
        bool UseOutputNodes { get; set; }

        /// <summary>Whether or not to generate mesh using points</summary>
        bool GenerateMeshUsingPoints { get; set; }

        /// <summary>Whether or not to use LOD groups</summary>
        bool UseLODGroups { get; set; }

        /// <summary>Whether or not to split geometry by group</summary>
        bool SplitGeosByGroup { get; set; }

        /// <summary>Whether or not session sync triggers auto cooking</summary>
        bool SessionSyncAutoCook { get; set; }

        /// <summary>Whether or not bake update keeps previous transform values</summary>
        bool BakeUpdateKeepPreviousTransformValues { get; set; }

        /// <summary>Whether or not to pause coking</summary>
        bool PauseCooking { get; set; }

        /// <summary>Whether or not the curve editor is enabled</summary>
        bool CurveEditorEnabled { get; set; }

        /// <summary>The set curve draw collision</summary>
        HEU_CurveDrawCollisionWrapper CurveDrawCollision { get; set; }

        /// <summary>The curve draw layer mask</summary>
        LayerMask CurveDrawLayerMask { get; set; }

        /// <summary>The curve project max distance</summary>
        float CurveProjectMaxDistance { get; set; }

        /// <summary>The curve project direction</summary>
        Vector3 CurveProjectDirection { get; set; }

        /// <summary>Whether or not the curve projection is projected from the scene view</summary>
        bool CurveProjectDirectionToView { get; set; }

        /// <summary> Whther or not to disable adding curve rot/scale attributes</summary>
        bool CurveDisableScaleRotation { get; set; }

        /// <summary>Whether or not to frame selected nodes using the F key</summary>
        bool CurveFrameSelectedNodes { get; set; }

        /// <summary>The distance when a curve is framed using the F key</summary>
        float CurveFrameSelectedNodeDistance { get; set; }

        /// <summary>Whether or not the handles are enabled</summary>
        bool HandlesEnabled { get; set; }

        /// <summary>Whether or not editable tools are enabled. </summary>
        bool EditableNodesToolsEnabled { get; set; }


        // Read only ====

        /// <summary>The asset type of the HDA</summary>
        HEU_AssetTypeWrapper AssetType { get; }

        /// <summary>The asset info of the HDA</summary>
        HAPI_AssetInfo AssetInfo { get; }

        /// <summary>The node info of the HDA</summary>
        HAPI_NodeInfo NodeInfo { get; }

        /// <summary>The asset name of the HDA</summary>
        string AssetName { get; }

        /// <summary>The asset operator of the HDA</summary>
        string AssetOpName { get; }

        /// <summary>The asset help of the HDA</summary>
        string AssetHelp { get; }

        /// <summary>The asset id of the HDA</summary>
        HAPI_NodeId AssetID { get; }

        /// <summary>The asset path of the HDA</summary>
        string AssetPath { get; }

        /// <summary>The owner gameobject (i.e. HDA_Data)</summary>
        GameObject OwnerGameObject { get; }

        /// <summary>The asset type of the HDA</summary>
        GameObject RootGameObject { get; }

        /// <summary>The material data for this HDA</summary>
        List<HEU_MaterialData> MaterialCache { get; }

        /// <summary>The parameter data for this HDA</summary>
        HEU_Parameters Parameters { get; }

        /// <summary>The asset cache folder</summary>
        string AssetCacheFolder { get; }

        /// <summary>The names of the subassets in this HDA</summary>
        string[] SubassetNames { get; }

        /// <summary>The selected subasset index</summary>
        int SelectedSubassetIndex { get; }

        /// <summary>The asset cook status</summary>
        HEU_AssetCookStatusWrapper CookStatus { get; }

        /// <summary> The last asset cook result </summary>
        HEU_AssetCookResultWrapper LastCookResult { get; }

        /// <summary>The session ID.</summary>
        long SessionID { get; }

        /// <summary>The Curves in this HDA</summary>
        List<HEU_Curve> Curves { get; }

        /// <summary>The input nodes for this HDA</summary>
        List<HEU_InputNode> InputNodes { get; }

        /// <summary>The volume caches for this HDA</summary>
        List<HEU_VolumeCache> VolumeCaches { get; }


        // ASSET EVENTS -----------------------------------------------------------------------------------------------

        /// <summary>The events that gets called on reload (rebuild)</summary>
        HEU_ReloadDataEvent ReloadDataEvent { get; }

        /// <summary>The events that gets called on cook</summary>
        HEU_CookedDataEvent CookedDataEvent { get; }

        /// <summary>The events that gets called on baked</summary>
        HEU_BakedDataEvent BakedDataEvent { get; }

        /// <summary>The events that gets called before cooking</summary>
        HEU_PreAssetEvent PreAssetEvent { get; }

        /// <summary>
        /// Public interface to request a cook of this asset.
        /// Can be async or blocking. If async will return once cook has finished.
        /// </summary>
        /// <param name="bCheckParamsChanged">If true, then will only upload parameters that have changed.</param>
        /// <param name="bAsync">Cook asynchronously or block until cooking is done.</param>
        /// <param name="bSkipCookCheck">If true, will force cook even if cooking is disabled.</param>
        /// <param name="bUploadParameters">If true, will upload parameter values before cooking.</param>
        bool RequestCook(bool bCheckParametersChanged = true, bool bAsync = false, bool bSkipCookCheck = true, bool bUploadParameters = true);

        /// <summary>
        /// Public interface to request a full reload / build of the asset.
        /// Will reset to same state as if it was just instantiated, but keep
        /// existing transform information and place in Hierarchy.
        /// </summary>
        /// <param name="bAsync">Reload asynchronoulsy if true, or block until reload completed.</param>
        bool RequestReload(bool bAsync = false);

        /// <summary>
        /// Reset the parameters and reload and rebuild the asset.
        /// </summary>
        /// <param name="bAsync">Reload asynchronously if true, or block until completed.</param>
        bool RequestResetParameters(bool bAsync = false);

        /// <summary>
        /// Create a copy of this asset in the Scene and returns it.
        /// </summary>
        /// <param name="newRootGameObject">An optional argument specifying the root gameobject</param>
        GameObject DuplicateAsset(GameObject newRootGameObject = null);

        /// <summary>
        /// Delete generated data used by this asset.
        /// </summary>
        /// <param name="bIsRebuild">Whether or not this is a rebuild (retains some information)</param>
        bool DeleteAllGeneratedData(bool bIsRebuild = false);

        /// <summary>
        /// Creates a prefab of this asset, without Houdini Engine data.
        /// Returns reference to new prefab.
        /// </summary>
        /// <param name="destinationPrefabPath">Opitional destination path to save prefab to (e.g. Assets/Prefabs)</param>
        /// <returns>Reference to created prefab</returns>
        GameObject BakeToNewPrefab(string destinationPrefabPath = null);

        /// <summary>
        /// Create a copy of this asset, without Houdini Engine data.
        /// Returns reference to newly created gameobject.
        /// </summary>
        /// <returns>Reference to created Gameobject</returns>
        GameObject BakeToNewStandalone();

        /// <summary>
        /// Bakes to existing prefab
        /// </summary>
        /// <param name="bakeTargetGO">The target prefab GO</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool BakeToExistingPrefab(GameObject bakeTargetGO);

        /// <summary>
        /// Bakes to existing gameobject
        /// </summary>
        /// <param name="bakeTargetGO">The target GO</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool BakeToExistingStandalone(GameObject bakeTargetGO);

        /// <summary>
        /// Returns true if this asset is valid in its own Houdini session.
        /// </summary>
        /// <returns>Whether or not this asset is valid</returns>
        bool IsAssetValid();

        /// <summary>
        /// Adds gameobjects that were output from this asset.
        /// </summary>
        /// <param name="outputObjects">List to add to</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetOutputGameObjects(List<GameObject> outputObjects);

        /// <summary>
        /// Adds this node's HEU_GeneratedOutput to given outputs list.
        /// </summary>
        /// <param name="outputs">List to add to</param>
        /// <returns>Whether or not the operation was successful</returns>
        bool GetOutput(List<HEU_GeneratedOutput> outputs);

        /// <summary>
        /// Gets the curve with the specified curveName
        /// </summary>
        /// <param name="curveName">The name of the curve</param>
        /// <returns>The curve with the name, if it exists</returns>
        HEU_Curve GetCurve(string curveName);

        /// <summary>
        /// Adds the curve draw collider to this asset
        /// </summary>
        /// <param name="newCollider">The new collider</param>
        /// <returns>Whether or not the operation is successful</returns>
        bool AddCurveDrawCollider(Collider newCollider);

        /// <summary>
        /// Removes the curve draw collider to this asset
        /// </summary>
        /// <param name="collider">The collider</param>
        /// <returns>Whether or not the operation is successful</returns>
        bool RemoveCurveDrawCollider(Collider collider);

        /// <summary>
        /// Clears the draw colliders
        /// </summary>
        /// <returns>Whether or not the operation is successful</returns>
        bool ClearCurveDrawColliders();

        /// <summary>
        /// Gets an input node with a name
        /// </summary>
        /// <param name="inputName">The name of the input</param>
        /// <returns>The input node</returns>
        HEU_InputNode GetInputNode(string inputName);

        /// <summary>
        /// Gets an asset input node with a name
        /// </summary>
        /// <param name="inputName">The name of the input</param>
        /// <returns>The input node</returns>
        HEU_InputNode GetAssetInputNode(string inputName);

        /// <summary>
        /// Gets an input node by index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The input node</returns>
        HEU_InputNode GetInputNodeByIndex(int index);

        /// <summary>
        /// Gets input nodes, ignoring parameter input nodes
        /// </summary>
        /// <returns>The list of input nodes</returns>
        List<HEU_InputNode> GetNonParameterInputNodes();

        /// <summary>
        /// Gets the volume cache count
        /// </summary>
        /// <returns>The number of volume caches for the asset</returns>
        int GetVolumeCacheCount();

        /// <summary>
        /// Returns the session that this asset was created / resides in.
        /// null if no valid session, or if this asset hasn't been created in one yet.
        /// </summary>
        /// <param name="bCreateIfInvalid">If true and current session is invalid, will try creating a new session.</param>
        /// <returns>Session containing this asset or null if unable to get one</returns>
        HEU_SessionBase GetAssetSession(bool bCreateIfInvalid);

        /// <summary>
        /// The object node by name
        /// </summary>
        /// <param name="objName">The name of the object node</param>
        /// <returns>The object node with the name, if it exists. </returns>
        HEU_ObjectNode GetObjectNodeByName(string objName);

        /// <summary>
        /// Gets the output geo nodes
        /// </summary>
        /// <param name="outputGeoNodes">The list of output geo nodes to return</param>
        void GetOutputGeoNodes(List<HEU_GeoNode> outputGeoNodes);

        /// <summary>
        /// Gets the HDA part data on the specified gameobject
        /// </summary>
        /// <param name="outputGameObject">The specified gameObject</param>
        /// <returns>The part data on the gameObject, if it exists.</returns>
        HEU_PartData GetInternalHDAPartWithGameObject(GameObject outputGameObject);

        /// <summary>
        /// Reset the HDA's parameters to default
        /// </summary>
        void ResetParametersToDefault();

        /// <summary>
        /// Hide all geometry contained within
        /// </summary>
        void HideAllGeometry();

        /// <summary>
        /// Disable all colliders in the HDA
        /// </summary>
        void DisableAllColliders();

        /// <summary>
        /// Gets the material data for a material, if it exists in a material cache
        /// </summary>
        /// <param name="material">The material to check</param>
        /// <returns>The material data, if it exists in the material cache.</returns>
        HEU_MaterialData GetMaterialData(Material material);

        /// <summary>
        /// Clears the material cache
        /// </summary>
        void ClearMaterialCache();

        /// <summary>
        /// Removes the material in the material cache
        /// </summary>
        /// <param name="material">The material to remove</param>
        void RemoveMaterial(Material material);

        /// <summary>
        /// Removes materials overrides on this asset for all its outputs,
        /// replacing them with the generated materials.
        /// </summary>
        void ResetMaterialOverrides();

        /// <summary>
        /// Return this asset's preset data in a new HEU_AssetPreset object.
        /// It will contain both parameter preset, as well as list of curve names
        /// and their presets.
        /// </summary>
        /// <returns>A new HEU_AssetPreset populated with parameter preset and curve presets</returns>
        HEU_AssetPreset GetAssetPreset(bool sceneRelativeObjects);

        /// <summary>
        /// Gets or creates a PDG asset link for this asset, if it doesn't exist.
        /// If the HDA is not a PDG asset, this function will also return null
        /// </summary>
        /// <returns>A PDGAssetLink of the asset, or null if the HDA doesn't contain PDG nodes</returns>
        HEU_PDGAssetLink GetOrCreatePDGAssetLink();
    }
} // HoudiniEngineUnity
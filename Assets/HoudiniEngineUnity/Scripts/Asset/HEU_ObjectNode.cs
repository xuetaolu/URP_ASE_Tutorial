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
using System.Text;
using UnityEngine;

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
    using HAPI_PartId = System.Int32;
    using HAPI_NodeTypeBits = System.Int32;
    using HAPI_NodeFlagsBits = System.Int32;


    /// <summary>
    /// Represents the Houdini Object node.
    /// Holds and manages geo nodes.
    /// </summary>
    public class HEU_ObjectNode : ScriptableObject, IHEU_ObjectNode, IHEU_HoudiniAssetSubcomponent,
        IEquivable<HEU_ObjectNode>
    {
        // PUBLIC FIELDS =================================================================

        /// <inheritdoc />
        public HEU_HoudiniAsset ParentAsset => _parentAsset;

        /// <inheritdoc />
        public HAPI_NodeId ObjectID => _objectInfo.nodeId;

        /// <inheritdoc />
        public string ObjectName => _objName;

        /// <inheritdoc />
        public HAPI_ObjectInfo ObjectInfo => _objectInfo;

        /// <inheritdoc />
        public List<HEU_GeoNode> GeoNodes => _geoNodes;

        /// <inheritdoc />
        public HAPI_Transform ObjectTransform => _objectTransform;

        // ===============================================================================

        //  DATA ------------------------------------------------------------------------------------------------------

        [SerializeField] private string _objName;

        [SerializeField] private HEU_HoudiniAsset _parentAsset;

        [SerializeField] private HAPI_ObjectInfo _objectInfo;

        [SerializeField] private List<HEU_GeoNode> _geoNodes;

        [SerializeField] private HAPI_Transform _objectTransform;

        internal List<HAPI_PartId> _recentlyDestroyedParts = new List<HAPI_PartId>();

        // PUBLIC FUNCTIONS ===========================================================================

        /// <inheritdoc />
        public HEU_SessionBase GetSession()
        {
            if (_parentAsset != null)
                return _parentAsset.GetAssetSession(true);
            else
                return HEU_SessionManager.GetOrCreateDefaultSession();
        }

        /// <inheritdoc />
        public void Recook()
        {
            if (_parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public bool IsInstanced()
        {
            return _objectInfo.isInstanced;
        }

        /// <inheritdoc />
        public bool IsVisible()
        {
            return _objectInfo.isVisible;
        }

        /// <inheritdoc />
        public bool IsUsingMaterial(HEU_MaterialData materialData)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes)
                if (geoNode.IsUsingMaterial(materialData))
                    return true;

            return false;
        }


        /// <inheritdoc />
        public void GetOutputGameObjects(List<GameObject> outputObjects)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes)
                // TODO: check if geoNode.Displayable? elmininates editable nodes
                geoNode.GetOutputGameObjects(outputObjects);
        }

        /// <inheritdoc />
        public void GetOutput(List<HEU_GeneratedOutput> outputs)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.GetOutput(outputs);
        }

        /// <inheritdoc />
        public HEU_PartData GetHDAPartWithGameObject(GameObject outputGameObject)
        {
            HEU_PartData foundPart = null;
            foreach (HEU_GeoNode geoNode in _geoNodes)
            {
                foundPart = geoNode.GetHDAPartWithGameObject(outputGameObject);
                if (foundPart != null) return foundPart;
            }

            return null;
        }

        /// <inheritdoc />
        public HEU_GeoNode GetGeoNode(string geoName)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes)
                if (geoNode.GeoName.Equals(geoName))
                    return geoNode;

            return null;
        }

        /// <inheritdoc />
        public void GetCurves(List<HEU_Curve> curves, bool bEditableOnly)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.GetCurves(curves, bEditableOnly);
        }

        /// <inheritdoc />
        public void GetOutputGeoNodes(List<HEU_GeoNode> outGeoNodes)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes)
                if (geoNode.Displayable)
                    outGeoNodes.Add(geoNode);
        }

        /// <inheritdoc />
        public void HideAllGeometry()
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.HideAllGeometry();
        }

        /// <inheritdoc />
        public void DisableAllColliders()
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.DisableAllColliders();
        }

        /// <inheritdoc />
        public bool IsInstancer()
        {
            if (_objectInfo.isInstancer)
                return true;
            else
                // Check parts for atrrib instancing
                foreach (HEU_GeoNode geoNode in _geoNodes)
                    if (geoNode.HasAttribInstancer())
                        return true;

            return false;
        }

        // ============================================================================================

        //  LOGIC -----------------------------------------------------------------------------------------------------

        public HEU_ObjectNode()
        {
            Reset();
        }

        internal void Reset()
        {
            _objName = "";

            _parentAsset = null;
            _objectInfo = new HAPI_ObjectInfo();
            _geoNodes = new List<HEU_GeoNode>();
            _objectTransform = new HAPI_Transform(true);
        }

        private void SyncWithObjectInfo(HEU_SessionBase session)
        {
            string realName = HEU_SessionManager.GetString(_objectInfo.nameSH, session);
            if (!HEU_PluginSettings.ShortenFolderPaths || realName.Length < 3)
                _objName = realName;
            else
                _objName = realName.Substring(0, 3) + this.GetHashCode();
        }

        internal void Initialize(HEU_SessionBase session, HAPI_ObjectInfo objectInfo, HAPI_Transform objectTranform,
            HEU_HoudiniAsset parentAsset, bool bUseOutputNodes)
        {
            _objectInfo = objectInfo;
            _objectTransform = objectTranform;
            _parentAsset = parentAsset;

            SyncWithObjectInfo(session);

            // Translate transform to Unity (TODO)
            List<HAPI_GeoInfo> geoInfos = new List<HAPI_GeoInfo>();

            HEU_HAPIUtility.GatherAllAssetGeoInfos(session, parentAsset.AssetInfo, objectInfo, bUseOutputNodes,
                ref geoInfos);
            int numGeoInfos = geoInfos.Count;
            for (int i = 0; i < numGeoInfos; ++i)
                // Create GeoNode for each
                _geoNodes.Add(CreateGeoNode(session, geoInfos[i]));
        }

        // This is the old way of getting outputs. Keep it for now for legacy. TODO: Remove this later
        internal void GatherAllAssetOutputsLegacy(HEU_SessionBase session, HAPI_ObjectInfo objectInfo,
            bool bUseOutputNodes, ref List<HEU_GeoNode> geoNodes)
        {
            List<HAPI_GeoInfo> geoInfos = new List<HAPI_GeoInfo>();

            // Get display geo info
            HAPI_GeoInfo displayGeoInfo = new HAPI_GeoInfo();
            if (!session.GetDisplayGeoInfo(objectInfo.nodeId, ref displayGeoInfo)) return;

            //HEU_Logger.LogFormat("Found geoinfo with name {0} and id {1}", HEU_SessionManager.GetString(displayGeoInfo.nameSH, session), displayGeoInfo.nodeId);
            geoInfos.Add(displayGeoInfo);

            if (bUseOutputNodes)
            {
                int outputCount = 0;
                if (!session.GetOutputGeoCount(objectInfo.nodeId, out outputCount)) outputCount = 0;

                if (outputCount > 0)
                {
                    HAPI_GeoInfo[] outputGeoInfos = new HAPI_GeoInfo[outputCount];
                    if (!session.GetOutputGeoInfos(objectInfo.nodeId, ref outputGeoInfos, outputCount))
                        outputGeoInfos = new HAPI_GeoInfo[0];

                    foreach (HAPI_GeoInfo geoInfo in outputGeoInfos)
                    {
                        if (geoInfo.nodeId == displayGeoInfo.nodeId) continue;

                        bool bValidOutput = true;
                        int parentId = HEU_HAPIUtility.GetParentNodeID(session, geoInfo.nodeId);
                        while (parentId >= 0)
                        {
                            if (parentId == geoInfo.nodeId)
                            {
                                // This output node is inside the display geo
                                // Do not use this output to avoid duplicates
                                bValidOutput = false;
                                break;
                            }

                            parentId = HEU_HAPIUtility.GetParentNodeID(session, parentId);
                        }

                        if (bValidOutput)
                        {
                            // Need to cook output geometry to get their parts
                            HAPI_GeoInfo cookedGeoInfo = new HAPI_GeoInfo();
                            session.CookNode(geoInfo.nodeId, HEU_PluginSettings.CookTemplatedGeos);

                            // Get the refreshed geo info
                            if (session.GetGeoInfo(geoInfo.nodeId, ref cookedGeoInfo)) geoInfos.Add(cookedGeoInfo);
                        }
                    }
                }
            }


            // Get editable nodes, cook em, then create geo nodes for them
            HAPI_NodeId[] editableNodes = null;
            HEU_SessionManager.GetComposedChildNodeList(session, objectInfo.nodeId,
                (int)HAPI_NodeType.HAPI_NODETYPE_SOP, (int)HAPI_NodeFlags.HAPI_NODEFLAGS_EDITABLE, true,
                out editableNodes);
            if (editableNodes != null)
                foreach (HAPI_NodeId editNodeID in editableNodes)
                    if (editNodeID != displayGeoInfo.nodeId)
                    {
                        session.CookNode(editNodeID, HEU_PluginSettings.CookTemplatedGeos);

                        HAPI_GeoInfo editGeoInfo = new HAPI_GeoInfo();
                        if (session.GetGeoInfo(editNodeID, ref editGeoInfo)) geoInfos.Add(editGeoInfo);
                    }

            //HEU_Logger.LogFormat("Object id={5}, name={0}, isInstancer={1}, isInstanced={2}, instancePath={3}, instanceId={4}", 
            //	HEU_SessionManager.GetString(objectInfo.nameSH, session), objectInfo.isInstancer, objectInfo.isInstanced, 
            //	HEU_SessionManager.GetString(objectInfo.objectInstancePathSH, session), objectInfo.objectToInstanceId, objectInfo.nodeId);

            // Go through geo infos to create geometry
            int numGeoInfos = geoInfos.Count;
            for (int i = 0; i < numGeoInfos; ++i)
                // Create GeoNode for each
                geoNodes.Add(CreateGeoNode(session, geoInfos[i]));
        }

        /// <summary>
        /// Destroy all data.
        /// </summary>
        internal void DestroyAllData(bool bIsRebuild = false)
        {
            if (_geoNodes != null)
            {
                for (int i = 0; i < _geoNodes.Count; ++i)
                {
                    _geoNodes[i].DestroyAllData(bIsRebuild);
                    HEU_GeneralUtility.DestroyImmediate(_geoNodes[i]);
                }

                _geoNodes.Clear();
            }
        }

        private HEU_GeoNode CreateGeoNode(HEU_SessionBase session, HAPI_GeoInfo geoInfo)
        {
            HEU_GeoNode geoNode = ScriptableObject.CreateInstance<HEU_GeoNode>();
            geoNode.Initialize(session, geoInfo, this);
            geoNode.UpdateGeo(session);
            return geoNode;
        }

        /// <summary>
        /// Get debug info for this object
        /// </summary>
        internal void GetDebugInfo(StringBuilder sb)
        {
            int numGeos = _geoNodes != null ? _geoNodes.Count : 0;

            sb.AppendFormat("ObjectID: {0}, Name: {1}, Geos: {2}, Parent: {3}\n", ObjectID, ObjectName, numGeos,
                _parentAsset);

            if (_geoNodes != null)
                foreach (HEU_GeoNode geo in _geoNodes)
                    geo.GetDebugInfo(sb);
        }

        internal void SetObjectInfo(HAPI_ObjectInfo newObjectInfo)
        {
            _objectInfo = newObjectInfo;
        }

        /// <summary>
        /// Retrieves object info from Houdini session and updates internal state.
        /// New geo nodes are created, unused geo nodes are destroyed.
        /// Geo nodes are then refreshed to be in sync with Houdini session.
        /// </summary>
        /// <returns>True if internal state has changed (including geometry).</returns>
        internal void UpdateObject(HEU_SessionBase session, bool bForceUpdate)
        {
            if (ParentAsset == null) return;

            // Update the geo info
            if (!session.GetObjectInfo(ObjectID, ref _objectInfo)) return;

            SyncWithObjectInfo(session);

            // Update the object transform
            _objectTransform = ParentAsset.GetObjectTransform(session, ObjectID);

            // Container for existing geo nodes that are still in use
            List<HEU_GeoNode> geoNodesToKeep = new List<HEU_GeoNode>();

            // Container for new geo infos that need to be created
            List<HAPI_GeoInfo> newGeoInfosToCreate = new List<HAPI_GeoInfo>();

            if (_objectInfo.haveGeosChanged || bForceUpdate)
            {
                // Indicates that the geometry nodes have changed
                //HEU_Logger.Log("Geos have changed!");

                // Form a list of geo infos that are now present after cooking
                List<HAPI_GeoInfo> postCookGeoInfos = new List<HAPI_GeoInfo>();


                bool useOutputNodes = true;
                if (ParentAsset) useOutputNodes = ParentAsset.UseOutputNodes;

                HEU_HAPIUtility.GatherAllAssetGeoInfos(session, ParentAsset.AssetInfo, _objectInfo, useOutputNodes,
                    ref postCookGeoInfos);

                // Now for each geo node that are present after cooking, we check if its
                // new or whether we already have it prior to cooking.
                int numPostCookGeoInfos = postCookGeoInfos.Count;
                for (int i = 0; i < numPostCookGeoInfos; i++)
                {
                    string geoName = HEU_SessionManager.GetString(postCookGeoInfos[i].nameSH, session);

                    bool bFound = false;
                    for (int j = 0; j < _geoNodes.Count; j++)
                    {
                        string oldGeoName = _geoNodes[j].GeoName;

                        if (geoName.Equals(oldGeoName)
                            // Fixes Bug #124004
                            // Newly created curves all use "curve" for their geo name, 
                            // but loaded curves use "curveX", this caused loaded curves to disappear if recooked
                            // due to the saved name for the geo being "curve" and the newly created node name to be "curveX"
                            || (oldGeoName.Equals("curve") && geoName.StartsWith("curve")))
                        {
                            _geoNodes[j].SetGeoInfo(postCookGeoInfos[i]);

                            geoNodesToKeep.Add(_geoNodes[j]);
                            _geoNodes.RemoveAt(j);

                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound) newGeoInfosToCreate.Add(postCookGeoInfos[i]);
                }

                // Whatever is left in _geoNodes is no longer needed so clean up
                int numCurrentGeos = _geoNodes.Count;
                for (int i = 0; i < numCurrentGeos; ++i) _geoNodes[i].DestroyAllData();
            }
            else
            {
                Debug.Assert(_objectInfo.geoCount == _geoNodes.Count, "Expected same number of geometry nodes.");
            }

            // Go through the old geo nodes that are still in use and update if necessary.
            foreach (HEU_GeoNode geoNode in geoNodesToKeep)
            {
                // Get geo info and check if geo changed
                bool bGeoChanged = bForceUpdate || geoNode.HasGeoNodeChanged(session);
                if (bGeoChanged)
                {
                    geoNode.UpdateGeo(session);
                }
                else
                {
                    if (_objectInfo.haveGeosChanged)
                        // Clear object instances since the object info has changed.
                        // Without this, the object instances were never getting updated
                        // if only the inputs changed but not outputs (of instancers).
                        geoNode.ClearObjectInstances();

                    // Visiblity might have changed, so update that
                    geoNode.CalculateVisiblity(IsVisible());
                    geoNode.CalculateColliderState();
                }
            }

            // Create the new geo infos and add to our keep list
            foreach (HAPI_GeoInfo newGeoInfo in newGeoInfosToCreate)
                geoNodesToKeep.Add(CreateGeoNode(session, newGeoInfo));

            // Overwrite the old list with new
            _geoNodes = geoNodesToKeep;

            // Updating the trasform is done in GenerateGeometry
        }

        internal void GenerateGeometry(HEU_SessionBase session, bool bRebuild)
        {
            // Volumes could come in as a geonode + part for each heightfield layer.
            // Otherwise the other geo types can be done individually.

            bool bResult = false;

            List<HEU_PartData> meshParts = new List<HEU_PartData>();
            List<HEU_PartData> volumeParts = new List<HEU_PartData>();

            List<HEU_PartData> partsToDestroy = new List<HEU_PartData>();

            HEU_HoudiniAsset parentAsset = ParentAsset;
            if (parentAsset == null) return;

            _recentlyDestroyedParts.Clear();

            foreach (HEU_GeoNode geoNode in _geoNodes)
            {
                geoNode.GetPartsByOutputType(meshParts, volumeParts);

                if (volumeParts.Count > 0)
                {
                    // Volumes
                    // Each layer in the volume is retrieved as a volume part, in the display geo node. 
                    // But we need to handle all layers as 1 terrain output in Unity, with 1 height layer and 
                    // other layers as alphamaps.
                    geoNode.ProcessVolumeParts(session, volumeParts, bRebuild);

                    // Clear the volume parts after processing since we are done with this set
                    volumeParts.Clear();
                }
            }

            // Meshes
            foreach (HEU_PartData part in meshParts)
            {
                // This returns false when there is no valid geometry or is not instancing. Should remove it as otherwise
                // stale data sticks around on recook
                bResult = part.GenerateMesh(session, parentAsset.GenerateUVs, parentAsset.GenerateTangents,
                    parentAsset.GenerateNormals, parentAsset.UseLODGroups);
                if (!bResult)
                {
                    partsToDestroy.Add(part);
                    _recentlyDestroyedParts.Add(part.PartID);
                }
            }

            int numPartsToDestroy = partsToDestroy.Count;
            for (int i = 0; i < numPartsToDestroy; ++i)
            {
                HEU_GeoNode parentNode = partsToDestroy[i].ParentGeoNode;
                if (parentNode != null)
                    parentNode.RemoveAndDestroyPart(partsToDestroy[i]);
                else
                    HEU_PartData.DestroyPart(partsToDestroy[i]);
            }

            partsToDestroy.Clear();

            ApplyObjectTransformToGeoNodes();

            // Set visibility and attribute-based tag, layer, and scripts
            bool bIsVisible = IsVisible();
            foreach (HEU_GeoNode geoNode in _geoNodes)
            {
                geoNode.CalculateVisiblity(bIsVisible);
                geoNode.CalculateColliderState();

                geoNode.SetAttributeModifiersOnPartOutputs(session);
            }

            // Create editable attributes.
            // This should happen after visibility has been calculated above
            // since we need to show/hide the intermediate geometry during painting.
            foreach (HEU_PartData part in meshParts)
                if (part.ParentGeoNode.IsIntermediateOrEditable())
                    part.SetupAttributeGeometry(session);
        }

        internal void GeneratePartInstances(HEU_SessionBase session)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.GeneratePartInstances(session);
        }

        internal void GenerateAttributesStore(HEU_SessionBase session)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.GenerateAttributesStore(session);
        }

        /// <summary>
        /// Apply this object's transform to all its geo nodes.
        /// </summary>
        internal void ApplyObjectTransformToGeoNodes()
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.ApplyHAPITransform(ref _objectTransform);
        }

        internal void GetClonableParts(List<HEU_PartData> clonableParts)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes)
                if (geoNode.Displayable)
                    geoNode.GetClonableParts(clonableParts);
        }


        /// <summary>
        /// Generates object instances.
        /// Skips parts that already have their instances generated.
        /// </summary>
        /// <param name="session">Active session to use</param>
        internal void GenerateObjectInstances(HEU_SessionBase session)
        {
            if (ParentAsset == null) return;

            if (!IsInstancer())
            {
                HEU_Logger.LogErrorFormat(
                    "Generate object instances called on a non-instancer object {0} for asset {1}!", ObjectName,
                    ParentAsset.AssetName);
                return;
            }

            //HEU_Logger.LogFormat("Generate Object Instances:: id={5}, name={0}, isInstancer={1}, isInstanced={2}, instancePath={3}, instanceId={4}", HEU_SessionManager.GetString(_objectInfo.nameSH, session), 
            //	_objectInfo.isInstancer, _objectInfo.isInstanced, HEU_SessionManager.GetString(_objectInfo.objectInstancePathSH, session), _objectInfo.objectToInstanceId, _objectInfo.nodeId);

            // Is this a Houdini attribute instancer?
            string instanceAttrName = HEU_PluginSettings.InstanceAttr;
            string unityInstanceAttrName = HEU_PluginSettings.UnityInstanceAttr;
            string instancePrefixAttrName = HEU_Defines.DEFAULT_INSTANCE_PREFIX_ATTR;

            HAPI_AttributeInfo instanceAttrInfo = new HAPI_AttributeInfo();
            HAPI_AttributeInfo unityInstanceAttrInfo = new HAPI_AttributeInfo();
            HAPI_AttributeInfo instancePrefixAttrInfo = new HAPI_AttributeInfo();
            HAPI_AttributeInfo materialAttrInfo = new HAPI_AttributeInfo();

            int numGeos = _geoNodes.Count;
            for (int i = 0; i < numGeos; ++i)
                if (_geoNodes[i].Displayable)
                {
                    List<HEU_PartData> parts = _geoNodes[i].GetParts();
                    int numParts = parts.Count;
                    for (int j = 0; j < numParts; ++j)
                    {
                        if (parts[j]._objectInstancesGenerated || parts[j].IsPartVolume())
                            // This prevents instances being created unnecessarily (e.g. part hasn't changed since last cook).
                            // Or for volumes that might have instance attributes.
                            continue;

                        HEU_GeneralUtility.GetAttributeInfo(session, _geoNodes[i].GeoID, parts[j].PartID,
                            instanceAttrName, ref instanceAttrInfo);
                        HEU_GeneralUtility.GetAttributeInfo(session, _geoNodes[i].GeoID, parts[j].PartID,
                            unityInstanceAttrName, ref unityInstanceAttrInfo);

                        string[] instancePrefixes = null;
                        HEU_GeneralUtility.GetAttributeInfo(session, _geoNodes[i].GeoID, parts[j].PartID,
                            instancePrefixAttrName, ref instancePrefixAttrInfo);
                        if (instancePrefixAttrInfo.exists)
                            instancePrefixes = HEU_GeneralUtility.GetAttributeStringData(session, _geoNodes[i].GeoID,
                                parts[j].PartID, instancePrefixAttrName, ref instancePrefixAttrInfo);

                        string[] instanceMaterialPaths = null;
                        HEU_GeneralUtility.GetAttributeInfo(session, _geoNodes[i].GeoID, parts[j].PartID,
                            HEU_PluginSettings.UnityMaterialAttribName, ref materialAttrInfo);
                        if (materialAttrInfo.exists)
                            instanceMaterialPaths = HEU_GeneralUtility.GetAttributeStringData(session,
                                _geoNodes[i].GeoID, parts[j].PartID, HEU_PluginSettings.UnityMaterialAttribName,
                                ref materialAttrInfo);

                        if (instanceAttrInfo.exists)
                        {
                            // Object instancing via Houdini instance attribute
                            parts[j].GenerateInstancesFromObjectIds(session, instancePrefixes, instanceMaterialPaths);
                        }
                        else if (unityInstanceAttrInfo.exists)
                        {
                            // Object instancing via existing Unity object (path from point attribute)
                            // Attribute owner type determines whether to use single instanced object (detail) or multiple (point)
                            if (unityInstanceAttrInfo.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT ||
                                unityInstanceAttrInfo.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL)
                            {
                                parts[j].GenerateInstancesFromUnityAssetPathAttribute(session, unityInstanceAttrName);
                            }
                            else
                            {
                                // Other attribute owned types are unsupported.
                                // Originally had a warning here, but unnecessary as in some cases (e.g. heightfield attrbiutes) the
                                // attribute owner could be changed in HAPI.
                            }
                        }
                        else
                        {
                            // Standard object instancing via single Houdini object
                            if (_objectInfo.objectToInstanceId == HEU_Defines.HEU_INVALID_NODE_ID)
                                // HEU_Logger.LogAssertionFormat("Invalid object ID {0} used for object instancing. "
                                // 	+ "Make sure to turn on Full point instancing and set the correct Instance Object.", _objectInfo.objectToInstanceId);
                                // Could be a part instancer
                                continue;

                            parts[j].GenerateInstancesFromObjectID(session, _objectInfo.objectToInstanceId,
                                instancePrefixes, instanceMaterialPaths);
                        }
                    }
                }
        }

        internal void ClearObjectInstances(HEU_SessionBase session)
        {
            if (!IsInstancer()) return;

            int numGeos = _geoNodes.Count;
            for (int i = 0; i < numGeos; ++i)
                if (_geoNodes[i].Displayable)
                {
                    List<HEU_PartData> parts = _geoNodes[i].GetParts();
                    int numParts = parts.Count;
                    for (int j = 0; j < numParts; ++j)
                    {
                        if (parts[j]._objectInstancesGenerated || parts[j].IsPartVolume()) continue;

                        // Must clear out instances, as otherwise we get duplicates
                        parts[j].ClearInstances();

                        // Clear out invalid object instance infos that no longer have any valid parts
                        parts[j].ClearInvalidObjectInstanceInfos();
                    }
                }
        }

        /// <summary>
        /// Fill in the objInstanceInfos list with the HEU_ObjectInstanceInfos used by this object.
        /// </summary>
        /// <param name="objInstanceInfos">List to fill in</param>
        internal void PopulateObjectInstanceInfos(List<HEU_ObjectInstanceInfo> objInstanceInfos)
        {
            if (IsInstancer())
            {
                int numGeos = _geoNodes.Count;
                for (int i = 0; i < numGeos; ++i)
                    if (_geoNodes[i].Displayable)
                    {
                        List<HEU_PartData> parts = _geoNodes[i].GetParts();
                        int numParts = parts.Count;
                        for (int j = 0; j < numParts; ++j) parts[j].PopulateObjectInstanceInfos(objInstanceInfos);
                    }
            }
        }

        /// <summary>
        /// Process custom attribute with Unity script name, and attach any scripts found.
        /// </summary>
        /// <param name="session">Session to use</param>
        internal void ProcessUnityScriptAttributes(HEU_SessionBase session)
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.ProcessUnityScriptAttribute(session);
        }

        /// <summary>
        /// Calculate visiblity of all geometry within
        /// </summary>
        internal void CalculateVisibility()
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.CalculateVisiblity(IsVisible());
        }

        internal void CalculateColliderState()
        {
            foreach (HEU_GeoNode geoNode in _geoNodes) geoNode.CalculateColliderState();
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(_objName) ? "ObjectNode: " + _objName : base.ToString();
        }

        public bool IsEquivalentTo(HEU_ObjectNode other)
        {
            bool bResult = true;

            string header = "HEU_ObjectNode";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._objectInfo.ToTestObject(), other._objectInfo.ToTestObject(),
                ref bResult, header, "Object Info");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._geoNodes, other._geoNodes, ref bResult, header, "Geo Node");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._objectTransform.ToTestObject(),
                other._objectTransform.ToTestObject(), ref bResult, header, "Object transform");

            return bResult;
        }
    }
} // HoudiniEngineUnity
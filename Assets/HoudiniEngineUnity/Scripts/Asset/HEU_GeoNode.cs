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

// Uncomment to profile
//#define HEU_PROFILER_ON

#define TERRAIN_SUPPORTED

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
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


    /// <summary>
    /// Represents a Geometry (SOP) node.
    /// </summary>
    public class HEU_GeoNode : ScriptableObject, IHEU_GeoNode, IHEU_HoudiniAssetSubcomponent, ISerializationCallbackReceiver, IEquivable<HEU_GeoNode>
    {
        // PUBLIC FIELDS ==========================================================================

        /// <inheritdoc />
        public HEU_HoudiniAsset ParentAsset
        {
            get { return (_containerObjectNode != null) ? _containerObjectNode.ParentAsset : null; }
        }

        /// <inheritdoc />
        public HAPI_NodeId GeoID
        {
            get { return _geoInfo.nodeId; }
        }

        /// <inheritdoc />
        public HAPI_GeoInfo GeoInfo
        {
            get { return _geoInfo; }
        }

        /// <inheritdoc />
        public string GeoName
        {
            get { return _geoName; }
        }

        /// <inheritdoc />
        public HAPI_GeoType GeoType
        {
            get { return _geoInfo.type; }
        }

        /// <inheritdoc />
        public bool Editable
        {
            get { return _geoInfo.isEditable; }
        }

        /// <inheritdoc />
        public bool Displayable
        {
            get
            {
                if (ParentAsset && ParentAsset.UseOutputNodes == true) return true; // Trust that it is displayable if using output nodes

                return _geoInfo.isDisplayGeo;
            }
        }

        /// <inheritdoc />
        public List<HEU_PartData> Parts
        {
            get { return _parts; }
        }

        /// <inheritdoc />
        public HEU_ObjectNode ObjectNode
        {
            get { return _containerObjectNode; }
        }

        /// <inheritdoc />
        public HEU_InputNode InputNode
        {
            get { return _inputNode; }
        }

        /// <inheritdoc />
        public HEU_Curve GeoCurve
        {
            get { return _geoCurve; }
        }

        /// <inheritdoc />
        public List<HEU_VolumeCache> VolumeCaches
        {
            get { return _volumeCaches; }
        }

        // =========================================================================================

        //	DATA ------------------------------------------------------------------------------------------------------

        [SerializeField] private HAPI_GeoInfo _geoInfo;

        [SerializeField] private string _geoName;

        [SerializeField] private List<HEU_PartData> _parts;

        [SerializeField] private HEU_ObjectNode _containerObjectNode;


        [SerializeField] private HEU_InputNode _inputNode;

        [SerializeField] private HEU_Curve _geoCurve;

        // Deprecated by _volumeCaches. Keeping it for backwards compatibility on saved assets.
        [SerializeField] private HEU_VolumeCache _volumeCache;

        [SerializeField] private List<HEU_VolumeCache> _volumeCaches;

        // PUBLIC FUNCTIONS ===============================================================
        public HEU_GeoNode()
        {
            Reset();
        }

        // Implement methods
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            // _volumeCaches replaces _volumeCache, and _volumeCache has been deprecated.
            // This takes care of moving in the old _volumeCache into _volumeCaches.
            if (_volumeCache != null && (_volumeCaches == null || _volumeCaches.Count == 0))
            {
                _volumeCaches = new List<HEU_VolumeCache>();
                _volumeCaches.Add(_volumeCache);
                _volumeCache = null;
            }
        }

        /// <inheritdoc />
        public HEU_SessionBase GetSession()
        {
            if (ParentAsset != null)
            {
                return ParentAsset.GetAssetSession(true);
            }
            else
            {
                return HEU_SessionManager.GetOrCreateDefaultSession();
            }
        }

        /// <inheritdoc />
        public void Recook()
        {
            if (ParentAsset != null) ParentAsset.RequestCook();
        }

        /// <inheritdoc />
        public bool IsVisible()
        {
            return _containerObjectNode.IsVisible();
        }

        /// <inheritdoc />
        public bool IsIntermediate()
        {
            return (_geoInfo.type == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE);
        }

        /// <inheritdoc />
        public bool IsIntermediateOrEditable()
        {
            return (_geoInfo.type == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE ||
                    (_geoInfo.type == HAPI_GeoType.HAPI_GEOTYPE_DEFAULT && _geoInfo.isEditable));
        }

        /// <inheritdoc />
        public bool IsGeoInputType()
        {
            return _geoInfo.isEditable && _geoInfo.type == HAPI_GeoType.HAPI_GEOTYPE_INPUT;
        }

        /// <inheritdoc />
        public bool IsGeoCurveType()
        {
            return _geoInfo.type == HAPI_GeoType.HAPI_GEOTYPE_CURVE;
        }

        /// <inheritdoc />
        public void DestroyAllData(bool bIsRebuild = false)
        {
            HEU_PartData.DestroyParts(_parts, bIsRebuild);

            if (_inputNode != null)
            {
                HEU_SessionBase session = null;
                if (ParentAsset != null)
                {
                    ParentAsset.RemoveInputNode(_inputNode);
                    session = ParentAsset.GetAssetSession(false);
                }

                _inputNode.DestroyAllData(session);
                HEU_GeneralUtility.DestroyImmediate(_inputNode);
                _inputNode = null;
            }

            if (_geoCurve != null)
            {
                if (ParentAsset != null)
                {
                    ParentAsset.RemoveCurve(_geoCurve);
                }

                _geoCurve.DestroyAllData(bIsRebuild);
                HEU_GeneralUtility.DestroyImmediate(_geoCurve);
                _geoCurve = null;
            }

            DestroyVolumeCache();
        }

        /// <inheritdoc />
        public void RemoveAndDestroyPart(HEU_PartData part)
        {
            _parts.Remove(part);
            HEU_PartData.DestroyPart(part);
        }

        /// <inheritdoc />
        public void GetOutputGameObjects(List<GameObject> outputObjects)
        {
            foreach (HEU_PartData part in _parts)
            {
                part.GetOutputGameObjects(outputObjects);
            }
        }

        /// <inheritdoc />
        public void GetOutput(List<HEU_GeneratedOutput> outputs)
        {
            foreach (HEU_PartData part in _parts)
            {
                part.GetOutput(outputs);
            }
        }

        /// <inheritdoc />
        public HEU_PartData GetHDAPartWithGameObject(GameObject outputGameObject)
        {
            HEU_PartData foundPart = null;
            foreach (HEU_PartData part in _parts)
            {
                foundPart = part.GetHDAPartWithGameObject(outputGameObject);
                if (foundPart != null)
                {
                    return foundPart;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public HEU_PartData GetPartFromPartID(HAPI_NodeId partID)
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                if (_parts[i].PartID == partID)
                {
                    return _parts[i];
                }
            }

            return null;
        }

        /// <inheritdoc />
        public void GetCurves(List<HEU_Curve> curves, bool bEditableOnly)
        {
            if (_geoCurve != null && (!bEditableOnly || _geoCurve.IsEditable()))
            {
                curves.Add(_geoCurve);
            }

            foreach (HEU_PartData part in _parts)
            {
                HEU_Curve partCurve = part.GetCurve(bEditableOnly);
                if (partCurve != null)
                {
                    curves.Add(partCurve);
                }
            }
        }

        /// <inheritdoc />
        public List<HEU_PartData> GetParts()
        {
            return _parts;
        }

        /// <inheritdoc />
        public void HideAllGeometry()
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                _parts[i].SetVisiblity(false);
            }
        }

        /// <inheritdoc />
        public void DisableAllColliders()
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                _parts[i].SetColliderState(false);
            }
        }

        /// <inheritdoc />
        public HEU_VolumeCache GetVolumeCacheByTileIndex(int tileIndex)
        {
            if (_volumeCaches != null)
            {
                int numCaches = _volumeCaches.Count;
                for (int i = 0; i < numCaches; ++i)
                {
                    if (_volumeCaches[i] != null && _volumeCaches[i].TileIndex == tileIndex)
                    {
                        return _volumeCaches[i];
                    }
                }
            }
            else if (_volumeCache != null)
            {
                return _volumeCache;
            }

            return null;
        }

        // ================================================================================

        //  LOGIC -----------------------------------------------------------------------------------------------------


        internal void Reset()
        {
            _geoName = "";

            _geoInfo = new HAPI_GeoInfo();
            _geoInfo.nodeId = -1;
            _geoInfo.type = HAPI_GeoType.HAPI_GEOTYPE_DEFAULT;
            _parts = new List<HEU_PartData>();
        }

        internal void Initialize(HEU_SessionBase session, HAPI_GeoInfo geoInfo, HEU_ObjectNode containerObjectNode)
        {
            _containerObjectNode = containerObjectNode;
            _geoInfo = geoInfo;

            string realName = HEU_SessionManager.GetString(_geoInfo.nameSH, session);
            if (!HEU_PluginSettings.ShortenFolderPaths || realName.Length < 3)
            {
                _geoName = HEU_SessionManager.GetString(_geoInfo.nameSH, session);
            }
            else
            {
                _geoName = realName.Substring(0, 3) + this.GetHashCode();
            }

            //HEU_Logger.Log(string.Format("GeoNode initialized with ID={0}, name={1}, type={2}", GeoID, GeoName, geoInfo.type));
        }

        internal bool DoesThisRequirePotentialCook()
        {
            if ((_geoInfo.type == HAPI_GeoType.HAPI_GEOTYPE_INPUT)
                || (_geoInfo.isTemplated && !HEU_PluginSettings.CookTemplatedGeos && !_geoInfo.isEditable)
                || (!_geoInfo.hasGeoChanged)
                || (!_geoInfo.isDisplayGeo && (_geoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_CURVE)))
            {
                return false;
            }

            return true;
        }

        internal void UpdateGeo(HEU_SessionBase session)
        {
            // Create or recreate parts.

            bool bObjectInstancer = _containerObjectNode.IsInstancer();

            // Save list of old parts. We'll destroy these after creating new parts.
            // The reason for temporarily keeping these is to transfer data (eg. instance overrides, attribute data)
            List<HEU_PartData> oldParts = new List<HEU_PartData>(_parts);
            _parts.Clear();

            if (ParentAsset == null)
            {
                return;
            }

            try
            {
                if (!_geoInfo.isDisplayGeo && !ParentAsset.UseOutputNodes)
                {
                    if (ParentAsset.IgnoreNonDisplayNodes)
                    {
                        return;
                    }
                    else if (!_geoInfo.isEditable
                             || (_geoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_DEFAULT
                                 && _geoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE
                                 && _geoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_CURVE))
                    {
                        return;
                    }
                }

                if (IsGeoCurveType())
                {
                    ProcessGeoCurve(session);
                }
                else
                {
                    int numParts = _geoInfo.partCount;
                    //HEU_Logger.Log("Number of parts: " + numParts);
                    //HEU_Logger.LogFormat("GeoNode type {0}, isTemplated: {1}, isDisplayGeo: {2}, isEditable: {3}", _geoInfo.type, _geoInfo.isTemplated, _geoInfo.isDisplayGeo, _geoInfo.isEditable);
                    for (int i = 0; i < numParts; ++i)
                    {
                        HAPI_PartInfo partInfo = new HAPI_PartInfo();
                        if (!session.GetPartInfo(GeoID, i, ref partInfo))
                        {
                            HEU_Logger.LogErrorFormat("Unable to get PartInfo for geo node {0} and part {1}.", GeoID, i);
                            continue;
                        }

                        // Find the old part for this new part.
                        HEU_PartData part = null;
                        HEU_PartData oldMatchedPart = null;

                        foreach (HEU_PartData oldPart in oldParts)
                        {
                            string partName = HEU_SessionManager.GetString(partInfo.nameSH, session);
                            if (oldPart.PartName.Equals(partName))
                            {
                                oldMatchedPart = oldPart;
                            }
                        }

                        if (oldMatchedPart != null)
                        {
                            //HEU_Logger.Log("Found matched part: " + oldMatchedPart.name);

                            // ProcessPart will clear out the object instances, so hence why
                            // we keep a copy here, then restore after processing the parts.
                            // Note that at this point we don't know if the the new part is an
                            // instancer or not (e.g. object, attribute instancer).
                            List<HEU_ObjectInstanceInfo> sourceObjectInstanceInfos = oldMatchedPart.GetObjectInstanceInfos();

                            // Clear out old generated data
                            oldMatchedPart.ClearGeneratedData();

                            part = oldMatchedPart;
                            oldParts.Remove(oldMatchedPart);

                            ProcessPart(session, i, ref partInfo, ref part);

                            if (sourceObjectInstanceInfos != null && part != null && (bObjectInstancer || part.IsAttribInstancer()))
                            {
                                // Set object instances from old part into new. This keeps the user set object inputs around.
                                part.SetObjectInstanceInfos(sourceObjectInstanceInfos);
                            }
                        }
                        else
                        {
                            ProcessPart(session, i, ref partInfo, ref part);
                        }
                    }
                }
            }
            finally
            {
                HEU_PartData.DestroyParts(oldParts);
            }
        }

        /// <summary>
        /// Process custom attribute with Unity script name, and attach any scripts found.
        /// </summary>
        /// <param name="session">Session to use</param>
        internal void ProcessUnityScriptAttribute(HEU_SessionBase session)
        {
            if (_parts == null || _parts.Count == 0)
            {
                return;
            }

            foreach (HEU_PartData part in _parts)
            {
                GameObject outputGO = part.OutputGameObject;
                if (outputGO != null)
                {
                    string scriptValue = HEU_GeneralUtility.GetUnityScriptAttributeValue(session, GeoID, part.PartID);
                    if (!string.IsNullOrEmpty(scriptValue))
                    {
                        HEU_GeneralUtility.AttachScriptWithInvokeFunction(scriptValue, outputGO);
                    }
                }
            }
        }

        /// <summary>
        /// Process the part at the given index, creating its data (geometry),
        /// and adding it to the list of parts.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="partID"></param>
        /// <returns>A valid HEU_PartData if it has been successfully processed.</returns>
        private void ProcessPart(HEU_SessionBase session, int partID, ref HAPI_PartInfo partInfo, ref HEU_PartData partData)
        {
            HEU_HoudiniAsset parentAsset = ParentAsset;
            if (parentAsset == null)
            {
                return;
            }

            bool bResult = true;
            //HEU_Logger.LogFormat("Part: name={0}, id={1}, type={2}, instanced={3}, instance count={4}, instance part count={5}", HEU_SessionManager.GetString(partInfo.nameSH, session), partID, partInfo.type, partInfo.isInstanced, partInfo.instanceCount, partInfo.instancedPartCount);

#if HEU_PROFILER_ON
			float processPartStartTime = Time.realtimeSinceStartup;
#endif

            bool isPartEditable = IsIntermediateOrEditable();
            bool isAttribInstancer = false;

            if (IsGeoInputType())
            {
                // Setup for input node to accept inputs
                if (_inputNode == null)
                {
                    string partName = HEU_SessionManager.GetString(partInfo.nameSH, session);
                    _inputNode = HEU_InputNode.CreateSetupInput(GeoID, 0, partName, partName, HEU_InputNode.InputNodeType.NODE, ParentAsset);
                    if (_inputNode != null)
                    {
                        ParentAsset.AddInputNode(_inputNode);
                    }
                }

                if (HEU_HAPIUtility.IsSupportedPolygonType(partInfo.type) && partInfo.vertexCount == 0)
                {
                    // No geometry for input asset

                    if (partData != null)
                    {
                        // Clean up existing part
                        HEU_PartData.DestroyPart(partData);
                        partData = null;
                    }

                    // No need to process further since we don't have geometry
                    return;
                }
            }
            else
            {
                // Preliminary check for attribute instancing (mesh type with no verts but has points with instances)
                if (HEU_HAPIUtility.IsSupportedPolygonType(partInfo.type) && partInfo.vertexCount == 0 && partInfo.pointCount > 0)
                {
                    if (HEU_GeneralUtility.HasValidInstanceAttribute(session, GeoID, partID, HEU_PluginSettings.UnityInstanceAttr))
                    {
                        isAttribInstancer = true;
                    }
                    else if (HEU_GeneralUtility.HasValidInstanceAttribute(session, GeoID, partID,
                                 HEU_Defines.HEIGHTFIELD_TREEINSTANCE_PROTOTYPEINDEX))
                    {
                        isAttribInstancer = true;
                    }
                }
            }

            if (partInfo.type == HAPI_PartType.HAPI_PARTTYPE_INVALID)
            {
                // Clean up invalid parts
                if (partData != null)
                {
                    HEU_PartData.DestroyPart(partData);
                    partData = null;
                }
            }
            else if (partInfo.type < HAPI_PartType.HAPI_PARTTYPE_MAX)
            {
                // Process the part based on type. Keep or ignore.

                // We treat parts of type curve as curves, along with geo nodes that are editable and type curves
                if (partInfo.type == HAPI_PartType.HAPI_PARTTYPE_CURVE)
                {
                    if (partData == null)
                    {
                        partData = ScriptableObject.CreateInstance<HEU_PartData>();
                    }

                    partData.Initialize(session, partID, GeoID, _containerObjectNode.ObjectID, this, ref partInfo,
                        HEU_PartData.PartOutputType.CURVE, isPartEditable, _containerObjectNode.IsInstancer(), false);
                    SetupGameObjectAndTransform(partData, parentAsset);
                    partData.ProcessCurvePart(session, partID);
                }
                else if (partInfo.type == HAPI_PartType.HAPI_PARTTYPE_VOLUME)
                {
                    // We only process "height" volume parts. Other volume parts are ignored for now.

#if TERRAIN_SUPPORTED
                    HAPI_VolumeInfo volumeInfo = new HAPI_VolumeInfo();
                    bResult = session.GetVolumeInfo(GeoID, partID, ref volumeInfo);
                    if (!bResult)
                    {
                        HEU_Logger.LogErrorFormat("Unable to get volume info for geo node {0} and part {1} ", GeoID, partID);
                    }
                    else
                    {
                        if (Displayable && !IsIntermediateOrEditable())
                        {
                            if (partData == null)
                            {
                                partData = ScriptableObject.CreateInstance<HEU_PartData>();
                            }
                            else
                            {
                                // Clear mesh data to handle case where switching from polygonal mesh to volume output.
                                partData.ClearGeneratedMeshOutput();
                            }

                            partData.Initialize(session, partID, GeoID, _containerObjectNode.ObjectID, this, ref partInfo,
                                HEU_PartData.PartOutputType.VOLUME, isPartEditable, _containerObjectNode.IsInstancer(), false);
                            SetupGameObjectAndTransform(partData, ParentAsset);
                        }
                    }
#else
					HEU_Logger.LogWarningFormat("Terrain (heightfield volume) is not yet supported.");
#endif
                }
                else if (partInfo.type == HAPI_PartType.HAPI_PARTTYPE_INSTANCER || isAttribInstancer)
                {
                    if (partData == null)
                    {
                        partData = ScriptableObject.CreateInstance<HEU_PartData>();
                    }
                    else
                    {
                        partData.ClearGeneratedMeshOutput();
                        partData.ClearGeneratedVolumeOutput();
                    }

                    partData.Initialize(session, partID, GeoID, _containerObjectNode.ObjectID, this, ref partInfo,
                        HEU_PartData.PartOutputType.INSTANCER, isPartEditable, _containerObjectNode.IsInstancer(), isAttribInstancer);
                    SetupGameObjectAndTransform(partData, parentAsset);
                }
                else if (HEU_HAPIUtility.IsSupportedPolygonType(partInfo.type))
                {
                    if (partData == null)
                    {
                        partData = ScriptableObject.CreateInstance<HEU_PartData>();
                    }
                    else
                    {
                        // Clear volume data (case where switching from something other output to mesh)
                        partData.ClearGeneratedVolumeOutput();
                    }

                    partData.Initialize(session, partID, GeoID, _containerObjectNode.ObjectID, this, ref partInfo,
                        HEU_PartData.PartOutputType.MESH, isPartEditable, _containerObjectNode.IsInstancer(), false);

                    // This check allows to ignore editable non-display nodes by default, but commented out to allow
                    // them for now. Users can also ignore them by turning on IgnoreNonDisplayNodes
                    //if (Displayable || (Editable && ParentAsset.EditableNodesToolsEnabled))
                    {
                        SetupGameObjectAndTransform(partData, parentAsset);
                    }
                }
                else
                {
                    HEU_Logger.LogWarningFormat("Unsupported part type {0}", partInfo.type);
                }

                if (partData != null)
                {
                    // Success!
                    _parts.Add(partData);

                    // Set unique name for the part
                    string partName = HEU_PluginSettings.UseFullPathNamesForOutput ? GeneratePartFullName(partData.PartName) : partData.PartName;
                    partData.SetGameObjectName(partName);

                    // For intermediate or default-type editable nodes, setup the HEU_AttributeStore
                    if (isPartEditable)
                    {
                        partData.SyncAttributesStore(session, _geoInfo.nodeId, ref partInfo);
                    }
                    else
                    {
                        // Remove attributes store if it has it
                        partData.DestroyAttributesStore();
                    }
                }
            }

#if HEU_PROFILER_ON
			HEU_Logger.LogFormat("PART PROCESS TIME:: NAME={0}, TIME={1}", HEU_SessionManager.GetString(partInfo.nameSH, session), (Time.realtimeSinceStartup - processPartStartTime));
#endif
        }

        private void SetupGameObjectAndTransform(HEU_PartData partData, HEU_HoudiniAsset parentAsset)
        {
            // Checking for nulls for undo safety
            if (partData == null || parentAsset == null || parentAsset.OwnerGameObject == null || parentAsset.RootGameObject == null)
            {
                return;
            }

            // Set a valid gameobject for this part
            if (partData.OutputGameObject == null)
            {
                partData.SetGameObject(HEU_GeneralUtility.CreateNewGameObject());
            }

            // The parent is either the asset root, OR if this is instanced and not visible, then the HDA data is the parent
            // The parent transform is either the asset root (for a display node),
            // or the HDA_Data gameobject (for instanced, not visible, intermediate, editable non-display nodes)
            Transform partTransform = partData.OutputGameObject.transform;
            if (partData.IsPartInstanced()
                || (_containerObjectNode.IsInstanced() && !_containerObjectNode.IsVisible())
                || partData.IsPartCurve()
                || (IsIntermediateOrEditable() && !Displayable))
            {
                partTransform.parent = parentAsset.OwnerGameObject.transform;
            }
            else
            {
                partTransform.parent = parentAsset.RootGameObject.transform;
            }

            HEU_GeneralUtility.CopyFlags(partTransform.parent.gameObject, partData.OutputGameObject, true);

            // Reset to origin
            partTransform.localPosition = Vector3.zero;
            partTransform.localRotation = Quaternion.identity;
            partTransform.localScale = Vector3.one;

            // Destroy the children generated from ComposeNChildren
            HEU_GeneralUtility.DestroyAutoGeneratedChildren(partData.OutputGameObject);
        }

        internal void GetPartsByOutputType(List<HEU_PartData> meshParts, List<HEU_PartData> volumeParts)
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                HEU_PartData part = _parts[i];
                if (part.IsPartMesh() || part.IsPartCurve())
                {
                    meshParts.Add(part);
                }
                else if (part.IsPartVolume())
                {
                    volumeParts.Add(part);
                }
            }
        }

        /// <summary>
        /// Generate the instances of instancer parts.
        /// </summary>
        internal void GeneratePartInstances(HEU_SessionBase session)
        {
            List<HEU_PartData> partsToDestroy = new List<HEU_PartData>();
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                if (_parts[i].IsPartInstancer() && !_parts[i].HaveInstancesBeenGenerated())
                {
                    if (!_parts[i].GeneratePartInstances(session))
                    {
                        partsToDestroy.Add(_parts[i]);
                    }
                }
            }

            int numPartsToDestroy = partsToDestroy.Count;
            for (int i = 0; i < numPartsToDestroy; ++i)
            {
                HEU_GeoNode parentNode = partsToDestroy[i].ParentGeoNode;
                if (parentNode != null)
                {
                    parentNode.RemoveAndDestroyPart(partsToDestroy[i]);
                }
                else
                {
                    HEU_PartData.DestroyPart(partsToDestroy[i]);
                }
            }

            partsToDestroy.Clear();
        }

        internal void GenerateAttributesStore(HEU_SessionBase session)
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                _parts[i].GenerateAttributesStore(session);
            }
        }

        private void ProcessGeoCurve(HEU_SessionBase session)
        {
            HEU_HoudiniAsset parentAsset = ParentAsset;
            if (parentAsset == null)
            {
                return;
            }

            string curveName = GenerateGeoCurveName();
            curveName = HEU_EditorUtility.GetUniqueNameForSibling(ParentAsset.RootGameObject.transform, curveName);

            bool bNewCurve = (_geoCurve == null);
            if (bNewCurve)
            {
                // New geo curve
                _geoCurve = HEU_Curve.CreateSetupCurve(session, parentAsset, Editable, curveName, GeoID, 0, true);
            }
            else
            {
                _geoCurve.UploadParameterPreset(session, GeoID, parentAsset);
            }

            SetupGeoCurveGameObjectAndTransform(_geoCurve);
            _geoCurve.SetCurveName(curveName);

            _geoCurve.SyncFromParameters(session, parentAsset, bNewCurve);

            if (bNewCurve)
            {
                _geoCurve.DownloadAsDefaultPresetData(session);
            }

            // If geo node has part, generate the mesh using position attribute. 
            // Note that without any parts we can't generate a mesh so we pass in an invalid part ID
            // to at least set default values.
            HAPI_PartId partID = _geoInfo.partCount > 0 ? 0 : HEU_Defines.HEU_INVALID_NODE_ID;
            _geoCurve.UpdateCurve(session, partID);


            _geoCurve.GenerateMesh(_geoCurve.TargetGameObject, session);

            bool bIsVisible = IsVisible() && HEU_PluginSettings.Curves_ShowInSceneView;
        }

        private void SetupGeoCurveGameObjectAndTransform(HEU_Curve curve)
        {
            if (ParentAsset == null)
            {
                return;
            }

            if (curve.TargetGameObject == null)
            {
                curve.TargetGameObject = HEU_GeneralUtility.CreateNewGameObject();
            }

            // For geo curve, the parent is the HDA_Data
            Transform curveTransform = curve.TargetGameObject.transform;
            curveTransform.parent = ParentAsset.OwnerGameObject.transform;

            HEU_GeneralUtility.CopyFlags(curveTransform.parent.gameObject, curve.TargetGameObject, true);

            // Reset to origin
            curveTransform.localPosition = Vector3.zero;
            curveTransform.localRotation = Quaternion.identity;
            curveTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// Clear object instances so that they can be re-created
        /// </summary>
        internal void ClearObjectInstances()
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                _parts[i]._objectInstancesGenerated = false;
            }
        }

        internal void SetGeoInfo(HAPI_GeoInfo geoInfo)
        {
            _geoInfo = geoInfo;

            // Also update input node ID in case of this node
            // being recreated in new session with a different GeoID
            if (_inputNode != null)
            {
                _inputNode.SetInputNodeID(GeoID);
            }
        }

        /// <summary>
        /// Returns the part's full name to set on the gamepboject
        /// </summary>
        /// <param name="partName"></param>
        /// <returns></returns>
        internal string GeneratePartFullName(string partName)
        {
            return _containerObjectNode.ObjectName + "_" + GeoName + "_" + partName;
        }

        internal string GenerateGeoCurveName()
        {
            // For a geo curve, just use the geo node name. 
            // The curve editor presumes this is unique for multiple curves in same asset.
            return _geoName;
        }

        /// <summary>
        /// Returns true if any of the geo data has changed and 
        /// therefore requires regeneration.
        /// </summary>
        /// <returns>True if changes occurred internal to geo data</returns>
        internal bool HasGeoNodeChanged(HEU_SessionBase session)
        {
            // Note: _geoInfo.hasMaterialChanged has been deprecated so we're not checking that

            if (!session.GetGeoInfo(GeoID, ref _geoInfo))
            {
                return false;
            }
            else if (_geoInfo.type == HAPI_GeoType.HAPI_GEOTYPE_INPUT)
            {
                return (_inputNode != null) ? _inputNode.RequiresCook : false;
            }
            else if (_geoInfo.isTemplated && !HEU_PluginSettings.CookTemplatedGeos && !_geoInfo.isEditable)
            {
                return false;
            }
            else if (!_geoInfo.hasGeoChanged)
            {
                return false;
            }
            // Commented out to allow non-display geo to be updated
            //else if (!_geoInfo.isDisplayGeo && (_geoInfo.type != HAPI_GeoType.HAPI_GEOTYPE_CURVE && !HEU_PluginSettings.CookTemplatedGeos && _geoInfo.isTemplated))
            //{
            //	return false;
            //}

            return true;
        }

        /// <summary>
        /// Apply the given HAPI transform to all parts of this node.
        /// </summary>
        /// <param name="hapiTransform">HAPI transform to apply</param>
        internal void ApplyHAPITransform(ref HAPI_Transform hapiTransform)
        {
            foreach (HEU_PartData part in _parts)
            {
                part.ApplyHAPITransform(ref hapiTransform);
            }
        }

        /// <summary>
        /// Get debug info for this geo
        /// </summary>
        internal void GetDebugInfo(StringBuilder sb)
        {
            int numParts = _parts != null ? _parts.Count : 0;

            sb.AppendFormat("GeoID: {0}, Name: {1}, Type: {2}, Displayable: {3}, Editable: {4}, Parts: {5}, Parent: {6}\n", GeoID, GeoName, GeoType,
                Displayable, Editable, numParts, ParentAsset);

            if (_parts != null)
            {
                foreach (HEU_PartData part in _parts)
                {
                    part.GetDebugInfo(sb);
                }
            }
        }

        /// <summary>
        /// Returns true if this geonode is using the given material.
        /// </summary>
        /// <param name="materialData">Material data containing the material to check</param>
        /// <returns>True if this geonode is using the given material</returns>
        internal bool IsUsingMaterial(HEU_MaterialData materialData)
        {
            foreach (HEU_PartData part in _parts)
            {
                if (part.IsUsingMaterial(materialData))
                {
                    return true;
                }
            }

            return false;
        }

        internal void GetClonableParts(List<HEU_PartData> clonableParts)
        {
            foreach (HEU_PartData part in _parts)
            {
                part.GetClonableParts(clonableParts);
            }
        }


        internal bool HasAttribInstancer()
        {
            foreach (HEU_PartData part in _parts)
            {
                if (part.IsAttribInstancer())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Set attribute-based modifiers such as tag, layer, and scripts on 
        /// the part outputs.
        /// </summary>
        /// <param name="session"></param>
        internal void SetAttributeModifiersOnPartOutputs(HEU_SessionBase session)
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                if (_parts[i].OutputGameObject != null)
                {
                    HEU_GeneralUtility.AssignUnityTag(session, GeoID, _parts[i].PartID, _parts[i].OutputGameObject);
                    HEU_GeneralUtility.AssignUnityLayer(session, GeoID, _parts[i].PartID, _parts[i].OutputGameObject);
                    HEU_GeneralUtility.MakeStaticIfHasAttribute(session, GeoID, _parts[i].PartID, _parts[i].OutputGameObject);
                }
            }
        }

        /// <summary>
        /// Calculate the visibility of this geo node and its parts, based on whether the parent is visible.
        /// </summary>
        /// <param name="bParentVisibility">True if parent is visible</param>
        internal void CalculateVisiblity(bool bParentVisibility)
        {
            if (_geoCurve != null)
            {
                bool curveVisiblity = bParentVisibility && HEU_PluginSettings.Curves_ShowInSceneView;
                _geoCurve.SetCurveGeometryVisibility(curveVisiblity);
            }

            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                _parts[i].CalculateVisibility(bParentVisibility, Displayable);
            }
        }

        internal void CalculateColliderState()
        {
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                _parts[i].CalculateColliderState();
            }
        }

        internal void ProcessVolumeParts(HEU_SessionBase session, List<HEU_PartData> volumeParts, bool bRebuild)
        {
            if (ParentAsset == null)
            {
                return;
            }

            int numVolumeParts = volumeParts.Count;
            if (numVolumeParts == 0)
            {
                DestroyVolumeCache();
            }
            else if (_volumeCaches == null)
            {
                _volumeCaches = new List<HEU_VolumeCache>();
            }

            // First update volume caches. Each volume cache represents a set of terrain layers grouped by tile index.
            // Therefore each volume cache represents a potential Unity Terrain (containing layers)
            _volumeCaches = HEU_VolumeCache.UpdateVolumeCachesFromParts(session, this, volumeParts, _volumeCaches);

            // Heightfield scatter nodes come in as mesh-type parts with attribute instancing.
            // So process them here to get all the tree/detail instance scatter information.
            int numParts = _parts.Count;
            for (int i = 0; i < numParts; ++i)
            {
                // Find the terrain tile (use primitive attr). Assume 0 tile if not set (i.e. not split into tiles)
                int terrainTile = 0;
                HEU_TerrainUtility.GetAttributeTile(session, GeoID, _parts[i].PartID, out terrainTile);

                // Find the volumecache associated with this part using the terrain tile index
                HEU_VolumeCache volumeCache = GetVolumeCacheByTileIndex(terrainTile);
                if (volumeCache == null)
                {
                    continue;
                }

                HEU_VolumeLayer volumeLayer = volumeCache.GetLayer(_parts[i].GetVolumeLayerName());
                if (volumeLayer != null && volumeLayer._layerType == HFLayerType.DETAIL)
                {
                    // Clear out outputs since it might have been created when the part was created.
                    _parts[i].DestroyAllData();

                    volumeCache.PopulateDetailPrototype(session, GeoID, _parts[i].PartID, volumeLayer);
                }
                else if (_parts[i].IsAttribInstancer())
                {
                    HAPI_AttributeInfo treeInstAttrInfo = new HAPI_AttributeInfo();
                    if (HEU_GeneralUtility.GetAttributeInfo(session, GeoID, _parts[i].PartID, HEU_Defines.HEIGHTFIELD_TREEINSTANCE_PROTOTYPEINDEX,
                            ref treeInstAttrInfo))
                    {
                        if (treeInstAttrInfo.exists && treeInstAttrInfo.count > 0)
                        {
                            // Clear out outputs since it might have been created when the part was created.
                            _parts[i].DestroyAllData();

                            // Mark the instancers as having been created so that the object instancer step skips this.
                            _parts[i]._objectInstancesGenerated = true;

                            bool throwWarningIfNoTileAttribute = _volumeCaches.Count > 1;

                            // Now populate scatter trees based on attributes on this part
                            // Note: Might do redudant work for each volume and might need refactoring for performance.
                            foreach (HEU_VolumeCache cache in _volumeCaches)
                            {
                                cache.PopulateScatterTrees(session, GeoID, _parts[i].PartID, treeInstAttrInfo.count, throwWarningIfNoTileAttribute);
                            }
                        }
                    }
                }
            }

            // Now generate the terrain for each volume cache
            foreach (HEU_VolumeCache cache in _volumeCaches)
            {
                cache.GenerateTerrainWithAlphamaps(session, ParentAsset, bRebuild);

                cache.IsDirty = false;
            }
        }

        internal void DestroyVolumeCache()
        {
            if (_volumeCaches != null)
            {
                int numCaches = _volumeCaches.Count;
                for (int i = 0; i < numCaches; ++i)
                {
                    if (_volumeCaches[i] != null)
                    {
                        if (ParentAsset)
                            ParentAsset.RemoveVolumeCache(_volumeCaches[i]);
                        HEU_GeneralUtility.DestroyImmediate(_volumeCaches[i]);
                        _volumeCaches[i] = null;
                    }
                }

                _volumeCaches = null;
            }
        }

        public override string ToString()
        {
            return (!string.IsNullOrEmpty(_geoName) ? ("GeoNode: " + _geoName) : base.ToString());
        }

        public bool IsEquivalentTo(HEU_GeoNode other)
        {
            bool bResult = true;

            string header = "HEU_GeoNode";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._geoInfo.ToTestObject(), other._geoInfo.ToTestObject(), ref bResult, header, "_geoInfo");

            //HEU_TestHelpers.AssertTrueLogEquivalent(this._geoName, other._geoName, ref bResult, header, "_geoName");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._parts, other._parts, ref bResult, header, "_part");

            // SKip _containerObjectNode/parentAsset stuff

            HEU_TestHelpers.AssertTrueLogEquivalent(this._geoCurve, other._geoCurve, ref bResult, header, "_geoCurve");

            // Skip volumCache

            HEU_TestHelpers.AssertTrueLogEquivalent(this._volumeCaches, other._volumeCaches, ref bResult, header, "_volumeCaches");


            return bResult;
        }
    }
} // HoudiniEngineUnity
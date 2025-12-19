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
using System.Text;
using System;
using System.Linq;

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
    using HAPI_ParmId = System.Int32;
    using HAPI_StringHandle = System.Int32;

    /// <summary>
    /// A class representing a curve CV
    /// </summary>
    [System.Serializable]
    public class CurveNodeData : IEquivable<CurveNodeData>
    {
        [SerializeField] public Vector3 position = Vector3.zero;
        [SerializeField] public Vector3 rotation = Vector3.zero;
        [SerializeField] public Vector3 scale = Vector3.one;

        // The index of the curve that this node belongs to
        [SerializeField] [HideInInspector] public int curveCountIndex = 0;

        public CurveNodeData()
        {
        }

        public CurveNodeData(Vector3 position)
        {
            this.position = position;
        }

        public CurveNodeData(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation.eulerAngles;
        }

        public CurveNodeData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation.eulerAngles;
            this.scale = scale;
        }

        public CurveNodeData(CurveNodeData other)
        {
            this.position = other.position;
            this.rotation = other.rotation;
            this.scale = other.scale;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(this.rotation);
        }

        public bool IsEquivalentTo(CurveNodeData other)
        {
            bool bResult = true;

            string header = "CurveNodeData";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this.position, other.position, ref bResult, header, "position");
            HEU_TestHelpers.AssertTrueLogEquivalent(this.rotation, other.rotation, ref bResult, header, "rotation");
            HEU_TestHelpers.AssertTrueLogEquivalent(this.scale, other.scale, ref bResult, header, "scale");

            return bResult;
        }
    };


    // Mimics HAPI_InputCurveInfo, but serializes fields for UX
    [System.Serializable]
    public class HEU_InputCurveInfo
    {
        public HAPI_CurveType curveType = HAPI_CurveType.HAPI_CURVETYPE_LINEAR;
        [Range(2, 11)] public int order = 2;
        public bool closed = false;
        public bool reverse = false;
        public HAPI_InputCurveMethod inputMethod;

        public HAPI_InputCurveParameterization breakpointParameterization;

        public static HEU_InputCurveInfo CreateFromHAPI_InputCurveInfo(HAPI_InputCurveInfo curveInfo)
        {
            HEU_InputCurveInfo inputCurveInfo = new HEU_InputCurveInfo();

            inputCurveInfo.curveType = curveInfo.curveType;
            inputCurveInfo.order = curveInfo.order;
            inputCurveInfo.closed = curveInfo.closed;
            inputCurveInfo.reverse = curveInfo.reverse;
            inputCurveInfo.inputMethod = curveInfo.inputMethod;
            inputCurveInfo.breakpointParameterization = curveInfo.breakpointParameterization;
            return inputCurveInfo;
        }

        public static string[] GetCurveTypeNames()
        {
            return new string[] { "Linear", "NURBS", "Bezier" };
        }

        public static string[] GetInputMethodNames()
        {
            return new string[] { "CVs", "Breakpoints" };
        }

        public static string[] GetBreakpointParameterizationNames()
        {
            return new string[] { "Uniform", "Chord", "Centripetal" };
        }
    }

    [System.Serializable]
    public enum HEU_CurveDataType
    {
        INVALID = 0,
        GEO_COORDS_PARAM = 1, // i.e. curve::1.0
        HAPI_COORDS_PARAM = 2,
        POSITION_ATTRIBUTE = 3
    };

    /// <summary>
    /// Contains data and logic for curve node drawing and editing.
    /// </summary>
    public class HEU_Curve : ScriptableObject, IHEU_Curve, IHEU_HoudiniAssetSubcomponent, IEquivable<HEU_Curve>
    {
        // PUBLIC FIELDS ==============================================================================

        /// <inheritdoc />
        public GameObject TargetGameObject
        {
            get => _targetGameObject;
            set => _targetGameObject = value;
        }

        /// <inheritdoc />
        public HAPI_NodeId GeoID => _geoID;

        /// <inheritdoc />
        public HAPI_NodeId PartID => _partID;

        /// <inheritdoc />
        public List<CurveNodeData> CurveNodeData => _curveNodeData;

        /// <inheritdoc />
        public HEU_Parameters Parameters => _parameters;

        /// <inheritdoc />
        public string CurveName => _curveName;

        /// <inheritdoc />
        public bool IsInputCurve => _bIsInputCurve;

        /// <inheritdoc />
        public bool IsPartCurve => _bIsPartCurve;

        /// <inheritdoc />
        public HEU_InputCurveInfo InputCurveInfo => _inputCurveInfo;


        // =====================================================================================

        // DATA -------------------------------------------------------------------------------------------------------

        [System.NonSerialized] private HAPI_NodeId _geoID = HEU_Defines.HEU_INVALID_NODE_ID;
        [System.NonSerialized] private HAPI_NodeId _partID;


        [SerializeField] private List<CurveNodeData> _curveNodeData = new List<CurveNodeData>();


        [SerializeField] private Vector3[] _vertices;
        [SerializeField] private bool _isEditable;
        [SerializeField] private HEU_Parameters _parameters;
        [SerializeField] private bool _bUploadParameterPreset;

        internal void SetUploadParameterPreset(bool bValue)
        {
            _bUploadParameterPreset = bValue;
        }

        [SerializeField] private string _curveName;
        [SerializeField] private GameObject _targetGameObject;
        [SerializeField] private bool _isGeoCurve;


        public enum CurveEditState
        {
            INVALID,
            GENERATED,
            EDITING,
            REQUIRES_GENERATION
        }

        [SerializeField] private CurveEditState _editState;

        public CurveEditState EditState => _editState;

        // Types of interaction with this curve. Used by Editor.
        public enum Interaction
        {
            VIEW,
            ADD,
            EDIT
        }

        // Preferred interaction mode when this a curve selected. Allows for quick access for curve editing.
        public static Interaction PreferredNextInteractionMode = Interaction.VIEW;

        internal enum CurveDrawCollision
        {
            COLLIDERS,
            LAYERMASK
        }

        [SerializeField] private HEU_HoudiniAsset _parentAsset;

        public HEU_HoudiniAsset ParentAsset => _parentAsset;

        // Whether or not this curve is a part generated from an input part (from another HDA)
        [SerializeField] private bool _bIsInputCurve = false;

        // Variables dealing with part curves

        [SerializeField] private bool _bIsPartCurve = true;
        [SerializeField] private bool _cachedCurveInfoValid = false;
        [SerializeField] private int[] _cachedCurveCounts = null;


        // Doesn't need to be serialized, but adding here for convinence
        private int[] _cachedCurveCountSums = null;

        [SerializeField] private HEU_CurveDataType _curveDataType = HEU_CurveDataType.INVALID;

        public HEU_CurveDataType CurveDataType => _curveDataType;

        // Stored curve settings to mimic HAPI_CurveInfo
        // Only used if it is not a geo curve / is not a legacy curve
        [SerializeField] private HEU_InputCurveInfo _inputCurveInfo = null;


        // PUBLIC FUNCTIONS ====================================================================

        /// <inheritdoc />
        public HEU_SessionBase GetSession()
        {
            if (_parentAsset != null)
            {
                return _parentAsset.GetAssetSession(true);
            }
            else
            {
                return HEU_SessionManager.GetOrCreateDefaultSession();
            }
        }

        /// <inheritdoc />
        public void Recook()
        {
            SetEditState(CurveEditState.REQUIRES_GENERATION);

            if (_parentAsset != null)
            {
                _parentAsset.RequestCook();
            }
        }

        public void Rebuild()
        {
            SetEditState(CurveEditState.INVALID);

            if (_parentAsset != null)
            {
                _parentAsset.RequestReload();
            }
        }

        /// <inheritdoc />
        public bool IsEditable()
        {
            return _isEditable;
        }

        /// <inheritdoc />
        public bool IsGeoCurve()
        {
            return _isGeoCurve;
        }

        /// <inheritdoc />
        public void SetCurveName(string name)
        {
            _curveName = name;
            if (_targetGameObject != null)
            {
                HEU_GeneralUtility.RenameGameObject(_targetGameObject, name);
            }
        }

        /// <inheritdoc />
        public void SetCurvePoint(int pointIndex, Vector3 newPosition, bool bRecookAsset = false)
        {
            if (pointIndex >= 0 && pointIndex < _curveNodeData.Count)
            {
                _curveNodeData[pointIndex].position = newPosition;
            }

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void SetCurvePoint(int pointIndex, CurveNodeData curveData, bool bRecookAsset = false)
        {
            if (pointIndex >= 0 && pointIndex < _curveNodeData.Count)
            {
                _curveNodeData[pointIndex].position = curveData.position;
                _curveNodeData[pointIndex].rotation = curveData.rotation;
                _curveNodeData[pointIndex].scale = curveData.scale;
            }

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void SetCurveNodeData(List<CurveNodeData> curveNodeData, bool bRecookAsset = false)
        {
            _curveNodeData = curveNodeData;

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public Vector3 GetCurvePoint(int pointIndex)
        {
            if (pointIndex >= 0 && pointIndex < _curveNodeData.Count)
            {
                return _curveNodeData[pointIndex].position;
            }

            return Vector3.zero;
        }

        /// <inheritdoc />
        public List<CurveNodeData> GetAllPointTransforms()
        {
            return _curveNodeData;
        }

        /// <inheritdoc />
        public List<Vector3> GetAllPoints()
        {
            List<Vector3> points = new List<Vector3>();

            _curveNodeData.ForEach((CurveNodeData transform) => points.Add(transform.position));

            return points;
        }

        /// <inheritdoc />
        public int GetNumPoints()
        {
            return _curveNodeData.Count;
        }

        /// <inheritdoc />
        public void InsertCurvePoint(int index, Vector3 position, bool bRecookAsset = false)
        {
            _curveNodeData.Insert(index, new CurveNodeData(position));

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void InsertCurvePoint(int index, CurveNodeData curveData, bool bRecookAsset = false)
        {
            _curveNodeData.Insert(index, curveData);

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void AddCurvePointToEnd(Vector3 position, bool bRecookAsset = false)
        {
            _curveNodeData.Add(new CurveNodeData(position));

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void AddCurvePointToEnd(CurveNodeData curveData, bool bRecookAsset = false)
        {
            _curveNodeData.Add(curveData);

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void RemoveCurvePoint(int pointIndex, bool bRecookAsset = false)
        {
            _curveNodeData.RemoveAt(pointIndex);

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void ClearCurveNodeData(bool bRecookAsset = false)
        {
            _curveNodeData.Clear();

            if (bRecookAsset && _parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public void ProjectToColliders(Vector3 rayDirection, float rayDistance, bool bRecookAsset = false)
        {
            ProjectToCollidersInternal(_parentAsset, rayDirection, rayDistance);

            if (bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void SetCurveGeometryVisibility(bool bVisible, bool bRecookAsset = false)
        {
            SetCurveGeometryVisibilityInternal(bVisible);

            if (bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public Vector3 GetTransformedPoint(int pointIndex)
        {
            if (pointIndex >= 0 && pointIndex < _curveNodeData.Count)
            {
                return GetTransformedPosition(_curveNodeData[pointIndex].position);
            }

            return Vector3.zero;
        }

        /// <inheritdoc />
        public List<Vector3> GetTransformedPoints()
        {
            List<Vector3> transformedPoints = new List<Vector3>();

            for (int i = 0; i < _curveNodeData.Count; i++)
            {
                transformedPoints.Add(GetTransformedPosition(_curveNodeData[i].position));
            }

            return transformedPoints;
        }

        /// <inheritdoc />
        public List<CurveNodeData> DuplicateCurveNodeData()
        {
            List<CurveNodeData> curveNodes = new List<CurveNodeData>();
            foreach (CurveNodeData curveData in _curveNodeData)
            {
                curveNodes.Add(new CurveNodeData(curveData));
            }

            return curveNodes;
        }

        // =====================================================================================

        // LOGIC ------------------------------------------------------------------------------------------------------

        internal static HEU_Curve CreateSetupCurve(HEU_SessionBase session, HEU_HoudiniAsset parentAsset,
            bool isEditable, string curveName, HAPI_NodeId geoID, HAPI_PartId partID, bool bGeoCurve)
        {
            HEU_Curve newCurve = ScriptableObject.CreateInstance<HEU_Curve>();
            newCurve._isEditable = isEditable;
            newCurve._curveName = curveName;
            newCurve._geoID = geoID;
            newCurve._partID = partID;

            newCurve.SetEditState(CurveEditState.INVALID);
            newCurve._isGeoCurve = bGeoCurve;
            newCurve._parentAsset = parentAsset;

            // Detect whether or not this is an input node - should delete to remove duplicates
            string parmName = HEU_Defines.HAPI_OBJPATH_1_PARAM;

            int parmId = -1;
            if (session.GetParmIDFromName(geoID, parmName, out parmId) && parmId != -1)
            {
                HAPI_NodeId nodeId = -1;
                if (session.GetParamNodeValue(geoID, parmName, out nodeId))
                {
                    foreach (HEU_InputNode input in parentAsset.GetInputNodes())
                    {
                        List<HEU_InputHDAInfo> assetInfos = input.InputAssetInfos;
                        foreach (HEU_InputHDAInfo info in assetInfos)
                        {
                            HAPI_AssetInfo assetInfo2 = new HAPI_AssetInfo();
                            session.GetAssetInfo(info._connectedMergeNodeID, ref assetInfo2);

                            if (assetInfo2.objectNodeId == nodeId)
                            {
                                newCurve._bIsInputCurve = true;
                                break;
                            }
                        }

                        if (newCurve._bIsInputCurve)
                            break;
                    }
                }
            }

            newCurve._curveDataType = newCurve.GetCurveDataType(session);

            if (partID != HEU_Defines.HEU_INVALID_NODE_ID &&
                newCurve._curveDataType != HEU_CurveDataType.GEO_COORDS_PARAM)
            {
                HAPI_PartInfo partInfo = new HAPI_PartInfo();
                session.GetPartInfo(geoID, partID, ref partInfo);
                newCurve._bIsPartCurve = (partInfo.type == HAPI_PartType.HAPI_PARTTYPE_CURVE);
                if (newCurve._bIsPartCurve)
                {
                    newCurve.UpdateCachedCurveInfo(session, true);
                }

                if (newCurve._curveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM)
                {
                    newCurve._inputCurveInfo = new HEU_InputCurveInfo();
                }
            }

            if (parentAsset.SerializedMetaData != null && parentAsset.SerializedMetaData.SavedCurveNodeData != null &&
                parentAsset.SerializedMetaData.SavedCurveNodeData.ContainsKey(curveName))
            {
                newCurve.UsePreviousCurveData(curveName);
            }
            else if (newCurve._curveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM &&
                     parentAsset.SerializedMetaData != null &&
                     parentAsset.SerializedMetaData.SavedCurveNodeData != null &&
                     parentAsset.SerializedMetaData.SavedCurveNodeData.Count > 0)
            {
                // Kind of hacky, but ...
                // ... if it is a HAPI input curve, then always get the first one if it exists
                List<string> keys = parentAsset.SerializedMetaData.SavedCurveNodeData.Keys.ToList();
                if (keys.Count > 0)
                {
                    newCurve.UsePreviousCurveData(keys[0]);
                }
            }

            parentAsset.AddCurve(newCurve);
            return newCurve;
        }

        // Use previous curve data (often after rebuild)
        private void UsePreviousCurveData(string curveName)
        {
            if (_parentAsset == null || _parentAsset.SerializedMetaData == null ||
                _parentAsset.SerializedMetaData.SavedCurveNodeData == null
                || !_parentAsset.SerializedMetaData.SavedCurveNodeData.ContainsKey(curveName))
            {
                return;
            }

            _curveNodeData = _parentAsset.SerializedMetaData.SavedCurveNodeData[curveName];
            _parentAsset.SerializedMetaData.SavedCurveNodeData.Remove(curveName);

            if (_parentAsset.SerializedMetaData.SavedInputCurveInfo != null &&
                _parentAsset.SerializedMetaData.SavedInputCurveInfo.ContainsKey(curveName) &&
                _curveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM)
            {
                _inputCurveInfo = _parentAsset.SerializedMetaData.SavedInputCurveInfo[curveName];
                _parentAsset.SerializedMetaData.SavedInputCurveInfo.Remove(curveName);
            }
        }

        // Determine whether or not we are using curve::2.0, input curves, or non-editable curves
        private HEU_CurveDataType GetCurveDataType(HEU_SessionBase session)
        {
            // Determine if it is a legacy curve (curve::1.0)
            // If it is NOT a legacy curve, then read it as a curve part.
            // curve::2.0 is not supported right now
            HAPI_ParmId parmID = -1;

            if (session.GetParmIDFromName(_geoID, HEU_Defines.CURVE_COORDS_PARAM, out parmID) && parmID != -1)
            {
                return HEU_CurveDataType.GEO_COORDS_PARAM;
            }
            else if (HEU_GeneralUtility.HasAttribute(session, _geoID, _partID,
                         HEU_HAPIConstants.HAPI_ATTRIB_INPUT_CURVE_COORDS, HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL))
            {
                return HEU_CurveDataType.HAPI_COORDS_PARAM;
            }
            else
            {
                return HEU_CurveDataType.POSITION_ATTRIBUTE;
            }
        }

        // Whether or not we should keep thie HEU_Curve as an output when generating mesh
        internal bool ShouldKeepNode(HEU_SessionBase session)
        {
            // We only need to keep information about geo curves at the moment.
            // Note: This will change in the future when we use parts as input

            if (_curveDataType == HEU_CurveDataType.GEO_COORDS_PARAM && _curveNodeData.Count <= 0)
            {
                return false;
            }

            if (_bIsInputCurve)
            {
                return false;
            }

            return true;
        }

        // Called on destroy
        internal void DestroyAllData(bool bIsRebuild = false)
        {
            if (_parameters != null)
            {
                _parameters.CleanUp();
                _parameters = null;
            }

            if (_isGeoCurve && _targetGameObject != null)
            {
                HEU_HAPIUtility.DestroyGameObject(_targetGameObject);
                _targetGameObject = null;
            }

            if (bIsRebuild && _parentAsset != null && _parentAsset.SerializedMetaData.SavedCurveNodeData != null)
            {
                _parentAsset.SerializedMetaData.SavedCurveNodeData.AddOrSet(_curveName, _curveNodeData);

                if (_inputCurveInfo != null)
                    _parentAsset.SerializedMetaData.SavedInputCurveInfo.AddOrSet(_curveName, _inputCurveInfo);
            }

            if (_targetGameObject != null && _curveDataType != HEU_CurveDataType.GEO_COORDS_PARAM)
            {
                for (int i = _targetGameObject.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject.DestroyImmediate(_targetGameObject.transform.GetChild(i).gameObject);
                }
            }
        }


        // Upload parameter preset to Houdini
        internal void UploadParameterPreset(HEU_SessionBase session, HAPI_NodeId geoID, HEU_HoudiniAsset parentAsset)
        {
            // TODO FIXME
            // This fixes up the geo IDs for curves, and upload parameter values to Houdini.
            // This is required for curves in saved scenes, as its parameter data is not part of the parent asset's
            // parameter preset. Also the _geoID and parameters._nodeID could be different so uploading the
            // parameter values before cooking would not be valid for those IDs. This waits until after cooking
            // to then upload and cook just the curve.
            // Admittedly this is a temporary solution until a proper workaround is in place. Ideally for an asset reload
            // the object node and geo node names can be used to match up the IDs and then parameter upload can happen
            // before cooking.

            _geoID = geoID;

            if (_parameters != null)
            {
                _parameters.NodeID = geoID;

                if (_bUploadParameterPreset)
                {
                    _parameters.UploadPresetData(session);
                    _parameters.UploadValuesToHoudini(session, parentAsset);

                    HEU_HAPIUtility.CookNodeInHoudini(session, geoID, false, _curveName);

                    _bUploadParameterPreset = false;
                }
            }

            OnPresyncParameters(session, parentAsset);
        }

        // Resets curve parameters and preset data
        internal void ResetCurveParameters(HEU_SessionBase session, HEU_HoudiniAsset parentAsset)
        {
            if (_parameters != null)
            {
                _parameters.ResetAllToDefault(session);

                // Force an upload here so that when the parent asset recooks, it will have updated parameter values.
                _parameters.UploadPresetData(session);
                _parameters.UploadValuesToHoudini(session, parentAsset);
            }
        }

        // Set curve parameter preset
        internal void SetCurveParameterPreset(HEU_SessionBase session, HEU_HoudiniAsset parentAsset,
            byte[] parameterPreset)
        {
            if (_parameters != null)
            {
                _parameters.SetPresetData(parameterPreset);

                // Force an upload here so that when the parent asset recooks, it will have updated parameter values.
                _parameters.UploadPresetData(session);
                _parameters.UploadValuesToHoudini(session, parentAsset);
            }
        }

        // Updates the curve guides by getting the P attribute
        internal void UpdateCurve(HEU_SessionBase session, HAPI_PartId partId)
        {
            int vertexCount = 0;
            float[] posAttr = new float[0];

            if (partId != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                // Get position attributes.
                // Note that for an empty curve (ie. no position attributes) this query will fail, 
                // but the curve is still valid, so we simply set to null vertices. This allows 
                // user to add points later on.
                HAPI_AttributeInfo posAttrInfo = new HAPI_AttributeInfo();
                HEU_GeneralUtility.GetAttribute(session, _geoID, partId, HEU_HAPIConstants.HAPI_ATTRIB_POSITION,
                    ref posAttrInfo, ref posAttr, session.GetAttributeFloatData);
                if (posAttrInfo.exists)
                {
                    vertexCount = posAttrInfo.count;
                }
            }

            // Curve guides from position attributes
            _vertices = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; ++i)
            {
                HEU_HAPIUtility.ConvertPositionUnityToHoudini(posAttr[i * 3 + 0], posAttr[i * 3 + 1],
                    posAttr[i * 3 + 2], ref _vertices[i]);
            }
        }

        // Helper to get curve counts (for curves with multiple lines)
        private static int[] GetCurveCounts(HEU_SessionBase session, HAPI_NodeId geoId, HAPI_PartId partID)
        {
            if (IsMeshCurve(session, geoId, partID))
            {
                return null;
            }

            HAPI_CurveInfo curveInfo = new HAPI_CurveInfo();
            if (!session.GetCurveInfo(geoId, partID, ref curveInfo))
            {
                return null;
            }

            int[] curveCounts = new int[curveInfo.curveCount];

            if (!session.GetCurveCounts(geoId, partID, curveCounts, 0, curveInfo.curveCount))
            {
                return null;
            }

            int totalCurveCounts = 0;

            foreach (int count in curveCounts)
            {
                totalCurveCounts += count;
            }

            HAPI_AttributeInfo pointAttributeInfo = new HAPI_AttributeInfo();
            if (session.GetAttributeInfo(geoId, partID, HEU_HAPIConstants.HAPI_ATTRIB_POSITION,
                    HAPI_AttributeOwner.HAPI_ATTROWNER_POINT, ref pointAttributeInfo))
            {
                bool bResult = totalCurveCounts > 0 && totalCurveCounts == pointAttributeInfo.count;
                if (!bResult)
                {
                    // Fallback on old style
                    curveCounts = new int[1] { pointAttributeInfo.count };
                }
            }
            else
            {
                // Should not happen. Error out!
                curveCounts = null;
            }

            return curveCounts;
        }

        // Generates the curve helper mesh
        internal void GenerateMesh(GameObject inGameObject, HEU_SessionBase session)
        {
            _targetGameObject = inGameObject;

            List<GameObject> childGameObjects = new List<GameObject>();

            int[] curveCounts = null;

            bool useCurveCounts = false;
            // If ccurve node data <= 1, mark as generated
            if (_curveNodeData.Count <= 1)
            {
                SetEditState(CurveEditState.GENERATED);
                return;
            }

            // If more than one curve count, then compose one object for each child, and then mark "useCurveCounts"
            if (_curveDataType != HEU_CurveDataType.GEO_COORDS_PARAM && !IsMeshCurve(session, _geoID, _partID))
            {
                curveCounts = GetCurveCounts(session, _geoID, _partID);
                if (curveCounts != null && curveCounts.Length > 1)
                {
                    HEU_GeneralUtility.ComposeNChildren(_targetGameObject, curveCounts.Length, ref childGameObjects,
                        true);
                    useCurveCounts = true;
                }
                else
                {
                    childGameObjects.Add(_targetGameObject);
                }
            }
            else
            {
                childGameObjects.Add(_targetGameObject);
            }

            // Add the vertices to the vertex list
            List<Vector3[]> vertexList = new List<Vector3[]>();
            if (!useCurveCounts)
            {
                vertexList.Add(_vertices);
            }
            else
            {
                // Destroy the meshfilter/component on the parent as it interferes with the children
                MeshFilter meshFilter = _targetGameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    DestroyImmediate(meshFilter);
                }

                MeshRenderer meshRenderer = _targetGameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    DestroyImmediate(meshRenderer);
                }

                // Iterate through the vertex list for each curve
                int startingIndex = 0;
                for (int i = 0; i < curveCounts.Length; i++)
                {
                    int curveCount = curveCounts[i];
                    Vector3[] newVertexList = new Vector3[curveCount];
                    for (int j = 0; j < curveCount; j++)
                    {
                        newVertexList[j] = _vertices[startingIndex + j];
                    }

                    startingIndex += curveCount;

                    vertexList.Add(newVertexList);
                }
            }


            // For each object, generate the mesh
            for (int i = 0; i < childGameObjects.Count; i++)
            {
                GenerateMeshForSingleObject(childGameObjects[i], vertexList[i]);
            }

            // Set to generated
            SetEditState(CurveEditState.GENERATED);
        }

        // Generates the curve display mesh using targetObject, with the given vertexList
        internal void GenerateMeshForSingleObject(GameObject targetObject, Vector3[] vertexList)
        {
            // Get Unity components
            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = targetObject.AddComponent<MeshFilter>();
            }

            MeshRenderer meshRenderer = targetObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = targetObject.AddComponent<MeshRenderer>();
            }

            // Attach the line shader and set the color.
            Shader shader = HEU_MaterialFactory.FindPluginShader(HEU_PluginSettings.DefaultCurveShader);
            meshRenderer.sharedMaterial = new Material(shader);
            meshRenderer.sharedMaterial.SetColor("_Color", HEU_PluginSettings.LineColor);

            Mesh mesh = null;
            // For some reason, attempting to reuse shared mesh results in an error for curve::2.0
            if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.isReadable)
            {
                mesh = meshFilter.sharedMesh;
            }

            // Upload mesh data
            if (_curveNodeData.Count <= 1)
            {
                if (mesh != null)
                {
                    mesh.Clear();
                    mesh = null;
                }
            }
            else
            {
                if (mesh == null)
                {
                    mesh = new Mesh();
                    mesh.name = "Curve";
                }

                int[] indices = new int[vertexList.Length];
                for (int i = 0; i < vertexList.Length; ++i)
                {
                    indices[i] = i;
                }

                mesh.Clear();
                mesh.vertices = vertexList;
                mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
                mesh.RecalculateBounds();

                mesh.UploadMeshData(false);
            }

            meshFilter.sharedMesh = mesh;
            meshRenderer.enabled = HEU_PluginSettings.Curves_ShowInSceneView;
        }

        // Upload data before syncing
        // Does work regarding rot/scale, or input curves here
        internal void OnPresyncParameters(HEU_SessionBase session, HEU_HoudiniAsset parentAsset)
        {
            if (!_isEditable)
            {
                return;
            }

            if (_curveDataType == HEU_CurveDataType.GEO_COORDS_PARAM)
            {
                UpdateCurveInputForCustomAttributes(session, parentAsset);
            }
            else if (_curveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM)
            {
                UpdateCurveInputForCurveParts(session, parentAsset);
            }
        }


        // Get order abiding by the curve rules as mentioned in GT_PrimCurveMesh::setBasis
        internal static int GetOrderForCurveType(int requestedOrder, HAPI_CurveType curveType)
        {
            switch (curveType)
            {
                case HAPI_CurveType.HAPI_CURVETYPE_LINEAR:
                    return 2;
                case HAPI_CurveType.HAPI_CURVETYPE_NURBS:
                    return Mathf.Clamp(requestedOrder, 2, 128);
                case HAPI_CurveType.HAPI_CURVETYPE_BEZIER:
                    return Mathf.Clamp(requestedOrder, 2, 31);
                default:
                    return Mathf.Clamp(requestedOrder, 4, 128);
            }
        }

        // Updates the curve data for input curves (for input curves only)
        internal bool UpdateCurveInputForCurveParts(HEU_SessionBase session, HEU_HoudiniAsset parentAsset)
        {
            // Re-create the curve attributes from scratch in order to modify the curve/rotation values
            // Additional CVs may be added or removed so we need to recreate the translation/rotation/scale lists
            // i.e. It is not guaranteed that positions.Count == rotations.Count == scales.Count so we have to do this
            bool hasRotations = !parentAsset.CurveDisableScaleRotation;
            bool hasScales = !parentAsset.CurveDisableScaleRotation;

            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            List<Vector3> scales = new List<Vector3>();
            List<int> curveCountIndices = new List<int>();

            _curveNodeData.ForEach((CurveNodeData data) =>
            {
                positions.Add(data.position);

                if (hasRotations)
                {
                    rotations.Add(data.GetRotation());
                }

                if (hasScales)
                {
                    scales.Add(data.scale);
                }

                curveCountIndices.Add(data.curveCountIndex);
            });

            // Probably not necessary, but just in case
            hasRotations &= rotations.Count == positions.Count;
            hasScales &= scales.Count == positions.Count;

            float[] posArr = new float[positions.Count * 3];
            float[] rotArr = new float[positions.Count * 4];
            float[] scaleArr = new float[positions.Count * 3];
            for (int i = 0; i < positions.Count; i++)
            {
                HEU_HAPIUtility.ConvertPositionUnityToHoudini(positions[i], out posArr[i * 3 + 0],
                    out posArr[i * 3 + 1], out posArr[i * 3 + 2]);

                if (hasRotations)
                {
                    HEU_HAPIUtility.ConvertRotationUnityToHoudini(rotations[i], out rotArr[i * 4 + 0],
                        out rotArr[i * 4 + 1], out rotArr[i * 4 + 2], out rotArr[i * 4 + 3]);
                }

                if (hasScales)
                {
                    HEU_HAPIUtility.ConvertScaleUnityToHoudini(scales[i], out scaleArr[i * 3 + 0],
                        out scaleArr[i * 3 + 1], out scaleArr[i * 3 + 2]);
                }
            }

            HAPI_InputCurveInfo inputCurveInfo = new HAPI_InputCurveInfo();
            inputCurveInfo.FillData(_inputCurveInfo);

            session.SetInputCurveInfo(_geoID, _partID, ref inputCurveInfo);

            if (_curveNodeData.Count > 0)
            {
                if (!hasRotations && !hasScales)
                {
                    session.SetInputCurvePositions(_geoID, _partID, posArr, 0, posArr.Length);
                }
                else
                {
                    session.SetInputCurvePositionsRotationsScales(_geoID, _partID, posArr, 0, posArr.Length, rotArr, 0,
                        rotArr.Length, scaleArr, 0, scaleArr.Length);
                }
            }

            return true;
        }

        // Updates the curve data for rot/scale (for curve::1.0 only)
        internal bool UpdateCurveInputForCustomAttributes(HEU_SessionBase session, HEU_HoudiniAsset parentAsset)
        {
            // Stop now just to be safe (Everything will be done Houdini-side) and we just fetch from there
            // If I add the option to add custom attributes, this might be moved to one level up in the future.
            if (parentAsset.CurveDisableScaleRotation)
            {
                session.RevertGeo(GeoID);
                return true;
            }

            // Curve code mostly copied from Unreal-v2s FHoudiniSplineTranslator::HapiCreateCurveInputNodeForData

            // In order to be able to add rotations and scale attributes to the curve SOP, we need to cook it twice:
            // 
            // - First, we send the positions string to it, and cook it without refinement.
            //   this will allow us to get the proper curve CVs, part attributes and curve info to create the desired curve.
            //
            // - We then need to send back all the info extracted from the curve SOP to it, and add the rotation 
            //   and scale attributes to it. This will lock the curve SOP, and prevent the curve type and method 
            //   parameters from functioning properly (hence why we needed the first cook to set that up)

            int numberOfCVs = _curveNodeData.Count;

            if (numberOfCVs >= 2)
            {
                // Re-create the curve attributes from scratch in order to modify the curve/rotation values
                // Additional CVs may be added or removed so we need to recreate the translation/rotation/scale lists
                // i.e. It is not guaranteed that positions.Count == rotations.Count == scales.Count so we have to do this
                List<Vector3> positions = new List<Vector3>();
                List<Quaternion> rotations = new List<Quaternion>();
                List<Vector3> scales = new List<Vector3>();

                _curveNodeData.ForEach((CurveNodeData data) =>
                {
                    positions.Add(data.position);
                    rotations.Add(data.GetRotation());
                    scales.Add(data.scale);
                });

                const string warningMessage = "\nRotation/Scale may not work properly.";

                if (!session.RevertGeo(GeoID))
                {
                    HEU_Logger.LogWarning("Unable to revert Geo!" + warningMessage);
                    return false;
                }

                HAPI_NodeId curveIdNode = GeoID;

                // Set the type, method, close, and reverse parameters
                HEU_ParameterData typeParameter = _parameters.GetParameter(HEU_Defines.CURVE_TYPE_PARAM);
                int curveTypeValue = typeParameter._intValues[0];
                if (!session.SetParamIntValue(curveIdNode, HEU_Defines.CURVE_TYPE_PARAM, 0, curveTypeValue))
                {
                    HEU_Logger.LogWarning("Unable to get 'type' parameter" + warningMessage);
                    return false;
                }

                HEU_ParameterData methodParameter = _parameters.GetParameter(HEU_Defines.CURVE_METHOD_PARAM);
                int curveMethodValue = methodParameter._intValues[0];
                if (!session.SetParamIntValue(curveIdNode, HEU_Defines.CURVE_METHOD_PARAM, 0, curveMethodValue))
                {
                    HEU_Logger.LogWarning("Unable to get 'method' parameter" + warningMessage);
                    return false;
                }

                HEU_ParameterData closeParameter = _parameters.GetParameter(HEU_Defines.CURVE_CLOSE_PARAM);
                int curveCloseValue = System.Convert.ToInt32(closeParameter._toggle);
                if (!session.SetParamIntValue(curveIdNode, HEU_Defines.CURVE_CLOSE_PARAM, 0, curveCloseValue))
                {
                    HEU_Logger.LogWarning("Unable to get 'close' parameter" + warningMessage);
                    return false;
                }

                HEU_ParameterData reverseParameter = _parameters.GetParameter(HEU_Defines.CURVE_REVERSE_PARAM);
                int curveReverseValue = System.Convert.ToInt32(reverseParameter._toggle);
                if (!session.SetParamIntValue(curveIdNode, HEU_Defines.CURVE_REVERSE_PARAM, 0, curveReverseValue))
                {
                    HEU_Logger.LogWarning("Unable to get 'reverse' parameter" + warningMessage);
                    return false;
                }

                // Reading the curve values
                session.GetParamIntValue(curveIdNode, HEU_Defines.CURVE_TYPE_PARAM, 0, out curveTypeValue);
                session.GetParamIntValue(curveIdNode, HEU_Defines.CURVE_METHOD_PARAM, 0, out curveMethodValue);
                session.GetParamIntValue(curveIdNode, HEU_Defines.CURVE_CLOSE_PARAM, 0, out curveCloseValue);
                session.GetParamIntValue(curveIdNode, HEU_Defines.CURVE_REVERSE_PARAM, 0, out curveReverseValue);


                // For closed NURBs (Cvs and Breakpoints), we have to close the curve manually, but duplicating its last point in order to be
                // able to set the rotation and scale propertly
                bool bCloseCurveManually = false;

                if (curveCloseValue == 1 && curveTypeValue == (int)(HAPI_CurveType.HAPI_CURVETYPE_NURBS) &&
                    curveMethodValue != 2)
                {
                    // The curve is not closed anymore
                    session.SetParamIntValue(curveIdNode, HEU_Defines.CURVE_CLOSE_PARAM, 0, 0);
                    bCloseCurveManually = true;

                    // Duplicating the first point to the end point
                    // This needs to be done before sending the position string
                    positions.Add(positions[0]);
                    curveCloseValue = 0;
                }

                // Set updated coordinates string
                string positionsString = GetPointsString(positions);

                int parmId = -1;
                if (!session.GetParmIDFromName(curveIdNode, HEU_Defines.CURVE_COORDS_PARAM, out parmId))
                {
                    HEU_Logger.LogWarning("Unable to get curve 'coords' parameter." + warningMessage);
                    return false;
                }

                session.SetParamStringValue(_geoID, positionsString, parmId, 0);

                // Setting up first first cook for refinement
                HAPI_CookOptions cookOptions = HEU_HAPIUtility.GetDefaultCookOptions(session);
                cookOptions.maxVerticesPerPrimitive = -1;
                cookOptions.refineCurveToLinear = false;

                if (!HEU_HAPIUtility.CookNodeInHoudiniWithOptions(session, curveIdNode, cookOptions, CurveName))
                {
                    HEU_Logger.LogWarning("Unable to cook curve part!" + warningMessage);
                    return false;
                }

                HAPI_PartInfo partInfos = new HAPI_PartInfo();
                session.GetPartInfo(GeoID, 0, ref partInfos);

                // Depending on the curve type and method, additional control points might have been created.
                // We now have to interpolate the rotations and scale attributes for these.

                // Lambda function that interpolates rotation, scale, and uniform scale values
                // Between two points using fCoeff as a weight, and insert the interpolated value at nInsertIndex
                Action<int, int, float, int> InterpolateRotScaleUScale =
                    (int nIndex1, int nIndex2, float fCoeff, int nInsertIndex) =>
                    {
                        if (rotations != null && rotations.IsValidIndex(nIndex1) && rotations.IsValidIndex(nIndex2))
                        {
                            Quaternion interpolation = Quaternion.Slerp(rotations[nIndex1], rotations[nIndex2], fCoeff);
                            if (rotations.IsValidIndex(nInsertIndex))
                                rotations.Insert(nInsertIndex, interpolation);
                            else
                                rotations.Add(interpolation);
                        }

                        if (scales != null && scales.IsValidIndex(nIndex1) && scales.IsValidIndex(nIndex2))
                        {
                            Vector3 interpolation = Vector3.Slerp(scales[nIndex1], scales[nIndex2], fCoeff);
                            if (scales.IsValidIndex(nInsertIndex))
                                scales.Insert(nInsertIndex, interpolation);
                            else
                                scales.Add(interpolation);
                        }
                    };

                // Lambda function that duplicates rotation and scale values at nIndex, and inserts/adds it at nInsertIndex
                Action<int, int> DuplicateRotScale = (int nIndex, int nInsertIndex) =>
                {
                    if (rotations != null && rotations.IsValidIndex(nIndex))
                    {
                        Quaternion value = rotations[nIndex];
                        if (rotations.IsValidIndex(nInsertIndex))
                            rotations.Insert(nInsertIndex, value);
                        else
                            rotations.Add(value);
                    }

                    if (scales != null && scales.IsValidIndex(nIndex))
                    {
                        Vector3 value = scales[nIndex];
                        if (scales.IsValidIndex(nInsertIndex))
                            scales.Insert(nInsertIndex, value);
                        else
                            scales.Add(value);
                    }
                };

                // Do we want to close the curve by ourselves?
                if (bCloseCurveManually)
                {
                    DuplicateRotScale(0, numberOfCVs++);
                    session.SetParamIntValue(curveIdNode, HEU_Defines.CURVE_CLOSE_PARAM, 0, 1);
                }

                // INTERPOLATION
                if (curveTypeValue == (int)HAPI_CurveType.HAPI_CURVETYPE_NURBS)
                {
                    // Closed NURBS have additional points  reproducing the first ones
                    if (curveCloseValue == 1)
                    {
                        // Only the first one if the method if freehand ...
                        DuplicateRotScale(0, numberOfCVs++);
                        if (curveMethodValue != 2)
                        {
                            // ... but also the 2nd and 3rd if the method is CVs or Breakpoints
                            DuplicateRotScale(1, numberOfCVs++);
                            DuplicateRotScale(2, numberOfCVs++);
                        }
                    }
                    else if (curveMethodValue == 1)
                    {
                        // Open NURBs have 2 new points if t he method is breakpoint:
                        // One between the 1st and 2nd ...
                        InterpolateRotScaleUScale(0, 1, 0.5f, 1);

                        // ... and one before the last one.
                        InterpolateRotScaleUScale(numberOfCVs, numberOfCVs - 1, 0.5f, numberOfCVs);
                        numberOfCVs += 2;
                    }
                }
                else if (curveTypeValue == (int)HAPI_CurveType.HAPI_CURVETYPE_BEZIER)
                {
                    // Bezier curves requires additional point if the method is breakpoints
                    if (curveMethodValue == 1)
                    {
                        // 2 interpolated control points are added per points (except the last one)
                        int nOffset = 0;
                        for (int n = 0; n < numberOfCVs - 1; n++)
                        {
                            int nIndex1 = n + nOffset;
                            int nIndex2 = n + nOffset + 1;

                            InterpolateRotScaleUScale(nIndex1, nIndex2, 0.33f, nIndex2);
                            nIndex2++;
                            InterpolateRotScaleUScale(nIndex1, nIndex2, 0.66f, nIndex2);

                            nOffset += 2;
                        }

                        numberOfCVs += nOffset;

                        if (curveCloseValue == 1)
                        {
                            // If the curve is closed, we need to add 2 points after the last
                            // interpolated between the last and the first one
                            int nIndex = numberOfCVs - 1;
                            InterpolateRotScaleUScale(nIndex, 0, 0.33f, numberOfCVs++);
                            InterpolateRotScaleUScale(nIndex, 0, 0.66f, numberOfCVs++);

                            // and finally, the last point is the first.
                            DuplicateRotScale(0, numberOfCVs++);
                        }
                    }
                    else if (curveCloseValue == 1)
                    {
                        // For the other methods, if the bezier curve is closed, the last point is the 1st
                        DuplicateRotScale(0, numberOfCVs++);
                    }
                }

                // Reset all other attributes

                // Even after interpolation, additional points might still be missing
                // Bezier curves require a certain number of points regarding their order
                // if points are lacking then HAPI duplicates the last one
                if (numberOfCVs < partInfos.pointCount)
                {
                    int nToAdd = partInfos.pointCount - numberOfCVs;
                    for (int n = 0; n < nToAdd; n++)
                    {
                        DuplicateRotScale(numberOfCVs - 1, numberOfCVs);
                        numberOfCVs++;
                    }
                }

                bool bAddRotations =
                    !parentAsset.CurveDisableScaleRotation && (rotations.Count == partInfos.pointCount);
                bool bAddScales = !parentAsset.CurveDisableScaleRotation && (scales.Count == partInfos.pointCount);

                if (!bAddRotations)
                {
                    HEU_Logger.LogWarning("Point count malformed! Skipping adding rotations to curve");
                }

                if (!bAddScales)
                {
                    HEU_Logger.LogWarning("Point count malformed! Skipping adding scales to curve");
                }


                // We need to increase the point attributes count for points in the part infos
                HAPI_AttributeOwner newAttributesOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
                HAPI_AttributeOwner originalAttributesOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;

                int originalPointParametersCount = partInfos.attributeCounts[(int)newAttributesOwner];
                if (bAddRotations)
                    partInfos.attributeCounts[(int)newAttributesOwner] += 1;

                if (bAddScales)
                    partInfos.attributeCounts[(int)newAttributesOwner] += 1;

                // Sending the updated PartInfos
                if (!session.SetPartInfo(curveIdNode, 0, ref partInfos))
                {
                    HEU_Logger.LogWarning("Unable to set part info!" + warningMessage);
                    return false;
                }

                // We need now to reproduce ALL the curves atttributes for ALL the owners
                for (int nOwner = 0; nOwner < (int)HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; nOwner++)
                {
                    int nOwnerAttributeCount = nOwner == (int)newAttributesOwner
                        ? originalPointParametersCount
                        : partInfos.attributeCounts[nOwner];
                    if (nOwnerAttributeCount == 0)
                        continue;

                    string[] AttributeNamesSH = new string[nOwnerAttributeCount];
                    if (!session.GetAttributeNames(curveIdNode, 0, (HAPI_AttributeOwner)nOwner, ref AttributeNamesSH,
                            nOwnerAttributeCount))
                    {
                        HEU_Logger.LogWarning("Unable to get attribute names!" + warningMessage);
                        return false;
                    }

                    for (int nAttribute = 0; nAttribute < AttributeNamesSH.Length; nAttribute++)
                    {
                        string attr_name = AttributeNamesSH[nAttribute];
                        if (attr_name == "") continue;

                        if (attr_name == "__topology")
                        {
                            continue;
                        }

                        HAPI_AttributeInfo attr_info = new HAPI_AttributeInfo();
                        session.GetAttributeInfo(curveIdNode, _partID, attr_name, (HAPI_AttributeOwner)nOwner,
                            ref attr_info);
                        switch (attr_info.storage)
                        {
                            case HAPI_StorageType.HAPI_STORAGETYPE_INT:
                                int[] intData = new int[attr_info.count * attr_info.tupleSize];
                                session.GetAttributeIntData(curveIdNode, _partID, attr_name, ref attr_info, intData, 0,
                                    attr_info.count);
                                session.AddAttribute(curveIdNode, _partID, attr_name, ref attr_info);
                                session.SetAttributeIntData(curveIdNode, _partID, attr_name, ref attr_info, intData, 0,
                                    attr_info.count);

                                break;
                            case HAPI_StorageType.HAPI_STORAGETYPE_FLOAT:
                                float[] floatData = new float[attr_info.count * attr_info.tupleSize];
                                session.GetAttributeFloatData(curveIdNode, _partID, attr_name, ref attr_info, floatData,
                                    0, attr_info.count);
                                session.AddAttribute(curveIdNode, _partID, attr_name, ref attr_info);
                                session.SetAttributeFloatData(curveIdNode, _partID, attr_name, ref attr_info, floatData,
                                    0, attr_info.count);

                                break;
                            case HAPI_StorageType.HAPI_STORAGETYPE_STRING:
                                string[] stringData = HEU_GeneralUtility.GetAttributeStringData(session, curveIdNode, 0,
                                    attr_name, ref attr_info);
                                session.AddAttribute(curveIdNode, _partID, attr_name, ref attr_info);
                                session.SetAttributeStringData(curveIdNode, _partID, attr_name, ref attr_info,
                                    stringData, 0, attr_info.count);
                                break;
                            default:
                                //=HEU_Logger.Log("Storage type: " + attr_info.storage + " " + attr_name);
                                // primitive list doesn't matter
                                break;
                        }
                    }
                }

                if (partInfos.type == HAPI_PartType.HAPI_PARTTYPE_CURVE)
                {
                    HAPI_CurveInfo curveInfo = new HAPI_CurveInfo();
                    session.GetCurveInfo(curveIdNode, _partID, ref curveInfo);

                    int[] curveCounts = new int[curveInfo.curveCount];
                    session.GetCurveCounts(curveIdNode, _partID, curveCounts, 0, curveInfo.curveCount);

                    int[] curveOrders = new int[curveInfo.curveCount];
                    session.GetCurveOrders(curveIdNode, _partID, curveOrders, 0, curveInfo.curveCount);

                    float[] knotsArray = null;
                    if (curveInfo.hasKnots)
                    {
                        knotsArray = new float[curveInfo.knotCount];
                        session.GetCurveKnots(curveIdNode, _partID, knotsArray, 0, curveInfo.knotCount);
                    }

                    session.SetCurveInfo(curveIdNode, _partID, ref curveInfo);

                    session.SetCurveCounts(curveIdNode, _partID, curveCounts, 0, curveInfo.curveCount);
                    session.SetCurveOrders(curveIdNode, _partID, curveOrders, 0, curveInfo.curveCount);

                    if (curveInfo.hasKnots)
                    {
                        session.SetCurveKnots(curveIdNode, _partID, knotsArray, 0, curveInfo.knotCount);
                    }
                }

                if (partInfos.faceCount > 0)
                {
                    int[] faceCounts = new int[partInfos.faceCount];
                    if (session.GetFaceCounts(curveIdNode, _partID, faceCounts, 0, partInfos.faceCount, false))
                    {
                        session.SetFaceCount(curveIdNode, _partID, faceCounts, 0, partInfos.faceCount);
                    }
                }

                if (partInfos.vertexCount > 0)
                {
                    int[] vertexList = new int[partInfos.vertexCount];
                    if (session.GetVertexList(curveIdNode, _partID, vertexList, 0, partInfos.vertexCount))
                    {
                        session.SetVertexList(curveIdNode, _partID, vertexList, 0, partInfos.vertexCount);
                    }
                }


                if (bAddRotations)
                {
                    HAPI_AttributeInfo attributeInfoRotation = new HAPI_AttributeInfo();
                    attributeInfoRotation.count = numberOfCVs;
                    attributeInfoRotation.tupleSize = 4;
                    attributeInfoRotation.exists = true;
                    attributeInfoRotation.owner = newAttributesOwner;
                    attributeInfoRotation.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
                    attributeInfoRotation.originalOwner = originalAttributesOwner;

                    session.AddAttribute(_geoID, _partID, HEU_Defines.HAPI_ATTRIB_ROTATION, ref attributeInfoRotation);

                    float[] curveRotations = new float[numberOfCVs * 4];

                    for (int i = 0; i < numberOfCVs; i++)
                    {
                        HEU_HAPIUtility.ConvertRotationUnityToHoudini(rotations[i], out curveRotations[i * 4 + 0],
                            out curveRotations[i * 4 + 1], out curveRotations[i * 4 + 2],
                            out curveRotations[i * 4 + 3]);
                    }

                    session.SetAttributeFloatData(curveIdNode, _partID, HEU_Defines.HAPI_ATTRIB_ROTATION,
                        ref attributeInfoRotation, curveRotations, 0, attributeInfoRotation.count);
                }

                if (bAddScales)
                {
                    HAPI_AttributeInfo attributeInfoScale = new HAPI_AttributeInfo();
                    attributeInfoScale.count = numberOfCVs;
                    attributeInfoScale.tupleSize = 3;
                    attributeInfoScale.exists = true;
                    attributeInfoScale.owner = newAttributesOwner;
                    attributeInfoScale.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
                    attributeInfoScale.originalOwner = originalAttributesOwner;

                    session.AddAttribute(_geoID, _partID, HEU_Defines.HAPI_ATTRIB_SCALE, ref attributeInfoScale);

                    float[] curveScales = new float[numberOfCVs * 3];

                    for (int i = 0; i < numberOfCVs; i++)
                    {
                        HEU_HAPIUtility.ConvertScaleUnityToHoudini(scales[i], out curveScales[i * 3 + 0],
                            out curveScales[i * 3 + 1], out curveScales[i * 3 + 2]);
                    }

                    session.SetAttributeFloatData(curveIdNode, _partID, HEU_Defines.HAPI_ATTRIB_SCALE,
                        ref attributeInfoScale, curveScales, 0, attributeInfoScale.count);
                }

                session.CommitGeo(GeoID);

                cookOptions.refineCurveToLinear = true;

                HEU_HAPIUtility.CookNodeInHoudiniWithOptions(session, curveIdNode, cookOptions, CurveName);

                // Cook one more time otherwise it won't properly update on rebuild!
                HEU_HAPIUtility.CookNodeInHoudini(session, parentAsset.AssetID, true, parentAsset.AssetName);
            }

            return true;
        }

        // Sync curve from parameters
        internal void SyncFromParameters(HEU_SessionBase session, HEU_HoudiniAsset parentAsset, bool bNewCurve)
        {
            HAPI_NodeInfo geoNodeInfo = new HAPI_NodeInfo();
            if (!session.GetNodeInfo(_geoID, ref geoNodeInfo))
            {
                return;
            }

            if (_parameters != null)
            {
                _parameters.CleanUp();
            }
            else
            {
                _parameters = ScriptableObject.CreateInstance<HEU_Parameters>();
            }

            if (_curveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM && _inputCurveInfo == null)
            {
                _inputCurveInfo = new HEU_InputCurveInfo();
            }


            string geoNodeName = HEU_SessionManager.GetString(geoNodeInfo.nameSH, session);
            _parameters._uiLabel = geoNodeName.ToUpper() + " PARAMETERS";

            bool bResult = _parameters.Initialize(session, _geoID, ref geoNodeInfo, null, null, parentAsset);
            if (!bResult)
            {
                HEU_Logger.LogWarningFormat("Parameter generate failed for geo node {0}.", geoNodeInfo.id);
                _parameters.CleanUp();
                return;
            }

            bool bDoUpdatePoints = true;

            // If reusing points, don't update them
            if (bDoUpdatePoints && _curveNodeData.Count != 0 && bNewCurve) bDoUpdatePoints = false;

            if (bDoUpdatePoints) UpdatePoints(session);

            // Since we just reset / created new our parameters and sync'd, we also need to 
            // get the preset from Houdini session
            if (!HEU_EditorUtility.IsEditorPlaying() && IsEditable())
            {
                DownloadPresetData(session);
            }
            // single_curve_operation/order set_up_prims/order round_corners/order rounded_corner_setup/order
        }

        // Actually Update curveNodeData based on parameter/input curve information
        private void UpdatePoints(HEU_SessionBase session)
        {
            if (_bIsPartCurve && _curveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM)
            {
                UpdateCachedCurveInfo(session, false);
            }

            // We want to keep positions in sync with Houdini, but use our rotations/scales because
            // The number of them depend on the curve type
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();

            _curveNodeData.ForEach((CurveNodeData data) =>
            {
                rotations.Add(data.rotation);
                scales.Add(data.scale);
            });

            _curveNodeData.Clear();

            switch (_curveDataType)
            {
                case HEU_CurveDataType.GEO_COORDS_PARAM:
                    string pointList = "";

                    _parameters.GetStringParameterValue(HEU_Defines.CURVE_COORDS_PARAM, out pointList);

                    if (!string.IsNullOrEmpty(pointList))
                    {
                        string[] pointSplit = pointList.Split(' ');
                        for (int i = 0; i < pointSplit.Length; i++)
                        {
                            string str = pointSplit[i];

                            string[] vecSplit = str.Split(',');
                            if (vecSplit.Length == 3)
                            {
                                Vector3 position = new Vector3(
                                    -System.Convert.ToSingle(vecSplit[0],
                                        System.Globalization.CultureInfo.InvariantCulture),
                                    System.Convert.ToSingle(vecSplit[1],
                                        System.Globalization.CultureInfo.InvariantCulture),
                                    System.Convert.ToSingle(vecSplit[2],
                                        System.Globalization.CultureInfo.InvariantCulture));

                                positions.Add(position);
                            }
                        }
                    }

                    break;

                case HEU_CurveDataType.HAPI_COORDS_PARAM:
                    HAPI_AttributeInfo hapiCoordsInfo = new HAPI_AttributeInfo();
                    session.GetAttributeInfo(_geoID, _partID, HEU_HAPIConstants.HAPI_ATTRIB_INPUT_CURVE_COORDS,
                        HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL, ref hapiCoordsInfo);

                    if (hapiCoordsInfo.exists)
                    {
                        float[] posAttr = new float[hapiCoordsInfo.totalArrayElements];
                        int[] sizesArr = new int[hapiCoordsInfo.count];
                        long numPts = hapiCoordsInfo.totalArrayElements / 3;
                        session.GetAttributeFloatArrayData(_geoID, _partID,
                            HEU_HAPIConstants.HAPI_ATTRIB_INPUT_CURVE_COORDS, ref hapiCoordsInfo, ref posAttr,
                            (int)hapiCoordsInfo.totalArrayElements, ref sizesArr, 0, hapiCoordsInfo.count);

                        for (int i = 0; i < numPts; i++)
                        {
                            positions.Add(HEU_HAPIUtility.ConvertPositionUnityToHoudini(posAttr[i * 3 + 0],
                                posAttr[i * 3 + 1], posAttr[i * 3 + 2]));
                        }
                    }

                    break;
                case HEU_CurveDataType.POSITION_ATTRIBUTE:
                    HAPI_AttributeInfo posAttrInfo = new HAPI_AttributeInfo();
                    float[] _posAttr = new float[0];
                    HEU_GeneralUtility.GetAttribute(session, GeoID, _partID, HEU_HAPIConstants.HAPI_ATTRIB_POSITION,
                        ref posAttrInfo, ref _posAttr, session.GetAttributeFloatData);

                    int numPositions = posAttrInfo.count;
                    for (int i = 0; i < numPositions; i++)
                    {
                        positions.Add(HEU_HAPIUtility.ConvertPositionUnityToHoudini(_posAttr[i * 3 + 0],
                            _posAttr[i * 3 + 1], _posAttr[i * 3 + 2]));
                    }

                    break;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                CurveNodeData data = new CurveNodeData(positions[i]);

                if (_parentAsset != null && !_parentAsset.CurveDisableScaleRotation)
                {
                    if (rotations.IsValidIndex(i))
                    {
                        data.rotation = rotations[i];
                    }

                    if (scales.IsValidIndex(i))
                    {
                        data.scale = scales[i];
                    }
                }

                if (_cachedCurveInfoValid)
                {
                    data.curveCountIndex = GetCurveCountIndexFromPositionIndex(i);
                }

                _curveNodeData.Add(data);
            }
        }

        /// <summary>
        /// Project curve points onto collider or layer.
        /// </summary>
        /// <param name="parentAsset">Parent asset of the curve</param>
        /// <param name="rayDirection">Direction to cast ray</param>
        /// <param name="rayDistance">Maximum ray cast distance</param>
        internal void ProjectToCollidersInternal(HEU_HoudiniAsset parentAsset, Vector3 rayDirection, float rayDistance)
        {
            bool bRequiresUpload = false;

            LayerMask layerMask = Physics.DefaultRaycastLayers;

            HEU_Curve.CurveDrawCollision collisionType =
                HEU_HoudiniAsset.CurveDrawCollision_WrapperToInternal(parentAsset.CurveDrawCollision);
            if (collisionType == CurveDrawCollision.COLLIDERS)
            {
                List<Collider> colliders = parentAsset.GetCurveDrawColliders();

                bool bFoundHit = false;
                int numPoints = _curveNodeData.Count;
                for (int i = 0; i < numPoints; ++i)
                {
                    bFoundHit = false;
                    RaycastHit[] rayHits = Physics.RaycastAll(_curveNodeData[i].position, rayDirection, rayDistance,
                        layerMask, QueryTriggerInteraction.Ignore);
                    foreach (RaycastHit hit in rayHits)
                    {
                        foreach (Collider collider in colliders)
                        {
                            if (hit.collider == collider)
                            {
                                _curveNodeData[i].position = hit.point;
                                bFoundHit = true;
                                bRequiresUpload = true;
                                break;
                            }
                        }

                        if (bFoundHit)
                        {
                            break;
                        }
                    }
                }
            }
            else if (collisionType == CurveDrawCollision.LAYERMASK)
            {
                layerMask = parentAsset.GetCurveDrawLayerMask();

                int numPoints = _curveNodeData.Count;
                for (int i = 0; i < numPoints; ++i)
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(_curveNodeData[i].position, rayDirection, out hitInfo, rayDistance, layerMask,
                            QueryTriggerInteraction.Ignore))
                    {
                        _curveNodeData[i].position = hitInfo.point;
                        bRequiresUpload = true;
                    }
                }
            }

            if (bRequiresUpload)
            {
                HEU_ParameterData paramData = _parameters.GetParameter(HEU_Defines.CURVE_COORDS_PARAM);
                if (paramData != null)
                {
                    paramData._stringValues[0] = GetPointsString(_curveNodeData);
                }

                SetEditState(CurveEditState.REQUIRES_GENERATION);
            }
        }

        /// <summary>
        /// Returns points array as string
        /// </summary>
        /// <param name="points">List of points to stringify</param>
        /// <returns></returns>
        public static string GetPointsString(List<CurveNodeData> points)
        {
            StringBuilder sb = new StringBuilder();
            foreach (CurveNodeData pt in points)
            {
                float x;
                float y;
                float z;
                HEU_HAPIUtility.ConvertPositionUnityToHoudini(pt.position, out x, out y, out z);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2} ", x, y, z);
            }

            return sb.ToString();
        }

        // Returns points array as string given a list of  vector 3
        public static string GetPointsString(List<Vector3> points)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Vector3 pt in points)
            {
                float x;
                float y;
                float z;
                HEU_HAPIUtility.ConvertPositionUnityToHoudini(pt, out x, out y, out z);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2} ", x, y, z);
            }

            return sb.ToString();
        }

        // Set curve edit state
        internal void SetEditState(CurveEditState editState)
        {
            _editState = editState;
        }

        // Gets the transformed position (transformed point =  gameobject.transform * inPosition )
        internal Vector3 GetTransformedPosition(Vector3 inPosition)
        {
            return this._targetGameObject.transform.TransformPoint(inPosition);
        }

        // Gets the (inverted transform position = gameobject.transform^-1 * inPosition ) 
        internal Vector3 GetInvertedTransformedPosition(Vector3 inPosition)
        {
            return this._targetGameObject.transform.InverseTransformPoint(inPosition);
        }

        // Gets the inverted transform direction
        internal Vector3 GetInvertedTransformedDirection(Vector3 inPosition)
        {
            return this._targetGameObject.transform.InverseTransformVector(inPosition);
        }

        // Gets vertices
        internal Vector3[] GetVertices()
        {
            return _vertices;
        }

        // Sets curve geometry visibility
        internal void SetCurveGeometryVisibilityInternal(bool bVisible)
        {
            if (_targetGameObject != null)
            {
                MeshRenderer renderer = _targetGameObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = bVisible;
                }
            }
        }

        // Fetches the preset data for the parameters
        internal void DownloadPresetData(HEU_SessionBase session)
        {
            if (_parameters != null)
            {
                _parameters.DownloadPresetData(session);
            }
        }

        // Uploads preset data to Houdini
        internal void UploadPresetData(HEU_SessionBase session)
        {
            if (_parameters != null)
            {
                _parameters.UploadPresetData(session);
            }
        }

        // Downloads default preset data
        internal void DownloadAsDefaultPresetData(HEU_SessionBase session)
        {
            if (_parameters != null)
            {
                _parameters.DownloadAsDefaultPresetData(session);
            }
        }

        // Update cached curve info
        private void UpdateCachedCurveInfo(HEU_SessionBase session, bool copyCurveSettings)
        {
            if (_curveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM)
            {
                HAPI_InputCurveInfo inputCurveInfo = new HAPI_InputCurveInfo();
                session.GetInputCurveInfo(_geoID, _partID, ref inputCurveInfo);
                _inputCurveInfo = HEU_InputCurveInfo.CreateFromHAPI_InputCurveInfo(inputCurveInfo);
            }

            if (IsMeshCurve(session, _geoID, _partID))
            {
                // Closed curves do not have the parttype curve
                _cachedCurveInfoValid = true;
                _cachedCurveCounts = new int[1] { 1 };

                _cachedCurveCountSums = new int[1] { 1 };
                return;
            }

            HAPI_CurveInfo curveInfo = new HAPI_CurveInfo();
            if (session.GetCurveInfo(_geoID, _partID, ref curveInfo))
            {
                _cachedCurveInfoValid = true;

                _cachedCurveCounts = new int[curveInfo.curveCount];
                session.GetCurveCounts(_geoID, _partID, _cachedCurveCounts, 0, curveInfo.curveCount);

                _cachedCurveCountSums = new int[_cachedCurveCounts.Length];

                // Cache sum array for finding index positions more effiicently
                int curSum = 0;
                for (int i = 0; i < _cachedCurveCounts.Length; i++)
                {
                    curSum += _cachedCurveCounts[i];
                    _cachedCurveCountSums[i] = curSum;
                }
            }
        }

        // Helper for getting curve count index from position index
        internal int GetCurveCountIndexFromPositionIndex(int positionIndex)
        {
            if (_cachedCurveCountSums == null)
            {
                return 0;
            }

            if (_cachedCurveCountSums.Length == 1)
            {
                return 0;
            }

            // for 0 < i < n, where n = curveCounts.Length
            // positionIndex = max i such that sum{curveCounts} < positionIndex
            // or n-1 if positionIndex > sum{curveCounts}

            int pointNum = positionIndex + 1;

            for (int i = 1; i < _cachedCurveCountSums.Length; i++)
            {
                if (_cachedCurveCounts[i] < pointNum)
                {
                    continue;
                }
                else
                {
                    return i;
                }
            }

            return _cachedCurveCountSums.Length - 1;
        }

        // Is the curve a mesh curve?
        private static bool IsMeshCurve(HEU_SessionBase session, HAPI_NodeId geoID, HAPI_PartId partID)
        {
            HAPI_PartInfo partInfos = new HAPI_PartInfo();
            session.GetPartInfo(geoID, partID, ref partInfos);

            return (partInfos.type != HAPI_PartType.HAPI_PARTTYPE_CURVE);
        }

        public bool IsEquivalentTo(HEU_Curve other)
        {
            bool bResult = true;

            string header = "HEU_Curve";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveNodeData, other._curveNodeData, ref bResult, header,
                "_curveNodeData");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._vertices, other._vertices, ref bResult, header, "_vertices");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._isEditable, other._isEditable, ref bResult, header,
                "_isEditable");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._parameters, other._parameters, ref bResult, header,
                "_parameters");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._bUploadParameterPreset, other._bUploadParameterPreset,
                ref bResult, header, "_bUploadParamterPreset");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._curveName, other._curveName, ref bResult, header, "_curveName");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._targetGameObject, other._targetGameObject, ref bResult,
                header, "_targetGameObject");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._isGeoCurve, other._isGeoCurve, ref bResult, header,
                "_isGeoCurve");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._editState, other._editState, ref bResult, header,
                "_editState");

            // Skip HEU_HoudiniAsset

            return bResult;
        }
    }
} // HoudiniEngineUnity
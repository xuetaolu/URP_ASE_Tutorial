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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

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


    // <summary>
    /// Represents a general node for sending data upstream to Houdini.
    /// Currently only supports sending geometry upstream.
    /// Specify input data as file (eg. bgeo), HDA, and Unity gameobjects.
    /// </summary>
    public class HEU_InputNode : ScriptableObject, IHEU_InputNode, IHEU_HoudiniAssetSubcomponent, IEquivable<HEU_InputNode>
    {
        // PUBLIC FIELDS =================================================================

        /// <inheritdoc />
        public HEU_HoudiniAsset ParentAsset
        {
            get { return _parentAsset; }
        }

        /// <inheritdoc />
        public bool KeepWorldTransform
        {
            get { return _keepWorldTransform; }
            set { _keepWorldTransform = value; }
        }

        /// <inheritdoc />
        public bool PackGeometryBeforeMerging
        {
            get { return _packGeometryBeforeMerging; }
            set { _packGeometryBeforeMerging = value; }
        }

        /// <inheritdoc />
        public HEU_InputNodeTypeWrapper NodeType
        {
            get { return InputNodeType_InternalToWrapper(_inputNodeType); }
        }

        /// <inheritdoc />
        public HEU_InputObjectTypeWrapper ObjectType
        {
            get { return InputObjectType_InternalToWrapper(_inputObjectType); }
        }

        /// <inheritdoc />
        public HEU_InputObjectTypeWrapper PendingObjectType
        {
            get { return InputObjectType_InternalToWrapper(_pendingInputObjectType); }
            set { _pendingInputObjectType = InputObjectType_WrapperToInternal(value); }
        }

        /// <inheritdoc />
        public HAPI_NodeId InputNodeID
        {
            get { return _nodeID; }
        }

        /// <inheritdoc />
        public string InputName
        {
            get { return _inputName; }
        }

        /// <inheritdoc />
        public string LabelName
        {
            get { return _labelName; }
        }

        /// <inheritdoc />
        public string ParamName
        {
            get { return _paramName; }
        }

        /// <inheritdoc />
        public HEU_InputInterfaceMeshSettings MeshSettings
        {
            get { return _meshSettings; }
        }

        /// <inheritdoc />
        public HEU_InputInterfaceTilemapSettings TilemapSettings
        {
            get { return _tilemapSettings; }
        }

        /// <inheritdoc />
        public HEU_InputInterfaceSplineSettings SplineSettings
        {
            get { return _splineSettings; }
        }

        // ========================================================================

        // DATA -------------------------------------------------------------------------------------------------------

        // The type of input node based on how it was specified in the HDA
        internal enum InputNodeType
        {
            CONNECTION, // As an asset connection
            NODE, // Pure input asset node
            PARAMETER, // As an input parameter
        }

        [SerializeField] private InputNodeType _inputNodeType;

        internal InputNodeType InputType
        {
            get { return _inputNodeType; }
        }

        // The type of input data set by user
        [System.Serializable]
        internal enum InputObjectType
        {
            HDA,
            UNITY_MESH,
            CURVE,
            TERRAIN,
            BOUNDING_BOX,
            TILEMAP,
#if UNITY_2022_1_OR_NEWER
            SPLINE
#endif
        }


        // I don't want to break backwards compatibility, but I want some options to map onto others to avoid duplication of tested code
        // So we will map InputObjectType -> InternalObjectType when uploading input.
        public enum InternalObjectType
        {
            UNKNOWN,
            HDA,
            UNITY_MESH,
        };

        [SerializeField] private InputObjectType _inputObjectType = InputObjectType.UNITY_MESH;

        [SerializeField] private InputObjectType _pendingInputObjectType = InputObjectType.UNITY_MESH;

        // The IDs of the object merge created for the input objects
        [SerializeField] private List<HEU_InputObjectInfo> _inputObjects = new List<HEU_InputObjectInfo>();

        internal List<HEU_InputObjectInfo> InputObjects
        {
            get { return _inputObjects; }
        }

        // This holds node IDs of input nodes that are created for uploading mesh data
        [SerializeField] private List<HAPI_NodeId> _inputObjectsConnectedAssetIDs = new List<HAPI_NodeId>();

#pragma warning disable 0414
        // [DEPRECATED: replaced with _inputAssetInfos]
        // Asset input: external reference used for UI
        [SerializeField] private GameObject _inputAsset;

        // [DEPRECATED: replaced with _inputAssetInfos]
        // Asset input: internal reference to the connected asset (valid if connected)
        [SerializeField] private GameObject _connectedInputAsset;
#pragma warning restore 0414

        // List of input HDAs
        [SerializeField] private List<HEU_InputHDAInfo> _inputAssetInfos = new List<HEU_InputHDAInfo>();

        internal List<HEU_InputHDAInfo> InputAssetInfos
        {
            get { return _inputAssetInfos; }
        }

        [SerializeField] private HAPI_NodeId _nodeID;

        [SerializeField] private int _inputIndex;

        [SerializeField] private bool _requiresCook;

        internal bool RequiresCook
        {
            get { return _requiresCook; }
            set { _requiresCook = value; }
        }

        [SerializeField] private bool _requiresUpload;

        internal bool RequiresUpload
        {
            get { return _requiresUpload; }
            set { _requiresUpload = value; }
        }

        [SerializeField] private string _inputName;


        [SerializeField] private string _labelName;


        [SerializeField] internal string _paramName;


        [SerializeField] private HAPI_NodeId _connectedNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

        [SerializeField]
        // Enabling Keep World Transform by default to keep consistent with other plugins
        private bool _keepWorldTransform = true;


        [SerializeField] private bool _packGeometryBeforeMerging;


        [SerializeField] private HEU_HoudiniAsset _parentAsset;

        public enum InputActions
        {
            ACTION,
            DELETE,
            INSERT
        }

        // Input Specific settings
        [SerializeField] private HEU_InputInterfaceMeshSettings _meshSettings = new HEU_InputInterfaceMeshSettings();

        // Tilemap specific settings:
        [SerializeField] private HEU_InputInterfaceTilemapSettings _tilemapSettings = new HEU_InputInterfaceTilemapSettings();

        // Spline specific settings:
        [SerializeField] private HEU_InputInterfaceSplineSettings _splineSettings = new HEU_InputInterfaceSplineSettings();

        // Field used in UI only.
        [SerializeField] internal bool _usingSelectFromHierarchy = false;

        // PUBLIC FUNCTIONS =====================================================================================

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
            _requiresCook = true;
            if (_parentAsset != null) _parentAsset.RequestCook();
        }

        /// <inheritdoc />
        public bool IsAssetInput()
        {
            return _inputNodeType == InputNodeType.CONNECTION;
        }

        /// <inheritdoc />
        public int NumInputEntries()
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                return _inputObjects.Count;
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                return _inputAssetInfos.Count;
            }

            return 0;
        }

        /// <inheritdoc />
        public GameObject GetInputEntryGameObject(int index)
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                if (index >= 0 && index < _inputObjects.Count)
                {
                    return _inputObjects[index]._gameObject;
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Get index {0} out of range (number of items is {1})", index, _inputObjects.Count);
                }
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                if (index >= 0 && index < _inputAssetInfos.Count)
                {
                    return _inputAssetInfos[index]._pendingGO;
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Get index {0} out of range (number of items is {1})", index, _inputAssetInfos.Count);
                }
            }

            return null;
        }

        /// <inheritdoc />
        public GameObject[] GetInputEntryGameObjects()
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                GameObject[] inputObjects = new GameObject[_inputObjects.Count];

                for (int i = 0; i < _inputObjects.Count; i++)
                {
                    inputObjects[i] = _inputObjects[i]._gameObject;
                }

                return inputObjects;
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                GameObject[] inputObjects = new GameObject[_inputAssetInfos.Count];
                for (int i = 0; i < _inputAssetInfos.Count; i++)
                {
                    inputObjects[i] = _inputAssetInfos[i]._pendingGO;
                }

                return inputObjects;
            }

            return null;
        }

        /// <inheritdoc />
        public void SetInputEntry(int index, GameObject newInputGameObject, bool bRecookAsset = false)
        {
            bool bSuccess = true;

            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                if (index >= 0 && index < _inputObjects.Count)
                {
                    _inputObjects[index] = CreateInputObjectInfo(newInputGameObject);
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Insert index {0} out of range (number of items is {1})", index, _inputObjects.Count);
                    bSuccess = false;
                }
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                if (index >= 0 && index < _inputAssetInfos.Count)
                {
                    _inputAssetInfos[index] = CreateInputHDAInfo(newInputGameObject);
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Insert index {0} out of range (number of items is {1})", index, _inputAssetInfos.Count);
                    bSuccess = false;
                }
            }

            if (bSuccess && bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void InsertInputEntry(int index, GameObject newInputGameObject, bool bRecookAsset = false)
        {
            bool bSuccess = true;
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                if (index >= 0 && index < _inputObjects.Count)
                {
                    _inputObjects.Insert(index, CreateInputObjectInfo(newInputGameObject));
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Insert index {0} out of range (number of items is {1})", index, _inputObjects.Count);
                    bSuccess = false;
                }
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                if (index >= 0 && index < _inputAssetInfos.Count)
                {
                    _inputAssetInfos.Insert(index, CreateInputHDAInfo(newInputGameObject));
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Insert index {0} out of range (number of items is {1})", index, _inputAssetInfos.Count);
                    bSuccess = false;
                }
            }

            if (bSuccess && bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void AddInputEntryAtEnd(GameObject newEntryGameObject, bool bRecookAsset = false)
        {
            bool bSuccess = true;

            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                InternalAddInputObjectAtEnd(newEntryGameObject);
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                InternalAddInputHDAAtEnd(newEntryGameObject);
            }
            else
            {
                HEU_Logger.LogWarning("Warning: Unsupported input type!");
                bSuccess = false;
            }

            if (bSuccess && bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void ResetInputNode(bool bRecookAsset = false)
        {
            HEU_SessionBase session = GetSession();
            if (session != null)
            {
                ResetInputNode(session);

                if (bRecookAsset) Recook();
            }
        }

        /// <inheritdoc />
        public void ChangeInputType(HEU_InputObjectTypeWrapper newType, bool bRecookAsset = false)
        {
            InputObjectType internalType = InputObjectType_WrapperToInternal(newType);
            if (internalType == _inputObjectType)
            {
                return;
            }

            HEU_SessionBase session = GetSession();
            if (session != null)
            {
                ChangeInputType(session, internalType);

                if (bRecookAsset) Recook();
            }
        }

        /// <inheritdoc />
        public void RemoveInputEntry(int index, bool bRecookAsset = false)
        {
            bool bSuccess = true;
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                if (index >= 0 && index < _inputObjects.Count)
                {
                    _inputObjects.RemoveAt(index);
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Insert index {0} out of range (number of items is {1})", index, _inputObjects.Count);
                    bSuccess = false;
                }
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                if (index >= 0 && index < _inputAssetInfos.Count)
                {
                    _inputAssetInfos.RemoveAt(index);
                }
                else
                {
                    HEU_Logger.LogErrorFormat("Insert index {0} out of range (number of items is {1})", index, _inputAssetInfos.Count);
                    bSuccess = false;
                }
            }

            if (bSuccess && bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void RemoveAllInputEntries(bool bRecookAsset = false)
        {
            _inputObjects.Clear();
            _inputAssetInfos.Clear();
        }

        /// <inheritdoc />
        public void SetInputEntryObjectUseTransformOffset(int index, bool value, bool bRecookAsset = false)
        {
            if (index >= _inputObjects.Count)
            {
                HEU_Logger.LogError("Index is out of range when setting offset transform.");
                return;
            }

            _inputObjects[index]._useTransformOffset = value;

            if (bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void SetInputEntryObjectTransformTranslateOffset(int index, Vector3 translateOffset, bool bRecookAsset = false)
        {
            if (index >= _inputObjects.Count)
            {
                HEU_Logger.LogError("Index is out of range when setting offset transform.");
                return;
            }

            _inputObjects[index]._translateOffset = translateOffset;

            if (bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void SetInputEntryObjectTransformRotateOffset(int index, Vector3 rotateOffset, bool bRecookAsset = false)
        {
            if (index >= _inputObjects.Count)
            {
                HEU_Logger.LogError("Index is out of range when setting offset transform.");
                return;
            }

            _inputObjects[index]._rotateOffset = rotateOffset;

            if (bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public void SetInputEntryObjectTransformScaleOffset(int index, Vector3 scaleOffset, bool bRecookAsset = false)
        {
            if (index >= _inputObjects.Count)
            {
                HEU_Logger.LogError("Index is out of range when setting offset transform.");
                return;
            }

            _inputObjects[index]._scaleOffset = scaleOffset;

            if (bRecookAsset) Recook();
        }

        /// <inheritdoc />
        public bool AreAnyInputHDAsConnected()
        {
            foreach (HEU_InputHDAInfo asset in _inputAssetInfos)
            {
                if (asset._connectedGO != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public int GetConnectedInputCount()
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                return _inputObjectsConnectedAssetIDs.Count;
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                return _inputAssetInfos.Count;
            }

            return 0;
        }

        /// <inheritdoc />
        public HAPI_NodeId GetConnectedNodeID(int index)
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                if (index >= 0 && index < _inputObjectsConnectedAssetIDs.Count)
                {
                    return _inputObjectsConnectedAssetIDs[index];
                }
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                return _inputAssetInfos[index]._connectedInputNodeID;
            }

            return HEU_Defines.HEU_INVALID_NODE_ID;
        }

        /// <inheritdoc />
        public void LoadPreset(HEU_InputPreset inputPreset)
        {
            HEU_SessionBase session = GetSession();
            if (session != null)
            {
                LoadPreset(session, inputPreset);
            }
        }

        /// <inheritdoc />
        public void PopulateInputPreset(HEU_InputPreset inputPreset, bool sceneRelativeGameObjects)
        {
            inputPreset._inputObjectType = _inputObjectType;

            // Deprecated and replaced with _inputAssetPresets. Leaving it in for backwards compatibility.
            //inputPreset._inputAssetName = _inputAsset != null ? _inputAsset.name : "";

            inputPreset._inputIndex = _inputIndex;
            inputPreset._inputName = _inputName;

            inputPreset._keepWorldTransform = _keepWorldTransform;
            inputPreset._packGeometryBeforeMerging = _packGeometryBeforeMerging;

            foreach (HEU_InputObjectInfo inputObject in _inputObjects)
            {
                HEU_InputObjectPreset inputObjectPreset = new HEU_InputObjectPreset();

                if (inputObject._gameObject != null)
                {
                    inputObjectPreset._isSceneObject = !HEU_GeneralUtility.IsGameObjectInProject(inputObject._gameObject);
                    if (!inputObjectPreset._isSceneObject)
                    {
                        // For inputs in project, use the project path as name
                        inputObjectPreset._gameObjectName = HEU_AssetDatabase.GetAssetOrScenePath(inputObject._gameObject);
                    }
                    else
                    {
                        if (sceneRelativeGameObjects)
                        {
                            // If scene relative (used for presets) store the name of the object so it works in any scene.
                            inputObjectPreset._gameObjectName = inputObject._gameObject.name;
                            inputObjectPreset._gameObject = null;
                        }
                        else
                        {
                            // If not scene relative, store the game object. This is used for rebuilding.
                            inputObjectPreset._gameObjectName = "";
                            inputObjectPreset._gameObject = inputObject._gameObject;
                        }
                    }
                }
                else
                {
                    inputObjectPreset._gameObjectName = "";
                }

                inputObjectPreset._useTransformOffset = inputObject._useTransformOffset;
                inputObjectPreset._translateOffset = inputObject._translateOffset;
                inputObjectPreset._rotateOffset = inputObject._rotateOffset;
                inputObjectPreset._scaleOffset = inputObject._scaleOffset;

                inputPreset._inputObjectPresets.Add(inputObjectPreset);
            }

            foreach (HEU_InputHDAInfo hdaInfo in _inputAssetInfos)
            {
                HEU_InputAssetPreset inputAssetPreset = new HEU_InputAssetPreset();

                if (hdaInfo._connectedGO != null)
                {
                    if (!HEU_GeneralUtility.IsGameObjectInProject(hdaInfo._connectedGO))
                    {
                        inputAssetPreset._gameObjectName = hdaInfo._connectedGO.name;
                    }
                    else
                    {
                        inputAssetPreset._gameObjectName = "";
                    }

                    inputPreset._inputAssetPresets.Add(inputAssetPreset);
                }
            }
        }

        // =====================================================================================================

        // LOGIC ------------------------------------------------------------------------------------------------------

        internal static HEU_InputNode CreateSetupInput(HAPI_NodeId nodeID, int inputIndex, string inputName, string labelName,
            InputNodeType inputNodeType, HEU_HoudiniAsset parentAsset)
        {
            HEU_InputNode newInput = ScriptableObject.CreateInstance<HEU_InputNode>();
            newInput._nodeID = nodeID;
            newInput._inputIndex = inputIndex;
            newInput._inputName = inputName;
            newInput._labelName = labelName;
            newInput._inputNodeType = inputNodeType;
            newInput._parentAsset = parentAsset;

            newInput._requiresUpload = false;
            newInput._requiresCook = false;

            return newInput;
        }

        internal void SetInputNodeID(HAPI_NodeId nodeID)
        {
            _nodeID = nodeID;
        }

        internal void DestroyAllData(HEU_SessionBase session)
        {
            ClearUICache();

            DisconnectAndDestroyInputs(session);
            RemoveAllInputEntries();
        }

        private void ResetInputObjectTransforms()
        {
            for (int i = 0; i < _inputObjects.Count; ++i)
            {
                _inputObjects[i]._syncdTransform = Matrix4x4.identity;
                _inputObjects[i]._syncdChildTransforms.Clear();
            }
        }

        internal void ResetInputNode(HEU_SessionBase session)
        {
            ResetConnectionForForceUpdate(session);
            RemoveAllInputEntries();
            ClearUICache();

            ChangeInputType(session, InputObjectType.UNITY_MESH);
        }

        // Add a new entry to the end (for UNITY_MESH)
        internal HEU_InputObjectInfo AddInputEntryAtEndMesh(GameObject newEntryGameObject)
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                return InternalAddInputObjectAtEnd(newEntryGameObject);
            }

            return null;
        }

        // Add a new entry to the end (for HDAs)
        internal HEU_InputHDAInfo AddInputEntryAtEndHDA(GameObject newEntryGameObject)
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                return InternalAddInputHDAAtEnd(newEntryGameObject);
            }

            return null;
        }

        // Change the input type
        internal void ChangeInputType(HEU_SessionBase session, InputObjectType newType)
        {
            if (newType == _inputObjectType)
                return;

            DisconnectAndDestroyInputs(session);

            _inputObjectType = newType;
            _pendingInputObjectType = _inputObjectType;
        }

        /// <summary>
        /// Reset the connected state so that any previous connection will be remade
        /// </summary>
        internal void ResetConnectionForForceUpdate(HEU_SessionBase session)
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                if (AreAnyInputHDAsConnected())
                {
                    // By disconnecting here, we can then properly reconnect again.
                    // This is needed when loading a saved scene and recooking.
                    DisconnectConnectedMergeNode(session);

                    // Clear out input HDA hooks (upstream callback)
                    ClearConnectedInputHDAs();
                }
            }
        }

        internal void UploadInput(HEU_SessionBase session)
        {
            if (_nodeID == HEU_Defines.HEU_INVALID_NODE_ID)
            {
                HEU_Logger.LogErrorFormat("Input Node ID is invalid. Unable to upload input. Try recooking.");
                return;
            }

            if (_pendingInputObjectType != _inputObjectType)
            {
                ChangeInputType(session, _pendingInputObjectType);
            }

            if (_inputObjectType == InputObjectType.CURVE)
            {
                // Curves are the same as HDAs except with type checking

                foreach (HEU_InputHDAInfo inputHDAInfo in _inputAssetInfos)
                {
                    if (inputHDAInfo == null || inputHDAInfo._pendingGO == null)
                    {
                        continue;
                    }

                    HEU_HoudiniAssetRoot assetRoot = inputHDAInfo._pendingGO.GetComponent<HEU_HoudiniAssetRoot>();
                    if (assetRoot != null && assetRoot._houdiniAsset != null)
                    {
                        if (assetRoot._houdiniAsset.Curves.Count == 0)
                        {
                            HEU_Logger.LogErrorFormat("Input asset {0} contains no curves!", assetRoot.gameObject.name);
                        }
                    }
                }

                UploadHDAInput(session);
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                // An HDA input should be able to use any kind of HDA
                UploadHDAInput(session);
            }
            else
            {
                UploadUnityInput(session);
                //HEU_Logger.LogErrorFormat("Unsupported input type {0}. Unable to upload input.", _inputObjectType);
            }

            RequiresUpload = false;
            RequiresCook = true;

            ClearUICache();
        }

        // Actually uploads the HDA input to Houdini
        private void UploadHDAInput(HEU_SessionBase session)
        {
            // Connect HDAs

            // First clear all previous input connections
            DisconnectAndDestroyInputs(session);

            // Create merge object, and connect all input HDAs
            bool bResult = HEU_InputUtility.CreateInputNodeWithMultiAssets(session, _parentAsset, ref _connectedNodeID, ref _inputAssetInfos,
                _keepWorldTransform, -1);
            if (!bResult)
            {
                DisconnectAndDestroyInputs(session);
                return;
            }

            // Now connect from this asset to the merge object
            ConnectToMergeObject(session);

            if (!UploadObjectMergeTransformType(session))
            {
                HEU_Logger.LogErrorFormat("Failed to upload object merge transform type!");
                return;
            }

            if (!UploadObjectMergePackGeometry(session))
            {
                HEU_Logger.LogErrorFormat("Failed to upload object merge pack geometry value!");
                return;
            }
        }

        // Actually uploads the Unity input to Houdini
        private void UploadUnityInput(HEU_SessionBase session)
        {
            // Connect regular gameobjects

            if (_inputObjects == null || _inputObjects.Count == 0)
            {
                DisconnectAndDestroyInputs(session);
            }
            else
            {
                DisconnectAndDestroyInputs(session);

                List<HEU_InputObjectInfo> inputObjectClone = new List<HEU_InputObjectInfo>(_inputObjects);

                // Special input interface preprocessing
                for (int i = inputObjectClone.Count - 1; i >= 0; i--)
                {
                    if (inputObjectClone[i] == null || inputObjectClone[i]._gameObject == null)
                    {
                        continue;
                    }

                    HEU_BoundingVolume boundingVolume = inputObjectClone[i]._gameObject.GetComponent<HEU_BoundingVolume>();
                    if (boundingVolume == null)
                    {
                        continue;
                    }

                    List<GameObject> boundingBoxObjects = boundingVolume.GetAllIntersectingObjects();
                    if (boundingBoxObjects == null)
                    {
                        continue;
                    }

                    foreach (GameObject obj in boundingBoxObjects)
                    {
                        if (obj == null)
                        {
                            continue;
                        }

                        HEU_InputObjectInfo newObjInfo = new HEU_InputObjectInfo();
                        inputObjectClone[i].CopyTo(newObjInfo);
                        newObjInfo._gameObject = obj;
                        inputObjectClone.Add(newObjInfo);
                    }

                    // Remove this because it's not a real interface
                    inputObjectClone.RemoveAt(i);
                }

                // Create merge object, and input nodes with data, then connect them to the merge object
                bool bResult = HEU_InputUtility.CreateInputNodeWithMultiObjects(session, _nodeID, ref _connectedNodeID, ref inputObjectClone,
                    ref _inputObjectsConnectedAssetIDs, this);
                if (!bResult)
                {
                    DisconnectAndDestroyInputs(session);
                    return;
                }

                // Now connect from this asset to the merge object
                ConnectToMergeObject(session);

                if (!UploadObjectMergeTransformType(session))
                {
                    HEU_Logger.LogErrorFormat("Failed to upload object merge transform type!");
                    return;
                }

                if (!UploadObjectMergePackGeometry(session))
                {
                    HEU_Logger.LogErrorFormat("Failed to upload object merge pack geometry value!");
                    return;
                }
            }
        }

        internal void ReconnectToUpstreamAsset()
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA && AreAnyInputHDAsConnected())
            {
                foreach (HEU_InputHDAInfo hdaInfo in _inputAssetInfos)
                {
                    HEU_HoudiniAssetRoot inputAssetRoot =
                        hdaInfo._connectedGO != null ? hdaInfo._connectedGO.GetComponent<HEU_HoudiniAssetRoot>() : null;
                    if (inputAssetRoot != null && inputAssetRoot._houdiniAsset != null)
                    {
                        _parentAsset.ConnectToUpstream(inputAssetRoot._houdiniAsset);
                    }
                }
            }
        }

        private HEU_InputObjectInfo CreateInputObjectInfo(GameObject inputGameObject)
        {
            HEU_InputObjectInfo newObjectInfo = new HEU_InputObjectInfo();
            newObjectInfo._gameObject = inputGameObject;
            newObjectInfo.SetReferencesFromGameObject();

            return newObjectInfo;
        }

        private HEU_InputHDAInfo CreateInputHDAInfo(GameObject inputGameObject)
        {
            HEU_InputHDAInfo newInputInfo = new HEU_InputHDAInfo();
            newInputInfo._pendingGO = inputGameObject;
            newInputInfo._connectedInputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            return newInputInfo;
        }

        // Helper for adding a new input object the end
        private HEU_InputObjectInfo InternalAddInputObjectAtEnd(GameObject newInputGameObject)
        {
            HEU_InputObjectInfo inputObject = CreateInputObjectInfo(newInputGameObject);
            _inputObjects.Add(inputObject);
            return inputObject;
        }

        // Helper for adding a new input object the end
        private HEU_InputHDAInfo InternalAddInputHDAAtEnd(GameObject newInputHDA)
        {
            HEU_InputHDAInfo inputInfo = CreateInputHDAInfo(newInputHDA);
            _inputAssetInfos.Add(inputInfo);
            return inputInfo;
        }

        private void DisconnectConnectedMergeNode(HEU_SessionBase session)
        {
            if (session != null && _parentAsset != null)
            {
                //HEU_Logger.LogWarningFormat("Disconnecting Node Input for _nodeID={0} with type={1}", _nodeID, _inputNodeType);

                if (_inputNodeType == InputNodeType.PARAMETER)
                {
                    HEU_ParameterData paramData = _parentAsset.Parameters.GetParameter(_paramName);
                    if (paramData == null)
                    {
                        HEU_Logger.LogErrorFormat("Unable to find parameter with name {0}!", _paramName);
                    }
                    else if (!session.SetParamStringValue(_nodeID, "", paramData.ParmID, 0))
                    {
                        HEU_Logger.LogErrorFormat("Unable to clear object path parameter for input node!");
                    }
                }
                else if (_nodeID != HEU_Defines.HEU_INVALID_NODE_ID)
                {
                    session.DisconnectNodeInput(_nodeID, _inputIndex, false);
                }
            }
        }

        private void ClearConnectedInputHDAs()
        {
            int numInputs = _inputAssetInfos.Count;
            for (int i = 0; i < numInputs; ++i)
            {
                if (_inputAssetInfos[i] == null)
                {
                    continue;
                }

                HEU_HoudiniAssetRoot inputAssetRoot = _inputAssetInfos[i]._connectedGO != null
                    ? _inputAssetInfos[i]._connectedGO.GetComponent<HEU_HoudiniAssetRoot>()
                    : null;
                if (inputAssetRoot != null)
                {
                    _parentAsset.DisconnectFromUpstream(inputAssetRoot._houdiniAsset);
                }

                _inputAssetInfos[i]._connectedGO = null;
                _inputAssetInfos[i]._connectedInputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            }
        }

        /// <summary>
        /// Connect the input to the merge object node
        /// </summary>
        /// <param name="session"></param>
        private void ConnectToMergeObject(HEU_SessionBase session)
        {
            if (_inputNodeType == InputNodeType.PARAMETER)
            {
                if (string.IsNullOrEmpty(_paramName))
                {
                    HEU_Logger.LogErrorFormat("Invalid parameter name for input node of parameter type!");
                    return;
                }

                if (!session.SetParamNodeValue(_nodeID, _paramName, _connectedNodeID))
                {
                    HEU_Logger.LogErrorFormat("Unable to connect to input node!");
                    return;
                }

                //HEU_Logger.LogFormat("Setting input connection for parameter {0} with {1} connecting to {2}", _paramName, _nodeID, _connectedNodeID);
            }
            else
            {
                if (!session.ConnectNodeInput(_nodeID, _inputIndex, _connectedNodeID))
                {
                    HEU_Logger.LogErrorFormat("Unable to connect to input node!");
                    return;
                }
            }
        }

        private void DisconnectAndDestroyInputs(HEU_SessionBase session)
        {
            // First disconnect the merge node from its connections
            DisconnectConnectedMergeNode(session);

            // Clear out input HDA hooks (upstream callback)
            ClearConnectedInputHDAs();

            if (session != null)
            {
                // Delete the input nodes that were created
                foreach (HAPI_NodeId nodeID in _inputObjectsConnectedAssetIDs)
                {
                    if (nodeID != HEU_Defines.HEU_INVALID_NODE_ID)
                    {
                        session.DeleteNode(nodeID);
                    }
                }

                // Delete the SOP/merge we created
                if (_connectedNodeID != HEU_Defines.HEU_INVALID_NODE_ID && HEU_HAPIUtility.IsNodeValidInHoudini(session, _connectedNodeID))
                {
                    // We'll delete the parent Object because we presume to have created the SOP/merge ourselves.
                    // If the parent Object doesn't get deleted, it sticks around unused.
                    HAPI_NodeInfo parentNodeInfo = new HAPI_NodeInfo();
                    if (session.GetNodeInfo(_connectedNodeID, ref parentNodeInfo))
                    {
                        if (parentNodeInfo.parentId != HEU_Defines.HEU_INVALID_NODE_ID)
                        {
                            session.DeleteNode(parentNodeInfo.parentId);
                        }
                    }
                }
            }

            _inputObjectsConnectedAssetIDs.Clear();
            _connectedNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
        }

        internal bool UploadObjectMergeTransformType(HEU_SessionBase session)
        {
            if (_connectedNodeID == HEU_Defines.HEU_INVALID_NODE_ID)
            {
                return false;
            }

            int transformType = _keepWorldTransform ? 1 : 0;

            HAPI_NodeId inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            // Use _connectedNodeID to find its connections, which should be
            // the object merge nodes. We set the pack parameter on those.
            // Presume that the number of connections to  _connectedNodeID is equal to 
            // size of GetConnectedInputCount() (i.e. the number of inputs)
            int numConnected = GetConnectedInputCount();
            for (int i = 0; i < numConnected; ++i)
            {
                if (GetConnectedNodeID(i) == HEU_Defines.HEU_INVALID_NODE_ID)
                {
                    continue;
                }

                inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
                if (session.QueryNodeInput(_connectedNodeID, i, out inputNodeID, false))
                {
                    session.SetParamIntValue(inputNodeID, HEU_Defines.HAPI_OBJMERGE_TRANSFORM_PARAM, 0, transformType);
                }
            }

            return true;
        }

        private bool UploadObjectMergePackGeometry(HEU_SessionBase session)
        {
            if (_connectedNodeID == HEU_HAPIConstants.HAPI_INVALID_PARM_ID)
            {
                return false;
            }

            int packEnabled = _packGeometryBeforeMerging ? 1 : 0;

            HAPI_NodeId inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

            // Use _connectedNodeID to find its connections, which should be
            // the object merge nodes. We set the pack parameter on those.
            // Presume that the number of connections to  _connectedNodeID is equal to 
            // size of GetConnectedInputCount() (i.e. the number of inputs)
            int numConnected = GetConnectedInputCount();
            for (int i = 0; i < numConnected; ++i)
            {
                if (GetConnectedNodeID(i) == HEU_Defines.HEU_INVALID_NODE_ID)
                {
                    continue;
                }

                inputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
                if (session.QueryNodeInput(_connectedNodeID, i, out inputNodeID, false))
                {
                    session.SetParamIntValue(inputNodeID, HEU_Defines.HAPI_OBJMERGE_PACK_GEOMETRY, 0, packEnabled);
                }
            }

            return true;
        }

        // Check if the input node has changed.
        internal bool HasInputNodeTransformChanged()
        {
            bool recursive = HEU_PluginSettings.ChildTransformChangeTriggersCooks;

            // Only need to check Mesh inputs, since HDA inputs don't upload transform
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                foreach (HEU_InputObjectInfo inputObject in _inputObjects)
                {
                    if (inputObject._gameObject != null)
                    {
                        if (inputObject._useTransformOffset)
                        {
                            if (!HEU_HAPIUtility.IsSameTransform(ref inputObject._syncdTransform, ref inputObject._translateOffset,
                                    ref inputObject._rotateOffset, ref inputObject._scaleOffset))
                            {
                                return true;
                            }
                        }
                        else if (inputObject._gameObject.transform.localToWorldMatrix != inputObject._syncdTransform)
                        {
                            return true;
                        }

                        if (recursive)
                        {
                            List<Matrix4x4> curMatrixTransforms = new List<Matrix4x4>();
                            HEU_InputUtility.GetChildrenTransforms(inputObject._gameObject.transform, ref curMatrixTransforms);

                            if (curMatrixTransforms.Count != inputObject._syncdChildTransforms.Count)
                            {
                                return true;
                            }

                            int length = curMatrixTransforms.Count;
                            for (int i = 0; i < length; i++)
                            {
                                if (curMatrixTransforms[i] != inputObject._syncdChildTransforms[i])
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        // Upload input object transforms
        internal void UploadInputObjectTransforms(HEU_SessionBase session)
        {
            // Only need to upload Mesh inputs, since HDA inputs don't upload transform
            if (_nodeID == HEU_HAPIConstants.HAPI_INVALID_PARM_ID ||
                HEU_InputNode.GetInternalObjectType(_inputObjectType) != InternalObjectType.UNITY_MESH)
            {
                return;
            }

            int numInputs = GetConnectedInputCount();
            for (int i = 0; i < numInputs; ++i)
            {
                HAPI_NodeId connectedNodeID = GetConnectedNodeID(i);
                if (connectedNodeID != HEU_Defines.HEU_INVALID_NODE_ID && _inputObjects[i]._gameObject != null)
                {
                    HEU_InputUtility.UploadInputObjectTransform(session, _inputObjects[i], connectedNodeID, _keepWorldTransform);
                }
            }
        }

        /// <summary>
        /// Update the input connection based on the fact that the owner asset was recreated
        /// in the given session.
        /// All connections will be invalidated without cleaning up because the IDs can't be trusted.
        /// </summary>
        /// <param name="session"></param>
        internal void UpdateOnAssetRecreation(HEU_SessionBase session)
        {
            if (GetInternalObjectType(_inputObjectType) == InternalObjectType.HDA)
            {
                // For HDA inputs, need to recreate the merge node, cook the HDAs, and connect the HDAs to the merge nodes

                // For backwards compatiblity, copy the previous single input asset reference into the new input asset list
                if (_inputAsset != null && _inputAssetInfos.Count == 0)
                {
                    InternalAddInputHDAAtEnd(_inputAsset);

                    // Clear out these deprecated references for forever
                    _inputAsset = null;
                    _connectedInputAsset = null;
                }

                // Don't delete the merge node ID as its most likely not valid
                _connectedNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

                int numInputs = _inputAssetInfos.Count;
                for (int i = 0; i < numInputs; ++i)
                {
                    _inputAssetInfos[i]._connectedGO = null;
                    _inputAssetInfos[i]._connectedInputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
                }
            }
            else if (GetInternalObjectType(_inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                // For mesh input, invalidate _inputObjectsConnectedAssetIDs and _connectedNodeID as their
                // nodes most likely don't exist, and the IDs will not be correct since this asset got recreated
                // Note that _inputObjects don't need to be cleared as they will be used when recreating the connections.
                _inputObjectsConnectedAssetIDs.Clear();
                _connectedNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
            }
        }

        // Helper to copy input values
        internal void CopyInputValuesTo(HEU_SessionBase session, HEU_InputNode destInputNode)
        {
            destInputNode._pendingInputObjectType = _inputObjectType;

            if (GetInternalObjectType(destInputNode._inputObjectType) == InternalObjectType.HDA)
            {
                destInputNode.ResetConnectionForForceUpdate(session);
            }

            destInputNode.RemoveAllInputEntries();

            foreach (HEU_InputObjectInfo srcInputObject in _inputObjects)
            {
                HEU_InputObjectInfo newInputObject = new HEU_InputObjectInfo();
                srcInputObject.CopyTo(newInputObject);

                destInputNode._inputObjects.Add(newInputObject);
            }

            foreach (HEU_InputHDAInfo srcInputInfo in _inputAssetInfos)
            {
                HEU_InputHDAInfo newInputInfo = new HEU_InputHDAInfo();
                srcInputInfo.CopyTo(newInputInfo);

                destInputNode._inputAssetInfos.Add(newInputInfo);
            }

            destInputNode._keepWorldTransform = _keepWorldTransform;
            destInputNode._packGeometryBeforeMerging = _packGeometryBeforeMerging;
        }


        internal void LoadPreset(HEU_SessionBase session, HEU_InputPreset inputPreset)
        {
            ResetInputNode(session);

            ChangeInputType(session, inputPreset._inputObjectType);

            if (GetInternalObjectType(inputPreset._inputObjectType) == InternalObjectType.UNITY_MESH)
            {
                bool bSet = false;
                int numObjects = inputPreset._inputObjectPresets.Count;
                for (int i = 0; i < numObjects; ++i)
                {
                    bSet = false;

                    // Fetch the gameObject. This will with be the name of an object in the scene, _gameObjectName,
                    // used for presets, or an explicit gameObject, _gameObject, used during rebuilds.

                    GameObject inputGO = null;
                    if (!string.IsNullOrEmpty(inputPreset._inputObjectPresets[i]._gameObjectName))
                    {
                        if (inputPreset._inputObjectPresets[i]._isSceneObject)
                        {
                            inputGO = HEU_GeneralUtility.GetGameObjectByNameInScene(inputPreset._inputObjectPresets[i]._gameObjectName);
                        }
                        else
                        {
                            // Use the _gameObjectName as path to find in scene
                            inputGO =
                                HEU_AssetDatabase.LoadAssetAtPath(inputPreset._inputObjectPresets[i]._gameObjectName, typeof(GameObject)) as
                                    GameObject;
                            if (inputGO == null)
                            {
                                HEU_Logger.LogErrorFormat("Unable to find input at {0}", inputPreset._inputObjectPresets[i]._gameObjectName);
                            }
                        }
                    } 
                    else if (inputPreset._inputObjectPresets[i]._gameObject != null)
                    {
                        inputGO = inputPreset._inputObjectPresets[i]._gameObject; 
                    }

                    if (inputGO != null)
                    {
                        HEU_InputObjectInfo inputObject = InternalAddInputObjectAtEnd(inputGO);
                        bSet = true;
                        inputObject._useTransformOffset = inputPreset._inputObjectPresets[i]._useTransformOffset;
                        inputObject._translateOffset = inputPreset._inputObjectPresets[i]._translateOffset;
                        inputObject._rotateOffset = inputPreset._inputObjectPresets[i]._rotateOffset;
                        inputObject._scaleOffset = inputPreset._inputObjectPresets[i]._scaleOffset;
                    }
                    else
                    {
                        HEU_Logger.LogWarningFormat("Gameobject with name {0} not found. Unable to set input object.",
                            inputPreset._inputAssetName);
                    }

                    if (!bSet)
                    {
                        // Add dummy spot (user can replace it manually)
                        InternalAddInputObjectAtEnd(null);
                    }
                }
            }
            else if (HEU_InputNode.GetInternalObjectType(inputPreset._inputObjectType) == HEU_InputNode.InternalObjectType.HDA)
            {
                bool bSet = false;
                int numInptus = inputPreset._inputAssetPresets.Count;
                for (int i = 0; i < numInptus; ++i)
                {
                    bSet = false;
                    if (!string.IsNullOrEmpty(inputPreset._inputAssetPresets[i]._gameObjectName))
                    {
                        bSet = FindAddToInputHDA(inputPreset._inputAssetPresets[i]._gameObjectName);
                    }

                    if (!bSet)
                    {
                        // Couldn't add for some reason, so just add dummy spot (user can replace it manually)
                        InternalAddInputHDAAtEnd(null);
                    }
                }

                if (numInptus == 0 && !string.IsNullOrEmpty(inputPreset._inputAssetName))
                {
                    // Old preset. Add it to input
                    FindAddToInputHDA(inputPreset._inputAssetName);
                }
            }

            KeepWorldTransform = inputPreset._keepWorldTransform;
            PackGeometryBeforeMerging = inputPreset._packGeometryBeforeMerging;

            RequiresUpload = true;

            ClearUICache();
        }

        private bool FindAddToInputHDA(string gameObjectName)
        {
            HEU_HoudiniAssetRoot inputAssetRoot = HEU_GeneralUtility.GetHDAByGameObjectNameInScene(gameObjectName);
            if (inputAssetRoot != null && inputAssetRoot._houdiniAsset != null)
            {
                // Adding to list will take care of reconnecting
                InternalAddInputHDAAtEnd(inputAssetRoot.gameObject);
                return true;
            }
            else
            {
                HEU_Logger.LogWarningFormat("HDA with gameobject name {0} not found. Unable to set input asset.", gameObjectName);
            }

            return false;
        }

        internal void NotifyParentRemovedInput()
        {
            if (_parentAsset != null)
            {
                _parentAsset.RemoveInputNode(this);
            }
        }

        // UI CACHE ---------------------------------------------------------------------------------------------------

        public HEU_InputNodeUICache _uiCache;

        internal void ClearUICache()
        {
            _uiCache = null;
        }

        /// <summary>
        /// Appends given selectedObjects to the input field.
        /// </summary>
        /// <param name="selectedObjects">Array of GameObjects that should be appended into new input entries</param>
        internal void HandleSelectedObjectsForInputObjects(GameObject[] selectedObjects)
        {
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                GameObject rootGO = ParentAsset.RootGameObject;

                foreach (GameObject selected in selectedObjects)
                {
                    if (selected == rootGO)
                    {
                        continue;
                    }

                    InternalAddInputObjectAtEnd(selected);
                }

                RequiresUpload = true;

                if (HEU_PluginSettings.CookingEnabled && ParentAsset.AutoCookOnParameterChange)
                {
                    ParentAsset.RequestCook(bCheckParametersChanged: true, bAsync: true, bSkipCookCheck: false, bUploadParameters: true);
                }
            }
        }

        /// <summary>
        ///  Appends given selectedObjects to the input field.
        /// </summary>
        /// <param name="selectedObjects">Array of HDAs that should be appended into new input entries</param>
        internal void HandleSelectedObjectsForInputHDAs(GameObject[] selectedObjects)
        {
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                GameObject rootGO = ParentAsset.RootGameObject;

                foreach (GameObject selected in selectedObjects)
                {
                    if (selected == rootGO)
                    {
                        continue;
                    }

                    InternalAddInputHDAAtEnd(selected);
                }

                RequiresUpload = true;

                if (HEU_PluginSettings.CookingEnabled && ParentAsset.AutoCookOnParameterChange)
                {
                    ParentAsset.RequestCook(bCheckParametersChanged: true, bAsync: true, bSkipCookCheck: false, bUploadParameters: true);
                }
            }
        }

        public bool IsEquivalentTo(HEU_InputNode other)
        {
            bool bResult = true;

            string header = "HEU_InputNode";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._inputNodeType, other._inputNodeType, ref bResult, header, "_inputNodeType");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._pendingInputObjectType, other._pendingInputObjectType, ref bResult, header,
                "_pendingInputObjectType");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._inputObjects.Count, other._inputObjects.Count, ref bResult, header, "_inputObjects.Count");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._inputObjects, other._inputObjects, ref bResult, header, "_inputObjects");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._inputAssetInfos, other._inputAssetInfos, ref bResult, header, "_inputAssetInfos");
            //HEU_TestHelpers.AssertTrueLogEquivalent(this._inputIndex, other._inputIndex, ref bResult, header, "_inputIndex");
            //HEU_TestHelpers.AssertTrueLogEquivalent(this._requiresCook, other._requiresCook, ref bResult, header, "_requiresCook");
            //HEU_TestHelpers.AssertTrueLogEquivalent(this._requiresUpload, other._requiresUpload, ref bResult, header, "_requiresUpload");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._inputName, other._inputName, ref bResult, header, "_inputName");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._labelName, other._labelName, ref bResult, header, "_labelName");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._paramName, other._paramName, ref bResult, header, "_paramName");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._keepWorldTransform, other._keepWorldTransform, ref bResult, header, "_keepWorldTransform");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._packGeometryBeforeMerging, other.PackGeometryBeforeMerging, ref bResult, header,
                "_packGeometryBeforeMerging");

            // Skip conneceted node id
            // Skip _inputObjectsConnectedAssetIds
            // Skip inputAsset/connectedINputAsset
            // Skip parent asset

            return bResult;
        }

        internal static InternalObjectType GetInternalObjectType(InputObjectType type)
        {
            switch (type)
            {
                case InputObjectType.HDA:
                case InputObjectType.CURVE:
                    return InternalObjectType.HDA;
                case InputObjectType.UNITY_MESH:
#if UNITY_2022_1_OR_NEWER
                case InputObjectType.SPLINE:
#endif
                case InputObjectType.TERRAIN:
                case InputObjectType.BOUNDING_BOX:
                case InputObjectType.TILEMAP:
                    return InternalObjectType.UNITY_MESH;
                default:
                    return InternalObjectType.UNKNOWN;
            }
        }

        internal static HEU_InputNodeTypeWrapper InputNodeType_InternalToWrapper(HEU_InputNode.InputNodeType inputNodeType)
        {
            switch (inputNodeType)
            {
                case HEU_InputNode.InputNodeType.CONNECTION:
                    return HEU_InputNodeTypeWrapper.CONNECTION;
                case HEU_InputNode.InputNodeType.NODE:
                    return HEU_InputNodeTypeWrapper.NODE;
                case HEU_InputNode.InputNodeType.PARAMETER:
                    return HEU_InputNodeTypeWrapper.PARAMETER;
                default:
                    return HEU_InputNodeTypeWrapper.CONNECTION;
            }
        }

        internal static HEU_InputNode.InputNodeType InputNodeType_InternalToWrapper(HEU_InputNodeTypeWrapper inputNodeType)
        {
            switch (inputNodeType)
            {
                case HEU_InputNodeTypeWrapper.CONNECTION:
                    return HEU_InputNode.InputNodeType.CONNECTION;
                case HEU_InputNodeTypeWrapper.NODE:
                    return HEU_InputNode.InputNodeType.NODE;
                case HEU_InputNodeTypeWrapper.PARAMETER:
                    return HEU_InputNode.InputNodeType.PARAMETER;
                default:
                    return HEU_InputNode.InputNodeType.CONNECTION;
            }
        }

        internal static HEU_InputObjectTypeWrapper InputObjectType_InternalToWrapper(HEU_InputNode.InputObjectType inputType)
        {
            switch (inputType)
            {
                case HEU_InputNode.InputObjectType.HDA:
                    return HEU_InputObjectTypeWrapper.HDA;
                case HEU_InputNode.InputObjectType.UNITY_MESH:
                    return HEU_InputObjectTypeWrapper.UNITY_MESH;
                case HEU_InputNode.InputObjectType.CURVE:
                    return HEU_InputObjectTypeWrapper.CURVE;
#if UNITY_2022_1_OR_NEWER
                case HEU_InputNode.InputObjectType.SPLINE:
                    return HEU_InputObjectTypeWrapper.SPLINE;
#endif
                case HEU_InputNode.InputObjectType.BOUNDING_BOX:
                    return HEU_InputObjectTypeWrapper.BOUNDING_BOX;
                case HEU_InputNode.InputObjectType.TILEMAP:
                    return HEU_InputObjectTypeWrapper.TILEMAP;
                default:
                    return HEU_InputObjectTypeWrapper.UNITY_MESH;
            }
        }

        internal static HEU_InputNode.InputObjectType InputObjectType_WrapperToInternal(HEU_InputObjectTypeWrapper inputType)
        {
            switch (inputType)
            {
                case HEU_InputObjectTypeWrapper.HDA:
                    return HEU_InputNode.InputObjectType.HDA;
                case HEU_InputObjectTypeWrapper.UNITY_MESH:
                    return HEU_InputNode.InputObjectType.UNITY_MESH;
                case HEU_InputObjectTypeWrapper.CURVE:
                    return HEU_InputNode.InputObjectType.CURVE;
#if UNITY_2022_1_OR_NEWER
                case HEU_InputObjectTypeWrapper.SPLINE:
                    return HEU_InputNode.InputObjectType.SPLINE;
#endif
                case HEU_InputObjectTypeWrapper.BOUNDING_BOX:
                    return HEU_InputNode.InputObjectType.BOUNDING_BOX;
                case HEU_InputObjectTypeWrapper.TILEMAP:
                    return HEU_InputNode.InputObjectType.TILEMAP;
                default:
                    return HEU_InputNode.InputObjectType.UNITY_MESH;
            }
        }
    }

    // Container for each input object in this node
    [System.Serializable]
    internal class HEU_InputObjectInfo : IEquivable<HEU_InputObjectInfo>
    {
        // Gameobject containing mesh
        public GameObject _gameObject;

        // Hidden variables to serialize UI references
        [HideInInspector] public Terrain _terrainReference;
        [HideInInspector] public HEU_BoundingVolume _boundingVolumeReference;
        [HideInInspector] public Tilemap _tilemapReference;

        // The last upload transform, for diff checks
        public Matrix4x4 _syncdTransform = Matrix4x4.identity;
        public List<Matrix4x4> _syncdChildTransforms = new List<Matrix4x4>();

        // Whether to use the transform offset
        [FormerlySerializedAs("_useTransformOverride")]
        public bool _useTransformOffset = false;

        // Transform offset
        [FormerlySerializedAs("_translateOverride")]
        public Vector3 _translateOffset = Vector3.zero;

        [FormerlySerializedAs("_rotateOverride")]
        public Vector3 _rotateOffset = Vector3.zero;

        [FormerlySerializedAs("_scaleOverride")]
        public Vector3 _scaleOffset = Vector3.one;

        public System.Type _inputInterfaceType;

        public void CopyTo(HEU_InputObjectInfo destObject)
        {
            destObject._gameObject = _gameObject;
            destObject._terrainReference = _terrainReference;
            destObject._boundingVolumeReference = _boundingVolumeReference;
            destObject._tilemapReference = _tilemapReference;
            destObject._syncdTransform = _syncdTransform;
            destObject._useTransformOffset = _useTransformOffset;
            destObject._translateOffset = _translateOffset;
            destObject._rotateOffset = _rotateOffset;
            destObject._scaleOffset = _scaleOffset;
            destObject._inputInterfaceType = _inputInterfaceType;
        }

        internal void SetReferencesFromGameObject()
        {
            if (_gameObject != null)
            {
                _terrainReference = _gameObject.GetComponent<Terrain>();
                _tilemapReference = _gameObject.GetComponent<Tilemap>();
                _boundingVolumeReference = _gameObject.GetComponent<HEU_BoundingVolume>();
            }
        }

        public bool IsEquivalentTo(HEU_InputObjectInfo other)
        {
            bool bResult = true;

            string header = "HEU_InputObjectInfo";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            HEU_TestHelpers.AssertTrueLogEquivalent(this._syncdTransform, other._syncdTransform, ref bResult, header, "_syncedTransform");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._useTransformOffset, other._useTransformOffset, ref bResult, header, "_useTransformOffset");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._translateOffset, other._translateOffset, ref bResult, header, "_translateOffset");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._rotateOffset, other._rotateOffset, ref bResult, header, "_rotateOffset");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._scaleOffset, other._scaleOffset, ref bResult, header, "_scaleOffset");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._inputInterfaceType, other._inputInterfaceType, ref bResult, header, "_inputInterfaceType");

            return bResult;
        }
    }

    [System.Serializable]
    internal class HEU_InputHDAInfo : IEquivable<HEU_InputHDAInfo>
    {
        // The HDA gameobject that needs to be connected
        public GameObject _pendingGO;

        // The HDA gameobject that has been connected
        public GameObject _connectedGO;

        // The ID of the connected HDA
        public HAPI_NodeId _connectedInputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

        public HAPI_NodeId _connectedMergeNodeID = HEU_Defines.HEU_INVALID_NODE_ID;

        public void CopyTo(HEU_InputHDAInfo destInfo)
        {
            destInfo._pendingGO = _pendingGO;
            destInfo._connectedGO = _connectedGO;

            destInfo._connectedInputNodeID = HEU_Defines.HEU_INVALID_NODE_ID;
        }

        public bool IsEquivalentTo(HEU_InputHDAInfo other)
        {
            bool bResult = true;

            string header = "HEU_InputHDAInfo";

            if (other == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }

            // HEU_TestHelpers.AssertTrueLogEquivalent(this._pendingGO, other._pendingGO, ref bResult, header, "_pendingGO");

            // HEU_TestHelpers.AssertTrueLogEquivalent(this._connectedGO, other._connectedGO, ref bResult, header, "_connectedGO");

            return bResult;
        }
    }

    // UI cache container
    public class HEU_InputNodeUICache
    {
#if UNITY_EDITOR
        public UnityEditor.SerializedObject _inputNodeSerializedObject;

        public UnityEditor.SerializedProperty _inputObjectTypeProperty;

        public UnityEditor.SerializedProperty _keepWorldTransformProperty;
        public UnityEditor.SerializedProperty _packBeforeMergeProperty;

        public UnityEditor.SerializedProperty _inputObjectsProperty;

        public UnityEditor.SerializedProperty _inputAssetsProperty;
        public UnityEditor.SerializedProperty _meshSettingsProperty;
        public UnityEditor.SerializedProperty _tilemapSettingsProperty;

        public UnityEditor.SerializedProperty _splineSettingsProperty;

#endif

        public class HEU_InputObjectUICache
        {
#if UNITY_EDITOR
            public UnityEditor.SerializedProperty _gameObjectProperty;
            public UnityEditor.SerializedProperty _transformOffsetProperty;
            public UnityEditor.SerializedProperty _translateProperty;
            public UnityEditor.SerializedProperty _rotateProperty;
            public UnityEditor.SerializedProperty _scaleProperty;
#endif
        }

        public List<HEU_InputObjectUICache> _inputObjectCache = new List<HEU_InputObjectUICache>();

        public class HEU_InputAssetUICache
        {
#if UNITY_EDITOR
            public UnityEditor.SerializedProperty _gameObjectProperty;
#endif
        }

        public List<HEU_InputAssetUICache> _inputAssetCache = new List<HEU_InputAssetUICache>();
    }
} // HoudiniEngineUnity
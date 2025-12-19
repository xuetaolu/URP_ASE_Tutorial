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

#if (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX)
#define HOUDINIENGINEUNITY_ENABLED
#endif

// Uncomment to profile
//#define HEU_PROFILER_ON

using System.Text;
using System.Collections.Generic;
using System.Collections;
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
    using HAPI_AssetLibraryId = System.Int32;
    using HAPI_StringHandle = System.Int32;
    using HAPI_ErrorCodeBits = System.Int32;
    using HAPI_NodeTypeBits = System.Int32;
    using HAPI_NodeFlagsBits = System.Int32;
    using HAPI_ParmId = System.Int32;
    using HAPI_PartId = System.Int32;

    /// <summary>
    /// Represents a Houdini Digital Asset in Unity.
    /// Contains object nodes, geo nodes, and parts for an HDA.
    /// Contains HDA's parameters.
    /// Load, (re)cook, and bake out asset.
    /// Can (and should) be excluded from builds & runtime.
    /// </summary>
    [ExecuteInEditMode] // OnEnable/OnDisable for registering for tick
    public sealed class HEU_HoudiniAsset : MonoBehaviour, IHEU_HoudiniAsset, IEquivable<HEU_HoudiniAsset>
    {
        //	ASSET DATA ------------------------------------------------------------------------------------------------

        internal enum HEU_AssetType
        {
            TYPE_INVALID = 0,
            TYPE_HDA,
            TYPE_CURVE,
            TYPE_INPUT
        }

        // PUBLIC FIELDS ==============================================================

        // Get/Set options

        /// <inheritdoc />
        public bool LoadAssetFromMemory
        {
            get { return _loadAssetFromMemory; }
            set { _loadAssetFromMemory = value; }
        }

        /// <inheritdoc />
        public bool AlwaysOverwriteOnLoad
        {
            get { return _alwaysOverwriteOnLoad; }
            set { _alwaysOverwriteOnLoad = value; }
        }

        /// <inheritdoc />
        public bool GenerateUVs
        {
            get { return _generateUVs; }
            set { _generateUVs = value; }
        }

        /// <inheritdoc />
        public bool GenerateTangents
        {
            get { return _generateTangents; }
            set { _generateTangents = value; }
        }

        /// <inheritdoc />
        public bool GenerateNormals
        {
            get { return _generateNormals; }
            set { _generateNormals = value; }
        }

        /// <inheritdoc />
        public bool PushTransformToHoudini
        {
            get { return _pushTransformToHoudini; }
            set { _pushTransformToHoudini = value; }
        }

        /// <inheritdoc />
        public bool TransformChangeTriggersCooks
        {
            get { return _transformChangeTriggersCooks; }
            set { _transformChangeTriggersCooks = value; }
        }

        /// <inheritdoc />
        public bool CookingTriggersDownCooks
        {
            get { return _cookingTriggersDownCooks; }
            set { _cookingTriggersDownCooks = value; }
        }

        /// <inheritdoc />
        public bool AutoCookOnParameterChange
        {
            get { return _autoCookOnParameterChange; }
            set { _autoCookOnParameterChange = value; }
        }

        /// <inheritdoc />
        public bool IgnoreNonDisplayNodes
        {
            get { return _ignoreNonDisplayNodes; }
            set { _ignoreNonDisplayNodes = value; }
        }

        /// <inheritdoc />
        public bool UseOutputNodes
        {
            get { return _useOutputNodes; }
            set { _useOutputNodes = value; }
        }

        /// <inheritdoc />
        public bool GenerateMeshUsingPoints
        {
            get { return _generateMeshUsingPoints; }
            set { _generateMeshUsingPoints = value; }
        }

        /// <inheritdoc />
        public bool UseLODGroups
        {
            get { return _useLODGroups; }
            set { _useLODGroups = value; }
        }

        /// <inheritdoc />
        public bool SplitGeosByGroup
        {
            get { return _splitGeosByGroup; }
            set { _splitGeosByGroup = value; }
        }

        /// <inheritdoc />
        public bool SessionSyncAutoCook
        {
            get { return _sessionSyncAutoCook; }
            set { _sessionSyncAutoCook = value; }
        }

        /// <inheritdoc />
        public bool BakeUpdateKeepPreviousTransformValues
        {
            get { return _bakeUpdateKeepPreviousTransformValues; }
            set { _bakeUpdateKeepPreviousTransformValues = value; }
        }

        /// <inheritdoc />
        public bool PauseCooking
        {
            get { return _pauseCooking; }
            set { _pauseCooking = value; }
        }

        /// <inheritdoc />
        public bool CurveEditorEnabled
        {
            get { return _curveEditorEnabled; }
            set { _curveEditorEnabled = value; }
        }

        /// <inheritdoc />
        public HEU_CurveDrawCollisionWrapper CurveDrawCollision
        {
            get { return CurveDrawCollision_InternalToWrapper(_curveDrawCollision); }
            set { _curveDrawCollision = CurveDrawCollision_WrapperToInternal(value); }
        }

        /// <inheritdoc />
        public LayerMask CurveDrawLayerMask
        {
            get { return _curveDrawLayerMask; }
            set { _curveDrawLayerMask = value; }
        }

        /// <inheritdoc />
        public float CurveProjectMaxDistance
        {
            get { return _curveProjectMaxDistance; }
            set { _curveProjectMaxDistance = value; }
        }

        /// <inheritdoc />
        public Vector3 CurveProjectDirection
        {
            get { return _curveProjectDirection; }
            set { _curveProjectDirection = value; }
        }

        /// <inheritdoc />
        public bool CurveProjectDirectionToView
        {
            get { return _curveProjectDirectionToView; }
            set { _curveProjectDirectionToView = value; }
        }

        /// <inheritdoc />
        public bool CurveDisableScaleRotation
        {
            get { return _curveDisableScaleRotation; }
            set { _curveDisableScaleRotation = value; }
        }

        /// <inheritdoc />
        public bool CurveFrameSelectedNodes
        {
            get { return _curveFrameSelectedNodes; }
            set { _curveFrameSelectedNodes = value; }
        }

        /// <inheritdoc />
        public float CurveFrameSelectedNodeDistance
        {
            get { return _curveFrameSelectedNodeDistance; }
            set { _curveFrameSelectedNodeDistance = value; }
        }

        /// <inheritdoc />
        public bool HandlesEnabled
        {
            get { return _handlesEnabled; }
            set { _handlesEnabled = value; }
        }

        /// <inheritdoc />
        public bool EditableNodesToolsEnabled
        {
            get { return _editableNodesToolsEnabled; }
            set { _editableNodesToolsEnabled = value; }
        }


        // Read only ====

        /// <inheritdoc />
        public HEU_AssetTypeWrapper AssetType
        {
            get { return AssetType_InternalToWrapper(_assetType); }
        }

        /// <inheritdoc />
        public HAPI_AssetInfo AssetInfo
        {
            get { return _assetInfo; }
        }

        /// <inheritdoc />
        public HAPI_NodeInfo NodeInfo
        {
            get { return _nodeInfo; }
        }

        /// <inheritdoc />
        public string AssetName
        {
            get { return _assetName; }
        }

        /// <inheritdoc />
        public string AssetOpName
        {
            get { return _assetOpName; }
        }

        /// <inheritdoc />
        public string AssetHelp
        {
            get { return _assetHelp; }
        }

        /// <inheritdoc />
        public HAPI_NodeId AssetID
        {
            get { return _assetID; }
        }

        /// <inheritdoc />
        public string AssetPath
        {
            get { return _assetPath; }
        }

        /// <inheritdoc />
        public GameObject OwnerGameObject
        {
            get { return this.gameObject; }
        }

        /// <inheritdoc />
        public GameObject RootGameObject
        {
            get { return _rootGameObject; }
        }

        /// <inheritdoc />
        public List<HEU_MaterialData> MaterialCache
        {
            get { return _materialCache; }
        }

        /// <inheritdoc />
        public HEU_Parameters Parameters
        {
            get { return _parameters; }
        }

        /// <inheritdoc />
        public string AssetCacheFolder
        {
            get { return _assetCacheFolderPath; }
        }

        /// <inheritdoc />
        public string[] SubassetNames
        {
            get { return _subassetNames; }
        }

        /// <inheritdoc />
        public int SelectedSubassetIndex
        {
            get { return _selectedSubassetIndex; }
        }

        /// <inheritdoc />
        public HEU_AssetCookStatusWrapper CookStatus
        {
            get { return AssetCookStatus_InternalToWrappper(_cookStatus); }
        }

        public HEU_AssetCookResultWrapper LastCookResult
        {
            get { return AssetCookResult_InternalToWrapper(_lastCookResult); }
        }

        /// <inheritdoc />
        public long SessionID
        {
            get { return _sessionID; }
        }

        /// <inheritdoc />
        public List<HEU_Curve> Curves
        {
            get { return _curves; }
        }

        /// <inheritdoc />
        public List<HEU_InputNode> InputNodes
        {
            get { return _inputNodes; }
        }

        /// <inheritdoc />
        public List<HEU_VolumeCache> VolumeCaches
        {
            get { return _volumeCaches; }
        }

        // ASSET EVENTS ----

        /// <inheritdoc />
        public HEU_ReloadDataEvent ReloadDataEvent
        {
            get { return _reloadDataEvent; }
        }

        /// <inheritdoc />
        public HEU_CookedDataEvent CookedDataEvent
        {
            get { return _cookedDataEvent; }
        }

        /// <inheritdoc />
        public HEU_BakedDataEvent BakedDataEvent
        {
            get { return _bakedDataEvent; }
        }

        /// <inheritdoc />
        public HEU_PreAssetEvent PreAssetEvent
        {
            get { return _preAssetEvent; }
        }

        // =====================================================================

        // PRIVATE MEMBERS ===================================================

        [SerializeField] private HEU_AssetType _assetType;

        internal HEU_AssetType AssetTypeInternal
        {
            get { return _assetType; }
        }

        [SerializeField] private HAPI_AssetInfo _assetInfo;

        [SerializeField] private HAPI_NodeInfo _nodeInfo;

        [SerializeField] private string _assetName;

        [SerializeField] private string _assetOpName;

        [SerializeField] private string _assetHelp;

        [System.NonSerialized] private HAPI_NodeId _assetID = HEU_Defines.HEU_INVALID_NODE_ID;

        [SerializeField] private string _assetPath;

        // If true, this asset file will be loaded into memory first
        // in Unity, then HARS will load it from memory buffer.
        [SerializeField] private bool _loadAssetFromMemory;

        // If true, always overwrite the existing HDA file in a call to LoadAssetLibraryFromFile (Without showing dialog)
        [SerializeField] private bool _alwaysOverwriteOnLoad;

#pragma warning disable 0414
        [SerializeField] private UnityEngine.Object _assetFileObject;
#pragma warning restore 0414

        [SerializeField] private List<HEU_ObjectNode> _objectNodes;

        [SerializeField] private GameObject _rootGameObject;

        [SerializeField] private List<HEU_MaterialData> _materialCache;

        [SerializeField] private HEU_Parameters _parameters;

        [SerializeField] private Matrix4x4 _lastSyncedTransformMatrix;

        [SerializeField] private List<Matrix4x4> _lastSyncedChildTransformMatrices;

        // Location of this asset's cache folder for storing persistant data
        [SerializeField] private string _assetCacheFolderPath;

        [SerializeField] private string[] _subassetNames;

        [SerializeField] private int _selectedSubassetIndex;

        // Unserialized asset preset used when re-building asset to reapply parameter values
        private HEU_AssetPreset _savedAssetPreset;

        // Pending presets to apply after a Recook, which is invoked after a Rebuild
        private HEU_RecookPreset _recookPreset;

        // Keeps track of total cooks for this asset in order to check if need to update from Houdini
        [SerializeField] private int _totalCookCount;


        // BUILD & COOK -----------------------------------------------------------------------------------------------

        internal enum AssetBuildAction
        {
            NONE,
            RELOAD,
            COOK,
            INVALID,
            STRIP_HEDATA,
            DUPLICATE,
            RESET_PARAMS
        }

        [SerializeField] private AssetBuildAction _requestBuildAction;

#pragma warning disable 0414
        [SerializeField] private bool _checkParameterChangeForCook;

        [SerializeField] private bool _skipCookCheck;

        [SerializeField] private bool _uploadParameters;

        [SerializeField] private bool _forceUploadInputs;

        [SerializeField] private bool _upstreamCookChanged;

        internal enum AssetCookStatus
        {
            NONE,
            COOKING,
            POSTCOOK,
            LOADING,
            POSTLOAD,
            PRELOAD,
            SELECT_SUBASSET
        }

        [SerializeField] private AssetCookStatus _cookStatus;

        internal AssetCookStatus GetCookStatus()
        {
            return _cookStatus;
        }


#pragma warning restore 0414

        internal enum AssetCookResult
        {
            NONE,
            SUCCESS,
            ERRORED
        }

        [SerializeField] private AssetCookResult _lastCookResult;

        internal AssetCookResult GetLastCookResult()
        {
            return _lastCookResult;
        }

        [SerializeField] private bool _isCookingAssetReloaded;

        // Force everything to be updated without checking if changed
        private bool _bForceUpdate;

        [SerializeField] private long _sessionID = HEU_SessionData.INVALID_SESSION_ID;

        [SerializeField] internal bool WarnedPrefabNotSupported { get; set; }

        // UI TOGGLES -------------------------------------------------------------------------------------------------

        // Disable the warning for unused variables. We're accessing these as SerializedProperty.
#pragma warning disable 0414


        // By default, this component's serialized properties on custom inspector is locked out
        // to reduce user tampering.
        [SerializeField] private bool _uiLocked = true;

        [SerializeField] private bool _showHDAOptions = false;

        [SerializeField] private bool _showGenerateSection = true;

        [SerializeField] private bool _showBakeSection = false;

        [SerializeField] private bool _showEventsSection = false;

        [SerializeField] private bool _showCurvesSection = false;

        [SerializeField] private bool _showInputNodesSection = false;

        [SerializeField] private bool _showToolsSection = false;

        [SerializeField] private bool _showTerrainSection = false;

        [SerializeField] private HEU_InstanceInputUIState _instanceInputUIState;

        [SerializeField]
        internal HEU_InstanceInputUIState InstanceInputUIState
        {
            get { return _instanceInputUIState; }
            set { _instanceInputUIState = value; }
        }

#pragma warning restore 0414

        // ASSET EVENTS -----------------------------------------------------------------------------------------------

        [SerializeField] private HEU_ReloadDataEvent _reloadDataEvent = new HEU_ReloadDataEvent();

        [SerializeField] private HEU_CookedDataEvent _cookedDataEvent = new HEU_CookedDataEvent();

        [SerializeField] private HEU_BakedDataEvent _bakedDataEvent = new HEU_BakedDataEvent();

        [SerializeField] private HEU_PreAssetEvent _preAssetEvent = new HEU_PreAssetEvent();

        // Delegate for Editor window to hook into for callback when needing updating
        public delegate void UpdateUIDelegate();

        [SerializeField] private UpdateUIDelegate _refreshUIDelegate;

        internal UpdateUIDelegate RefreshUIDelegate
        {
            get { return _refreshUIDelegate; }
            set { _refreshUIDelegate = value; }
        }


        // CONNECTIONS ------------------------------------------------------------------------------------------------

        private HEU_CookedDataEvent _downstreamConnectionCookedEvent = new HEU_CookedDataEvent();

        // HDA OPTIONS ------------------------------------------------------------------------------------------------

        [SerializeField] private bool _generateUVs = false;

        [SerializeField] private bool _generateTangents = true;

        [SerializeField] private bool _generateNormals = true;

        [SerializeField] private bool _pushTransformToHoudini = true;

        [SerializeField] private bool _transformChangeTriggersCooks = false;

        [SerializeField] private bool _cookingTriggersDownCooks = true;

        [SerializeField] private bool _autoCookOnParameterChange = true;

        [SerializeField] private bool _ignoreNonDisplayNodes = false;

        [SerializeField] private bool _useOutputNodes = true;

        [SerializeField] private bool _generateMeshUsingPoints = false;

        [SerializeField] private bool _useLODGroups = true;

        [SerializeField] private bool _splitGeosByGroup = false;


        [SerializeField] private bool _sessionSyncAutoCook = true;


        [SerializeField] private bool _bakeUpdateKeepPreviousTransformValues = false;


        // If false, pauses all cooking on this HDA until set back to true. Meant for unit testing use.
       private bool _pauseCooking = false;

        // CURVES -----------------------------------------------------------------------------------------------------

        // Toggle curve editing tool in Scene view
        [SerializeField] private bool _curveEditorEnabled = true;

        [SerializeField] private List<HEU_Curve> _curves;

        [SerializeField] private HEU_Curve.CurveDrawCollision _curveDrawCollision;

        internal HEU_Curve.CurveDrawCollision CurveDrawCollisionInternal
        {
            get { return _curveDrawCollision; }
        }

        [SerializeField] private List<Collider> _curveDrawColliders = new List<Collider>();

        internal List<Collider> GetCurveDrawColliders()
        {
            return _curveDrawColliders;
        }

        [SerializeField] private LayerMask _curveDrawLayerMask;

        internal LayerMask GetCurveDrawLayerMask()
        {
            return _curveDrawLayerMask;
        }

        internal void SetCurveDrawLayerMask(LayerMask mask)
        {
            _curveDrawLayerMask = mask;
        }

        [SerializeField] private float _curveProjectMaxDistance = 1000f;

        [SerializeField] private Vector3 _curveProjectDirection = Vector3.down;

        [SerializeField] private bool _curveProjectDirectionToView = true;

        [SerializeField] private bool _curveDisableScaleRotation = true;

        [SerializeField] private bool _curveFrameSelectedNodes = true;

        [SerializeField] private float _curveFrameSelectedNodeDistance = 20f;

        // INPUT NODES ------------------------------------------------------------------------------------------------

        [SerializeField] private List<HEU_InputNode> _inputNodes;

        // HANDLES ----------------------------------------------------------------------------------------------------

        [SerializeField] private List<HEU_Handle> _handles;

        internal List<HEU_Handle> Handles
        {
            get { return _handles; }
        }

        [SerializeField] private bool _handlesEnabled = true;

        // TERRAIN ----------------------------------------------------------------------------------------------------

        [SerializeField] private List<HEU_VolumeCache> _volumeCaches;

        // TOOLS ------------------------------------------------------------------------------------------------------

        [SerializeField] private List<HEU_AttributesStore> _attributeStores;

        internal List<HEU_AttributesStore> AttributeStores
        {
            get { return _attributeStores; }
        }

        [SerializeField] private bool _editableNodesToolsEnabled = false;


        [SerializeField] private HEU_ToolsInfo _toolsInfo;

        internal HEU_ToolsInfo ToolsInfo
        {
            get { return _toolsInfo; }
        }


        [SerializeField, HideInInspector] private HEU_AssetSerializedMetaData _serializedMetaData;

        internal HEU_AssetSerializedMetaData SerializedMetaData
        {
            get { return _serializedMetaData; }
        }

        private bool _pendingAutoCookOnMouseRelease;

        internal bool PendingAutoCookOnMouseRelease
        {
            get { return _pendingAutoCookOnMouseRelease; }
            set { _pendingAutoCookOnMouseRelease = value; }
        }

        // Enum to guess how Unity instantiated this object (because Unity doesn't provide instantiation callbacks)
        private enum AssetInstantiationMethod
        {
            DEFAULT,
            DUPLICATED,
            UNDO
        };

        // PROFILE ----------------------------------------------------------------------------------------------------

#if HEU_PROFILER_ON
	private float _cookStartTime;
	private float _hapiCookEndTime;
	private float _postCookStartTime;
#endif

        // PUBLIC FUNCTIONS ================================================================

        /// <inheritdoc />
        public bool RequestCook(bool bCheckParametersChanged = true, bool bAsync = false, bool bSkipCookCheck = true, bool bUploadParameters = true)
        {
#if HOUDINIENGINEUNITY_ENABLED
            //HEU_Logger.Log(HEU_Defines.HEU_NAME + ": Requesting Cook");

            if (!HEU_PluginSettings.CookDisabledGameObjects && !this.gameObject.activeInHierarchy)
            {
                HEU_Logger.LogWarning("Houdini Asset: " + this.RootGameObject.name +
                                      " Skipped cooking due to being disabled. Enable and recook manually to resync!");
                return false;
            }

            if (bAsync)
            {
                // We don't want to override Reload or Invalid actions, so
                // for now, only set request if no other pending build actions.
                if (_requestBuildAction == AssetBuildAction.NONE)
                {
                    _requestBuildAction = AssetBuildAction.COOK;
                }

                // This could be an update on the cook settings
                if (_requestBuildAction == AssetBuildAction.COOK)
                {
                    _checkParameterChangeForCook = bCheckParametersChanged;
                    _skipCookCheck = bSkipCookCheck;
                    _uploadParameters = bUploadParameters;
                }
                else
                {
                    HEU_Logger.LogWarning(HEU_Defines.HEU_NAME + ": Asset busy. Unable to start cooking!");
                }
            }
            else
            {
                if (_cookStatus == AssetCookStatus.NONE || _cookStatus == AssetCookStatus.POSTLOAD)
                {
                    RecookBlocking(bCheckParametersChanged, bSkipCookCheck,
                        bUploadParameters, bUploadParameterPreset: false,
                        bForceUploadInputs: false, bCookingSessionSync: false);
                }
                else
                {
                    HEU_Logger.LogWarningFormat(HEU_Defines.HEU_NAME + ": Houdini Engine: Asset busy (cook status: {0}). Unable to start cooking!",
                        _cookStatus);
                }
            }

#endif
            return true;
        }

        /// <inheritdoc />
        public bool RequestReload(bool bAsync = false)
        {
#if HOUDINIENGINEUNITY_ENABLED
            if (!HEU_PluginSettings.CookDisabledGameObjects && !this.gameObject.activeInHierarchy)
            {
                HEU_Logger.LogWarning("Houdini Asset: " + this.RootGameObject.name +
                                      " Skipped cooking due to being disabled. Enable and recook manually to resync!");
                return false;
            }

            if (bAsync)
            {
                _requestBuildAction = AssetBuildAction.RELOAD;
            }
            else
            {
                ClearBuildRequest();
                SetCookStatus(AssetCookStatus.PRELOAD, AssetCookResult.NONE);
                ProcessRebuild(true, -1);
            }

#endif
            return true;
        }

        /// <inheritdoc />
        public bool RequestResetParameters(bool bAsync = false)
        {
#if HOUDINIENGINEUNITY_ENABLED
            if (bAsync)
            {
                _requestBuildAction = AssetBuildAction.RESET_PARAMS;
            }
            else
            {
                ClearBuildRequest();
                SetCookStatus(AssetCookStatus.PRELOAD, AssetCookResult.NONE);
                ResetParametersToDefault();
                ProcessRebuild(true, -1);
            }
#endif
            return true;
        }

        /// <inheritdoc />
        public GameObject DuplicateAsset(GameObject newRootGameObject = null)
        {
            string goName = _rootGameObject.name + "_copy";

            bool bBuildAsync = false;
            HEU_SessionBase session = GetAssetSession(true);
            Transform thisParentTransform = _rootGameObject.transform.parent;

            if (_assetType == HEU_AssetType.TYPE_HDA)
            {
                newRootGameObject = HEU_HAPIUtility.InstantiateHDA(_assetPath, _rootGameObject.transform.position, session, bBuildAsync,
                    bAlwaysOverwriteOnLoad: false, rootGO: newRootGameObject);
            }
            else if (_assetType == HEU_AssetType.TYPE_CURVE)
            {
                newRootGameObject = HEU_HAPIUtility.CreateNewCurveAsset(parentTransform: thisParentTransform, session: session,
                    bBuildAsync: bBuildAsync, rootGO: newRootGameObject);
            }
            else if (_assetType == HEU_AssetType.TYPE_INPUT)
            {
                newRootGameObject = HEU_HAPIUtility.CreateNewInputAsset(parentTransform: thisParentTransform, session: session,
                    bBuildAsync: bBuildAsync, rootGO: newRootGameObject);
            }
            else
            {
                HEU_Logger.LogErrorFormat("Unsupported asset type {0} for duplication.", _assetType);
                return null;
            }

            HEU_HoudiniAssetRoot newRoot = newRootGameObject.GetComponent<HEU_HoudiniAssetRoot>();
            HEU_HoudiniAsset newAsset = newRoot._houdiniAsset;

            Transform newRootTransform = newRootGameObject.transform;
            newRootTransform.parent = thisParentTransform;
            newRootTransform.localPosition = _rootGameObject.transform.localPosition;
            newRootTransform.localRotation = _rootGameObject.transform.localRotation;
            newRootTransform.localScale = _rootGameObject.transform.localScale;

            this.CopyPropertiesTo(newAsset);

            // Select it
            HEU_EditorUtility.SelectObject(newRootGameObject);

            return newRootGameObject;
        }

        /// <inheritdoc />
        public bool DeleteAllGeneratedData(bool bIsRebuild = false)
        {
            if (_assetID != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                DeleteSessionDataOnly();
                _assetID = HEU_Defines.HEU_INVALID_NODE_ID;
            }

            // Clean up object nodes which in turns cleans up meshes.
            if (_objectNodes != null)
            {
                for (int i = 0; i < _objectNodes.Count; ++i)
                {
                    if (_objectNodes[i] != null)
                    {
                        _objectNodes[i].DestroyAllData(bIsRebuild);
                        HEU_GeneralUtility.DestroyImmediate(_objectNodes[i]);
                    }
                }

                _objectNodes.Clear();
            }

            // The materials for this asset will be deleted when we delete the asset cache.
            // So we'll just clear the material cache without actually deleting them.
            ClearMaterialCache();

            // Clear out connection callbacks using parameter input nodes
            ClearAllUpstreamConnections();

            if (_parameters != null)
            {
                _parameters.CleanUp();
                _parameters = null;
            }

            CleanUpInputNodes();

            CleanUpHandles();

            // Delete children objects just in case.
            // Don't delete children if part of a prefab. 
            bool isPrefab = HEU_EditorUtility.IsPrefabAsset(gameObject);
            if (!isPrefab)
            {
                Transform[] transforms = this.GetComponentsInChildren<Transform>();
                foreach (Transform trans in transforms)
                {
                    if (trans != this.transform)
                    {
                        DestroyImmediate(trans.gameObject);
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        public GameObject BakeToNewPrefab(string destinationPrefabPath = null)
        {
            if (_preAssetEvent != null)
            {
                _preAssetEvent.Invoke(new HEU_PreAssetEventData(this, HEU_AssetEventType.BAKE_NEW));
            }

            // This creates a temporary clone of the asset without the HDA data
            // in the scene, then creates a prefab of the cloned object.

            string bakedAssetPath = null;
            if (!string.IsNullOrEmpty(destinationPrefabPath))
            {
                char[] trimChars = { '/', '\\' };
                bakedAssetPath = destinationPrefabPath.TrimEnd(trimChars);
            }

            bool bWriteMeshesToAssetDatabase = true;
            bool bReconnectPrefabInstances = false;
            GameObject newClonedRoot = CloneAssetWithoutHDA(ref bakedAssetPath, bWriteMeshesToAssetDatabase, bReconnectPrefabInstances);
            if (newClonedRoot != null)
            {
                try
                {
                    if (string.IsNullOrEmpty(bakedAssetPath))
                    {
                        // Need to create the baked folder to store the prefab
                        bakedAssetPath = HEU_AssetDatabase.CreateUniqueBakePath(_assetName);
                    }

                    string prefabPath = HEU_AssetDatabase.AppendPrefabPath(bakedAssetPath, _assetName);
                    GameObject prefabGO = HEU_EditorUtility.SaveAsPrefabAsset(prefabPath, newClonedRoot);
                    if (prefabGO != null)
                    {
                        HEU_EditorUtility.SelectObject(prefabGO);

                        InvokeBakedEvent(true, new List<GameObject>() { prefabGO }, true);

                        HEU_Logger.LogFormat("Exported prefab to {0}", bakedAssetPath);
                    }

                    return prefabGO;
                }
                finally
                {
                    // Don't need the new object anymore since its just prefab that's required
                    HEU_GeneralUtility.DestroyImmediate(newClonedRoot);
                }
            }

            return null;
        }

        /// <inheritdoc />
        public GameObject BakeToNewStandalone()
        {
            if (_preAssetEvent != null)
            {
                _preAssetEvent.Invoke(new HEU_PreAssetEventData(this, HEU_AssetEventType.BAKE_NEW));
            }

            string bakedAssetPath = null;

            // Make sure to write mesh to database because otherwise if user tries to make prefab after, it fails to create mesh.
            bool bWriteMeshesToAssetDatabase = true;
            bool bReconnectPrefabInstances = true;

            GameObject newClonedRoot = CloneAssetWithoutHDA(ref bakedAssetPath, bWriteMeshesToAssetDatabase, bReconnectPrefabInstances);
            if (newClonedRoot != null)
            {
                HEU_EditorUtility.SelectObject(newClonedRoot);

                InvokeBakedEvent(true, new List<GameObject>() { newClonedRoot }, true);
            }

            return newClonedRoot;
        }

        /// <inheritdoc />
        public bool BakeToExistingPrefab(GameObject bakeTargetGO)
        {
            if (!HEU_EditorUtility.IsPrefabAsset(bakeTargetGO))
            {
                HEU_Logger.LogErrorFormat("Unable to bake to existing prefab as specified object is not a prefab asset!");
                return false;
            }

            if (bakeTargetGO == this.gameObject || bakeTargetGO.GetComponent<HEU_HoudiniAssetRoot>() != null)
            {
                HEU_Logger.LogErrorFormat("Baking to a HoudiniAssetRoot gameobject is not supported!");
                return false;
            }

            if (_preAssetEvent != null)
            {
                _preAssetEvent.Invoke(new HEU_PreAssetEventData(this, HEU_AssetEventType.BAKE_UPDATE));
            }

            // Since the prefab would have persistent files on disk, we'll need to get
            // the existing prefab's asset folder, and delete relevant subfolders
            // such as: Materials, Textures, Meshes
            string existingPrefabFolder = HEU_AssetDatabase.GetAssetPath(bakeTargetGO);
            if (!string.IsNullOrEmpty(existingPrefabFolder))
            {
                existingPrefabFolder = HEU_Platform.GetFolderPath(existingPrefabFolder);
                existingPrefabFolder = HEU_Platform.TrimLastDirectorySeparator(existingPrefabFolder);

                string[] subFolders = HEU_AssetDatabase.GetAssetSubFolders();
                foreach (string subfolder in subFolders)
                {
                    string folderPath = HEU_Platform.BuildPath(existingPrefabFolder, subfolder);
                    HEU_AssetDatabase.DeleteAssetCacheFolder(folderPath);
                }
            }

            // Replace the specified prefab with a new cloned gameobject
            string bakedAssetPath = existingPrefabFolder;
            bool bWriteMeshesToAssetDatabase = true;
            bool bReconnectPrefabInstances = false;

            List<TransformData> previousTransformValues = null;

            if (_bakeUpdateKeepPreviousTransformValues)
            {
                previousTransformValues = new List<TransformData>();
                List<Transform> previousTransforms = HEU_GeneralUtility.GetLODTransforms(bakeTargetGO);
                previousTransforms.ForEach((Transform trans) => { previousTransformValues.Add(new TransformData(trans)); });
            }

            GameObject newClonedRoot = CloneAssetWithoutHDA(ref bakedAssetPath, bWriteMeshesToAssetDatabase, bReconnectPrefabInstances);
            if (newClonedRoot != null)
            {
                if (previousTransformValues != null)
                {
                    HEU_GeneralUtility.SetLODTransformValues(newClonedRoot, previousTransformValues);
                }

                try
                {
                    if (string.IsNullOrEmpty(bakedAssetPath))
                    {
                        // Need to create the baked folder to store the prefab
                        bakedAssetPath = HEU_AssetDatabase.CreateUniqueBakePath(_assetName);
                    }

                    // Note using ReplacePrefabOptions.ReplaceNameBased will keep local transform values and other changes on instances.
                    HEU_EditorUtility.ReplacePrefab(newClonedRoot, bakeTargetGO, HEU_EditorUtility.HEU_ReplacePrefabOptions.ReplaceNameBased);

                    InvokeBakedEvent(true, new List<GameObject>() { bakeTargetGO }, false);
                }
                finally
                {
                    // Don't need the new object since its just prefab that's required
                    HEU_GeneralUtility.DestroyImmediate(newClonedRoot);
                }
            }

            return true;
        }

        /// <inheritdoc />
        public bool BakeToExistingStandalone(GameObject bakeTargetGO)
        {
            if (bakeTargetGO == this.gameObject || bakeTargetGO.GetComponent<HEU_HoudiniAssetRoot>() != null)
            {
                HEU_Logger.LogErrorFormat("Baking to a HoudiniAssetRoot gameobject is not supported!");
                return false;
            }

            if (_preAssetEvent != null)
            {
                _preAssetEvent.Invoke(new HEU_PreAssetEventData(this, HEU_AssetEventType.BAKE_UPDATE));
            }

            // Step through all the game objects that need to be cloned, clean up existing properties, 
            // and copy over new ones.

            // If the target is a prefab instance, we shouldn't delete the shared resources.
            // Instead new resources will be generated.
            bool bPrefabInstance = HEU_EditorUtility.IsPrefabInstance(bakeTargetGO);
            bool bDontDeletePersistantResources = bPrefabInstance;

            bool bWriteMeshesToAssetDatabase = true;
            bool bDeleteExistingComponents = true;
            bool bReconnectPrefabInstances = true;
            bool bKeepPreviousTransformValues = _bakeUpdateKeepPreviousTransformValues;

            UnityEngine.Object targetAssetDBObject = null;

            // Need to get raw OP name otherwise duplicates may mess up the naming
            string rawOpName = HEU_GeneralUtility.GetRawOperatorName(_assetOpName);
            string assetDBObjectFileName = HEU_AssetDatabase.AppendMeshesAssetFileName(rawOpName);

            List<HEU_PartData> clonableParts = new List<HEU_PartData>();
            GetClonableParts(clonableParts);
            if (clonableParts.Count == 0)
            {
                HEU_Logger.LogFormat("Empty bake output. Not updating existing target gameobject as that would mean destroying the it.");
                return false;
            }

            // As we find meshes, we add to a map. The map helps track whether we already created
            // unique copy that can be shared.
            Dictionary<Mesh, Mesh> sourceToTargetMeshMap = new Dictionary<Mesh, Mesh>();

            // Map of materials copied for corresponding source materials (for reuse)
            Dictionary<Material, Material> sourceToCopiedMaterials = new Dictionary<Material, Material>();

            string targetAssetPath = null;

            HashSet<Material> generatedMaterials = new HashSet<Material>();
            foreach (HEU_PartData part in clonableParts)
            {
                if (part == null || part.ParentAsset == null)
                {
                    continue;
                }

                List<HEU_MaterialData> materialCache = part.ParentAsset.MaterialCache;

                foreach (HEU_MaterialData materialData in materialCache)
                {
                    if (materialData != null && (materialData._materialSource == HEU_MaterialData.Source.DEFAULT ||
                                                 materialData._materialSource == HEU_MaterialData.Source.HOUDINI))
                    {
                        generatedMaterials.Add(materialData._material);
                    }
                }
            }

            string foundParentFolder = HEU_EditorUtility.GetObjectParentFolder(bakeTargetGO, generatedMaterials);
            if (foundParentFolder != "")
            {
                targetAssetPath = foundParentFolder;
            }

            List<GameObject> outputObjects = new List<GameObject>();
            bool bBakedSuccessful = false;

            if (clonableParts.Count == 1)
            {
                // Single object

                clonableParts[0].BakePartToGameObject(bakeTargetGO, bDeleteExistingComponents, bDontDeletePersistantResources,
                    bWriteMeshesToAssetDatabase, ref targetAssetPath, sourceToTargetMeshMap, sourceToCopiedMaterials, ref targetAssetDBObject,
                    assetDBObjectFileName, bReconnectPrefabInstances, bKeepPreviousTransformValues);

                outputObjects.Add(bakeTargetGO);
                bBakedSuccessful = true;
            }
            else if (clonableParts.Count > 1)
            {
                // Multi objects
                // Leave root as is. Update each child object by matching via "name + HEU_Defines.HEU_BAKED_CLONE"

                List<GameObject> unprocessedTargetChildren = HEU_GeneralUtility.GetNonInstanceChildObjects(bakeTargetGO);

                Transform targetParentTransform = bakeTargetGO.transform;

                foreach (HEU_PartData partData in clonableParts)
                {
                    if (partData.OutputGameObject == null)
                    {
                        continue;
                    }

                    string targetGameObjectName = HEU_PartData.AppendBakedCloneName(partData.OutputGameObject.name);
                    GameObject targetObject = HEU_GeneralUtility.GetGameObjectByName(unprocessedTargetChildren, targetGameObjectName);
                    if (targetObject == null)
                    {
                        targetObject = partData.BakePartToNewGameObject(targetParentTransform, bWriteMeshesToAssetDatabase, ref targetAssetPath,
                            sourceToTargetMeshMap, sourceToCopiedMaterials, ref targetAssetDBObject, assetDBObjectFileName,
                            bReconnectPrefabInstances);
                    }
                    else
                    {
                        // Remove from target child list to avoid destroying it later when we process excess child gameobjects
                        unprocessedTargetChildren.Remove(targetObject);

                        partData.BakePartToGameObject(targetObject, bDeleteExistingComponents, bDontDeletePersistantResources,
                            bWriteMeshesToAssetDatabase, ref targetAssetPath, sourceToTargetMeshMap, sourceToCopiedMaterials, ref targetAssetDBObject,
                            assetDBObjectFileName, bReconnectPrefabInstances, bKeepPreviousTransformValues);
                    }

                    outputObjects.Add(targetObject);
                    bBakedSuccessful = true;
                }

                // Clean up any children that we haven't processed
                if (unprocessedTargetChildren.Count > 0)
                {
                    HEU_Logger.LogWarningFormat(
                        "Bake target has more children than bake output. GameObjects with names ending in {0} will be destroyed!",
                        HEU_Defines.HEU_BAKED_CLONE);

                    // Clean up any children that we haven't processed
                    HEU_GeneralUtility.DestroyBakedGameObjectsWithEndName(unprocessedTargetChildren, HEU_Defines.HEU_BAKED_CLONE);
                }
            }

            InvokeBakedEvent(bBakedSuccessful, outputObjects, false);

            return true;
        }

        /// <inheritdoc />
        public bool IsAssetValid()
        {
            if (_assetID != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                HEU_SessionBase session = GetAssetSession(false);
                if (session == null)
                {
                    return false;
                }

                return IsAssetValidInHoudini(session);
            }

            return false;
        }

        /// <inheritdoc />
        public bool GetOutputGameObjects(List<GameObject> outputObjects)
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.GetOutputGameObjects(outputObjects);
            }

            return true;
        }

        /// <inheritdoc />
        public bool GetOutput(List<HEU_GeneratedOutput> outputs)
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.GetOutput(outputs);
            }

            return true;
        }

        /// <inheritdoc />
        public HEU_Curve GetCurve(string curveName)
        {
            foreach (HEU_Curve curve in _curves)
            {
                if (curve != null && curve.CurveName.Equals(curveName))
                {
                    return curve;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public bool AddCurveDrawCollider(Collider newCollider)
        {
            if (!_curveDrawColliders.Contains(newCollider))
            {
                _curveDrawColliders.Add(newCollider);
            }

            return true;
        }

        /// <inheritdoc />
        public bool RemoveCurveDrawCollider(Collider collider)
        {
            if (_curveDrawColliders != null)
            {
                _curveDrawColliders.Remove(collider);
            }

            return true;
        }

        /// <inheritdoc />
        public bool ClearCurveDrawColliders()
        {
            if (_curveDrawColliders != null)
            {
                _curveDrawColliders.Clear();
            }

            return true;
        }

        /// <inheritdoc />
        public HEU_InputNode GetInputNode(string inputName)
        {
            foreach (HEU_InputNode node in _inputNodes)
            {
                if (node.InputName.Equals(inputName))
                {
                    return node;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public HEU_InputNode GetAssetInputNode(string inputName)
        {
            foreach (HEU_InputNode node in _inputNodes)
            {
                if (node.IsAssetInput() && node.InputName.Equals(inputName))
                {
                    return node;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public HEU_InputNode GetInputNodeByIndex(int index)
        {
            if (index >= 0 && index < _inputNodes.Count)
            {
                return _inputNodes[index];
            }

            return null;
        }

        /// <inheritdoc />
        public List<HEU_InputNode> GetNonParameterInputNodes()
        {
            // Helps rebuild - Needed to get some undo operations to not error
            bool bNeedsRebuild = false;

            List<HEU_InputNode> nodes = new List<HEU_InputNode>();
            foreach (HEU_InputNode node in _inputNodes)
            {
                if (node == null)
                {
                    bNeedsRebuild = true;
                    continue;
                }

                if (node.InputType != HEU_InputNode.InputNodeType.PARAMETER)
                {
                    nodes.Add(node);
                }
            }

            if (bNeedsRebuild)
                _inputNodes = _inputNodes.Filter(node => node != null);

            return nodes;
        }

        /// <inheritdoc />
        public int GetVolumeCacheCount()
        {
            return _volumeCaches != null ? _volumeCaches.Count : 0;
        }

        /// <inheritdoc />
        public HEU_SessionBase GetAssetSession(bool bCreateIfInvalid)
        {
            HEU_SessionBase session = HEU_SessionManager.GetSessionWithID(_sessionID);

            if ((session == null || !session.IsSessionValid()) && bCreateIfInvalid)
            {
                // Invalid session could either mean that this asset hasn't been created in any session (after a Scene load)
                // or that we aren't able to create Houdini sessions (installation/license problems).
                // To handle former case, we ask again to get us a valid (and new if none exist) session
                session = HEU_SessionManager.GetOrCreateDefaultSession();
                if (session != null && session.IsSessionValid())
                {
                    // Update this asset's session ID with the new session.
                    _sessionID = session.GetSessionData().SessionID;
                    return session;
                }
            }

            // Nullify the session if not valid so that callers don't need to check for validity themselves.
            if (session != null && !session.IsSessionValid())
            {
                session = null;
            }

            return session;
        }

        /// <inheritdoc />
        public HEU_ObjectNode GetObjectNodeByName(string objName)
        {
            int numObjects = _objectNodes.Count;
            if (numObjects == 1)
            {
                // If just 1 object, and names have same start, then its a match.
                string strippedName = System.Text.RegularExpressions.Regex.Replace(objName, @"[\d-]", string.Empty);
                if (_objectNodes[0].ObjectName.StartsWith(strippedName))
                {
                    return _objectNodes[0];
                }
            }
            else
            {
                // Multiple objects so name might match exactly
                foreach (HEU_ObjectNode objNode in _objectNodes)
                {
                    if (objNode.ObjectName.Equals(objName))
                    {
                        return objNode;
                    }
                }
            }

            return null;
        }

        /// <inheritdoc />
        public void GetOutputGeoNodes(List<HEU_GeoNode> outputGeoNodes)
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.GetOutputGeoNodes(outputGeoNodes);
            }
        }

        /// <inheritdoc />
        public HEU_PartData GetInternalHDAPartWithGameObject(GameObject outputGameObject)
        {
            HEU_PartData foundPart = null;
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                foundPart = objNode.GetHDAPartWithGameObject(outputGameObject);
                if (foundPart != null)
                {
                    return foundPart;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public void ResetParametersToDefault()
        {
            HEU_SessionBase session = GetAssetSession(true);

            if (_parameters != null)
            {
                _parameters.ResetAllToDefault(session);
            }

            List<HEU_Curve> curves = Curves;
            foreach (HEU_Curve curve in curves)
            {
                curve.ResetCurveParameters(session, this);
            }

            // Reset inputs
            foreach (HEU_InputNode inputNode in _inputNodes)
            {
                inputNode.ResetInputNode(session);
            }

            // Reset volume caches
            foreach (HEU_VolumeCache volumeCache in _volumeCaches)
            {
                volumeCache.ResetParameters();
                volumeCache.IsDirty = true;
            }

            // Note that the asset should be recooked after the parameters have been reset.
        }

        /// <inheritdoc />
        public void HideAllGeometry()
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.HideAllGeometry();
            }
        }

        /// <inheritdoc />
        public void DisableAllColliders()
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.DisableAllColliders();
            }
        }

        /// <inheritdoc />
        public HEU_MaterialData GetMaterialData(Material material)
        {
            foreach (HEU_MaterialData materialData in _materialCache)
            {
                if (materialData._material == material)
                {
                    return materialData;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public void ClearMaterialCache()
        {
            _materialCache.Clear();
        }

        /// <inheritdoc />
        public void RemoveMaterial(Material material)
        {
            HEU_MaterialData materialData = null;
            for (int i = 0; i < _materialCache.Count; ++i)
            {
                if (_materialCache[i]._material == material)
                {
                    materialData = _materialCache[i];
                    _materialCache.RemoveAt(i);

                    HEU_GeneralUtility.DestroyImmediate(materialData, false);
                    break;
                }
            }
        }


        /// <inheritdoc />
        public void ResetMaterialOverrides()
        {
            List<HEU_GeneratedOutput> outputs = new List<HEU_GeneratedOutput>();
            GetOutput(outputs);

            foreach (HEU_GeneratedOutput output in outputs)
            {
                HEU_GeneratedOutput.ResetMaterialOverrides(output);
            }
        }

        /// <inheritdoc />
        public HEU_AssetPreset GetAssetPreset(bool sceneRelativeGameObjects)
        {
            if (string.IsNullOrEmpty(_assetName))
            {
                return null;
            }

            HEU_AssetPreset assetPreset = new HEU_AssetPreset();

            assetPreset._assetOPName = _assetOpName;

            // TODO: Save asset options

            if (_parameters != null)
            {
                byte[] srcPreset = _parameters.GetPresetData();
                if (srcPreset != null && srcPreset.Length > 0)
                {
                    assetPreset._parameterPreset = new byte[srcPreset.Length];
                    System.Array.Copy(srcPreset, assetPreset._parameterPreset, srcPreset.Length);
                }
            }

            List<HEU_Curve> curves = Curves;
            foreach (HEU_Curve curve in curves)
            {
                byte[] srcPreset = curve.Parameters != null ? curve.Parameters.GetPresetData() : null;
                if (srcPreset != null && srcPreset.Length > 0)
                {
                    byte[] destPreset = new byte[srcPreset.Length];
                    System.Array.Copy(srcPreset, destPreset, destPreset.Length);

                    assetPreset._curveNames.Add(curve.CurveName);
                    assetPreset._curvePresets.Add(destPreset);
                }
            }

            foreach (HEU_InputNode inputNode in _inputNodes)
            {
                if (inputNode != null)
                {
                    HEU_InputPreset inputPreset = new HEU_InputPreset();
                    inputNode.PopulateInputPreset(inputPreset, sceneRelativeGameObjects);
                    assetPreset.inputPresets.Add(inputPreset);
                }
            }

            foreach (HEU_VolumeCache volumeCache in _volumeCaches)
            {
                if (volumeCache != null)
                {
                    HEU_VolumeCachePreset volumeCachePreset = new HEU_VolumeCachePreset();
                    volumeCache.PopulatePreset(volumeCachePreset);
                    assetPreset.volumeCachePresets.Add(volumeCachePreset);
                }
            }

            return assetPreset;
        }

        /// <inheritdoc />
        public HEU_PDGAssetLink GetOrCreatePDGAssetLink()
        {
            HEU_PDGAssetLink assetComponent = RootGameObject.GetComponent<HEU_PDGAssetLink>();
            if (assetComponent != null)
            {
                if (assetComponent.ParentAsset != this)
                    assetComponent.Setup(this);

                return assetComponent;
            }

            HEU_SessionBase session = GetAssetSession(false);
            if (session == null)
            {
                HEU_Logger.LogError("Session cannot be found!");
                return null;
            }

            if (HEU_PDGSession.IsPDGAsset(session, _assetID))
            {
                HEU_PDGAssetLink assetLink = RootGameObject.AddComponent<HEU_PDGAssetLink>();
                assetLink.Setup(this);
                return assetLink;
            }
            else
            {
                HEU_Logger.LogError("HDA does not contain a valid PDG TOP network!");
                return null;
            }
        }

        public static HEU_HoudiniAssetRoot InstantiateHDA(string filePath, bool bAsync = false, Vector3 initialPosition = new Vector3())
        {
            HEU_SessionBase session = HEU_SessionManager.GetOrCreateDefaultSession(false);
            if (session != null)
            {
                GameObject go = HEU_HAPIUtility.InstantiateHDA(filePath, initialPosition, session, bAsync);
                return go.GetComponent<HEU_HoudiniAssetRoot>();
            }

            return null;
        }

        /// <summary>
        /// In the given scene, for the given gameobject, get the HEU_PartData that created it.
        /// </summary>
        /// <param name="outputGameObject">The output gameobject associated with the part</param>
        /// <returns>Valid HEU_PartData or null if no match</returns>
        public static HEU_PartData GetSceneHDAPartWithGameObject(GameObject outputGameObject)
        {
            // The structure of an HDA inside a Unity scene should be such that
            // outputGameObject should have parent with HEU_HoudiniAssetRoot component.
            // Then get the HEU_HoudiniAsset, and find the part with the matching gameobject.

            if (outputGameObject.transform.parent != null)
            {
                GameObject parentGO = outputGameObject.transform.parent.gameObject;
                HEU_HoudiniAssetRoot assetRoot = parentGO.GetComponent<HEU_HoudiniAssetRoot>();
                if (assetRoot != null && assetRoot._houdiniAsset != null)
                {
                    return assetRoot._houdiniAsset.GetInternalHDAPartWithGameObject(outputGameObject);
                }
            }

            return null;
        }

        /// <summary>
        /// In the given scene, for the given gameobject, get the parent HEU_HoudiniAsset.
        /// </summary>
        /// <param name="outputGameObject">The output gameobject associated with the asset</param>
        /// <returns>Valid HEU_HoudiniAsset or null if no match</returns>
        public static HEU_HoudiniAsset GetSceneHDAAssetFromGameObject(GameObject outputGameObject)
        {
            if (outputGameObject.transform.parent != null)
            {
                GameObject parentGO = outputGameObject.transform.parent.gameObject;
                HEU_HoudiniAssetRoot assetRoot = parentGO.GetComponent<HEU_HoudiniAssetRoot>();
                if (assetRoot != null && assetRoot._houdiniAsset != null)
                {
                    return assetRoot._houdiniAsset;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if given object is an output of an HDA.
        /// </summary>
        /// <param name="go">GameObject to check if output</param>
        /// <returns>True if object is an output of an HDA</returns>
        public static bool IsHoudiniAssetOutput(GameObject go)
        {
            return (go.transform.parent != null) && (go.transform.parent.gameObject.GetComponent<HEU_HoudiniAssetRoot>() != null)
                                                 && (go.GetComponent<HEU_HoudiniAsset>() == null);
        }

        /// <summary>
        /// Returns true if given object is the root of an HDA.
        /// </summary>
        /// <param name="go">GameObject to check</param>
        /// <returns>Returns true if given object is the root of an HDA</returns>
        public static bool IsHoudiniAssetRoot(GameObject go)
        {
            return go.GetComponent<HEU_HoudiniAssetRoot>() != null;
        }


        // ==================================================================================


        // PRIVATE LOGIC -----------------------------------------------------------------------------------------------------

        /// <summary>
        /// Setup as a new asset
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="filePath"></param>
        /// <param name="rootGameObject"></param>
        internal void SetupAsset(HEU_AssetType assetType, string filePath, GameObject rootGameObject, HEU_SessionBase session)
        {
            _assetType = assetType;
            _assetPath = filePath;
            _rootGameObject = rootGameObject;
            _objectNodes = new List<HEU_ObjectNode>();
            _materialCache = new List<HEU_MaterialData>();
            _parameters = null;
            _curves = new List<HEU_Curve>();
            _inputNodes = new List<HEU_InputNode>();
            _handles = new List<HEU_Handle>();
            _volumeCaches = new List<HEU_VolumeCache>();
            _attributeStores = new List<HEU_AttributesStore>();
            _toolsInfo = ScriptableObject.CreateInstance<HEU_ToolsInfo>();

            _showCurvesSection = _assetType == HEU_AssetType.TYPE_CURVE;
            _showInputNodesSection = _assetType == HEU_AssetType.TYPE_INPUT;
            _showTerrainSection = false;

            _instanceInputUIState = ScriptableObject.CreateInstance<HEU_InstanceInputUIState>();

            Debug.AssertFormat(session != null && session.IsSessionValid(), "Must have valid session for new asset");
            _sessionID = session.GetSessionData().SessionID;

            _totalCookCount = 0;
            if (_serializedMetaData == null)
            {
                _serializedMetaData = ScriptableObject.CreateInstance<HEU_AssetSerializedMetaData>();
            }

            _serializedMetaData.SavedCurveNodeData.Clear();
        }

        /// <summary>
        /// Clean up generated data and disable this asset.
        /// </summary>
        internal void CleanUpAndDisable()
        {
            InvalidateAsset();
            DeleteAllGeneratedData();

            // Setup again to avoid null references
            SetupAsset(_assetType, _assetPath, _rootGameObject, GetAssetSession(true));
        }

        /// <summary>
        /// Returns true if this asset has been saved in a scene.
        /// </summary>
        /// <returns>True if asset has been saved in a scene.</returns>
        internal bool IsAssetSavedInScene()
        {
            return HEU_AssetDatabase.IsAssetSavedInScene(this.gameObject);
        }

        private void Awake()
        {
#if HOUDINIENGINEUNITY_ENABLED
            //HEU_Logger.Log("HEU_HoudiniAsset::Awake - " + AssetName);

            if (_serializedMetaData == null)
            {
                _serializedMetaData = ScriptableObject.CreateInstance<HEU_AssetSerializedMetaData>();
            }

            // We want to support Object.Instantiate, but ScriptableObjects cannot copy by value by 
            // default. So we simulate the "duplicate" function when we detect that this occurs
            // This would be a lot easier if Unity provided some sort of Instantiate() callback...
            AssetInstantiationMethod instantiationMethod = this.GetInstantiationMethod();
            if (instantiationMethod == AssetInstantiationMethod.DUPLICATED)
            {
                HEU_HoudiniAsset instantiatedAsset = this.GetInstantiatedObject();
                this.ResetAndCopyInstantiatedProperties(instantiatedAsset);
            }

            // All assets are checked if valid in Houdini Engine session in Awake.
            // Awake is called at scene load / script compilation / play mode change.
            // This will re-register with existing session that created this asset
            // and which still has the instance in Houdini.
            // This is required because the session might have lost its internal reference
            // to this asset if Unity had destroyed the asset during a play mode change
            // or script compilation refresh.
            HEU_SessionBase session = GetAssetSession(false);
            if (session != null && HEU_HAPIUtility.IsNodeValidInHoudini(session, _assetID))
            {
                session.ReregisterOnAwake(this);
            }

            // Clear out the delegate because receiver might not exist on code refresh
            _refreshUIDelegate = null;

            if (_assetID != HEU_Defines.HEU_INVALID_NODE_ID && instantiationMethod == AssetInstantiationMethod.UNDO)
            {
                Transform[] gos = _rootGameObject.GetComponentsInChildren<Transform>();
                foreach (Transform trans in gos)
                {
                    if (trans != null && trans.gameObject != null && trans.gameObject != _rootGameObject && trans.gameObject != this.gameObject)
                    {
                        DestroyImmediate(trans.gameObject);
                    }
                }

                this._serializedMetaData.SoftDeleted = false;

                HEU_Logger.LogWarning("Undoing a deleted HDA may also remove its parameter undo stack.");
                RequestReload(false);
            }

            // If there are curves they need to be cooked.
            if (Curves != null)
            {
                foreach (var curve in Curves)
                {
                    curve.Rebuild();
                }
            }

#endif
        }

        /// <summary>
        /// Forces the asset to be invalidated so that it will need to be recreated
        /// in a Houdini session on the next recook, rebuild, or parameter change.
        /// This should be done in asset is not found to be valid in an existing session.
        /// </summary>
        internal void InvalidateAsset()
        {
            _assetID = HEU_Defines.HEU_INVALID_NODE_ID;
        }

        private void OnEnable()
        {
#if HOUDINIENGINEUNITY_ENABLED
            // Adding in OnEnable as its called after a code recompile (Awake is not).
            HEU_AssetUpdater.AddAssetForUpdate(this);

            // Not supporting prefab so disable and clean up if inside prefab stage
            if (HEU_EditorUtility.IsEditingInPrefabMode(gameObject))
            {
                CleanUpAndDisable();
            }

            // This is required when coming back from play mode, after code compilation,
            // or scene load to re-add the upstream notifications for input assets.
            // Note that this done in OnEnable instead of Awake since OnEnable seems to
            // cover all cases including code compilation refresh.
            ReconnectInputsUpstreamNotifications();

#endif
        }

        private void OnDestroy()
        {
            if (this.PauseCooking == true)
            {
                return;
            }

            //HEU_Logger.Log("Asset:OnDestroy");
#if HOUDINIENGINEUNITY_ENABLED
            HEU_AssetUpdater.RemoveAsset(this);
#endif
        }

        /// <summary>
        /// Update asset state, and handle asset update requests such as cook, rebuild, etc.
        /// </summary>
        internal void AssetUpdate()
        {
#if HOUDINIENGINEUNITY_ENABLED

            if (_cookStatus == AssetCookStatus.POSTCOOK || _cookStatus == AssetCookStatus.POSTLOAD)
            {
                SetCookStatus(AssetCookStatus.NONE, AssetCookResult.NONE);
            }

            if (_cookStatus == AssetCookStatus.COOKING)
            {
                // Wait for cooking in Houdini to complete
                ProcessHoudiniCookStatus(true);
            }
            else if (_cookStatus == AssetCookStatus.SELECT_SUBASSET)
            {
                if (_selectedSubassetIndex > -1)
                {
                    // Continue loading now that we have a valid _selectedSubassetIndex
                    SetCookStatus(AssetCookStatus.LOADING, AssetCookResult.NONE);
                    ProcessRebuild(false, -1);
                }
            }
            else if (_cookStatus == AssetCookStatus.NONE)
            {
                // Not cooking. Process any requests.

                if (HEU_PluginSettings.TransformChangeTriggersCooks && TransformChangeTriggersCooks)
                {
                    if (HasTransformChangedSinceLastUpdate() || HasInputNodeTransformChanged())
                    {
                        RequestCook(true, false, false, true);
                    }
                }

                if (_requestBuildAction == AssetBuildAction.RELOAD)
                {
                    ClearBuildRequest();
                    SetCookStatus(AssetCookStatus.PRELOAD, AssetCookResult.NONE);
                    ProcessRebuild(true, -1);
                }
                else if (_requestBuildAction == AssetBuildAction.COOK)
                {
                    bool thisCheckParameterChangeForCook = _checkParameterChangeForCook;
                    bool thisSkipCookCheck = _skipCookCheck;
                    bool thisUploadParameters = _uploadParameters;
                    bool thisUploadParameterPreset = false;
                    bool thisForceUploadInputs = _forceUploadInputs;
                    bool thisSessionSyncCook = false;
                    ClearBuildRequest();
                    RecookAsync(thisCheckParameterChangeForCook, thisSkipCookCheck,
                        thisUploadParameters, thisUploadParameterPreset,
                        thisForceUploadInputs, thisSessionSyncCook);
                }
                else if (_requestBuildAction == AssetBuildAction.STRIP_HEDATA)
                {
                    ClearBuildRequest();
                    HEU_HoudiniAssetRoot assetRoot = _rootGameObject.GetComponent<HEU_HoudiniAssetRoot>();
                    if (assetRoot != null)
                    {
                        assetRoot.RemoveHoudiniEngineAssetData();
                    }
                    else
                    {
                        HEU_Logger.LogError(HEU_Defines.HEU_NAME + ": Unable to Bake In Place due to HEU_HoudiniAssetRoot not found!");
                    }
                }
                else if (_requestBuildAction == AssetBuildAction.DUPLICATE)
                {
                    ClearBuildRequest();
                    DuplicateAsset();
                }
                else if (_requestBuildAction == AssetBuildAction.RESET_PARAMS)
                {
                    ClearBuildRequest();
                    ResetParametersToDefault();

                    // Doing a Reload here to clear everything out after resetting the parameters.
                    // Originally was doing a Recook but because it will keep stuff around (e.g. terrain), a full reset seems better.
                    RequestReload(bAsync: true);
                }
                else if (_pendingAutoCookOnMouseRelease == true && HEU_EditorUtility.ReleasedMouse())
                {
                    _pendingAutoCookOnMouseRelease = false;
                    RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false, bUploadParameters: true);
                }
                else
                {
                    // For Houdini Engine Session Sync, update any originating changes from Houdini side
                    UpdateSessionSync();
                }
            }
#endif
        }

        /// <summary>
        /// Do follow up work after updating the asset.
        /// Currently progresses state after cook and loading for UI.
        /// </summary>
        internal void PostAssetUpdate()
        {
#if HOUDINIENGINEUNITY_ENABLED

            if (_cookStatus == AssetCookStatus.POSTCOOK || _cookStatus == AssetCookStatus.POSTLOAD)
            {
                SetCookStatus(AssetCookStatus.NONE, AssetCookResult.NONE);
            }

#endif
        }

        internal void RequestBakeInPlace()
        {
            if (_requestBuildAction == AssetBuildAction.NONE)
            {
                _requestBuildAction = AssetBuildAction.STRIP_HEDATA;
            }
        }

        internal void ClearBuildRequest()
        {
            //HEU_Logger.Log("ClearBuildRequest");
            _requestBuildAction = AssetBuildAction.NONE;
            _checkParameterChangeForCook = false;
            _skipCookCheck = false;
            _uploadParameters = true;
            _forceUploadInputs = false;
        }

        private bool HasValidAssetPath()
        {
            return !string.IsNullOrEmpty(_assetPath);
        }

        /// <summary>
        /// Start or continue rebuilding this asset.
        /// Notifies listeners when loading has finished successfully or failed.
        /// Uses _cookStatus to progress the rebuild state.
        /// </summary>
        /// <param name="bPromptForSubasset"></param>
        /// <param name="desiredSubassetIndex"></param>
        private void ProcessRebuild(bool bPromptForSubasset, int desiredSubassetIndex)
        {
            if (_preAssetEvent != null)
            {
                _preAssetEvent.Invoke(new HEU_PreAssetEventData(this, HEU_AssetEventType.RELOAD));
            }

            ClearInvalidLists();

            bool bResult = false;

            try
            {
                if (_cookStatus == AssetCookStatus.PRELOAD)
                {
                    // Start the Rebuild.
                    bResult = StartRebuild(bPromptForSubasset, desiredSubassetIndex);
                }

                if (_cookStatus == AssetCookStatus.LOADING)
                {
                    // Continue rebuild
                    bResult = FinishRebuild();
                }
            }
            catch (System.Exception ex)
            {
                HEU_Logger.LogErrorFormat("Rebuild error: " + ex.ToString());
                bResult = false;
            }

            if (!bResult)
            {
                SetCookStatus(AssetCookStatus.POSTLOAD, AssetCookResult.ERRORED);
            }

            if (_reloadDataEvent != null)
            {
                // Do callbacks regardless of success or failure as listeners might need to know
                List<GameObject> outputObjects = new List<GameObject>();
                GetOutputGameObjects(outputObjects);
                InvokeReloadEvent(bResult, outputObjects);
            }
        }

        private void InvokeReloadEvent(bool bCookSuccess, List<GameObject> outputObjects)
        {
            if (_reloadDataEvent != null)
            {
                _reloadDataEvent.Invoke(new HEU_ReloadEventData(this, bCookSuccess, outputObjects));
            }
        }

        /// <summary>
        /// Start the rebuild process.
        /// Destroys existing state and data, and resets parameters.
        /// This separates the initial loading of the asset so that we can handle any pre-loading steps 
        /// such as prompting user to select a subasset.
        /// </summary>
        /// <param name="bPromptForSubasset">Whether to ask user to select the subasset if more than one is found</param>
        /// <param name="desiredSubassetIndex">The index of the subasset to use if multiple subassets exist</param>
        /// <returns>True if successfully started the rebuild process, otherwise false for failure</returns>
        private bool StartRebuild(bool bPromptForSubasset, int desiredSubassetIndex)
        {
            HEU_SessionBase session = GetAssetSession(true);
            if (session == null)
            {
                return false;
            }

            // Save parameter preset
            _savedAssetPreset = GetAssetPreset(false);

            if (_assetID != HEU_Defines.HEU_INVALID_NODE_ID && !IsAssetValidInHoudini(session))
            {
                // Invalidate asset ID since this is not valid in the current session.
                InternalSetAssetID(HEU_Defines.HEU_INVALID_NODE_ID);
            }

            if (_assetID == HEU_Defines.HEU_INVALID_NODE_ID)
            {
                // Invalidating the session ID while calling DeleteAllGeneratedData next 
                // so that it doesn't modify the session that it doesn't exist in.
                _sessionID = HEU_SessionData.INVALID_SESSION_ID;
            }

            DeleteAllGeneratedData(bIsRebuild: true);

            // Setting the session ID to the session that this asset will now be created in.
            _sessionID = session.GetSessionData().SessionID;

            Debug.Assert(_assetID == HEU_Defines.HEU_INVALID_NODE_ID, "Asset must be new or cleaned up! Missing call to CleanUpAsset?");
            Debug.Assert(_objectNodes.Count == 0, "Object list must be empty! Missing call to DeleteAllPersistentData?");

            _subassetNames = new string[0];
            _selectedSubassetIndex = -1;

            // Load and cook the HDA
            if (_assetType == HEU_AssetType.TYPE_HDA)
            {
                bool bResult = LoadAssetFileWithSubasset(session, bPromptForSubasset, desiredSubassetIndex);
                if (!bResult)
                {
                    return false;
                }
            }

            // Progress to LOADING only if still in PRELOAD. Otherwise we might be waiting on user prompt.
            if (_cookStatus == AssetCookStatus.PRELOAD)
            {
                SetCookStatus(AssetCookStatus.LOADING, AssetCookResult.SUCCESS);
            }

            return true;
        }

        /// <summary>
        /// Finish rebuilding the asset, which was started by StartRebuild.
        /// Creates asset node, cooks, and generates all output.
        /// </summary>
        /// <returns>True if successfully completed building the asset</returns>
        private bool FinishRebuild()
        {
            HEU_SessionBase session = GetAssetSession(true);
            if (session == null)
            {
                return false;
            }

            // Load and cook the HDA
            HAPI_NodeId newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
            if (_assetType == HEU_AssetType.TYPE_HDA)
            {
                if (_selectedSubassetIndex < 0)
                {
                    HEU_Logger.LogFormat("Invalid subasset index {0}", _selectedSubassetIndex);
                    return false;
                }

                bool bResult = CreateAndCookAsset(session, _selectedSubassetIndex, out newAssetID, HEU_PluginSettings.CookTemplatedGeos);
                if (!bResult)
                {
                    if (newAssetID != HEU_Defines.HEU_INVALID_NODE_ID)
                    {
                        DeleteAllGeneratedData();
                    }

                    return false;
                }
            }
            else if (_assetType == HEU_AssetType.TYPE_CURVE)
            {
                if (_assetName == null)
                {
                    _assetName = "curve";
                }

                bool bResult = HEU_HAPIUtility.CreateAndCookCurveAsset(session, _assetName, HEU_PluginSettings.CookTemplatedGeos, out newAssetID);
                if (!bResult)
                {
                    if (newAssetID != HEU_Defines.HEU_INVALID_NODE_ID)
                    {
                        DeleteAllGeneratedData();
                    }

                    return false;
                }
            }
            else if (_assetType == HEU_AssetType.TYPE_INPUT)
            {
                if (_assetName == null)
                {
                    _assetName = "";
                }

                bool bResult = HEU_HAPIUtility.CreateAndCookInputAsset(session, _assetName, HEU_PluginSettings.CookTemplatedGeos, out newAssetID);
                if (!bResult)
                {
                    if (newAssetID != HEU_Defines.HEU_INVALID_NODE_ID)
                    {
                        DeleteAllGeneratedData();
                    }

                    return false;
                }
            }
            else
            {
                HEU_Logger.LogErrorFormat(HEU_Defines.HEU_NAME + ": Unsupported asset type {0}!", _assetType);
                return false;
            }

            InternalSetAssetID(newAssetID);
            session.RegisterAsset(this);

            session.GetNodeInfo(_assetID, ref _nodeInfo);
            session.GetAssetInfo(_assetID, ref _assetInfo);

            UpdateTotalCookCount();

            // Cache asset info
            string realName = HEU_SessionManager.GetString(_assetInfo.nameSH, session);

            if (!HEU_PluginSettings.ShortenFolderPaths || realName.Length < 3)
            {
                _assetName = realName;
            }
            else
            {
                _assetName = realName.Substring(0, 3) + this.GetHashCode();
            }

            _assetOpName = HEU_SessionManager.GetString(_assetInfo.fullOpNameSH, session);
            _assetHelp = HEU_SessionManager.GetString(_assetInfo.helpTextSH, session);

            //HEU_Logger.Log(HEU_Defines.HEU_NAME + ": Asset Loaded - ID: " + _assetInfo.nodeId + "\n" +
            //					"    Full Name: " + _assetOpName + "\n" +
            //					"    Version: " + HEU_SessionManager.GetString(_assetInfo.versionSH, session) + "\n" +
            //					"    Unique Node Id: " + _nodeInfo.uniqueHoudiniNodeId + "\n" +
            //					"    Internal Node Path: " + HEU_SessionManager.GetString(_nodeInfo.internalNodePathSH, session) + "\n" +
            //					"    Asset Library File: " + HEU_SessionManager.GetString(_assetInfo.filePathSH, session) + "\n");

            if (RootGameObject.name.Equals(HEU_Defines.HEU_DEFAULT_ASSET_NAME))
            {
                HEU_GeneralUtility.RenameGameObject(RootGameObject, _assetName);
            }

            // Add input connections
            CreateAssetInputs(session);

            // Build the parameters
            GenerateParameters(session);

            // Save the default preset
            if (_parameters != null)
            {
                _parameters.DownloadAsDefaultPresetData(session);
            }

            // Create objects in this asset. It will create object nodes, geometry, and anything else required.
            if (!CreateObjects(session))
            {
                // Failed to create objects means that this asset is not valid
                HEU_Logger.LogErrorFormat(HEU_Defines.HEU_NAME + ": Failed to create objects for asset {0}", _assetName);
                DeleteAllGeneratedData();
                return false;
            }

            GenerateObjectsGeometry(session, bRebuild: true);

            GenerateInstances(session);

            GenerateAttributesStore(session);

            GenerateHandles(session);

            // Upload transform. This should happen after generating outputs above.
            if (HEU_PluginSettings.PushUnityTransformToHoudini && PushTransformToHoudini)
            {
                UploadUnityTransform(session, false);
            }

            NotifyInputNodesCookFinished();

            // This is required in order to flag to Unity that the scene data has changed. Otherwise saving the scene does not work.
            HEU_EditorUtility.MarkSceneDirty();

            SetCookStatus(AssetCookStatus.POSTLOAD, AssetCookResult.SUCCESS);

            // Finally load the saved preset and request another cook.
            if (_savedAssetPreset != null)
            {
                LoadAssetPresetAndCook(_savedAssetPreset);
                _savedAssetPreset = null;
            }

            if (HEU_PDGSession.IsPDGAsset(session, newAssetID) && RootGameObject.GetComponent<HEU_PDGAssetLink>() == null)
            {
                HEU_PDGAssetLink assetLink = RootGameObject.AddComponent<HEU_PDGAssetLink>();
                assetLink.Setup(this);
            }

            return true;
        }

        /// <summary>
        /// Cook this asset in Houdini, then handle the outcome.
        /// Cooking is done asynchrnously.
        /// </summary>
        /// <param name="bCheckParamsChanged">If true, then will only cook if parameters have changed.</param>
        /// <param name="bSkipCookCheck">If true, will check if cooking is enabled.</param>
        /// <param name="bUploadParameters"> If true, will upload parameter values before cooking.</param>
        /// <param name="bUploadParameterPreset">If true, will upload parameter preset into Houdini before cooking.</param>
        /// <param name="bForceUploadInputs">If true, will upload all input geometry into Houdini before cooking.</param>
        /// <param name="bCookingSessionSync">If true, this is a SessionSync cook.</param>
        /// <returns>True if cooking started.</returns>
        private bool RecookAsync(bool bCheckParamsChanged,
            bool bSkipCookCheck, bool bUploadParameters,
            bool bUploadParameterPreset, bool bForceUploadInputs,
            bool bCookingSessionSync)
        {
#if HEU_PROFILER_ON
	    _cookStartTime = Time.realtimeSinceStartup;
#endif

            bool bStarted = false;
            try
            {
                bStarted = InternalStartRecook(bCheckParamsChanged,
                    bSkipCookCheck, bUploadParameters,
                    bUploadParameterPreset, bForceUploadInputs,
                    bCookingSessionSync);
            }
            catch (System.Exception ex)
            {
                HEU_Logger.LogError("Recook error: " + ex.ToString());
                bStarted = false;
            }

            if (!bStarted)
            {
                SetCookStatus(AssetCookStatus.NONE, AssetCookResult.ERRORED);
                ExecutePostCookCallbacks();
            }

            return bStarted;
        }

        /// <summary>
        /// Cook this asset in Houdini, then handle the outcome.
        /// Cooking is done synchronously so this will block until finished.
        /// </summary>
        /// <param name="bCheckParamsChanged">If true, then will only cook if parameters have changed.</param>
        /// <param name="bSkipCookCheck">If true, will check if cooking is enabled.</param>
        /// <param name="bUploadParameters"> If true, will upload parameter values before cooking.</param>
        /// <param name="bUploadParameterPreset">If true, will upload parameter preset into Houdini before cooking.</param>
        /// <param name="bForceUploadInputs">If true, will upload all input geometry into Houdini before cooking.</param>
        /// <param name="bCookingSessionSync">If true, this is a SessionSync cook.</param>
        /// <returns>True if cooking was done.</returns>
        private bool RecookBlocking(bool bCheckParamsChanged, bool bSkipCookCheck,
            bool bUploadParameters, bool bUploadParameterPreset,
            bool bForceUploadInputs, bool bCookingSessionSync)
        {
#if HEU_PROFILER_ON
	    _cookStartTime = Time.realtimeSinceStartup;
#endif

            if (!HEU_PluginSettings.CookDisabledGameObjects && !this.gameObject.activeInHierarchy)
            {
                HEU_Logger.LogWarning("Houdini Asset: " + this.RootGameObject.name +
                                      " Skipped cooking due to being disabled. Enable and recook manually to resync!");
                return false;
            }

            bool bStarted = false;

            try
            {
                bStarted = InternalStartRecook(bCheckParamsChanged, bSkipCookCheck,
                    bUploadParameters, bUploadParameterPreset,
                    bForceUploadInputs, bCookingSessionSync);
            }
            catch (System.Exception ex)
            {
                HEU_Logger.LogError("Recook error: " + ex.ToString());
                bStarted = false;
            }

            if (!bStarted)
            {
                SetCookStatus(AssetCookStatus.NONE, AssetCookResult.ERRORED);
                ExecutePostCookCallbacks();
            }
            else
            {
                ProcessHoudiniCookStatus(false);
            }

            return bStarted;
        }

        /// <summary>
        /// Do any post-cook work
        /// </summary>
        private void DoPostCookWork(HEU_SessionBase session)
        {
            UpdateTotalCookCount();
            bool bNeedsRebuild = false;

            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                if (objNode == null)
                {
                    bNeedsRebuild = true;
                    continue;
                }

                objNode.ProcessUnityScriptAttributes(session);
            }

            if (bNeedsRebuild)
            {
                _objectNodes = _objectNodes.Filter(obj => obj != null);
            }

            // Update the Editor UI
            if (_refreshUIDelegate != null)
            {
                _refreshUIDelegate();
            }
        }

        /// <summary>
        /// Returns true if this asset is in a valid state for interactive
        /// parameter changes, cooking, and generating results.
        /// </summary>
        /// <param name="errorMessage">Fills with error message if not valid</param>
        /// <returns>True if valid</returns>
        internal bool IsValidForInteraction(ref string errorMessage)
        {
            bool valid = true;
            if (HEU_EditorUtility.IsPrefabAsset(gameObject))
            {
                // Disable UI when HDA is prefab
                errorMessage = "Houdini Engine Asset Error\n" +
                               "HDA as prefab not supported!";
                valid = false;
            }
            else
            {
#if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
#if UNITY_2021_2_OR_NEWER
                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
		var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif

                if (stage != null)
                {
                    // Disable UI when HDA is in prefab stage
                    errorMessage = "Houdini Engine Asset Error\n" +
                                   "HDA as prefab not supported!";
                    valid = false;
                }
#endif
            }

            return valid;
        }

        private void OnValidate()
        {
            // This gets called when copying component or applying prefab

            // Not supporting HDA as prefab, so disable the asset
            if (HEU_EditorUtility.IsPrefabAsset(gameObject))
            {
                CleanUpAndDisable();
            }
        }

        /// <summary>
        /// Invoke the callbacks after a cook.
        /// </summary>
        private void ExecutePostCookCallbacks()
        {
            if (_cookedDataEvent != null)
            {
                List<GameObject> outputObjects = new List<GameObject>();
                GetOutputGameObjects(outputObjects);
                bool bCookSuccess = (_lastCookResult == AssetCookResult.SUCCESS);

                InvokePostCookEvent(bCookSuccess, outputObjects);
            }
        }

        private void InvokePostCookEvent(bool bCookSuccess, List<GameObject> outputObjects)
        {
            if (_cookedDataEvent != null)
            {
                _cookedDataEvent.Invoke(new HEU_CookedEventData(this, bCookSuccess, outputObjects));
            }
        }

        /// <summary>
        /// Start the cooking process.
        /// </summary>
        /// <param name="bCheckParamsChanged">If true, then will only cook if parameters have changed.</param>
        /// <param name="bSkipCookCheck">If true, will check if cooking is enabled.</param>
        /// <param name="bUploadParameters"> If true, will upload parameter values before cooking.</param>
        /// <param name="bUploadParameterPreset">If true, will upload parameter preset into Houdini before cooking.</param>
        /// <param name="bForceUploadInputs">If true, will upload all input geometry into Houdini before cooking.</param>
        /// <param name="bCookingSessionSync">If true, this is a SessionSync cook.</param>
        /// <returns></returns>
        private bool InternalStartRecook(bool bCheckParamsChanged,
            bool bSkipCookCheck, bool bUploadParameters,
            bool bUploadParameterPreset, bool bForceUploadInputs,
            bool bCookingSessionSync)
        {
            if (!HEU_PluginSettings.CookDisabledGameObjects && !this.gameObject.activeInHierarchy)
            {
                HEU_Logger.LogWarning("Houdini Asset: " + this.RootGameObject.name +
                                      " Skipped cooking due to being disabled. Enable and recook manually to resync!");
                return false;
            }

            if (_preAssetEvent != null)
            {
                _preAssetEvent.Invoke(new HEU_PreAssetEventData(this, HEU_AssetEventType.COOK));
            }

            // Lists can be broken in Undo
            ClearInvalidLists();

            HEU_SessionBase session = GetAssetSession(true);
            if (session == null)
            {
                return false;
            }

            if (!bSkipCookCheck && !HEU_PluginSettings.CookingEnabled)
            {
                return false;
            }

            if (_pauseCooking)
            {
                return false;
            }

            // A recook is called when the asset has already been created previously.
            // We have to determine if the asset is in a valid state, upload its state,
            // then cook, and find out what has changed.
            //HEU_Logger.Log(HEU_Defines.HEU_NAME + ": Recooking " + AssetName);

            bool bResult = false;
            _isCookingAssetReloaded = false;

            // Not checking if parameters have changed implies we update everything
            // TODO: consolidate bCheckParamsChanged and _bForceUpdate
            _bForceUpdate = !bCheckParamsChanged;

            if ((_assetID < 0) || !IsAssetValidInHoudini(session))
            {
                // This asset does not exist in Houdini session.
                // This can happen after loading a scene with a saved HDA.
                // We'll need to reload asset into Houdini, upload the parameter preset, then cook.

                // Load and cook the HDA
                HAPI_NodeId newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
                if (_assetType == HEU_AssetType.TYPE_HDA)
                {
                    // Asset ID isn't valid so do full rebuild
                    if (!HasValidAssetPath())
                    {
                        HEU_Logger.LogError(HEU_Defines.HEU_NAME +
                                            ": Recook failed: asset needs to be reloaded but does not have valid asset path. Recommend instantiating new asset.");
                        return false;
                    }

                    bResult = LoadAssetFileWithSubasset(session, false, _selectedSubassetIndex);
                    if (bResult)
                    {
                        bResult = CreateAndCookAsset(session, _selectedSubassetIndex, out newAssetID, HEU_PluginSettings.CookTemplatedGeos);
                    }

                    if (!bResult)
                    {
                        // Asset load failed
                        return false;
                    }
                }
                else if (_assetType == HEU_AssetType.TYPE_CURVE)
                {
                    bResult = HEU_HAPIUtility.CreateAndCookCurveAsset(session, AssetName, HEU_PluginSettings.CookTemplatedGeos, out newAssetID);
                    if (!bResult)
                    {
                        // Asset load failed
                        return false;
                    }
                }
                else if (_assetType == HEU_AssetType.TYPE_INPUT)
                {
                    if (!HEU_HAPIUtility.CreateAndCookInputAsset(session, AssetName, HEU_PluginSettings.CookTemplatedGeos, out newAssetID))
                    {
                        return false;
                    }
                }
                else
                {
                    HEU_Logger.LogErrorFormat(HEU_Defines.HEU_NAME + ": Recook failed: unsupported asset type {0}!", _assetType);
                    return false;
                }

                InternalSetAssetID(newAssetID);
                session.RegisterAsset(this);

                // Flag it to show that the asset was reloaded in Houdini, and therefore requires extra setup
                _isCookingAssetReloaded = true;

                // Force updating everything on asset reload
                _bForceUpdate = true;

                session.GetNodeInfo(_assetID, ref _nodeInfo);
                session.GetAssetInfo(_assetID, ref _assetInfo);

                // Cache asset info
                _assetName = HEU_SessionManager.GetString(_assetInfo.nameSH, session);
                _assetOpName = HEU_SessionManager.GetString(_assetInfo.fullOpNameSH, session);
                _assetHelp = HEU_SessionManager.GetString(_assetInfo.helpTextSH, session);

                // This will update inputs based on the fact that the asset was recreated
                // in this session. Connections might get invalidated or updated.
                UpdateInputsOnAssetRecreation(session);

                // Upload parameter presets if exists
                UploadParameterPresetToHoudini(session);

                // Upload connected data for input node parameters. Required because after loading scene
                // Houdini wouldn't have the input geometry.
                UpdateParameterInputsToHoudini(session, _bForceUpdate);

                // Continue on with rest of recook...
            }

            // At this point, the asset exists in Houdini.
            // We will upload our parameters, cook (again), then handle any changes.

            // Upload this asset's Unity transform if it has changed
            // Note that uploading transforms before uploading parameters is important
            // since Houdini Engine will update the parameter values automatically.
            if (HEU_PluginSettings.PushUnityTransformToHoudini && PushTransformToHoudini)
            {
                UploadUnityTransform(session, !_isCookingAssetReloaded);
            }

            bool bParamsUpdated = false;

            // Let's try to upload existing parameter values.
            // It might fail if the parameters have changed in the HDA since last loaded in Unity.
            if (_parameters != null && !_parameters.RequiresRegeneration && bUploadParameters)
            {
                if (_assetID != _parameters.NodeID)
                {
                    // Parameters
                    HEU_Logger.LogWarning(HEU_Defines.HEU_NAME + ": Our parameter object must have our asset ID.\n"
                                                               + "If this fails, something went wrong earlier and need to catch it! Please try rebuilding!");
                    _parameters.CleanUp();
                    return false;
                }

                // Do parameter modifiers first. These change number of parameters (eg. multiparm).
                // If there are no modifiers, we can upload any changes to the actual values.
                if (_parameters.HasModifiersPending())
                {
                    _parameters.ProcessModifiers(session);
                }
                else
                {
                    if (!_parameters.UploadValuesToHoudini(session, this, bCheckParamsChanged, bForceUploadInputs))
                    {
                        HEU_Logger.LogWarningFormat(
                            HEU_Defines.HEU_NAME + ": Failed to upload parameter changes to Houdini for asset {0}. Please rebuild this asset.",
                            AssetName);
                    }

                    bParamsUpdated = true;
                }
            }

            if (!bCookingSessionSync || true)
            {
                // Only upload the following if we are not cooking as a result of SessionSync
                // i.e. Houdini already cook so no need to upload our own

                if (!_isCookingAssetReloaded)
                {
                    // Non-reloaded asset: handle parameter preset

                    if (bUploadParameterPreset)
                    {
                        // Parameter preset needs to be uploaded (could be due to parameter reset)
                        UploadParameterPresetToHoudini(session);
                    }
                    else
                    {
                    }
                }

                // Otherwise curves should upload their parameters
                UploadCurvesParameters(session, bCheckParamsChanged);

                // Upload attributes. For edit nodes, this will be a cumulative update. So if the source geo has
                // changed earlier in the graph, it will most likely be ignored here since the edit node has its own
                // version of the geo with custom attributes. The only way to resolve it would be to blow away the custom
                // attribute data (reset all edit node changes). Currently not enabled, though could be added as a 
                // button that invokes edit node's Reset All Changes.
                UploadAttributeValues(session);

                // Upload asset inputs. 
                // bForceUploadInputs allows to upload the input geometry when user hits Recook.
                UploadInputNodes(session, _bForceUpdate | bForceUploadInputs, !bParamsUpdated);
            }

            bResult = StartHoudiniCookNode(session);
            if (!bResult)
            {
                // Cooking failed.
                HEU_Logger.LogErrorFormat(HEU_Defines.HEU_NAME + ": Failed to cook asset {0}!", AssetName);
                return false;
            }

#if HEU_PROFILER_ON
	    _hapiCookEndTime = Time.realtimeSinceStartup;
#endif

            return true;
        }

        private void InternalSetAssetID(HAPI_NodeId assetID)
        {
            _assetID = assetID;

            // Update the input node IDs since they use the asset ID.
            foreach (HEU_InputNode input in _inputNodes)
            {
                if (input != null)
                {
                    input.SetInputNodeID(_assetID);
                }
            }
        }

        // Sets the asset cook status
        private void SetCookStatus(AssetCookStatus status, AssetCookResult result)
        {
            _cookStatus = status;
            _lastCookResult = result;
        }

        /// <summary>
        /// After cook has finished in Houdini, process the output and update/generate Unity side.
        /// </summary>
        private void ProcessPoskCook()
        {
#if HEU_PROFILER_ON
	    _postCookStartTime = Time.realtimeSinceStartup;
#endif

            HEU_SessionBase session = GetAssetSession(false);
            if (session == null)
            {
                SetCookStatus(AssetCookStatus.NONE, _lastCookResult = AssetCookResult.ERRORED);
                return;
            }

            // Refresh our node and asset infos again just in case anything changed after cooking
            session.GetNodeInfo(_assetID, ref _nodeInfo);
            session.GetAssetInfo(_assetID, ref _assetInfo);

            if (HEU_PluginSettings.WriteCookLogs)
            {
                string nodeStatusAll = session.ComposeNodeCookResult(_assetID, HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_ALL);
                if (nodeStatusAll != "")
                {
                    HEU_CookLogs.Instance.AppendCookLog(nodeStatusAll);
                }
            }

            // Output or suppress errors
            string nodeStatusError = session.ComposeNodeCookResult(_assetID, HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_ERRORS);
            if (nodeStatusError != "")
            {
                SetCookStatus(AssetCookStatus.NONE, _lastCookResult = AssetCookResult.ERRORED);

                string resultString = string.Format(HEU_Defines.HEU_NAME + ": Failed to cook asset {0}! \n{1}", AssetName, nodeStatusError);

                bool ignoreError = false;
                // Some heightfield nodes seem bugged in Houdini at the the moment, always producing an error
                if (!ignoreError && resultString.Contains("Invalid volume \"__temp_debris\" specified"))
                {
                    ignoreError = true;
                }

                // Depending on your OS, certain nodes, i.e. shader-related nodes may contain non-fatal errors (see Bug: 116237)
                if (!ignoreError && resultString.Contains("Unable to load shader"))
                {
                    ignoreError = true;
                }

                if (!ignoreError && resultString.Contains("slump_water/make_tmp_planes/repeat_end"))
                {
                    ignoreError = true;
                }

                if (!ignoreError)
                {
                    HEU_Logger.LogWarningFormat(resultString);
                    return;
                }
                else
                {
                    HEU_CookLogs.Instance.AppendCookLog(resultString);
                }
            }

            // We will always regenerate parameters after cooking to make sure we're in sync.
            GenerateParameters(session);

            // Download & save the parameter preset
            DownloadParameterPresetFromHoudini(session);

            //HEU_Logger.LogFormat("Node Input Count: {0}", _nodeInfo.inputCount);
            //HEU_Logger.LogFormat("Asset Input Count: {0}", _assetInfo.geoInputCount);

            // Update the Houdini materials in use by this asset.
            // This should be done before update the objects as below.
            UpdateHoudiniMaterials(session);

            // Number of objects might have changed.
            // This gets latest object infos, adds and removes objects, then refreshes them
            UpdateAllObjectNodes(session);

            GenerateObjectsGeometry(session, bRebuild: false);

            GenerateInstances(session);

            GenerateAttributesStore(session);

            if (_upstreamCookChanged)
            {
                // This sync allows to reupload local attribute values
                // to Houdini after an upstream input changed and we had
                // to reset the local changes on the edit node by reverting.
                SyncDirtyAttributesToHoudini(session);

                _upstreamCookChanged = false;
            }

            GenerateHandles(session);

            // Now apply saved presets that haven't been applied before
            if (_recookPreset != null)
            {
                ApplyRecookPreset();
            }

            // After all the objects have been processed, go through our materials list
            // and remove any unused materials.
            RemoveUnusedMaterials();

            // This forces the attribute editor to recache
            if (_toolsInfo)
                _toolsInfo._recacheRequired = true;

            // This is required in order to flag to Unity that the scene data has changed.
            // Otherwise saving the scene does not work.
            // Should we make this more specific by checking if there were any changes above?
            HEU_EditorUtility.MarkSceneDirty();

            DoPostCookWork(session);

            // Notify listeners that we've cooked!
            List<GameObject> outputObjects = new List<GameObject>();
            GetOutputGameObjects(outputObjects);
            if (_downstreamConnectionCookedEvent != null && HEU_PluginSettings.CookingTriggersDownstreamCooks && _cookingTriggersDownCooks)
            {
                _downstreamConnectionCookedEvent.Invoke(new HEU_CookedEventData(this, true, outputObjects));
            }

            SetCookStatus(AssetCookStatus.NONE, AssetCookResult.SUCCESS);

            if (session.IsSessionSync())
            {
                // Force a repaint in SessionSync so the Scene view updates
                HEU_EditorUtility.RepaintScene();
            }

#if HEU_PROFILER_ON
	    HEU_Logger.LogFormat("RECOOK PROFILE:: TOTAL={0}, HAPI={1}, POST={2}", (Time.realtimeSinceStartup - _cookStartTime), (_hapiCookEndTime - _cookStartTime), (Time.realtimeSinceStartup - _postCookStartTime));
#endif
        }

        // Actually Cooks the node using HAPI
        private bool StartHoudiniCookNode(HEU_SessionBase session)
        {
            bool bResult = session.CookNode(AssetID, HEU_PluginSettings.CookTemplatedGeos, SplitGeosByGroup);
            if (bResult)
            {
                SetCookStatus(AssetCookStatus.COOKING, AssetCookResult.NONE);
            }

            return bResult;
        }

        // Processes the cook status. This is where we block on async, and process post cook events
        private void ProcessHoudiniCookStatus(bool bAsync)
        {
            HAPI_State statusCode = HAPI_State.HAPI_STATE_STARTING_LOAD;

            HEU_SessionBase session = GetAssetSession(false);
            if (session == null)
            {
                HEU_Logger.LogWarning(HEU_Defines.HEU_NAME + ": No valid session for cooking!");
                SetCookStatus(AssetCookStatus.NONE, AssetCookResult.ERRORED);
            }
            else
            {
                bool bResult = true;
                do
                {
                    bResult = session.GetCookState(out statusCode);

                    // Add to cook log
                    if (HEU_PluginSettings.WriteCookLogs)
                    {
                        string cookStatus = session.GetStatusString(HAPI_StatusType.HAPI_STATUS_COOK_STATE,
                            HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_ERRORS);
                        HEU_CookLogs.Instance.AppendCookLog(cookStatus);
                    }

                    if (bResult && (statusCode > HAPI_State.HAPI_STATE_MAX_READY_STATE))
                    {
                        // Still cooking. If async, we'll return, otherwise busy wait.
                        if (bAsync)
                        {
                            return;
                        }
                    }
                    else
                    {
                        break;
                    }
                } while (bResult);

                // Check cook results for any errors
                if (statusCode == HAPI_State.HAPI_STATE_READY_WITH_FATAL_ERRORS)
                {
                    string statusString = session.GetStatusString(HAPI_StatusType.HAPI_STATUS_COOK_RESULT,
                        HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_ERRORS);
                    HEU_Logger.LogError(string.Format(HEU_Defines.HEU_NAME + ": Cooking failed for asset: {0}\n{1}", AssetName, statusString));

                    SetCookStatus(AssetCookStatus.NONE, AssetCookResult.ERRORED);
                }
                else
                {
                    if (statusCode == HAPI_State.HAPI_STATE_READY_WITH_COOK_ERRORS)
                    {
                        // We should be able to continue even with these errors, but at least notify user.
                        string statusString = session.GetStatusString(HAPI_StatusType.HAPI_STATUS_COOK_RESULT,
                            HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_WARNINGS);
                        HEU_Logger.LogWarning(string.Format(HEU_Defines.HEU_NAME + ": Cooking finished with some errors for asset: {0}\n{1}",
                            AssetName, statusString));
                    }
                    else
                    {
                        //HEU_Logger.LogFormat(HEU_Defines.HEU_NAME + ": Cooking result {0} for asset: {1}", (HAPI_State)statusCode, AssetName);
                    }

                    SetCookStatus(AssetCookStatus.POSTCOOK, AssetCookResult.SUCCESS);

                    UpdateTotalCookCount();

                    try
                    {
                        ProcessPoskCook();
                    }
                    catch (System.Exception ex)
                    {
                        HEU_Logger.LogError("Recook error: " + ex.ToString());
                        SetCookStatus(AssetCookStatus.POSTCOOK, AssetCookResult.ERRORED);
                    }
                }
            }

            // We do callbacks after everything to flag both success and error
            ExecutePostCookCallbacks();
        }

        /// <summary>
        /// Returns true if asset requires a recook.
        /// </summary>
        /// <returns>True if asset requires a recook.</returns>
        internal bool DoesAssetRequireRecook()
        {
            if (_parameters == null || _parameters.RequiresRegeneration || _parameters.HaveParametersChanged() || _parameters.HasModifiersPending())
            {
                return true;
            }

            // Check curves
            foreach (HEU_Curve curve in _curves)
            {
                if (_curves == null)
                    continue;

                if (curve.Parameters.HaveParametersChanged())
                {
                    return true;
                }
            }

            foreach (HEU_InputNode inputNode in _inputNodes)
            {
                if (_inputNodes == null)
                    continue;

                if (inputNode.InputType != HEU_InputNode.InputNodeType.PARAMETER &&
                    (inputNode.RequiresUpload || inputNode.HasInputNodeTransformChanged()))
                {
                    return true;
                }
            }

            foreach (HEU_VolumeCache volume in _volumeCaches)
            {
                if (volume == null)
                    continue;

                if (volume.IsDirty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes session only data. Does not delete persistent data as part of project.
        /// It deletes the asset node from Houdini session.
        /// </summary>
        internal void DeleteSessionDataOnly()
        {
            if (_assetID != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                //HEU_Logger.LogFormat(HEU_Defines.HEU_NAME + ": Deleting asset {0} in Houdini session.", _assetName);
                HEU_SessionBase session = GetAssetSession(false);
                if (session != null)
                {
                    session.DeleteNode(_assetID);
                    session.UnregisterAsset(_assetID);
                }
            }
        }

        // Remove input nodes
        private void CleanUpInputNodes()
        {
            if (_inputNodes != null && _inputNodes.Count > 0)
            {
                HEU_SessionBase session = GetAssetSession(false);

                List<HEU_InputNode> tempNodes = new List<HEU_InputNode>();

                for (int i = 0; i < _inputNodes.Count; ++i)
                {
                    // Only cleaning up connections as those are the ones this asset creates. The other types
                    // are handled by those that created them (geo node, parameter).
                    if (_inputNodes[i] != null && _inputNodes[i].InputType == HEU_InputNode.InputNodeType.CONNECTION)
                    {
                        tempNodes.Add(_inputNodes[i]);
                    }
                }

                for (int i = 0; i < tempNodes.Count; ++i)
                {
                    //HEU_Logger.LogFormat("Destroying input: {0}", tempNodes[i].InputName);
                    _inputNodes.Remove(tempNodes[i]);

                    tempNodes[i].DestroyAllData(session);
                    HEU_GeneralUtility.DestroyImmediate(tempNodes[i]);
                }
            }
        }

        internal void DeleteAssetCacheData(bool bRegisterUndo)
        {
            // Clear material cache as it won't be relevant when deleting the asset cache folder
            ClearMaterialCache();

            if (!string.IsNullOrEmpty(_assetCacheFolderPath))
            {
                // TODO: handle undo for deleting a folder in project
                HEU_AssetDatabase.DeleteAssetCacheFolder(_assetCacheFolderPath);
                _assetCacheFolderPath = null;
            }
        }

        /// <summary>
        /// Generate all the parameters for this asset based on information from HAPI.
        /// </summary>
        private void GenerateParameters(HEU_SessionBase session)
        {
            //HEU_Logger.Log(HEU_Defines.HEU_NAME + ": Generating parameters!");

#if HEU_PROFILER_ON
	    float parameterGenStartTime = Time.realtimeSinceStartup;
#endif

            // Store the previous folder and input node parameters so we can transfer them over to new parameters
            Dictionary<string, HEU_ParameterData> previousParamFolders = new Dictionary<string, HEU_ParameterData>();
            Dictionary<string, HEU_InputNode> previousParamInputNodes = new Dictionary<string, HEU_InputNode>();

            if (_parameters != null)
            {
                _parameters.GetParameterDataForUIRestore(previousParamFolders, previousParamInputNodes);

                // If parameter exists, just clean it up. Don't nullify or destroy it as it loses Undo history.
                _parameters.CleanUp();
            }
            else
            {
                _parameters = ScriptableObject.CreateInstance<HEU_Parameters>();
            }

            bool bResult = _parameters.Initialize(session, _assetID, ref _nodeInfo, previousParamFolders, previousParamInputNodes, this);
            if (!bResult)
            {
                HEU_Logger.LogWarningFormat(HEU_Defines.HEU_NAME + ": Parameter generate failed for asset {0}.", AssetName);
                _parameters.CleanUp();
            }

#if HEU_PROFILER_ON
	    HEU_Logger.LogFormat("PARAMETERS GENERATION TIME:: {0}", (Time.realtimeSinceStartup - parameterGenStartTime));
#endif
        }

        private void DownloadParameterPresetFromHoudini(HEU_SessionBase session)
        {
            if (HEU_EditorUtility.IsEditorPlaying())
            {
                return;
            }

            if (_parameters != null)
            {
                _parameters.DownloadPresetData(session);
            }

            // Note that we aren't downloading presets for our curves here as thats done after
            // the curve is re-generated.
        }

        private void UploadParameterPresetToHoudini(HEU_SessionBase session)
        {
            if (_parameters != null)
            {
                // Make sure that the parameters object has the latest node ID of our asset
                _parameters.NodeID = _assetID;

                _parameters.UploadPresetData(session);
            }

            ClearInvalidCurves();

            List<HEU_Curve> curves = Curves;
            foreach (HEU_Curve curve in curves)
            {
                HEU_Parameters curveParams = curve.Parameters;
                if (curveParams != null)
                {
                    // See note in HEU_Curve::UploadParameterPreset
                    curve.SetUploadParameterPreset(true);
                }
            }
        }

        private void UpdateParameterInputsToHoudini(HEU_SessionBase session, bool bForceUpdate)
        {
            if (_parameters != null)
            {
                _parameters.UploadParameterInputs(session, this, bForceUpdate);
            }
        }

        /// <summary>
        /// Load the file for this asset, and find the subasset if specified.
        /// </summary>
        /// <param name="session">Current asset session</param>
        /// <param name="bPromptForSubasset">If multiple subassets found, then whether to wait to prompt for subasset</param>
        /// <param name="desiredSubassetIndex">If multiple subassets found, this is the index of the preferred subasset to use</param>
        /// <returns>True if successfully loaded asset</returns>
        private bool LoadAssetFileWithSubasset(HEU_SessionBase session, bool bPromptForSubasset, int desiredSubassetIndex)
        {
            if (_assetType != HEU_AssetType.TYPE_HDA)
            {
                HEU_Logger.LogErrorFormat("Trying to build asset type: {0}. Expected type: {1}.", _assetType, HEU_AssetType.TYPE_HDA);
                return false;
            }

            // Load the asset file.

            // First try using object reference if its valid.
            string validAssetPath = HEU_HAPIUtility.LocateValidFilePath(_assetFileObject);
            if (string.IsNullOrEmpty(validAssetPath))
            {
                // Otherwise use the _assetPath which might be environment mapped, in which case convert to real path.
                validAssetPath = HEU_PluginStorage.Instance.ConvertEnvKeyedPathToReal(_assetPath);
            }

            if (string.IsNullOrEmpty(validAssetPath))
            {
                return false;
            }

            if (_assetFileObject == null || !HEU_Platform.DoesFileExist(validAssetPath))
            {
                // This handles 2 cases:
                // - set the _assetFileObject reference if possible (which allows to get local path easily via Unity AssetDatabase)
                // - when using assets from Packages/, need to convert to real local path so that Houdini can load it
                //   but the issue is the current path might not be local, so to convert it, first convert into a format
                //   that AssetDatabase can load as object, then get the real local path to pass to Houdini

                validAssetPath = HEU_AssetDatabase.GetValidAssetPath(validAssetPath);
                _assetFileObject = HEU_AssetDatabase.LoadAssetAtPath(validAssetPath, typeof(Object));

                // Update the load path from _assetFileObject to get local path
                if (_assetFileObject != null)
                {
                    validAssetPath = HEU_AssetDatabase.GetAssetPath(_assetFileObject);
                }
            }

            HAPI_AssetLibraryId libraryID = 0;
            bool bResult = false;
            if (!_loadAssetFromMemory)
            {
                bResult = session.LoadAssetLibraryFromFile(validAssetPath, _alwaysOverwriteOnLoad, out libraryID);
            }
            else
            {
                byte[] buffer = null;
                bResult = HEU_Platform.LoadFileIntoMemory(validAssetPath, out buffer);
                if (bResult)
                {
                    bResult = session.LoadAssetLibraryFromMemory(buffer, _alwaysOverwriteOnLoad, out libraryID);
                }
            }

            if (!bResult)
            {
                return false;
            }

            // Convert asset path back to environment mapped key format (ie. convert to $key/blah.hda)
            _assetPath = HEU_PluginStorage.Instance.ConvertRealPathToEnvKeyedPath(validAssetPath);
            if (!_assetPath.Equals(validAssetPath))
            {
                HEU_Logger.LogFormat("Storing asset file path with environment mapping: {0}", _assetPath);
            }

            int assetCount = 0;
            bResult = session.GetAvailableAssetCount(libraryID, out assetCount);
            if (!bResult)
            {
                return false;
            }

            if (assetCount <= 0)
            {
                HEU_Logger.LogErrorFormat("Houdini Engine: Invalid Asset Count of {0}", assetCount);
                return false;
            }

            HAPI_StringHandle[] assetNameLengths = new HAPI_StringHandle[assetCount];
            bResult = session.GetAvailableAssets(libraryID, ref assetNameLengths, assetCount);
            if (!bResult)
            {
                return false;
            }

            // Sanity check that our array hasn't changed size
            Debug.Assert(assetNameLengths.Length == assetCount, "Houdini Engine: Invalid Asset Names");

            string[] assetNames = new string[assetCount];
            for (int i = 0; i < assetCount; ++i)
            {
                assetNames[i] = HEU_SessionManager.GetString(assetNameLengths[i], session);
            }

            _subassetNames = assetNames;

            if (bPromptForSubasset && assetCount > 1 && (desiredSubassetIndex == -1 || desiredSubassetIndex >= assetCount))
            {
                // Ask user to select subasset since there is more than one and the specified index isn't valid
                _selectedSubassetIndex = -1;
                SetCookStatus(AssetCookStatus.SELECT_SUBASSET, AssetCookResult.NONE);
            }
            else
            {
                if (desiredSubassetIndex >= 0 && desiredSubassetIndex < assetCount)
                {
                    _selectedSubassetIndex = desiredSubassetIndex;
                }
                else
                {
                    _selectedSubassetIndex = 0;
                }
            }

            return true;
        }

        // Creates and cooks assets (on recook)
        private bool CreateAndCookAsset(HEU_SessionBase session, int subassetIndex, out HAPI_NodeId newAssetID, bool bCookTemplatedGeos)
        {
            newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;

            if (subassetIndex < 0 || subassetIndex >= _subassetNames.Length)
            {
                HEU_Logger.LogFormat("Invalid subasset index {0}", subassetIndex);
                return false;
            }

            // Create top level node. Note that CreateNode will cook the node if HAPI was initialized with threaded cook setting on.
            string topNodeName = _subassetNames[subassetIndex];
            bool bResult = session.CreateNode(-1, topNodeName, "", false, out newAssetID);
            if (!bResult)
            {
                return false;
            }

            // Make sure cooking is successfull before proceeding. Any licensing or file data issues will be caught here.
            if (!HEU_HAPIUtility.ProcessHoudiniCookStatus(session, AssetName))
            {
                return false;
            }

            // In case the cooking wasn't done previously, force it now.
            bResult = HEU_HAPIUtility.CookNodeInHoudini(session, newAssetID, bCookTemplatedGeos, AssetName);
            if (!bResult)
            {
                // When cook failed, delete the node created earlier
                session.DeleteNode(newAssetID);
                newAssetID = HEU_Defines.HEU_INVALID_NODE_ID;
                return false;
            }

            // Get the asset ID
            HAPI_AssetInfo assetInfo = new HAPI_AssetInfo();
            bResult = session.GetAssetInfo(newAssetID, ref assetInfo);
            if (bResult)
            {
                // Check for any errors
                HAPI_ErrorCodeBits errors =
                    session.CheckForSpecificErrors(newAssetID, (HAPI_ErrorCodeBits)HAPI_ErrorCode.HAPI_ERRORCODE_ASSET_DEF_NOT_FOUND);
                if (errors > 0)
                {
                    HEU_EditorUtility.DisplayDialog("Asset Missing Sub-asset Definitions",
                        "There are undefined nodes. This is due to not being able to find specific " +
                        "asset definitions. You might need to load other (dependent) HDAs first.", "Ok");
                }
            }

            return true;
        }

        /// <summary>
        /// Create and setup asset input nodes.
        /// </summary>
        /// <param name="session"></param>
        private void CreateAssetInputs(HEU_SessionBase session)
        {
            if (_assetType == HEU_AssetType.TYPE_INPUT || _assetType == HEU_AssetType.TYPE_CURVE)
            {
                // Not creating input nodes for purely Input or Curve type assets.
                // This is because an input type asset creates its own input nodes via its geo node.
                return;
            }

            int updatedAssetInputCount = _assetInfo.geoInputCount;

            if (updatedAssetInputCount == 0)
            {
                CleanUpInputNodes();
                return;
            }

            if (_nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ && _assetInfo.transformInputCount > 0)
            {
                // TODO: handle upstream transform connections for objects
            }

            // Go through all asset inputs, add new, and remove old (unfound)

            List<HEU_InputNode> nodesToRemove = _inputNodes != null ? new List<HEU_InputNode>(_inputNodes) : new List<HEU_InputNode>();
            List<string> newInputNames = new List<string>();
            List<int> newInputIndex = new List<int>();

            for (int i = 0; i < updatedAssetInputCount; ++i)
            {
                HAPI_StringHandle tempNameHandle = HEU_Defines.HEU_INVALID_NODE_ID;
                if (session.GetNodeInputName(_assetID, i, out tempNameHandle))
                {
                    string inputName = HEU_SessionManager.GetString(tempNameHandle, session);
                    //HEU_Logger.Log("Found input: " + inputName);

                    HEU_InputNode inputNode = GetAssetInputNode(inputName);
                    if (inputNode != null)
                    {
                        nodesToRemove.Remove(inputNode);
                    }
                    else
                    {
                        newInputNames.Add(inputName);
                        newInputIndex.Add(i);
                    }
                }
            }

            // Remove input nodes not found
            int numLeftover = nodesToRemove.Count;
            for (int i = numLeftover - 1; i >= 0; --i)
            {
                HEU_InputNode inputNode = nodesToRemove[i];
                RemoveInputNode(inputNode);

                if (inputNode != null)
                {
                    inputNode.DestroyAllData(session);
                    HEU_GeneralUtility.DestroyImmediate(inputNode);
                }
            }

            // Nodes to add
            int numNodesToAdd = newInputNames.Count;
            for (int i = 0; i < numNodesToAdd; ++i)
            {
                HEU_InputNode inputNode = HEU_InputNode.CreateSetupInput(_assetID, newInputIndex[i], newInputNames[i], newInputNames[i],
                    HEU_InputNode.InputNodeType.CONNECTION, this);
                if (inputNode != null)
                {
                    AddInputNode(inputNode);
                }
            }

            _showInputNodesSection = (_assetType == HEU_AssetType.TYPE_INPUT) || (updatedAssetInputCount > 0);
        }

        // Upload curve parameters
        private void UploadCurvesParameters(HEU_SessionBase session, bool bCheckParamsChanged)
        {
            bool bNeedsRebuild = false;
            foreach (HEU_Curve curve in _curves)
            {
                if (curve == null)
                {
                    bNeedsRebuild = true;
                    continue;
                }

                if (curve.IsEditable())
                {
                    curve.OnPresyncParameters(session, this);
                    HEU_Parameters curveParameters = curve.Parameters;
                    if (curveParameters != null)
                    {
                        curveParameters.UploadValuesToHoudini(session, this, bCheckParamsChanged);
                    }
                }
            }

            if (bNeedsRebuild)
            {
                _curves = _curves.Filter(curve => curve != null);
            }
        }

        private void UploadAttributeValues(HEU_SessionBase session)
        {
            // Rebuild for undo safety in UI:
            bool bNeedsRebuild = false;

            for (int i = _attributeStores.Count - 1; i >= 0; i--)
            {
                HEU_AttributesStore attributeStore = _attributeStores[i];
                if (attributeStore == null)
                {
                    bNeedsRebuild = true;
                    continue;
                }

                if (!attributeStore.IsValidStore(session))
                {
                    RemoveAttributeStore(attributeStore);
                }
            }

            if (bNeedsRebuild)
            {
                _attributeStores = _attributeStores.Filter(store => store != null);
            }

            // Normally only the attribute stores that are dirty will be uploaded to Houdini.
            // But if _toolsInfo._alwaysCookUpstream is true, we will upload all attributes
            // if there is at least one of them that is dirty. This is to handle case where
            // multiple editable nodes are being edited. In this case, each one will need
            // to have its modifications re-uploaded as each node will need to recook its
            // upstream inputs before doing so.

            bool bForceUpload = false;
            if (_toolsInfo != null && _toolsInfo._alwaysCookUpstream)
            {
                if (_upstreamCookChanged)
                {
                    // Note that for _upstreamCookChanged, force the refresh of the upstream input
                    // so that the edit node is unlocked and able to get updated values.
                    foreach (HEU_AttributesStore attributeStore in _attributeStores)
                    {
                        attributeStore.RefreshUpstreamInputs(session);
                    }

                    // Don't want to upload right now because the buffer sizes might
                    // not be in sync (especially for delete curve points).
                    // Instead wait until after Houdini cook is completed then upload.
                    // So early out here.
                    return;
                }

                foreach (HEU_AttributesStore attributeStore in _attributeStores)
                {
                    if (attributeStore.HasDirtyAttributes())
                    {
                        bForceUpload = true;
                        break;
                    }
                }
            }

            foreach (HEU_AttributesStore attributeStore in _attributeStores)
            {
                if (bForceUpload || attributeStore.HasDirtyAttributes())
                {
                    if (_toolsInfo != null && _toolsInfo._alwaysCookUpstream)
                    {
                        attributeStore.RefreshUpstreamInputs(session);
                    }

                    attributeStore.SyncDirtyAttributesToHoudini(session);
                }
            }
        }

        private void SyncDirtyAttributesToHoudini(HEU_SessionBase session)
        {
            foreach (HEU_AttributesStore attributeStore in _attributeStores)
            {
                if (attributeStore.AreAttributesDirty())
                {
                    attributeStore.SyncDirtyAttributesToHoudini(session);
                }
            }
        }

        private void UploadInputNodes(HEU_SessionBase session, bool bForceUpdate, bool bUpdateAll)
        {
            foreach (HEU_InputNode inputNode in _inputNodes)
            {
                if (inputNode == null)
                {
                    continue;
                }

                // Upload all but parameter types, as those are taken care of in the parameter update
                if ((inputNode.InputType != HEU_InputNode.InputNodeType.PARAMETER || bUpdateAll)
                    && (bForceUpdate || inputNode.RequiresUpload || inputNode.HasInputNodeTransformChanged())
                    && inputNode.InputNodeID != HEU_Defines.HEU_INVALID_NODE_ID)
                {
                    if (bForceUpdate)
                    {
                        inputNode.ResetConnectionForForceUpdate(session);
                    }

                    inputNode.UploadInput(session);
                }
            }
        }

        internal bool HasInputNodeTransformChanged()
        {
            foreach (HEU_InputNode inputNode in _inputNodes)
            {
                if (inputNode == null)
                    return true;

                if (inputNode.HasInputNodeTransformChanged())
                {
                    return true;
                }
            }

            return false;
        }

        private void NotifyInputNodesCookFinished()
        {
            foreach (HEU_InputNode inputNode in _inputNodes)
            {
                if (inputNode.RequiresCook)
                {
                    inputNode.RequiresCook = false;
                }
            }
        }

        /// <summary>
        /// Creates object nodes for this asset.
        /// In turn, geo nodes and parts will be created as well.
        /// </summary>
        /// <returns>True if successfully created all the asset's objects and their data</returns>
        private bool CreateObjects(HEU_SessionBase session)
        {
            Debug.Assert(_objectNodes.Count == 0, HEU_Defines.HEU_NAME + ": Object list must be empty!");

            // Fill in object infos and transforms based on node type and number of child objects
            HAPI_ObjectInfo[] objectInfos = null;
            HAPI_Transform[] objectTransforms = null;

            if (!HEU_HAPIUtility.GetObjectInfos(session, _assetID, ref _nodeInfo, out objectInfos, out objectTransforms))
            {
                return false;
            }

            Debug.Assert(objectInfos.Length == objectTransforms.Length, HEU_Defines.HEU_NAME + ": Object info and object transform array mismatch!");

            // Create object nodes
            int numObjects = objectInfos.Length;
            for (int i = 0; i < numObjects; ++i)
            {
                _objectNodes.Add(CreateObjectNode(session, ref objectInfos[i], ref objectTransforms[i]));
            }

            return true;
        }

        /// <summary>
        /// Synchronizes all local objects with Houdini session.
        /// Creates new objects, and removes old objects no longer in use.
        /// Refreshes each object to make their its internal state is
        /// synchronized.
        /// </summary>
        /// <returns>True if any changes were applied</returns>
        private void UpdateAllObjectNodes(HEU_SessionBase session)
        {
            // Fill in latest object infos and transforms based on node type and number of child objects
            HAPI_ObjectInfo[] objectInfos = null;
            HAPI_Transform[] objectTransforms = null;

            if (!HEU_HAPIUtility.GetObjectInfos(session, _assetID, ref _nodeInfo, out objectInfos, out objectTransforms))
            {
                return;
            }

            // We need to go through the new list of object infos and 
            // check against our internal state. 
            // For new object infos, add new object nodes. 
            // Remove any unused object nodes.
            // Then refresh all object nodes.

            List<HEU_ObjectNode> newObjectNodes = new List<HEU_ObjectNode>();

            int numObjNodes = _objectNodes.Count;
            int numObjInfos = objectInfos.Length;
            if (numObjInfos == 1)
            {
                if (numObjNodes == 1)
                {
                    // Since its just 1 object found and 1 object we already had, presume they're the same
                    // Update object info, add to new list
                    _objectNodes[0].SetObjectInfo(objectInfos[0]);
                    newObjectNodes.Add(_objectNodes[0]);
                }
                else
                {
                    // Our internal number of objects don't match. We can't really try to match because names
                    // could be different. So we'll create as new object and warn user.
                    newObjectNodes.Add(CreateObjectNode(session, ref objectInfos[0], ref objectTransforms[0]));

                    if (numObjNodes > 1)
                    {
                        HEU_Logger.LogWarning(
                            "Unable to match previous objects with new objects after cooking this asset. Might lose asset changes.");
                    }
                }
            }
            else
            {
                // Multiple objects found. Try to match them via name.

                for (int infoIndex = 0; infoIndex < numObjInfos; ++infoIndex)
                {
                    // Find object
                    bool bFound = false;

                    for (int nodeIndex = 0; nodeIndex < numObjNodes; ++nodeIndex)
                    {
                        string objName = HEU_SessionManager.GetString(objectInfos[infoIndex].nameSH, session);
                        if (objName.Equals(_objectNodes[nodeIndex].ObjectName))
                        {
                            // Update object info, add to new list
                            _objectNodes[nodeIndex].SetObjectInfo(objectInfos[infoIndex]);
                            newObjectNodes.Add(_objectNodes[nodeIndex]);
                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound)
                    {
                        // New object
                        newObjectNodes.Add(CreateObjectNode(session, ref objectInfos[infoIndex], ref objectTransforms[infoIndex]));
                    }
                }
            }

            // Go through _objectNodes and remove any nodes not in new list
            numObjNodes = _objectNodes.Count;
            if (numObjNodes > 0)
            {
                for (int i = 0; i < numObjNodes; ++i)
                {
                    if (!newObjectNodes.Contains(_objectNodes[i]))
                    {
                        _objectNodes[i].DestroyAllData();
                        HEU_GeneralUtility.DestroyImmediate(_objectNodes[i]);
                    }
                }

                _objectNodes.Clear();
            }

            //HEU_Logger.LogFormat(HEU_Defines.HEU_NAME + ": Replacing {0} old objects with {1} new objects!", numObjNodes, newObjectNodes.Count);

            // Update to new list
            _objectNodes = newObjectNodes;

            // Now refresh all object nodes
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.UpdateObject(session, true);
            }
        }

        // Creates a new object node
        private HEU_ObjectNode CreateObjectNode(HEU_SessionBase session, ref HAPI_ObjectInfo objectInfo, ref HAPI_Transform objectTranform)
        {
            HEU_ObjectNode objectNode = ScriptableObject.CreateInstance<HEU_ObjectNode>();
            objectNode.Initialize(session, objectInfo, objectTranform, this, _useOutputNodes);
            return objectNode;
        }

        /// <summary>
        /// Generate geometry (mesh, curves, terrain) for all object nodes.
        /// </summary>
        /// <param name="session">Current session</param>
        /// <param name="bRebuild">True if this is a rebuild or recook</param>
        private void GenerateObjectsGeometry(HEU_SessionBase session, bool bRebuild)
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.GenerateGeometry(session, bRebuild);
            }
        }

        internal int NumAttributeStores()
        {
            return _attributeStores != null ? _attributeStores.Count : 0;
        }

        internal HEU_AttributesStore GetAttributeStore(string geoName, HAPI_PartId partID)
        {
            foreach (HEU_AttributesStore attrStore in _attributeStores)
            {
                if (attrStore.GeoName.Equals(geoName) && attrStore.PartID == partID)
                {
                    return attrStore;
                }
            }

            return null;
        }

        private void GenerateAttributesStore(HEU_SessionBase session)
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.GenerateAttributesStore(session);
            }
        }

        /// <summary>
        /// Generate instances for all object nodes.
        /// </summary>
        /// <param name="session"></param>
        private void GenerateInstances(HEU_SessionBase session)
        {
            // Instancing - process part instances first, then do object instances.
            // This assures that if objects being instanced have all their parts completed.

            // Clear part instances, to make sure that the object instances don't overwrite the part instances.
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                if (objNode == null)
                    continue;

                if (objNode.IsInstancer())
                {
                    objNode.ClearObjectInstances(session);
                }
            }

            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                if (objNode == null)
                    continue;

                objNode.GeneratePartInstances(session);
            }

            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                if (objNode == null)
                    continue;

                if (objNode.IsInstancer())
                {
                    objNode.GenerateObjectInstances(session);
                }
            }
        }

        private void GenerateHandles(HEU_SessionBase session)
        {
            // This will get us an updated list of handles in this asset
            List<HEU_Handle> newHandles =
                HEU_GeneralUtility.FindOrGenerateHandles(session, ref _assetInfo, _assetID, _assetName, _parameters, _handles);

            // Clean up handles not in new list
            int numHandles = _handles.Count;
            for (int i = 0; i < numHandles; ++i)
            {
                if (!newHandles.Contains(_handles[i]))
                {
                    _handles[i].CleanUp();
                    HEU_GeneralUtility.DestroyImmediate(_handles[i]);
                    _handles[i] = null;
                }
            }

            _handles.Clear();

            _handles = newHandles;
        }

        internal void CleanUpHandles()
        {
            if (_handles != null)
            {
                for (int i = 0; i < _handles.Count; ++i)
                {
                    _handles[i].CleanUp();
                    HEU_GeneralUtility.DestroyImmediate(_handles[i]);
                    _handles[i] = null;
                }

                _handles.Clear();
            }
        }

        internal HEU_Handle GetHandleByName(string handleName)
        {
            foreach (HEU_Handle handle in _handles)
            {
                if (handle.HandleName.Equals(handleName))
                {
                    return handle;
                }
            }

            return null;
        }

        internal List<HEU_Handle> GetHandles()
        {
            return _handles;
        }

        internal int NumHandles()
        {
            return _handles != null ? _handles.Count : 0;
        }

        /// <summary>
        /// Returns the given object's (OBJ) transform.
        /// The returned transform is based on what type of asset this is (SOP, OBJ with and
        /// without children)
        /// </summary>
        /// <param name="objectID">The ID of the object to query</param>
        /// <returns>A transform for this object</returns>
        internal HAPI_Transform GetObjectTransform(HEU_SessionBase session, HAPI_NodeId objectID)
        {
            if (_nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP)
            {
                return new HAPI_Transform(true);
            }
            else if (_nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ)
            {
                int objectCount = 0;
                if (!session.ComposeObjectList(AssetID, out objectCount))
                {
                    return new HAPI_Transform(true);
                }

                if (objectCount <= 0)
                {
                    return new HAPI_Transform(true);
                }
                else
                {
                    HAPI_Transform hapiTransform = new HAPI_Transform(true);
                    session.GetObjectTransform(objectID, AssetID, HAPI_RSTOrder.HAPI_SRT, ref hapiTransform);
                    if (Mathf.Approximately(0f, hapiTransform.scale[0]) || Mathf.Approximately(0f, hapiTransform.scale[1]) ||
                        Mathf.Approximately(0f, hapiTransform.scale[2]))
                    {
                        HEU_Logger.LogWarning(string.Format(
                            HEU_Defines.HEU_NAME + ": Object id {0} for asset {1} has scale components with 0 values!", objectID, AssetName));
                    }
                    else
                    {
                        return hapiTransform;
                    }
                }
            }

            return new HAPI_Transform(true);
        }

        /// <summary>
        /// Returns the object node (OBJ) with specified ID
        /// </summary>
        /// <param name="objId">The object ID to match</param>
        /// <returns>Object node with specified ID</returns>
        internal HEU_ObjectNode GetObjectWithID(HAPI_NodeId objId)
        {
            int numObjects = _objectNodes.Count;
            for (int i = 0; i < numObjects; ++i)
            {
                if (_objectNodes[i].ObjectID == objId)
                {
                    return _objectNodes[i];
                }
            }

            return null;
        }

        private void InvokeBakedEvent(bool bSuccess, List<GameObject> outputObjects, bool isNewBake)
        {
            if (_bakedDataEvent != null)
            {
                _bakedDataEvent.Invoke(new HEU_BakedEventData(this, bSuccess, outputObjects, isNewBake));
            }
        }

        /// <summary>
        /// Return a clone of this asset. The returned object might be a single
        /// gameobject containing relevant components such as mesh, collider, materials, and textures.
        /// It could also be a root object with several children underneath corresponding to an 
        /// asset with multiple objects and/or instances.
        /// </summary>
        /// <param name="bakedAssetPath">Reference to the new clone's asset path, or empty if not filled in.</param>
        /// <param name="bWriteMeshesToAssetDatabase">Whether to write meshes to persistant storage (asset database)</param>
        /// <returns>The new gameobject containing the cloned data</returns>
        private GameObject CloneAssetWithoutHDA(ref string bakedAssetPath, bool bWriteMeshesToAssetDatabase, bool bReconnectPrefabInstances)
        {
            GameObject newRoot = null;

            if (_rootGameObject == null)
            {
                HEU_Logger.LogErrorFormat("{0}: Unable to bake due to no HEU_HoudiniAssetRoot found!", HEU_Defines.HEU_NAME);
                return newRoot;
            }

            // If we're storing meshes in Asset Database, then we need  to create an asset object to store inside.
            UnityEngine.Object newAssetDBObject = null;

            // Need to get raw OP name otherwise duplicates may mess up the naming
            string rawOpName = HEU_GeneralUtility.GetRawOperatorName(_assetOpName);
            string newAssetDBObjectFileName = HEU_AssetDatabase.AppendMeshesAssetFileName(rawOpName);

            Transform rootTransform = _rootGameObject.transform;
            int numCreatedObjects = 0;

            // First get a list of clonable parts of the asset.
            // Then copy each part's gameobject, and place it under a common root if there are multiple objects. 
            // If just a single object, no separate root needed.

            // As we find meshes, we add to a map. The map helps track whether we already created
            // unique copy that can be shared.
            Dictionary<Mesh, Mesh> sourceToTargetMeshMap = new Dictionary<Mesh, Mesh>();

            // Map of materials copied for corresponding source materials (for reuse)
            Dictionary<Material, Material> sourceToCopiedMaterials = new Dictionary<Material, Material>();

            List<HEU_PartData> clonableParts = new List<HEU_PartData>();
            GetClonableParts(clonableParts);

            if (clonableParts.Count > 0)
            {
                Transform newRootTransform = null;

                // Children go under a common root
                if (clonableParts.Count > 1)
                {
                    newRoot = HEU_GeneralUtility.CreateNewGameObject();
                    newRootTransform = newRoot.transform;
                    HEU_GeneralUtility.CopyWorldTransformValues(rootTransform, newRootTransform);
                }

                foreach (HEU_PartData clonePart in clonableParts)
                {
                    GameObject newChildGO = clonePart.BakePartToNewGameObject(newRootTransform, bWriteMeshesToAssetDatabase, ref bakedAssetPath,
                        sourceToTargetMeshMap, sourceToCopiedMaterials, ref newAssetDBObject, newAssetDBObjectFileName, bReconnectPrefabInstances);

                    // In case of only 1 object being cloned, we'll set that as the newRoot
                    if (newChildGO != null)
                    {
                        if (newRoot == null)
                        {
                            newRoot = newChildGO;
                            newRootTransform = newRoot.transform;
                            // Instead of copying, multiplying the root transform to keep local offets (e.g. terrain)
                            HEU_GeneralUtility.ApplyTransformTo(rootTransform, newRootTransform);
                        }

                        numCreatedObjects++;
                    }
                }

                HEU_AssetDatabase.SaveAndRefreshDatabase();
            }

            if (newRoot != null)
            {
                if (numCreatedObjects != 0)
                {
                    HEU_GeneralUtility.RenameGameObject(newRoot, _assetName + HEU_Defines.HEU_BAKED_HDA);
                }
                else
                {
                    // Delete the root as nothing was generated
                    HEU_GeneralUtility.DestroyImmediate(newRoot);
                }
            }

            if (numCreatedObjects == 0)
            {
                HEU_Logger.LogFormat("Nothing to bake as no geometry available!");
            }

            return newRoot;
        }


        // EVENTS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Callback after upstream connection input node has cooked.
        /// </summary>
        /// <param name="asset">The asset that cooked and invoking us</param>
        /// <param name="bSuccess">Whether cook succeeded</param>
        /// <param name="outputs">Output gameobjects</param>
        internal void NotifyUpstreamCooked(HEU_CookedEventData Data)
        {
            if (Data.CookSuccess)
            {
                //HEU_Logger.LogFormat("NotifyUpstreamCooked from {0}", AssetName);
                HEU_SessionBase session = GetAssetSession(true);

                // Required for reverting and uploading local data for edit nodes
                _upstreamCookChanged = true;

                // Recook after upstream cook
                // Check parameter changes otherwise can cause input nodes to be disconnected
                // then reconnected.
                RequestCook(true, true, false, true);
            }
        }

        internal void ConnectToUpstream(HEU_HoudiniAsset upstreamAsset)
        {
            upstreamAsset.AddDownstreamConnection(this.NotifyUpstreamCooked);
        }

        internal void DisconnectFromUpstream(HEU_HoudiniAsset upstreamAsset)
        {
            upstreamAsset.RemoveDownstreamConnection(this.NotifyUpstreamCooked);
        }

        // Adds a downstream connection (If an HDA references this as input)
        private void AddDownstreamConnection(UnityEngine.Events.UnityAction<HEU_CookedEventData> receiver)
        {
            // Doing remove first makes sure we don't have duplicate entries for the receiver
            _downstreamConnectionCookedEvent.RemoveListener(receiver);
            _downstreamConnectionCookedEvent.AddListener(receiver);
        }

        private void RemoveDownstreamConnection(UnityEngine.Events.UnityAction<HEU_CookedEventData> receiver)
        {
            _downstreamConnectionCookedEvent.RemoveListener(receiver);
        }

        private void ClearAllUpstreamConnections()
        {
            if (_parameters != null)
            {
                List<GameObject> inputNodeObjects = new List<GameObject>();
                _parameters.GetInputNodeConnectionObjects(inputNodeObjects);
                foreach (GameObject go in inputNodeObjects)
                {
                    HEU_HoudiniAssetRoot assetRoot = go.GetComponent<HEU_HoudiniAssetRoot>();
                    if (assetRoot != null && assetRoot._houdiniAsset != null)
                    {
                        DisconnectFromUpstream(assetRoot._houdiniAsset);
                    }
                }
            }
        }

        /// <summary>
        /// This will update input assets when this asset gets recreated in session so
        /// that any input IDs in use will be invalidated or updated.
        /// </summary>
        /// <param name="session">Session that the asset was recreated in</param>
        private void UpdateInputsOnAssetRecreation(HEU_SessionBase session)
        {
            foreach (HEU_InputNode inputNode in _inputNodes)
            {
                inputNode.UpdateOnAssetRecreation(session);
            }
        }

        /// <summary>
        /// Reconnect to upstream input assets for notifications so that when they
        /// are recooked, this asset will get notified.
        /// This should fix up issues on play mode change, code compilation,
        /// and scene load where the input assets don't trigger the callback.
        /// </summary>
        internal void ReconnectInputsUpstreamNotifications()
        {
            if (_inputNodes != null)
            {
                foreach (HEU_InputNode input in _inputNodes)
                {
                    if (input != null)
                    {
                        input.ReconnectToUpstreamAsset();
                    }
                }
            }
        }

        // TRANSFORMS -------------------------------------------------------------------------------------------------

        internal void GetHoudiniTransformAndApply(HEU_SessionBase session)
        {
            int queryNodeID = _assetID;
            if (_nodeInfo.id != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                if (_nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP)
                {
                    queryNodeID = _nodeInfo.parentId;
                    Debug.Assert(queryNodeID != HEU_Defines.HEU_INVALID_NODE_ID, "Invalid parent ID for SOP!");
                }
                else if (_nodeInfo.type != HAPI_NodeType.HAPI_NODETYPE_OBJ)
                {
                    return;
                }

                HAPI_Transform hapiTransform = new HAPI_Transform(true);
                session.GetObjectTransform(queryNodeID, -1, HAPI_RSTOrder.HAPI_SRT, ref hapiTransform);
                if (Mathf.Approximately(0f, hapiTransform.scale[0]) || Mathf.Approximately(0f, hapiTransform.scale[1]) ||
                    Mathf.Approximately(0f, hapiTransform.scale[2]))
                {
                    HEU_Logger.LogWarningFormat("Asset id {0} with name {1} has scale components with 0 values!", AssetID, AssetName);
                }

                // Using root transform as that represents our asset in the world
                HEU_HAPIUtility.ApplyWorldTransfromFromHoudiniToUnity(ref hapiTransform, _rootGameObject.transform);

                // Save last sync'd transform
                _lastSyncedTransformMatrix = _rootGameObject.transform.localToWorldMatrix;
                SyncChildTransforms();
            }
        }

        internal void UploadUnityTransform(HEU_SessionBase session, bool bOnlySendIfChangedFromLastSync)
        {
            int queryNodeID = _assetID;
            if (_nodeInfo.id != HEU_Defines.HEU_INVALID_NODE_ID)
            {
                if (_nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP)
                {
                    queryNodeID = _nodeInfo.parentId;
                    Debug.Assert(queryNodeID != HEU_Defines.HEU_INVALID_NODE_ID, "Invalid parent ID for SOP!");
                }
                else if (_nodeInfo.type != HAPI_NodeType.HAPI_NODETYPE_OBJ)
                {
                    return;
                }

                if (!session.IsSessionValid())
                {
                    return;
                }

                Matrix4x4 transformMatrix = _rootGameObject.transform.localToWorldMatrix;

                if (bOnlySendIfChangedFromLastSync)
                {
                    if (!HasTransformChangedSinceLastUpdate())
                    {
                        return;
                    }
                }

                HAPI_TransformEuler transformEuler = HEU_HAPIUtility.GetHAPITransformFromMatrix(ref transformMatrix);
                if (!session.SetObjectTransform(queryNodeID, ref transformEuler))
                {
                    HEU_Logger.LogWarningFormat("Unable to upload transform for asset id {0} with name {1}!", AssetID, AssetName);
                }
                else
                {
                    _lastSyncedTransformMatrix = transformMatrix;
                    SyncChildTransforms();
                }

                // Not updating parameters after setting object transform as that is
                // updating the pre-transform, causing double transform issues.
                // Instead let user set pre - transform via the Parameters UI.
            }
        }


        // MATERIALS --------------------------------------------------------------------------------------------------

        /// <summary>
        /// Go through the materials in use and update them with values from Houdini.
        /// </summary>
        private void UpdateHoudiniMaterials(HEU_SessionBase session)
        {
            HAPI_MaterialInfo materialInfo = new HAPI_MaterialInfo();

            foreach (HEU_MaterialData materialData in _materialCache)
            {
                // Non-Houdini material so no need to update it.
                if (materialData == null || materialData._materialSource != HEU_MaterialData.Source.HOUDINI ||
                    materialData._materialKey == HEU_Defines.HEU_INVALID_MATERIAL
                    || materialData._materialKey == HEU_Defines.EDITABLE_MATERIAL_KEY)
                {
                    continue;
                }

                session.GetMaterialInfo(materialData._materialKey, ref materialInfo, false);

                //HEU_Logger.LogFormat("Material id={0}, exists={1}, changed={2}", materialData._materialID, materialInfo.exists, materialInfo.hasChanged);

                if (materialInfo.exists)
                {
                    if (materialInfo.hasChanged)
                    {
                        materialData.UpdateMaterialFromHoudini(materialInfo, GetValidAssetCacheFolderPath());
                    }
                }
            }
        }

        private void RemoveUnusedMaterials()
        {
            // Find the unused materials
            List<HEU_MaterialData> materialsToRemove = new List<HEU_MaterialData>();
            foreach (HEU_MaterialData materialData in _materialCache)
            {
                bool bUsing = false;

                foreach (HEU_ObjectNode objectNode in _objectNodes)
                {
                    if (objectNode.IsUsingMaterial(materialData))
                    {
                        bUsing = true;
                        break;
                    }
                }

                if (!bUsing)
                {
                    materialsToRemove.Add(materialData);
                }
            }

            // Delete them
            for (int i = 0; i < materialsToRemove.Count; ++i)
            {
                HEU_MaterialData materialData = materialsToRemove[i];
                // Skipping editable materials as those are needed when painting
                if ((materialData._materialSource == HEU_MaterialData.Source.HOUDINI ||
                     materialData._materialSource == HEU_MaterialData.Source.DEFAULT)
                    && materialData._materialKey != HEU_Defines.EDITABLE_MATERIAL_KEY)
                {
                    // Houdini materials and default material were created dynamically so we can delete them
                    // TODO: Currently not deleting any textures not in use. Revisit to address it
                    HEU_MaterialFactory.DeleteAssetMaterial(materialData._material);
                }

                // Other materials (Unity, Substance) were presumably part of the project
                // already and not created by us, so we don't explicility delete them.

                // Now remove from our cache which should clear reference
                _materialCache.Remove(materialData);

                HEU_GeneralUtility.DestroyImmediate(materialData, false);
            }
        }

        // MISC -------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the asset is valid in given Houdini session
        /// </summary>
        /// <returns>True if the asset is valid in Houdini</returns>
        internal bool IsAssetValidInHoudini(HEU_SessionBase session)
        {
            return HEU_HAPIUtility.IsNodeValidInHoudini(session, _assetID) && session.IsAssetRegistered(this);
        }


        /// <summary>
        /// Returns true if the current asset transform has changed from
        /// the last upload to HAPI.
        /// </summary>
        /// <returns>True if transform has changed since last upload</returns>
        internal bool HasTransformChangedSinceLastUpdate()
        {
            if (_lastSyncedTransformMatrix != _rootGameObject.transform.localToWorldMatrix)
            {
                return true;
            }

            bool recursive = HEU_PluginSettings.ChildTransformChangeTriggersCooks;

            if (recursive && _lastSyncedChildTransformMatrices != null)
            {
                List<Matrix4x4> curTransformValues = new List<Matrix4x4>();
                HEU_InputUtility.GetChildrenTransforms(_rootGameObject.transform, ref curTransformValues);

                if (_lastSyncedChildTransformMatrices.Count != curTransformValues.Count)
                {
                    return true;
                }

                for (int i = 0; i < curTransformValues.Count; i++)
                {
                    if (_lastSyncedChildTransformMatrices[i] != curTransformValues[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal void GetClonableParts(List<HEU_PartData> clonableParts)
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                if (!objNode.IsInstanced() || objNode.IsVisible())
                {
                    objNode.GetClonableParts(clonableParts);
                }
            }
        }

        // Curves can be invalid if destroyed by undo.
        internal void ClearInvalidCurves()
        {
            _curves = _curves.Filter((HEU_Curve curve) => curve != null);
        }

        internal int GetEditableCurveCount()
        {
            ClearInvalidCurves();

            int count = 0;
            foreach (HEU_Curve curve in _curves)
            {
                if (curve.IsEditable())
                {
                    count++;
                }
            }

            return count;
        }

        internal void AddCurve(HEU_Curve curve)
        {
            if (!_curves.Contains(curve))
            {
                _curves.Add(curve);
            }
        }

        internal void RemoveCurve(HEU_Curve curve)
        {
            _curves.Remove(curve);
        }

        internal List<HEU_InputNode> GetInputNodes()
        {
            return _inputNodes;
        }


        internal void AddInputNode(HEU_InputNode node)
        {
            if (!_inputNodes.Contains(node))
            {
                _inputNodes.Add(node);
            }
        }

        internal void RemoveInputNode(HEU_InputNode node)
        {
            _inputNodes.Remove(node);
        }

        internal void InputNodeNotifyRemoved(HEU_InputNode node)
        {
            RemoveInputNode(node);
        }

        internal void AddVolumeCache(HEU_VolumeCache cache)
        {
            if (!_volumeCaches.Contains(cache))
            {
                _volumeCaches.Add(cache);
            }
        }

        internal void RemoveVolumeCache(HEU_VolumeCache cache)
        {
            if (cache != null)
            {
                _volumeCaches.Remove(cache);
            }
        }

        internal void AddAttributeStore(HEU_AttributesStore attributeStore)
        {
            if (!_attributeStores.Contains(attributeStore))
            {
                // Add in alphabetical order of GeoName. This allows users to use editable node names to
                // set order of edit operations.
                int numAttrs = _attributeStores.Count;
                for (int i = 0; i < numAttrs; ++i)
                {
                    if (string.Compare(attributeStore.GeoName, _attributeStores[i].GeoName) < 0)
                    {
                        _attributeStores.Insert(i, attributeStore);
                        return;
                    }
                }

                _attributeStores.Add(attributeStore);
            }
        }

        internal void RemoveAttributeStore(HEU_AttributesStore attributeStore)
        {
            _attributeStores.Remove(attributeStore);
        }

        /// <summary>
        /// Move the attribute store at oldIndex to newIndex.
        /// </summary>
        /// <param name="oldIndex">The attribute store at this index will be moved</param>
        /// <param name="newIndex">The new index to move it to</param>
        internal void ReorderAttributeStore(int oldIndex, int newIndex)
        {
            int count = _attributeStores.Count;

            if (oldIndex == newIndex || oldIndex < 0 || oldIndex >= count || newIndex < 0 || newIndex >= count)
            {
                return;
            }

            HEU_AttributesStore attrStore = _attributeStores[oldIndex];

            if ((oldIndex < newIndex) || (newIndex < count - 1))
            {
                _attributeStores.RemoveAt(oldIndex);
                _attributeStores.Insert(newIndex, attrStore);
            }
        }


        /// <summary>
        /// Fill in the objInstanceInfos list with the HEU_ObjectInstanceInfos used by this asset.
        /// </summary>
        /// <param name="objInstanceInfos">List to fill in</param>
        internal void PopulateObjectInstanceInfos(List<HEU_ObjectInstanceInfo> objInstanceInfos)
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                if (objNode != null)
                {
                    objNode.PopulateObjectInstanceInfos(objInstanceInfos);
                }
            }
        }

        /// <summary>
        /// Add given object to this asset's asset database cache.
        /// </summary>
        /// <param name="assetObjectFileName">File name of asset database object</param>
        /// <param name="objectToAdd">The object to add</param>
        /// <param name="targetAssetDBObject">Existing asset database object to overwrite or null. Returns valid written object.</param>
        internal void AddToAssetDBCache(string assetObjectFileName, UnityEngine.Object objectToAdd, string relativeFolderPath,
            ref UnityEngine.Object targetAssetDBObject)
        {
            // Once the asset cache folder is set, CreateAddObjectInAssetCacheFolder will not update it
            string assetCacheFolder = GetValidAssetCacheFolderPath();
            HEU_AssetDatabase.CreateAddObjectInAssetCacheFolder(AssetName, assetObjectFileName, objectToAdd, relativeFolderPath, ref assetCacheFolder,
                ref targetAssetDBObject);
        }

        /// <summary>
        /// Show or hide all curves in the current scene.
        /// </summary>
        /// <param name="bShow">True to show</param>
        internal static void SetCurvesVisibilityInScene(bool bShow)
        {
            HEU_HoudiniAsset[] houdiniAssets = GameObject.FindObjectsOfType<HEU_HoudiniAsset>();
            foreach (HEU_HoudiniAsset asset in houdiniAssets)
            {
                List<HEU_Curve> curves = asset.Curves;
                foreach (HEU_Curve curve in curves)
                {
                    curve.SetCurveGeometryVisibility(bShow);
                }
            }
        }

        /// <summary>
        /// Returns a valid asset cache folder path for this asset.
        /// Creates the cache folder path if not already done so.
        /// </summary>
        /// <returns>Valid asset cache folder path</returns>
        internal string GetValidAssetCacheFolderPath()
        {
            if (string.IsNullOrEmpty(_assetCacheFolderPath))
            {
                // Create folder based on asset name + unique id in plugin cache folder
                // Store materials and textures in folder
                // Delete folder when asset is deleted

                string suggestedFileName = _assetPath;

                if (string.IsNullOrEmpty(suggestedFileName)
                    && (_assetType == HEU_AssetType.TYPE_CURVE || _assetType == HEU_AssetType.TYPE_INPUT))
                {
                    // Since curves and input nodes are not loaded from an asset file, use the gameobject name
                    suggestedFileName = _rootGameObject.name;
                }

                _assetCacheFolderPath = HEU_AssetDatabase.CreateAssetCacheFolder(suggestedFileName, this.GetHashCode());
            }

            return _assetCacheFolderPath;
        }

        /// <summary>
        /// Calculate visiblity of all geometry within
        /// </summary>
        internal void CalculateVisibility()
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.CalculateVisibility();
            }
        }


        /// <summary>
        /// Calculate visiblity of all geometry within
        /// </summary>
        internal void CalculateColliderState()
        {
            foreach (HEU_ObjectNode objNode in _objectNodes)
            {
                objNode.CalculateColliderState();
            }
        }

        /// <summary>
        /// Load the parameter preset and curve presets contained in assetPreset into
        /// this asset, then cook it with the updated parameter values.
        /// </summary>
        /// <param name="assetPreset">Object containing parameter and curve presets</param>
        internal void LoadAssetPresetAndCook(HEU_AssetPreset assetPreset)
        {
            HEU_SessionBase session = GetAssetSession(true);

            if (!assetPreset._assetOPName.Equals(_assetOpName))
            {
                string presetErrorMsg =
                    string.Format(
                        "The saved asset OP name from preset file: '{0}'\ndiffers from this asset's OP name: '{1}'.\nMake sure you are using the correct preset file.",
                        assetPreset._assetOPName, _assetOpName);
                if (!HEU_EditorUtility.DisplayDialog("Preset Does Not Match", presetErrorMsg, "Continue Anyway", "Cancel"))
                {
                    HEU_Logger.LogWarningFormat("Unable to load preset due to mismatch of asset OP name!");
                    return;
                }
                else
                {
                    HEU_Logger.LogWarningFormat("Saved preset does not match asset OP name. User selected to continue to load the preset.");
                }
            }

            // TODO: Load asset options

            if (_parameters != null && assetPreset._parameterPreset != null)
            {
                _parameters.SetPresetData(assetPreset._parameterPreset);
            }

            ClearInvalidCurves();

            bool failedFindingCurves = false;

            int numCurves = assetPreset._curveNames.Count;
            for (int i = 0; i < numCurves; ++i)
            {
                if (assetPreset._curvePresets[i] != null)
                {
                    HEU_Curve curve = GetCurve(assetPreset._curveNames[i]);
                    if (curve != null)
                    {
                        curve.SetCurveParameterPreset(session, this, assetPreset._curvePresets[i]);
                    }
                    else
                    {
                        failedFindingCurves = true;
                        break;
                    }
                }
            }

            // Do a second pass in case the curve names changed.
            if (failedFindingCurves)
            {
                for (int i = 0; i < numCurves; ++i)
                {
                    if (i >= _curves.Count)
                    {
                        HEU_Logger.LogWarningFormat("Curve with name {0} not found for loading its parameter preset!", assetPreset._curveNames[i]);
                    }
                    else
                    {
                        HEU_Curve curve = _curves[i];
                        if (curve != null)
                        {
                            curve.SetCurveParameterPreset(session, this, assetPreset._curvePresets[i]);
                        }
                    }
                }
            }

            // Load input nodes (reattach connections)
            ApplyInputPresets(session, assetPreset.inputPresets, true);

            // Load volume caches (for terrain layers). Note that some of the volume cache presets
            // might have already been applied during rebuild, but they should have been removed from this list.
            // Whatever is leftover are for volume caches which might not have been created during this cook,
            // so should be added to the recook presets.
            if (assetPreset.volumeCachePresets != null && assetPreset.volumeCachePresets.Count > 0)
            {
                if (_recookPreset == null)
                {
                    _recookPreset = new HEU_RecookPreset();
                }

                _recookPreset._volumeCachePresets.AddRange(assetPreset.volumeCachePresets);
            }

            Parameters.RecacheUI = true;

            RecookBlocking(bCheckParamsChanged: false, bSkipCookCheck: true,
                bUploadParameters: false, bUploadParameterPreset: true,
                bForceUploadInputs: false, bCookingSessionSync: false);
        }

        /// <summary>
        /// Applies presets after recooking the asset.
        /// </summary>
        internal void ApplyRecookPreset()
        {
            if (_recookPreset != null)
            {
                bool bApplied = ApplyInputPresets(GetAssetSession(true), _recookPreset._inputPresets, false);
                bApplied |= ApplyVolumeCachePresets(_recookPreset._volumeCachePresets);

                _recookPreset = null;
                if (bApplied)
                {
                    Parameters.RecacheUI = true;
                    RequestCook(bCheckParametersChanged: false, bAsync: true, bSkipCookCheck: true, bUploadParameters: false);
                }
            }
        }

        private bool ApplyInputPresets(HEU_SessionBase session, List<HEU_InputPreset> inputPresets, bool bAddMissingInputsToRecookPreset)
        {
            bool bApplied = false;

            if (inputPresets != null && inputPresets.Count > 0)
            {
                foreach (HEU_InputPreset inputPreset in inputPresets)
                {
                    HEU_InputNode inputNode = GetInputNode(inputPreset._inputName);
                    if (inputNode != null)
                    {
                        inputNode.LoadPreset(session, inputPreset);
                        bApplied = true;
                    }
                    else
                    {
                        if (bAddMissingInputsToRecookPreset)
                        {
                            if (_recookPreset == null)
                            {
                                _recookPreset = new HEU_RecookPreset();
                            }

                            _recookPreset._inputPresets.Add(inputPreset);
                        }
                        else
                        {
                            HEU_Logger.LogWarningFormat("Input node with name {0} not found! Unable to set input preset.", inputPreset._inputName);
                        }
                    }
                }
            }

            return bApplied;
        }

        internal HEU_VolumeCachePreset GetVolumeCachePreset(string objName, string geoName, int tile)
        {
            if (_savedAssetPreset == null || _savedAssetPreset.volumeCachePresets == null)
            {
                return null;
            }

            foreach (HEU_VolumeCachePreset volumeCachePreset in _savedAssetPreset.volumeCachePresets)
            {
                if (volumeCachePreset._objName.Equals(objName) && volumeCachePreset._geoName.Equals(geoName) && volumeCachePreset._tile == tile)
                {
                    return volumeCachePreset;
                }
            }

            return null;
        }

        internal void RemoveVolumeCachePreset(HEU_VolumeCachePreset preset)
        {
            if (_savedAssetPreset != null && _savedAssetPreset.volumeCachePresets != null)
            {
                _savedAssetPreset.volumeCachePresets.Remove(preset);
            }
        }

        /// <summary>
        /// Applies volumecache presets to volume parts. This sets terrain layer settings such as material.
        /// </summary>
        /// <param name="volumeCachePresets">The source volumecache preset to apply</param>
        /// <param name="bAddMissingVolumesToRecookPreset">Whether to add unapplied presets to the RecookPreset for applying later</param>
        /// <returns>True if applied the preset, therefore requiring another recook.</returns>
        private bool ApplyVolumeCachePresets(List<HEU_VolumeCachePreset> volumeCachePresets)
        {
            bool bApplied = false;

            if (volumeCachePresets != null && volumeCachePresets.Count > 0)
            {
                foreach (HEU_VolumeCachePreset volumeCachePreset in volumeCachePresets)
                {
                    HEU_ObjectNode objNode = GetObjectNodeByName(volumeCachePreset._objName);
                    if (objNode == null)
                    {
                        HEU_Logger.LogWarningFormat("No object node with name: {0}. Unable to set heightfield preset.", volumeCachePreset._objName);
                        continue;
                    }

                    HEU_GeoNode geoNode = objNode.GetGeoNode(volumeCachePreset._geoName);
                    if (geoNode == null)
                    {
                        HEU_Logger.LogWarningFormat("No geo node with name: {0}. Unable to set heightfield preset.", volumeCachePreset._geoName);
                        continue;
                    }

                    HEU_VolumeCache volumeCache = geoNode.GetVolumeCacheByTileIndex(volumeCachePreset._tile);
                    if (volumeCache == null)
                    {
                        HEU_Logger.LogWarningFormat("Volume cache at tile {0} not found for geo node {1}. Unable to set heightfield preset.",
                            volumeCachePreset._tile, volumeCachePreset._geoName);
                        continue;
                    }

                    volumeCache.ApplyPreset(volumeCachePreset);

                    bApplied = true;
                }
            }

            return bApplied;
        }

        /// <summary>
        /// Get the current parameter values from Houdini and store in
        /// the internal parameter value set. This is used for doing
        /// comparision of which values had changed after an Undo.
        /// </summary>
        internal void SyncInternalParametersForUndoCompare()
        {
            HEU_SessionBase session = GetAssetSession(true);

            if (_parameters != null)
            {
                _parameters.SyncInternalParametersForUndoCompare(session);
            }

            List<HEU_Curve> curves = Curves;
            foreach (HEU_Curve curve in curves)
            {
                if (curve.Parameters != null)
                {
                    curve.Parameters.SyncInternalParametersForUndoCompare(session);
                }
            }
        }

        /// <summary>
        /// Returns true if this has kicked off a local cook because of changes 
        /// on the Houdini side (e.g. Houdini Engine Session Sync).
        /// Otherwise returns false if it does nothing.
        /// </summary>
        internal bool UpdateSessionSync()
        {
            //HEU_Logger.Log("Time: " + Time.realtimeSinceStartup);

            if (_requestBuildAction != AssetBuildAction.NONE || !HEU_PluginSettings.SessionSyncAutoCook || !SessionSyncAutoCook)
            {
                return false;
            }

            HEU_SessionBase session = GetAssetSession(false);
            if (session == null || !session.IsSessionValid() || !session.IsSessionSync())
            {
                return false;
            }

            int oldCount = _totalCookCount;
            UpdateTotalCookCount();
            bool bRequiresCook = oldCount != _totalCookCount;

            int numInputNodes = this._inputNodes.Count;


            if (bRequiresCook)
            {
                //HEU_Logger.LogFormat("Recooking asset because of cook count mismatch: current={0} != new={1}", oldCount, _totalCookCount);

                // Disable parm and input uploading for the recook process
                bool thisCheckParameterChangeForCook = false;
                bool thiSkipCookCheck = false;
                bool thisUploadParameters = false;
                bool thisUploadParameterPreset = false;
                bool thisForceUploadInputs = false;
                bool thisSessionSyncCook = true;
                ClearBuildRequest();
                return RecookAsync(thisCheckParameterChangeForCook, thiSkipCookCheck,
                    thisUploadParameters, thisUploadParameterPreset,
                    thisForceUploadInputs, thisSessionSyncCook);
            }

            return false;
        }

        // Updates the total cook count (important for session sync)
        internal void UpdateTotalCookCount()
        {
            HEU_SessionBase session = GetAssetSession(true);
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            // The reason to query recursively for all assets (SOP and OBJ) is to handle cases
            // where nodes are added dynamically within geometry node network. If we only look
            // at the SOP node's count, it won't update until the user comes out the network
            session.GetTotalCookCount(
                _assetID,
                (int)(HAPI_NodeType.HAPI_NODETYPE_OBJ | HAPI_NodeType.HAPI_NODETYPE_SOP),
                (int)(HAPI_NodeFlags.HAPI_NODEFLAGS_OBJ_GEOMETRY | HAPI_NodeFlags.HAPI_NODEFLAGS_DISPLAY),
                true, out _totalCookCount);
        }

        // On GameObject.Instantiate(), setup and copy the parameter values 
        private void ResetAndCopyInstantiatedProperties(HEU_HoudiniAsset newAsset)
        {
            InvalidateAsset();
            // Setup again to avoid null references
            SetupAsset(_assetType, _assetPath, _rootGameObject, GetAssetSession(true));


            // Destroy everything except the root object and this
            // This ensures that there are no dangling gameobjects from the instantiation
            // Note that this creates a limitation, where a duplicated object destroys all children of it
            // but I think it's fine, since we stated in the docs that we don't fully support duplication in the first
            // place ;)
            Transform[] gos = _rootGameObject.GetComponentsInChildren<Transform>();
            foreach (Transform trans in gos)
            {
                if (trans != null && trans.gameObject != null && trans.gameObject != _rootGameObject)
                {
                    DestroyImmediate(trans.gameObject);
                }
            }

            Component[] rootComponents = _rootGameObject.GetComponents<Component>();
            foreach (Component comp in rootComponents)
            {
                if (comp.GetType() != typeof(Transform))
                {
                    DestroyImmediate(comp);
                }
            }

            _rootGameObject.transform.position = Vector3.zero;

            newAsset.DuplicateAsset(_rootGameObject);
        }

        // Gets the instantiation method (undo, default, duplicated)
        private AssetInstantiationMethod GetInstantiationMethod()
        {
            if (this._serializedMetaData.SoftDeleted)
            {
                return AssetInstantiationMethod.UNDO;
            }

            if (this._objectNodes == null)
            {
                return AssetInstantiationMethod.DEFAULT;
            }

            // ScriptableObjects do not instanitate correctly. The only way I found
            // to check this is to check if _objectNodes[i].ParentAsset is our object
            foreach (HEU_ObjectNode objNode in this._objectNodes)
            {
                if (objNode == null)
                {
                    // Corrupted gameObject. Likely due to undo. Do not duplicate.
                    return AssetInstantiationMethod.UNDO;
                }

                if (objNode.ParentAsset != this)
                {
                    return AssetInstantiationMethod.DUPLICATED;
                }
            }

            return AssetInstantiationMethod.DEFAULT;
        }

        // Gets the original object, if it has been duplicated
        private HEU_HoudiniAsset GetInstantiatedObject()
        {
            if (this._objectNodes == null || this._objectNodes.Count == 0)
            {
                return null;
            }

            if (GetInstantiationMethod() != AssetInstantiationMethod.DUPLICATED)
            {
                return null;
            }

            // See: HasBeenInstantiated()
            return this._objectNodes[0].ParentAsset;
        }

        // Clear invalid curve lists. Mostly for undo safety
        private void ClearInvalidLists()
        {
            // Lists can be broken in Undo
            _objectNodes = _objectNodes.Filter((HEU_ObjectNode node) => node != null);
            _curves = _curves.Filter((HEU_Curve curve) => curve != null);
            _materialCache = _materialCache.Filter((HEU_MaterialData data) => data != null);
        }

        // Copy all values from this asset to the new asset
        private void CopyPropertiesTo(HEU_HoudiniAsset newAsset)
        {
            HEU_SessionBase session = GetAssetSession(true);

            // Set parameter preset for asset
            if (newAsset.Parameters != null && _parameters != null)
                newAsset.Parameters.SetPresetData(_parameters.GetPresetData());

            // Set parameter preset for curves
            // The curve names for the new asset might be different than that of the old one for reasons
            // We want to match it if possible, but if not, then just iterate list
            bool bGetCurveFromNames = true;
            bGetCurveFromNames &= this._curves.Count == newAsset._curves.Count;

            for (int i = 0; i < newAsset._curves.Count; i++)
            {
                bGetCurveFromNames &= this._curves.Find((HEU_Curve curve) => curve.CurveName == newAsset._curves[i].CurveName) != null;
            }

            if (bGetCurveFromNames)
            {
                int numCurves = newAsset._curves.Count;
                for (int i = 0; i < numCurves; ++i)
                {
                    HEU_Curve srcCurve = GetCurve(newAsset._curves[i].CurveName);
                    if (srcCurve != null)
                    {
                        newAsset._curves[i].Parameters.SetPresetData(srcCurve.Parameters.GetPresetData());
                        newAsset._curves[i].SetCurveNodeData(srcCurve.DuplicateCurveNodeData());
                    }
                }
            }
            else
            {
                int numCurves = Mathf.Min(this._curves.Count, newAsset._curves.Count);
                for (int i = 0; i < numCurves; i++)
                {
                    HEU_Curve srcCurve = this._curves[i];
                    if (srcCurve != null)
                    {
                        newAsset._curves[i].Parameters.SetPresetData(srcCurve.Parameters.GetPresetData());
                        newAsset._curves[i].SetCurveNodeData(srcCurve.DuplicateCurveNodeData());
                    }
                }
            }

            newAsset._curveEditorEnabled = this._curveEditorEnabled;
            newAsset._curveDrawCollision = this._curveDrawCollision;
            newAsset._curveDrawColliders = new List<Collider>(this._curveDrawColliders);
            newAsset._curveDrawLayerMask = this._curveDrawLayerMask;
            newAsset._curveDisableScaleRotation = this._curveDisableScaleRotation;

            // Upload parameter preset
            newAsset.UploadParameterPresetToHoudini(newAsset.GetAssetSession(false));

            this._instanceInputUIState.CopyTo(newAsset._instanceInputUIState);

            // Copy over asset options
            newAsset._showHDAOptions = this._showHDAOptions;
            newAsset._showGenerateSection = this._showGenerateSection;
            newAsset._showBakeSection = this._showBakeSection;
            newAsset._showEventsSection = this._showEventsSection;
            newAsset._showCurvesSection = this._showCurvesSection;
            newAsset._showInputNodesSection = this._showInputNodesSection;
            newAsset._showToolsSection = this._showToolsSection;
            newAsset._showTerrainSection = this._showTerrainSection;

            newAsset._generateUVs = this._generateUVs;
            newAsset._generateTangents = this._generateTangents;
            newAsset._generateNormals = this._generateNormals;
            newAsset._pushTransformToHoudini = this._pushTransformToHoudini;
            newAsset._transformChangeTriggersCooks = this._transformChangeTriggersCooks;
            newAsset._cookingTriggersDownCooks = this._cookingTriggersDownCooks;
            newAsset._autoCookOnParameterChange = this._autoCookOnParameterChange;
            newAsset._ignoreNonDisplayNodes = this._ignoreNonDisplayNodes;
            newAsset._generateMeshUsingPoints = this._generateMeshUsingPoints;
            newAsset._useLODGroups = this._useLODGroups;
            newAsset._alwaysOverwriteOnLoad = this._alwaysOverwriteOnLoad;

            // Copy over tools state
            newAsset._editableNodesToolsEnabled = this._editableNodesToolsEnabled;
            newAsset._toolsInfo = ScriptableObject.Instantiate(this._toolsInfo) as HEU_ToolsInfo;

            // Copy events

            newAsset._reloadDataEvent = this._reloadDataEvent;
            newAsset._cookedDataEvent = this._cookedDataEvent;
            newAsset._bakedDataEvent = this._bakedDataEvent;
            newAsset._preAssetEvent = this._preAssetEvent;

            newAsset._downstreamConnectionCookedEvent = this._downstreamConnectionCookedEvent;

            // Copy and upload attribute values
            int numAttributeStores = newAsset._attributeStores.Count;
            for (int i = 0; i < numAttributeStores; ++i)
            {
                HEU_AttributesStore newAttrStore = newAsset._attributeStores[i];

                HEU_AttributesStore srcAttrStore = this.GetAttributeStore(newAttrStore.GeoName, newAttrStore.PartID);
                if (srcAttrStore != null)
                {
                    srcAttrStore.CopyAttributeValuesTo(newAttrStore);
                }
            }

            // Copy and upload input nodes
            int numInputNodes = newAsset._inputNodes.Count;
            for (int i = 0; i < numInputNodes; ++i)
            {
                HEU_InputNode newInputNode = newAsset._inputNodes[i];

                HEU_InputNode srcInputNode = GetInputNodeByIndex(i);
                if (srcInputNode != null)
                {
                    srcInputNode.CopyInputValuesTo(session, newInputNode);

                    newInputNode.RequiresCook = srcInputNode.RequiresCook;
                    newInputNode.RequiresUpload = srcInputNode.RequiresUpload;
                }
            }

            // Copy and upload volume data
            int numVolumeCaches = newAsset._volumeCaches.Count;
            for (int i = 0; i < numVolumeCaches; ++i)
            {
                HEU_VolumeCache newVolumeCache = newAsset._volumeCaches[i];

                HEU_ObjectNode newObject = newAsset.GetObjectNodeByName(newVolumeCache.ObjectName);
                HEU_ObjectNode srcObject = GetObjectNodeByName(newVolumeCache.ObjectName);

                if (newObject != null && srcObject != null)
                {
                    HEU_GeoNode newGeoNode = newObject.GetGeoNode(newAsset._volumeCaches[i].GeoName);
                    HEU_GeoNode srcGeoNode = srcObject.GetGeoNode(newAsset._volumeCaches[i].GeoName);

                    if (newGeoNode != null && srcGeoNode != null)
                    {
                        HEU_VolumeCache srcVolumeCache = srcGeoNode.GetVolumeCacheByTileIndex(newVolumeCache.TileIndex);
                        if (srcVolumeCache != null)
                        {
                            srcVolumeCache.CopyValuesTo(newVolumeCache);
                            newVolumeCache.IsDirty = true;
                        }
                    }
                }
            }

            if (newAsset._cookStatus == AssetCookStatus.POSTLOAD)
            {
                newAsset.SetCookStatus(AssetCookStatus.NONE, AssetCookResult.SUCCESS);
            }

            newAsset.RequestCook(false, false, false, false);

            // For outputs, copy over material overrides, and custom (non-generated) components

            List<HEU_GeneratedOutput> sourceOutputs = new List<HEU_GeneratedOutput>();
            this.GetOutput(sourceOutputs);

            List<HEU_GeneratedOutput> destOutputs = new List<HEU_GeneratedOutput>();
            newAsset.GetOutput(destOutputs);

            int numSourceOutuputs = sourceOutputs.Count;
            int numDestOutputs = destOutputs.Count;
            if (numSourceOutuputs == numDestOutputs)
            {
                for (int i = 0; i < numSourceOutuputs; ++i)
                {
                    // Main gameobject -> copy components (skip existing)
                    if (sourceOutputs[i]._outputData._gameObject != null && destOutputs[i]._outputData._gameObject != null)
                    {
                        HEU_GeneralUtility.CopyComponents(sourceOutputs[i]._outputData._gameObject, destOutputs[i]._outputData._gameObject);
                    }

                    bool bSrcHasLODGroup = HEU_GeneratedOutput.HasLODGroup(sourceOutputs[i]);
                    bool bDestHasLODGroup = HEU_GeneratedOutput.HasLODGroup(destOutputs[i]);
                    if (bSrcHasLODGroup && bDestHasLODGroup)
                    {
                        // LOD Group -> copy child components (skip existing), and copy material overrides

                        int numSrcChildren = sourceOutputs[i]._childOutputs.Count;
                        int numDestChildren = destOutputs[i]._childOutputs.Count;
                        if (numSrcChildren == numDestChildren)
                        {
                            for (int j = 0; j < numSrcChildren; ++j)
                            {
                                HEU_GeneralUtility.CopyComponents(sourceOutputs[i]._childOutputs[j]._gameObject,
                                    destOutputs[i]._childOutputs[j]._gameObject);

                                HEU_GeneratedOutput.CopyMaterialOverrides(sourceOutputs[i]._childOutputs[j], destOutputs[i]._childOutputs[j]);
                            }
                        }
                    }
                    else if (!bSrcHasLODGroup && !bDestHasLODGroup)
                    {
                        // Non-LOD Group -> Copy material overrides

                        HEU_GeneratedOutput.CopyMaterialOverrides(sourceOutputs[i]._outputData, destOutputs[i]._outputData);
                    }
                }
            }
        }


        internal void SetSoftDeleted()
        {
            if (_serializedMetaData == null)
            {
                _serializedMetaData = ScriptableObject.CreateInstance<HEU_AssetSerializedMetaData>();
            }

            this._serializedMetaData.SoftDeleted = true;


            // rot/scale values are lost when soft deleted!
            // I think it might work if I move it to _serializedMetaData, but I think it'll be most costly than what it's worth 
            foreach (HEU_Curve curve in _curves)
            {
                if (_serializedMetaData.SavedCurveNodeData != null && !this.CurveDisableScaleRotation)
                {
                    _serializedMetaData.SavedCurveNodeData.Add(curve.CurveName, curve.CurveNodeData);
                }
            }
        }

        private void SyncChildTransforms()
        {
            if (_lastSyncedChildTransformMatrices == null)
            {
                _lastSyncedChildTransformMatrices = new List<Matrix4x4>();
            }

            _lastSyncedChildTransformMatrices.Clear();
            HEU_InputUtility.GetChildrenTransforms(_rootGameObject.transform, ref _lastSyncedChildTransformMatrices);
        }


        // Private/Internal helper functions ===
        internal static HEU_AssetCookStatusWrapper AssetCookStatus_InternalToWrappper(HEU_HoudiniAsset.AssetCookStatus assetCookStatus)
        {
            switch (assetCookStatus)
            {
                case HEU_HoudiniAsset.AssetCookStatus.NONE:
                    return HEU_AssetCookStatusWrapper.NONE;
                case HEU_HoudiniAsset.AssetCookStatus.COOKING:
                    return HEU_AssetCookStatusWrapper.COOKING;
                case HEU_HoudiniAsset.AssetCookStatus.POSTCOOK:
                    return HEU_AssetCookStatusWrapper.POSTCOOK;
                case HEU_HoudiniAsset.AssetCookStatus.LOADING:
                    return HEU_AssetCookStatusWrapper.LOADING;
                case HEU_HoudiniAsset.AssetCookStatus.POSTLOAD:
                    return HEU_AssetCookStatusWrapper.POSTLOAD;
                case HEU_HoudiniAsset.AssetCookStatus.PRELOAD:
                    return HEU_AssetCookStatusWrapper.PRELOAD;
                case HEU_HoudiniAsset.AssetCookStatus.SELECT_SUBASSET:
                    return HEU_AssetCookStatusWrapper.SELECT_SUBASSET;
                default:
                    return HEU_AssetCookStatusWrapper.NONE;
            }
        }

        internal static HEU_HoudiniAsset.AssetCookStatus AssetCookStatus_WrapperToInternal(HEU_AssetCookStatusWrapper assetCookStatus)
        {
            switch (assetCookStatus)
            {
                case HEU_AssetCookStatusWrapper.NONE:
                    return HEU_HoudiniAsset.AssetCookStatus.NONE;
                case HEU_AssetCookStatusWrapper.COOKING:
                    return HEU_HoudiniAsset.AssetCookStatus.COOKING;
                case HEU_AssetCookStatusWrapper.POSTCOOK:
                    return HEU_HoudiniAsset.AssetCookStatus.POSTCOOK;
                case HEU_AssetCookStatusWrapper.LOADING:
                    return HEU_HoudiniAsset.AssetCookStatus.LOADING;
                case HEU_AssetCookStatusWrapper.POSTLOAD:
                    return HEU_HoudiniAsset.AssetCookStatus.POSTLOAD;
                case HEU_AssetCookStatusWrapper.PRELOAD:
                    return HEU_HoudiniAsset.AssetCookStatus.PRELOAD;
                case HEU_AssetCookStatusWrapper.SELECT_SUBASSET:
                    return HEU_HoudiniAsset.AssetCookStatus.SELECT_SUBASSET;
                default:
                    return HEU_HoudiniAsset.AssetCookStatus.NONE;
            }
        }

        internal static HEU_AssetCookResultWrapper AssetCookResult_InternalToWrapper(HEU_HoudiniAsset.AssetCookResult assetCookResult)
        {
            switch (assetCookResult)
            {
                case HEU_HoudiniAsset.AssetCookResult.NONE:
                    return HEU_AssetCookResultWrapper.NONE;
                case HEU_HoudiniAsset.AssetCookResult.ERRORED:
                    return HEU_AssetCookResultWrapper.ERRORED;
                case HEU_HoudiniAsset.AssetCookResult.SUCCESS:
                    return HEU_AssetCookResultWrapper.SUCCESS;
                default:
                    return HEU_AssetCookResultWrapper.NONE;
            }
        }

        internal static HEU_HoudiniAsset.AssetCookResult AssetCookResult_WrapperToInternal(HEU_AssetCookResultWrapper assetCookResult)
        {
            switch (assetCookResult)
            {
                case HEU_AssetCookResultWrapper.NONE:
                    return HEU_HoudiniAsset.AssetCookResult.NONE;
                case HEU_AssetCookResultWrapper.ERRORED:
                    return HEU_HoudiniAsset.AssetCookResult.ERRORED;
                case HEU_AssetCookResultWrapper.SUCCESS:
                    return HEU_HoudiniAsset.AssetCookResult.SUCCESS;
                default:
                    return HEU_HoudiniAsset.AssetCookResult.NONE;
            }
        }

        internal static HEU_CurveDrawCollisionWrapper CurveDrawCollision_InternalToWrapper(HEU_Curve.CurveDrawCollision curveDrawCollision)
        {
            switch (curveDrawCollision)
            {
                case HEU_Curve.CurveDrawCollision.COLLIDERS:
                    return HEU_CurveDrawCollisionWrapper.COLLIDERS;
                case HEU_Curve.CurveDrawCollision.LAYERMASK:
                    return HEU_CurveDrawCollisionWrapper.LAYERMASK;
                default:
                    return HEU_CurveDrawCollisionWrapper.INVALID;
            }
        }

        internal static HEU_Curve.CurveDrawCollision CurveDrawCollision_WrapperToInternal(HEU_CurveDrawCollisionWrapper curveDrawCollision)
        {
            switch (curveDrawCollision)
            {
                case HEU_CurveDrawCollisionWrapper.COLLIDERS:
                    return HEU_Curve.CurveDrawCollision.COLLIDERS;
                case HEU_CurveDrawCollisionWrapper.LAYERMASK:
                    return HEU_Curve.CurveDrawCollision.LAYERMASK;
                default:
                    return HEU_Curve.CurveDrawCollision.COLLIDERS;
            }
        }

        internal static HEU_AssetTypeWrapper AssetType_InternalToWrapper(HEU_HoudiniAsset.HEU_AssetType assetType)
        {
            switch (assetType)
            {
                case HEU_HoudiniAsset.HEU_AssetType.TYPE_INVALID:
                    return HEU_AssetTypeWrapper.TYPE_INVALID;
                case HEU_HoudiniAsset.HEU_AssetType.TYPE_HDA:
                    return HEU_AssetTypeWrapper.TYPE_HDA;
                case HEU_HoudiniAsset.HEU_AssetType.TYPE_CURVE:
                    return HEU_AssetTypeWrapper.TYPE_CURVE;
                case HEU_HoudiniAsset.HEU_AssetType.TYPE_INPUT:
                    return HEU_AssetTypeWrapper.TYPE_INPUT;
                default:
                    return HEU_AssetTypeWrapper.TYPE_INVALID;
            }
        }

        internal static HEU_HoudiniAsset.HEU_AssetType AssetType_WrapperToInternal(HEU_AssetTypeWrapper assetType)
        {
            switch (assetType)
            {
                case HEU_AssetTypeWrapper.TYPE_INVALID:
                    return HEU_HoudiniAsset.HEU_AssetType.TYPE_INVALID;
                case HEU_AssetTypeWrapper.TYPE_HDA:
                    return HEU_HoudiniAsset.HEU_AssetType.TYPE_HDA;
                case HEU_AssetTypeWrapper.TYPE_CURVE:
                    return HEU_HoudiniAsset.HEU_AssetType.TYPE_CURVE;
                case HEU_AssetTypeWrapper.TYPE_INPUT:
                    return HEU_HoudiniAsset.HEU_AssetType.TYPE_INPUT;
                default:
                    return HEU_HoudiniAsset.HEU_AssetType.TYPE_INVALID;
            }
        }


        // Equivalence function (Mostly for testing purposes) =======================================================================

        public bool IsEquivalentTo(HEU_HoudiniAsset asset)
        {
            bool bResult = true;

            string header = "HEU_HoudiniAsset";

            if (asset == null)
            {
                HEU_Logger.LogError(header + " Not equivalent");
                return false;
            }


            HEU_TestHelpers.AssertTrueLogEquivalent(this._assetInfo.ToTestObject(), asset._assetInfo.ToTestObject(), ref bResult, header,
                "Asset Info");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._assetType, asset._assetType, ref bResult, header, "Asset type");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._nodeInfo.ToTestObject(), asset._nodeInfo.ToTestObject(), ref bResult, header, "Node Info");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._assetName == asset._assetName, ref bResult, header, "Asset name");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._assetOpName, asset._assetOpName, ref bResult, header, "Asset op");

            //HEU_TestHelpers.AssertTrueLogEquivalent(this._assetHelp, asset._assetHelp, ref bResult, header, "_assetHelp");


            // TransformInputCount/GeoInputCount not necessary because it is a part of _assetInfo
            // assetId not necessary because doesn't  need to be equivalent

            HEU_TestHelpers.AssertTrueLogEquivalent(this._assetPath, asset.AssetPath, ref bResult, header, "Asset path");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._loadAssetFromMemory, asset._loadAssetFromMemory, ref bResult, header, "Load from memory");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._alwaysOverwriteOnLoad, asset._alwaysOverwriteOnLoad, ref bResult, header,
                "Always overwrite on load");

            // Ignore assetFileObject

            // Skip HandleCount

            HEU_TestHelpers.AssertTrueLogEquivalent(this._objectNodes, asset._objectNodes, ref bResult, "Object node", header);


            // Skip ownerGameObject, rootGameObject

            HEU_TestHelpers.AssertTrueLogEquivalent(this._materialCache, asset._materialCache, ref bResult, header, "Material cache");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._parameters, asset._parameters, ref bResult, header, "Parameters");


            // Skip lastSyncedTransformMatrix

            // Skip asset folder cache
            //if (HEU_GeneralUtility.ShouldBeTested(this._assetCacheFolderPath, asset._assetCacheFolderPath, ref bResult, header, "_assetCacheFolderPath"))
            //HEU_TestHelpers.AssertTrueLogEquivalent(this._assetCacheFolderPath == asset._assetCacheFolderPath, ref bResult, header, "Asset cache folder");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._subassetNames, asset._subassetNames, ref bResult, header, "_subassetNames");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._selectedSubassetIndex, asset._selectedSubassetIndex, ref bResult, header,
                "_selectedSubassetIndex");

            //if (this._savedAssetPreset != null || asset._savedAssetPreset != null)
            //   HEU_TestHelpers.AssertTrueLogEquivalent(this._savedAssetPreset.IsEquivalentTo(asset._savedAssetPreset), ref bResult, header, "Saved asset preset");

            //if (this._recookPreset != null || asset._recookPreset != null)
            //    HEU_TestHelpers.AssertTrueLogEquivalent(this._recookPreset.IsEquivalentTo(asset._recookPreset), ref bResult, header, "Recook preset");

            // HEU_TestHelpers.AssertTrueLogEquivalent(this._totalCookCount, asset._totalCookCount, ref bResult, header, "Recook preset");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._requestBuildAction, asset._requestBuildAction, ref bResult, header, "Request build action");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._checkParameterChangeForCook, asset._checkParameterChangeForCook, ref bResult, header, "Check parameter change for cook");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._skipCookCheck, asset._skipCookCheck, ref bResult, header, "Skip cook check");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._uploadParameters, asset._uploadParameters, ref bResult, header, "Upload parameters");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._forceUploadInputs, asset._forceUploadInputs, ref bResult, header, "Force upload inputs");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._upstreamCookChanged, asset._upstreamCookChanged, ref bResult, header, "Upstream cook changed");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._cookStatus, asset._cookStatus, ref bResult, header, "Cook status");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._lastCookResult, asset._lastCookResult, ref bResult, header, "Last cook result");
            // HEU_TestHelpers.AssertTrueLogEquivalent(this._isCookingAssetReloaded, asset._isCookingAssetReloaded, ref bResult, header, "Is cooking asset reloaded");

            // Skip sessionId

            HEU_TestHelpers.AssertTrueLogEquivalent(this._uiLocked, asset._uiLocked, ref bResult, header, "UI locked");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showHDAOptions, asset._showHDAOptions, ref bResult, header, "Show HDA options");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showGenerateSection, asset._showGenerateSection, ref bResult, header,
                "Show generate section");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showBakeSection, asset._showBakeSection, ref bResult, header, "Show bake section");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showEventsSection, asset._showEventsSection, ref bResult, header, "Show events section");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showCurvesSection, asset._showCurvesSection, ref bResult, header, "Show curves section");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showInputNodesSection, asset._showInputNodesSection, ref bResult, header,
                "Show input nodes section");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showToolsSection, asset._showToolsSection, ref bResult, header, "Show tools section");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._showTerrainSection, asset._showTerrainSection, ref bResult, header, "Show Terrain section");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._instanceInputUIState, asset._instanceInputUIState, ref bResult, header,
                "Instance input UI state");

            // Skip events

            HEU_TestHelpers.AssertTrueLogEquivalent(this._generateUVs, asset._generateUVs, ref bResult, header, "Generate UVs");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._generateTangents, asset._generateTangents, ref bResult, header, "Generate Tangents");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._generateNormals, asset._generateNormals, ref bResult, header, "Generate Normals");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._pushTransformToHoudini, asset._pushTransformToHoudini, ref bResult, header,
                "Push Transform to Houdini");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._transformChangeTriggersCooks, asset._transformChangeTriggersCooks, ref bResult, header,
                "Transform changes triggers cooks");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._cookingTriggersDownCooks, asset._cookingTriggersDownCooks, ref bResult, header,
                "Cooking triggers down cooks");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._autoCookOnParameterChange, asset._autoCookOnParameterChange, ref bResult, header,
                "Auto cook on parameter change");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._ignoreNonDisplayNodes, asset._ignoreNonDisplayNodes, ref bResult, header,
                "Ignore non-display nodes");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._generateMeshUsingPoints, asset._generateMeshUsingPoints, ref bResult, header,
                "Generate mesh using points");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._useLODGroups, asset._useLODGroups, ref bResult, header, "Use LOD groups");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._splitGeosByGroup, asset._splitGeosByGroup, ref bResult, header, "Split geos by group");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._sessionSyncAutoCook, asset._sessionSyncAutoCook, ref bResult, header,
                "Session sync auto cook");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._bakeUpdateKeepPreviousTransformValues, asset._bakeUpdateKeepPreviousTransformValues,
                ref bResult, header, "Bake update keep previous transform values");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveEditorEnabled, asset._curveEditorEnabled, ref bResult, header, "Curve editor enabled");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._curves, asset._curves, ref bResult, header, "Curves");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveDrawCollision, asset._curveDrawCollision, ref bResult, header, "Curve draw collision");


            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveDrawLayerMask.ToTestObject(), asset._curveDrawLayerMask.ToTestObject(), ref bResult,
                header, "Curve draw layer mask");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveProjectMaxDistance, asset._curveProjectMaxDistance, ref bResult, header,
                "Curve Project max distance");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveProjectDirection, asset._curveProjectDirection, ref bResult, header,
                "Curve project direction");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveDisableScaleRotation, asset.CurveDisableScaleRotation, ref bResult, header,
                "Curve disable scale rotation");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveFrameSelectedNodes, asset._curveFrameSelectedNodes, ref bResult, header,
                "Curve Frame selected nodes");
            HEU_TestHelpers.AssertTrueLogEquivalent(this._curveFrameSelectedNodeDistance, asset._curveFrameSelectedNodeDistance, ref bResult, header,
                "Curve Frame selected nodes distance");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._inputNodes, asset._inputNodes, ref bResult, header, "Input node:");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._handles, asset._handles, ref bResult, header, "Handles node:");


            HEU_TestHelpers.AssertTrueLogEquivalent(this._handlesEnabled, asset._handlesEnabled, ref bResult, header, "Handles enabled");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._volumeCaches, asset._volumeCaches, ref bResult, header, "Volume caches node:");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._attributeStores, asset._attributeStores, ref bResult, header, "Attribute stores:");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._editableNodesToolsEnabled, asset._editableNodesToolsEnabled, ref bResult, header,
                "_editableNodesToolsEnabled");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._toolsInfo, asset._toolsInfo, ref bResult, header, "_toolsInfo");

            HEU_TestHelpers.AssertTrueLogEquivalent(this._serializedMetaData, asset._serializedMetaData, ref bResult, header, "_serializedMetaData");

            return bResult;
        }
    }
} // HoudiniEngineUnity
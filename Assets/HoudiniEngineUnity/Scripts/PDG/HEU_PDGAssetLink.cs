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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_StringHandle = System.Int32;
    using HAPI_NodeId = System.Int32;
    using HAPI_PDG_WorkItemId = System.Int32;
    using HAPI_PDG_GraphContextId = System.Int32;
    using HAPI_AssetLibraryId = System.Int32;


    /// <summary>
    /// Callback when asset is cooked.
    /// <param name="CookedEventData">The reload data.</param>
    /// </summary>
    [System.Serializable]
    public class HEU_PDGCookedDataEvent : UnityEvent<HEU_PDGCookedEventData>
    {
    }

    public class HEU_PDGCookedEventData
    {
        public bool CookSuccess;
        public HEU_TOPNodeData TopNodeData;

        public HEU_PDGCookedEventData(bool bSuccess, HEU_TOPNodeData bTopNodeData)
        {
            CookSuccess = bSuccess;
            TopNodeData = bTopNodeData;
        }
    }

    /// <summary>
    /// Connects to an instanced HDA containing TOP networks and TOP nodes, manages PDG graph cook, and keeps in sync.
    /// Handles automatic loading of generated results (geometry). Show / hide, unload results.
    /// This behaves differently from a regular HDA as it doesn't expose any parms, 
    /// nor the regular Houdini Engine cooking or baking functionality. Rather its strictly meant for PDG workflow.
    /// It also uses a different loading (geometry generation) mechanism that focuses on asynchronous, fast loading,
    /// lightweight footprint, and reduction of "editor" state. This means it doesn't support normal editor workflow such
    /// as undo, parms, custom tools (e.g curve editor), baking, etc.
    /// Currently the loaded geometry are not saved to persistent files in the Editor, but only saved to the current scene.
    /// This limititation will be fixed in the near future.
    /// </summary>
    [ExecuteInEditMode]
    public class HEU_PDGAssetLink : MonoBehaviour, ISerializationCallbackReceiver
    {
        // PUBLIC FIELDS =========================================================================================

        public bool AutoCook
        {
            get => _autoCook;
            set => _autoCook = value;
        }

        public bool UseHEngineData
        {
            get => _useHEngineData;
            set => _useHEngineData = value;
        }

        // Filter strings
        public bool UseTOPNodeFilter
        {
            get => _bUseTOPNodeFilter;
            set => _bUseTOPNodeFilter = value;
        }

        public bool UseTOPOutputFilter
        {
            get => _bUseTOPOutputFilter;
            set => _bUseTOPOutputFilter = value;
        }

        public string TopNodeFilter
        {
            get => _topNodeFilter;
            set => _topNodeFilter = value;
        }

        public string TopOutputFilter
        {
            get => _topOutputFilter;
            set => _topOutputFilter = value;
        }


        public HEU_HoudiniAsset ParentAsset => _heu;

        public string AssetPath => _assetPath;

        public GameObject AssetGO => _assetGO;

        public string AssetName => _assetName;

        public HAPI_NodeId AssetID => _assetID;

        public List<HEU_TOPNetworkData> TopNetworks => _topNetworks;

        public string[] TopNetworkNames => _topNetworkNames;

        public int SelectedTOPNetwork => _selectedTOPNetwork;

        public HEU_LinkStateWrapper PDGLinkState => LinkState_InternalToWrapper(_linkState);

        // The root gameobject to place all loaded geometry under
        public GameObject LoadRootGameObject => _loadRootGameObject;

        // The root directory for generated output
        public string OutputCachePathRoot => _outputCachePathRoot;

        [SerializeField] private HEU_PDGCookedDataEvent _cookedDataEvent = new HEU_PDGCookedDataEvent();

        public HEU_PDGCookedDataEvent CookedDataEvent => _cookedDataEvent;

        // ==========================================================================================================

        //	DATA ------------------------------------------------------------------------------------------------------

#pragma warning disable 0414
        [SerializeField] private string _assetPath;

        [SerializeField] private GameObject _assetGO;
#pragma warning restore 0414

        [SerializeField] private string _assetName;

        [SerializeField] private HAPI_NodeId _assetID = HEU_Defines.HEU_INVALID_NODE_ID;

        // Linked HDA
        [SerializeField] private HEU_HoudiniAsset _heu;

        // List of TOP networks within HDA
        [SerializeField] private List<HEU_TOPNetworkData> _topNetworks = new List<HEU_TOPNetworkData>();

        // Names of TOP networks within HDA
        [SerializeField] private string[] _topNetworkNames = new string[0];

        // Currently selected TOP network
        [SerializeField] private int _selectedTOPNetwork;

        [SerializeField] private LinkState _linkState = LinkState.INACTIVE;

        internal LinkState AssetLinkStateInternal => _linkState;

        internal enum LinkState
        {
            INACTIVE,
            LINKING,
            LINKED,
            ERROR_NOT_LINKED
        }

        [SerializeField] private bool _autoCook;

        [SerializeField] private bool _useHEngineData = false;

        // Delegate for Editor window to hook into for callback when needing updating
        public delegate void UpdateUIDelegate();

        public UpdateUIDelegate _repaintUIDelegate;

        internal HEU_WorkItemTally _workItemTally = new HEU_WorkItemTally();

        // The root gameobject to place all loaded geometry under
        [SerializeField] private GameObject _loadRootGameObject;

        // The root directory for generated output
        [SerializeField] private string _outputCachePathRoot;

        // Filter strings
        [SerializeField] private bool _bUseTOPNodeFilter;
        [SerializeField] private bool _bUseTOPOutputFilter;
        [SerializeField] private string _topNodeFilter;
        [SerializeField] private string _topOutputFilter;

        private int _numLoadingResults = 0;
        private int _numTotalResults = 0;

        // PUBLIC FUNCTIONS =========================================================================================


        public void Setup(HEU_HoudiniAsset hdaAsset)
        {
            _heu = hdaAsset;
            _assetGO = _heu.RootGameObject;
            _assetPath = _heu.AssetPath;
            _assetName = _heu.AssetName;
            _bUseTOPNodeFilter = true;
            _bUseTOPOutputFilter = true;
            _topNodeFilter = HEU_Defines.DEFAULT_TOP_NODE_FILTER;
            _topOutputFilter = HEU_Defines.DEFAULT_TOP_OUTPUT_FILTER;

            // Use the HDAs cache folder for generating output files
            string hdaCachePath = _heu.GetValidAssetCacheFolderPath();
            _outputCachePathRoot = HEU_Platform.BuildPath(hdaCachePath, "PDGCache");

            Reset();
            Refresh();
        }

        /// <summary>
        /// Reset all TOP network and node state.
        /// Should be done after the linked HDA has rebuilt.
        /// </summary>
        public void Reset()
        {
            ClearAllTOPData();
        }

        /// <summary>
        /// Refresh this object's internal state by querying and populating TOP network and nodes
        /// from linked HDA.
        /// </summary>
        public void Refresh()
        {
            if (_heu == null)
            {
                _linkState = LinkState.ERROR_NOT_LINKED;
                _assetID = HEU_Defines.HEU_INVALID_NODE_ID;
                return;
            }

            if (!_heu.IsAssetValid())
            {
                _linkState = LinkState.INACTIVE;
                _assetID = HEU_Defines.HEU_INVALID_NODE_ID;
            }

            if (_linkState == LinkState.INACTIVE || _assetID == HEU_Defines.HEU_INVALID_NODE_ID)
            {
                // Never linked before, so do some setup, and cook the HDA

                _linkState = LinkState.LINKING;

                // Removing then adding listener guarantees no duplicate entries
                _heu.CookedDataEvent.RemoveListener(NotifyAssetCooked);
                _heu.CookedDataEvent.AddListener(NotifyAssetCooked);

                _heu.ReloadDataEvent.RemoveListener(NotifyAssetCooked);
                _heu.ReloadDataEvent.AddListener(NotifyAssetCooked);

                // Do a asynchronouse cook of the linked HDA so that we get its latest state
                _heu.RequestCook(true, true, true, true);

                RepaintUI();
            }
            else
            {
                // Linked already, so now populate this
                PopulateFromHDA();
            }
        }

        public List<KeyValuePair<int, HEU_TOPNodeData>> GetNonHiddenTOPNodes(HEU_TOPNetworkData topNetwork)
        {
            List<KeyValuePair<int, HEU_TOPNodeData>> nonHiddenNodes = new List<KeyValuePair<int, HEU_TOPNodeData>>();

            for (int i = 0; i < topNetwork._topNodes.Count; ++i)
            {
                if (topNetwork._topNodes[i]._tags._show)
                {
                    nonHiddenNodes.Add(new KeyValuePair<int, HEU_TOPNodeData>(i, topNetwork._topNodes[i]));
                }
            }

            return nonHiddenNodes;
        }

        /// <summary>
        /// Set the TOP network at the given index as currently selected TOP network
        /// </summary>
        /// <param name="newIndex">Index of the TOP network</param>
        public void SelectTOPNetwork(int newIndex)
        {
            if (newIndex < 0 || newIndex >= _topNetworks.Count)
            {
                return;
            }

            _selectedTOPNetwork = newIndex;
        }

        /// <summary>
        /// Set the TOP node at the given index in the given TOP network as currently selected TOP node
        /// </summary>
        /// <param name="network">Container TOP network</param>
        /// <param name="newIndex">Index of the TOP node to be selected</param>
        public void SelectTOPNode(HEU_TOPNetworkData network, int newIndex)
        {
            if (newIndex < 0 || newIndex >= network._topNodes.Count)
            {
                return;
            }

            network._selectedTOPIndex = newIndex;
        }

        public HEU_TOPNetworkData GetSelectedTOPNetwork()
        {
            return GetTOPNetwork(_selectedTOPNetwork);
        }

        public HEU_TOPNodeData GetSelectedTOPNode()
        {
            HEU_TOPNetworkData topNetwork = GetTOPNetwork(_selectedTOPNetwork);
            if (topNetwork != null)
            {
                if (topNetwork._selectedTOPIndex >= 0 && topNetwork._selectedTOPIndex < topNetwork._topNodes.Count)
                {
                    return topNetwork._topNodes[topNetwork._selectedTOPIndex];
                }
            }

            return null;
        }

        public HEU_TOPNetworkData GetTOPNetwork(int index)
        {
            if (index >= 0 && index < _topNetworks.Count)
            {
                return _topNetworks[index];
            }

            return null;
        }

        /// <summary>
        /// Dirty the specified TOP node and clear its work item results.
        /// </summary>
        /// <param name="topNode"></param>
        public void DirtyTOPNode(HEU_TOPNodeData topNode)
        {
            HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
            if (pdgSession != null && pdgSession.DirtyTOPNode(topNode._nodeID))
            {
                ClearTOPNodeWorkItemResults(topNode);
            }
        }

        /// <summary>
        /// Cook the specified TOP node.
        /// </summary>
        /// <param name="topNode"></param>
        public void CookTOPNode(HEU_TOPNodeData topNode)
        {
            HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
            if (pdgSession != null)
            {
                pdgSession.CookTOPNode(topNode._nodeID);
            }
        }

        /// <summary>
        /// Dirty the currently selected TOP network and clear all work item results.
        /// </summary>
        public void DirtyAll()
        {
            HEU_TOPNetworkData topNetwork = GetSelectedTOPNetwork();
            if (topNetwork != null)
            {
                HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
                if (pdgSession != null && pdgSession.DirtyAll(topNetwork._nodeID))
                {
                    ClearTOPNetworkWorkItemResults(topNetwork);
                }
            }
        }

        /// <summary>
        /// Cook the output TOP node of the currently selected TOP network.
        /// </summary>
        public void CookOutput()
        {
            HEU_SessionBase session = GetHAPISession();
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            HEU_TOPNetworkData topNetwork = GetSelectedTOPNetwork();
            if (topNetwork != null)
            {
                //HEU_Logger.Log("Cooking output!");

                _workItemTally.ZeroAll();
                ResetTOPNetworkWorkItemTally(topNetwork);

                HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
                if (pdgSession != null)
                {
                    pdgSession.CookTOPNetworkOutputNode(topNetwork, OnSyncComplete);
                }
            }
        }

        /// <summary>
        /// Pause the PDG cook of the currently selected TOP network
        /// </summary>
        public void PauseCook()
        {
            HEU_SessionBase session = GetHAPISession();
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            HEU_TOPNetworkData topNetwork = GetSelectedTOPNetwork();
            if (topNetwork != null)
            {
                //HEU_Logger.Log("Cooking output!");

                _workItemTally.ZeroAll();
                ResetTOPNetworkWorkItemTally(topNetwork);

                HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
                if (pdgSession != null)
                {
                    pdgSession.PauseCook(topNetwork);
                }
            }
        }

        /// <summary>
        /// Cancel the PDG cook of the currently selected TOP network
        /// </summary>
        public void CancelCook()
        {
            HEU_SessionBase session = GetHAPISession();
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            HEU_TOPNetworkData topNetwork = GetSelectedTOPNetwork();
            if (topNetwork != null)
            {
                //HEU_Logger.Log("Cooking output!");

                _workItemTally.ZeroAll();
                ResetTOPNetworkWorkItemTally(topNetwork);

                HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
                if (pdgSession != null)
                {
                    pdgSession.CancelCook(topNetwork);
                }
            }
        }

        public HEU_SessionBase GetHAPISession()
        {
            return _heu != null ? _heu.GetAssetSession(true) : null;
        }

        public HEU_TOPNodeData GetTOPNode(HAPI_NodeId nodeID)
        {
            int numNetworks = _topNetworks.Count;
            for (int i = 0; i < numNetworks; ++i)
            {
                int numNodes = _topNetworks[i]._topNodes.Count;
                for (int j = 0; j < numNodes; ++j)
                {
                    if (_topNetworks[i]._topNodes[j]._nodeID == nodeID)
                    {
                        return _topNetworks[i]._topNodes[j];
                    }
                }
            }

            return null;
        }

        public string GetTOPNodeStatus(HEU_TOPNodeData topNode)
        {
            if (topNode._pdgState == HEU_TOPNodeData.PDGState.COOK_FAILED || topNode.AnyWorkItemsFailed())
            {
                return "Cook Failed";
            }
            else if (topNode._pdgState == HEU_TOPNodeData.PDGState.COOK_COMPLETE)
            {
                return "Cook Completed";
            }
            else if (topNode._pdgState == HEU_TOPNodeData.PDGState.COOKING)
            {
                return "Cook In Progress";
            }
            else if (topNode._pdgState == HEU_TOPNodeData.PDGState.DIRTIED)
            {
                return "Dirtied";
            }
            else if (topNode._pdgState == HEU_TOPNodeData.PDGState.DIRTYING)
            {
                return "Dirtying";
            }

            return "";
        }

        public static HEU_TOPNetworkData GetTOPNetworkByName(string name, List<HEU_TOPNetworkData> topNetworks)
        {
            for (int i = 0; i < topNetworks.Count; ++i)
            {
                if (topNetworks[i]._nodeName.Equals(name))
                {
                    return topNetworks[i];
                }
            }

            return null;
        }

        public static HEU_TOPNodeData GetTOPNodeByName(string name, List<HEU_TOPNodeData> topNodes)
        {
            for (int i = 0; i < topNodes.Count; ++i)
            {
                if (topNodes[i]._nodeName.Equals(name))
                {
                    return topNodes[i];
                }
            }

            return null;
        }

        // =======================================================================================================


        private void Awake()
        {
            //HEU_Logger.Log("Awake");

            HandleInitialLoad();
        }

        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// Callback on scene load, or code refresh.
        /// </summary>
        public void OnAfterDeserialize()
        {
            //HEU_Logger.Log("OnAfterDeserialize");

            HandleInitialLoad();
        }

        /// <summary>
        /// Register self with the global HEU_PDGAssetLink list.
        /// </summary>
        private void HandleInitialLoad()
        {
#if HOUDINIENGINEUNITY_ENABLED
            HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
            if (pdgSession != null)
            {
                pdgSession.AddAsset(this);
            }

            if (_linkState != LinkState.INACTIVE)
            {
                // On load this, need to relink
                _assetID = HEU_Defines.HEU_INVALID_NODE_ID;
                _linkState = LinkState.INACTIVE;

                // UI will take care of refreshing
                //Refresh();
            }
#endif
        }

        private void OnDestroy()
        {
            HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
            if (pdgSession != null)
            {
                // Unregister on clean up
                pdgSession.RemoveAsset(this);
            }
        }

        /// <summary>
        /// Callback when linked HDA has been cooked. Allows to trigger a PDG graph cook.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="bSuccess"></param>
        /// <param name="generatedOutputs"></param>
        private void NotifyAssetCooked(HEU_HoudiniAsset asset, bool bSuccess, List<GameObject> generatedOutputs)
        {
            //HEU_Logger.LogFormat("NotifyAssetCooked: {0} - {1} - {2}", asset.AssetName, bSuccess, _linkState);
            if (bSuccess)
            {
                if (_linkState == LinkState.LINKED)
                {
                    if (_autoCook)
                    {
                        CookOutput();
                    }
                }
                else
                {
                    PopulateFromHDA();
                }
            }
            else
            {
                _linkState = LinkState.ERROR_NOT_LINKED;
            }
        }

        private void NotifyAssetCooked(HEU_CookedEventData cookedEventData)
        {
            if (cookedEventData == null)
            {
                return;
            }

            NotifyAssetCooked(cookedEventData.Asset, cookedEventData.CookSuccess, cookedEventData.OutputObjects);
        }

        private void NotifyAssetCooked(HEU_ReloadEventData reloadEventData)
        {
            if (reloadEventData == null)
            {
                return;
            }

            NotifyAssetCooked(reloadEventData.Asset, reloadEventData.CookSuccess, reloadEventData.OutputObjects);
        }


        /// <summary>
        /// Populate TOP data from linked HDA
        /// </summary>
        private void PopulateFromHDA()
        {
            if (!_heu.IsAssetValid())
            {
                _linkState = LinkState.ERROR_NOT_LINKED;
                return;
            }

            if (_heu != null)
            {
                _assetID = _heu.AssetID;
                _assetName = _heu.AssetName;
            }

            if (PopulateTOPNetworks())
            {
                _linkState = LinkState.LINKED;
            }
            else
            {
                _linkState = LinkState.ERROR_NOT_LINKED;
                HEU_Logger.LogErrorFormat("Failed to populate TOP network info for asset {0}!", _assetName);
            }

            RepaintUI();
        }

        /// <summary>
        /// Find all TOP networks from linked HDA, as well as the TOP nodes within, and populate internal state.
        /// </summary>
        /// <returns>True if successfully populated data</returns>
        public bool PopulateTOPNetworks()
        {
            HEU_SessionBase session = GetHAPISession();

            HAPI_NodeId[] allNetworkNodeIds = HEU_PDGSession.GetNonBypassedNetworkIds(session, _assetID);
            if (allNetworkNodeIds == null || allNetworkNodeIds.Length == 0)
            {
                return false;
            }

            // Holds TOP networks in use
            List<HEU_TOPNetworkData> newNetworks = new List<HEU_TOPNetworkData>();

            // Find nodes with TOP child nodes
            foreach (HAPI_NodeId currentNodeId in allNetworkNodeIds)
            {
                HAPI_NodeInfo topNodeInfo = new HAPI_NodeInfo();
                if (!session.GetNodeInfo(currentNodeId, ref topNodeInfo))
                {
                    return false;
                }

                string nodeName = HEU_SessionManager.GetString(topNodeInfo.nameSH, session);
                //HEU_Logger.LogFormat("Top node: {0} - {1}", nodeName, topNodeInfo.type);

                // Skip any non TOP or SOP networks
                if (topNodeInfo.type != HAPI_NodeType.HAPI_NODETYPE_TOP &&
                    topNodeInfo.type != HAPI_NodeType.HAPI_NODETYPE_SOP)
                {
                    continue;
                }

                // Get list of all TOP nodes within this network.
                HAPI_NodeId[] topNodeIDs = null;
                if (!HEU_SessionManager.GetComposedChildNodeList(session, currentNodeId,
                        (int)(HAPI_NodeType.HAPI_NODETYPE_TOP), (int)HAPI_NodeFlags.HAPI_NODEFLAGS_TOP_NONSCHEDULER,
                        true, out topNodeIDs))
                {
                    continue;
                }

                // Skip networks without TOP nodes
                if (topNodeIDs == null || topNodeIDs.Length == 0)
                {
                    continue;
                }

                // Get any filter tags from spare parms on TOP nodes
                TOPNodeTags tags = new TOPNodeTags();
                if (_useHEngineData)
                {
                    ParseHEngineData(session, currentNodeId, ref topNodeInfo, ref tags);

                    if (!tags._showHEngineData)
                    {
                        continue;
                    }
                }
                else
                {
                    tags._show = true;
                    tags._showHEngineData = true;
                }

                HEU_TOPNetworkData topNetworkData = GetTOPNetworkByName(nodeName, _topNetworks);
                if (topNetworkData == null)
                {
                    topNetworkData = new HEU_TOPNetworkData();
                }
                else
                {
                    // Found previous TOP network, so remove it from old list. This makes
                    // sure to not remove it when cleaning up old nodes.
                    _topNetworks.Remove(topNetworkData);
                }

                newNetworks.Add(topNetworkData);

                topNetworkData._nodeID = currentNodeId;
                topNetworkData._nodeName = nodeName;
                topNetworkData._parentName = _assetName;
                topNetworkData._tags = tags;

                if (PopulateTOPNodes(session, topNetworkData, topNodeIDs, _useHEngineData))
                {
                    for (int i = 0; i < topNetworkData._topNodes.Count; i++)
                    {
                        if (topNetworkData._topNodes[i]._tags._show)
                        {
                            topNetworkData._selectedTOPIndex = i;
                            break;
                        }
                    }
                }
            }

            // Clear old TOP networks and nodes
            ClearAllTOPData();
            _topNetworks = newNetworks;

            // Update latest TOP network names
            _topNetworkNames = new string[_topNetworks.Count];
            for (int i = 0; i < _topNetworks.Count; ++i)
            {
                _topNetworkNames[i] = _topNetworks[i]._nodeName;
            }

            return true;
        }

        /// <summary>
        /// Given TOP nodes from a TOP network, populate internal state from each TOP node.
        /// </summary>
        /// <param name="session">Houdini Engine session</param>
        /// <param name="topNetwork">TOP network to query TOP nodes from</param>
        /// <param name="topNodeIDs">List of TOP nodes in the TOP network</param>
        /// <param name="useHEngineData">Whether or not to use HEngine data for filtering</param>
        /// <returns>True if successfully populated data</returns>
        private bool PopulateTOPNodes(HEU_SessionBase session, HEU_TOPNetworkData topNetwork, HAPI_NodeId[] topNodeIDs,
            bool useHEngineData)
        {
            // Holds list of found TOP nodes
            List<HEU_TOPNodeData> newNodes = new List<HEU_TOPNodeData>();

            foreach (HAPI_NodeId topNodeID in topNodeIDs)
            {
                // Not necessary. Blocks main thread.
                //session.CookNode(childNodeID, HEU_PluginSettings.CookTemplatedGeos);

                HAPI_NodeInfo childNodeInfo = new HAPI_NodeInfo();
                if (!session.GetNodeInfo(topNodeID, ref childNodeInfo))
                {
                    return false;
                }

                string nodeName = HEU_SessionManager.GetString(childNodeInfo.nameSH, session);
                //HEU_Logger.LogFormat("TOP Node: name={0}, type={1}", nodeName, childNodeInfo.type);

                TOPNodeTags tags = new TOPNodeTags();
                if (useHEngineData)
                {
                    ParseHEngineData(session, topNodeID, ref childNodeInfo, ref tags);

                    if (!tags._showHEngineData)
                    {
                        continue;
                    }
                }
                else
                {
                    tags._show = true;
                    tags._showHEngineData = true;
                }

                HEU_TOPNodeData topNodeData = GetTOPNodeByName(nodeName, topNetwork._topNodes);
                if (topNodeData == null)
                {
                    topNodeData = new HEU_TOPNodeData();
                }
                else
                {
                    topNetwork._topNodes.Remove(topNodeData);
                }

                newNodes.Add(topNodeData);

                //topNodeData.Reset();
                topNodeData._nodeID = topNodeID;
                topNodeData._nodeName = nodeName;
                topNodeData._parentName = topNetwork._parentName + "_" + topNetwork._nodeName;
                topNodeData._tags = tags;

                // Note: Don't have to compare with _showHEngineData because it won't exist in network if false
                if (_bUseTOPOutputFilter && _topNodeFilter != "")
                {
                    if (!nodeName.StartsWith(_topNodeFilter))
                    {
                        topNodeData._tags._show = false;
                    }
                }

                if (_bUseTOPOutputFilter)
                {
                    bool bAutoLoad = false;
                    if (_topOutputFilter == "")
                    {
                        bAutoLoad = true;
                    }
                    else if (nodeName.StartsWith(_topOutputFilter))
                    {
                        bAutoLoad = true;
                    }

                    topNodeData._tags._autoload |= bAutoLoad;
                    topNodeData._showResults = topNodeData._tags._autoload;
                }
            }

            // Clear old unused TOP nodes
            for (int i = 0; i < topNetwork._topNodes.Count; ++i)
            {
                ClearTOPNodeWorkItemResults(topNetwork._topNodes[i]);
            }

            topNetwork._topNodes = newNodes;

            // Get list of updated TOP node names
            SetupTopNetworkNames(topNetwork);

            return true;
        }


        private void ClearAllTOPData()
        {
            // Clears all TOP data

            foreach (HEU_TOPNetworkData network in _topNetworks)
            {
                foreach (HEU_TOPNodeData node in network._topNodes)
                {
                    ClearTOPNodeWorkItemResults(node);
                }
            }

            _topNetworks.Clear();
            _topNetworkNames = new string[0];
        }

        private static void ClearTOPNetworkWorkItemResults(HEU_TOPNetworkData topNetwork)
        {
            foreach (HEU_TOPNodeData node in topNetwork._topNodes)
            {
                ClearTOPNodeWorkItemResults(node);
            }
        }

        internal static void ClearTOPNodeWorkItemResults(HEU_TOPNodeData topNode)
        {
            int numResults = topNode._workResults.Count;
            for (int i = 0; i < numResults; ++i)
            {
                DestroyWorkItemResultData(topNode, topNode._workResults[i]);
            }

            topNode._workResults.Clear();

            if (topNode._workResultParentGO != null)
            {
                HEU_GeneralUtility.DestroyImmediate(topNode._workResultParentGO);
            }
        }

        internal static void ClearWorkItemResultByID(HEU_TOPNodeData topNode, HAPI_PDG_WorkItemId workItemID)
        {
            HEU_TOPWorkResult result = GetWorkResultByID(topNode, workItemID);
            ClearWorkItemResult(topNode, result);
        }

        private static void ClearWorkItemResult(HEU_TOPNodeData topNode, HEU_TOPWorkResult result)
        {
            if (result != null)
            {
                DestroyWorkItemResultData(topNode, result);

                topNode._workResults.Remove(result);
            }
        }

        internal void UpdateTOPNodeResultsVisibility(HEU_TOPNodeData topNode)
        {
            if (topNode._workResultParentGO != null)
            {
                topNode._workResultParentGO.SetActive(topNode._showResults);
            }
        }

        private static HEU_TOPWorkResult GetWorkResultByID(HEU_TOPNodeData topNode, HAPI_PDG_WorkItemId workItemID)
        {
            HEU_TOPWorkResult result = null;
            foreach (HEU_TOPWorkResult res in topNode._workResults)
            {
                if (res._workItemID == workItemID)
                {
                    result = res;
                    break;
                }
            }

            return result;
        }

        private static void DestroyWorkItemResultData(HEU_TOPNodeData topNode, HEU_TOPWorkResult result)
        {
            if (result._generatedGOs != null)
            {
                int numGOs = result._generatedGOs.Count;
                for (int i = 0; i < numGOs; ++i)
                {
                    if (result._generatedGOs[i] != null)
                    {
                        HEU_GeoSync geoSync = result._generatedGOs[i].GetComponent<HEU_GeoSync>();
                        if (geoSync != null)
                        {
                            geoSync.Unload();
                        }

                        //HEU_Logger.LogFormat("Destroy result: " + result._generatedGOs[i].name);
                        HEU_GeneralUtility.DestroyImmediate(result._generatedGOs[i]);
                        result._generatedGOs[i] = null;
                    }
                }

                result._generatedGOs.Clear();
            }
        }


        /// <summary>
        /// Load the geometry generated as results of the given work item, of the given TOP node.
        /// The load will be done asynchronously.
        /// Results must be tagged with 'file', and must have a file path, otherwise will not be loaded.
        /// </summary>
        /// <param name="session">Houdini Engine session that the TOP node is in</param>
        /// <param name="topNode">TOP node that the work item belongs to</param>
        /// <param name="workItemInfo">Work item whose results to load</param>
        /// <param name="resultInfos">Results data</param>
        /// <param name="workItemID">The work item's ID. Required for clearning its results.</param>
        internal void LoadResults(HEU_SessionBase session, HEU_TOPNodeData topNode, HAPI_PDG_WorkItemInfo workItemInfo,
            HAPI_PDG_WorkItemOutputFile[] resultInfos, HAPI_PDG_WorkItemId workItemID,
            System.Action<HEU_TOPNodeData, HEU_SyncedEventData> OnSynced)
        {
            // Create HEU_GeoSync objects, set results, and sync it

            string workItemName = HEU_SessionManager.GetString(workItemInfo.nameSH, session);
            //HEU_Logger.LogFormat("Work item: {0}:: name={1}, results={2}", workItemInfo.index, workItemName, workItemInfo.numResults);

            // Clear previously generated result
            ClearWorkItemResultByID(topNode, workItemID);

            if (resultInfos == null || resultInfos.Length == 0)
            {
                return;
            }

            HEU_TOPWorkResult result = GetWorkResultByID(topNode, workItemID);
            if (result == null)
            {
                result = new HEU_TOPWorkResult();
                result._workItemIndex = workItemInfo.index;
                result._workItemID = workItemID;

                topNode._workResults.Add(result);
            }

            // Load each result geometry
            int numResults = resultInfos.Length;
            _numTotalResults = numResults;
            _numLoadingResults = 0;
            for (int i = 0; i < numResults; ++i)
            {
                if (resultInfos[i].tagSH <= 0 || resultInfos[i].filePathSH <= 0)
                {
                    continue;
                }

                string tag = HEU_SessionManager.GetString(resultInfos[i].tagSH, session);
                string path = HEU_SessionManager.GetString(resultInfos[i].filePathSH, session);


                //HEU_Logger.LogFormat("Result for work item {0}: result={1}, tag={2}, path={3}", result._workItemIndex, i, tag, path);

                if (string.IsNullOrEmpty(tag) || !tag.StartsWith("file"))
                {
                    continue;
                }

                string name = string.Format("{0}_{1}_{2}",
                    topNode._parentName,
                    workItemName,
                    workItemInfo.index);

                // Get or create parent GO
                if (topNode._workResultParentGO == null)
                {
                    topNode._workResultParentGO = HEU_GeneralUtility.CreateNewGameObject(topNode._nodeName);
                    HEU_GeneralUtility.SetParentWithCleanTransform(GetLoadRootTransform(),
                        topNode._workResultParentGO.transform);
                    topNode._workResultParentGO.SetActive(topNode._showResults);
                }

                GameObject newOrExistingGO = null;
                int existingObjectIndex = -1;

                for (int j = 0; j < result._generatedGOs.Count; j++)
                {
                    if (result._generatedGOs[j] != null)
                    {
                        HEU_GeoSync oldGeoSync = result._generatedGOs[j].GetComponent<HEU_GeoSync>();
                        if (oldGeoSync != null && oldGeoSync._filePath == path)
                        {
                            oldGeoSync.Reset();
                            existingObjectIndex = j;
                            newOrExistingGO = result._generatedGOs[j];
                            break;
                        }
                    }
                }

                if (existingObjectIndex < 0)
                {
                    newOrExistingGO = HEU_GeneralUtility.CreateNewGameObject(name);
                    ;
                    result._generatedGOs.Add(newOrExistingGO);
                }


                HEU_GeneralUtility.SetParentWithCleanTransform(topNode._workResultParentGO.transform,
                    newOrExistingGO.transform);

                // HEU_GeoSync does the loading
                HEU_GeoSync geoSync = newOrExistingGO.GetComponent<HEU_GeoSync>();

                if (geoSync == null)
                {
                    geoSync = newOrExistingGO.AddComponent<HEU_GeoSync>();
                }

                geoSync._filePath = path;
                geoSync.SetOutputCacheDirectory(_outputCachePathRoot);

                if (geoSync != null && OnSynced != null)
                {
                    System.Action<HEU_SyncedEventData> OnSyncedCallback = (HEU_SyncedEventData Data) =>
                    {
                        _numLoadingResults++;
                        if (_numLoadingResults >= _numTotalResults)
                        {
                            OnSynced(topNode, Data);
                        }

                        if (geoSync)
                        {
                            geoSync.OnSynced = null;
                        }
                    };

                    geoSync.OnSynced = OnSyncedCallback;
                }

                geoSync.StartSync();
            }
        }

        private Transform GetLoadRootTransform()
        {
            if (_loadRootGameObject == null)
            {
                _loadRootGameObject = HEU_GeneralUtility.CreateNewGameObject(_assetName + " _OUTPUTS");
            }

            return _loadRootGameObject.transform;
        }

        public void RepaintUI()
        {
            if (_repaintUIDelegate != null)
            {
                _repaintUIDelegate();
            }
        }

        internal void UpdateWorkItemTally()
        {
            _workItemTally.ZeroAll();

            int numNetworks = _topNetworks.Count;
            for (int i = 0; i < numNetworks; ++i)
            {
                int numNodes = _topNetworks[i]._topNodes.Count;
                for (int j = 0; j < numNodes; ++j)
                {
                    _workItemTally._totalWorkItems += _topNetworks[i]._topNodes[j]._workItemTally._totalWorkItems;
                    _workItemTally._waitingWorkItems += _topNetworks[i]._topNodes[j]._workItemTally._waitingWorkItems;
                    _workItemTally._scheduledWorkItems +=
                        _topNetworks[i]._topNodes[j]._workItemTally._scheduledWorkItems;
                    _workItemTally._cookingWorkItems += _topNetworks[i]._topNodes[j]._workItemTally._cookingWorkItems;
                    _workItemTally._cookedWorkItems += _topNetworks[i]._topNodes[j]._workItemTally._cookedWorkItems;
                    _workItemTally._erroredWorkItems += _topNetworks[i]._topNodes[j]._workItemTally._erroredWorkItems;
                }
            }
        }

        internal void ResetTOPNetworkWorkItemTally(HEU_TOPNetworkData topNetwork)
        {
            if (topNetwork != null)
            {
                int numNodes = topNetwork._topNodes.Count;
                for (int i = 0; i < numNodes; ++i)
                {
                    topNetwork._topNodes[i]._workItemTally.ZeroAll();
                }
            }
        }

        internal void OnTOPNodeFilterChanged(string filter)
        {
            _topNodeFilter = filter;

            foreach (HEU_TOPNetworkData topNetwork in _topNetworks)
            {
                foreach (HEU_TOPNodeData topNode in topNetwork._topNodes)
                {
                    // Note: Don't have to compare with _showHEngineData because it won't exist in network if false
                    if (_bUseTOPNodeFilter)
                    {
                        topNode._tags._show = _topNodeFilter == "" || topNode._nodeName.StartsWith(_topNodeFilter);
                    }
                    else
                    {
                        topNode._tags._show = true;
                    }
                }

                SetupTopNetworkNames(topNetwork);
            }
        }

        internal void OnTOPOutputFilterChanged(string filter)
        {
            _topOutputFilter = filter;

            foreach (HEU_TOPNetworkData topNetwork in _topNetworks)
            {
                foreach (HEU_TOPNodeData topNode in topNetwork._topNodes)
                {
                    bool bAutoLoad = false;
                    if (_topOutputFilter == "")
                    {
                        bAutoLoad = true;
                    }
                    else if (topNode._nodeName.StartsWith(_topOutputFilter))
                    {
                        bAutoLoad = true;
                    }

                    topNode._tags._autoload = bAutoLoad | topNode._tags._autoloadHEngineData;
                    topNode._showResults = topNode._tags._autoload;
                }
            }
        }

        /// <summary>
        /// Helper to parse spare parm containing the filter key words.
        /// </summary>
        /// <param name="session">Houdini Engine session that the TOP node is in</param>
        /// <param name="topNodeID">TOP node to get spare parm from</param>
        /// <param name="nodeInfo">Previously queried TOP node info</param>
        /// <param name="nodeTags">Tag data to populate</param>
        private static void ParseHEngineData(HEU_SessionBase session, HAPI_NodeId topNodeID, ref HAPI_NodeInfo nodeInfo,
            ref TOPNodeTags nodeTags)
        {
            // Turn off session logging error when querying string parm that might not be there
            bool bLogError = session.LogErrorOverride;
            session.LogErrorOverride = false;

            int numStrings = nodeInfo.parmStringValueCount;
            HAPI_StringHandle henginedatash = 0;
            if (numStrings > 0 && session.GetParamStringValue(topNodeID, "henginedata", 0, out henginedatash))
            {
                string henginedatastr = HEU_SessionManager.GetString(henginedatash, session);
                //HEU_Logger.Log("HEngine data: " + henginedatastr);

                if (!string.IsNullOrEmpty(henginedatastr))
                {
                    string[] tags = henginedatastr.Split(',');
                    if (tags != null && tags.Length > 0)
                    {
                        foreach (string t in tags)
                        {
                            if (t.Equals("show"))
                            {
                                nodeTags._showHEngineData = true;
                                nodeTags._show = true;
                            }
                            else if (t.Equals("autoload"))
                            {
                                nodeTags._autoloadHEngineData = true;
                                nodeTags._autoload = true;
                            }
                        }
                    }
                }
            }

            // Logging error back on
            session.LogErrorOverride = bLogError;
        }

        private void SetupTopNetworkNames(HEU_TOPNetworkData topNetwork)
        {
            // Get list of updated TOP node names
            List<KeyValuePair<int, HEU_TOPNodeData>> displayNodeNames = GetNonHiddenTOPNodes(topNetwork);
            topNetwork._topNodeNames = new string[displayNodeNames.Count];
            for (int i = 0; i < displayNodeNames.Count; ++i)
            {
                topNetwork._topNodeNames[i] = displayNodeNames[i].Value._nodeName;
            }
        }

        private void OnSyncComplete(HEU_PDGCookedEventData Data)
        {
            if (_cookedDataEvent != null && Data != null)
            {
                _cookedDataEvent.Invoke(Data);
            }
        }

        internal static HEU_LinkStateWrapper LinkState_InternalToWrapper(LinkState linkState)
        {
            switch (linkState)
            {
                case LinkState.INACTIVE:
                    return HEU_LinkStateWrapper.INACTIVE;
                case LinkState.LINKING:
                    return HEU_LinkStateWrapper.LINKING;
                case LinkState.LINKED:
                    return HEU_LinkStateWrapper.LINKED;
                case LinkState.ERROR_NOT_LINKED:
                    return HEU_LinkStateWrapper.ERROR_NOT_LINKED;
                default:
                    return HEU_LinkStateWrapper.INACTIVE;
            }
        }

        internal static LinkState LinkState_WrapperToInternal(HEU_LinkStateWrapper linkState)
        {
            switch (linkState)
            {
                case HEU_LinkStateWrapper.INACTIVE:
                    return LinkState.INACTIVE;
                case HEU_LinkStateWrapper.LINKING:
                    return LinkState.LINKING;
                case HEU_LinkStateWrapper.LINKED:
                    return LinkState.LINKED;
                case HEU_LinkStateWrapper.ERROR_NOT_LINKED:
                    return LinkState.ERROR_NOT_LINKED;
                default:
                    return LinkState.INACTIVE;
            }
        }
    }
} // HoudiniEngineUnity
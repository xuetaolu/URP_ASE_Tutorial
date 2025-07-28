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
using System.Text;
using UnityEngine;

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
    using HAPI_SessionId = System.Int64;

    /// <summary>
    /// Global object that manages all PDG-specific things on Unity side.
    /// Handles PDG events for all PDG graph contexts.
    /// Manages and updates all HEU_PDGAssetLink objects in scene.
    /// </summary>
    public class HEU_PDGSession
    {
        public static HEU_PDGSession GetPDGSession()
        {
            if (_pdgSession == null)
            {
                _pdgSession = new HEU_PDGSession();
            }

            return _pdgSession;
        }

        public HEU_PDGSession()
        {
#if UNITY_EDITOR && HOUDINIENGINEUNITY_ENABLED
            EditorApplication.update += Update;
#endif
        }

        public void AddAsset(HEU_PDGAssetLink asset)
        {
#if HOUDINIENGINEUNITY_ENABLED
            if (!_pdgAssets.Contains(asset))
            {
                _pdgAssets.Add(asset);
                //HEU_Logger.Log("Adding asset " + asset.AssetName + " with total " + _pdgAssets.Count);
            }
#endif
        }

        public void RemoveAsset(HEU_PDGAssetLink asset)
        {
#if HOUDINIENGINEUNITY_ENABLED
            // Setting the asset reference to null and removing
            // later in Update in case of removing while iterating the list
            int index = _pdgAssets.IndexOf(asset);
            if (index >= 0)
            {
                _pdgAssets[index] = null;
            }
#endif
        }

        private void Update()
        {
#if HOUDINIENGINEUNITY_ENABLED
            CleanUp();

            UpdatePDGContext();
#endif
        }

        private void CleanUp()
        {
            for (int i = 0; i < _pdgAssets.Count; ++i)
            {
                if (_pdgAssets[i] == null)
                {
                    _pdgAssets.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Query all the PDG graph context in the current Houdini Engine session.
        /// Handle PDG events, work item status updates.
        /// Forward relevant events to HEU_PDGAssetLink objects.
        /// </summary>
        private void UpdatePDGContext()
        {
#if HOUDINIENGINEUNITY_ENABLED
            HEU_SessionBase session = GetHAPIPDGSession(false);
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            // Get current PDG graph contexts
            ReinitializePDGContext();

            // Process next set of events for each graph context
            if (_pdgContextIDs != null)
            {
                foreach (HAPI_PDG_GraphContextId contextID in _pdgContextIDs)
                {
                    int pdgStateInt;
                    if (!session.GetPDGState(contextID, out pdgStateInt))
                    {
                        SetErrorState("Failed to get PDG state", true);
                        continue;
                    }

                    _pdgState = (HAPI_PDG_State)pdgStateInt;

                    // Only initialize event array if not valid, or user resized max size
                    if (_pdgQueryEvents == null || _pdgQueryEvents.Length != _pdgMaxProcessEvents)
                    {
                        _pdgQueryEvents = new HAPI_PDG_EventInfo[_pdgMaxProcessEvents];
                    }

                    for (int i = 0; i < _pdgQueryEvents.Length; ++i)
                    {
                        ResetPDGEventInfo(ref _pdgQueryEvents[i]);
                    }

                    int eventCount = 0;
                    int remainingCount = 0;
                    if (!session.GetPDGEvents(contextID, _pdgQueryEvents, _pdgMaxProcessEvents, out eventCount,
                            out remainingCount))
                    {
                        SetErrorState("Failed to get PDG events", true);
                        continue;
                    }

                    for (int i = 0; i < eventCount; ++i)
                    {
                        ProcessPDGEvent(session, contextID, ref _pdgQueryEvents[i]);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Query the currently active PDG graph contexts in the Houdini Engine session.
        /// Should be done each time to get latest set of graph contexts.
        /// </summary>
        public void ReinitializePDGContext()
        {
#if HOUDINIENGINEUNITY_ENABLED
            HEU_SessionBase session = GetHAPIPDGSession(false);
            if (session == null || !session.IsSessionValid())
            {
                _pdgContextIDs = null;
                return;
            }

            int numContexts = 0;
            if (session.GetPDGGraphContextsCount(out numContexts) && numContexts > 0)
            {
                HAPI_StringHandle[] contextNames = new HAPI_StringHandle[numContexts];
                if (_pdgContextIDs == null || numContexts != _pdgContextIDs.Length)
                {
                    _pdgContextIDs = new HAPI_PDG_GraphContextId[numContexts];
                }

                if (!session.GetPDGGraphContexts(contextNames, _pdgContextIDs, 0, numContexts, false))
                {
                    _pdgContextIDs = null;
                }
            }
            else
            {
                _pdgContextIDs = null;
            }
#endif
        }

        /// <summary>
        /// Process a PDG event. Notify the relevant HEU_PDGAssetLink object.
        /// </summary>
        /// <param name="session">Houdini Engine session</param>
        /// <param name="contextID">PDG graph context ID</param>
        /// <param name="eventInfo">PDG event info</param>
        private void ProcessPDGEvent(HEU_SessionBase session, HAPI_PDG_GraphContextId contextID,
            ref HAPI_PDG_EventInfo eventInfo)
        {
#if HOUDINIENGINEUNITY_ENABLED
            HEU_PDGAssetLink assetLink = null;
            HEU_TOPNodeData topNode = null;

            HAPI_PDG_EventType evType = (HAPI_PDG_EventType)eventInfo.eventType;
            HAPI_PDG_WorkItemState currentState = (HAPI_PDG_WorkItemState)eventInfo.currentState;
            HAPI_PDG_WorkItemState lastState = (HAPI_PDG_WorkItemState)eventInfo.lastState;

            GetTOPAssetLinkAndNode(eventInfo.nodeId, out assetLink, out topNode);

            //string topNodeName = topNode != null ? string.Format("node={0}", topNode._nodeName) : string.Format("id={0}", eventInfo.nodeId);
            //HEU_Logger.LogFormat("PDG Event: {0}, type={1}, workitem={2}, curState={3}, lastState={4}", topNodeName, evType.ToString(), 
            //	eventInfo.workitemId, currentState, lastState);

            if (assetLink == null || topNode == null || topNode._nodeID != eventInfo.nodeId)
            {
                return;
            }

            EventMessageColor msgColor = EventMessageColor.DEFAULT;

            // Events can be split into TOP node specific or work item specific

            if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_NULL)
            {
                SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.NONE);
            }
            else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_NODE_CLEAR)
            {
                NotifyTOPNodePDGStateClear(assetLink, topNode);
            }
            else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_DIRTY_START)
            {
                SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.DIRTYING);

                //HEU_PDGAssetLink.ClearTOPNodeWorkItemResults(topNode);
            }
            else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_DIRTY_STOP)
            {
                SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.DIRTIED);
            }
            else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_COOK_ERROR)
            {
                SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.COOK_FAILED);
                msgColor = EventMessageColor.ERROR;
            }
            else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_COOK_WARNING)
            {
                msgColor = EventMessageColor.WARNING;
            }
            else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_COOK_COMPLETE)
            {
                SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.COOK_COMPLETE);
            }
            else
            {
                // Work item events

                HEU_TOPNodeData.PDGState currentTOPPDGState = topNode._pdgState;

                if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_WORKITEM_ADD)
                {
                    _totalNumItems++;
                    NotifyTOPNodeTotalWorkItem(assetLink, topNode, 1);
                }
                else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_WORKITEM_REMOVE)
                {
                    NotifyTOPNodeTotalWorkItem(assetLink, topNode, -1);
                }
                else if (evType == HAPI_PDG_EventType.HAPI_PDG_EVENT_WORKITEM_STATE_CHANGE)
                {
                    // Last states
                    if (lastState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_WAITING &&
                        currentState != HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_WAITING)
                    {
                        NotifyTOPNodeWaitingWorkItem(assetLink, topNode, -1);
                    }
                    else if (lastState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_COOKING &&
                             currentState != HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_COOKING)
                    {
                        NotifyTOPNodeCookingWorkItem(assetLink, topNode, -1);
                    }
                    else if (lastState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_SCHEDULED &&
                             currentState != HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_SCHEDULED)
                    {
                        NotifyTOPNodeScheduledWorkItem(assetLink, topNode, -1);
                    }

                    // New states
                    if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_WAITING)
                    {
                        NotifyTOPNodeWaitingWorkItem(assetLink, topNode, 1);
                    }
                    else if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_UNCOOKED)
                    {
                    }
                    else if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_DIRTY)
                    {
                        //HEU_Logger.LogFormat("Dirty: id={0}", eventInfo.workitemId);

                        ClearWorkItemResult(session, contextID, eventInfo, topNode);
                    }
                    else if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_SCHEDULED)
                    {
                        NotifyTOPNodeScheduledWorkItem(assetLink, topNode, 1);
                    }
                    else if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_COOKING)
                    {
                        NotifyTOPNodeCookingWorkItem(assetLink, topNode, 1);
                    }
                    else if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_COOKED_SUCCESS ||
                             currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_COOKED_CACHE)
                    {
                        NotifyTOPNodeCookedWorkItem(assetLink, topNode);

                        // On cook success, handle results
                        if (topNode._tags._autoload)
                        {
                            HAPI_PDG_WorkItemInfo workItemInfo = new HAPI_PDG_WorkItemInfo();
                            if (!session.GetWorkItemInfo(contextID, eventInfo.workItemId, ref workItemInfo))
                            {
                                HEU_Logger.LogErrorFormat("Failed to get work item {1} info for {0}", topNode._nodeName,
                                    eventInfo.workItemId);
                                return;
                            }

                            if (workItemInfo.outputFileCount > 0)
                            {
                                HAPI_PDG_WorkItemOutputFile[] outputFiles =
                                    new HAPI_PDG_WorkItemOutputFile[workItemInfo.outputFileCount];
                                int resultCount = workItemInfo.outputFileCount;
                                if (!session.GetWorkItemOutputFiles(topNode._nodeID, eventInfo.workItemId, outputFiles,
                                        resultCount))
                                {
                                    HEU_Logger.LogErrorFormat("Failed to get work item {1} result info for {0}",
                                        topNode._nodeName, eventInfo.workItemId);
                                    return;
                                }

                                assetLink.LoadResults(session, topNode, workItemInfo, outputFiles, eventInfo.workItemId,
                                    OnWorkItemLoadResults);
                            }
                        }
                    }
                    else if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_COOKED_FAIL)
                    {
                        // TODO: on cook failure, get log path?
                        NotifyTOPNodeErrorWorkItem(assetLink, topNode);
                        msgColor = EventMessageColor.ERROR;
                    }
                    else if (currentState == HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_COOKED_CANCEL)
                    {
                        // Ignore it because in-progress cooks can be cancelled when automatically recooking graph
                    }
                }

                if (currentTOPPDGState == HEU_TOPNodeData.PDGState.COOKING)
                {
                    if (topNode.AreAllWorkItemsComplete())
                    {
                        if (topNode.AnyWorkItemsFailed())
                        {
                            SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.COOK_FAILED);
                        }
                        else
                        {
                            SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.COOK_COMPLETE);
                        }
                    }
                }
                else if (topNode.AnyWorkItemsPending())
                {
                    SetTOPNodePDGState(assetLink, topNode, HEU_TOPNodeData.PDGState.COOKING);
                }
            }

            if (eventInfo.msgSH >= 0)
            {
                string eventMsg = HEU_SessionManager.GetString(eventInfo.msgSH, session);
                if (!string.IsNullOrEmpty(eventMsg))
                {
                    AddEventMessage(string.Format("<color={0}>{1} - {2}: {3}</color>\n",
                        _eventMessageColorCode[(int)msgColor],
                        evType,
                        topNode._nodeName,
                        eventMsg));
                }
            }

            CheckCallback(topNode);
#endif
        }

        private delegate void OnWorkItemLoadResultsDelegate(HEU_SyncedEventData OnSynced);

        private void OnWorkItemLoadResults(HEU_TOPNodeData topNode, HEU_SyncedEventData OnSynced)
        {
            _numItemsCompleted++;
            CheckCallback(topNode);
        }

        private void CheckCallback(HEU_TOPNodeData topNode)
        {
            if (_cookedDataEvent != null && _pendingCallback && _numItemsCompleted >= _totalNumItems)
            {
                _cookedDataEvent.Invoke(new HEU_PDGCookedEventData(_callbackSuccess, topNode));
                ResetCallbackVariables();
            }
        }


        /// <summary>
        /// Returns the HEU_PDGAssetLink and HEU_TOPNodeData associated with this TOP node ID
        /// </summary>
        /// <param name="nodeID">Node ID to query</param>
        /// <param name="assetLink">Found HEU_PDGAssetLink or null</param>
        /// <param name="topNode">Found top node with ID or null</param>
        /// <returns>Returns true if found</returns>
        private bool GetTOPAssetLinkAndNode(HAPI_NodeId nodeID, out HEU_PDGAssetLink assetLink,
            out HEU_TOPNodeData topNode)
        {
            assetLink = null;
            topNode = null;
            int numAssets = _pdgAssets.Count;
            for (int i = 0; i < numAssets; ++i)
            {
                topNode = _pdgAssets[i].GetTOPNode(nodeID);
                if (topNode != null)
                {
                    assetLink = _pdgAssets[i];
                    return true;
                }
            }

            return false;
        }

        private void SetTOPNodePDGState(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode,
            HEU_TOPNodeData.PDGState pdgState)
        {
            topNode._pdgState = pdgState;
            assetLink.RepaintUI();

            if (_cookedDataEvent != null && (pdgState == HEU_TOPNodeData.PDGState.COOK_COMPLETE ||
                                             pdgState == HEU_TOPNodeData.PDGState.COOK_FAILED))
            {
                bool bSuccess = pdgState == HEU_TOPNodeData.PDGState.COOK_COMPLETE;
                _callbackSuccess &= bSuccess;
                _pendingCallback = true;
            }
        }

        private void NotifyTOPNodePDGStateClear(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode)
        {
            //HEU_Logger.LogFormat("NotifyTOPNodePDGStateClear:: {0}", topNode._nodeName);
            topNode._pdgState = HEU_TOPNodeData.PDGState.NONE;
            topNode._workItemTally.ZeroAll();
            assetLink.RepaintUI();
        }

        private void NotifyTOPNodeTotalWorkItem(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode, int inc)
        {
            topNode._workItemTally._totalWorkItems = Mathf.Max(topNode._workItemTally._totalWorkItems + inc, 0);
            assetLink.RepaintUI();
        }

        private void NotifyTOPNodeCookedWorkItem(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode)
        {
            topNode._workItemTally._cookedWorkItems++;
            assetLink.RepaintUI();
        }

        private void NotifyTOPNodeErrorWorkItem(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode)
        {
            topNode._workItemTally._erroredWorkItems++;
            assetLink.RepaintUI();
        }

        private void NotifyTOPNodeWaitingWorkItem(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode, int inc)
        {
            topNode._workItemTally._waitingWorkItems = Mathf.Max(topNode._workItemTally._waitingWorkItems + inc, 0);
            assetLink.RepaintUI();
        }

        private void NotifyTOPNodeScheduledWorkItem(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode, int inc)
        {
            topNode._workItemTally._scheduledWorkItems = Mathf.Max(topNode._workItemTally._scheduledWorkItems + inc, 0);
            assetLink.RepaintUI();
        }

        private void NotifyTOPNodeCookingWorkItem(HEU_PDGAssetLink assetLink, HEU_TOPNodeData topNode, int inc)
        {
            topNode._workItemTally._cookingWorkItems = Mathf.Max(topNode._workItemTally._cookingWorkItems + inc, 0);
            assetLink.RepaintUI();
        }

        private static void ResetPDGEventInfo(ref HAPI_PDG_EventInfo eventInfo)
        {
            eventInfo.nodeId = HEU_Defines.HEU_INVALID_NODE_ID;
            eventInfo.workItemId = -1;
            eventInfo.dependencyId = -1;
            eventInfo.currentState = (int)HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_UNDEFINED;
            eventInfo.lastState = (int)HAPI_PDG_WorkItemState.HAPI_PDG_WORKITEM_UNDEFINED;
            eventInfo.eventType = (int)HAPI_PDG_EventType.HAPI_PDG_EVENT_NULL;
        }

        private void SetErrorState(string msg, bool bLogIt)
        {
            // Log first error
            if (!_errored && bLogIt)
            {
                HEU_Logger.LogError(msg);
            }

            _errored = true;
            _errorMsg = msg;
        }

        private void ClearErrorState()
        {
            _errored = false;
            _errorMsg = "";
        }

        /// <summary>
        /// Return the current Houdini Engine session
        /// </summary>
        /// <returns></returns>
        public HEU_SessionBase GetHAPIPDGSession(bool bCreate = true)
        {
            if (bCreate)
            {
                return HEU_SessionManager.GetOrCreateDefaultSession();
            }
            else
            {
                return HEU_SessionManager.GetDefaultSession();
            }
        }

        /// <summary>
        /// Cook the PDG graph of the specified TOP network
        /// </summary>
        /// <param name="topNetwork"></param>
        public void CookTOPNetworkOutputNode(HEU_TOPNetworkData topNetwork,
            System.Action<HEU_PDGCookedEventData> OnCook = null)
        {
#if HOUDINIENGINEUNITY_ENABLED
            ClearEventMessages();

            HEU_SessionBase session = GetHAPIPDGSession();
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            // Cancel all cooks. This is required as otherwise the graph gets into an infinite cook
            // state (bug?)
            if (_pdgContextIDs != null)
            {
                foreach (HAPI_PDG_GraphContextId contextID in _pdgContextIDs)
                {
                    session.CancelPDGCook(contextID);
                }
            }

            if (!session.CookPDG(topNetwork._nodeID, 0, 0))
            {
                HEU_Logger.LogErrorFormat("Cook node failed!");
            }

            _cookedDataEvent = OnCook;

            ResetCallbackVariables();

#endif
        }

        /// <summary>
        /// Pause the PDG graph cook of the specified TOP network
        /// </summary>
        /// <param name="topNetwork"></param>
        public void PauseCook(HEU_TOPNetworkData topNetwork)
        {
#if HOUDINIENGINEUNITY_ENABLED
            HEU_SessionBase session = GetHAPIPDGSession();
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            // Cancel all cooks.
            if (_pdgContextIDs != null)
            {
                foreach (HAPI_PDG_GraphContextId contextID in _pdgContextIDs)
                {
                    session.PausePDGCook(contextID);
                }
            }
#endif
        }

        /// <summary>
        /// Cancel the PDG graph cook of the specified TOP network
        /// </summary>
        /// <param name="topNetwork"></param>
        public void CancelCook(HEU_TOPNetworkData topNetwork)
        {
#if HOUDINIENGINEUNITY_ENABLED
            HEU_SessionBase session = GetHAPIPDGSession();
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            // Cancel all cooks.
            if (_pdgContextIDs != null)
            {
                foreach (HAPI_PDG_GraphContextId contextID in _pdgContextIDs)
                {
                    session.CancelPDGCook(contextID);
                }
            }
#endif
        }

        /// <summary>
        /// Clear all work items' results of the specified TOP node. This destroys any loaded results (geometry etc).
        /// </summary>
        /// <param name="session"></param>
        /// <param name="contextID"></param>
        /// <param name="eventInfo"></param>
        /// <param name="topNode"></param>
        public void ClearWorkItemResult(HEU_SessionBase session, HAPI_PDG_GraphContextId contextID,
            HAPI_PDG_EventInfo eventInfo, HEU_TOPNodeData topNode)
        {
#if HOUDINIENGINEUNITY_ENABLED
            session.LogErrorOverride = false;

            HEU_PDGAssetLink.ClearWorkItemResultByID(topNode, eventInfo.workItemId);

            session.LogErrorOverride = true;
#endif
        }

        /// <summary>
        /// Returns true if successfully dirtied the TOP node.
        /// </summary>
        /// <param name="topNode">TOP node to dirty</param>
        public bool DirtyTOPNode(HAPI_NodeId nodeID)
        {
#if HOUDINIENGINEUNITY_ENABLED
            ClearEventMessages();

            HEU_SessionBase session = GetHAPIPDGSession();
            if (session != null && session.IsSessionValid())
            {
                return session.DirtyPDGNode(nodeID, true);
            }
#endif
            return false;
        }

        /// <summary>
        /// Returns true if cooked the specified TOP node.
        /// </summary>
        /// <param name="topNode"></param>
        public bool CookTOPNode(HAPI_NodeId nodeID)
        {
#if HOUDINIENGINEUNITY_ENABLED
            ClearEventMessages();

            HEU_SessionBase session = GetHAPIPDGSession();
            if (session != null && session.IsSessionValid())
            {
                return session.CookPDG(nodeID, 0, 0);
            }
#endif
            return false;
        }

        /// <summary>
        /// Returns true if dirtied the TOP network.
        /// </summary>
        public bool DirtyAll(HAPI_NodeId nodeID)
        {
#if HOUDINIENGINEUNITY_ENABLED
            ClearEventMessages();
            ResetCallbackVariables();

            HEU_SessionBase session = GetHAPIPDGSession();
            if (session != null && session.IsSessionValid())
            {
                return session.DirtyPDGNode(nodeID, true);
            }
#endif
            return false;
        }

        public void AddEventMessage(string msg)
        {
            _pdgEventMessages.AppendLine(msg);
        }

        public string GetEventMessages()
        {
            return _pdgEventMessages.ToString();
        }

        public void ClearEventMessages()
        {
            // .Net 3.5 and lower does not have StringBuilder.clear()
            _pdgEventMessages.Length = 0;
        }

        public static HAPI_NodeId[] GetNonBypassedNetworkIds(HEU_SessionBase session, HAPI_NodeId assetId)
        {
            if (assetId < 0)
            {
                return null;
            }

            // Get all network nodes recursively
            // Getting all networks because TOP nework SOPs aren't considered being of TTOP network type, but SOP type
            int networkNodeCount = 0;
            if (!session.ComposeChildNodeList(assetId, (int)HAPI_NodeType.HAPI_NODETYPE_ANY,
                    (int)HAPI_NodeFlags.HAPI_NODEFLAGS_NETWORK, true, ref networkNodeCount, false))
            {
                return null;
            }

            if (networkNodeCount <= 0)
            {
                return null;
                ;
            }

            HAPI_NodeId[] allNetworkNodeIds = new HAPI_NodeId[networkNodeCount];
            if (!session.GetComposedChildNodeList(assetId, allNetworkNodeIds, networkNodeCount, false))
            {
                return null;
            }


            int byPassedTOPNetNodeCount = 0;
            if (!session.ComposeChildNodeList(assetId,
                    (int)HAPI_NodeType.HAPI_NODETYPE_ANY,
                    (int)(HAPI_NodeFlags.HAPI_NODEFLAGS_NETWORK | HAPI_NodeFlags.HAPI_NODEFLAGS_BYPASS),
                    true, ref byPassedTOPNetNodeCount, false))
            {
                return allNetworkNodeIds;
            }

            // Get only non bypassed nodes
            if (byPassedTOPNetNodeCount > 0)
            {
                HAPI_NodeId[] allBypassedTOPNetNodeIDs = new HAPI_NodeId[byPassedTOPNetNodeCount];
                if (!session.GetComposedChildNodeList(assetId, allBypassedTOPNetNodeIDs, byPassedTOPNetNodeCount,
                        false))
                {
                    return allNetworkNodeIds;
                }

                int lastIndex = allNetworkNodeIds.Length - 1;
                for (int idx = allNetworkNodeIds.Length - 1; idx >= 0; idx--)
                {
                    if (System.Array.Exists<HAPI_NodeId>(allBypassedTOPNetNodeIDs,
                            (HAPI_NodeId id) => id == allNetworkNodeIds[idx]))
                    {
                        // Remove idx by swapping to end and resizing
                        int tmp = allNetworkNodeIds[idx];
                        allNetworkNodeIds[idx] = allNetworkNodeIds[lastIndex];
                        allNetworkNodeIds[lastIndex] = tmp;
                        lastIndex--;
                    }
                }

                System.Array.Resize<HAPI_NodeId>(ref allNetworkNodeIds, lastIndex + 1);
            }

            return allNetworkNodeIds;
        }

        // Checks whether or not an asset is a PDG asset, similar to the unreal plugin
        public static bool IsPDGAsset(HEU_SessionBase session, HAPI_NodeId assetId)
        {
            if (assetId < 0) return false;

            // Get the list of all non-bypassed TOP nodes within the current network (ignoring schedulers)
            int TOPNodeCount = 0;

            if (!session.ComposeChildNodeList(assetId,
                    (int)HAPI_NodeType.HAPI_NODETYPE_TOP,
                    (int)(HAPI_NodeFlags.HAPI_NODEFLAGS_TOP_NONSCHEDULER | HAPI_NodeFlags.HAPI_NODEFLAGS_NON_BYPASS),
                    true,
                    ref TOPNodeCount, false))
            {
                return false;
            }

            if (TOPNodeCount > 0) return true;

            // Old method of determining if it is a PDG asset. Is too slow for certain HDAs
            /*
            HAPI_NodeId[] allNetworkNodeIds = GetNonBypassedNetworkIds(session, assetId);
            if (allNetworkNodeIds == null || allNetworkNodeIds.Length == 0)
            {
            return false;
            }

            // Find nodes with TOP child nodes
            foreach (HAPI_NodeId currentNodeId in allNetworkNodeIds)
            {
            if (currentNodeId < 0)
            {
                continue;
            }

            HAPI_NodeInfo currentNodeInfo = new HAPI_NodeInfo();
            if (!session.GetNodeInfo(currentNodeId, ref currentNodeInfo, false))
            {
                continue;
            }

            if (currentNodeInfo.type != HAPI_NodeType.HAPI_NODETYPE_TOP
                && currentNodeInfo.type != HAPI_NodeType.HAPI_NODETYPE_SOP)
            {
                continue;
            }

            int topNodeCount = 0;
            if (!session.ComposeChildNodeList(currentNodeId,
                (int)HAPI_NodeType.HAPI_NODETYPE_TOP, (int)HAPI_NodeFlags.HAPI_NODEFLAGS_TOP_NONSCHEDULER, true, ref topNodeCount))
            {
                continue;
            }

            if (topNodeCount > 0)
            {
                return true;
            }
            }
            */


            // No valid TOP node found :(
            return false;
        }

        private void ResetCallbackVariables()
        {
            _pendingCallback = false;
            _numItemsCompleted = 0;
            _totalNumItems = 0;
            _callbackSuccess = true;
        }

        //	DATA ------------------------------------------------------------------------------------------------------

        // Global PDG session object
        private static HEU_PDGSession _pdgSession;

        // List of all registered HEU_PDGAssetLink in the scene
        private List<HEU_PDGAssetLink> _pdgAssets = new List<HEU_PDGAssetLink>();

        // Maximum number of PDG events to process at a time
        public int _pdgMaxProcessEvents = 100;

        // Storage of latest PDG events
        public HAPI_PDG_EventInfo[] _pdgQueryEvents;

        // Storage of latest PDG graph context data
        public HAPI_PDG_GraphContextId[] _pdgContextIDs;

        public bool _errored;
        public string _errorMsg;

        public HAPI_PDG_State _pdgState = HAPI_PDG_State.HAPI_PDG_STATE_READY;

        private System.Action<HEU_PDGCookedEventData> _cookedDataEvent;

        public System.Action<HEU_PDGCookedEventData> CookedDataEvent
        {
            get => _cookedDataEvent;
            set => _cookedDataEvent = value;
        }

        private bool _pendingCallback = false;
        private int _numItemsCompleted = 0;
        private int _totalNumItems = 0;
        private bool _callbackSuccess = true;

        // PDG event messages generated during cook
        [SerializeField] private StringBuilder _pdgEventMessages = new StringBuilder();

        private enum EventMessageColor
        {
            DEFAULT,
            WARNING,
            ERROR
        }

        private string[] _eventMessageColorCode =
        {
            "#c0c0c0ff",
            "#ffa500ff",
            "#ff0000ff"
        };
    }
} // namespace HoudiniEngineUnity
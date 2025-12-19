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

using UnityEngine;

namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;

    //[ExecuteInEditMode] // Needed to get OnDestroy callback when deleted in Editor
    public class HEU_NodeSync : HEU_BaseSync
    {
        #region FUNCTIONS

        #region SETUP

        private void OnEnable()
        {
#if HOUDINIENGINEUNITY_ENABLED
            // Adding in OnEnable as its called after a code recompile (Awake is not).
            HEU_AssetUpdater.AddNodeSyncForUpdate(this);
#endif
        }

        private void OnDestroy()
        {
            // Need to remove the NodySync from AssetUpdater.
            // Parent's OnDestroy doesn't get called so
            // do session deletion here as well.

#if HOUDINIENGINEUNITY_ENABLED
            HEU_AssetUpdater.RemoveNodeSync(this);
#endif

            DeleteSessionData();
        }

        public void InitializeFromHoudini(HEU_SessionBase session, HAPI_NodeId nodeID,
            string nodeName, string filePath)
        {
            Initialize();

            _sessionID = session.GetSessionData().SessionID;
            _cookNodeID = nodeID;
            _nodeName = nodeName;
            _nodeSaveFilePath = filePath;

            StartSync();
        }

        protected override void SetupLoadTask(HEU_SessionBase session)
        {
            if (_loadTask == null)
            {
                _loadTask = new HEU_ThreadedTaskLoadGeo();
            }

            _loadTask.SetupLoadNode(session, this, _cookNodeID, _nodeName);
            _loadTask.Start();
        }

        #endregion

        #region UTILITY

        public bool SaveNodeToFile(string filePath)
        {
            HEU_SessionBase session = GetHoudiniSession(false);
            if (session == null)
            {
                return false;
            }

            HEU_Logger.Log("Saving to " + filePath);
            _nodeSaveFilePath = filePath;

            return session.SaveNodeToFile(_cookNodeID, filePath);
        }

        public static void CreateNodeSync(HEU_SessionBase session, string opName, string nodeNabel)
        {
            if (session == null)
            {
                session = HEU_SessionManager.GetDefaultSession();
            }

            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            HAPI_NodeId newNodeID = -1;
            HAPI_NodeId parentNodeId = -1;

            if (!session.CreateNode(parentNodeId, opName, nodeNabel, true, out newNodeID))
            {
                HEU_Logger.LogErrorFormat("Unable to create merge SOP node for connecting input assets.");
                return;
            }

            if (parentNodeId == -1)
            {
                // When creating a node without a parent, for SOP nodes, a container
                // geometry object will have been created by HAPI.
                // In all cases we want to use the node ID of that object container
                // so the below code sets the parent's node ID.

                // But for SOP/subnet we actually do want the subnet SOP node ID
                // hence the useSOPNodeID argument here is to override it.
                bool useSOPNodeID = opName.Equals("SOP/subnet");

                HAPI_NodeInfo nodeInfo = new HAPI_NodeInfo();
                if (!session.GetNodeInfo(newNodeID, ref nodeInfo))
                {
                    return;
                }

                if (nodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP)
                {
                    if (!useSOPNodeID)
                    {
                        newNodeID = nodeInfo.parentId;
                    }
                }
                else if (nodeInfo.type != HAPI_NodeType.HAPI_NODETYPE_OBJ)
                {
                    HEU_Logger.LogErrorFormat("Unsupported node type {0}", nodeInfo.type);
                    return;
                }
            }

            GameObject newGO = HEU_GeneralUtility.CreateNewGameObject(nodeNabel);

            HEU_NodeSync nodeSync = newGO.AddComponent<HEU_NodeSync>();
            nodeSync.InitializeFromHoudini(session, newNodeID, nodeNabel, "");
        }

        #endregion

        #region SYNC

        public override void Resync()
        {
            if (_syncing)
            {
                return;
            }

            // Not unloading, but rather just generating local geometry
            DestroyGeneratedData();
            StartSync();
        }

        #endregion

        #endregion

        #region UPDATE

        public override void SyncUpdate()
        {
            if (_syncing || _cookNodeID == -1 || !_firstSyncComplete)
            {
                return;
            }

            if (!HEU_PluginSettings.SessionSyncAutoCook || !_sessionSyncAutoCook)
            {
                return;
            }

            HEU_SessionBase session = GetHoudiniSession(false);
            if (session == null || !session.IsSessionValid() || !session.IsSessionSync())
            {
                return;
            }

            // TODO: should check parent obj, or turn off recurse?
            // TODO: instead of cook count, how about cook time? but how to handle hierarchy cook change?
            int oldCount = _totalCookCount;
            session.GetTotalCookCount(
                _cookNodeID,
                (int)(HAPI_NodeType.HAPI_NODETYPE_OBJ | HAPI_NodeType.HAPI_NODETYPE_SOP),
                (int)(HAPI_NodeFlags.HAPI_NODEFLAGS_OBJ_GEOMETRY | HAPI_NodeFlags.HAPI_NODEFLAGS_DISPLAY |
                      HAPI_NodeFlags.HAPI_NODEFLAGS_RENDER),
                true, out _totalCookCount);
            if (oldCount != _totalCookCount)
            {
                //HEU_Logger.LogFormat("Resyncing due to cook count (old={0}, new={1})", oldCount, _totalCookCount);

                if (_loadTask != null)
                {
                    _loadTask.Stop();
                }

                DestroyGeneratedData();

                StartSync();
            }
        }

        #endregion


        #region DATA

        public string _nodeSaveFilePath;

        #endregion
    }
} // HoudiniEngineUnity
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

    /// <summary>
    /// Instantiates an HDA in a separate thread, and generates its geometry.
    /// </summary>
    //[ExecuteInEditMode] // Needed to get OnDestroy callback when deleted in Editor
    public class HEU_AssetSync : HEU_BaseSync
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

        public void InitializeAsset(HEU_SessionBase session, string assetPath,
            string nodeName, Transform parent, Vector3 startPosition)
        {
            Initialize();

            //_cookNodeID = -1;

            _sessionID = session.GetSessionData().SessionID;
            _assetPath = assetPath;
            _nodeName = nodeName;

            Transform transform = gameObject.transform;
            transform.parent = parent;
            transform.position = startPosition;
        }

        private HEU_ThreadedTaskLoadGeo CreateThreadedTask()
        {
            HEU_ThreadedTaskLoadGeo loadTask = new HEU_ThreadedTaskLoadGeo();
            loadTask.Priority = System.Threading.ThreadPriority.Normal;
            loadTask.IsBackground = true;
            return loadTask;
        }

        public void SetLoadCallback(HEU_ThreadedTaskLoadGeo.HEU_LoadCallback callback)
        {
            if (_loadTask == null)
            {
                _loadTask = CreateThreadedTask();
            }

            _loadTask.SetLoadCallback(callback);
        }

        protected override void SetupLoadTask(HEU_SessionBase session)
        {
            if (_loadTask == null)
            {
                _loadTask = CreateThreadedTask();
            }

            _loadTask.SetupLoadAsset(session, this, _assetPath, _nodeName);
            _loadTask.Start();
        }

        #endregion

        #region UTILITY

        public override void OnLoadComplete(HEU_ThreadedTaskLoadGeo.HEU_LoadData loadData)
        {
            base.OnLoadComplete(loadData);

            if (_onAssetLoaded != null)
            {
                _onAssetLoaded.Invoke(this);
            }
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


        #region DATA

        public delegate void AssetSyncCallback(HEU_AssetSync assetSync);

        public AssetSyncCallback _onAssetLoaded;

        public string _assetPath;

        #endregion
    }
} // HoudiniEngineUnity
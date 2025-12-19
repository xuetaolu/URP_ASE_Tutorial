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

using System.Threading;
using UnityEngine;

namespace HoudiniEngineUnity
{
    /// <summary>
    /// Contains the SessionSync local state information for Unity plugin.
    /// The HEU_SesionSyncWindow uses the data stored here.
    /// This is stored as part of the Houdini Engine data (HEU_SessionData)
    /// when SessionSync is active.
    /// </summary>
    [System.Serializable]
    public class HEU_SessionSyncData
    {
        public enum Status
        {
            Stopped,
            Started,
            Connecting,
            Initializing,
            Connected
        }

        // The SessionSync state for local session
        [SerializeField] private int _status = 0;

        // The time since last update
        public float _timeLastUpdate = 0;

        // The time when connecting to Houdini was started (used for timing out)
        public float _timeStartConnection = 0;

        // Thread-safe access to _status
        public Status SyncStatus
        {
            get
            {
                int istatus = Interlocked.CompareExchange(ref _status, 0, 0);
                return (Status)istatus;
            }
            set
            {
                int istatus = (int)value;
                Interlocked.Exchange(ref _status, istatus);
            }
        }

        // UI name for new node
        public string _newNodeName = "geo1";

        // UI index of node type
        public int _nodeTypeIndex = 0;

        // Flag to disregard this object due to Unity serialization
        // automatically creating it on code donmain reload
        public bool _validForConnection;

        // The last HAPI_Viewport update from HAPI
        public HAPI_Viewport _viewportHAPI = new HAPI_Viewport(true);

        // The last HAPI_Viewport update from local 
        public HAPI_Viewport _viewportLocal = new HAPI_Viewport(true);

        // Whether the viewport was just update locally
        public bool _viewportJustUpdated;

        // The last HAPI_SessionSyncInfo update
        public HAPI_SessionSyncInfo _syncInfo = new HAPI_SessionSyncInfo();
    }
}
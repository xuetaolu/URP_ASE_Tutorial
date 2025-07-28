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

using System;
using UnityEngine;

namespace HoudiniEngineUnity
{
    [Serializable]
    public enum SessionConnectionState
    {
        NOT_CONNECTED,
        CONNECTED,
        FAILED_TO_CONNECT
    }

    [Serializable]
    public enum SessionMode
    {
        Socket,
        Pipe
    }

    /// <summary>
    /// Container for session-specific data.
    /// Note that this is sealed for serialization purposes.
    /// </summary>
    [Serializable]
    public sealed class HEU_SessionData
    {
        public static long INVALID_SESSION_ID = -1;

        // Actual HAPI session data
        public HAPI_Session _HAPISession = new HAPI_Session();

#pragma warning disable 0414
        // Process ID for Thrift pipe session
        [SerializeField] private int _serverProcessID = -1;

        // Whether the session has been initialized
        [SerializeField] private bool _initialized;

        // Name of pipe (for pipe session)
        [SerializeField] private string _pipeName;

        [SerializeField] private int _port;
#pragma warning restore 0414

        // ID for the HEU_SessionBase class type
        [SerializeField] private string _sessionClassType;

        // Whether this is the default session
        [SerializeField] private bool _isDefaultSession;

        [SerializeField] private HEU_SessionSyncData _sessionSync = null;

        public HEU_SessionSyncData GetOrCreateSessionSync()
        {
            if (_sessionSync == null)
            {
                _sessionSync = new HEU_SessionSyncData();
            }

            return _sessionSync;
        }

        public HEU_SessionSyncData GetSessionSync()
        {
            return _sessionSync;
        }

        public void SetSessionSync(HEU_SessionSyncData syncData)
        {
            _sessionSync = syncData;
        }

        public long SessionID
        {
            get
            {
#if HOUDINIENGINEUNITY_ENABLED
                return _HAPISession.id;
#else
		return INVALID_SESSION_ID;
#endif
            }

            set { _HAPISession.id = value; }
        }

        public int ProcessID
        {
            get
            {
#if HOUDINIENGINEUNITY_ENABLED
                return _serverProcessID;
#else
		return -1;
#endif
            }

            set { _serverProcessID = value; }
        }

        public HAPI_SessionType SessionType
        {
            get
            {
#if HOUDINIENGINEUNITY_ENABLED
                return _HAPISession.type;
#else
		return 0;
#endif
            }

            set { _HAPISession.type = value; }
        }

        public bool IsInitialized
        {
            get
            {
#if HOUDINIENGINEUNITY_ENABLED
                return _initialized;
#else
		return false;
#endif
            }

            set { _initialized = value; }
        }

        public bool IsValidSessionID
        {
            get
            {
#if HOUDINIENGINEUNITY_ENABLED
                return SessionID > 0;
#else
		return false;
#endif
            }
        }

        public string PipeName
        {
            get
            {
#if HOUDINIENGINEUNITY_ENABLED
                return _pipeName;
#else
		return "";
#endif
            }

            set { _pipeName = value; }
        }

        public int Port
        {
            get => _port;

            set => _port = value;
        }

        public System.Type SessionClassType
        {
            get
            {
                if (string.IsNullOrEmpty(_sessionClassType))
                {
                    return null;
                }
                else
                {
                    return System.Type.GetType(_sessionClassType);
                }
            }

            set => _sessionClassType = value.ToString();
        }

        public bool IsDefaultSession
        {
            get => _isDefaultSession;

            set => _isDefaultSession = value;
        }

        public bool IsSessionSync => _sessionSync != null;

        [SerializeField] private SessionConnectionState _connectionState;

        public SessionConnectionState ThisConnectionMode
        {
            get => _connectionState;
            set => _connectionState = value;
        }

        [SerializeField] private SessionMode _sessionMode;

        public SessionMode ThisSessionMode
        {
            get => _sessionMode;
            set => _sessionMode = value;
        }
    }
} // HoudiniEngineUnity
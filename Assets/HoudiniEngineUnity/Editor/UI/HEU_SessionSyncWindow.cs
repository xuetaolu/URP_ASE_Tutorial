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

using System.Text;
using UnityEditor;
using UnityEngine;

namespace HoudiniEngineUnity
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;

    /// <summary>
    /// Handles the SessionSync UI and behaviour.
    /// Users can start Houdini with SessionSync or connect to SessionSync
    /// already running in Houdini.
    /// Allows to set connection settings, and HAPI_SessionSyncInfo settings.
    /// Allows to create new nodes in Houdini.
    /// Synchronizes viewport.
    /// </summary>
    [InitializeOnLoad]
    public class HEU_SessionSyncWindow : EditorWindow
    {
        /// <summary>
        /// Helper to display this window
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow(typeof(HEU_SessionSyncWindow), false, "HEngine SessionSync");
        }

        /// <summary>
        /// Unity callback when this window is enabled.
        /// </summary>
        private void OnEnable()
        {
            ReInitialize();

            // Request callback for ticking this window so we can update state,
            // and synchronize viewport.
            EditorApplication.update += UpdateSync;
        }

        /// <summary>
        /// Unity callback when this window is disabled / closed.
        /// </summary>
        private void OnDisable()
        {
            // Remove callback for ticking this window.
            EditorApplication.update -= UpdateSync;
        }

        /// <summary>
        /// Initialize the UI.
        /// </summary>
        private void ReInitialize()
        {
            _sessionMode = HEU_PluginSettings.Session_Mode;

            _port = HEU_PluginSettings.Session_Port;
            _pipeName = HEU_PluginSettings.Session_PipeName;

            _log = new StringBuilder();

            if (_connectionSyncData != null && !_connectionSyncData._validForConnection)
            {
                // The serializer creates a default _connectionSyncData which isn't
                // the correct SessionInfo. The real one is stored in SessionData.
                _connectionSyncData = null;
            }
        }

        /// <summary>
        /// Unity callback to draw this UI.
        /// </summary>
        private void OnGUI()
        {
            SetupUI();

            HEU_SessionSyncData syncData = GetSessionSyncData();

            EditorGUI.BeginChangeCheck();

            bool bSessionStarted = (syncData != null && syncData.SyncStatus != HEU_SessionSyncData.Status.Stopped);
            bool bSessionCanStart = !bSessionStarted;

            if (bSessionCanStart)
            {
                // Only able to start a session if no session exists.
                HEU_SessionBase session = HEU_SessionManager.GetDefaultSession();
                if (session != null && session.IsSessionValid())
                {
                    bSessionCanStart = false;
                }
            }

            HEU_HoudiniAssetUI.DrawHeaderSection();

            // Draw SessionSync status.
            if (syncData != null)
            {
                if (syncData.SyncStatus == HEU_SessionSyncData.Status.Stopped)
                {
                    if (!bSessionCanStart)
                    {
                        EditorGUILayout.LabelField(
                            "Another session already running. Disconnect it to start SessionSync.");
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Status: " + syncData.SyncStatus);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Status: " + syncData.SyncStatus);
                }
            }
            else
            {
                if (!bSessionCanStart)
                {
                    EditorGUILayout.LabelField("Another session already running. Disconnect it to start SessionSync.");
                }
                else
                {
                    EditorGUILayout.LabelField("No active session.");
                }
            }

            EditorGUILayout.Separator();

            EditorGUI.indentLevel++;

            // Draw initial connection buttons (Start, Connect)
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(bSessionStarted || !bSessionCanStart))
                {
                    if (GUILayout.Button("Start Houdini"))
                    {
                        StartAndConnectToHoudini(syncData);
                    }
                    else if (GUILayout.Button("Connect to Houdini"))
                    {
                        ConnectSessionSync(syncData);
                    }
                }
            }

            using (new EditorGUI.DisabledScope((syncData == null || !bSessionStarted) && bSessionCanStart))
            {
                if (GUILayout.Button("Disconnect"))
                {
                    Disconnect(syncData);
                }
            }

            EditorGUILayout.Separator();

            // Draw Connection Settings
            EditorGUILayout.LabelField("Connection Settings");

            using (new EditorGUI.DisabledScope(bSessionStarted))
            {
                SessionMode newSessionMode = (SessionMode)EditorGUILayout.EnumPopup("Type", _sessionMode);
                if (_sessionMode != newSessionMode)
                {
                    _sessionMode = newSessionMode;
                    HEU_PluginSettings.Session_Mode = newSessionMode;
                }

                EditorGUI.indentLevel++;
                if (_sessionMode == SessionMode.Pipe)
                {
                    string newPipeName = EditorGUILayout.DelayedTextField("Pipe Name", _pipeName);
                    if (_pipeName != newPipeName)
                    {
                        HEU_PluginSettings.Session_PipeName = newPipeName;
                        _pipeName = newPipeName;
                    }
                }
                else if (_sessionMode == SessionMode.Socket)
                {
                    int newPort = EditorGUILayout.DelayedIntField("Port", _port);
                    HEU_PluginSettings.Session_Port = newPort;
                    if (_port != newPort)
                    {
                        HEU_PluginSettings.Session_Port = newPort;
                        _port = newPort;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();

            // The rest requires syncData

            // Synchronization settings, and new nodes
            if (syncData != null)
            {
                using (new EditorGUI.DisabledScope(syncData.SyncStatus != HEU_SessionSyncData.Status.Connected))
                {
                    EditorGUILayout.LabelField("Synchronization Settings");

                    EditorGUI.indentLevel++;

                    HEU_PluginSettings.SessionSyncAutoCook =
                        HEU_EditorUI.DrawToggleLeft(HEU_PluginSettings.SessionSyncAutoCook, "Sync With Houdini Cook");

                    bool enableHoudiniTime = HEU_EditorUI.DrawToggleLeft(syncData._syncInfo.cookUsingHoudiniTime,
                        "Cook Using Houdini Time");
                    if (syncData._syncInfo.cookUsingHoudiniTime != enableHoudiniTime)
                    {
                        syncData._syncInfo.cookUsingHoudiniTime = enableHoudiniTime;
                        UploadSessionSyncInfo(null, syncData);
                    }

                    bool enableSyncViewport =
                        HEU_EditorUI.DrawToggleLeft(syncData._syncInfo.syncViewport, "Sync Viewport");
                    if (syncData._syncInfo.syncViewport != enableSyncViewport)
                    {
                        syncData._syncInfo.syncViewport = enableSyncViewport;
                        UploadSessionSyncInfo(null, syncData);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("New Node");

                using (new EditorGUI.DisabledScope(syncData.SyncStatus != HEU_SessionSyncData.Status.Connected))
                {
                    EditorGUI.indentLevel++;

                    syncData._newNodeName = EditorGUILayout.TextField("Name", syncData._newNodeName);

                    syncData._nodeTypeIndex = EditorGUILayout.Popup("Type", syncData._nodeTypeIndex, _nodeTypesLabels);

                    using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(syncData._newNodeName)))
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            if (GUILayout.Button("Create"))
                            {
                                if (syncData._nodeTypeIndex >= 0 && syncData._nodeTypeIndex < 3)
                                {
                                    HEU_NodeSync.CreateNodeSync(null, _nodeTypes[syncData._nodeTypeIndex],
                                        syncData._newNodeName);
                                }
                                else if (syncData._nodeTypeIndex == 3)
                                {
                                    CreateCurve(syncData._newNodeName);
                                }
                                else if (syncData._nodeTypeIndex == 4)
                                {
                                    CreateInput(syncData._newNodeName);
                                }
                            }

                            if (GUILayout.Button("Load NodeSync"))
                            {
                                LoadNodeSyncDialog(syncData._newNodeName);
                            }
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Separator();

                if (_outputLogUI != null)
                {
                    _outputLogUI.OnGUI(GetLog());
                }
            }

            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck() && syncData != null)
            {
                HEU_SessionBase sessionBase = HEU_SessionManager.GetDefaultSession();
                if (sessionBase != null)
                {
                    HEU_SessionManager.SaveAllSessionData();
                }
            }
        }

        /// <summary>
        /// Connect to a running instance of Houdini with SessionSync enabled.
        /// </summary>
        private void ConnectSessionSync(HEU_SessionSyncData syncData)
        {
            if (syncData != null && syncData.SyncStatus != HEU_SessionSyncData.Status.Stopped)
            {
                return;
            }

            Log("Connecting To Houdini...");

            HEU_SessionManager.RecreateDefaultSessionData();

            if (syncData == null)
            {
                HEU_SessionData sessionData = HEU_SessionManager.GetSessionData();
                if (sessionData != null)
                {
                    syncData = sessionData.GetOrCreateSessionSync();
                }
                else
                {
                    syncData = new HEU_SessionSyncData();
                }
            }

            bool result = InternalConnect(_sessionMode, _pipeName,
                HEU_PluginSettings.Session_Localhost, _port,
                HEU_PluginSettings.Session_AutoClose,
                HEU_PluginSettings.Session_Timeout,
                true);

            if (result)
            {
                try
                {
                    HEU_SessionManager.InitializeDefaultSession();

                    HEU_SessionManager.GetDefaultSession().GetSessionData().SetSessionSync(syncData);

                    syncData.SyncStatus = HEU_SessionSyncData.Status.Connected;
                    Log("Connected!");
                }
                catch (HEU_HoudiniEngineError ex)
                {
                    syncData.SyncStatus = HEU_SessionSyncData.Status.Stopped;

                    Log("Connection errored!");
                    Log(ex.ToString());
                }
            }
            else
            {
                Log("Connection failed!");
            }
        }

        /// <summary>
        /// Helper to connect to a running instance of Houdini with SessionSync enabled.
        /// </summary>
        private bool InternalConnect(
            SessionMode sessionType, string pipeName,
            string ip, int port, bool autoClose, float timeout,
            bool logError)
        {
            if (sessionType == SessionMode.Pipe)
            {
                return HEU_SessionManager.ConnectSessionSyncUsingThriftPipe(
                    pipeName,
                    autoClose,
                    timeout,
                    logError);
            }
            else
            {
                return HEU_SessionManager.ConnectSessionSyncUsingThriftSocket(
                    ip,
                    port,
                    autoClose,
                    timeout,
                    logError);
            }
        }

        /// <summary>
        /// Disconnect from SessionSync and close session.
        /// </summary>
        private void Disconnect(HEU_SessionSyncData syncData)
        {
            if (syncData != null)
            {
                syncData.SyncStatus = HEU_SessionSyncData.Status.Stopped;

                // Store the sync info as it gets cleared in the session below
                _connectionSyncData = syncData;
            }

            if (HEU_SessionManager.CloseDefaultSession())
            {
                Log("Connection closed!");
            }
            else
            {
                Log("Failed to close session! ");
            }
        }

        /// <summary>
        /// Launch Houdini with SessionSync enabled and return true if successful.
        /// </summary>
        private bool OpenHoudini()
        {
            string args = "";

            // Form argument
            if (_sessionMode == SessionMode.Pipe)
            {
                args = string.Format("-hess=pipe:{0}", _pipeName);
            }
            else
            {
                args = string.Format("-hess=port:{0}", _port);
            }

            Log("Opening Houdini...");

            if (!HEU_SessionManager.OpenHoudini(args))
            {
                Log("Failed to start Houdini!");
                return false;
            }

            Log("Houdini started!");

            return true;
        }

        /// <summary>
        /// Launch Houdini with SessionSync enabled and automatically connect to it.
        /// </summary>
        private void StartAndConnectToHoudini(HEU_SessionSyncData syncData)
        {
            if (syncData != null && syncData.SyncStatus != HEU_SessionSyncData.Status.Stopped)
            {
                return;
            }

            if (!OpenHoudini())
            {
                return;
            }

            // Now attempt to connect to it by moving into Connecting state

            HEU_SessionManager.RecreateDefaultSessionData();

            if (syncData == null)
            {
                HEU_SessionData sessionData = HEU_SessionManager.GetSessionData();
                if (sessionData != null)
                {
                    syncData = sessionData.GetOrCreateSessionSync();
                }
                else
                {
                    syncData = new HEU_SessionSyncData();
                }

                syncData._validForConnection = true;
            }

            syncData.SyncStatus = HEU_SessionSyncData.Status.Connecting;

            _connectionSyncData = syncData;
            Log("Connecting...");

            syncData._timeStartConnection = Time.realtimeSinceStartup;
            syncData._timeLastUpdate = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Callback to update the local SessionSync state.
        /// </summary>
        private void UpdateSync()
        {
            HEU_SessionSyncData syncData = GetSessionSyncData();

            if (syncData != null)
            {
                if (syncData.SyncStatus == HEU_SessionSyncData.Status.Connecting)
                {
                    UpdateConnecting(syncData);
                }
                else if (syncData.SyncStatus == HEU_SessionSyncData.Status.Connected)
                {
                    UpdateConnected(syncData);
                }
            }
        }

        /// <summary>
        /// Attempts to connect to running instance of Houdini with SessionSync enabled,
        /// with CONNECTION_ATTEMPT_RATE delay between attempts. Presuming that Houdini was just,
        /// launched this might take a few tries. Times out if unsuccessful after CONNECTION_TIME_OUT
        /// time.
        /// </summary>
        private void UpdateConnecting(HEU_SessionSyncData syncData)
        {
            if (syncData == null || syncData.SyncStatus != HEU_SessionSyncData.Status.Connecting)
            {
                return;
            }

            // Attempt connection after waiting for a bit.
            if (Time.realtimeSinceStartup - syncData._timeLastUpdate >= CONNECTION_ATTEMPT_RATE)
            {
                if (InternalConnect(_sessionMode, _pipeName,
                        HEU_PluginSettings.Session_Localhost, _port,
                        HEU_PluginSettings.Session_AutoClose,
                        HEU_PluginSettings.Session_Timeout, false))
                {
                    Log("Initializing...");
                    syncData.SyncStatus = HEU_SessionSyncData.Status.Initializing;

                    try
                    {
                        HEU_SessionManager.InitializeDefaultSession();
                        HEU_SessionManager.GetDefaultSession().GetSessionData().SetSessionSync(syncData);

                        syncData.SyncStatus = HEU_SessionSyncData.Status.Connected;
                        Log("Connected!");
                    }
                    catch (System.Exception ex)
                    {
                        syncData.SyncStatus = HEU_SessionSyncData.Status.Stopped;
                        Log("Connection errored!");
                        Log(ex.ToString());

                        HEU_Logger.Log(ex.ToString());
                    }
                    finally
                    {
                        // Clear this to get out of the connection state
                        _connectionSyncData = null;
                    }
                }
                else if (Time.realtimeSinceStartup - syncData._timeStartConnection >= CONNECTION_TIME_OUT)
                {
                    syncData.SyncStatus = HEU_SessionSyncData.Status.Stopped;
                    Log("Timed out trying to connect to Houdini."
                        + "\nCheck if Houdini is running and SessionSync is enabled."
                        + "\nCheck port or pipe name are correct by comparing with Houdini SessionSync panel.");
                }
                else
                {
                    // Try again in a bit
                    syncData._timeLastUpdate = Time.realtimeSinceStartup;
                }
            }
        }

        /// <summary>
        /// Update the local SessionSync state while connected to Houdini.
        /// Synchronizes viewport if enabled.
        /// Disconnects if Houdini Engine session is not valid.
        /// </summary>
        private void UpdateConnected(HEU_SessionSyncData syncData)
        {
            if (!HEU_PluginSettings.SessionSyncAutoCook)
            {
                return;
            }

            HEU_SessionBase session = HEU_SessionManager.GetDefaultSession();
            if (session == null || !session.IsSessionValid() || !session.IsSessionSync())
            {
                return;
            }

            if (session.ConnectionState == SessionConnectionState.CONNECTED)
            {
                // Get latest SessionSync info from Houdini Engine to synchronize
                // local state.
                DownloadSessionSyncInfo(null, syncData);

                // Use the above call to check validity of the session.
                // Note that once HAPI_IsSessionValid is improved, we might just use that.
                if (session.LastCallResultCode == HAPI_Result.HAPI_RESULT_INVALID_SESSION)
                {
                    // Bad session
                    Log("Session is invalid. Disconnecting.");
                    Disconnect(syncData);
                    return;
                }

                if (syncData._syncInfo.syncViewport)
                {
                    UpdateViewport(session, syncData);
                }
            }
            else
            {
                if (syncData.SyncStatus == HEU_SessionSyncData.Status.Connected)
                {
                    // Bad session
                    Log("Session is invalid. Disconnecting.");
                    Disconnect(syncData);
                }
            }
        }

        /// <summary>
        /// Download the latest HAPI_SessionSyncInfo from Houdini Engine
        /// to update the local state.
        /// </summary>
        private void DownloadSessionSyncInfo(HEU_SessionBase session, HEU_SessionSyncData syncData)
        {
            if (session == null)
            {
                session = HEU_SessionManager.GetDefaultSession();
                if (session == null || !session.IsSessionValid())
                {
                    return;
                }
            }

            HAPI_SessionSyncInfo syncInfo = new HAPI_SessionSyncInfo();
            if (session.GetSessionSyncInfo(ref syncInfo))
            {
                if (HEU_HAPIUtility.IsSessionSyncEqual(ref syncInfo, ref syncData._syncInfo))
                {
                    Repaint();
                }

                syncData._syncInfo = syncInfo;
            }
        }

        /// <summary>
        /// Upload the local HAPI_SessionSyncInfo to Houdini Engine.
        /// </summary>
        private void UploadSessionSyncInfo(HEU_SessionBase session, HEU_SessionSyncData syncData)
        {
            if (session == null)
            {
                session = HEU_SessionManager.GetDefaultSession();
                if (session == null || !session.IsSessionValid())
                {
                    return;
                }
            }

            session.SetSessionSyncInfo(ref syncData._syncInfo);
        }

        /// <summary>
        /// Synchronize the viewport between HAPI and Unity.
        /// </summary>
        private void UpdateViewport(HEU_SessionBase session, HEU_SessionSyncData syncData)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                return;
            }

            // Get the latest viewport from HAPI, and check it agianst last update.
            HAPI_Viewport viewHAPI = new HAPI_Viewport(true);
            session.GetViewport(ref viewHAPI);

            if (!HEU_HAPIUtility.IsViewportEqual(ref viewHAPI, ref syncData._viewportHAPI))
            {
                // HAPI has changed. Update local viewport.

                Transform target = sceneView.camera.transform;

                // Account for left-handed coordinate system
                Vector3 pivot = new Vector3(-viewHAPI.position[0], viewHAPI.position[1], viewHAPI.position[2]);

                Quaternion rotation = new Quaternion(viewHAPI.rotationQuaternion[0],
                    viewHAPI.rotationQuaternion[1], viewHAPI.rotationQuaternion[2],
                    viewHAPI.rotationQuaternion[3]);
                Vector3 euler = rotation.eulerAngles;
                euler.y = -euler.y;
                euler.z = -euler.z;
                // Flip the camera direction for Unity camera
                rotation = Quaternion.Euler(euler) * Quaternion.Euler(0, 180f, 0);

                // TODO: use viewHAPI.offset to set camera distance
                // Unfortuantely no direct API to set the camera distance in Unity

                sceneView.LookAtDirect(pivot, rotation);
                sceneView.Repaint();

                // Store HAPI viewport for comparison on next update
                syncData._viewportHAPI = viewHAPI;
                syncData._viewportLocal = viewHAPI;
                syncData._viewportJustUpdated = true;
            }
            else
            {
                // HAPI hasn't changed, so let's see if local viewport has

                Vector3 pivot = sceneView.pivot;
                Quaternion rotation = sceneView.rotation;
                float localDistance = sceneView.cameraDistance;

                // Generate the local HAPI_Viewport
                HAPI_Viewport viewLocal = new HAPI_Viewport(true);

                // Account for left-handed coordinate system
                viewLocal.position[0] = -pivot.x;
                viewLocal.position[1] = pivot.y;
                viewLocal.position[2] = pivot.z;

                // Flip the camera direction for Unity camera
                rotation = rotation * Quaternion.Euler(0, 180f, 0);
                Vector3 euler = rotation.eulerAngles;
                euler.y = -euler.y;
                euler.z = -euler.z;
                rotation = Quaternion.Euler(euler);

                viewLocal.rotationQuaternion[0] = rotation.x;
                viewLocal.rotationQuaternion[1] = rotation.y;
                viewLocal.rotationQuaternion[2] = rotation.z;
                viewLocal.rotationQuaternion[3] = rotation.w;

                viewLocal.offset = syncData._viewportHAPI.offset;

                if (!HEU_HAPIUtility.IsViewportEqual(ref viewLocal, ref syncData._viewportLocal))
                {
                    // Always store local viewport for comparison on next update
                    syncData._viewportLocal = viewLocal;

                    if (syncData._viewportJustUpdated)
                    {
                        // Unity's SceneView internally updates the
                        // viewport after setting it, so this makes sure
                        // to update and store the latest change locally,
                        // and skip sending it to HAPI
                        syncData._viewportJustUpdated = false;
                    }
                    else
                    {
                        session.SetViewport(ref viewLocal);

                        // Store HAPI viewport for comparison on next update
                        syncData._viewportHAPI = viewLocal;
                    }

                    //HEU_Logger.Log("Setting HAPI (from local)");
                    //HEU_Logger.LogFormat("Pos: {0}, {1}, {2}", viewLocal.position[0], viewLocal.position[1], viewLocal.position[2]);
                    //HEU_Logger.LogFormat("Rot: {0}, {1}, {2}, {3}", viewLocal.rotationQuaternion[0], 
                    //viewLocal.rotationQuaternion[1], viewLocal.rotationQuaternion[2], viewLocal.rotationQuaternion[3]);
                    //HEU_Logger.LogFormat("Dis: {0}, sceneView.camDist: {1}", viewLocal.offset, sceneView.cameraDistance);
                }
            }
        }

        /// <summary>
        /// Returns the local HEU_SessionSyncData state.
        /// </summary>
        private HEU_SessionSyncData GetSessionSyncData()
        {
            HEU_SessionSyncData syncData = _connectionSyncData;
            if (syncData == null)
            {
                HEU_SessionData sessionData = HEU_SessionManager.GetSessionData();
                if (sessionData != null)
                {
                    // On domain reload, re-acquire serialized SessionSync
                    // if session exists 
                    syncData = sessionData.GetOrCreateSessionSync();
                }
            }

            return syncData;
        }

        /// <summary>
        /// Create a new Curve SOP with given name.
        /// </summary>
        private void CreateCurve(string name)
        {
            GameObject newCurveGO = HEU_HAPIUtility.CreateNewCurveAsset(name: name);
            if (newCurveGO != null)
            {
                HEU_Curve.PreferredNextInteractionMode = HEU_Curve.Interaction.ADD;
                HEU_EditorUtility.SelectObject(newCurveGO);
            }
        }

        /// <summary>
        /// Create a new Input SOP with given name.
        /// </summary>
        private void CreateInput(string name)
        {
            GameObject newCurveGO = HEU_HAPIUtility.CreateNewInputAsset(name: name);
            if (newCurveGO != null)
            {
                HEU_EditorUtility.SelectObject(newCurveGO);
            }
        }

        /// <summary>
        /// Setup this window's UI.
        /// </summary>
        private void SetupUI()
        {
            _eventMessageContent = new GUIContent("Log", "Status messages logged here.");
            if (_outputLogUI == null)
            {
                _outputLogUI = new HEU_OutputLogUIComponent(_eventMessageContent, ClearLog);
            }

            _outputLogUI.SetupUI();
        }

        /// <summary>
        /// Show the Load NodeSync dialog for loading a NodeSync saved file.
        /// </summary>
        private void LoadNodeSyncDialog(string name)
        {
            string fileName = "untitled.hess";
            string filePattern = "hess";
            string newPath = EditorUtility.OpenFilePanel("Load Node Sync", fileName + "." + filePattern, filePattern);
            if (newPath != null && !string.IsNullOrEmpty(newPath))
            {
                CreateNodeSyncFromFile(newPath, name);
            }
        }

        /// <summary>
        /// Load a NodeSync file and create its construct in Unity.
        /// </summary>
        /// <param name="filePath">Path to the NodeSync file</param>
        /// <param name="name">Name of the NodeSync node</param>
        private void CreateNodeSyncFromFile(string filePath, string name)
        {
            HEU_SessionBase session = HEU_SessionManager.GetDefaultSession();
            if (session == null || !session.IsSessionValid())
            {
                return;
            }

            HAPI_NodeId parentNodeID = -1;
            string nodeName = name;
            HAPI_NodeId newNodeID = -1;

            // This loads the node network from file, and returns the node that was created
            // with newNodeID. It is either a SOP object, or a subnet object.
            // The actual loader (HEU_ThreadedTaskLoadGeo) will deal with either case.
            if (!session.LoadNodeFromFile(filePath, parentNodeID, nodeName, true, out newNodeID))
            {
                Log(string.Format("Failed to load node network from file: {0}.", filePath));
                return;
            }

            // Wait until finished
            if (!HEU_HAPIUtility.ProcessHoudiniCookStatus(session, nodeName))
            {
                Log(string.Format("Failed to cook loaded node with name: {0}.", nodeName));
                return;
            }

            GameObject newGO = HEU_GeneralUtility.CreateNewGameObject(nodeName);

            HEU_NodeSync nodeSync = newGO.AddComponent<HEU_NodeSync>();
            nodeSync.InitializeFromHoudini(session, newNodeID, nodeName, filePath);
        }

        /// <summary>
        /// Add given msg to log
        /// </summary>
        public void Log(string msg)
        {
            lock (_log)
            {
                _log.AppendLine(msg);
            }
        }

        /// <summary>
        /// Returns the log
        /// </summary>
        public string GetLog()
        {
            lock (_log)
            {
                return _log.ToString();
            }
        }

        /// <summary>
        /// Clear the log
        /// </summary>
        public void ClearLog()
        {
            lock (_log)
            {
                _log.Length = 0;
            }
        }

        // DATA ---------------------------------------------------------------

        private GUIContent _eventMessageContent;

        // Sync data while connecting or if session is disconnected.
        [SerializeField] private HEU_SessionSyncData _connectionSyncData;

        // Session protocol (pipe or socket)
        public SessionMode _sessionMode = SessionMode.Socket;

        // Socket port
        public int _port = 0;

        // Pipe name
        public string _pipeName = "";

        // Seconds between connection attempts while Houdini launches
        private const float CONNECTION_ATTEMPT_RATE = 5f;

        // Maximum seconds to wait before timing out while connecting
        private const float CONNECTION_TIME_OUT = 60f;

        // UI log
        [SerializeField] private StringBuilder _log = new StringBuilder();

        // Operator names for creating new nodes
        private string[] _nodeTypes =
        {
            "SOP/output",
            "Object/subnet",
            "SOP/subnet",
            "curve",
            "input"
        };

        // Labels for the operator names above
        private string[] _nodeTypesLabels =
        {
            "Experimental/Object/Geometry",
            "Experimental/Object/Subnet",
            "Experimental/SOP/Subnet",
            "Curve",
            "Input"
        };

        private HEU_OutputLogUIComponent _outputLogUI = null;
    }
}
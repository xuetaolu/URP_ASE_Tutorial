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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HoudiniEngineUnity
{
    [CustomEditor(typeof(HEU_NodeSync))]
    public class HEU_NodeSyncUI : Editor
    {
        #region FUNCTIONS

        private void OnEnable()
        {
            AcquireTarget();
        }

        private void AcquireTarget()
        {
            _nodeSync = target as HEU_NodeSync;
        }

        public override void OnInspectorGUI()
        {
            if (_nodeSync == null)
            {
                AcquireTarget();
            }

            using (new EditorGUILayout.VerticalScope())
            {
                //EditorGUILayout.TextField("Node name", _nodeSync._nodeName);
                ReadOnlyTextField("Node name", _nodeSync._nodeName);
                ReadOnlyTextField("Node ID", _nodeSync._cookNodeID.ToString());

                ReadOnlyTextField("File", _nodeSync._nodeSaveFilePath);

                if (GUILayout.Button("Save Node"))
                {
                    //_nodeSync._nodeSaveFilePath = EditorUtility.SaveFilePanel("Save Node", _nodeSync._nodeSaveFilePath, _nodeSync._nodeName, ".hess");

                    string fileName = _nodeSync._nodeName;
                    string filePattern = "hess";
                    string newPath =
                        EditorUtility.SaveFilePanel("Save Node", "", fileName + "." + filePattern, filePattern);
                    if (newPath != null && !string.IsNullOrEmpty(newPath))
                    {
                        _nodeSync.SaveNodeToFile(newPath);
                    }
                }

                if (_nodeSync._syncing)
                {
                    EditorGUILayout.LabelField(_statusSyncContent);

                    if (GUILayout.Button(_stopContent))
                    {
                        _nodeSync.StopSync();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(_statusIdleContent);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool bLoaded = _nodeSync.IsLoaded();

                        if (GUILayout.Button(_syncContent))
                        {
                            _nodeSync.Resync();
                        }
                    }
                }
            }
        }

        public static void ReadOnlyTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(text, EditorStyles.textField,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion


        #region DATA

        private HEU_NodeSync _nodeSync;

        private GUIContent _syncContent = new GUIContent("Resync", "Recook and regenerate geometry.");

        private GUIContent _stopContent = new GUIContent("Stop", "Stop the loading.");

        private GUIContent _statusIdleContent = new GUIContent("Idle");

        private GUIContent _statusSyncContent = new GUIContent("Syncing");

        private GUIContent _unloadContent =
            new GUIContent("Unload", "Delete the file node and clean up all generated content.");

        #endregion
    }
} // namespace HoudiniEngineUnity
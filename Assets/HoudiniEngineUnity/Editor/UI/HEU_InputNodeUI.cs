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

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace HoudiniEngineUnity
{
    /// <summary>
    /// Editor drawing logic for input nodes.
    /// </summary>
    public static class HEU_InputNodeUI
    {
        private static GUIContent _meshExportCollidersContent =
            new GUIContent("Export colliders", "If checked, will export colliders on the object.");

        private static GUIContent _tilemapCreateGroupsContent = new GUIContent("Create Groups for Tiles",
            "If checked, will create a point group for each kind of tile using its tile name. If unchecked, will create a point string attribute instead.");

        private static GUIContent _tilemapExportUnusedTilesContent =
            new GUIContent("Keep Unused Tiles", "If checked, will create a point for an empty tile");

        private static GUIContent _tilemapColorContent =
            new GUIContent("Apply Tile color", "If checked, will output a Cd color attribute to point.");

        private static GUIContent _tilemapOrientationContent = new GUIContent("Apply Tilemap Orientation",
            "If checked, will offset position by the tilemap position offset, and produce orient/pscale attributes to the points.");

        private static GUIContent _samplingResolutionContent = new GUIContent("Unity Spline Resolution",
            "Resolution used when marshalling Unity Splines to Houdini Engine (step in m between control points). Set this to 0 to only export the control points.");

        /// <summary>
        /// Populate the UI cache for the given input node
        /// </summary>
        /// <param name="inputNode"></param>
        public static void PopulateCache(HEU_InputNode inputNode)
        {
            if (inputNode._uiCache == null)
            {
                inputNode._uiCache = new HEU_InputNodeUICache();

                inputNode._uiCache._inputNodeSerializedObject = new SerializedObject(inputNode);

                inputNode._uiCache._inputObjectTypeProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_inputObjectType");
                inputNode._uiCache._keepWorldTransformProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_keepWorldTransform");
                inputNode._uiCache._packBeforeMergeProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_packGeometryBeforeMerging");

                inputNode._uiCache._inputObjectsProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_inputObjects");

                inputNode._uiCache._meshSettingsProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_meshSettings");
                inputNode._uiCache._tilemapSettingsProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_tilemapSettings");

                inputNode._uiCache._splineSettingsProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_splineSettings");

                int inputCount = inputNode._uiCache._inputObjectsProperty.arraySize;
                for (int i = 0; i < inputCount; ++i)
                {
                    SerializedProperty inputObjectProperty =
                        inputNode._uiCache._inputObjectsProperty.GetArrayElementAtIndex(i);

                    HEU_InputNodeUICache.HEU_InputObjectUICache objectCache =
                        new HEU_InputNodeUICache.HEU_InputObjectUICache();

                    objectCache._gameObjectProperty = inputObjectProperty.FindPropertyRelative("_gameObject");

                    objectCache._transformOffsetProperty =
                        inputObjectProperty.FindPropertyRelative("_useTransformOffset");

                    objectCache._translateProperty = inputObjectProperty.FindPropertyRelative("_translateOffset");
                    objectCache._rotateProperty = inputObjectProperty.FindPropertyRelative("_rotateOffset");
                    objectCache._scaleProperty = inputObjectProperty.FindPropertyRelative("_scaleOffset");


                    inputNode._uiCache._inputObjectCache.Add(objectCache);
                }

                inputNode._uiCache._inputAssetsProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_inputAssetInfos");

                inputCount = inputNode._uiCache._inputAssetsProperty.arraySize;
                for (int i = 0; i < inputCount; ++i)
                {
                    SerializedProperty inputAssetProperty =
                        inputNode._uiCache._inputAssetsProperty.GetArrayElementAtIndex(i);

                    HEU_InputNodeUICache.HEU_InputAssetUICache assetInfoCache =
                        new HEU_InputNodeUICache.HEU_InputAssetUICache();

                    assetInfoCache._gameObjectProperty = inputAssetProperty.FindPropertyRelative("_pendingGO");

                    inputNode._uiCache._inputAssetCache.Add(assetInfoCache);
                }
            }
        }

        /// <summary>
        /// Draw the UI for the given input node
        /// </summary>
        /// <param name="inputNode"></param>
        public static void EditorDrawInputNode(HEU_InputNode inputNode)
        {
            int plusButtonWidth = 20;

            const string inputTypeTooltip = @"Input type of the object. 

The HDA type can accept any object with a HEU_HoudiniAssetRoot component. (Including curves)

The UNITY_MESH type can accept any GameObject (Including Terrain, HEU_BoundingVolumes).";
            GUIContent inputTypeLabel = new GUIContent("Input Type", inputTypeTooltip);

            GUIContent translateLabel = new GUIContent("    Translate");
            GUIContent rotateLabel = new GUIContent("    Rotate");
            GUIContent scaleLabel = new GUIContent("    Scale");

            PopulateCache(inputNode);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string labelName = inputNode.LabelName;
            if (!string.IsNullOrEmpty(labelName))
            {
                EditorGUILayout.LabelField(labelName);
            }

            EditorGUI.indentLevel++;

            HEU_InputNode.InputObjectType inputObjectType =
                (HEU_InputNode.InputObjectType)inputNode._uiCache._inputObjectTypeProperty.intValue;
            HEU_InputNode.InputObjectType userSelectedInputObjectType =
                (HEU_InputNode.InputObjectType)EditorGUILayout.EnumPopup(inputTypeLabel, inputObjectType);
            if (userSelectedInputObjectType != inputObjectType)
            {
                SerializedProperty pendingInputObjectTypeProperty =
                    HEU_EditorUtility.GetSerializedProperty(inputNode._uiCache._inputNodeSerializedObject,
                        "_pendingInputObjectType");
                if (pendingInputObjectTypeProperty != null)
                {
                    pendingInputObjectTypeProperty.intValue = (int)userSelectedInputObjectType;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(inputNode._uiCache._keepWorldTransformProperty);
                EditorGUILayout.PropertyField(inputNode._uiCache._packBeforeMergeProperty);

                if (HEU_InputNode.GetInternalObjectType(inputObjectType) == HEU_InputNode.InternalObjectType.HDA)
                {
                    SerializedProperty inputAssetsProperty = inputNode._uiCache._inputAssetsProperty;
                    if (inputAssetsProperty != null)
                    {
                        int inputCount = inputAssetsProperty.arraySize;
                        bool bSkipElements = false;

                        HEU_EditorUI.DrawSeparator();

                        EditorGUILayout.LabelField(string.Format("{0} input objects", inputCount));

                        using (var hs1 = new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Add Slot"))
                            {
                                inputAssetsProperty.InsertArrayElementAtIndex(inputCount);
                                bSkipElements = true;
                            }

                            if (GUILayout.Button("Clear"))
                            {
                                inputAssetsProperty.ClearArray();
                                bSkipElements = true;
                            }
                        }

                        DrawSelectionWindow(HEU_InputNode.InputObjectType.HDA, inputNode);

                        if (!bSkipElements)
                        {
                            using (var vs1 = new EditorGUILayout.VerticalScope())
                            {
                                for (int i = 0; i < inputCount; ++i)
                                {
                                    using (var hs2 = new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField("Input " + (i + 1));

                                        if (GUILayout.Button("+", GUILayout.Width(plusButtonWidth)))
                                        {
                                            inputAssetsProperty.InsertArrayElementAtIndex(i);
                                            break;
                                        }

                                        if (GUILayout.Button("-", GUILayout.Width(plusButtonWidth)))
                                        {
                                            inputAssetsProperty.DeleteArrayElementAtIndex(i);
                                            break;
                                        }
                                    }

                                    EditorGUI.indentLevel++;
                                    using (var vs4 = new EditorGUILayout.VerticalScope())
                                    {
                                        if (i < inputNode._uiCache._inputAssetCache.Count)
                                        {
                                            HEU_InputNodeUICache.HEU_InputAssetUICache assetCache =
                                                inputNode._uiCache._inputAssetCache[i];

                                            UnityEngine.Object setObject =
                                                EditorGUILayout.ObjectField(
                                                    assetCache._gameObjectProperty.objectReferenceValue,
                                                    typeof(HEU_HoudiniAssetRoot), true);
                                            if (setObject != assetCache._gameObjectProperty.objectReferenceValue)
                                            {
                                                GameObject inputGO = setObject != null
                                                    ? (setObject as HEU_HoudiniAssetRoot).gameObject
                                                    : null;
                                                // Check not setting same asset as self
                                                if (inputGO == null || inputGO != inputNode.ParentAsset.RootGameObject)
                                                {
                                                    assetCache._gameObjectProperty.objectReferenceValue = inputGO;
                                                }
                                            }
                                        }
                                    }

                                    EditorGUI.indentLevel--;
                                }
                            }
                        }
                    }
                }
                else if (HEU_InputNode.GetInternalObjectType(inputObjectType) ==
                         HEU_InputNode.InternalObjectType.UNITY_MESH)
                {
                    SerializedProperty inputObjectsProperty = inputNode._uiCache._inputObjectsProperty;
                    if (inputObjectsProperty != null)
                    {
                        bool bSkipElements = false;

                        HEU_EditorUI.DrawSeparator();

                        EditorGUILayout.LabelField(string.Format("{0} input objects", inputObjectsProperty.arraySize));

                        using (var hs1 = new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Add Slot"))
                            {
                                inputObjectsProperty.arraySize++;
                                FixUpScaleProperty(inputObjectsProperty, inputObjectsProperty.arraySize - 1);
                                bSkipElements = true;
                            }

                            if (GUILayout.Button("Clear"))
                            {
                                inputObjectsProperty.ClearArray();
                                bSkipElements = true;
                            }
                        }

                        DrawSelectionWindow(inputObjectType, inputNode);

                        if (inputObjectType == HEU_InputNode.InputObjectType.UNITY_MESH &&
                            inputNode.MeshSettings != null)
                        {
                            HEU_EditorUI.DrawHeadingLabel("Mesh settings");
                            EditorGUI.indentLevel++;
                            {
                                UnityEditor.SerializedProperty exportCollidersProperty =
                                    inputNode._uiCache._meshSettingsProperty.FindPropertyRelative("_exportColliders");

                                exportCollidersProperty.boolValue = HEU_EditorUI.DrawToggleLeft(
                                    exportCollidersProperty.boolValue, _meshExportCollidersContent.text,
                                    _meshExportCollidersContent.tooltip);
                            }
                            EditorGUI.indentLevel--;
                        }
                        else if (inputObjectType == HEU_InputNode.InputObjectType.TILEMAP &&
                                 inputNode.TilemapSettings != null)
                        {
                            HEU_EditorUI.DrawHeadingLabel("Tilemap settings");
                            EditorGUI.indentLevel++;
                            {
                                UnityEditor.SerializedProperty createGroupsForTilesProperty =
                                    inputNode._uiCache._tilemapSettingsProperty.FindPropertyRelative(
                                        "_createGroupsForTiles");
                                UnityEditor.SerializedProperty exportUnusedTilesProperty =
                                    inputNode._uiCache._tilemapSettingsProperty.FindPropertyRelative(
                                        "_exportUnusedTiles");
                                UnityEditor.SerializedProperty applyTileColorProperty =
                                    inputNode._uiCache._tilemapSettingsProperty.FindPropertyRelative("_applyTileColor");
                                UnityEditor.SerializedProperty applyTilemapOrientationProperty =
                                    inputNode._uiCache._tilemapSettingsProperty.FindPropertyRelative(
                                        "_applyTilemapOrientation");

                                createGroupsForTilesProperty.boolValue = HEU_EditorUI.DrawToggleLeft(
                                    createGroupsForTilesProperty.boolValue, _tilemapCreateGroupsContent.text,
                                    _tilemapCreateGroupsContent.tooltip);
                                exportUnusedTilesProperty.boolValue = HEU_EditorUI.DrawToggleLeft(
                                    exportUnusedTilesProperty.boolValue, _tilemapExportUnusedTilesContent.text,
                                    _tilemapExportUnusedTilesContent.tooltip);
                                applyTileColorProperty.boolValue = HEU_EditorUI.DrawToggleLeft(
                                    applyTileColorProperty.boolValue, _tilemapColorContent.text,
                                    _tilemapColorContent.tooltip);
                                applyTilemapOrientationProperty.boolValue = HEU_EditorUI.DrawToggleLeft(
                                    applyTilemapOrientationProperty.boolValue, _tilemapOrientationContent.text,
                                    _tilemapOrientationContent.tooltip);
                            }
                            EditorGUI.indentLevel--;
                        }
#if UNITY_2022_1_OR_NEWER
                        else if (inputObjectType == HEU_InputNode.InputObjectType.SPLINE &&
                                 inputNode.SplineSettings != null)
                        {
                            if (!HEU_SplinesPacakageManager.IsInstalled())
                            {
                                HEU_EditorUI.DrawWarningLabel("Unity.Splines Package Missing");
                                EditorGUILayout.LabelField(
                                    "The 'Spline' Input Type requires the Unity.Splines package to be installed.");

                                if (GUILayout.Button("Install"))
                                    HEU_SplinesPacakageManager.Add();
                            }
                            else
                            {
                                HEU_EditorUI.DrawHeadingLabel("Spline settings");
                                EditorGUI.indentLevel++;
                                {
                                    UnityEditor.SerializedProperty samplingResolution =
                                        inputNode._uiCache._splineSettingsProperty.FindPropertyRelative(
                                            "_samplingResolution");
                                    samplingResolution.floatValue = EditorGUILayout.Slider(
                                        _samplingResolutionContent.text, samplingResolution.floatValue, 0.0f, 100.0f);
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
#endif

                        if (!bSkipElements)
                        {
                            using (var vs1 = new EditorGUILayout.VerticalScope())
                            {
                                int inputCount = inputObjectsProperty.arraySize;
                                for (int i = 0; i < inputCount; ++i)
                                {
                                    using (var hs2 = new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField("Input " + (i + 1));

                                        {
                                            if (GUILayout.Button("+", GUILayout.Width(plusButtonWidth)))
                                            {
                                                inputObjectsProperty.InsertArrayElementAtIndex(i);
                                                FixUpScaleProperty(inputObjectsProperty, i);
                                                break;
                                            }

                                            if (GUILayout.Button("-", GUILayout.Width(plusButtonWidth)))
                                            {
                                                inputObjectsProperty.DeleteArrayElementAtIndex(i);
                                                break;
                                            }
                                        }
                                    }

                                    EditorGUI.indentLevel++;
                                    using (var vs4 = new EditorGUILayout.VerticalScope())
                                    {
                                        if (i < inputNode._uiCache._inputObjectCache.Count &&
                                            i < inputNode.InputObjects.Count)
                                        {
                                            HEU_InputNodeUICache.HEU_InputObjectUICache objectCache =
                                                inputNode._uiCache._inputObjectCache[i];
                                            GameObject oldObject = inputNode.InputObjects[i]._gameObject;

                                            GameObject newObject = null;


                                            switch (inputObjectType)
                                            {
                                                case HEU_InputNode.InputObjectType.TERRAIN:
                                                    inputNode.InputObjects[i]._terrainReference =
                                                        EditorGUILayout.ObjectField(
                                                            inputNode.InputObjects[i]._terrainReference,
                                                            typeof(Terrain), true) as Terrain;

                                                    if (inputNode.InputObjects[i]._terrainReference != null)
                                                    {
                                                        newObject = inputNode.InputObjects[i]._terrainReference
                                                            .gameObject;
                                                    }

                                                    break;
                                                case HEU_InputNode.InputObjectType.BOUNDING_BOX:
                                                    inputNode.InputObjects[i]._boundingVolumeReference =
                                                        EditorGUILayout.ObjectField(
                                                            inputNode.InputObjects[i]._boundingVolumeReference,
                                                            typeof(HEU_BoundingVolume), true) as HEU_BoundingVolume;

                                                    if (inputNode.InputObjects[i]._boundingVolumeReference != null)
                                                    {
                                                        newObject = inputNode.InputObjects[i]._boundingVolumeReference
                                                            .gameObject;
                                                    }

                                                    break;
                                                case HEU_InputNode.InputObjectType.TILEMAP:
                                                    inputNode.InputObjects[i]._tilemapReference =
                                                        EditorGUILayout.ObjectField(
                                                            inputNode.InputObjects[i]._tilemapReference,
                                                            typeof(Tilemap), true) as Tilemap;

                                                    if (inputNode.InputObjects[i]._tilemapReference != null)
                                                    {
                                                        newObject = inputNode.InputObjects[i]._tilemapReference
                                                            .gameObject;
                                                    }

                                                    break;

                                                default:
                                                    newObject = EditorGUILayout.ObjectField(
                                                        inputNode.InputObjects[i]._gameObject, typeof(GameObject),
                                                        true) as GameObject;
                                                    break;
                                            }


                                            if (oldObject != newObject)
                                            {
                                                Undo.RecordObject(inputNode, "GameObject Assign");
                                                inputNode.InputObjects[i]._gameObject = newObject;
                                                // Set the reference to avoid strange bugs when switching input type modes
                                                inputNode.InputObjects[i].SetReferencesFromGameObject();
                                                EditorUtility.SetDirty(inputNode);
                                            }

                                            using (new EditorGUI.DisabledScope(!inputNode._uiCache
                                                       ._keepWorldTransformProperty.boolValue))
                                            {
                                                objectCache._transformOffsetProperty.boolValue =
                                                    HEU_EditorUI.DrawToggleLeft(
                                                        objectCache._transformOffsetProperty.boolValue,
                                                        "Transform Offset");
                                                if (objectCache._transformOffsetProperty.boolValue)
                                                {
                                                    objectCache._translateProperty.vector3Value =
                                                        EditorGUILayout.Vector3Field(translateLabel,
                                                            objectCache._translateProperty.vector3Value);
                                                    objectCache._rotateProperty.vector3Value =
                                                        EditorGUILayout.Vector3Field(rotateLabel,
                                                            objectCache._rotateProperty.vector3Value);
                                                    objectCache._scaleProperty.vector3Value =
                                                        EditorGUILayout.Vector3Field(scaleLabel,
                                                            objectCache._scaleProperty.vector3Value);
                                                }
                                            }
                                        }
                                    }

                                    EditorGUI.indentLevel--;
                                }
                            }
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                inputNode._uiCache._inputNodeSerializedObject.ApplyModifiedProperties();

                // When cooking, this will force input data to be uploaded
                inputNode.RequiresUpload = true;
                inputNode.ClearUICache();
            }
        }

        public static void FixUpScaleProperty(SerializedProperty inputObjectsProperty, int index)
        {
            SerializedProperty newInputProperty = inputObjectsProperty.GetArrayElementAtIndex(index);
            if (newInputProperty != null)
            {
                SerializedProperty scaleOverrideProperty = newInputProperty.FindPropertyRelative("_scaleOffset");
                if (scaleOverrideProperty != null)
                {
                    scaleOverrideProperty.vector3Value = Vector3.one;
                }
            }
        }

        public static void HandleSelectedObjectsForInputHDAs(GameObject[] selectedObjects, HEU_InputNode inputNode)
        {
            inputNode.HandleSelectedObjectsForInputHDAs(selectedObjects);

            inputNode._uiCache._inputNodeSerializedObject.ApplyModifiedProperties();
            inputNode.RequiresUpload = true;

            inputNode.ClearUICache();
        }

        public static void HandleSelectedObjectsForInputObjects(GameObject[] selectedObjects, HEU_InputNode inputNode)
        {
            inputNode.HandleSelectedObjectsForInputObjects(selectedObjects);
            inputNode._uiCache._inputNodeSerializedObject.ApplyModifiedProperties();
            inputNode.RequiresUpload = true;

            inputNode.ClearUICache();
        }

        private static void DrawSelectionWindow(HEU_InputNode.InputObjectType inputObjectType, HEU_InputNode inputNode)
        {
            using (var hs1 = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Selection Window",
                        "Use a custom window to select the objects from the Hierarchy.")))
                {
                    if (HEU_InputNode.GetInternalObjectType(inputObjectType) == HEU_InputNode.InternalObjectType.HDA)
                    {
                        HEU_SelectionWindow.ShowWindow(HandleSelectedObjectsForInputHDAs, typeof(HEU_HoudiniAssetRoot),
                            inputNode);
                    }
                    else if (inputObjectType == HEU_InputNode.InputObjectType.TERRAIN)
                    {
                        HEU_SelectionWindow.ShowWindow(HandleSelectedObjectsForInputObjects, typeof(Terrain),
                            inputNode);
                    }
                    else if (inputObjectType == HEU_InputNode.InputObjectType.BOUNDING_BOX)
                    {
                        HEU_SelectionWindow.ShowWindow(HandleSelectedObjectsForInputObjects, typeof(HEU_BoundingVolume),
                            inputNode);
                    }
                    else if (inputObjectType == HEU_InputNode.InputObjectType.TILEMAP)
                    {
                        HEU_SelectionWindow.ShowWindow(HandleSelectedObjectsForInputObjects, typeof(Tilemap),
                            inputNode);
                    }
                    else
                    {
                        HEU_SelectionWindow.ShowWindow(HandleSelectedObjectsForInputObjects, typeof(GameObject),
                            inputNode);
                    }
                }

                if (!inputNode._usingSelectFromHierarchy)
                {
                    string title = "Select from Hierarchy (Locks Inspector)";
                    float shortenLength = 420;
                    float reallyShortLength = 320;

                    float screenWidth = Screen.width;
                    if (screenWidth < reallyShortLength)
                    {
                        title = "From Hierarchy";
                    }
                    else if (screenWidth < shortenLength)
                    {
                        title = "Select from Hierarchy";
                    }

                    if (GUILayout.Button(new GUIContent(title,
                            "Locks the inspector and so you can select GameObjects from the Hierarchy. Once select, press Use Current Selection to add the specified objects as inputs.")))
                    {
                        SetInspectorLock(true);
                        inputNode._usingSelectFromHierarchy = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Use Current Selection"))
                    {
                        SetInspectorLock(false);
                        inputNode._usingSelectFromHierarchy = false;

                        GameObject[] selection = Selection.gameObjects;
                        List<GameObject> filteredObjects = new List<GameObject>(selection);


                        filteredObjects = filteredObjects.Filter((GameObject obj) =>
                        {
                            if (obj == null)
                            {
                                return false;
                            }

                            bool result = true;

                            if (HEU_InputNode.GetInternalObjectType(inputObjectType) ==
                                HEU_InputNode.InternalObjectType.HDA
                                && obj.GetComponent<HEU_HoudiniAssetRoot>() == null)
                            {
                                result = false;
                            }
                            else if (inputObjectType == HEU_InputNode.InputObjectType.TERRAIN
                                     && obj.GetComponent<Terrain>() == null)
                            {
                                result = false;
                            }
                            else if (inputObjectType == HEU_InputNode.InputObjectType.BOUNDING_BOX
                                     && obj.GetComponent<HEU_BoundingVolume>() == null)
                            {
                                result = false;
                            }
                            else if (inputObjectType == HEU_InputNode.InputObjectType.TILEMAP
                                     && obj.GetComponent<Tilemap>() == null)
                            {
                                result = false;
                            }

                            if (result == false)
                            {
                                HEU_Logger.LogWarning("Houdini GameObject selection: " + obj.name +
                                                      " filtered out due to invalid type!");
                                return false;
                            }

                            return true;
                        });

                        if (HEU_InputNode.GetInternalObjectType(inputObjectType) ==
                            HEU_InputNode.InternalObjectType.HDA)
                        {
                            HandleSelectedObjectsForInputHDAs(filteredObjects.ToArray(), inputNode);
                        }
                        else
                        {
                            HandleSelectedObjectsForInputObjects(filteredObjects.ToArray(), inputNode);
                        }

                        // Populate input cache if modified.
                        if (inputNode._uiCache == null)
                        {
                            PopulateCache(inputNode);
                        }

                        if (inputNode.ParentAsset && inputNode.ParentAsset.RootGameObject)
                        {
                            // Select this gameObject so it doesn't jump to the last selection as soon as it unlocks.
                            Selection.activeGameObject = inputNode.ParentAsset.RootGameObject.gameObject;
                        }
                    }
                }
            }
        }

        private static void SetInspectorLock(bool set)
        {
            ActiveEditorTracker.sharedTracker.isLocked = set;
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
    }
} // HoudiniEngineUnity
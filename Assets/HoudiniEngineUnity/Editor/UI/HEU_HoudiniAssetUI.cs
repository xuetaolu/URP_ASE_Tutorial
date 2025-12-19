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
    /// <summary>
    /// Custom Inspector UI for Houdini Asset.
    /// It uses HEU_HoudiniAssetRoot as the target object in order to access
    /// the underlying HEU_HoudiniAsset object whih contains actual data and logic.
    /// This allows to both show custom UI (via HEU_HoudiniAssetRoot) and 
    /// exclude Houdini-specific data at runtime (via HEU_HoudiniAsset which is EditorOnly).
    /// </summary>
    [CustomEditor(typeof(HEU_HoudiniAssetRoot))]
    public class HEU_HoudiniAssetUI : Editor
    {
        //	DATA ------------------------------------------------------------------------------------------------------

        // The root gameobject for an HDA. Used to show this custom UI.
        private HEU_HoudiniAssetRoot _houdiniAssetRoot;

        // Actual HDA data and logic
        private HEU_HoudiniAsset _houdiniAsset;

        // Serialized asset object
        private SerializedObject _houdiniAssetSerializedObject;

        // Cache reference to the custom parameter editor
        private Editor _parameterEditor;

        // Cache reference to the custom curve editor
        private Editor _curveEditor;

        // Cache reference to the custom curve parameter editor
        private Editor _curveParameterEditor;

        // Cache reference to the custom Tools editor
        private Editor _toolsEditor;

        // Cache reference to the custom Handles editor
        private Editor _handlesEditor;

        // Draws UI for instance inputs
        private HEU_InstanceInputUI _instanceInputUI;

        private SceneView _sceneView;

        //	GUI CONTENT -----------------------------------------------------------------------------------------------

        private static Texture2D _reloadhdaIcon;
        private static Texture2D _recookhdaIcon;
        private static Texture2D _bakegameobjectIcon;
        private static Texture2D _bakeprefabIcon;
        private static Texture2D _bakeandreplaceIcon;
        private static Texture2D _removeheIcon;
        private static Texture2D _duplicateAssetIcon;
        private static Texture2D _resetParamIcon;

        private static GUIContent _reloadhdaContent;
        private static GUIContent _recookhdaContent;
        private static GUIContent _bakegameobjectContent;
        private static GUIContent _bakeprefabContent;
        private static GUIContent _bakeandreplaceContent;
        private static GUIContent _removeheContent;
        private static GUIContent _duplicateContent;
        private static GUIContent _resetParamContent;

        private static GUIContent _dragAndDropField = new GUIContent("Drag & drop GameObjects / Prefabs:",
            "Place GameObjects and/or Prefabs here that were previously baked out and need to be updated, then click Bake Update.");

        private static GUIContent _resetMaterialOverridesButton = new GUIContent("Reset Material Overrides",
            "Remove overridden materials, and replace with generated materials for this asset's output.");

        private static GUIContent _projectCurvePointsButton = new GUIContent("Project Curve",
            "Project all points in curves to colliders or layers specified above.");

        private static GUIContent _savePresetButton =
            new GUIContent("Save HDA Preset", "Save the HDA's current preset to a file.");

        private static GUIContent _loadPresetButton =
            new GUIContent("Load HDA Preset", "Load a HDA preset file into this asset and cook it.");

        private static GUIContent _useCurveScaleRotContent = new GUIContent("Disable Curve scale/rot",
            "Disables the usage of scale/rot attributes. Useful if the scale/rot attribute values are causing issues with your curve.");

        private static GUIContent _cookCurveOnDragContent = new GUIContent("Cook Curve on Drag",
            "Cooks the curve while you are dragging the curve point. Useful if you need responsiveness over performance. Disable this option to improve performance.");

        private static GUIContent _curveFrameSelectedNodesContent = new GUIContent("Frame Selected Nodes Only",
            "Frames only the currently selected nodes when you press the F hotkey instead of the whole curve.");

        private static GUIContent _curveFrameSelectedNodeDistanceContent = new GUIContent(
            "Frame Selected Node Distance",
            "The distance between the selected node and the editor camera when you frame the selected node.");

        private static HashSet<string> _delayAutoCookStrings = new HashSet<string>
            { "ColorPickerChanged", "CurveChanged", "GradientPickerChanged" };

        //	LOGIC -----------------------------------------------------------------------------------------------------

        private void OnEnable()
        {
            _reloadhdaIcon = Resources.Load("heu_reloadhdaIcon") as Texture2D;
            _recookhdaIcon = Resources.Load("heu_recookhdaIcon") as Texture2D;
            _bakegameobjectIcon = Resources.Load("heu_bakegameobjectIcon") as Texture2D;
            _bakeprefabIcon = Resources.Load("heu_bakeprefabIcon") as Texture2D;
            _bakeandreplaceIcon = Resources.Load("heu_bakeandreplaceIcon") as Texture2D;
            _removeheIcon = Resources.Load("heu_removeheIcon") as Texture2D;
            _duplicateAssetIcon = Resources.Load("heu_duplicateassetIcon") as Texture2D;
            _resetParamIcon = Resources.Load("heu_resetparametersIcon") as Texture2D;

            _reloadhdaContent = new GUIContent("  Rebuild  ", _reloadhdaIcon,
                "Reload the asset in Houdini and cook it. Current parameter values and input objects will be re-applied. Material overrides will be removed.");
            _recookhdaContent = new GUIContent("  Recook   ", _recookhdaIcon,
                "Force recook of the asset in Houdini with the current parameter values and specified input data. Updates asset if changed in Houdini.");
            _bakegameobjectContent = new GUIContent("  GameObject", _bakegameobjectIcon,
                "Bakes the output to a new GameObject. Meshes and Materials are copied.");
            _bakeprefabContent = new GUIContent("  Prefab", _bakeprefabIcon,
                "Bakes the output to a new Prefab. Meshes and Materials are copied.");
            _bakeandreplaceContent = new GUIContent("  Update", _bakeandreplaceIcon,
                "Update existing GameObject(s) and Prefab(s). Generated components, meshes, and materials are updated. Assumes the that all asset resources are in the default cook folder indexed by the gameObject name.");
            _removeheContent = new GUIContent("  Keep Only Output", _removeheIcon,
                "Remove Houdini Engine data (HDA_Data, Houdini Asset Root object), and leave just the generated Unity data (meshes, materials, instances, etc.).");
            _duplicateContent = new GUIContent("  Duplicate", _duplicateAssetIcon,
                "Safe duplication of this asset to create an exact copy. The asset is duplicated in Houdini. All data is copied over.");
            _resetParamContent = new GUIContent("  Reset All", _resetParamIcon,
                "Reset all parameters, materials, and inputs to their HDA default values, clear cache, reload HDA, cook, and generate output.");

            _sceneView = UnityEditor.EditorWindow.GetWindow<SceneView>();

            // Get the root gameobject, and the HDA bound to it
            _houdiniAssetRoot = target as HEU_HoudiniAssetRoot;
            TryAcquiringAsset();
        }

        private void TryAcquiringAsset()
        {
            if (_houdiniAsset == null && _houdiniAssetRoot != null)
            {
                _houdiniAsset = _houdiniAssetRoot._houdiniAsset;
            }

            if (_houdiniAsset != null && _houdiniAssetSerializedObject == null)
            {
                _houdiniAssetSerializedObject = new SerializedObject(_houdiniAsset);
            }
        }

        public void RefreshUI()
        {
            // Clear out the instance input cache.
            // Needed after a cook.
            _instanceInputUI = null;

            HEU_UIRepaint();
        }

        public void HEU_UIRepaint()
        {
            SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            // Try acquiring asset reference in here again due to Undo.
            // Eg. After a delete, Undo requires us to re-acquire references.
            TryAcquiringAsset();

            string msg = "Houdini Engine Asset Error\n" +
                         "No HEU_HoudiniAsset found!";
            if (_houdiniAsset == null || !_houdiniAsset.IsValidForInteraction(ref msg))
            {
                DrawHDAUIMessage(msg);
                return;
            }

            // Always hook into asset UI callback. This could have got reset on code refresh.
            _houdiniAsset.RefreshUIDelegate = RefreshUI;

            serializedObject.Update();
            _houdiniAssetSerializedObject.Update();

            bool guiEnabled = GUI.enabled;

            using (new EditorGUILayout.VerticalScope())
            {
                DrawHeaderSection();

                DrawLicenseInfo();

                HEU_HoudiniAsset.AssetBuildAction pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.NONE;
                SerializedProperty pendingBuildProperty =
                    HEU_EditorUtility.GetSerializedProperty(_houdiniAssetSerializedObject, "_requestBuildAction");
                if (pendingBuildProperty != null)
                {
                    pendingBuildAction = (HEU_HoudiniAsset.AssetBuildAction)pendingBuildProperty.enumValueIndex;
                }

                // Track changes to Houdini Asset gameobject
                EditorGUI.BeginChangeCheck();

                bool bSkipAutoCook = DrawGenerateSection(_houdiniAssetRoot, serializedObject, _houdiniAsset,
                    _houdiniAssetSerializedObject, ref pendingBuildAction);
                if (!bSkipAutoCook)
                {
                    SerializedProperty assetCookStatusProperty =
                        HEU_EditorUtility.GetSerializedProperty(_houdiniAssetSerializedObject, "_cookStatus");
                    if (assetCookStatusProperty != null)
                    {
                        // If this is a Curve asset, we don't need to draw parameters as its redundant
                        if (_houdiniAsset.AssetTypeInternal != HEU_HoudiniAsset.HEU_AssetType.TYPE_CURVE)
                        {
                            DrawParameters(_houdiniAsset.Parameters, ref _parameterEditor);
                            HEU_EditorUI.DrawSeparator();
                        }

                        DrawCurvesSection(_houdiniAsset, _houdiniAssetSerializedObject);

                        DrawInputNodesSection(_houdiniAsset, _houdiniAssetSerializedObject);

                        DrawTerrainSection(_houdiniAsset, _houdiniAssetSerializedObject);

                        DrawInstanceInputs(_houdiniAsset, _houdiniAssetSerializedObject);

                        bSkipAutoCook = DrawBakeSection(_houdiniAssetRoot, serializedObject, _houdiniAsset,
                            _houdiniAssetSerializedObject, ref pendingBuildAction);

                        DrawAssetOptions(_houdiniAsset, _houdiniAssetSerializedObject);

                        DrawEventsSection(_houdiniAsset, _houdiniAssetSerializedObject);
                    }
                }

                ProcessPendingBuildAction(pendingBuildAction, pendingBuildProperty,
                    _houdiniAssetRoot, serializedObject, _houdiniAsset, _houdiniAssetSerializedObject);

                // Check if any changes occurred, and if so, trigger a recook
                if (EditorGUI.EndChangeCheck())
                {
                    // Check options that require a rebuild/recook if changed.
                    bool oldUseOutputNodes = _houdiniAsset.UseOutputNodes;
                    bool oldUsePoints = _houdiniAsset.GenerateMeshUsingPoints;

                    _houdiniAssetSerializedObject.ApplyModifiedProperties();
                    serializedObject.ApplyModifiedProperties();

                    bool bNeedsRebuild = false;
                    bool bNeedsRecook = false;

                    // UseOutputNodes is a special parameter that requires us to rebuild in order to use it.
                    if (_houdiniAsset.UseOutputNodes != oldUseOutputNodes)
                    {
                        bNeedsRebuild = true;
                    }

                    if (_houdiniAsset.GenerateMeshUsingPoints != oldUsePoints)
                    {
                        bNeedsRecook = true;
                    }

                    if (!bSkipAutoCook)
                    {
                        // If we need a rebuild, do that first
                        if (HEU_PluginSettings.CookingEnabled && _houdiniAsset.AutoCookOnParameterChange &&
                            bNeedsRebuild)
                        {
                            _houdiniAsset.RequestReload(true);
                        }
                        else if (bNeedsRecook)
                        {
                            _houdiniAsset.RequestCook();
                        }
                        else if (HEU_PluginSettings.CookingEnabled && _houdiniAsset.AutoCookOnParameterChange &&
                                 _houdiniAsset.DoesAssetRequireRecook())
                        {
                            // Often times, cooking while dragging mouse results in poor UX
                            bool isDragging = (EditorGUIUtility.hotControl != 0);
                            bool blockAutoCook = _houdiniAsset.PendingAutoCookOnMouseRelease == true ||
                                                 (isDragging && Event.current != null &&
                                                  _delayAutoCookStrings.Contains(Event.current.commandName));

                            if (HEU_PluginSettings.CookOnMouseUp && blockAutoCook)
                            {
                                _houdiniAsset.PendingAutoCookOnMouseRelease = true;
                            }
                            else
                            {
                                _houdiniAsset.PendingAutoCookOnMouseRelease = false;
                                _houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false,
                                    bSkipCookCheck: false, bUploadParameters: true);
                            }
                        }
                    }
                }
            }

            GUI.enabled = guiEnabled;
        }

        /// <summary>
        /// Callback when Scene is updated
        /// </summary>
        public void OnSceneGUI()
        {
            if (_houdiniAsset == null)
            {
                return;
            }

            if (!_houdiniAsset.IsAssetValid())
            {
                return;
            }

            if (_houdiniAsset.SerializedMetaData != null && _houdiniAsset.SerializedMetaData.SoftDeleted == true)
            {
                return;
            }

            if ((Event.current.type == EventType.ValidateCommand &&
                 Event.current.commandName.Equals("UndoRedoPerformed")))
            {
                Event.current.Use();
            }

            if ((Event.current.type == EventType.ExecuteCommand &&
                 Event.current.commandName.Equals("UndoRedoPerformed")))
            {
                // On Undo, need to check which parameters have changed in order to update and recook.
                _houdiniAsset.SyncInternalParametersForUndoCompare();

                _houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false,
                    bUploadParameters: true);

                // Force a repaint here to update the UI when Undo is invoked. Handles case where the Inspector window is
                // no longer the focus. Without this the Inspector window still shows old value until user selects it.
                HEU_UIRepaint();
            }

            // Draw custom scene elements. Should be called for any event, not just repaint.
            DrawSceneElements(_houdiniAsset);
        }

        /// <summary>
        /// Draw Houdini Engine license info.
        /// </summary>
        private void DrawLicenseInfo()
        {
            HAPI_License license = HEU_SessionManager.GetCurrentLicense(false);
            if (license == HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE_INDIE)
            {
                HEU_EditorUI.DrawSeparator();

                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.normal.textColor = HEU_EditorUI.IsEditorDarkSkin() ? Color.yellow : Color.red;
                EditorGUILayout.LabelField("Houdini Engine Indie - For Limited Commercial Use Only", labelStyle);

                HEU_EditorUI.DrawSeparator();
            }
        }

        private void DrawHDAUIMessage(string msg)
        {
            HEU_EditorUI.DrawSeparator();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = HEU_EditorUI.IsEditorDarkSkin() ? Color.yellow : Color.red;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.wordWrap = true;
            EditorGUILayout.LabelField(msg, labelStyle);

            HEU_EditorUI.DrawSeparator();
        }

        /// <summary>
        /// Draw the Object Instance Inputs section for given asset.
        /// </summary>
        /// <param name="asset">The HDA asset</param>
        /// <param name="assetObject">Serialized HDA asset object</param>
        private void DrawInstanceInputs(HEU_HoudiniAsset asset, SerializedObject assetObject)
        {
            if (_instanceInputUI == null)
            {
                _instanceInputUI = new HEU_InstanceInputUI();
            }

            _instanceInputUI.DrawInstanceInputs(asset, assetObject);
        }

        /// <summary>
        /// Draw asset options for given asset.
        /// </summary>
        /// <param name="asset">The HDA asset</param>
        /// <param name="assetObject">Serialized HDA asset object</param>
        private void DrawAssetOptions(HEU_HoudiniAsset asset, SerializedObject assetObject)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.fixedHeight = 24;

            HEU_EditorUI.BeginSection();
            {
                SerializedProperty showHDAOptionsProperty = assetObject.FindProperty("_showHDAOptions");

                showHDAOptionsProperty.boolValue =
                    HEU_EditorUI.DrawFoldOut(showHDAOptionsProperty.boolValue, "ASSET OPTIONS");
                if (showHDAOptionsProperty.boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.BeginHorizontal();

                    // If inspector is not too small, create two columns for more visually pleasing UX
                    int shortenLength = 420;

                    int screenWidth = Screen.width;

                    bool useTwoColumns = screenWidth > shortenLength;

                    System.Action drawGenerateSection = () =>
                    {
                        HEU_EditorUI.BeginSimpleSection("Generate");
                        HEU_EditorUI.DrawPropertyField(assetObject, "_useOutputNodes", "Use output nodes",
                            "Create outputs using output nodes. Note: Requires a full rebuild if changed");
                        HEU_EditorUI.DrawPropertyField(assetObject, "_useLODGroups", "LOD Groups",
                            "Automatically create Unity LOD group if found.");
                        HEU_EditorUI.DrawPropertyField(assetObject, "_generateNormals", "Normals",
                            "Generate normals in Unity for output geometry.");
                        HEU_EditorUI.DrawPropertyField(assetObject, "_generateTangents", "Tangents",
                            "Generate tangents in Unity for output geometry.");
                        HEU_EditorUI.DrawPropertyField(assetObject, "_generateUVs", "UVs",
                            "Force Unity to generate UVs for output geometry.");
                        HEU_EditorUI.DrawPropertyField(assetObject, "_generateMeshUsingPoints", "Using Points",
                            "Use point attributes instead of vertex attributes for geometry. Ignores vertex attributes.");
                        HEU_EditorUI.EndSimpleSection();
                    };

                    EditorGUILayout.BeginVertical();

                    HEU_EditorUI.BeginSimpleSection("Cook Triggers");
                    HEU_EditorUI.DrawPropertyField(assetObject, "_autoCookOnParameterChange", "Parameter Change",
                        "Automatically cook when a parameter changes. If off, must use Recook to cook.");
                    HEU_EditorUI.DrawPropertyField(assetObject, "_transformChangeTriggersCooks", "Transform Change",
                        "Changing the transform (e.g. moving) the asset in Unity will invoke cook in Houdini.");
                    HEU_EditorUI.DrawPropertyField(assetObject, "_cookingTriggersDownCooks", "Downstream Cooks",
                        "Cooking this asset will trigger dependent assets' to also cook.");
                    HEU_EditorUI.DrawPropertyField(assetObject, "_sessionSyncAutoCook", "Session Sync: Auto Cook",
                        "When using Session Sync, this asset will automatically cook and generated output when it is cooked separately in Houdini (e.g. via parm changes).");
                    HEU_EditorUI.EndSimpleSection();

                    if (!useTwoColumns)
                    {
                        drawGenerateSection();
                    }

                    HEU_EditorUI.BeginSimpleSection("Miscellaneous");
                    HEU_EditorUI.DrawPropertyField(assetObject, "_pushTransformToHoudini", "Push Transform To Houdini",
                        "Send the asset's transform to Houdini and apply to object.");
                    HEU_EditorUI.DrawPropertyField(assetObject, "_ignoreNonDisplayNodes", "Ignore Non-Display Nodes",
                        "Only display node geometry will be created.");
                    HEU_EditorUI.DrawPropertyField(assetObject, "_splitGeosByGroup", "Split Geos By Group",
                        "Split geometry into separate gameobjects by group. Deprecated feature and only recommended for simple use cases.");
                    HEU_EditorUI.EndSimpleSection();

                    EditorGUILayout.EndVertical();

                    if (useTwoColumns)
                    {
                        EditorGUILayout.BeginVertical();
                        drawGenerateSection();
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndHorizontal();

                    if (asset.NumAttributeStores() > 0)
                    {
                        HEU_EditorUI.DrawPropertyField(assetObject, "_editableNodesToolsEnabled",
                            "Enable Editable Node Tools",
                            "Displays Editable Node Tools and generates the node's geometry, if asset has editable nodes.");
                    }

                    if (asset.NumHandles() > 0)
                    {
                        HEU_EditorUI.DrawPropertyField(assetObject, "_handlesEnabled", "Enable Handles",
                            "Creates Houdini Handles if asset has them.");
                    }

                    EditorGUILayout.Space();

                    using (var hs = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(_savePresetButton, buttonStyle))
                        {
                            string fileName = asset.AssetName;
                            string filePattern = "heupreset";
                            string newPath = EditorUtility.SaveFilePanel("Save HDA preset", "",
                                fileName + "." + filePattern, filePattern);

                            if (newPath != null && !string.IsNullOrEmpty(newPath))
                            {
                                HEU_AssetPresetUtility.SaveAssetPresetToFile(asset, newPath);
                            }
                        }

                        if (GUILayout.Button(_loadPresetButton, buttonStyle))
                        {
                            string fileName = asset.AssetName;
                            string filePattern = "heupreset,preset";
                            string newPath = EditorUtility.OpenFilePanel("Load HDA preset", "", filePattern);

                            if (newPath != null && !string.IsNullOrEmpty(newPath))
                            {
                                HEU_AssetPresetUtility.LoadPresetFileIntoAssetAndCook(asset, newPath);
                            }
                        }
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button(_resetMaterialOverridesButton, buttonStyle))
                    {
                        asset.ResetMaterialOverrides();
                    }

                    EditorGUI.indentLevel--;
                }
            }
            HEU_EditorUI.EndSection();

            HEU_EditorUI.DrawSeparator();
        }

        private static HEU_HoudiniAsset.AssetCookStatus GetCookStatusFromSerializedAsset(SerializedObject assetObject)
        {
            HEU_HoudiniAsset.AssetCookStatus cookStatus = HEU_HoudiniAsset.AssetCookStatus.NONE;

            SerializedProperty cookStatusProperty = HEU_EditorUtility.GetSerializedProperty(assetObject, "_cookStatus");
            if (cookStatusProperty != null)
            {
                cookStatus = (HEU_HoudiniAsset.AssetCookStatus)cookStatusProperty.enumValueIndex;
            }

            return cookStatus;
        }

        private static GUIStyle _mainButtonStyle;
        private static GUIStyle _mainCentredButtonStyle;
        private static GUIStyle _mainButtonSetStyle;
        private static GUIStyle _mainBoxStyle;
        private static GUIStyle _mainPromptStyle;

        private const float _mainButtonSeparatorDistance = 5f;

        private static void CreateMainButtonStyle()
        {
            if (_mainButtonStyle != null)
            {
                return;
            }

            float screenWidth = EditorGUIUtility.currentViewWidth;

            float buttonHeight = 30f;
            float widthPadding = 55f;
            float doubleButtonWidth = Mathf.Round(screenWidth - widthPadding + _mainButtonSeparatorDistance);
            float singleButtonWidth = Mathf.Round((screenWidth - widthPadding) * 0.5f);

            _mainButtonStyle = new GUIStyle(GUI.skin.button);
            _mainButtonStyle.fontStyle = FontStyle.Bold;
            _mainButtonStyle.fontSize = 12;
            _mainButtonStyle.alignment = TextAnchor.MiddleCenter;
            _mainButtonStyle.fixedHeight = buttonHeight;
            _mainButtonStyle.padding.left = 6;
            _mainButtonStyle.padding.right = 6;
            _mainButtonStyle.margin.left = 0;
            _mainButtonStyle.margin.right = 0;
            _mainButtonStyle.clipping = TextClipping.Clip;
            _mainButtonStyle.wordWrap = true;

            _mainCentredButtonStyle = new GUIStyle(_mainButtonStyle);
            _mainCentredButtonStyle.alignment = TextAnchor.MiddleCenter;

            _mainButtonSetStyle = new GUIStyle(GUI.skin.box);
            RectOffset br = _mainButtonSetStyle.margin;
            br.left = 4;
            br.right = 4;
            _mainButtonSetStyle.margin = br;

            _mainBoxStyle = new GUIStyle(GUI.skin.GetStyle("ColorPickerBackground"));
            br = _mainBoxStyle.margin;
            br.left = 4;
            br.right = 4;
            _mainBoxStyle.margin = br;
            _mainBoxStyle.padding = br;

            _mainPromptStyle = new GUIStyle(GUI.skin.button);
            _mainPromptStyle.fontSize = 11;
            _mainPromptStyle.alignment = TextAnchor.MiddleCenter;
            _mainPromptStyle.fixedHeight = 30;
            _mainPromptStyle.margin.left = 34;
            _mainPromptStyle.margin.right = 34;
        }

        /// <summary>
        /// Draw the Generate section.
        /// </summary>
        private static bool DrawGenerateSection(HEU_HoudiniAssetRoot assetRoot,
            SerializedObject assetRootSerializedObject,
            HEU_HoudiniAsset asset, SerializedObject assetObject,
            ref HEU_HoudiniAsset.AssetBuildAction pendingBuildAction)
        {
            bool bSkipAutoCook = false;

            CreateMainButtonStyle();

            _recookhdaContent.text = "  Recook";

            HEU_HoudiniAsset.AssetCookStatus cookStatus = GetCookStatusFromSerializedAsset(assetObject);

            if (cookStatus == HEU_HoudiniAsset.AssetCookStatus.SELECT_SUBASSET)
            {
                // Prompt user to select subasset

                GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
                promptStyle.fontStyle = FontStyle.Bold;
                promptStyle.normal.textColor = HEU_EditorUI.IsEditorDarkSkin() ? Color.green : Color.blue;
                EditorGUILayout.LabelField("SELECT AN ASSET TO INSTANTIATE:", promptStyle);

                EditorGUILayout.Separator();

                int selectedIndex = -1;
                string[] subassetNames = asset.SubassetNames;

                for (int i = 0; i < subassetNames.Length; ++i)
                {
                    if (GUILayout.Button(subassetNames[i], _mainPromptStyle))
                    {
                        selectedIndex = i;
                        break;
                    }

                    EditorGUILayout.Separator();
                }

                if (selectedIndex >= 0)
                {
                    SerializedProperty selectedIndexProperty =
                        HEU_EditorUtility.GetSerializedProperty(assetObject, "_selectedSubassetIndex");
                    if (selectedIndexProperty != null)
                    {
                        selectedIndexProperty.intValue = selectedIndex;
                    }
                }

                bSkipAutoCook = true;
            }
            else
            {
                HEU_EditorUI.BeginSection();
                {
                    if (cookStatus == HEU_HoudiniAsset.AssetCookStatus.COOKING ||
                        cookStatus == HEU_HoudiniAsset.AssetCookStatus.POSTCOOK)
                    {
                        _recookhdaContent.text = "  Cooking Asset";
                    }
                    else if (cookStatus == HEU_HoudiniAsset.AssetCookStatus.LOADING ||
                             cookStatus == HEU_HoudiniAsset.AssetCookStatus.POSTLOAD)
                    {
                        _reloadhdaContent.text = "  Loading Asset";
                    }

                    SerializedProperty showGenerateProperty = assetObject.FindProperty("_showGenerateSection");

                    showGenerateProperty.boolValue =
                        HEU_EditorUI.DrawFoldOut(showGenerateProperty.boolValue, "GENERATE");
                    if (showGenerateProperty.boolValue)
                    {
                        HEU_EditorUI.DrawSeparator();

                        using (var hs = new EditorGUILayout.HorizontalScope(_mainBoxStyle))
                        {
                            if (GUILayout.Button(_reloadhdaContent, _mainButtonStyle))
                            {
                                pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.RELOAD;
                                bSkipAutoCook = true;
                            }

                            GUILayout.Space(_mainButtonSeparatorDistance);

                            if (!bSkipAutoCook && GUILayout.Button(_recookhdaContent, _mainButtonStyle))
                            {
                                pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.COOK;
                                bSkipAutoCook = true;
                            }
                        }

                        using (var hs = new EditorGUILayout.HorizontalScope(_mainBoxStyle))
                        {
                            if (GUILayout.Button(_duplicateContent, _mainButtonStyle))
                            {
                                pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.DUPLICATE;
                                bSkipAutoCook = true;
                            }

                            GUILayout.Space(_mainButtonSeparatorDistance);

                            if (GUILayout.Button(_resetParamContent, _mainButtonStyle))
                            {
                                pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.RESET_PARAMS;
                                bSkipAutoCook = true;
                            }
                        }
                    }
                }

                HEU_EditorUI.EndSection();
            }

            HEU_EditorUI.DrawSeparator();

            return bSkipAutoCook;
        }

        private static void ProcessPendingBuildAction(
            HEU_HoudiniAsset.AssetBuildAction pendingBuildAction,
            SerializedProperty pendingBuildProperty,
            HEU_HoudiniAssetRoot assetRoot,
            SerializedObject assetRootSerializedObject,
            HEU_HoudiniAsset asset,
            SerializedObject assetObject)
        {
            if (pendingBuildAction != HEU_HoudiniAsset.AssetBuildAction.NONE)
            {
                // Sanity check to make sure the asset is part of the AssetUpater
                HEU_AssetUpdater.AddAssetForUpdate(asset);

                // Apply pending build action based on user UI interaction above
                pendingBuildProperty.enumValueIndex = (int)pendingBuildAction;

                if (pendingBuildAction == HEU_HoudiniAsset.AssetBuildAction.COOK)
                {
                    // Recook should only update parameters that haven't changed. Otherwise if not checking and updating parameters,
                    // then buttons will trigger callbacks on Recook which is not desired.
                    SerializedProperty checkParameterChange =
                        HEU_EditorUtility.GetSerializedProperty(assetObject, "_checkParameterChangeForCook");
                    if (checkParameterChange != null)
                    {
                        checkParameterChange.boolValue = true;
                    }

                    // But we do want to always upload input geometry on user hitting Recook expliclity
                    SerializedProperty forceUploadInputs =
                        HEU_EditorUtility.GetSerializedProperty(assetObject, "_forceUploadInputs");
                    if (forceUploadInputs != null)
                    {
                        forceUploadInputs.boolValue = true;
                    }
                }
            }
        }

        private static bool DrawBakeSection(HEU_HoudiniAssetRoot assetRoot,
            SerializedObject assetRootSerializedObject,
            HEU_HoudiniAsset asset, SerializedObject assetObject,
            ref HEU_HoudiniAsset.AssetBuildAction pendingBuildAction)
        {
            bool bSkipAutoCook = false;

            HEU_EditorUI.BeginSection();
            {
                SerializedProperty showBakeProperty = assetObject.FindProperty("_showBakeSection");

                showBakeProperty.boolValue = HEU_EditorUI.DrawFoldOut(showBakeProperty.boolValue, "BAKE");
                if (showBakeProperty.boolValue)
                {
                    // Bake -> New Instance, New Prefab, Existing instance or prefab

                    using (var hs = new EditorGUILayout.HorizontalScope(_mainBoxStyle))
                    {
                        if (GUILayout.Button(_bakegameobjectContent, _mainButtonStyle))
                        {
                            asset.BakeToNewStandalone();
                        }

                        GUILayout.Space(_mainButtonSeparatorDistance);

                        if (GUILayout.Button(_bakeprefabContent, _mainButtonStyle))
                        {
                            asset.BakeToNewPrefab();
                        }
                    }

                    using (var vs = new EditorGUILayout.VerticalScope(_mainBoxStyle))
                    {
                        if (GUILayout.Button(_removeheContent, _mainButtonStyle))
                        {
                            pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.STRIP_HEDATA;
                            bSkipAutoCook = true;
                        }
                    }

                    using (var hs2 = new EditorGUILayout.VerticalScope(_mainBoxStyle))
                    {
                        if (GUILayout.Button(_bakeandreplaceContent, _mainCentredButtonStyle))
                        {
                            if (assetRoot._bakeTargets == null || assetRoot._bakeTargets.Count == 0)
                            {
                                // No bake target means user probably forgot to set one. So complain!
                                HEU_EditorUtility.DisplayDialog("No Bake Targets",
                                    "Bake Update requires atleast one valid GameObject.\n\nDrag a GameObject or Prefab onto the Drag and drop GameObjects / Prefabs field!",
                                    "OK");
                            }
                            else
                            {
                                int numTargets = assetRoot._bakeTargets.Count;
                                for (int i = 0; i < numTargets; ++i)
                                {
                                    GameObject bakeGO = assetRoot._bakeTargets[i];
                                    if (bakeGO != null)
                                    {
                                        if (HEU_EditorUtility.IsPrefabAsset(bakeGO))
                                        {
                                            // Prefab asset means its the source prefab, and not an instance of it
                                            asset.BakeToExistingPrefab(bakeGO);
                                        }
                                        else
                                        {
                                            // This is for all standalone (including prefab instances)
                                            asset.BakeToExistingStandalone(bakeGO);
                                        }
                                    }
                                    else
                                    {
                                        HEU_Logger.LogWarning("Unable to bake to null target at index " + i);
                                    }
                                }
                            }
                        }

                        using (var hs = new EditorGUILayout.VerticalScope(_mainButtonSetStyle))
                        {
                            SerializedProperty bakeTargetsProp = assetRootSerializedObject.FindProperty("_bakeTargets");
                            if (bakeTargetsProp != null)
                            {
                                EditorGUILayout.PropertyField(bakeTargetsProp, _dragAndDropField, true);
                            }
                        }

                        HEU_EditorUI.BeginSimpleSection("Bake Update");
                        HEU_EditorUI.DrawPropertyField(assetObject, "_bakeUpdateKeepPreviousTransformValues",
                            "Keep Previous Transform Values",
                            "Copy previous transform values when doing a Bake Update.");
                        HEU_EditorUI.EndSimpleSection();
                    }
                }
            }
            HEU_EditorUI.EndSection();

            HEU_EditorUI.DrawSeparator();

            return bSkipAutoCook;
        }

        /// <summary>
        /// Draw the Houdini Engine header image
        /// </summary>
        public static void DrawHeaderSection()
        {
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            string fileName = HEU_EditorUI.IsEditorDarkSkin() ? "heu_hengine_d" : "heu_hengine";
            Texture2D headerImage = Resources.Load(fileName) as Texture2D;

            HEU_EditorUI.DrawSeparator();
            GUILayout.Label(headerImage);

            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Draw Asset Events section.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="assetObject"></param>
        private void DrawEventsSection(HEU_HoudiniAsset asset, SerializedObject assetObject)
        {
            HEU_EditorUI.BeginSection();
            {
                SerializedProperty showEventsProperty = assetObject.FindProperty("_showEventsSection");

                showEventsProperty.boolValue = HEU_EditorUI.DrawFoldOut(showEventsProperty.boolValue, "EVENTS");
                if (showEventsProperty.boolValue)
                {
                    HEU_EditorUI.DrawSeparator();

                    SerializedProperty reloadDataEvent = assetObject.FindProperty("_reloadDataEvent");
                    EditorGUILayout.PropertyField(reloadDataEvent, new GUIContent("Rebuild Events"));

                    HEU_EditorUI.DrawSeparator();

                    SerializedProperty recookDataEvent = assetObject.FindProperty("_cookedDataEvent");
                    EditorGUILayout.PropertyField(recookDataEvent, new GUIContent("Cooked Events"));

                    HEU_EditorUI.DrawSeparator();

                    SerializedProperty bakedDataEvent = assetObject.FindProperty("_bakedDataEvent");
                    EditorGUILayout.PropertyField(bakedDataEvent, new GUIContent("Baked Events"));

                    HEU_EditorUI.DrawSeparator();

                    SerializedProperty preAssetEvent = assetObject.FindProperty("_preAssetEvent");
                    EditorGUILayout.PropertyField(preAssetEvent, new GUIContent("Pre-Asset Events"));
                }
            }

            HEU_EditorUI.EndSection();

            HEU_EditorUI.DrawSeparator();
        }

        private void DrawParameters(HEU_Parameters parameters, ref Editor parameterEditor)
        {
            if (parameters != null)
            {
                SerializedObject paramObject = new SerializedObject(parameters);
                Editor.CreateCachedEditor(paramObject.targetObject, null, ref parameterEditor);
                parameterEditor.OnInspectorGUI();
            }
        }

        private void DrawCurvesSection(HEU_HoudiniAsset asset, SerializedObject assetObject)
        {
            if (!asset.IsAssetValid())
            {
                return;
            }


            if (asset.GetEditableCurveCount() <= 0)
            {
                return;
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 11;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.fixedHeight = 24;
            buttonStyle.margin.left = 34;

            HEU_EditorUI.BeginSection();
            {
                List<HEU_Curve> curves = asset.Curves;


                SerializedProperty showCurvesProperty =
                    HEU_EditorUtility.GetSerializedProperty(assetObject, "_showCurvesSection");
                if (showCurvesProperty != null)
                {
                    showCurvesProperty.boolValue = HEU_EditorUI.DrawFoldOut(showCurvesProperty.boolValue, "CURVES");
                    if (showCurvesProperty.boolValue)
                    {
                        HEU_EditorUI.DrawHeadingLabel("Curve Data");

                        EditorGUI.indentLevel++;

                        List<SerializedObject> serializedCurves = new List<SerializedObject>();
                        for (int i = 0; i < curves.Count; i++)
                        {
                            serializedCurves.Add(new SerializedObject(curves[i]));
                        }

                        bool bHasBeenModifiedInInspector = false;

                        for (int i = 0; i < serializedCurves.Count; i++)
                        {
                            HEU_Curve curve = curves[i];

                            SerializedObject serializedCurve = serializedCurves[i];
                            EditorGUI.BeginChangeCheck();

                            if (curve.CurveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM)
                            {
                                HEU_EditorUI.DrawHeadingLabel("Input Curve Info:");
                                EditorGUI.indentLevel++;

                                // Create the UI manually to have more control
                                SerializedProperty inputCurveInfoProperty =
                                    HEU_EditorUtility.GetSerializedProperty(serializedCurve, "_inputCurveInfo");

                                System.Action<int> onCurveTypeChanged = (int value) =>
                                {
                                    SerializedProperty orderProperty =
                                        inputCurveInfoProperty.FindPropertyRelative("order");
                                    int curOrder = orderProperty.intValue;

                                    HAPI_CurveType curveType = (HAPI_CurveType)value;
                                    if (curOrder < 4 && (curveType == HAPI_CurveType.HAPI_CURVETYPE_NURBS ||
                                                         curveType == HAPI_CurveType.HAPI_CURVETYPE_BEZIER))
                                    {
                                        orderProperty.intValue = 4;
                                    }
                                    else if (curveType == HAPI_CurveType.HAPI_CURVETYPE_LINEAR)
                                    {
                                        orderProperty.intValue = 2;
                                    }
                                };


                                HEU_EditorUtility.EnumToPopup(
                                    inputCurveInfoProperty.FindPropertyRelative("curveType"),
                                    "Curve Type",
                                    (int)curve.InputCurveInfo.curveType,
                                    HEU_InputCurveInfo.GetCurveTypeNames(),
                                    true,
                                    "Type of the curve. Can be Linear, NURBs or Bezier. May impose restrictions on the order depending on what you choose.",
                                    onCurveTypeChanged
                                );

                                EditorGUILayout.PropertyField(inputCurveInfoProperty.FindPropertyRelative("order"));

                                EditorGUILayout.PropertyField(inputCurveInfoProperty.FindPropertyRelative("closed"));

                                EditorGUILayout.PropertyField(inputCurveInfoProperty.FindPropertyRelative("reverse"));

                                HEU_EditorUtility.EnumToPopup(
                                    inputCurveInfoProperty.FindPropertyRelative("inputMethod"),
                                    "Input Method",
                                    (int)curve.InputCurveInfo.inputMethod,
                                    HEU_InputCurveInfo.GetInputMethodNames(),
                                    true,
                                    "How the curve behaves with respect to the provided CVs. Can be either CVs, which influence the curve, or breakpoints, which intersects the curve."
                                );

                                using (new EditorGUI.DisabledScope(curve.InputCurveInfo.inputMethod !=
                                                                   HAPI_InputCurveMethod.HAPI_CURVEMETHOD_BREAKPOINTS))
                                {
                                    HEU_EditorUtility.EnumToPopup(
                                        inputCurveInfoProperty.FindPropertyRelative("breakpointParameterization"),
                                        "Breakpoint Parameterization",
                                        (int)curve.InputCurveInfo.breakpointParameterization,
                                        HEU_InputCurveInfo.GetBreakpointParameterizationNames(),
                                        true,
                                        "Defines which method is used to refine the curve when using breakpoints."
                                    );
                                }

                                EditorGUI.indentLevel--;
                            }

                            HEU_EditorUtility.EditorDrawSerializedProperty(serializedCurve, "_curveNodeData",
                                label: curve.CurveName + " Data");

                            if (EditorGUI.EndChangeCheck())
                            {
                                curves[i].SetEditState(HEU_Curve.CurveEditState.REQUIRES_GENERATION);
                                serializedCurve.ApplyModifiedProperties();

                                bHasBeenModifiedInInspector = true;
                            }

                            EditorGUI.indentLevel--;

                            if (curve.Parameters != null)
                            {
                                DrawParameters(curve.Parameters, ref _curveParameterEditor);
                            }

                            EditorGUI.indentLevel++;
                        }

                        if (bHasBeenModifiedInInspector)
                        {
                            if (asset.GetEditableCurveCount() > 0)
                            {
                                HEU_Curve[] curvesArray = asset.Curves.ToArray();
                                Editor.CreateCachedEditor(curvesArray, null, ref _curveEditor);
                                (_curveEditor as HEU_CurveUI).RepaintCurves();

                                if (HEU_PluginSettings.CookingEnabled && asset.AutoCookOnParameterChange)
                                {
                                    _houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false,
                                        bSkipCookCheck: false, bUploadParameters: true);
                                }
                            }
                        }

                        EditorGUI.indentLevel--;

                        HEU_EditorUI.DrawSeparator();

                        HEU_EditorUI.DrawHeadingLabel("Curve Node Settings");

                        EditorGUI.indentLevel++;

                        SerializedProperty curveEditorProperty =
                            HEU_EditorUtility.GetSerializedProperty(assetObject, "_curveEditorEnabled");
                        if (curveEditorProperty != null)
                        {
                            EditorGUILayout.PropertyField(curveEditorProperty);
                        }

                        SerializedProperty useScaleRotProperty =
                            HEU_EditorUtility.GetSerializedProperty(assetObject, "_curveDisableScaleRotation");

                        bool oldUseScaleRotValue = useScaleRotProperty.boolValue;
                        useScaleRotProperty.boolValue =
                            EditorGUILayout.Toggle(_useCurveScaleRotContent, useScaleRotProperty.boolValue);
                        if (useScaleRotProperty.boolValue != oldUseScaleRotValue)
                        {
                            for (int i = 0; i < curves.Count; ++i)
                            {
                                curves[i].SetEditState(HEU_Curve.CurveEditState.REQUIRES_GENERATION);
                            }
                        }

                        SerializedProperty curveFrameSelectedNodesProperty =
                            HEU_EditorUtility.GetSerializedProperty(assetObject, "_curveFrameSelectedNodes");
                        curveFrameSelectedNodesProperty.boolValue =
                            EditorGUILayout.Toggle(_curveFrameSelectedNodesContent,
                                curveFrameSelectedNodesProperty.boolValue);

                        EditorGUI.indentLevel++;
                        using (new EditorGUI.DisabledScope(!curveFrameSelectedNodesProperty.boolValue))
                        {
                            HEU_EditorUtility.EditorDrawFloatProperty(assetObject, "_curveFrameSelectedNodeDistance",
                                label: _curveFrameSelectedNodeDistanceContent.text,
                                tooltip: _curveFrameSelectedNodeDistanceContent.tooltip);
                        }

                        EditorGUI.indentLevel--;

                        EditorGUI.indentLevel--;

                        HEU_EditorUI.DrawHeadingLabel("Collision Settings");
                        EditorGUI.indentLevel++;

                        string projectLabel = "Project Curves To ";

                        SerializedProperty curveCollisionProperty =
                            HEU_EditorUtility.GetSerializedProperty(assetObject, "_curveDrawCollision");
                        if (curveCollisionProperty != null)
                        {
                            EditorGUILayout.PropertyField(curveCollisionProperty, new GUIContent("Collision Type"));
                            if (curveCollisionProperty.enumValueIndex == (int)HEU_Curve.CurveDrawCollision.COLLIDERS)
                            {
                                HEU_EditorUtility.EditorDrawSerializedProperty(assetObject, "_curveDrawColliders",
                                    label: "Colliders");
                                projectLabel += "Colliders";
                            }
                            else if (curveCollisionProperty.enumValueIndex ==
                                     (int)HEU_Curve.CurveDrawCollision.LAYERMASK)
                            {
                                HEU_EditorUtility.EditorDrawSerializedProperty(assetObject, "_curveDrawLayerMask",
                                    label: "Layer Mask");
                                projectLabel += "Layer";
                            }

                            HEU_EditorUI.DrawSeparator();

                            EditorGUI.indentLevel--;
                            HEU_EditorUI.DrawHeadingLabel("Projection Settings");
                            EditorGUI.indentLevel++;

                            SerializedProperty projectCurveToSceneViewProperty =
                                HEU_EditorUtility.GetSerializedProperty(assetObject, "_curveProjectDirectionToView");
                            HEU_EditorUtility.EditorDrawSerializedProperty(assetObject, "_curveProjectDirectionToView",
                                label: "Project Direction To Scene View",
                                tooltip: "Project the curve points according to the scene view.");

                            bool curveToSceneView = projectCurveToSceneViewProperty.boolValue;
                            Vector3 projectDir = Vector3.down;

                            if (curveToSceneView && _sceneView != null)
                            {
                                Quaternion sceneRot = _sceneView.rotation;

                                if (sceneRot != Quaternion.identity)
                                {
                                    projectDir = sceneRot * Vector3.forward;
                                }
                                else
                                {
                                    curveToSceneView = false; // Fallback to hard coded direction
                                }
                            }

                            using (new EditorGUI.DisabledScope(curveToSceneView))
                            {
                                HEU_EditorUtility.EditorDrawSerializedProperty(assetObject, "_curveProjectDirection",
                                    label: "Project Direction",
                                    tooltip: "The ray cast direction for projecting the curve points.");
                            }

                            HEU_EditorUtility.EditorDrawFloatProperty(assetObject, "_curveProjectMaxDistance",
                                label: "Project Max Distance",
                                tooltip: "The maximum ray cast distance for projecting the curve points.");

                            _projectCurvePointsButton.text = projectLabel;
                            if (GUILayout.Button(_projectCurvePointsButton, buttonStyle, GUILayout.MaxWidth(180)))
                            {
                                SerializedProperty projectDirProperty =
                                    HEU_EditorUtility.GetSerializedProperty(assetObject, "_curveProjectDirection");
                                SerializedProperty maxDistanceProperty =
                                    HEU_EditorUtility.GetSerializedProperty(assetObject, "_curveProjectMaxDistance");

                                if (!curveToSceneView && projectDirProperty != null)
                                {
                                    projectDir = projectDirProperty.vector3Value;
                                }

                                float maxDistance = maxDistanceProperty != null ? maxDistanceProperty.floatValue : 0;

                                for (int i = 0; i < curves.Count; ++i)
                                {
                                    curves[i].ProjectToCollidersInternal(asset, projectDir, maxDistance);
                                }
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }
            HEU_EditorUI.EndSection();

            HEU_EditorUI.DrawSeparator();
        }

        private void DrawInputNodesSection(HEU_HoudiniAsset asset, SerializedObject assetObject)
        {
            List<HEU_InputNode> inputNodes = asset.GetNonParameterInputNodes();
            if (inputNodes.Count > 0)
            {
                HEU_EditorUI.BeginSection();

                SerializedProperty showInputNodesProperty =
                    HEU_EditorUtility.GetSerializedProperty(assetObject, "_showInputNodesSection");
                if (showInputNodesProperty != null)
                {
                    showInputNodesProperty.boolValue =
                        HEU_EditorUI.DrawFoldOut(showInputNodesProperty.boolValue, "INPUT NODES");
                    if (showInputNodesProperty.boolValue)
                    {
                        foreach (HEU_InputNode inputNode in inputNodes)
                        {
                            HEU_InputNodeUI.EditorDrawInputNode(inputNode);

                            if (inputNodes.Count > 1)
                            {
                                HEU_EditorUI.DrawSeparator();
                            }
                        }
                    }

                    HEU_EditorUI.DrawSeparator();
                }

                HEU_EditorUI.EndSection();

                HEU_EditorUI.DrawSeparator();
            }
        }

        private void DrawSceneElements(HEU_HoudiniAsset asset)
        {
            if (asset == null || !asset.IsAssetValid())
            {
                return;
            }

            // Curve Editor
            if (asset.CurveEditorEnabled)
            {
                if (asset.GetEditableCurveCount() > 0)
                {
                    HEU_Curve[] curvesArray = asset.Curves.ToArray();
                    Editor.CreateCachedEditor(curvesArray, null, ref _curveEditor);
                    (_curveEditor as HEU_CurveUI).UpdateSceneCurves(asset);

                    bool bRequiresCook = !System.Array.TrueForAll(curvesArray,
                        c => c.EditState != HEU_Curve.CurveEditState.REQUIRES_GENERATION);
                    if (bRequiresCook && HEU_PluginSettings.CookingEnabled && asset.AutoCookOnParameterChange)
                    {
                        _houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false,
                            bUploadParameters: true);
                    }
                }
            }

            // Tools Editor
            if (asset.EditableNodesToolsEnabled)
            {
                List<HEU_AttributesStore> attributesStores = asset.AttributeStores;
                if (attributesStores.Count > 0)
                {
                    HEU_AttributesStore[] attributesStoresArray = attributesStores.ToArray();
                    Editor.CreateCachedEditor(attributesStoresArray, null, ref _toolsEditor);
                    HEU_ToolsUI toolsUI = (_toolsEditor as HEU_ToolsUI);
                    toolsUI.DrawToolsEditor(asset);

                    if (asset.ToolsInfo._liveUpdate && !asset.ToolsInfo._isPainting)
                    {
                        bool bAttributesDirty =
                            !System.Array.TrueForAll(attributesStoresArray, s => !s.AreAttributesDirty());
                        if (bAttributesDirty)
                        {
                            _houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false,
                                bSkipCookCheck: false, bUploadParameters: true);
                        }
                    }
                }
            }

            // Handles
            if (asset.HandlesEnabled)
            {
                List<HEU_Handle> handles = asset.GetHandles();
                if (handles.Count > 0)
                {
                    HEU_Handle[] handlesArray = handles.ToArray();
                    Editor.CreateCachedEditor(handlesArray, null, ref _handlesEditor);
                    HEU_HandlesUI handlesUI = (_handlesEditor as HEU_HandlesUI);
                    bool bHandlesChanged = handlesUI.DrawHandles(asset);

                    if (bHandlesChanged)
                    {
                        _houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false,
                            bUploadParameters: true);
                    }
                }
            }
        }

        private void DrawTerrainSection(HEU_HoudiniAsset asset, SerializedObject assetObject)
        {
            int numVolumes = asset.GetVolumeCacheCount();
            if (numVolumes <= 0)
            {
                return;
            }

            HEU_EditorUI.BeginSection();
            {
                SerializedProperty showTerrainProperty =
                    HEU_EditorUtility.GetSerializedProperty(assetObject, "_showTerrainSection");
                if (showTerrainProperty != null)
                {
                    showTerrainProperty.boolValue = HEU_EditorUI.DrawFoldOut(showTerrainProperty.boolValue, "TERRAIN");
                    if (showTerrainProperty.boolValue)
                    {
                        // Draw each volume layer
                        List<HEU_VolumeCache> volumeCaches = asset.VolumeCaches;
                        int numCaches = volumeCaches.Count;
                        for (int i = 0; i < numCaches; ++i)
                        {
                            SerializedObject cacheObjectSerialized = new SerializedObject(volumeCaches[i]);
                            bool bChanged = false;
                            bool bStrengthChanged = false;

                            SerializedProperty layersProperty = cacheObjectSerialized.FindProperty("_layers");
                            if (layersProperty == null || layersProperty.arraySize == 0)
                            {
                                continue;
                            }

                            string heading = string.Format("{0}-{1}:", volumeCaches[i].ObjectName,
                                volumeCaches[i].GeoName);

                            if (HEU_EditorUI.DrawFoldOutSerializedProperty(
                                    HEU_EditorUtility.GetSerializedProperty(cacheObjectSerialized, "_uiExpanded"),
                                    heading, ref bChanged))
                            {
                                EditorGUI.indentLevel++;

                                int numlayers = layersProperty.arraySize;
                                for (int j = 0; j < numlayers; ++j)
                                {
                                    SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(j);
                                    if (layerProperty == null)
                                    {
                                        continue;
                                    }

                                    // Skipping "height" layer on UI since its treated as Houdini-specific layer
                                    string layerName = layerProperty.FindPropertyRelative("_layerName").stringValue;
                                    if (layerName.Equals(HEU_Defines.HAPI_HEIGHTFIELD_LAYERNAME_HEIGHT))
                                    {
                                        continue;
                                    }

                                    layerName = string.Format("Layer: {0}", layerName);

                                    SerializedProperty uiExpandedProperty =
                                        layerProperty.FindPropertyRelative("_uiExpanded");
                                    bool bExpanded = uiExpandedProperty != null ? uiExpandedProperty.boolValue : true;
                                    bool bNewExpanded = HEU_EditorUI.DrawFoldOut(bExpanded, layerName);
                                    if (uiExpandedProperty != null && bExpanded != bNewExpanded)
                                    {
                                        bChanged = true;
                                        uiExpandedProperty.boolValue = bNewExpanded;
                                    }

                                    if (!bNewExpanded)
                                    {
                                        continue;
                                    }

                                    if (HEU_EditorUtility.EditorDrawFloatSliderProperty(layerProperty, "_strength",
                                            "Strength", "Amount to multiply the layer values by on import."))
                                    {
                                        bStrengthChanged = true;
                                    }

                                    HEU_EditorUI.DrawSeparator();
                                }

                                EditorGUI.indentLevel--;
                            }

                            if (bStrengthChanged)
                            {
                                SerializedProperty dirtyProperty = cacheObjectSerialized.FindProperty("_isDirty");
                                if (dirtyProperty != null)
                                {
                                    dirtyProperty.boolValue = true;
                                    bChanged = true;
                                }

                                if (HEU_PluginSettings.CookOnMouseUp && _houdiniAsset != null &&
                                    !HEU_EditorUtility.ReleasedMouse())
                                {
                                    _houdiniAsset.PendingAutoCookOnMouseRelease = true;
                                }
                            }

                            if (bChanged)
                            {
                                cacheObjectSerialized.ApplyModifiedProperties();
                            }
                        }
                    }
                }
            }
            HEU_EditorUI.EndSection();

            HEU_EditorUI.DrawSeparator();
        }
    }
} // HoudiniEngineUnity
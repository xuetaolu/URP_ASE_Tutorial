// @author : xue
// @created : 2024,04,07,14:08
// @desc:

// Decompiled with JetBrains decompiler
// Type: UnityEditor.AnimationWindow
// Assembly: UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99FEAB6F-3DB6-4074-9392-4FE4A179A95B
// Assembly location: D:\Program Files\Unity 2021.3.10f1\Editor\Data\Managed\UnityEngine\UnityEditor.CoreModule.dll
// XML documentation location: D:\Program Files\Unity 2021.3.10f1\Editor\Data\Managed\UnityEngine\UnityEditor.CoreModule.xml

using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
    /// <summary>
    ///   <para>Use the AnimationWindow class to select and edit Animation clips.</para>
    /// </summary>
    [EditorWindowTitle(title = "MyAnimation")]
    public sealed class MyAnimationWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/MyAnimationWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<MyAnimationWindow>();
            window.titleContent = new GUIContent("MyAnimationWindow");
            window.Show();
        }
        
        [MenuItem("Window/New MyAnimationWindow")]
        private static void NewWindow()
        {
            var window = CreateWindow<MyAnimationWindow>();
            window.titleContent = new GUIContent("MyAnimationWindow");
            window.Show();
        }
        
        private static List<MyAnimationWindow> s_AnimationWindows = new List<MyAnimationWindow>();
        private AnimEditor m_AnimEditor;

        [SerializeField]
        private EditorGUIUtility.EditorLockTracker m_LockTracker = new EditorGUIUtility.EditorLockTracker();

        [SerializeField] private int m_LastSelectedObjectID;
        private GUIStyle m_LockButtonStyle;
        private GUIContent m_DefaultTitleContent;
        private GUIContent m_RecordTitleContent;

        internal static List<MyAnimationWindow> GetAllMyAnimationWindows() => MyAnimationWindow.s_AnimationWindows;

        internal AnimEditor animEditor => this.m_AnimEditor;

        internal AnimationWindowState state => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null
            ? this.m_AnimEditor.state
            : (AnimationWindowState)null;

        /// <summary>
        ///   <para>The animation clip selected in the Animation window.</para>
        /// </summary>
        public AnimationClip animationClip
        {
            get => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null
                ? this.m_AnimEditor.state.activeAnimationClip
                : (AnimationClip)null;
            set
            {
                if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                    return;
                this.m_AnimEditor.state.activeAnimationClip = value;
            }
        }

        /// <summary>
        ///   <para>This property toggles previewing in the Animation window.</para>
        /// </summary>
        public bool previewing
        {
            get => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null &&
                   this.m_AnimEditor.state.previewing;
            set
            {
                if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                    return;
                if (value)
                    this.m_AnimEditor.state.StartPreview();
                else
                    this.m_AnimEditor.state.StopPreview();
            }
        }

        /// <summary>
        ///   <para>True if Animation window can enable preview mode. False otherwise. (Read Only)</para>
        /// </summary>
        public bool canPreview => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null &&
                                  this.m_AnimEditor.state.canPreview;

        /// <summary>
        ///   <para>This property toggles recording in the Animation window.</para>
        /// </summary>
        public bool recording
        {
            get => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null &&
                   this.m_AnimEditor.state.recording;
            set
            {
                if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                    return;
                if (value)
                    this.m_AnimEditor.state.StartRecording();
                else
                    this.m_AnimEditor.state.StopRecording();
            }
        }

        /// <summary>
        ///   <para>True if Animation window can enable recording mode. False otherwise. (Read Only)</para>
        /// </summary>
        public bool canRecord => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null &&
                                 this.m_AnimEditor.state.canRecord;

        /// <summary>
        ///   <para>This property toggles animation playback in the Animation window.</para>
        /// </summary>
        public bool playing
        {
            get => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null && this.m_AnimEditor.state.playing;
            set
            {
                if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                    return;
                if (value)
                    this.m_AnimEditor.state.StartPlayback();
                else
                    this.m_AnimEditor.state.StopPlayback();
            }
        }

        /// <summary>
        ///   <para>The time value at which the Animation window playhead is located.</para>
        /// </summary>
        public float time
        {
            get => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null
                ? this.m_AnimEditor.state.currentTime
                : 0.0f;
            set
            {
                if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                    return;
                this.m_AnimEditor.state.currentTime = value;
            }
        }

        /// <summary>
        ///   <para>The frame number at which the Animation window playhead is located.</para>
        /// </summary>
        public int frame
        {
            get => (UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null
                ? this.m_AnimEditor.state.currentFrame
                : 0;
            set
            {
                if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                    return;
                this.m_AnimEditor.state.currentFrame = value;
            }
        }

        private MyAnimationWindow()
        {
        }

        internal void ForceRefresh()
        {
            if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                return;
            this.m_AnimEditor.state.ForceRefresh();
        }

        private void OnEnable()
        {
            if ((UnityEngine.Object)this.m_AnimEditor == (UnityEngine.Object)null)
            {
                this.m_AnimEditor = ScriptableObject.CreateInstance(typeof(AnimEditor)) as AnimEditor;
                this.m_AnimEditor.hideFlags = HideFlags.HideAndDontSave;
            }

            MyAnimationWindow.s_AnimationWindows.Add(this);
            this.titleContent = this.GetLocalizedTitleContent();
            this.m_DefaultTitleContent = this.titleContent;
            this.m_RecordTitleContent =
                EditorGUIUtility.TextContentWithIcon(this.titleContent.text, "Animation.Record");
            this.OnSelectionChange();
            Undo.undoRedoPerformed += new Undo.UndoRedoCallback(this.UndoRedoPerformed);
        }

        private void OnDisable()
        {
            MyAnimationWindow.s_AnimationWindows.Remove(this);
            this.m_AnimEditor.OnDisable();
            Undo.undoRedoPerformed -= new Undo.UndoRedoCallback(this.UndoRedoPerformed);
        }

        private void OnDestroy() => UnityEngine.Object.DestroyImmediate((UnityEngine.Object)this.m_AnimEditor);

        private void Update()
        {
            if ((UnityEngine.Object)this.m_AnimEditor == (UnityEngine.Object)null)
                return;
            this.m_AnimEditor.Update();
        }

        private void OnGUI()
        {
            if ((UnityEngine.Object)this.m_AnimEditor == (UnityEngine.Object)null)
                return;
            this.titleContent = this.m_AnimEditor.state.recording
                ? this.m_RecordTitleContent
                : this.m_DefaultTitleContent;
            this.m_AnimEditor.OnAnimEditorGUI((EditorWindow)this, this.position);
        }

        internal void OnSelectionChange()
        {
            if ((UnityEngine.Object)this.m_AnimEditor == (UnityEngine.Object)null)
                return;
            UnityEngine.Object activeObject = Selection.activeObject;
            bool flag = false;
            if (this.m_LockTracker.isLocked && this.m_AnimEditor.stateDisabled)
            {
                activeObject = EditorUtility.InstanceIDToObject(this.m_LastSelectedObjectID);
                flag = true;
                this.m_LockTracker.isLocked = false;
            }

            GameObject gameObject = activeObject as GameObject;
            if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
            {
                this.EditGameObject(gameObject);
            }
            else
            {
                Transform transform = activeObject as Transform;
                if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
                {
                    this.EditGameObject(transform.gameObject);
                }
                else
                {
                    AnimationClip animationClip = activeObject as AnimationClip;
                    if ((UnityEngine.Object)animationClip != (UnityEngine.Object)null)
                        this.EditAnimationClip(animationClip);
                }
            }

            if (!flag || this.m_AnimEditor.stateDisabled)
                return;
            this.m_LockTracker.isLocked = true;
        }

        private void OnFocus() => this.OnSelectionChange();

        internal void OnControllerChange() => this.OnSelectionChange();

        private void OnLostFocus()
        {
            if (!((UnityEngine.Object)this.m_AnimEditor != (UnityEngine.Object)null))
                return;
            this.m_AnimEditor.OnLostFocus();
        }

        [UnityEditor.Callbacks.OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (!(bool)(UnityEngine.Object)(EditorUtility.InstanceIDToObject(instanceID) as AnimationClip))
                return false;
            EditorWindow.GetWindow<AnimationWindow>();
            return true;
        }

        internal bool EditGameObject(GameObject gameObject) =>
            this.EditGameObjectInternal(gameObject, (IAnimationWindowControl)null);

        internal bool EditAnimationClip(AnimationClip animationClip)
        {
            if (this.state.linkedWithSequencer)
                return false;
            this.EditAnimationClipInternal(animationClip, (UnityEngine.Object)null, (IAnimationWindowControl)null);
            return true;
        }

        internal bool EditSequencerClip(
            AnimationClip animationClip,
            UnityEngine.Object sourceObject,
            IAnimationWindowControl controlInterface)
        {
            this.EditAnimationClipInternal(animationClip, sourceObject, controlInterface);
            this.state.linkedWithSequencer = true;
            return true;
        }

        internal void UnlinkSequencer()
        {
            if (!this.state.linkedWithSequencer)
                return;
            this.state.linkedWithSequencer = false;
            this.EditAnimationClip((AnimationClip)null);
            this.OnSelectionChange();
        }

        private bool EditGameObjectInternal(
            GameObject gameObject,
            IAnimationWindowControl controlInterface)
        {
            if (EditorUtility.IsPersistent((UnityEngine.Object)gameObject) ||
                (gameObject.hideFlags & HideFlags.NotEditable) != 0)
                return false;
            GameObjectSelectionItem selectedItem = GameObjectSelectionItem.Create(gameObject);
            if (this.ShouldUpdateGameObjectSelection(selectedItem))
            {
                this.m_AnimEditor.selection = (AnimationWindowSelectionItem)selectedItem;
                this.m_AnimEditor.overrideControlInterface = controlInterface;
                this.m_LastSelectedObjectID = (UnityEngine.Object)gameObject != (UnityEngine.Object)null
                    ? gameObject.GetInstanceID()
                    : 0;
            }
            else
                this.m_AnimEditor.OnSelectionUpdated();

            return true;
        }

        private void EditAnimationClipInternal(
            AnimationClip animationClip,
            UnityEngine.Object sourceObject,
            IAnimationWindowControl controlInterface)
        {
            AnimationClipSelectionItem selectedItem = AnimationClipSelectionItem.Create(animationClip, sourceObject);
            if (this.ShouldUpdateSelection((AnimationWindowSelectionItem)selectedItem))
            {
                this.m_AnimEditor.selection = (AnimationWindowSelectionItem)selectedItem;
                this.m_AnimEditor.overrideControlInterface = controlInterface;
                this.m_LastSelectedObjectID = (UnityEngine.Object)animationClip != (UnityEngine.Object)null
                    ? animationClip.GetInstanceID()
                    : 0;
            }
            else
                this.m_AnimEditor.OnSelectionUpdated();
        }

        private void ShowButton(Rect r)
        {
            if (this.m_LockButtonStyle == null)
                this.m_LockButtonStyle = (GUIStyle)"IN LockButton";
            EditorGUI.BeginChangeCheck();
            this.m_LockTracker.ShowButton(r, this.m_LockButtonStyle, this.m_AnimEditor.stateDisabled);
            if (!EditorGUI.EndChangeCheck())
                return;
            this.OnSelectionChange();
        }

        private bool ShouldUpdateGameObjectSelection(GameObjectSelectionItem selectedItem)
        {
            if (this.m_LockTracker.isLocked || this.state.linkedWithSequencer)
                return false;
            if ((UnityEngine.Object)selectedItem.rootGameObject == (UnityEngine.Object)null)
                return true;
            AnimationWindowSelectionItem currentSelection = this.m_AnimEditor.selection;
            return (UnityEngine.Object)selectedItem.rootGameObject !=
                   (UnityEngine.Object)currentSelection.rootGameObject ||
                   (UnityEngine.Object)currentSelection.animationClip == (UnityEngine.Object)null ||
                   (UnityEngine.Object)currentSelection.rootGameObject != (UnityEngine.Object)null &&
                   !Array.Exists<AnimationClip>(AnimationUtility.GetAnimationClips(currentSelection.rootGameObject),
                       (Predicate<AnimationClip>)(x =>
                           (UnityEngine.Object)x == (UnityEngine.Object)currentSelection.animationClip));
        }

        private bool ShouldUpdateSelection(AnimationWindowSelectionItem selectedItem)
        {
            if (this.m_LockTracker.isLocked)
                return false;
            AnimationWindowSelectionItem selection = this.m_AnimEditor.selection;
            return selectedItem.GetRefreshHash() != selection.GetRefreshHash();
        }

        private void UndoRedoPerformed() => this.Repaint();

        public void AddItemsToMenu(GenericMenu menu) =>
            this.m_LockTracker.AddItemsToMenu(menu, this.m_AnimEditor.stateDisabled);
    }
}
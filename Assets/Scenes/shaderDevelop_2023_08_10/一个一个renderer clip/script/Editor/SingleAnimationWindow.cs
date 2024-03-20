// @author : xue
// @created : 2024,02,26,18:02
// @desc:

using System;
using Common.bridge;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Timeline
{
    public class SingleAnimationWindow : EditorWindow
    {
        static AnimationModeDriver s_PreviewDriver;
        public static AnimationModeDriver previewDriver
        {
            get
            {
                if (s_PreviewDriver == null)
                    s_PreviewDriver = ScriptableObject.CreateInstance<AnimationModeDriver>();
                return s_PreviewDriver;
            }
        }
        
        protected GameObject go;
        protected AnimationClip animationClip;
        protected float time = 0.0f;
        protected bool lockSelection = false;
        protected bool animationMode = false;
        
        [MenuItem("Window/SingleAnimationWindows")]
        private static void ShowWindow()
        {
            var window = GetWindow<SingleAnimationWindow>();
            window.titleContent = new GUIContent("SingleAnimationWindows");
            window.Show();
        }

        // Has a GameObject been selection?
        public void OnSelectionChange()
        {
            if (!lockSelection)
            {
                go = Selection.activeGameObject;
                Repaint();
            }
        }

        // Main editor window
        public void OnGUI()
        {
            // Wait for user to select a GameObject
            if (go == null)
            {
                EditorGUILayout.HelpBox("Please select a GameObject", MessageType.Info);
                return;
            }

            // Animate and Lock buttons.  Check if Animate has changed
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool inAnimationMode = AnimationMode.InAnimationMode(previewDriver);
            GUILayout.Toggle(inAnimationMode, "Animate");
            if (EditorGUI.EndChangeCheck())
                ToggleAnimationMode();

            GUILayout.FlexibleSpace();
            lockSelection = GUILayout.Toggle(lockSelection, "Lock");
            GUILayout.EndHorizontal();

            // Slider to use when Animate has been ticked
            EditorGUILayout.BeginVertical();
            animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
            if (animationClip != null)
            {
                float startTime = 0.0f;
                float stopTime  = animationClip.length;
                time = EditorGUILayout.Slider(time, startTime, stopTime);
            }
            else if (AnimationMode.InAnimationMode(previewDriver)) 
                AnimationMode.StopAnimationMode(previewDriver);
            EditorGUILayout.EndVertical();
        }

        void Update()
        {
            if (go == null)
                return;

            if (animationClip == null)
                return;

            // Animate the GameObject
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode(previewDriver))
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(go, animationClip, time);
                AnimationMode.EndSampling();

                SceneView.RepaintAll();
            }
        }

        void ToggleAnimationMode()
        {
            bool inAnimationMode2 = AnimationMode.InAnimationMode(previewDriver);
            if (AnimationMode.InAnimationMode(previewDriver))
            {
                AnimationMode.StopAnimationMode(previewDriver);
            }
            else
            {
                AnimationMode.StartAnimationMode(previewDriver);
                bool inAnimationMode0 = AnimationMode.InAnimationMode(previewDriver);
                AnimationModeBridge.InitializePropertyModificationForGameObject(go, animationClip);
                AnimationWindow animationWindow = GetWindow<AnimationWindow>();
                // animationWindow.animationClip = animationClip;
                // TimelineAnimatio
                
                bool inAnimationMode = AnimationMode.InAnimationMode(previewDriver);
            }
        }

        private void OnDisable()
        {
            if (AnimationMode.InAnimationMode(previewDriver))
                AnimationMode.StopAnimationMode(previewDriver);
        }
    }
}
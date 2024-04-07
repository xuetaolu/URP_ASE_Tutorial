// @author : xue
// @created : 2024,04,07,14:19
// @desc:

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Common.bridge
{
    public class DummyAnimationClipSource : MonoBehaviour, IAnimationClipSource, IAnimationWindowPreview
    {
        private static List<AnimationClip> s_EmptyList = new List<AnimationClip>();

        public AnimationClip clip;

        private static MethodInfo s_GetAllAnimationWindowsMethod;
        private static List<AnimationWindow> GetAllAnimationWindows()
        {
            // return AnimationModeBridge.GetAllAnimationWindows();
            
            if (s_GetAllAnimationWindowsMethod == null)
                s_GetAllAnimationWindowsMethod = typeof(AnimationWindow).GetMethod("GetAllAnimationWindows",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            return s_GetAllAnimationWindowsMethod.Invoke(null, null) as List<AnimationWindow>;
        }

        public void GetAnimationClips(List<AnimationClip> results)
        {
            if (clip != null)
                results.Add(clip);
        }

        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            EditorApplication.update += SyncAnimationWindow;
        }

        public static void SyncAnimationWindow()
        {
            AnimationWindow previewingTarget = null;
            foreach (AnimationWindow animationWindow in GetAllAnimationWindows())
            {
                if (animationWindow.previewing)
                {
                    previewingTarget = animationWindow;
                    break;
                }

            }

            if (previewingTarget == null)
                return;
            
            foreach (AnimationWindow other in GetAllAnimationWindows())
            {
                if (other == previewingTarget)
                    continue;
                if (!Mathf.Approximately(other.time, previewingTarget.time))
                {
                    other.time = previewingTarget.time;
                    other.Repaint();
                }
            }
        }
        #endif
        
        #region IAnimationWindowPreview

        private List<PlayableGraph> otherPreviewGraphs = new List<PlayableGraph>();
        
        public void StartPreview()
        {
            
        }

        public void StopPreview()
        {
            foreach (PlayableGraph playableGraph in otherPreviewGraphs)
            {
                if (playableGraph.IsValid())
                    playableGraph.Destroy();
            }
            otherPreviewGraphs.Clear();
        }

        public static float GetPreviewingTime()
        {
            foreach (AnimationWindow myAnimationWindow in GetAllAnimationWindows())
            {
                if (myAnimationWindow.previewing)
                {
                    return myAnimationWindow.time;
                }
            }

            return -1;
        }
        
        public void UpdatePreviewGraph(PlayableGraph graph)
        {
            // 其他预览 playableGraph 更新一下
            foreach (PlayableGraph playableGraph in otherPreviewGraphs)
            {
                float time = GetPreviewingTime();
                AnimationMode.SamplePlayableGraph(playableGraph, 0, time);
            }
        }

        public Playable BuildPreviewGraph(PlayableGraph graph, Playable graphRoot)
        {
            // 确保清除了全部
            StopPreview();

            DummyAnimationClipSource[] others = UnityEngine.Object.FindObjectsOfType<DummyAnimationClipSource>(true);

            foreach (DummyAnimationClipSource other in others)
            {
                if (other == this)
                    continue;
                PlayableGraph playableGraph = default;
                bool success = other.OnOtherCallBuildPreviewGraph(ref playableGraph);
                if (success)
                    otherPreviewGraphs.Add(playableGraph);
                    
            }
            
            // 不修改当前的预览 playableGraph
            return graphRoot;
        }


        public bool OnOtherCallBuildPreviewGraph(ref PlayableGraph playableGraph)
        {
            if (clip == null)
                return false;
            
            Animator animator = GetComponent<Animator>();
            if (animator == null)
                return false;

            Playable rootPlayable = Playable.Null;

            playableGraph = PlayableGraph.Create($"PreviewGraph-{gameObject.name}");

            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            
            AnimationLayerMixerPlayable mixerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, 1);

            rootPlayable = mixerPlayable;
            
            int num = 0;
            mixerPlayable.ConnectInput(num++, clipPlayable, 0, 1);
            
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(playableGraph, "ouput", animator);
            output.SetSourcePlayable<AnimationPlayableOutput, Playable>(rootPlayable);
            output.SetWeight<AnimationPlayableOutput>(1.0f);

            return true;
        }

        #endregion
    }
}
// @author : xue
// @created : 2024,04,07,14:19
// @desc:

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Common.bridge
{
    public class DummyAnimationClipSource : MonoBehaviour
        #if UNITY_EDITOR
        , IAnimationClipSource, IAnimationWindowPreview
        #endif
    {
        private static List<AnimationClip> s_EmptyList = new List<AnimationClip>();

        public AnimationClip clip;

        public string clipAssetFolder;

        // private AnimationClip _actualClip;
        
        private AnimationClip _getAnimationClip()
        {
            if (clip != null)
                return clip;
            
            #if UNITY_EDITOR
            AnimationClip _clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(clipAssetFolder, gameObject.name) + ".anim");

            if (_clip != null)
            {
                return _clip;
            }
            #endif

            return null;
        }
        

        #if UNITY_EDITOR

        #region GetAllAnimationWindows

        private static MethodInfo s_GetAllAnimationWindowsMethod;
        private static List<UnityEditor.AnimationWindow> GetAllAnimationWindows()
        {
            // return AnimationModeBridge.GetAllAnimationWindows();
            
            if (s_GetAllAnimationWindowsMethod == null)
                s_GetAllAnimationWindowsMethod = typeof(UnityEditor.AnimationWindow).GetMethod("GetAllAnimationWindows",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            return s_GetAllAnimationWindowsMethod.Invoke(null, null) as List<UnityEditor.AnimationWindow>;
        }
        
        #endregion
        
        #region IAnimationClipSource
        public void GetAnimationClips(List<AnimationClip> results)
        {
            AnimationClip _clip = _getAnimationClip();
            if (_clip != null)
                results.Add(_clip);
        }

        #endregion
        
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
            foreach (UnityEditor.AnimationWindow myAnimationWindow in GetAllAnimationWindows())
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
                UnityEditor.AnimationMode.SamplePlayableGraph(playableGraph, 0, time);
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
            AnimationClip _clip = _getAnimationClip();
            
            if (_clip == null)
                return false;
            
            Animator animator = GetComponent<Animator>();
            if (animator == null)
                return false;

            Playable rootPlayable = Playable.Null;

            playableGraph = PlayableGraph.Create($"PreviewGraph-{gameObject.name}");

            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(playableGraph, _clip);
            
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
        
        #region InitializeOnLoadMethod

        [UnityEditor.InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            UnityEditor.EditorApplication.update += SyncAnimationWindow;
        }

        public static void SyncAnimationWindow()
        {
            UnityEditor.AnimationWindow previewingTarget = null;
            foreach (UnityEditor.AnimationWindow animationWindow in GetAllAnimationWindows())
            {
                if (animationWindow.previewing)
                {
                    previewingTarget = animationWindow;
                    break;
                }

            }

            if (previewingTarget == null)
                return;
            
            foreach (UnityEditor.AnimationWindow other in GetAllAnimationWindows())
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
        
        #endregion
        
        #endif
    }
}
// @author : xue
// @created : 2024,03,20,10:46
// @desc:

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Common.bridge
{
    public class AnimationModeBridge
    {
        public static void InitializePropertyModificationForGameObject(GameObject gameObject, AnimationClip clip)
        {
            AnimationMode.InitializePropertyModificationForGameObject(gameObject, clip);
        }

        public static void InitializePropertyModificationForObject(UnityEngine.Object target, AnimationClip clip)
        {
            AnimationMode.InitializePropertyModificationForObject(target, clip);
        }
        
        public static List<AnimationWindow> GetAllAnimationWindows() => AnimationWindow.GetAllAnimationWindows();
    }
}
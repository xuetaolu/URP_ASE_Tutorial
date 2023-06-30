// @author : xue
// @created : 2023,06,30,14:14
// @desc:

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common
{
    public static class objcleaner
    {
        public interface IDestroy
        {
            void Destroy(); 
        }
        
        public static bool ShouldDestroyImmediate
        {
            get
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying) return true;
#endif
                return false;
            }
        }
        
        public static void Destroy(GameObject go)
        {
            DestroyInner(go);
        }
        public static void Destroy(ref GameObject go)
        {
            DestroyInner(go);
            go = null;
        }
        
        public static void Destroy(Component comp, bool destroy_go)
        {
            if (comp != null)
            {
                var obj = destroy_go ? (Object)comp.gameObject : (Object)comp;
                DestroyInner(obj);
            }
        }
        
        public static void Destroy(Object obj)
        {
            if (obj is Component) throw new Exception("销毁 Component 请使用 Destroy(comp, bool)");
            DestroyInner(obj);
        }
        
        static void DestroyInner(Object obj)
        {
            if (obj == null) return;
            if (obj is IDestroy x)
            {
                x.Destroy();
                return;
            }
            if (ShouldDestroyImmediate)
            {
                Object.DestroyImmediate(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }
    }
}
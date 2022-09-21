// @author : xue
// @created : 2022,08,29,14:54
// @desc:

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace URPExtend.Scripts.Runtime.RendererFeature
{
    [Serializable]
    public class CallbackRendererFeature : ScriptableRendererFeature
    {
        public delegate void AddRendererPassesEvent(ScriptableRenderer s, ref RenderingData r);

        public static event AddRendererPassesEvent s_addRendererPassesEvent;
        public override void Create()
        {
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            s_addRendererPassesEvent?.Invoke(renderer, ref renderingData);
        }
    }
}
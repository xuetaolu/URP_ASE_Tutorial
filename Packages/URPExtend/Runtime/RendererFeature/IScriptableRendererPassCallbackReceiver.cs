// @author : xue
// @created : 2022,08,29,15:26
// @desc:

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URPExtend.Scripts.Runtime.RendererFeature
{
    public interface IScriptableRendererPassCallbackReceiver
    {
        void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd);
        
    }
}
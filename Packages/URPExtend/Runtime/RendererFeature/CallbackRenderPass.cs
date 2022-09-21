// @author : xue
// @created : 2022,08,29,14:57
// @desc:

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URPExtend.Scripts.Runtime.RendererFeature
{
    
    public class CallbackRenderPass : ScriptableRenderPass
    {
        private IScriptableRendererPassCallbackReceiver m_callbackReceiver;

        public IScriptableRendererPassCallbackReceiver callbackReceiver {
            get => m_callbackReceiver;
            set => m_callbackReceiver = value;
        }

        public CallbackRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques - 10;
            profilingSampler = new ProfilingSampler(nameof(CallbackRenderPass));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                m_callbackReceiver?.OnExecute(context, ref renderingData, cmd);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        
    }
}
// @author : xue
// @created : 2022,09,15,15:48
// @desc:

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URPExtend.Scripts.Runtime.ClipPostProcessOutline.pass
{
    public class DrawOutlinePass : ScriptableRenderPass
    {
        private Material m_drawOutlineMaterial;

        private RenderTargetHandle m_renderTargetHandle;
        
        public static readonly int s_FillColorTexturePropertyId = Shader.PropertyToID("_FillColorTexture");

        public static readonly String  s_ScreenRectPropertyName = "_ScreenRect";
        public static readonly int s_ScreenRectPropertyId = Shader.PropertyToID(s_ScreenRectPropertyName);
        
        
        private Vector4 m_ViewportRect = new Vector4(1, 1, 0, 0);

        private ScriptableRenderer m_renderer;
        
        public ScriptableRenderer renderer
        {
            get => m_renderer;
            set => m_renderer = value;
        }

        public Material drawOutlineMaterial
        {
            get => m_drawOutlineMaterial;
            set => m_drawOutlineMaterial = value;
        }

        public Vector4 viewportRect
        {
            get => m_ViewportRect;
            set => m_ViewportRect = value;
        }

        public DrawOutlinePass( RenderTargetHandle renderTargetHandle )
        {
            m_renderTargetHandle = renderTargetHandle;
            
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents + 1;
            profilingSampler = new ProfilingSampler(nameof(DrawOutlinePass));
            
        }
        
        

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                CoreUtils.SetRenderTarget(cmd, renderer.cameraColorTarget);
                m_drawOutlineMaterial.SetVector(s_ScreenRectPropertyId, m_ViewportRect);
                cmd.SetGlobalTexture(s_FillColorTexturePropertyId, m_renderTargetHandle.Identifier());
                cmd.DrawProcedural(Matrix4x4.identity, m_drawOutlineMaterial, 0, MeshTopology.Quads, 4, 1, null);

            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
        }
    }
}
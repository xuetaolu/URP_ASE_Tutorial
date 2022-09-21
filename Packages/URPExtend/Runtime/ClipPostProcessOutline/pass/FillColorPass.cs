// @author : xue
// @created : 2022,09,15,14:06
// @desc:

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URPExtend.Scripts.Runtime.ClipPostProcessOutline.pass
{
    public class FillColorPass : ScriptableRenderPass
    {
        private List<Renderer> m_rendererses;
        private Material m_fillColorMaterial;

        private RenderTargetHandle m_renderTargetHandle;

        private Vector2Int m_textureSize = new Vector2Int(512, 512);
        private Vector4 m_ViewportRect = new Vector4(1, 1, 0, 0);
        public static readonly String  s_ScreenRectPropertyName = "_ScreenRect";
        public static readonly int s_ScreenRectPropertyId = Shader.PropertyToID(s_ScreenRectPropertyName);

        public List<Renderer> renderers
        {
            get => m_rendererses;
            set => m_rendererses = value;
        }
        public Material fillColorMaterial
        {
            get => m_fillColorMaterial;
            set => m_fillColorMaterial = value;
        }

        public Vector2Int textureSize
        {
            get => m_textureSize;
            set => m_textureSize = value;
        }

        public Vector4 viewportRect
        {
            get => m_ViewportRect;
            set => m_ViewportRect = value;
        }

        public FillColorPass( RenderTargetHandle renderTargetHandle )
        {
            m_renderTargetHandle = renderTargetHandle;
            
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents + 1;
            profilingSampler = new ProfilingSampler(nameof(FillColorPass));
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;
            var (width, height) = (descriptor.width, descriptor.height);
            descriptor.width = Mathf.Max(64, m_textureSize.x);
            descriptor.height = Mathf.Max(64, m_textureSize.y);
            descriptor.colorFormat = RenderTextureFormat.R8;
            
            
            base.OnCameraSetup(cmd, ref renderingData);
            cmd.GetTemporaryRT(m_renderTargetHandle.id, descriptor, FilterMode.Bilinear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                CoreUtils.SetRenderTarget(cmd, m_renderTargetHandle.Identifier(), ClearFlag.All, Color.black);
                
                m_fillColorMaterial.SetVector(s_ScreenRectPropertyId, m_ViewportRect);
                for (int i = 0; i < m_rendererses.Count; i++)
                {
                    var item = m_rendererses[i];
                    if (item == null)
                        continue;
                    cmd.DrawRenderer(item, m_fillColorMaterial, 0, 0);
                }
                
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            cmd.ReleaseTemporaryRT(m_renderTargetHandle.id);
        }
    }
}
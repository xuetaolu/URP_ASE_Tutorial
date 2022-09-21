// @author : xue
// @created : 2022,09,15,14:41
// @desc:

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URPExtend.Scripts.Runtime.ClipPostProcessOutline.pass
{
    public class BlurColorPass : ScriptableRenderPass
    {
        private Material m_blurColorMaterial;
        private Material m_blurColorMaterial2;

        private RenderTargetHandle m_renderTargetHandle;

        private float m_blurPixelSize = 1.0f;
        private Vector2Int m_textureSize = new Vector2Int(512, 512);
        public static readonly int s_MainTexPropertyId = Shader.PropertyToID("_MainTex");
        public static readonly int s_UVDeltaPropertyId = Shader.PropertyToID("_UV_Delta");

        private RenderTargetHandle m_tempRenderTargetHandle;
        
        public Material blurColorMaterial
        {
            get => m_blurColorMaterial;
            set => m_blurColorMaterial = value;
        }

        public Material blurColorMaterial2
        {
            get => m_blurColorMaterial2;
            set => m_blurColorMaterial2 = value;
        }

        public Vector2Int textureSize
        {
            get => m_textureSize;
            set => m_textureSize = value;
        }

        public float blurPixelSize
        {
            get => m_blurPixelSize;
            set => m_blurPixelSize = value;
        }


        public BlurColorPass( RenderTargetHandle renderTargetHandle )
        {
            m_renderTargetHandle = renderTargetHandle;
            
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents + 1;
            profilingSampler = new ProfilingSampler(nameof(BlurColorPass));
            
            m_tempRenderTargetHandle.Init("_TempRenderTargetHandle");
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
            cmd.GetTemporaryRT(m_tempRenderTargetHandle.id, descriptor, FilterMode.Bilinear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                Vector4 firstBlurVector4 = new Vector4(m_blurPixelSize / m_textureSize.x, 0, 0, 0);
                Vector4 secondBlurVector4 = new Vector4(0, m_blurPixelSize / m_textureSize.y, 0, 0);
                
                // CoreUtils.SetRenderTarget(cmd, m_tempRenderTargetHandle.Identifier());
                cmd.SetRenderTarget(m_tempRenderTargetHandle.Identifier(), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd.SetGlobalTexture(s_MainTexPropertyId, m_renderTargetHandle.Identifier());
                m_blurColorMaterial.SetVector(s_UVDeltaPropertyId, firstBlurVector4);
                cmd.DrawProcedural(Matrix4x4.identity, m_blurColorMaterial, 0, MeshTopology.Quads, 4, 1, null);

                // CoreUtils.SetRenderTarget(cmd, m_renderTargetHandle.Identifier());
                cmd.SetRenderTarget(m_renderTargetHandle.Identifier(), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd.SetGlobalTexture(s_MainTexPropertyId, m_tempRenderTargetHandle.Identifier());
                m_blurColorMaterial2.SetVector(s_UVDeltaPropertyId, secondBlurVector4);
                cmd.DrawProcedural(Matrix4x4.identity, m_blurColorMaterial2, 0, MeshTopology.Quads, 4, 1, null);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            cmd.ReleaseTemporaryRT(m_tempRenderTargetHandle.id);
        }
    }
}
// @author : xue
// @created : 2023,09,19,15:30
// @desc:

using System;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Scenes.shaderDevelop_2023_08_10.原神水底焦散.sub.water_ssr.script
{
    public class GenshipSSRRenderFeature : ScriptableRendererFeature
    {
        private Shader ssrShader => Shader.Find("genship/water_ssr_v2");
        
        private Material _ssrMaterial;
        
        private Material ssrMaterial
        {
            get
            {
                if (_ssrMaterial == null)
                {
                    _ssrMaterial = new Material(ssrShader);
                    _ssrMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
        
                return _ssrMaterial;
            }
        }
        
        private Shader ssrProcessShader => Shader.Find("genship/water_ssr_process");
        
        private Material _ssrProcessMaterial;
        
        private Material ssrProcessMaterial
        {
            get
            {
                if (_ssrProcessMaterial == null)
                {
                    Material mat = new Material(ssrProcessShader);
                    mat.hideFlags = HideFlags.HideAndDontSave;
                    _ssrProcessMaterial = mat;
                }
        
                return _ssrProcessMaterial;
            }
        }
        
        private GenshipSSRRenderPass m_GenshipSSRRenderPass;
        public override void Create()
        {
            m_GenshipSSRRenderPass = new GenshipSSRRenderPass();
            m_GenshipSSRRenderPass.m_ssrMaterial = ssrMaterial;
            m_GenshipSSRRenderPass.m_ssrProcessMatrial = ssrProcessMaterial;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            UniversalRenderer universalRenderer = renderer as UniversalRenderer;
            if (universalRenderer == null)
                return;
            // m_GenshipSSRRenderPass.m_ActiveCameraDepthAttachment = universalRenderer.activeCameraDepthAttachment;
            renderer.EnqueuePass(m_GenshipSSRRenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            objcleaner.Destroy(_ssrMaterial);
            objcleaner.Destroy(_ssrProcessMaterial);
            base.Dispose(disposing);
        }
    }

    public class GenshipSSRRenderPass : ScriptableRenderPass
    {
        // public RenderTargetHandle m_ActiveCameraDepthAttachment;
        public Material m_ssrMaterial;
        public Material m_ssrProcessMatrial;
        
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        
        private RenderTargetHandle m_ssrTextureHandle0;
        private RenderTargetHandle m_ssrTextureHandle;
        private RenderTargetHandle m_downSampleDepthTextureAsBufferHandle;
        private RenderTargetHandle m_downSampleDepthLinear01TextureHandle;

        public GenshipSSRRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 1;
            
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
            
            m_ssrTextureHandle0.Init("_ssrTexture0");
            m_ssrTextureHandle.Init("_GlobalSSRTexture");
            m_downSampleDepthTextureAsBufferHandle.Init("_downSampleDepthTextureAsBuffer");
            m_downSampleDepthLinear01TextureHandle.Init("_downSampleDepthLinear01Texture");
            

            profilingSampler = new ProfilingSampler(nameof(GenshipSSRRenderPass));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                Camera camera = renderingData.cameraData.camera;
                int width  = camera.pixelWidth / 2;
                int height = camera.pixelHeight / 2;
                RenderTextureFormat format = RenderTextureUtils.GetSupportedFormat(camera.allowHDR, true);
                
                RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height, format);
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;

                // 有窟窿的 ssr0 贴图
                cmd.GetTemporaryRT(m_ssrTextureHandle0.id, desc, FilterMode.Bilinear);
                
                // 填充窟窿的 ssr1 贴图
                cmd.GetTemporaryRT(m_ssrTextureHandle.id, desc, FilterMode.Bilinear);
                
                // 降分辨率抓深度，用于生成 Linear01Depth 贴图，在 ssr0 贴图生成时作为 深度缓冲
                desc.colorFormat = RenderTextureFormat.Depth;
                desc.depthBufferBits = 24;
                cmd.GetTemporaryRT(m_downSampleDepthTextureAsBufferHandle.id, desc, FilterMode.Point);
                
                // Linear01 到 R 贴图，用于 ssr0 贴图生成的采样贴图
                desc.colorFormat = RenderTextureFormat.RFloat;
                desc.depthBufferBits = 0;
                cmd.GetTemporaryRT(m_downSampleDepthLinear01TextureHandle.id, desc, FilterMode.Point);
                
                // 生成降分辨率的 depthTexture
                cmd.Blit(null, m_downSampleDepthTextureAsBufferHandle.Identifier(), m_ssrProcessMatrial, m_ssrProcessMatrial.FindPass("GetFarthestDepth"));
                
                // 生成降分辨率的 Linear01DepthTexture 原神是这样的，当然直接在 ssr shader里计算 Linear 也可以，我认为
                cmd.SetGlobalTexture("_CameraDepthTextureDownSize", m_downSampleDepthTextureAsBufferHandle.id);
                cmd.Blit(null, m_downSampleDepthLinear01TextureHandle.Identifier(), m_ssrProcessMatrial, m_ssrProcessMatrial.FindPass("Linear01Depth"));
                cmd.SetGlobalTexture("_CameraDepthTextureLinear01DownSize", m_downSampleDepthLinear01TextureHandle.id);
                
                // 渲染水
                cmd.SetRenderTarget(m_ssrTextureHandle0.Identifier(), m_downSampleDepthLinear01TextureHandle.Identifier());
                cmd.ClearRenderTarget(false, true, Color.clear);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                {
                    SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
                    DrawingSettings drawingSettings =
                        CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
                    
                    FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all, (LayerMask)0xffff, 2);
                    
                    drawingSettings.overrideMaterial = m_ssrMaterial;
                    
                    
                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                }
                
                cmd.SetGlobalTexture("_MainTex", m_ssrTextureHandle0.id);
                cmd.Blit(null, m_ssrTextureHandle.Identifier(), m_ssrProcessMatrial, m_ssrProcessMatrial.FindPass("FillEmptyPixel"));
                cmd.SetGlobalTexture("_GlobalSSRTexture", m_ssrTextureHandle.id);
                
                // 释放临时 RT，只有 m_ssrTextureHandle 后续需要使用
                cmd.ReleaseTemporaryRT(m_ssrTextureHandle0.id);
                cmd.ReleaseTemporaryRT(m_downSampleDepthLinear01TextureHandle.id);
                cmd.ReleaseTemporaryRT(m_downSampleDepthTextureAsBufferHandle.id);
                // context.ExecuteCommandBuffer(cmd);
                // cmd.Clear();
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            base.OnFinishCameraStackRendering(cmd);
            cmd.ReleaseTemporaryRT(m_ssrTextureHandle.id);
        }
    }
}
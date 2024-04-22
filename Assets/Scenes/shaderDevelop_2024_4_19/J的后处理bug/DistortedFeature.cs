using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
//using UnityEngine.Experimental.Rendering.Universal;
using System.Collections.Generic;

public class DistortedFeature : ScriptableRendererFeature
{
    class DistortedRenderPass : ScriptableRenderPass
    {
        ShaderTagId shaderTagId;
        //RenderTargetHandle tempRT = RenderTargetHandle.CameraTarget;
        RenderTargetIdentifier sourceRT;
        RenderStateBlock renderStateBlock;
        FilteringSettings filteringSettings;

        RenderTargetIdentifier SourceTexDownSample;
        int SourceTexDownSampleID;

        public DistortedRenderPass()
        {
            //tempRT.Init("_DistortionTexture");
            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
            shaderTagId = new ShaderTagId("URPDistorted");
        }

        public void Setup(RenderTargetIdentifier sourceRT)
        {
            this.sourceRT = sourceRT;
            SourceTexDownSampleID = Shader.PropertyToID("_SourceTexDownSample");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor SSdesc = cameraTextureDescriptor;
            SSdesc.depthBufferBits = 0;
            SSdesc.msaaSamples = 1;
            SSdesc.width /=1;
            SSdesc.height /= 1;

            cmd.GetTemporaryRT(SourceTexDownSampleID, SSdesc, FilterMode.Bilinear);
            cmd.SetGlobalTexture("_DistortionTexture", SourceTexDownSample);
            //cmd.GetTemporaryRT(tempRT.id, SSdesc, FilterMode.Bilinear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            DrawingSettings drawSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonTransparent);
            CommandBuffer cmd = CommandBufferPool.Get();
            SourceTexDownSample = new RenderTargetIdentifier(SourceTexDownSampleID);

            using (new ProfilingScope(cmd, new ProfilingSampler("Draw DistortionTexture")))
            {
                cmd.Blit(sourceRT, SourceTexDownSample); 
                //cmd.Blit(sourceRT, tempRT.Identifier());
                
                //cmd.Blit(SourceTexDownSample, sourceRT);
                //cmd.Blit(tempRT.Identifier(), sourceRT);
                //cmd.SetRenderTarget(sourceRT);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                //cmd.SetRenderTarget(colorAttachment);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings, ref renderStateBlock);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            //cmd.ReleaseTemporaryRT(tempRT.id);
            cmd.ReleaseTemporaryRT(SourceTexDownSampleID);
        }
    }

    DistortedRenderPass m_DistortedPass;
    public override void Create()
    {
        m_DistortedPass = new DistortedRenderPass() { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // if (!UniversalRenderPipeline.enableFXDistort)
        // {
        //     return;
        // }

        m_DistortedPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_DistortedPass);
    }
}
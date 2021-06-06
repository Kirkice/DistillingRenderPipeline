using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class CopyPosWFeature : ScriptableRendererFeature
{
    class CopyPosWPass : ScriptableRenderPass
    {
        private Material _material;
        public int soildColorID = 0;
        public ShaderTagId shaderTag = new ShaderTagId("UniversalForward");
        const string m_ProfilerTag = "CopyPosW Pass"; 
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        FilteringSettings filtering;
        public CopyPosWPass()
        {
            filtering = new FilteringSettings(RenderQueueRange.opaque, 1 << LayerMask.NameToLayer("Default"));
            _material = new Material(Shader.Find("Hidden/Distilling RP/CopyPosW"));
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            int temp = Shader.PropertyToID("_CameraPosWTexture");
            RenderTextureDescriptor desc = cameraTextureDescriptor;
            cmd.GetTemporaryRT(temp, desc);
            soildColorID = temp;
            ConfigureTarget(temp);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                var draw = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                draw.overrideMaterial = _material;
                draw.overrideMaterialPassIndex = 0;
                context.DrawRenderers(renderingData.cullResults, ref draw, ref filtering);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
    }

    CopyPosWPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CopyPosWPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
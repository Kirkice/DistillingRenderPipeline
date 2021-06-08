using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class StochasticScreenSpaceRayTracing : ScriptableRendererFeature
{
    class StochasticScreenSpaceRayTracingPass : ScriptableRenderPass
    {
        private int mRayDistance = 70;
        private float mThickness = 0.1f;
        private float mBRDFBias = 0.7f;
        private Texture m_NoiseTex;
    
        public StochasticScreenSpaceRayTracingPass()
        {

        }
    
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }
    
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

        }
        
        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
    }
    
    StochasticScreenSpaceRayTracingPass m_StochasticScreenSpaceRayTracingPass;

    public override void Create()
    {
        m_StochasticScreenSpaceRayTracingPass = new StochasticScreenSpaceRayTracingPass();
        m_StochasticScreenSpaceRayTracingPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_StochasticScreenSpaceRayTracingPass);
    }
}

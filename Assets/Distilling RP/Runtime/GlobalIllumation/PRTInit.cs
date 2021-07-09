using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class PRTInit : ScriptableRendererFeature
{
    public Cubemap _cubemap;
    class PRTPass : ScriptableRenderPass
    {
        public Texture2D lut;
        public Cubemap cubemap;
        public PRTPass(Cubemap _cube)
        {
            lut = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Distilling RP/Textures/GlobalIllumation/LUT.jpg");
            cubemap = _cube;
        }
    
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (lut && cubemap)
            {
                Shader.SetGlobalTexture("_LUT",lut);
                Shader.SetGlobalTexture("_GlobalCubeMap",cubemap);   
            }
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }
    PRTPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new PRTPass(_cubemap);
    }
    
    public PRTInit(Cubemap cubemap)
    {
        _cubemap = cubemap;
        Create();
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


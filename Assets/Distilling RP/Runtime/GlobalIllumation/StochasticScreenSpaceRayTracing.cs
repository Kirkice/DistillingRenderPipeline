using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class StochasticScreenSpaceRayTracing : ScriptableRendererFeature
{
    private SSRData _ssrData = null;
    class StochasticScreenSpaceRayTracingPass : ScriptableRenderPass
    {
        #region Private Parames
        private ResolutionMode depthMode = ResolutionMode.halfRes;
        private ResolutionMode rayMode = ResolutionMode.halfRes;
        private FilterMode rayFilterMode = FilterMode.Point;
        
        private int mRayDistance = 70;
        private float mThickness = 0.1f;
        private float mBRDFBias = 0.7f;
        private Texture m_NoiseTex;
        
        private ResolutionMode resolveMode = ResolutionMode.fullRes;
        private bool rayReuse = true;
        private bool normalization = true;
        private bool reduceFireflies = true;
        private bool useMipMap = true;
        private int maxMipMap = 5;
        private bool useTemporal = true;
        private float scale = 2.0f;
        private float response = 0.85f;
        private float minResponse = 0.85f;
        private float maxResponse = 0.95f;
        private bool useUnityMotion;
        private bool useFresnel = true;
        private float screenFadeSize = 0.25f;
        private float smoothnessRange = 1.0f;
        private SSRDebugPass debugPass = SSRDebugPass.Combine;
        
        private Matrix4x4 projectionMatrix;
        private Matrix4x4 viewProjectionMatrix;
        private Matrix4x4 inverseViewProjectionMatrix;
        private Matrix4x4 worldToCameraMatrix;
        private Matrix4x4 cameraToWorldMatrix;
        private Matrix4x4 prevViewProjectionMatrix;

        private RenderTexture temporalBuffer;
        private RenderTexture mainBuffer0, mainBuffer1;
        private RenderTexture mipMapBuffer0, mipMapBuffer1, mipMapBuffer2;
        
        private RenderBuffer[] renderBuffer = new RenderBuffer[2];
        private Vector4 project;
        private Vector2[] dirX = new Vector2[5];
        private Vector2[] dirY = new Vector2[5];
        private int[] mipLevel = new int[5] { 0, 2, 3, 4, 5 };
        private Vector2 jitterSample;
        private Material m_rendererMaterial = null;
        #endregion

        public StochasticScreenSpaceRayTracingPass(SSRData ssrData)
        {
            #region SetData
            depthMode = ssrData.depthMode;
            rayMode = ssrData.rayMode;
            mRayDistance = ssrData.rayDistance;
            mBRDFBias = ssrData.BRDFBias;
            resolveMode = ssrData.resolveMode;
            rayReuse = ssrData.rayReuse;
            normalization = ssrData.normalization;
            reduceFireflies = ssrData.reduceFireflies;
            useMipMap = ssrData.useMipMap;
            useTemporal = ssrData.useTemporal;
            scale = ssrData.scale;
            response = ssrData.response;
            useUnityMotion = ssrData.useUnityMotion;
            useFresnel = ssrData.useFresnel;
            screenFadeSize = ssrData.screenFadeSize;
            smoothnessRange = ssrData.smoothnessRange;
            debugPass = ssrData.debugPass;
            #endregion
            m_NoiseTex = Resources.Load(ShaderIDs.BlueNoiseTexPath) as Texture2D;
            m_rendererMaterial = new Material(Shader.Find("Hidden/Distilling RP/ScreenSpaceRayTracing"));
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

    public StochasticScreenSpaceRayTracing(SSRData ssrData)
    {
        _ssrData = ssrData;
    }
    
    public override void Create()
    {
        if (_ssrData != null)
        {
            m_StochasticScreenSpaceRayTracingPass = new StochasticScreenSpaceRayTracingPass(_ssrData);
            m_StochasticScreenSpaceRayTracingPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;   
        }
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_StochasticScreenSpaceRayTracingPass);
    }
}

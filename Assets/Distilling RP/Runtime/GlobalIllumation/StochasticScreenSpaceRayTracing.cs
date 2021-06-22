using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class StochasticScreenSpaceRayTracing : ScriptableRendererFeature
{
    public SSRData _ssrData = null;
    const string m_ProfilerTag = "ScreenSpaceRayTracingPass"; 
    class StochasticScreenSpaceRayTracingPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        
        RenderTargetHandle m_temporaryColorTexture;
        
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

        /// <summary>
        /// StochasticScreenSpaceRayTracingPass
        /// </summary>
        /// <param name="ssrData"></param>
        #region StochasticScreenSpaceRayTracingPass
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
            m_NoiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(ShaderIDs.BlueNoiseTexPath);
            m_rendererMaterial = new Material(Shader.Find("Hidden/Distilling RP/ScreenSpaceRayTracing"));
            m_temporaryColorTexture.Init("temporaryColorTexture");
        }
        #endregion

        public void Setup(RenderTargetIdentifier src, RenderTargetHandle dest)
        {
            this.source = src;
            this.destination = dest;
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }
    
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera)
            {
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            Render(cmd,context,renderingData);
        }

        public void Render(CommandBuffer cmd, ScriptableRenderContext context, RenderingData renderingData)
        {
            if (m_rendererMaterial)
            {
                Camera cam = renderingData.cameraData.camera;
                int width = cam.pixelWidth;
                int height = cam.pixelHeight;
                
                int rayWidth = width / (int)rayMode;
                int rayHeight = height / (int)rayMode;
            
                int resolveWidth = width / (int)resolveMode;
                int resolveHeight = height / (int)resolveMode;
                
                m_rendererMaterial.SetVector("_JitterSizeAndOffset",
                    new Vector4
                    (
                        (float)rayWidth / (float)m_NoiseTex.width,
                        (float)rayHeight / (float)m_NoiseTex.height,
                        jitterSample.x,
                        jitterSample.y
                    )
                );
                
                m_rendererMaterial.SetVector("_ScreenSize", new Vector2((float)width, (float)height));
                m_rendererMaterial.SetVector("_RayCastSize", new Vector2((float)rayWidth, (float)rayHeight));
                m_rendererMaterial.SetVector("_ResolveSize", new Vector2((float)resolveWidth, (float)resolveHeight));
                
                UpdatePrevMatrices(cam);
                UpdateRenderTargets(width, height);
                UpdateVariable();
                project = new Vector4(Mathf.Abs(cam.projectionMatrix.m00 * 0.5f), Mathf.Abs(cam.projectionMatrix.m11 * 0.5f), ((cam.farClipPlane * cam.nearClipPlane) / (cam.nearClipPlane - cam.farClipPlane)) * 0.5f, 0.0f);
                m_rendererMaterial.SetVector("_Project", project);
                
                RenderTexture rayCast = CreateTempBuffer(rayWidth, rayHeight, 0, RenderTextureFormat.ARGBHalf);
                RenderTexture rayCastMask = CreateTempBuffer(rayWidth, rayHeight, 0, RenderTextureFormat.RHalf);
                RenderTexture depthBuffer = CreateTempBuffer(width / (int)depthMode, height / (int)depthMode, 0, RenderTextureFormat.RFloat);
                rayCast.filterMode = rayFilterMode;
                depthBuffer.filterMode = FilterMode.Point;
                
                // Depth Buffer
                Blit(cmd,source,depthBuffer,m_rendererMaterial,4);
                ReleaseTempBuffer(depthBuffer);
                
                m_rendererMaterial.SetTexture("_RayCast", rayCast);
                m_rendererMaterial.SetTexture("_RayCastMask", rayCastMask);
                m_rendererMaterial.SetTexture("_CameraDepthBuffer", depthBuffer);
                
                RenderTexture resolvePass = CreateTempBuffer(resolveWidth, resolveHeight, 0, RenderTextureFormat.DefaultHDR);
                
                if (useMipMap)
                {
                    dirX[0] = new Vector2(width, 0.0f);
                    dirX[1] = new Vector2(dirX[0].x / 4.0f, 0.0f);
                    dirX[2] = new Vector2(dirX[1].x / 2.0f, 0.0f);
                    dirX[3] = new Vector2(dirX[2].x / 2.0f, 0.0f);
                    dirX[4] = new Vector2(dirX[3].x / 2.0f, 0.0f);
                
                
                    dirY[0] = new Vector2(0.0f, height);
                    dirY[1] = new Vector2(0.0f, dirY[0].y / 4.0f);
                    dirY[2] = new Vector2(0.0f, dirY[1].y / 2.0f);
                    dirY[3] = new Vector2(0.0f, dirY[2].y / 2.0f);
                    dirY[4] = new Vector2(0.0f, dirY[3].y / 2.0f);
                
                    m_rendererMaterial.SetInt("_MaxMipMap", maxMipMap);
                
                    Blit(cmd,mainBuffer0, mipMapBuffer0);
                
                    for (int i = 0; i < maxMipMap; i++)
                    {
                        m_rendererMaterial.SetVector("_GaussianDir", new Vector2(1.0f / dirX[i].x, 0.0f));
                        m_rendererMaterial.SetInt("_MipMapCount", mipLevel[i]);
                        Blit(cmd,mipMapBuffer0, mipMapBuffer1, m_rendererMaterial, 6);
                        m_rendererMaterial.SetVector("_GaussianDir", new Vector2(0.0f, 1.0f / dirY[i].y));
                        m_rendererMaterial.SetInt("_MipMapCount", mipLevel[i]);
                        Blit(cmd,mipMapBuffer1, mipMapBuffer0, m_rendererMaterial, 6);
                    }
                
                    Blit(cmd,mipMapBuffer2, resolvePass, m_rendererMaterial, 0);
                }
                else
                {
                    Blit(cmd, mainBuffer0, resolvePass, m_rendererMaterial, 0);
                }
                
                m_rendererMaterial.SetTexture("_ReflectionBuffer", resolvePass);
                
                ReleaseTempBuffer(rayCast);
                ReleaseTempBuffer(rayCastMask);
                
                if (useTemporal && Application.isPlaying)
                {
                    m_rendererMaterial.SetFloat("_TScale", scale);
                    m_rendererMaterial.SetFloat("_TResponse", response);
                    m_rendererMaterial.SetFloat("_TMinResponse", minResponse);
                    m_rendererMaterial.SetFloat("_TMaxResponse", maxResponse);
                
                    RenderTexture temporalBuffer0 = CreateTempBuffer(width, height, 0, RenderTextureFormat.DefaultHDR);
                
                    m_rendererMaterial.SetTexture("_PreviousBuffer", temporalBuffer);
                    Blit(cmd,resolvePass, temporalBuffer0, m_rendererMaterial, 5);
                    m_rendererMaterial.SetTexture("_ReflectionBuffer", temporalBuffer0);
                    Blit(cmd,temporalBuffer0, temporalBuffer);
                    ReleaseTempBuffer(temporalBuffer0);
                }
                
                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;
                if (destination == RenderTargetHandle.CameraTarget)
                {
                    cmd.GetTemporaryRT(m_temporaryColorTexture.id, opaqueDesc);
                    switch (debugPass)
                    {
                        case SSRDebugPass.Reflection:
                        case SSRDebugPass.Cubemap:
                        case SSRDebugPass.CombineNoCubemap:
                        case SSRDebugPass.RayCast:
                        case SSRDebugPass.ReflectionAndCubemap:
                        case SSRDebugPass.SSRMask:
                        case SSRDebugPass.Jitter:
                            Blit(cmd,source,m_temporaryColorTexture.Identifier(),m_rendererMaterial, 2);
                            break;
                        case SSRDebugPass.Combine:
                            Blit(cmd,source,mainBuffer1,m_rendererMaterial, 2);
                            Blit(cmd,mainBuffer1, source);
                            break;
                    }
                }
                else
                {
                    Blit(cmd, source, destination.Identifier());
                }
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                
                ReleaseTempBuffer(resolvePass);
                
                prevViewProjectionMatrix = viewProjectionMatrix;
            }
            else
            {
                Blit(cmd, source, destination.Identifier());
                CommandBufferPool.Release(cmd);
            }
        }
        /// <summary>
        /// CreateRenderTexture
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="d"></param>
        /// <param name="f"></param>
        /// <param name="useMipMap"></param>
        /// <param name="generateMipMap"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        #region CreateRenderTexture
        public static RenderTexture CreateRenderTexture(int w, int h, int d, RenderTextureFormat f, bool useMipMap, bool generateMipMap, FilterMode filterMode)
        {
            RenderTexture r = new RenderTexture(w, h, d, f);
            r.filterMode = filterMode;
            r.useMipMap = useMipMap;
            r.autoGenerateMips = generateMipMap;
            r.Create();
            return r;
        }
        #endregion

        /// <summary>
        /// ReleaseRenderTargets
        /// </summary>
        #region ReleaseRenderTargets
        private void ReleaseRenderTargets()
        {

            if (temporalBuffer != null)
            {
                temporalBuffer.Release();
                temporalBuffer = null;
            }

            if (mainBuffer0 != null || mainBuffer1 != null)
            {
                mainBuffer0.Release();
                mainBuffer0 = null;
                mainBuffer1.Release();
                mainBuffer1 = null;
            }

            if (mipMapBuffer0 != null)
            {
                mipMapBuffer0.Release();
                mipMapBuffer0 = null;
            }
        }
        #endregion

        /// <summary>
        /// UpdateRenderTargets
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        #region UpdateRenderTargets
        private void UpdateRenderTargets(int width, int height)
        {
            if (temporalBuffer != null && temporalBuffer.width != width)
            {
                ReleaseRenderTargets();
            }

            if (temporalBuffer == null || !temporalBuffer.IsCreated())
            {
                temporalBuffer = CreateRenderTexture(width, height, 0, RenderTextureFormat.DefaultHDR, false, false, FilterMode.Bilinear);
            }

            if (mainBuffer0 == null || !mainBuffer0.IsCreated())
            {
                mainBuffer0 = CreateRenderTexture(width, height, 0, RenderTextureFormat.DefaultHDR, false, false, FilterMode.Bilinear);
                mainBuffer1 = CreateRenderTexture(width, height, 0, RenderTextureFormat.DefaultHDR, false, false, FilterMode.Bilinear);
            }

            if (mipMapBuffer0 == null || !mipMapBuffer0.IsCreated())
            {
                mipMapBuffer0 = CreateRenderTexture(width, height, 0, RenderTextureFormat.DefaultHDR, true, true, FilterMode.Bilinear); // Need to be power of two
                mipMapBuffer1 = CreateRenderTexture(width, height, 0, RenderTextureFormat.DefaultHDR, true, true, FilterMode.Bilinear); // Need to be power of two
                mipMapBuffer2 = CreateRenderTexture(width, height, 0, RenderTextureFormat.DefaultHDR, true, false, FilterMode.Bilinear); // Need to be power of two
            }
        }
        #endregion
        
        /// <summary>
        /// UpdateVariable
        /// </summary>
        #region UpdateVariable
        private void UpdateVariable()
        {
            m_rendererMaterial.SetTexture("_Noise", m_NoiseTex);
            m_rendererMaterial.SetVector("_NoiseSize", new Vector2(m_NoiseTex.width, m_NoiseTex.height));
            m_rendererMaterial.SetFloat("_BRDFBias", mBRDFBias);
            m_rendererMaterial.SetFloat("_SmoothnessRange", smoothnessRange);
            m_rendererMaterial.SetFloat("_EdgeFactor", screenFadeSize);
            m_rendererMaterial.SetInt("_NumSteps", mRayDistance);
            m_rendererMaterial.SetFloat("_Thickness", mThickness);


            if (!rayReuse)
                m_rendererMaterial.SetInt("_RayReuse", 0);
            else
                m_rendererMaterial.SetInt("_RayReuse", 1);

            if (!normalization)
                m_rendererMaterial.SetInt("_UseNormalization", 0);
            else
                m_rendererMaterial.SetInt("_UseNormalization", 1);

            if (!useFresnel)
                m_rendererMaterial.SetInt("_UseFresnel", 0);
            else
                m_rendererMaterial.SetInt("_UseFresnel", 1);

            if (!useTemporal)
                m_rendererMaterial.SetInt("_UseTemporal", 0);
            else if (useTemporal && Application.isPlaying)
                m_rendererMaterial.SetInt("_UseTemporal", 1);

            if (!useUnityMotion)
                m_rendererMaterial.SetInt("_ReflectionVelocity", 1);
            else if (useTemporal)
                m_rendererMaterial.SetInt("_ReflectionVelocity", 0);

            if (!reduceFireflies)
                m_rendererMaterial.SetInt("_Fireflies", 0);
            else
                m_rendererMaterial.SetInt("_Fireflies", 1);

            switch (debugPass)
            {
                case SSRDebugPass.Combine:
                    m_rendererMaterial.SetInt("_DebugPass", 0);
                    break;
                case SSRDebugPass.Reflection:
                    m_rendererMaterial.SetInt("_DebugPass", 1);
                    break;
                case SSRDebugPass.Cubemap:
                    m_rendererMaterial.SetInt("_DebugPass", 2);
                    break;
                case SSRDebugPass.ReflectionAndCubemap:
                    m_rendererMaterial.SetInt("_DebugPass", 3);
                    break;
                case SSRDebugPass.SSRMask:
                    m_rendererMaterial.SetInt("_DebugPass", 4);
                    break;
                case SSRDebugPass.CombineNoCubemap:
                    m_rendererMaterial.SetInt("_DebugPass", 5);
                    break;
                case SSRDebugPass.RayCast:
                    m_rendererMaterial.SetInt("_DebugPass", 6);
                    break;
                case SSRDebugPass.Jitter:
                    m_rendererMaterial.SetInt("_DebugPass", 7);
                    break;
            }
        }
        #endregion

        /// <summary>
        /// UpdatePrevMatrices
        /// </summary>
        /// <param name="cam"></param>
        #region UpdatePrevMatrices
        private void UpdatePrevMatrices(Camera cam)
        {
            worldToCameraMatrix = cam.worldToCameraMatrix;
            cameraToWorldMatrix = worldToCameraMatrix.inverse;

            projectionMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);

            viewProjectionMatrix = projectionMatrix * worldToCameraMatrix;
            inverseViewProjectionMatrix = viewProjectionMatrix.inverse;

            m_rendererMaterial.SetMatrix("_ProjectionMatrix", projectionMatrix);
            m_rendererMaterial.SetMatrix("_ViewProjectionMatrix", viewProjectionMatrix);
            m_rendererMaterial.SetMatrix("_InverseProjectionMatrix", projectionMatrix.inverse);
            m_rendererMaterial.SetMatrix("_InverseViewProjectionMatrix", inverseViewProjectionMatrix);
            m_rendererMaterial.SetMatrix("_WorldToCameraMatrix", worldToCameraMatrix);
            m_rendererMaterial.SetMatrix("_CameraToWorldMatrix", cameraToWorldMatrix);

            m_rendererMaterial.SetMatrix("_PrevViewProjectionMatrix", prevViewProjectionMatrix);
            m_rendererMaterial.SetMatrix("_PrevInverseViewProjectionMatrix", prevViewProjectionMatrix * Matrix4x4.Inverse(viewProjectionMatrix));
        }
        #endregion

        /// <summary>
        /// CreateTempBuffer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="depth"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        #region CreateTempBuffer
        private RenderTexture CreateTempBuffer(int x, int y, int depth, RenderTextureFormat format)
        {
            return RenderTexture.GetTemporary(x, y, depth, format);
        }
        #endregion

        /// <summary>
        /// ReleaseTempBuffer
        /// </summary>
        /// <param name="rt"></param>
        #region ReleaseTempBuffer
        private void ReleaseTempBuffer(RenderTexture rt)
        {
            RenderTexture.ReleaseTemporary(rt);
        }
        #endregion

        #region Unity TAA
        // From Unity TAA
        private int m_SampleIndex = 0;
        private const int k_SampleCount = 64;

        private float GetHaltonValue(int index, int radix)
        {
            float result = 0f;
            float fraction = 1f / (float)radix;

            while (index > 0)
            {
                result += (float)(index % radix) * fraction;

                index /= radix;
                fraction /= (float)radix;
            }

            return result;
        }

        private Vector2 GenerateRandomOffset()
        {
            var offset = new Vector2(
                GetHaltonValue(m_SampleIndex & 1023, 2),
                GetHaltonValue(m_SampleIndex & 1023, 3));

            if (++m_SampleIndex >= k_SampleCount)
                m_SampleIndex = 0;

            return offset;
        }
        //

        private void OnPreCull()
        {
            jitterSample = GenerateRandomOffset();
        }
        #endregion
        
        /// <summary>
        /// DrawFullScreenQuad
        /// </summary>
        #region DrawFullScreenQuad
        private void DrawFullScreenQuad()
        {
            GL.PushMatrix();
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f); // BL

            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f); // BR

            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 0.0f); // TR

            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

            GL.End();
            GL.PopMatrix();
        }
        #endregion
        
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destination == RenderTargetHandle.CameraTarget)
                cmd.ReleaseTemporaryRT(m_temporaryColorTexture.id);
        }
    }
    
    StochasticScreenSpaceRayTracingPass m_StochasticScreenSpaceRayTracingPass;

    public StochasticScreenSpaceRayTracing(SSRData ssrData)
    {
        _ssrData = ssrData;
        Create();
    }
    
    public override void Create()
    {
        if (_ssrData != null)
        {
            m_StochasticScreenSpaceRayTracingPass = new StochasticScreenSpaceRayTracingPass(_ssrData);
            m_StochasticScreenSpaceRayTracingPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;   
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        var dest = RenderTargetHandle.CameraTarget;
        m_StochasticScreenSpaceRayTracingPass.Setup(src,dest);
        renderer.EnqueuePass(m_StochasticScreenSpaceRayTracingPass);
    }
}

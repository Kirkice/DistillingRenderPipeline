using UnityEngine.Rendering.Distilling.Internal;
using System.Reflection;

namespace UnityEngine.Rendering.Distilling
{
    /// <summary>
    /// Rendering modes for Universal renderer.
    /// </summary>
    public enum RenderingMode
    {
        /// <summary>Render all objects and lighting in one pass, with a hard limit on the number of lights that can be applied on an object.</summary>
        Forward,
        /// <summary>Render all objects first in a g-buffer pass, then apply all lighting in a separate pass using deferred shading.</summary>
        Deferred,
        
        RTXRayTracing,
    };
    /// <summary>
    /// Default renderer for Universal RP.
    /// This renderer is supported on all Universal RP supported platforms.
    /// It uses a classic forward rendering strategy with per-object light culling.
    /// </summary>
    public sealed class ForwardRenderer : ScriptableRenderer
    {
        const int k_DepthStencilBufferBits = 32;
        // 使用SSR
        private bool m_BoolScreenSpaceRayTracing;
        // 使用VXGI
        private bool m_BoolVoxelGI;
        private static class Profiling
        {
            private const string k_Name = nameof(ForwardRenderer);
            public static readonly ProfilingSampler createCameraRenderTarget = new ProfilingSampler($"{k_Name}.{nameof(CreateCameraRenderTarget)}");
        }

        // Rendering mode setup from UI.
        internal RenderingMode renderingMode { get { return m_RenderingMode;  } }
        // Actual rendering mode, which may be different (ex: wireframe rendering, harware not capable of deferred rendering).
        internal RenderingMode actualRenderingMode { get { return GL.wireframe || m_DeferredLights == null || !m_DeferredLights.IsRuntimeSupportedThisFrame()  ? RenderingMode.Forward : this.renderingMode; } }
        internal bool accurateGbufferNormals { get { return m_DeferredLights != null ? m_DeferredLights.AccurateGbufferNormals : false; } }

        SphericalHarmonics m_SphericalHarmonics;
        RealTimeRayTracing m_RealTimeRayTracing;
        StochasticScreenSpaceRayTracing m_SSR; 
        VoxelGI m_VoxelGI;
        DepthOnlyPass m_DepthPrepass;
        DepthNormalOnlyPass m_DepthNormalPrepass;
        MainLightShadowCasterPass m_MainLightShadowCasterPass;
        AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;
        GBufferPass m_GBufferPass;
        CopyDepthPass m_GBufferCopyDepthPass;
        TileDepthRangePass m_TileDepthRangePass;
        TileDepthRangePass m_TileDepthRangeExtraPass;
        DeferredPass m_DeferredPass;
        DrawObjectsPass m_RenderOpaqueForwardOnlyPass;
        DrawObjectsPass m_RenderOpaqueForwardPass;
        DrawSkyboxPass m_DrawSkyboxPass;
        CopyDepthPass m_CopyDepthPass;
        CopyColorPass m_CopyColorPass;
        TransparentSettingsPass m_TransparentSettingsPass;
        DrawObjectsPass m_RenderTransparentForwardPass;
        InvokeOnRenderObjectCallbackPass m_OnRenderObjectCallbackPass;
        FinalBlitPass m_FinalBlitPass;

        CopyNormalWFeature m_CopyNormalWPass;
        CopyPosWFeature m_CopyPosWPass;
        CopyTangentFeature m_CopyTangentPass;
        CopyShadowMaskFeature m_CopyShadowMaskPass;
#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
        PostProcessPassCompat m_OpaquePostProcessPassCompat;
        PostProcessPassCompat m_PostProcessPassCompat;
#endif

#if UNITY_EDITOR
        SceneViewDepthCopyPass m_SceneViewDepthCopyPass;
#endif

        RenderTargetHandle m_ActiveCameraColorAttachment;
        RenderTargetHandle m_ActiveCameraDepthAttachment;
        RenderTargetHandle m_CameraColorAttachment;
        RenderTargetHandle m_CameraDepthAttachment;
        RenderTargetHandle m_DepthTexture;
        RenderTargetHandle m_NormalsTexture;

        RenderTargetHandle[] m_GBufferHandles;
        RenderTargetHandle m_OpaqueColor;
        RenderTargetHandle m_DepthInfoTexture;
        RenderTargetHandle m_TileDepthInfoTexture;
        ForwardLights m_ForwardLights;
        DeferredLights m_DeferredLights;
        RenderingMode m_RenderingMode;
        StencilState m_DefaultStencilState;
        
        
        Material m_BlitMaterial = null;
        Material m_CopyDepthMaterial = null;
        Material m_SamplingMaterial = null;
        Material m_ScreenspaceShadowsMaterial = null;
        Material m_TileDepthInfoMaterial = null;
        Material m_TileDeferredMaterial = null;
        Material m_StencilDeferredMaterial = null;

        PostProcessPass m_PostProcessPass;
        ColorGradingLutPass m_ColorGradingLutPass;
        PostProcessPass m_FinalPostProcessPass;
        CapturePass m_CapturePass;
        RenderTargetHandle m_AfterPostProcessColor;
        RenderTargetHandle m_ColorGradingLut;
        
        public ForwardRenderer(ForwardRendererData data) : base(data)
        {
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(data.shaders.blitPS);
            m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(data.shaders.copyDepthPS);
            m_SamplingMaterial = CoreUtils.CreateEngineMaterial(data.shaders.samplingPS);
            //m_TileDepthInfoMaterial = CoreUtils.CreateEngineMaterial(data.shaders.tileDepthInfoPS);
            //m_TileDeferredMaterial = CoreUtils.CreateEngineMaterial(data.shaders.tileDeferredPS);
            m_StencilDeferredMaterial = CoreUtils.CreateEngineMaterial(data.shaders.stencilDeferredPS);
            // m_ScreenspaceShadowsMaterial = CoreUtils.CreateEngineMaterial(data.shaders.screenSpaceShadowPS);

            StencilStateData stencilData = data.defaultStencilState;
            m_DefaultStencilState = StencilState.defaultValue;
            m_DefaultStencilState.enabled = stencilData.overrideStencilState;
            m_DefaultStencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            m_DefaultStencilState.SetPassOperation(stencilData.passOperation);
            m_DefaultStencilState.SetFailOperation(stencilData.failOperation);
            m_DefaultStencilState.SetZFailOperation(stencilData.zFailOperation);

            m_ForwardLights = new ForwardLights();
            this.m_RenderingMode = data.renderingMode;
            this.m_BoolScreenSpaceRayTracing = data.BoolScreenSpaceRayTracing;
            this.m_BoolVoxelGI = data.BoolVoxelGI;
            m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
            m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
            
            m_DepthPrepass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrepasses, RenderQueueRange.opaque, data.opaqueLayerMask);
            m_DepthNormalPrepass = new DepthNormalOnlyPass(RenderPassEvent.BeforeRenderingPrepasses, RenderQueueRange.opaque, data.opaqueLayerMask);

            if (this.renderingMode == RenderingMode.Deferred)
            {
                m_DeferredLights = new DeferredLights(m_TileDepthInfoMaterial, m_TileDeferredMaterial, m_StencilDeferredMaterial);
                m_DeferredLights.AccurateGbufferNormals = data.accurateGbufferNormals;
                
                m_DeferredLights.TiledDeferredShading = false;
                
                m_GBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference, m_DeferredLights);
                
                StencilState forwardOnlyStencilState = DeferredLights.OverwriteStencil(m_DefaultStencilState, (int)StencilUsage.MaterialMask);
                ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[]
                {
                    new ShaderTagId("UniversalForwardOnly"),
                    new ShaderTagId("SRPDefaultUnlit"), // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
                    new ShaderTagId("LightweightForward") // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
                };
                int forwardOnlyStencilRef = stencilData.stencilReference | (int)StencilUsage.MaterialUnlit;
                m_RenderOpaqueForwardOnlyPass = new DrawObjectsPass("Render Opaques Forward Only", forwardOnlyShaderTagIds, true, RenderPassEvent.BeforeRenderingOpaques + 1, RenderQueueRange.opaque, data.opaqueLayerMask, forwardOnlyStencilState, forwardOnlyStencilRef);
                m_GBufferCopyDepthPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingOpaques + 2, m_CopyDepthMaterial);
                m_TileDepthRangePass = new TileDepthRangePass(RenderPassEvent.BeforeRenderingOpaques + 3, m_DeferredLights, 0);
                m_TileDepthRangeExtraPass = new TileDepthRangePass(RenderPassEvent.BeforeRenderingOpaques + 4, m_DeferredLights, 1);
                m_DeferredPass = new DeferredPass(RenderPassEvent.BeforeRenderingOpaques + 5, m_DeferredLights);
            }
            
            if(this.renderingMode == RenderingMode.RTXRayTracing)
                m_RealTimeRayTracing = new RealTimeRayTracing();
                        
            m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrepasses, data.postProcessData);
            m_RenderOpaqueForwardPass = new DrawObjectsPass("Render Opaques", true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference);
            m_CopyDepthPass = new CopyDepthPass(RenderPassEvent.AfterRenderingSkybox, m_CopyDepthMaterial);
            m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
            m_CopyColorPass = new CopyColorPass(RenderPassEvent.AfterRenderingSkybox, m_SamplingMaterial);
            m_SphericalHarmonics = new SphericalHarmonics(RenderPassEvent.AfterRenderingSkybox, (int)data.SphericalHarmonicsDegree);

            m_SSR = new StochasticScreenSpaceRayTracing(data.m_SSRData);
            m_VoxelGI = new VoxelGI(data.m_VXGIData);
            m_CopyNormalWPass = new CopyNormalWFeature();
            m_CopyPosWPass = new CopyPosWFeature();
            m_CopyTangentPass = new CopyTangentFeature();
            m_CopyShadowMaskPass = new CopyShadowMaskFeature();
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            if (!DistillingRenderPipeline.asset.useAdaptivePerformance || AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipTransparentObjects == false)
#endif
            {
                m_TransparentSettingsPass = new TransparentSettingsPass(RenderPassEvent.BeforeRenderingTransparents, data.shadowTransparentReceive);
                m_RenderTransparentForwardPass = new DrawObjectsPass("Render Transparents", false, RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent, data.transparentLayerMask, m_DefaultStencilState, stencilData.stencilReference);
            }

            m_OnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);
            m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, data.postProcessData, m_BlitMaterial);
            m_FinalPostProcessPass = new PostProcessPass(RenderPassEvent.AfterRendering + 1, data.postProcessData, m_BlitMaterial);
            m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
            m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering + 1, m_BlitMaterial);

#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
            m_OpaquePostProcessPassCompat = new PostProcessPassCompat(RenderPassEvent.BeforeRenderingOpaques, true);
            m_PostProcessPassCompat = new PostProcessPassCompat(RenderPassEvent.BeforeRenderingPostProcessing);
#endif

#if UNITY_EDITOR
            m_SceneViewDepthCopyPass = new SceneViewDepthCopyPass(RenderPassEvent.AfterRendering + 9, m_CopyDepthMaterial);
#endif

            // RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
            // Samples (MSAA) depend on camera and pipeline
            m_CameraColorAttachment.Init("_CameraColorTexture");
            m_CameraDepthAttachment.Init("_CameraDepthAttachment");
            m_DepthTexture.Init("_CameraDepthTexture");
            m_NormalsTexture.Init("_CameraNormalsTexture");

            if (this.renderingMode == RenderingMode.Deferred)
            {
                m_GBufferHandles = new RenderTargetHandle[(int)DeferredLights.GBufferHandles.Count];
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.DepthAsColor].Init("_GBufferDepthAsColor");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.Albedo].Init("_GBuffer0");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.SpecularMetallic].Init("_GBuffer1");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.NormalSmoothness].Init("_GBuffer2");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.Lighting] = new RenderTargetHandle();
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.ShadowMask].Init("_GBuffer4");
            }
            m_OpaqueColor.Init("_CameraOpaqueTexture");
            m_DepthInfoTexture.Init("_DepthInfoTexture");
            m_TileDepthInfoTexture.Init("_TileDepthInfoTexture");
            
            m_AfterPostProcessColor.Init("_AfterPostProcessTexture");
            m_ColorGradingLut.Init("_InternalGradingLut");

            supportedRenderingFeatures = new RenderingFeatures()
            {
                cameraStacking = true,
            };
            
            if (this.renderingMode == RenderingMode.Deferred)
            {
                // Deferred rendering does not support MSAA.
                this.supportedRenderingFeatures.msaa = false;

                // Avoid legacy platforms: use vulkan instead.
                unsupportedGraphicsDeviceTypes = new GraphicsDeviceType[]
                {
                    GraphicsDeviceType.OpenGLCore,
                    GraphicsDeviceType.OpenGLES2,
                    GraphicsDeviceType.OpenGLES3
                };
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // always dispose unmanaged resources
            m_PostProcessPass.Cleanup();
            m_FinalPostProcessPass.Cleanup();
            m_ColorGradingLutPass.Cleanup();

            CoreUtils.Destroy(m_BlitMaterial);
            CoreUtils.Destroy(m_CopyDepthMaterial);
            CoreUtils.Destroy(m_SamplingMaterial);
            CoreUtils.Destroy(m_ScreenspaceShadowsMaterial);
            CoreUtils.Destroy(m_TileDepthInfoMaterial);
            CoreUtils.Destroy(m_TileDeferredMaterial);
            CoreUtils.Destroy(m_StencilDeferredMaterial);
        }

        /// <inheritdoc />
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            bool needTransparencyPass = !DistillingRenderPipeline.asset.useAdaptivePerformance || !AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipTransparentObjects;
#endif
            Camera camera = renderingData.cameraData.camera;
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            // Special path for depth only offscreen cameras. Only write opaques + transparents.
            bool isOffscreenDepthTexture = cameraData.targetTexture != null && cameraData.targetTexture.format == RenderTextureFormat.Depth;
            if (isOffscreenDepthTexture)
            {
                ConfigureCameraTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);

                for (int i = 0; i < rendererFeatures.Count; ++i)
                {
                    if(rendererFeatures[i].isActive)
                        rendererFeatures[i].AddRenderPasses(this, ref renderingData);
                }

                EnqueuePass(m_RenderOpaqueForwardPass);
                EnqueuePass(m_DrawSkyboxPass);
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
                if (!needTransparencyPass)
                    return;
#endif
                EnqueuePass(m_RenderTransparentForwardPass);
                return;
            }

            if (m_DeferredLights != null)
                m_DeferredLights.ResolveMixedLightingMode(ref renderingData);
            
            // Should apply post-processing after rendering this camera?
            bool applyPostProcessing = cameraData.postProcessEnabled;
            // There's at least a camera in the camera stack that applies post-processing
            bool anyPostProcessing = renderingData.postProcessingEnabled;

            var postProcessFeatureSet = DistillingRenderPipeline.asset.postProcessingFeatureSet;

            // We generate color LUT in the base camera only. This allows us to not break render pass execution for overlay cameras.
            bool generateColorGradingLUT = cameraData.postProcessEnabled;
#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
            // PPv2 doesn't need to generate color grading LUT.
            if (postProcessFeatureSet == PostProcessingFeatureSet.PostProcessingV2)
                generateColorGradingLUT = false;
#endif
            
            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            bool isPreviewCamera = cameraData.isPreviewCamera;
            bool requiresDepthTexture = cameraData.requiresDepthTexture || this.actualRenderingMode == RenderingMode.Deferred;
            bool isStereoEnabled = cameraData.isStereoEnabled;

            bool mainLightShadows = m_MainLightShadowCasterPass.Setup(ref renderingData);
            bool additionalLightShadows = m_AdditionalLightsShadowCasterPass.Setup(ref renderingData);
            bool transparentsNeedSettingsPass = m_TransparentSettingsPass.Setup(ref renderingData);

            bool requireNormalWSTexture = cameraData.requiresNormalWSTexture;
            bool requiresPosWSTexture = cameraData.requiresPosWSTexture;
            bool requiresTangentWSTexture = cameraData.requiresTangentWSTexture;
            bool requiresObjectIDTexture = cameraData.requiresObjectIDTexture;
            bool requiresTransparentColor = cameraData.requiresTransparentColor;
            bool requiresShadowMaskTexture = cameraData.requiresShadowMaskTexture;
            
            // Depth prepass is generated in the following cases:
            // - If game or offscreen camera requires it we check if we can copy the depth from the rendering opaques pass and use that instead.
            // - Scene or preview cameras always require a depth texture. We do a depth pre-pass to simplify it and it shouldn't matter much for editor.
            bool requiresDepthPrepass = requiresDepthTexture && !CanCopyDepth(ref renderingData.cameraData);
            requiresDepthPrepass |= isSceneViewCamera;
            requiresDepthPrepass |= isPreviewCamera;

            // The copying of depth should normally happen after rendering opaques.
            // But if we only require it for post processing or the scene camera then we do it after rendering transparent objects
            m_CopyDepthPass.renderPassEvent = (!requiresDepthTexture && (applyPostProcessing || isSceneViewCamera)) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingOpaques;

            // TODO: CopyDepth pass is disabled in XR due to required work to handle camera matrices in URP.
            // IF this condition is removed make sure the CopyDepthPass.cs is working properly on all XR modes. This requires PureXR SDK integration.
            if (isStereoEnabled && requiresDepthTexture)
                requiresDepthPrepass = true;

            bool isRunningHololens = false;
#if ENABLE_VR && ENABLE_VR_MODULE
            isRunningHololens = DistillingRenderPipeline.IsRunningHololens(camera);
#endif
            bool createColorTexture = RequiresIntermediateColorTexture(ref cameraData);
            createColorTexture |= (rendererFeatures.Count != 0 && !isRunningHololens);
            createColorTexture &= !isPreviewCamera;

            // If camera requires depth and there's no depth pre-pass we create a depth texture that can be read later by effect requiring it.
            bool createDepthTexture = cameraData.requiresDepthTexture && !requiresDepthPrepass;
            createDepthTexture |= (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget);
            // Deferred renderer always need to access depth buffer.
            createDepthTexture |= this.actualRenderingMode == RenderingMode.Deferred;
#if UNITY_ANDROID || UNITY_WEBGL
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
            {
                // GLES can not use render texture's depth buffer with the color buffer of the backbuffer
                // in such case we create a color texture for it too.
                createColorTexture |= createDepthTexture;
            }
#endif

            // Configure all settings require to start a new camera stack (base camera only)
            if (cameraData.renderType == CameraRenderType.Base)
            {
                m_ActiveCameraColorAttachment = (createColorTexture) ? m_CameraColorAttachment : RenderTargetHandle.CameraTarget;
                m_ActiveCameraDepthAttachment = (createDepthTexture) ? m_CameraDepthAttachment : RenderTargetHandle.CameraTarget;

                bool intermediateRenderTexture = createColorTexture || createDepthTexture;

                // Doesn't create texture for Overlay cameras as they are already overlaying on top of created textures.
                bool createTextures = intermediateRenderTexture;
                if (createTextures)
                    CreateCameraRenderTarget(context, ref renderingData.cameraData);

                // if rendering to intermediate render texture we don't have to create msaa backbuffer
                int backbufferMsaaSamples = (intermediateRenderTexture) ? 1 : cameraTargetDescriptor.msaaSamples;

                if (Camera.main == camera && camera.cameraType == CameraType.Game && cameraData.targetTexture == null)
                    SetupBackbufferFormat(backbufferMsaaSamples, isStereoEnabled);
            }
            else
            {
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;
                m_ActiveCameraDepthAttachment = m_CameraDepthAttachment;
            }

            {
                var activeColorRenderTargetId = m_ActiveCameraColorAttachment.Identifier();
                var activeDepthRenderTargetId = m_ActiveCameraDepthAttachment.Identifier();   
                ConfigureCameraTarget(activeColorRenderTargetId, activeDepthRenderTargetId);
            }

            for (int i = 0; i < rendererFeatures.Count; ++i)
            {
                if(rendererFeatures[i].isActive)
                    rendererFeatures[i].AddRenderPasses(this, ref renderingData);
            }

            int count = activeRenderPassQueue.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if(activeRenderPassQueue[i] == null)
                    activeRenderPassQueue.RemoveAt(i);
            }
            bool hasPassesAfterPostProcessing = activeRenderPassQueue.Find(x => x.renderPassEvent == RenderPassEvent.AfterRendering) != null;

            if (mainLightShadows)
                EnqueuePass(m_MainLightShadowCasterPass);

            if (additionalLightShadows)
                EnqueuePass(m_AdditionalLightsShadowCasterPass);

            //SH
            EnqueuePass(m_SphericalHarmonics);
            
            if (requiresDepthPrepass)
            {
                m_DepthPrepass.Setup(cameraTargetDescriptor, m_DepthTexture);
                EnqueuePass(m_DepthPrepass);
            }
            if (renderingMode == RenderingMode.Deferred)
            {
                m_DepthNormalPrepass.Setup(cameraTargetDescriptor, m_DepthTexture, m_NormalsTexture);
                EnqueuePass(m_DepthNormalPrepass);
            }
            if (m_BoolScreenSpaceRayTracing)
            {
                m_SSR.AddRenderPasses(this, ref renderingData);
            }
            if (m_BoolVoxelGI)
            {
                m_VoxelGI.AddRenderPasses(this,ref renderingData);
            }
            if (renderingMode == RenderingMode.RTXRayTracing)
            {
                m_RealTimeRayTracing.AddRenderPasses(this, ref renderingData);
            }
            if (requireNormalWSTexture)
            {
                m_CopyNormalWPass.AddRenderPasses(this, ref renderingData);
            }

            if (requiresPosWSTexture)
            {
                m_CopyPosWPass.AddRenderPasses(this,ref renderingData);
            }

            if (requiresTangentWSTexture)
            {
                m_CopyTangentPass.AddRenderPasses(this,ref renderingData);
            }

            if (requiresShadowMaskTexture)
            {
                m_CopyShadowMaskPass.AddRenderPasses(this,ref renderingData);
            }
            if (generateColorGradingLUT)
            {
                m_ColorGradingLutPass.Setup(m_ColorGradingLut);
                EnqueuePass(m_ColorGradingLutPass);
            }

#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
#pragma warning disable 0618 // Obsolete
            bool hasOpaquePostProcessCompat = applyPostProcessing &&
                postProcessFeatureSet == PostProcessingFeatureSet.PostProcessingV2 &&
                renderingData.cameraData.postProcessLayer.HasOpaqueOnlyEffects(RenderingUtils.postProcessRenderContext);

            if (hasOpaquePostProcessCompat)
            {
                m_OpaquePostProcessPassCompat.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_ActiveCameraColorAttachment);
                EnqueuePass(m_OpaquePostProcessPassCompat);
            }
#pragma warning restore 0618
#endif

            if (this.actualRenderingMode == RenderingMode.Deferred)
                EnqueueDeferred(ref renderingData, requiresDepthPrepass, mainLightShadows, additionalLightShadows);
            else
                EnqueuePass(m_RenderOpaqueForwardPass);
            
            Skybox cameraSkybox;
            cameraData.camera.TryGetComponent<Skybox>(out cameraSkybox);
            bool isOverlayCamera = cameraData.renderType == CameraRenderType.Overlay;
            if (camera.clearFlags == CameraClearFlags.Skybox && (RenderSettings.skybox != null || cameraSkybox?.material != null) && !isOverlayCamera)
                EnqueuePass(m_DrawSkyboxPass);

            // If a depth texture was created we necessarily need to copy it, otherwise we could have render it to a renderbuffer
            if (!requiresDepthPrepass && renderingData.cameraData.requiresDepthTexture && createDepthTexture && this.actualRenderingMode != RenderingMode.Deferred)
            {
                m_CopyDepthPass.Setup(m_ActiveCameraDepthAttachment, m_DepthTexture);
                EnqueuePass(m_CopyDepthPass);
            }

            if (renderingData.cameraData.requiresOpaqueTexture)
            {
                // TODO: Downsampling method should be store in the renderer instead of in the asset.
                // We need to migrate this data to renderer. For now, we query the method in the active asset.
                Downsampling downsamplingMethod = DistillingRenderPipeline.asset.opaqueDownsampling;
                m_CopyColorPass.Setup(m_ActiveCameraColorAttachment.Identifier(), m_OpaqueColor, downsamplingMethod);
                EnqueuePass(m_CopyColorPass);
            }

            
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            if (needTransparencyPass)
#endif
            {
                if (transparentsNeedSettingsPass)
                {
                    EnqueuePass(m_TransparentSettingsPass);
                }

                EnqueuePass(m_RenderTransparentForwardPass);
            }
            EnqueuePass(m_OnRenderObjectCallbackPass);

            bool lastCameraInTheStack = cameraData.resolveFinalTarget;
            bool hasCaptureActions = renderingData.cameraData.captureActions != null && lastCameraInTheStack;
            bool applyFinalPostProcessing = anyPostProcessing && lastCameraInTheStack &&
                                     renderingData.cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;

            // When post-processing is enabled we can use the stack to resolve rendering to camera target (screen or RT).
            // However when there are render passes executing after post we avoid resolving to screen so rendering continues (before sRGBConvertion etc)
            bool resolvePostProcessingToCameraTarget = !hasCaptureActions && !hasPassesAfterPostProcessing && !applyFinalPostProcessing;

            #region Post-processing v2 support
#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
            // To keep things clean we'll separate the logic from builtin PP and PPv2 - expect some copy/pasting
            if (postProcessFeatureSet == PostProcessingFeatureSet.PostProcessingV2)
            {
                // if we have additional filters
                // we need to stay in a RT
                if (hasPassesAfterPostProcessing)
                {
                    // perform post with src / dest the same
                    if (applyPostProcessing)
                    {
                        m_PostProcessPassCompat.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_ActiveCameraColorAttachment);
                        EnqueuePass(m_PostProcessPassCompat);
                    }

                    //now blit into the final target
                    if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
                    {
                        if (renderingData.cameraData.captureActions != null)
                        {
                            m_CapturePass.Setup(m_ActiveCameraColorAttachment);
                            EnqueuePass(m_CapturePass);
                        }

                        m_FinalBlitPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment);
                        EnqueuePass(m_FinalBlitPass);
                    }
                }
                else
                {
                    if (applyPostProcessing)
                    {
                        m_PostProcessPassCompat.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, RenderTargetHandle.CameraTarget);
                        EnqueuePass(m_PostProcessPassCompat);
                    }
                    else if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
                    {
                        m_FinalBlitPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment);
                        EnqueuePass(m_FinalBlitPass);
                    }
                }
            }
            else
#endif
            #endregion
            {

                if (lastCameraInTheStack)
                {
                    // Post-processing will resolve to final target. No need for final blit pass.
                    if (applyPostProcessing)
                    {
                        var destination = resolvePostProcessingToCameraTarget ? RenderTargetHandle.CameraTarget : m_AfterPostProcessColor;

                        // if resolving to screen we need to be able to perform sRGBConvertion in post-processing if necessary
                        bool doSRGBConvertion = resolvePostProcessingToCameraTarget;
                        m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, destination, m_ActiveCameraDepthAttachment, m_ColorGradingLut, applyFinalPostProcessing, doSRGBConvertion);
                        EnqueuePass(m_PostProcessPass);
                    }

                    if (renderingData.cameraData.captureActions != null)
                    {
                        m_CapturePass.Setup(m_ActiveCameraColorAttachment);
                        EnqueuePass(m_CapturePass);
                    }

                    // if we applied post-processing for this camera it means current active texture is m_AfterPostProcessColor
                    var sourceForFinalPass = (applyPostProcessing) ? m_AfterPostProcessColor : m_ActiveCameraColorAttachment;

                    // Do FXAA or any other final post-processing effect that might need to run after AA.
                    if (applyFinalPostProcessing)
                    {
                        m_FinalPostProcessPass.SetupFinalPass(sourceForFinalPass);
                        EnqueuePass(m_FinalPostProcessPass);
                    }

                    // if post-processing then we already resolved to camera target while doing post.
                    // Also only do final blit if camera is not rendering to RT.
                    bool cameraTargetResolved =
                        // final PP always blit to camera target
                        applyFinalPostProcessing ||
                        // no final PP but we have PP stack. In that case it blit unless there are render pass after PP
                        (applyPostProcessing && !hasPassesAfterPostProcessing) ||
                        // offscreen camera rendering to a texture, we don't need a blit pass to resolve to screen
                        m_ActiveCameraColorAttachment == RenderTargetHandle.CameraTarget;

                    // We need final blit to resolve to screen
                    if (!cameraTargetResolved)
                    {
                        m_FinalBlitPass.Setup(cameraTargetDescriptor, sourceForFinalPass);
                        EnqueuePass(m_FinalBlitPass);
                    }
                }

                // stay in RT so we resume rendering on stack after post-processing
                else if (applyPostProcessing)
                {
                    m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_AfterPostProcessColor, m_ActiveCameraDepthAttachment, m_ColorGradingLut, false, false);
                    EnqueuePass(m_PostProcessPass);
                }

            }

#if UNITY_EDITOR
            if (isSceneViewCamera)
            {
                // Scene view camera should always resolve target (not stacked)
                Assertions.Assert.IsTrue(lastCameraInTheStack, "Editor camera must resolve target upon finish rendering.");
                m_SceneViewDepthCopyPass.Setup(m_DepthTexture);
                EnqueuePass(m_SceneViewDepthCopyPass);
            }
#endif
        }

        /// <inheritdoc />
        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_ForwardLights.Setup(context, ref renderingData);
            // Perform per-tile light culling on CPU
            if (this.actualRenderingMode == RenderingMode.Deferred)
                m_DeferredLights.SetupLights(context, ref renderingData);
        }

        /// <inheritdoc />
        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters,
            ref CameraData cameraData)
        {
            // TODO: PerObjectCulling also affect reflection probes. Enabling it for now.
            // if (asset.additionalLightsRenderingMode == LightRenderingMode.Disabled ||
            //     asset.maxAdditionalLightsCount == 0)
            // {
            //     cullingParameters.cullingOptions |= CullingOptions.DisablePerObjectCulling;
            // }

            // We disable shadow casters if both shadow casting modes are turned off
            // or the shadow distance has been turned down to zero
            bool isShadowCastingDisabled = !DistillingRenderPipeline.asset.supportsMainLightShadows && !DistillingRenderPipeline.asset.supportsAdditionalLightShadows;
            bool isShadowDistanceZero = Mathf.Approximately(cameraData.maxShadowDistance, 0.0f);
            if (isShadowCastingDisabled || isShadowDistanceZero)
            {
                cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
            }

            if (this.actualRenderingMode == RenderingMode.Deferred)
                cullingParameters.maximumVisibleLights = 0xFFFF;
            else
            {
                // We set the number of maximum visible lights allowed and we add one for the mainlight...
                cullingParameters.maximumVisibleLights = DistillingRenderPipeline.maxVisibleAdditionalLights + 1;   
            }
            cullingParameters.shadowDistance = cameraData.maxShadowDistance;
        }

        /// <inheritdoc />
        public override void FinishRendering(CommandBuffer cmd)
        {
            if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraColorAttachment.id);
                m_ActiveCameraColorAttachment = RenderTargetHandle.CameraTarget;
            }

            if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraDepthAttachment.id);
                m_ActiveCameraDepthAttachment = RenderTargetHandle.CameraTarget;
            }
        }

        void EnqueueDeferred(ref RenderingData renderingData, bool hasDepthPrepass, bool applyMainShadow, bool applyAdditionalShadow)
        {
            // the last slice is the lighting buffer created in DeferredRenderer.cs
            m_GBufferHandles[(int)DeferredLights.GBufferHandles.Lighting] = m_ActiveCameraColorAttachment;

            m_DeferredLights.Setup(
                ref renderingData,
                applyAdditionalShadow ? m_AdditionalLightsShadowCasterPass : null,
                hasDepthPrepass,
                renderingData.cameraData.renderType == CameraRenderType.Overlay,
                m_DepthTexture,
                m_DepthInfoTexture,
                m_TileDepthInfoTexture,
                m_ActiveCameraDepthAttachment, m_GBufferHandles
            );

            EnqueuePass(m_GBufferPass);

            EnqueuePass(m_RenderOpaqueForwardOnlyPass);

            //Must copy depth for deferred shading: TODO wait for API fix to bind depth texture as read-only resource.
            if (!hasDepthPrepass)
            {
                m_GBufferCopyDepthPass.Setup(m_CameraDepthAttachment, m_DepthTexture);
                EnqueuePass(m_GBufferCopyDepthPass);
            }

            // Note: DeferredRender.Setup is called by UniversalRenderPipeline.RenderSingleCamera (overrides ScriptableRenderer.Setup).
            // At this point, we do not know if m_DeferredLights.m_Tilers[x].m_Tiles actually contain any indices of lights intersecting tiles (If there are no lights intersecting tiles, we could skip several following passes) : this information is computed in DeferredRender.SetupLights, which is called later by UniversalRenderPipeline.RenderSingleCamera (via ScriptableRenderer.Execute).
            // However HasTileLights uses m_HasTileVisLights which is calculated by CheckHasTileLights from all visibleLights. visibleLights is the list of lights that have passed camera culling, so we know they are in front of the camera. So we can assume m_DeferredLights.m_Tilers[x].m_Tiles will not be empty in that case.
            // m_DeferredLights.m_Tilers[x].m_Tiles could be empty if we implemented an algorithm accessing scene depth information on the CPU side, but this (access depth from CPU) will probably not happen.
            if (m_DeferredLights.HasTileLights())
            {
                // Compute for each tile a 32bits bitmask in which a raised bit means "this 1/32th depth slice contains geometry that could intersect with lights".
                // Per-tile bitmasks are obtained by merging together the per-pixel bitmasks computed for each individual pixel of the tile.
                EnqueuePass(m_TileDepthRangePass);

                // On some platform, splitting the bitmasks computation into two passes:
                //   1/ Compute bitmasks for individual or small blocks of pixels
                //   2/ merge those individual bitmasks into per-tile bitmasks
                // provides better performance that doing it in a single above pass.
                if (m_DeferredLights.HasTileDepthRangeExtraPass())
                    EnqueuePass(m_TileDepthRangeExtraPass);
            }

            EnqueuePass(m_DeferredPass);
        }
                
        void CreateCameraRenderTarget(ScriptableRenderContext context, ref CameraData cameraData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, Profiling.createCameraRenderTarget))
            {
                var descriptor = cameraData.cameraTargetDescriptor;
                if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
                {
                    bool useDepthRenderBuffer = m_ActiveCameraDepthAttachment == RenderTargetHandle.CameraTarget;
                    var colorDescriptor = descriptor;
                    colorDescriptor.useMipMap = false;
                    colorDescriptor.autoGenerateMips = false;
                    colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? k_DepthStencilBufferBits : 0;
                    cmd.GetTemporaryRT(m_ActiveCameraColorAttachment.id, colorDescriptor, FilterMode.Bilinear);
                }

                if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
                {
                    var depthDescriptor = descriptor;
                    depthDescriptor.useMipMap = false;
                    depthDescriptor.autoGenerateMips = false;
                    depthDescriptor.colorFormat = RenderTextureFormat.Depth;
                    depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                    cmd.GetTemporaryRT(m_ActiveCameraDepthAttachment.id, depthDescriptor, FilterMode.Point);
                }   
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void SetupBackbufferFormat(int msaaSamples, bool stereo)
        {
#if ENABLE_VR && ENABLE_VR_MODULE
            if (!stereo)
                return;

            bool msaaSampleCountHasChanged = false;
            int currentQualitySettingsSampleCount = QualitySettings.antiAliasing;
            if (currentQualitySettingsSampleCount != msaaSamples &&
                !(currentQualitySettingsSampleCount == 0 && msaaSamples == 1))
            {
                msaaSampleCountHasChanged = true;
            }

            // There's no exposed API to control how a backbuffer is created with MSAA
            // By settings antiAliasing we match what the amount of samples in camera data with backbuffer
            // We only do this for the main camera and this only takes effect in the beginning of next frame.
            // This settings should not be changed on a frame basis so that's fine.
            if (msaaSampleCountHasChanged)
            {
                QualitySettings.antiAliasing = msaaSamples;
                XR.XRDevice.UpdateEyeTextureMSAASetting();
            }
#endif
        }

        bool PlatformRequiresExplicitMsaaResolve()
        {
            // On Metal/iOS the MSAA resolve is done implicitly as part of the renderpass, so we do not need an extra intermediate pass for the explicit autoresolve.
            // TODO: should also be valid on Metal MacOS/Editor, but currently not working as expected. Remove the "mobile only" requirement once trunk has a fix.

            return !SystemInfo.supportsMultisampleAutoResolve &&
                   !(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal && Application.isMobilePlatform);
        }

        /// <summary>
        /// Checks if the pipeline needs to create a intermediate render texture.
        /// </summary>
        /// <param name="cameraData">CameraData contains all relevant render target information for the camera.</param>
        /// <seealso cref="CameraData"/>
        /// <returns>Return true if pipeline needs to render to a intermediate render texture.</returns>
        bool RequiresIntermediateColorTexture(ref CameraData cameraData)
        {
            if (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget)
                return true;
            if (this.actualRenderingMode == RenderingMode.Deferred)
                return true;
            
            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            var cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
            int msaaSamples = cameraTargetDescriptor.msaaSamples;
            bool isStereoEnabled = cameraData.isStereoEnabled;
            bool isScaledRender = !Mathf.Approximately(cameraData.renderScale, 1.0f) && !cameraData.isStereoEnabled;
            bool isCompatibleBackbufferTextureDimension = cameraTargetDescriptor.dimension == TextureDimension.Tex2D;
            bool requiresExplicitMsaaResolve = msaaSamples > 1 && PlatformRequiresExplicitMsaaResolve();
            bool isOffscreenRender = cameraData.targetTexture != null && !isSceneViewCamera;
            bool isCapturing = cameraData.captureActions != null;

#if ENABLE_VR && ENABLE_VR_MODULE
            if (isStereoEnabled)
                isCompatibleBackbufferTextureDimension = UnityEngine.XR.XRSettings.deviceEyeTextureDimension == cameraTargetDescriptor.dimension;
#endif

            bool requiresBlitForOffscreenCamera = cameraData.postProcessEnabled || cameraData.requiresOpaqueTexture || requiresExplicitMsaaResolve || !cameraData.isDefaultViewport;
            if (isOffscreenRender)
                return requiresBlitForOffscreenCamera;

            return requiresBlitForOffscreenCamera || isSceneViewCamera || isScaledRender || cameraData.isHdrEnabled ||
                   !isCompatibleBackbufferTextureDimension || isCapturing ||
                   (Display.main.requiresBlitToBackbuffer && !isStereoEnabled);
        }

        bool CanCopyDepth(ref CameraData cameraData)
        {
            bool msaaEnabledForCamera = cameraData.cameraTargetDescriptor.msaaSamples > 1;
            bool supportsTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            bool supportsDepthTarget = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            bool supportsDepthCopy = !msaaEnabledForCamera && (supportsDepthTarget || supportsTextureCopy);

            // TODO:  We don't have support to highp Texture2DMS currently and this breaks depth precision.
            // currently disabling it until shader changes kick in.
            //bool msaaDepthResolve = msaaEnabledForCamera && SystemInfo.supportsMultisampledTextures != 0;
            bool msaaDepthResolve = false;
            return supportsDepthCopy || msaaDepthResolve;
        }
    }
}

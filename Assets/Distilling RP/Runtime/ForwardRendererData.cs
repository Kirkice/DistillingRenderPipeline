#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Distilling
{
    [Serializable, ReloadGroup, ExcludeFromPreset]
    [MovedFrom("UnityEngine.Rendering.DistillingRP")]
    public class ForwardRendererData : ScriptableRendererData
    {
#if UNITY_EDITOR
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateForwardRendererAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<ForwardRendererData>();
                AssetDatabase.CreateAsset(instance, pathName);
                ResourceReloader.ReloadAllNullIn(instance, DistillingRenderPipelineAsset.packagePath);
                Selection.activeObject = instance;
            }
        }

        [MenuItem("Assets/Create/Rendering/Distilling Render Pipeline/Forward Renderer", priority = CoreUtils.assetCreateMenuPriority2)]
        static void CreateForwardRendererData()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateForwardRendererAsset>(), "CustomForwardRendererData.asset", null, null);
        }
#endif

        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            [Reload("Shaders/Utils/Blit.shader")]
            public Shader blitPS;

            [Reload("Shaders/Utils/CopyDepth.shader")]
            public Shader copyDepthPS;

            [Reload("Shaders/Utils/ScreenSpaceShadows.shader")]
            public Shader screenSpaceShadowPS;

            [Reload("Shaders/Utils/Sampling.shader")]
            public Shader samplingPS;

            [Reload("Shaders/Utils/CopyNormalWS.shader")]
            public Shader copyNormalWS;
            
            [Reload("Shaders/Utils/DRPStencilDeferred.shader")]
            public Shader stencilDeferredPS;
            
            [Reload("Shaders/Utils/FallbackError.shader")]
            public Shader fallbackErrorPS;
        }

        [Reload("Runtime/Data/PostProcessData.asset")]
        public PostProcessData postProcessData = null;

        [Reload("Runtime/Data/ScreenSpaceRayTracingData.asset")]
        public SSRData m_SSRData = null;
        public ShaderResources shaders = null;

        [SerializeField] LayerMask m_OpaqueLayerMask = -1;
        [SerializeField] LayerMask m_TransparentLayerMask = -1;
        [SerializeField] StencilStateData m_DefaultStencilState = new StencilStateData();
        [SerializeField] bool m_ShadowTransparentReceive = true;
        
        [SerializeField] bool m_BoolScreenSpaceRayTracing = false;
        [SerializeField] bool m_BoolPRT = false;
        [SerializeField] Cubemap m_GlobalCubeMap;
            
        [SerializeField] RenderingMode m_RenderingMode = RenderingMode.Forward;
        [SerializeField] bool m_AccurateGbufferNormals = false;
        [SerializeField] bool m_TiledDeferredShading = false;
        
        protected override ScriptableRenderer Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ResourceReloader.TryReloadAllNullIn(this, "Assets/Distilling RP");
                ResourceReloader.TryReloadAllNullIn(postProcessData, "Assets/Distilling RP");
                ResourceReloader.TryReloadAllNullIn(m_SSRData, "Assets/Distilling RP");
            }
#endif
            return new ForwardRenderer(this);
        }

        /// <summary>
        /// Use this to configure how to filter opaque objects.
        /// </summary>
        public LayerMask opaqueLayerMask
        {
            get => m_OpaqueLayerMask;
            set
            {
                SetDirty();
                m_OpaqueLayerMask = value;
            }
        }

        /// <summary>
        /// Use this to configure how to filter transparent objects.
        /// </summary>
        public LayerMask transparentLayerMask
        {
            get => m_TransparentLayerMask;
            set
            {
                SetDirty();
                m_TransparentLayerMask = value;
            }
        }

        public StencilStateData defaultStencilState
        {
            get => m_DefaultStencilState;
            set
            {
                SetDirty();
                m_DefaultStencilState = value;
            }
        }

        public SSRData screenSpaceRayTracingData
        {
            get => m_SSRData;
            set
            {
                SetDirty();
                m_SSRData = value;
            }
        }
        /// <summary>
        /// True if transparent objects receive shadows.
        /// </summary>
        public bool shadowTransparentReceive
        {
            get => m_ShadowTransparentReceive;
            set
            {
                SetDirty();
                m_ShadowTransparentReceive = value;
            }
        }

        /// <summary>
        /// 屏幕空间光线追踪
        /// </summary>
        public bool BoolScreenSpaceRayTracing
        {
            get => m_BoolScreenSpaceRayTracing;
            set
            {
                SetDirty();
                m_BoolScreenSpaceRayTracing = false;
            }
        }
        /// <summary>
        /// PRT
        /// </summary>
        public bool BoolPRT
        {
            get => m_BoolPRT;
            set
            {
                SetDirty();
                m_BoolPRT = false;
            }
        }

        public Cubemap GlobalCubeMapProp
        {
            get => m_GlobalCubeMap;
            set
            {
                SetDirty();
                m_GlobalCubeMap = value;
            }
        }
        /// <summary>
        /// Rendering mode.
        /// </summary>
        public RenderingMode renderingMode
        {
            get => m_RenderingMode;
            set
            {
                SetDirty();
                m_RenderingMode = value;
            }
        }
        
        /// <summary>
        /// Use Octaedron Octahedron normal vector encoding for gbuffer normals.
        /// The overhead is negligible from desktop GPUs, while it should be avoided for mobile GPUs.
        /// </summary>
        public bool accurateGbufferNormals
        {
            get => m_AccurateGbufferNormals;
            set
            {
                SetDirty();
                m_AccurateGbufferNormals = value;
            }
        }

        public bool tiledDeferredShading
        {
            get => m_TiledDeferredShading;
            set
            {
                SetDirty();
                m_TiledDeferredShading = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Upon asset creation, OnEnable is called and `shaders` reference is not yet initialized
            // We need to call the OnEnable for data migration when updating from old versions of UniversalRP that
            // serialized resources in a different format. Early returning here when OnEnable is called
            // upon asset creation is fine because we guarantee new assets get created with all resources initialized.
            if (shaders == null)
                return;

#if UNITY_EDITOR
            ResourceReloader.TryReloadAllNullIn(this, DistillingRenderPipelineAsset.packagePath);
            ResourceReloader.TryReloadAllNullIn(postProcessData, DistillingRenderPipelineAsset.packagePath);
            ResourceReloader.TryReloadAllNullIn(m_SSRData, DistillingRenderPipelineAsset.packagePath);
#endif
        }
    }
}

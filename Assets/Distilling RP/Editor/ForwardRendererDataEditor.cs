using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEditor.Rendering.Distilling
{
    [CustomEditor(typeof(ForwardRendererData), true)]
    [MovedFrom("UnityEditor.Rendering.LWRP")] public class ForwardRendererDataEditor : ScriptableRendererDataEditor
    {
        private static class Styles
        {
            public static readonly GUIContent RendererTitle = new GUIContent("Postprocessing Settings", "Custom Forward Renderer for Universal RP.");
            public static readonly GUIContent PostProcessLabel = new GUIContent("Post Process Data", "The asset containing references to shaders and Textures that the Renderer uses for post-processing.");
            public static readonly GUIContent FilteringLabel = new GUIContent("Filtering Settings", "Controls filter rendering settings for this renderer.");
            public static readonly GUIContent OpaqueMask = new GUIContent("Opaque Layer Mask", "Controls which opaque layers this renderer draws.");
            public static readonly GUIContent TransparentMask = new GUIContent("Transparent Layer Mask", "Controls which transparent layers this renderer draws.");
            public static readonly GUIContent defaultStencilStateLabel = EditorGUIUtility.TrTextContent("Default Stencil State", "Configure stencil state for the opaque and transparent render passes.");
            public static readonly GUIContent shadowTransparentReceiveLabel = EditorGUIUtility.TrTextContent("Transparent Receive Shadows", "When disabled, none of the transparent objects will receive shadows.");
            public static readonly GUIContent LightingLabel = new GUIContent("Rendering Settings", "Settings related to lighting and rendering paths.");
            public static readonly GUIContent RenderingModeLabel = new GUIContent("Rendering Mode", "Select a rendering path.");

            public static readonly GUIContent GlobalIllumationLabel = new GUIContent("GlobalIllumation Settings", "Setting GlobalIllumation.");
            public static readonly GUIContent SphericalHarmonicsLabel = new GUIContent("Degree Number");
            public static readonly GUIContent BoolScreenSpaceLabel = new GUIContent("Use SSGI", "Select use Screen Space Ray Tracing.");
            public static readonly GUIContent SSRDataLabel = new GUIContent("SSR Data", "Set Screen Space Ray Tracing Data.");
            public static readonly GUIContent BoolVoxelGILabel = new GUIContent("Use VoxelGI", "Select use Voxel GI.");
            public static readonly GUIContent VXGIDataLabel = new GUIContent("VoxelGI Data", "Set Voxel GI Data.");
            
            public static readonly GUIContent accurateGbufferNormalsLabel = EditorGUIUtility.TrTextContent("G-buffer Depth Normals", "Normals in G-buffer use octahedron encoding/decoding. This improves visual quality but might reduce performance.");
            // public static readonly GUIContent tiledDeferredShadingLabel = EditorGUIUtility.TrTextContent("Tiled Deferred Shading (Experimental)", "Allows Tiled Deferred Shading on appropriate lights");
        }

        private SerializedProperty m_OpaqueLayerMask;
        private SerializedProperty m_TransparentLayerMask;
        private SerializedProperty m_RenderingMode;
        private SerializedProperty m_SphericalHarmonicsDegree;
        private SerializedProperty m_AccurateGbufferNormals;
        private SerializedProperty m_TiledDeferredShading;
        private SerializedProperty m_DefaultStencilState;
        private SerializedProperty m_screenSpaceRayTracingData;
        private SerializedProperty m_PostProcessData;
        private SerializedProperty m_Shaders;
        private SerializedProperty m_ShadowTransparentReceiveProp;
        private SerializedProperty m_BoolScreenSpaceRayTracing;
        private SerializedProperty m_BoolVoxelGI;
        private SerializedProperty m_VoxelGIData;

        private void OnEnable()
        {
            m_OpaqueLayerMask = serializedObject.FindProperty("m_OpaqueLayerMask");
            m_TransparentLayerMask = serializedObject.FindProperty("m_TransparentLayerMask");
            m_RenderingMode = serializedObject.FindProperty("m_RenderingMode");
            m_SphericalHarmonicsDegree = serializedObject.FindProperty("m_SphericalHarmonicsDegree");
            
            m_BoolScreenSpaceRayTracing = serializedObject.FindProperty("m_BoolScreenSpaceRayTracing");
            m_BoolVoxelGI = serializedObject.FindProperty("m_BoolVoxelGI");
            m_AccurateGbufferNormals = serializedObject.FindProperty("m_AccurateGbufferNormals");
            m_TiledDeferredShading = serializedObject.FindProperty("m_TiledDeferredShading");
            m_DefaultStencilState = serializedObject.FindProperty("m_DefaultStencilState");
            m_screenSpaceRayTracingData = serializedObject.FindProperty("m_SSRData");
            m_VoxelGIData = serializedObject.FindProperty("m_VXGIData");
            m_PostProcessData = serializedObject.FindProperty("postProcessData");
            m_Shaders = serializedObject.FindProperty("shaders");
            m_ShadowTransparentReceiveProp = serializedObject.FindProperty("m_ShadowTransparentReceive");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.RendererTitle, EditorStyles.boldLabel); // Title
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_PostProcessData, Styles.PostProcessLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Styles.LightingLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_RenderingMode, Styles.RenderingModeLabel);
            if (m_RenderingMode.intValue == (int)RenderingMode.Deferred)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_AccurateGbufferNormals, Styles.accurateGbufferNormalsLabel, true);
                // EditorGUILayout.PropertyField(m_TiledDeferredShading, Styles.tiledDeferredShadingLabel, true);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField(Styles.FilteringLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_OpaqueLayerMask, Styles.OpaqueMask);
            EditorGUILayout.PropertyField(m_TransparentLayerMask, Styles.TransparentMask);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();


            EditorGUILayout.LabelField(Styles.GlobalIllumationLabel, EditorStyles.boldLabel);
                
            EditorGUI.indentLevel++;
            //SH
            EditorGUILayout.PropertyField(m_SphericalHarmonicsDegree, Styles.SphericalHarmonicsLabel);
            
            //SSGI
            EditorGUILayout.PropertyField(m_BoolScreenSpaceRayTracing, Styles.BoolScreenSpaceLabel, true);
            if (m_BoolScreenSpaceRayTracing.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_screenSpaceRayTracingData, Styles.SSRDataLabel, true);
                EditorGUI.indentLevel--;
            }
            //VXGI
            EditorGUILayout.PropertyField(m_BoolVoxelGI, Styles.BoolVoxelGILabel, true);
            if (m_BoolVoxelGI.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_VoxelGIData, Styles.VXGIDataLabel, true);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Shadows Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_ShadowTransparentReceiveProp, Styles.shadowTransparentReceiveLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Overrides Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_DefaultStencilState, Styles.defaultStencilStateLabel, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI(); // Draw the base UI, contains ScriptableRenderFeatures list

            // Add a "Reload All" button in inspector when we are in developer's mode
            if (EditorPrefs.GetBool("DeveloperMode"))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_Shaders, true);

                if (GUILayout.Button("Reload All"))
                {
                    var resources = target as ForwardRendererData;
                    resources.shaders = null;
                    ResourceReloader.ReloadAllNullIn(target, DistillingRenderPipelineAsset.packagePath);
                }
            }
        }
    }
}

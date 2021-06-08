using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEditor.Rendering.Distilling
{
    [CustomEditor(typeof(SSRData), true)]
    public class SSRDataEditor : Editor
    {
        private static class Styles
        {
            public static readonly GUIContent RayCastLable = new GUIContent("Ray Cast", "Ray Cast.");
            public static readonly GUIContent ResolveLabel = new GUIContent("Resolve", "Resolve.");
            public static readonly GUIContent TemporalLabel = new GUIContent("Temporal", "Temporal.");
            public static readonly GUIContent GeneralLabel = new GUIContent("General", "General.");
            public static readonly GUIContent DebugLabel = new GUIContent("Debug", "Debug.");
        }
        
        private SerializedProperty m_depthMode;
        private SerializedProperty m_rayMode;
        private SerializedProperty m_rayDistance;
        
        private SerializedProperty m_BRDFBias;
        private SerializedProperty m_resolveMode;
        private SerializedProperty m_rayReuse;
        private SerializedProperty m_normalization;
        private SerializedProperty m_reduceFireflies;
        private SerializedProperty m_useMipMap;
        private SerializedProperty m_useTemporal;
        private SerializedProperty m_scale;
        private SerializedProperty m_response;
        private SerializedProperty m_useUnityMotion;

        private SerializedProperty m_useFresnel;
        private SerializedProperty m_screenFadeSize;
        private SerializedProperty m_smoothnessRange;
        private SerializedProperty m_debugPass;
        
        private void OnEnable()
        {
            m_depthMode = serializedObject.FindProperty("m_OpaqueLayerMask");
            m_rayMode = serializedObject.FindProperty("m_TransparentLayerMask");
            m_rayDistance = serializedObject.FindProperty("m_RenderingMode");
            m_BRDFBias = serializedObject.FindProperty("m_BoolScreenSpaceRayTracing");
            m_resolveMode = serializedObject.FindProperty("m_AccurateGbufferNormals");
            m_rayReuse = serializedObject.FindProperty("m_TiledDeferredShading");
            m_normalization = serializedObject.FindProperty("m_DefaultStencilState");
            m_reduceFireflies = serializedObject.FindProperty("m_SSRData");
            m_useMipMap = serializedObject.FindProperty("postProcessData");
            m_useTemporal = serializedObject.FindProperty("shaders");
            m_scale = serializedObject.FindProperty("m_ShadowTransparentReceive");
            
            m_response = serializedObject.FindProperty("m_ShadowTransparentReceive");
            m_useUnityMotion = serializedObject.FindProperty("m_ShadowTransparentReceive");
            m_useFresnel = serializedObject.FindProperty("m_ShadowTransparentReceive");
            m_screenFadeSize = serializedObject.FindProperty("m_ShadowTransparentReceive");
            m_smoothnessRange = serializedObject.FindProperty("m_ShadowTransparentReceive");
            m_debugPass = serializedObject.FindProperty("m_ShadowTransparentReceive");
        }
    }   
}

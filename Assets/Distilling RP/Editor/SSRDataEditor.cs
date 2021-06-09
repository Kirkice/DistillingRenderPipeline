using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEditor.Rendering.Distilling
{
    [CustomEditor(typeof(SSRData), true)]
    public class SSRDataEditor : ScriptableRendererDataEditor
    {
        private static class Styles
        {
            public static readonly GUIContent RayCastLable = new GUIContent("Ray Cast", "Ray Cast.");
            public static readonly GUIContent ResolveLabel = new GUIContent("Resolve", "Resolve.");
            public static readonly GUIContent TemporalLabel = new GUIContent("Temporal", "Temporal.");
            public static readonly GUIContent GeneralLabel = new GUIContent("General", "General.");
            public static readonly GUIContent DebugLabel = new GUIContent("Debug", "Debug.");
            
            public static readonly GUIContent depthModeLabel = new GUIContent("Depth Mode", "DepthMode.");
            public static readonly GUIContent rayModeLabel = new GUIContent("Ray Mode", "RayMode.");
            public static readonly GUIContent rayDistanceLabel = new GUIContent("Ray Distance", "RayDistance.");
            
            public static readonly GUIContent BRDFBiasLabel = new GUIContent("BRDF Bias", "BRDFBias.");
            public static readonly GUIContent resolveModeLabel = new GUIContent("Resolve Mode", "ResolveMode.");
            public static readonly GUIContent rayReuseLabel = new GUIContent("Ray Reuse", "RayReuse.");
            public static readonly GUIContent normalizationLabel = new GUIContent("Normalization", "Normalization.");
            public static readonly GUIContent reduceFirefliesLabel = new GUIContent("Reduce Fireflies", "reduceFireflies.");
            public static readonly GUIContent useMipMapLabel = new GUIContent("Use MipMap", "useMipMap.");
            
            public static readonly GUIContent useTemporalLabel = new GUIContent("Use Temporal", "useTemporal.");
            public static readonly GUIContent scaleLabel = new GUIContent("Scale", "scale.");
            public static readonly GUIContent responseLabel = new GUIContent("Response", "response.");
            public static readonly GUIContent useUnityMotionLabel = new GUIContent("Use Unity Motion", "useUnityMotion.");
            
            public static readonly GUIContent useFresnelLabel = new GUIContent("Use Fresnel", "useFresnel.");
            public static readonly GUIContent screenFadeSizeLabel = new GUIContent("Screen Fade Size", "screenFadeSize.");
            
            public static readonly GUIContent smoothnessRangeLabel = new GUIContent("Smoothness Range", "smoothnessRange.");
            public static readonly GUIContent debugPassLabel = new GUIContent("Debug Pass", "debugPass.");
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
            m_depthMode = serializedObject.FindProperty("depthMode");
            m_rayMode = serializedObject.FindProperty("rayMode");
            m_rayDistance = serializedObject.FindProperty("rayDistance");
            m_BRDFBias = serializedObject.FindProperty("BRDFBias");
            m_resolveMode = serializedObject.FindProperty("resolveMode");
            m_rayReuse = serializedObject.FindProperty("rayReuse");
            m_normalization = serializedObject.FindProperty("normalization");
            m_reduceFireflies = serializedObject.FindProperty("reduceFireflies");
            m_useMipMap = serializedObject.FindProperty("useMipMap");
            m_useTemporal = serializedObject.FindProperty("useTemporal");
            m_scale = serializedObject.FindProperty("scale");
            
            m_response = serializedObject.FindProperty("response");
            m_useUnityMotion = serializedObject.FindProperty("useUnityMotion");
            m_useFresnel = serializedObject.FindProperty("useFresnel");
            m_screenFadeSize = serializedObject.FindProperty("screenFadeSize");
            m_smoothnessRange = serializedObject.FindProperty("smoothnessRange");
            m_debugPass = serializedObject.FindProperty("debugPass");
        } 
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.RayCastLable, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_depthMode, Styles.depthModeLabel);
            EditorGUILayout.PropertyField(m_rayMode, Styles.rayModeLabel);
            EditorGUILayout.PropertyField(m_rayDistance, Styles.rayDistanceLabel);
            EditorGUILayout.PropertyField(m_BRDFBias, Styles.BRDFBiasLabel);
            EditorGUI.indentLevel--; 
            EditorGUILayout.Space();

            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.ResolveLabel, EditorStyles.boldLabel); 
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_resolveMode, Styles.resolveModeLabel);
            EditorGUILayout.PropertyField(m_rayReuse, Styles.rayReuseLabel);
            EditorGUILayout.PropertyField(m_normalization, Styles.normalizationLabel);
            EditorGUILayout.PropertyField(m_reduceFireflies, Styles.reduceFirefliesLabel);
            EditorGUILayout.PropertyField(m_useMipMap, Styles.useMipMapLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.TemporalLabel, EditorStyles.boldLabel); 
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_useTemporal, Styles.useTemporalLabel);
            EditorGUILayout.PropertyField(m_scale, Styles.scaleLabel);
            EditorGUILayout.PropertyField(m_response, Styles.responseLabel);
            EditorGUILayout.PropertyField(m_useUnityMotion, Styles.useUnityMotionLabel);
            EditorGUI.indentLevel--; 
            EditorGUILayout.Space();
            
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.GeneralLabel, EditorStyles.boldLabel); 
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_useFresnel, Styles.useFresnelLabel);
            EditorGUILayout.PropertyField(m_screenFadeSize, Styles.screenFadeSizeLabel);
            EditorGUI.indentLevel--; 
            EditorGUILayout.Space();
            
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.DebugLabel, EditorStyles.boldLabel); 
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_smoothnessRange, Styles.smoothnessRangeLabel);
            EditorGUILayout.PropertyField(m_debugPass, Styles.debugPassLabel);
            EditorGUI.indentLevel--; 
            EditorGUILayout.Space();
            
            serializedObject.ApplyModifiedProperties();
        }
    }   
}

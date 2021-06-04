using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

namespace UnityEditor.Rendering.Distilling
{
    [CustomEditor(typeof(PostProcessData), true)]
    public class PostProcessDataEditor : Editor
    {
        SerializedProperty m_Shaders;
        SerializedProperty m_Textures;

        private void OnEnable()
        {
            m_Shaders = serializedObject.FindProperty("shaders");
            m_Textures = serializedObject.FindProperty("textures");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Add a "Reload All" button in inspector when we are in developer's mode
            if (EditorPrefs.GetBool("DeveloperMode"))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_Shaders, true);
                EditorGUILayout.PropertyField(m_Textures, true);

                if (GUILayout.Button("Reload All"))
                {
                    var resources = target as PostProcessData;
                    resources.shaders = null;
                    resources.textures = null;
                    ResourceReloader.ReloadAllNullIn(target, DistillingRenderPipelineAsset.packagePath);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

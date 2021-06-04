using UnityEngine;
using UnityEngine.Rendering.Distilling;

namespace UnityEditor.Rendering.Distilling
{
    [CustomEditor(typeof(ScriptableRendererFeature), true)]
    public class ScriptableRendererFeatureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }
    }
}

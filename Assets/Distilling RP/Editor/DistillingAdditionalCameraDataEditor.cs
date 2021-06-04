using UnityEngine.Rendering.Distilling;

namespace UnityEditor.Rendering.Distilling
{
    [CanEditMultipleObjects]
    // Disable the GUI for additional camera data
    [CustomEditor(typeof(DistillingAdditionalCameraData))]
    class DistillingAdditionalCameraDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }
        [MenuItem("CONTEXT/UniversalAdditionalCameraData/Remove Component")]
        static void RemoveComponent(MenuCommand command)
        {
            EditorUtility.DisplayDialog("Component Info", "You can not delete this component, you will have to remove the camera.", "OK");
        }
    }
}

using UnityEngine.Rendering.Distilling;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEditor.Rendering.Distilling
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DistillingAdditionalLightData))]
    [MovedFrom("UnityEditor.Rendering.LWRP")] public class DistillingAdditionLightDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }

        [MenuItem("CONTEXT/LWRPAdditionalLightData/Remove Component")]
        static void RemoveComponent(MenuCommand command)
        {
            if (EditorUtility.DisplayDialog("Remove Component?", "Are you sure you want to remove this component? If you do, you will lose some settings.", "Remove", "Cancel"))
            {
                Undo.DestroyObjectImmediate(command.context);
            }
        }
    }
}

using UnityEditor.Rendering.Distilling.Internal;

namespace UnityEditor.Rendering.Distilling
{
    internal static class NewRendererFeatureDropdownItem
    {
        static readonly string defaultNewClassName = "CustomRenderPassFeature.cs";

        [MenuItem("Assets/Create/Rendering/Distilling Render Pipeline/Renderer Feature", priority = EditorUtils.lwrpAssetCreateMenuPriorityGroup2)]
        internal static void CreateNewRendererFeature()
        {
            string templatePath = AssetDatabase.GUIDToAssetPath(ResourceGuid.rendererTemplate);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultNewClassName);
        }
    }
}

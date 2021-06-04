using UnityEngine.Rendering.Distilling;

namespace UnityEditor.Rendering.Distilling
{
    [VolumeComponentEditor(typeof(Vignette))]
    sealed class VignetteEditor : VolumeComponentEditor
    {
        public override void OnInspectorGUI()
        {
            if (DistillingRenderPipeline.asset?.postProcessingFeatureSet == PostProcessingFeatureSet.PostProcessingV2)
            {
                EditorGUILayout.HelpBox(DistillingRenderPipelineAssetEditor.Styles.postProcessingGlobalWarning, MessageType.Warning);
                return;
            }

            base.OnInspectorGUI();
        }
    }
}

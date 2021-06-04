using UnityEngine.Rendering.Distilling;

namespace UnityEditor.Rendering.Distilling
{
    [VolumeComponentEditor(typeof(Tonemapping))]
    sealed class TonemappingEditor : VolumeComponentEditor
    {
        SerializedDataParameter m_Mode;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Tonemapping>(serializedObject);

            m_Mode = Unpack(o.Find(x => x.mode));
        }

        public override void OnInspectorGUI()
        {
            if (DistillingRenderPipeline.asset?.postProcessingFeatureSet == PostProcessingFeatureSet.PostProcessingV2)
            {
                EditorGUILayout.HelpBox(DistillingRenderPipelineAssetEditor.Styles.postProcessingGlobalWarning, MessageType.Warning);
                return;
            }

            PropertyField(m_Mode);

            // Display a warning if the user is trying to use a tonemap while rendering in LDR
            if (DistillingRenderPipeline.asset?.supportsHDR == false)
                EditorGUILayout.HelpBox("Tonemapping should only be used when working in HDR.", MessageType.Warning);
        }
    }
}

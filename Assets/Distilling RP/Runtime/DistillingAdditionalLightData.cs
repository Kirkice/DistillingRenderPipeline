using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Distilling
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public class DistillingAdditionalLightData : MonoBehaviour
    {
        [Tooltip("Controls the usage of pipeline settings.")]
        [SerializeField] bool m_UsePipelineSettings = true;

        public bool usePipelineSettings
        {
            get { return m_UsePipelineSettings; }
            set { m_UsePipelineSettings = value; }
        }
    }
}

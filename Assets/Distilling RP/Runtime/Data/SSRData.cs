#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Distilling
{
    [System.Serializable]
    public enum ResolutionMode
    {
        halfRes = 2,
        fullRes = 1,
    };
    
    [System.Serializable]
    public enum SSRDebugPass
    {
        Combine,
        Reflection,
        Cubemap,
        ReflectionAndCubemap,
        SSRMask,
        CombineNoCubemap,
        RayCast,
        Jitter,
    };

    public class SSRData : ScriptableObject
    {
        /// <summary>
        /// RayCast
        /// </summary>
        [SerializeField] public ResolutionMode depthMode = ResolutionMode.halfRes;
        [SerializeField] public ResolutionMode rayMode = ResolutionMode.halfRes;
        [SerializeField] public int rayDistance = 70;
        [Range(0.00001f, 1.0f)]
        [SerializeField] public float thickness = 0.1f;
        [Range(0.00001f, 1.0f)]
        [SerializeField] public float BRDFBias = 0.7f;
        
        /// <summary>
        /// Resolve
        /// </summary>
        [SerializeField] public ResolutionMode resolveMode = ResolutionMode.fullRes;
        [SerializeField] public bool rayReuse = true;
        [SerializeField] public bool normalization = true;
        [SerializeField] public bool reduceFireflies = true;
        [SerializeField] public bool useMipMap = true;
        
        /// <summary>
        /// Temporal
        /// </summary>
        [SerializeField] public bool useTemporal = true;
        [SerializeField] public float scale = 2.0f;
        [Range(0.0f, 1.0f)]
        [SerializeField] public float response = 0.85f;
        [SerializeField] public bool useUnityMotion;
        
        /// <summary>
        /// General
        /// </summary>
        [SerializeField] public bool useFresnel = true;
        [Range(0.0f, 1.0f)]
        [SerializeField] public float screenFadeSize = 0.25f;
        
        /// <summary>
        /// Debug
        /// </summary>
        [Range(0.0f, 1.0f)]
        [SerializeField] public float smoothnessRange = 1.0f;
        [SerializeField] public SSRDebugPass debugPass = SSRDebugPass.Combine;
        
    }
}

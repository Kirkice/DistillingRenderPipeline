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
        [SerializeField] ResolutionMode depthMode = ResolutionMode.halfRes;
        [SerializeField] ResolutionMode rayMode = ResolutionMode.halfRes;
        [SerializeField] int rayDistance = 70;
        [Range(0.00001f, 1.0f)]
        [SerializeField] float BRDFBias = 0.7f;
        
        /// <summary>
        /// Resolve
        /// </summary>
        [SerializeField] ResolutionMode resolveMode = ResolutionMode.fullRes;
        [SerializeField] bool rayReuse = true;
        [SerializeField] bool normalization = true;
        [SerializeField] bool reduceFireflies = true;
        [SerializeField] bool useMipMap = true;
        
        /// <summary>
        /// Temporal
        /// </summary>
        [SerializeField] bool useTemporal = true;
        [SerializeField] float scale = 2.0f;
        [Range(0.0f, 1.0f)]
        [SerializeField] float response = 0.85f;
        [SerializeField] bool useUnityMotion;
        
        /// <summary>
        /// General
        /// </summary>
        [SerializeField] bool useFresnel = true;
        [Range(0.0f, 1.0f)]
        [SerializeField] float screenFadeSize = 0.25f;
        
        /// <summary>
        /// Debug
        /// </summary>
        [Range(0.0f, 1.0f)]
        [SerializeField] float smoothnessRange = 1.0f;
        [SerializeField] SSRDebugPass debugPass = SSRDebugPass.Combine;
        
    }
}

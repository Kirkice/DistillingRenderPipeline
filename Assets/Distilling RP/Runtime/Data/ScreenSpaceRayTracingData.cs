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
        [SerializeField]
        public static ResolutionMode depthMode = ResolutionMode.halfRes;
        [SerializeField]
        public static ResolutionMode rayMode = ResolutionMode.halfRes;
        [SerializeField]
        public static int rayDistance = 70;
        [Range(0.00001f, 1.0f)]
        [SerializeField]
        public static float BRDFBias = 0.7f;
        
        /// <summary>
        /// Resolve
        /// </summary>
        [SerializeField]
        public static ResolutionMode resolveMode = ResolutionMode.fullRes;
        [SerializeField]
        public static bool rayReuse = true;
        [SerializeField]
        public static bool normalization = true;
        [SerializeField]
        public static bool reduceFireflies = true;
        [SerializeField]
        public static bool useMipMap = true;
        
        /// <summary>
        /// Temporal
        /// </summary>
        [SerializeField]
        public static bool useTemporal = true;
        [SerializeField]
        public static float scale = 2.0f;
        [Range(0.0f, 1.0f)]
        [SerializeField]
        public static float response = 0.85f;
        public static bool useUnityMotion;
        
        /// <summary>
        /// General
        /// </summary>
        [SerializeField]
        public static bool useFresnel = true;
        [Range(0.0f, 1.0f)]
        [SerializeField]
        public static float screenFadeSize = 0.25f;
        
        /// <summary>
        /// Debug
        /// </summary>
        [Range(0.0f, 1.0f)]
        [SerializeField]
        public static float smoothnessRange = 1.0f;
        public static SSRDebugPass debugPass = SSRDebugPass.Combine;
        
    }
}

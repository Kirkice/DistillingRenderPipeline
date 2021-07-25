Shader "Hidden/Distilling RP/ScreenSpaceRayTracing" 
{
    SubShader 
    {
		ZTest Always Cull Off ZWrite Off
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        
        Pass    //0
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_Resolve
            ENDHLSL
        }
        
        Pass    //1
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS
            ENDHLSL
        }
        
        Pass    //2
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_Combine
            ENDHLSL
        }
        
        Pass    //3
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_RayCast
            ENDHLSL
        }
        
        Pass    //4
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_Depth
            ENDHLSL
        }
        
        Pass    //5
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_Temporal
            ENDHLSL
        }
        
        Pass    //6
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_MipMapBlur
            ENDHLSL
        }
        
        Pass    //7
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_Debug
            ENDHLSL
        }
        
        Pass    //8
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_Recursive
            ENDHLSL
        }
        
        Pass    //9
        {
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_RayCastMask
            ENDHLSL
        }
    }
}
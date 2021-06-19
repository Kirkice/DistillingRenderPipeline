Shader "Hidden/Distilling RP/ScreenSpaceRayTracing" 
{
    SubShader 
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #include "ScreenSpaceRayTracingPass.hlsl"
            #pragma vertex VS
            #pragma fragment PS_Temporal
            ENDHLSL
        }
    }
}
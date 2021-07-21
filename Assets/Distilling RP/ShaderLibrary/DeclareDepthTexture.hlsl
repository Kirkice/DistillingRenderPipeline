#ifndef UNITY_DECLARE_DEPTH_TEXTURE_INCLUDED
#define UNITY_DECLARE_DEPTH_TEXTURE_INCLUDED
#include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"

TEXTURE2D_X_FLOAT(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

float SampleSceneDepth(float2 uv)
{
    float z = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv)).r;
    #if defined(UNITY_REVERSED_Z)
    z = 1.0f - z;
    #endif
    return z;
}

float LoadSceneDepth(uint2 uv)
{
    return LOAD_TEXTURE2D_X(_CameraDepthTexture, uv).r;
}
#endif

/* $Header: /SSR/SSR_Lib.hlsl         6/09/21 23:35p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : SSR_Lib.hlsl                                                 *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#include "SSR_Input.hlsl"

#define     PI                                          3.141592

/// <summary>
/// Sqrt
/// </summary>
float sqr(float x)
{
    return                                              x * x;
}

/// <summary>
/// fract
/// </summary>
float fract(float x)
{
    return                                              x - floor( x );
}

/// <summary>
/// GetSampleColor
/// </summary>
float4 GetSampleColor (TEXTURE2D(Tex), SAMPLER(sampler_Tex),float2 uv)
{
    return                                              SAMPLE_TEXTURE2D(Tex, sampler_Tex, uv);
}

/// <summary>
/// GetCubeMap
/// </summary>
float4 GetCubeMap (float2 uv)
{
    return                                              tex2D(_CameraReflectionsTexture, uv);
}

/// <summary>
/// GetAlbedo
/// </summary>
float4 GetAlbedo (float2 uv)
{
    return                                              tex2D(_CameraGBufferTexture0, uv);
}

/// <summary>
/// GetSpecular
/// </summary>
float4 GetSpecular (float2 uv)
{
    return                                              tex2D(_CameraGBufferTexture1, uv);
}

/// <summary>
/// GetRoughness
/// </summary>
float GetRoughness (float smoothness)
{
    return                                              max(min(_SmoothnessRange, 1 - smoothness), 0.05f);
}

/// <summary>
/// GetNormal
/// </summary>
float4 GetNormal (float2 uv) 
{ 
    float4                                              gbuffer2 = tex2D(_CameraGBufferTexture2, uv);
    return                                              gbuffer2*2-1;
}

/// <summary>
/// GetVelocity
/// </summary>
float4 GetVelocity(float2 uv)
{
    return                                              tex2D(_CameraMotionVectorsTexture, uv);
}

/// <summary>
/// GetReflection
/// </summary>
float4 GetReflection(float2 uv)
{
    return                                              tex2D(_ReflectionBuffer, uv);
}

/// <summary>
/// ComputeDepth
/// </summary>
float ComputeDepth(float4 clippos)
{
    #if defined(SHADER_TARGET_GLSL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
    return                                              (clippos.z / clippos.w) * 0.5 + 0.5;
    #else
    return                                              clippos.z / clippos.w;
    #endif
}

/// <summary>
/// GetViewNormal
/// </summary>
float3 GetViewNormal (float3 normal)
{
    float3 viewNormal                                   =  mul((float3x3)_WorldToCameraMatrix, normal.rgb);
    return                                              normalize(viewNormal);
}

/// <summary>
/// GetScreenPos
/// </summary>
float3 GetScreenPos (float2 uv, float depth)
{
    return                                              float3(uv.xy * 2 - 1, depth);
}

/// <summary>
/// GetViewRayFromUv
/// </summary>
float3 GetViewRayFromUv(float2 uv)
{
    float4 _CamScreenDir                                = float4(1.0 / _ProjectionMatrix[0][0], 1.0 / _ProjectionMatrix[1][1], 1, 1);
    float3 ray                                          = float3(uv.x * 2 - 1, uv.y * 2 - 1, 1);
    ray                                                 *= _CamScreenDir.xyz;
    ray                                                 = ray * (_ProjectionParams.z / ray.z);
    return ray;
}

/// <summary>
/// GetWorlPos
/// </summary>
float3 GetWorlPos (float3 screenPos)
{
    float4 worldPos                                     = mul(_InverseViewProjectionMatrix, float4(screenPos, 1));
    return                                              worldPos.xyz / worldPos.w;
}

/// <summary>
/// GetViewPos
/// </summary>
float3 GetViewPos (float3 screenPos)
{
    float4 viewPos                                      = mul(_InverseProjectionMatrix, float4(screenPos, 1));
    return                                              viewPos.xyz / viewPos.w;
}

/// <summary>
/// GetViewDir
/// </summary>
float3 GetViewDir (float3 worldPos)
{
    return                                              normalize(worldPos - _WorldSpaceCameraPos);
}

static const float2 offset[4] =
{
    float2(0,            0),
    float2(2,           -2),
    float2(-2,          -2),
    float2(0,            2)
};

/// <summary>
/// RayAttenBorder
/// </summary>
float RayAttenBorder (float2 pos, float value)
{
    float borderDist                                    = min(1.0 - max(pos.x, pos.y), min(pos.x, pos.y));
    return                                              saturate(borderDist > value ? 1.0 : borderDist / value);
}

/// <summary>
/// CalculateMotion
/// </summary>
inline half2 CalculateMotion(float rawDepth, float2 inUV)
{
    float3 screenPos                                    = GetScreenPos(inUV, rawDepth);
    float4 worldPos                                     = float4(GetWorlPos(screenPos),1);
                                
    float4 prevClipPos                                  = mul(_PrevViewProjectionMatrix, worldPos);
    float4 curClipPos                                   = mul(_ViewProjectionMatrix, worldPos);

    float2 prevHPos                                     = prevClipPos.xy / prevClipPos.w;
    float2 curHPos                                      = curClipPos.xy / curClipPos.w;

    // V is the viewport position at this pixel in the range 0 to 1.
    float2 vPosPrev                                     = (prevHPos.xy + 1.0f) / 2.0f;
    float2 vPosCur                                      = (curHPos.xy + 1.0f) / 2.0f;
    return                                              vPosCur - vPosPrev;
}
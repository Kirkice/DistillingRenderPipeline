/* $Header: /SSR/SSR_Lib.hlsl         6/09/21 23:35p KirkZhu $ */
/*--------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : SSR_Lib.hlsl                                                 *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#include "SSR_Input.hlsl"
#include "NoiseLib.hlsl"
#include "BRDFLib.hlsl"

/// <summary>
/// Sqrt
/// </summary>
float sqr(float x)
{
    return x * x;
}

/// <summary>
/// fract
/// </summary>
float fract(float x)
{
    return x - floor( x );
}

/// <summary>
/// GetSampleColor
/// </summary>
float4 GetSampleColor (TEXTURE2D(Tex), SAMPLER(sampler_Tex),float2 uv)
{
    return SAMPLE_TEXTURE2D(Tex, sampler_Tex, uv);
}

/// <summary>
/// GetCubeMap
/// </summary>
float4 GetCubeMap (float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraReflectionsTexture, sampler_CameraReflectionsTexture, uv);
}

/// <summary>
/// GetAlbedo
/// </summary>
float4 GetAlbedo (float2 uv)
{
    return SAMPLE_TEXTURE2D(_GBuffer0, sampler_GBuffer0, uv);
}

/// <summary>
/// GetSpecular
/// </summary>
float4 GetSpecular (float2 uv)
{
    return SAMPLE_TEXTURE2D(_GBuffer1, sampler_GBuffer1, uv);
}

/// <summary>
/// GetRoughness
/// </summary>
float GetRoughness (float smoothness)
{
    return max(min(_SmoothnessRange, 1 - smoothness), 0.05f);
}

/// <summary>
/// GetNormal
/// </summary>
float4 GetNormal (float2 uv) 
{
    float4 normal                                                                               = SAMPLE_TEXTURE2D(_GBuffer2, sampler_GBuffer2, uv);
    normal.rgb                                                                                  = normal.rgb * 2 - 1;
    return                                                                                      normal;
}

/// <summary>
/// GetRoughness
/// </summary>
float4 GetRoughness (float2 uv) 
{
    float4 color                                                                                 = SAMPLE_TEXTURE2D(_GBuffer2, sampler_GBuffer2, uv);
    return color.aaaa;
}

/// <summary>
/// GetVelocity
/// </summary>
float4 GetVelocity(float2 uv)
{
    return SAMPLE_TEXTURE2D(_MotionVectorTexture , sampler_MotionVectorTexture , uv);
}

/// <summary>
/// GetReflection
/// </summary>
float4 GetReflection(float2 uv)
{
    return SAMPLE_TEXTURE2D(_ReflectionBuffer, sampler_ReflectionBuffer, uv);
}

/// <summary>
/// ComputeDepth
/// </summary>
float ComputeDepth(float4 clippos)
{
    #if defined(SHADER_TARGET_GLSL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
    return (clippos.z / clippos.w) * 0.5 + 0.5;
    #else
    return clippos.z / clippos.w;
    #endif
}

/// <summary>
/// GetViewNormal
/// </summary>
float3 GetViewNormal (float3 normal)
{
    float3 viewNormal                                                                       =  mul((float3x3)_WorldToCameraMatrix, normal.rgb);
    return normalize(viewNormal);
}

/// <summary>
/// GetScreenPos
/// </summary>
float3 GetScreenPos (float2 uv, float depth)
{
    return float3(uv.xy * 2 - 1, depth);
}

/// <summary>
/// GetViewRayFromUv
/// </summary>
float3 GetViewRayFromUv(float2 uv)
{
    float4 _CamScreenDir                                                                    = float4(1.0 / _ProjectionMatrix[0][0], 1.0 / _ProjectionMatrix[1][1], 1, 1);
    float3 ray                                                                              = float3(uv.x * 2 - 1, uv.y * 2 - 1, 1);
    ray                                                                                     *= _CamScreenDir.xyz;
    ray                                                                                     = ray * (_ProjectionParams.z / ray.z);
    return                                                                                  ray;
}

/// <summary>
/// GetWorlPos
/// </summary>
float3 GetWorlPos (float3 screenPos)
{
    float4 worldPos                                                                         = mul(_InverseViewProjectionMatrix, float4(screenPos, 1));
    return worldPos.xyz / worldPos.w;
}

/// <summary>
/// GetViewPos
/// </summary>
float3 GetViewPos (float3 screenPos)
{
    float4 viewPos                                                                          = mul(_InverseProjectionMatrix, float4(screenPos, 1));
    return viewPos.xyz / viewPos.w;
}

/// <summary>
/// GetViewDir
/// </summary>
float3 GetViewDir (float3 worldPos)
{
    return normalize(worldPos - _WorldSpaceCameraPos);
}

static const float2 offset[4] =
{
    float2(0, 0),
    float2(2, -2),
    float2(-2, -2),
    float2(0, 2)
};

/// <summary>
/// RayAttenBorder
/// </summary>
float RayAttenBorder (float2 pos, float value)
{
    float borderDist                                                                        = min(1.0 - max(pos.x, pos.y), min(pos.x, pos.y));
    return saturate(borderDist > value ? 1.0 : borderDist / value);
}

/// <summary>
/// CalculateMotion
/// </summary>
inline half2 CalculateMotion(float rawDepth, float2 inUV)
{
    float3 screenPos                                                                        = GetScreenPos(inUV, rawDepth);
    float4 worldPos                                                                         = float4(GetWorlPos(screenPos),1);

    float4 prevClipPos                                                                      = mul(_PrevViewProjectionMatrix, worldPos);
    float4 curClipPos                                                                       = mul(_ViewProjectionMatrix, worldPos);

    float2 prevHPos                                                                         = prevClipPos.xy / prevClipPos.w;
    float2 curHPos                                                                          = curClipPos.xy / curClipPos.w;

    // V is the viewport position at this pixel in the range 0 to 1.
    float2 vPosPrev                                                                         = (prevHPos.xy + 1.0f) / 2.0f;
    float2 vPosCur                                                                          = (curHPos.xy + 1.0f) / 2.0f;
    return                                                                                  vPosCur - vPosPrev;
}

/// <summary>
/// GetCommonVector
/// </summary>
void GetCommonVector_Temporal(float2 setUV, inout float2 uv, inout float depth, inout float3 PosS, inout float3 hitPacked, inout float2 velocity)
{
    uv                                                                                      = setUV;
    depth                                                                                   = SampleSceneDepth(uv);
    PosS                                                                                    = GetScreenPos(uv, depth);
    hitPacked                                                                               = SAMPLE_TEXTURE2D(_RayCast,sampler_RayCast,uv);

    if(_ReflectionVelocity == 1)
    {
        #if defined(UNITY_REVERSED_Z)
        hitPacked.z                                                                         = 1.0f - hitPacked.z;
        #endif
        velocity                                                                            = CalculateMotion(hitPacked.z,uv);
    }
    else
        velocity                                                                            = GetVelocity(uv);
}

/// <summary>
/// GetAverageDepthAndPreUV
/// </summary>
void GetAverageDepthAndPreUV(float3 hitPacked, float2 velocity, float2 uv, inout float2 averageDepthMin, inout float2 averageDepthMax,inout float2 prevUV)
{
        averageDepthMin                                                                     = min(hitPacked, velocity);
        averageDepthMax                                                                     = max(hitPacked, velocity);
        prevUV                                                                              = uv - velocity;
}

/// <summary>
/// GetSampleColorCurrent
/// </summary>
void GetSampleColorCurrent(float2 uv, float2 preUV, inout float4 current, inout float4 previous, inout float4 currentTopLeft, inout float4 currentTopCenter,
    inout float4 currentTopRight, inout float4 currentMiddleLeft, inout float4 currentMiddleCenter, inout float4 currentMiddleRight, inout float4 currentBottomLeft,
    inout float4 currentBottomCenter, inout float4 currentBottomRight)
{
    current                                                                                 = GetSampleColor(_MainTex, sampler_MainTex,uv);
    previous                                                                                = GetSampleColor(_PreviousBuffer, sampler_PreviousBuffer, preUV);

    float2 du                                                                               = float2(1.0 / _ScreenSize.x, 0.0);
    float2 dv                                                                               = float2(0.0, 1.0 / _ScreenSize.y);

    currentTopLeft                                                                          = GetSampleColor(_MainTex, sampler_MainTex, uv.xy - dv - du);
    currentTopCenter                                                                        = GetSampleColor(_MainTex, sampler_MainTex, uv.xy - dv);
    currentTopRight                                                                         = GetSampleColor(_MainTex, sampler_MainTex, uv.xy - dv + du);
    currentMiddleLeft                                                                       = GetSampleColor(_MainTex, sampler_MainTex, uv.xy - du);
    currentMiddleCenter                                                                     = GetSampleColor(_MainTex, sampler_MainTex, uv.xy);
    currentMiddleRight                                                                      = GetSampleColor(_MainTex, sampler_MainTex, uv.xy + du);
    currentBottomLeft                                                                       = GetSampleColor(_MainTex, sampler_MainTex, uv.xy + dv - du);
    currentBottomCenter                                                                     = GetSampleColor(_MainTex, sampler_MainTex, uv.xy + dv);
    currentBottomRight                                                                      = GetSampleColor(_MainTex, sampler_MainTex, uv.xy + dv + du);
}

/// <summary>
/// GetCurrentMaxMin
/// </summary>
void GetCurrentMaxMin(inout float4 currentMin, inout float4 currentMax)
{
    float scale                                                                             = _TScale;

    float4 center                                                                           = (currentMin + currentMax) * 0.5f;
    currentMin                                                                              = (currentMin - center) * scale + center;
    currentMax                                                                              = (currentMax - center) * scale + center;
}

/// <summary>
/// GetCommonVector_Resolve
/// </summary>
void GetCommonVector_Resolve(float2 setUV, inout float roughness, inout float depth,inout float NoV, inout float coneTangent, inout float maxMipLevel,
    inout float2 uv, inout int2 pos, inout float2 random, inout float2 blueNoise,inout float3 viewNormal, inout float3 screenPos, inout float3 worldPos, inout float3 viewPos, inout float3 viewDir,
    inout float4 worldNormal, inout float4 specular)
{
    uv                                                                                      = setUV;
    pos                                                                                     = uv * _ScreenSize.xy;

    worldNormal                                                                             = GetNormal (uv);
    viewNormal                                                                              = GetViewNormal (worldNormal);
    specular                                                                                = GetSpecular (uv);
    roughness                                                                               = GetRoughness (specular.a);

    depth                                                                                   = SampleSceneDepth(uv);
    screenPos                                                                               = GetScreenPos(uv, depth);
    worldPos                                                                                = GetWorlPos(screenPos);
    viewPos                                                                                 = GetViewPos(screenPos);
    viewDir                                                                                 = GetViewDir(worldPos);

    random                                                                                  = RandN2(pos, _SinTime.xx * _UseTemporal);
    blueNoise                                                                               = SAMPLE_TEXTURE2D(_Noise, sampler_Noise,(uv + _JitterSizeAndOffset.zw) * _ScreenSize.xy / _NoiseSize.xy) * 2.0 - 1.0;

    NoV                                                                                     = saturate(dot(worldNormal, -viewDir));
    coneTangent                                                                             = lerp(0.0, roughness * (1.0 - _BRDFBias), NoV * sqrt(roughness));
    maxMipLevel                                                                             = (float)_MaxMipMap - 1.0;
    
}

/// <summary>
/// GetOffsetRotationMatrix
/// </summary>
float2x2 GetOffsetRotationMatrix(float2 blueNoise)
{
    return                                                                                  float2x2(blueNoise.x, blueNoise.y, -blueNoise.y, blueNoise.x);
}

float4 RayMarch(float4x4 _ProjectionMatrix, float3 viewDir, int NumSteps, float3 viewPos, float3 screenPos, float2 screenUV, float stepSize, float thickness)
{
    float4 dirProject                                                                       = float4
    (
        abs(unity_CameraProjection._m00 * 0.5), 
        abs(unity_CameraProjection._m11 * 0.5), 
        ((_ProjectionParams.z * _ProjectionParams.y) / (_ProjectionParams.y - _ProjectionParams.z)) * 0.5,
        0.0
    );

    float linearDepth                                                                       =  LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV.xy),_ZBufferParams);

    float3 ray                                                                              = viewPos / viewPos.z;
    float3 rayDir                                                                           = normalize(float3(viewDir.xy - ray * viewDir.z, viewDir.z / linearDepth) * dirProject);
    rayDir.xy                                                                               *= 0.5;

    float3 rayStart                                                                         = float3(screenPos.xy * 0.5 + 0.5,  screenPos.z);

    float3 samplePos                                                                        = rayStart;

    float project                                                                           = ( _ProjectionParams.z * _ProjectionParams.y) / (_ProjectionParams.y - _ProjectionParams.z);
    
    float mask                                                                              = 0.0;

    float oldDepth                                                                          = samplePos.z;
    float oldDelta                                                                          = 0.0;
    float3 oldSamplePos                                                                     = samplePos;

    UNITY_LOOP
    for (int i = 0;  i < NumSteps; i++)
    {
        float depth                                                                         = SampleSceneDepth (samplePos.xy);
        float delta                                                                         = samplePos.z - depth;
        
        if (0.0 < delta)
        {
            if(delta)
            {
                mask                                                                        = 1.0;
                break;
            }
        }
        else
        {
            oldDelta                                                                        = -delta;
            oldSamplePos                                                                    = samplePos;
        }
        oldDepth                                                                            = depth; 
        samplePos                                                                           += rayDir * stepSize;
    }
	
    return                                                                                  float4(samplePos, mask);
}

/// <summary>
/// SpecularStrength
/// </summary>
half SpecularStrength(half3 specular)
{
    #if (SHADER_TARGET < 30)
    // SM2.0: instruction count limitation
    // SM2.0: simplified SpecularStrength
    return specular.r; // Red channel - because most metals are either monocrhome or with redish/yellowish tint
    #else
    return max (max (specular.r, specular.g), specular.b);
    #endif
}

/// <summary>
/// EnergyConservationBetweenDiffuseAndSpecular
/// </summary>
inline half3 EnergyConservationBetweenDiffuseAndSpecular (half3 albedo, half3 specColor, out half oneMinusReflectivity)
{
    oneMinusReflectivity = 1 - SpecularStrength(specColor);
    #if !UNITY_CONSERVE_ENERGY
    return albedo;
    #elif UNITY_CONSERVE_ENERGY_MONOCHROME
    return albedo * oneMinusReflectivity;
    #else
    return albedo * (half3(1,1,1) - specColor);
    #endif
}
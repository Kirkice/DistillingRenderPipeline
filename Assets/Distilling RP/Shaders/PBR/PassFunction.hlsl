/* $Header: /PBR/PassFunction.hlsl         6/26/21 20:55p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : PassFunction.hlsl                                            *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef PassFunction_INCLUDE
#define PassFunction_INCLUDE
#include "Assets/Distilling RP/Shaders/PBR/Input.hlsl" 
#include "Assets/Distilling RP/Shaders/PBR/BRDFs.hlsl"

/// <summary>
/// SampleNormal
/// </summary>
inline half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = 1.0h)
{
    half4 n                                             = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    return                                              UnpackNormalScale(n, scale);
}

/// <summary>
/// InitPBRData
/// </summary>
inline void InitPBRData(float2 uv, inout PBRData data)
{
    data.Albedo                                         = SAMPLE_TEXTURE2D(_Albedo,sampler_Albedo,uv);
    data.Metallic                                       = SAMPLE_TEXTURE2D(_MetallicMap,sampler_MetallicMap,uv).r;
    data.Roughness                                      = SAMPLE_TEXTURE2D(_RoughnessMap,sampler_RoughnessMap,uv).r;
    data.Occlusion                                      = SAMPLE_TEXTURE2D(_OcclusionMap,sampler_OcclusionMap,uv).r;
    data.Normal                                         = SampleNormal(uv, TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), _NormalScale);
    data.Emission                                       = SAMPLE_TEXTURE2D(_EmissionMap,sampler_EmissionMap,uv);                         
}

/// <summary>
/// SetPBRData
/// </summary>
inline void SetPBRData(float3 N, inout PBRData data)
{
    data.Albedo                                         = data.Albedo * _AlbeodColor;
    data.Metallic                                       = data.Metallic * _MetallicStrength;
    data.Roughness                                      = data.Roughness * _RoughnessStrength;
    data.Emission                                       = data.Emission * _EmissionColor * _UseEmission;
    data.Occlusion                                      = data.Occlusion * _OcclusionStrength;
    data.Normal                                         = N * (1 - _UseNormalMap) + _UseNormalMap * data.Normal;
}

/// <summary>
/// SetDirectionData
/// </summary>
inline void SetDirectionData(inout DirectionData data, float3 N, float3 V, float4 PosL,float3 PosW, float4 PosS)
{
    data.N                                              = N;
    data.V                                              = V;
    data.PosL                                           = PosL;
    data.PosW                                           = PosW;
    data.PosS                                           = PosS;
    data.shadowCorrd                                    = TransformWorldToShadowCoord(PosW);
    data.L                                              = GetMainLight(data.shadowCorrd);
}

/// <summary>
/// SetBRDFData
/// </summary>
inline void SetBRDFData(PBRData pbrData, out mBRDFData outBRDFData)
{
    float oneMinusReflectivity                          = OneMinusReflectivityMetallic(pbrData.Metallic);
    float reflectivity                                  = 1.0 - oneMinusReflectivity;

    outBRDFData.diffuse                                 = pbrData.Albedo * oneMinusReflectivity;
    outBRDFData.specular                                = lerp(kDieletricSpec.rgb, pbrData.Albedo, pbrData.Metallic);
    outBRDFData.grazingTerm                             = saturate((1 - pbrData.Roughness) + reflectivity);
    outBRDFData.perceptualRoughness                     = PerceptualSmoothnessToPerceptualRoughness(1 - pbrData.Roughness);
    outBRDFData.roughness                               = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness), HALF_MIN);
    outBRDFData.roughness2                              = outBRDFData.roughness * outBRDFData.roughness;
    outBRDFData.normalizationTerm                       = outBRDFData.roughness * 4.0h + 2.0h;
    outBRDFData.roughness2MinusOne                      = outBRDFData.roughness2 - 1.0h;
}

/// <summary>
/// EnvironmentBRDF
/// </summary>
float3 EnvironmentBRDF(BRDFData brdfData, float roughness2, float3 specular, float grazingTerm, float3 indirectDiffuse, float3 indirectSpecular, float fresnelTerm)
{
    float3 c                                            = indirectDiffuse * brdfData.diffuse;
    float surfaceReduction                              = 1.0 / (roughness2 + 1.0);
    c                                                   += surfaceReduction * indirectSpecular * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm);
    return                                              c;
}

/// <summary>
/// GetDirectLighting
/// </summary>
inline void GetDirectLighting(float3 V, float4 shadowCoord, PBRData data, inout float4 outColor)
{
    Light mainLight                                     = GetMainLight(shadowCoord);
    float3 L                                            = normalize(mainLight.direction);
    float3 N                                            = data.Normal;
    float3 H                                            = SafeNormalize(L + V);
    float3 NdotL                                        = abs(dot(N, L));
    float3 NdotH                                        = abs(dot(N, H));
    float3 VdotH                                        = abs(dot(V, H));
    float3 NdotV                                        = abs(dot(N, V));

    float diffuseRatio                                  = 0.5 * (1.0 - data.Metallic);
    float specularRoatio                                = 1 - diffuseRatio;

    float NDF                                           = DistributionGGX(N, H, data.Roughness);
    float G                                             = GeometrySmith(N, V, L, data.Roughness);
    float3 F                                            = FresnelSchlick(max(dot(H, V), 0.0), _Rf0);

    float3 specularBrdf                                 = SpecularBRDF(NDF, G, F, V, L, N);
    float speccualrPdf                                  = ImportanceSampleGGX_PDF(NDF, NdotH, VdotH);

    half3 kS                                            = F;
    half3 kD                                            = 1.0 - kS;
    kD                                                  *= 1.0 - data.Metallic;
                                
    half3 diffuseBrdf                                   = DiffuseBRDF(data.Albedo);
    half diffusePdf                                     = CosinSamplingPDF(NdotL);
            
    half3 totalBrdf                                     = (diffuseBrdf * kD + specularBrdf) * NdotL;
    half totalPdf                                       = diffuseRatio * diffusePdf + specularRoatio * speccualrPdf;

    half3 energy                                        = saturate(totalBrdf + totalPdf);
    outColor.rgb                                        = totalBrdf; 
}


/// <summary>
/// Unity PRT
/// </summary>
inline void GetInDirectLighting(float3 V, PBRData data, inout float4 outColor)
{
    float3 N                                            = data.Normal;
    float NdotV                                         = abs(dot(N, V));
    float roughness                                     = data.Roughness * data.Roughness;
    float3 ambient_contrib                              = SampleSH(float4(N, 1));

    float3 ambient                                      = 0.03 * data.Albedo;
    float3 iblDiffuse                                   = max(half3(0, 0, 0), ambient.rgb + ambient_contrib);
    float3 Flast                                        = fresnelSchlickRoughness(max(NdotV, 0.0), _Rf0, roughness);
    float kdLast                                        = (1 - Flast) * (1 - data.Metallic);
    float3 iblDiffuseResult                             = iblDiffuse * kdLast * data.Albedo;

    float mip_roughness                                 = data.Roughness * (1.7 - 0.7 * data.Roughness);
    float3 reflectVec                                   = reflect(-V, N);
    half mip                                            = mip_roughness * UNITY_SPECCUBE_LOD_STEPS;
    half4 rgbm                                          = SAMPLE_TEXTURECUBE_LOD(_GlobalCubeMap, sampler_GlobalCubeMap,reflectVec, mip);
    float3 iblSpecular                                  = DecodeHDREnvironment(rgbm, _CubeMap_HDR);
    float2 envBDRF                                      = SAMPLE_TEXTURE2D(_LUT, sampler_LUT,float2(lerp(0, 0.99, NdotV.x), lerp(0, 0.99, roughness))).rg; // LUT采样
    float3 iblSpecularResult                            = iblSpecular * (Flast * envBDRF.r + envBDRF.g);
    float3 IndirectResult                               = iblDiffuseResult + iblSpecularResult;
    outColor.rgb                                        += IndirectResult;
}

/// <summary>
/// Transparent Cut
/// </summary>
inline void TransparentCut(float4 albedo)
{
    if(_RenderingMode > 0)
    {
        if(_RenderingMode < 2)
        {
            clip(albedo.a - _Cutoff);
        }
    }
}

#endif
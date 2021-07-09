/* $Header: /PBR/Common.hlsl         6/27/21 15:11p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : Common.hlsl                                                  *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef COMMON_INCLUDE
#define COMMON_INCLUDE

uniform float2                                                              _Pixel;
uniform float                                                               _Seed;
#define kDieletricSpec                                                      half4(0.04, 0.04, 0.04, 1.0 - 0.04)
#define HALF_MIN                                                            6.103515625e-5 

/// <summary>
/// rand
/// </summary>
float rand()
{
    float result                                                            = frac(sin(_Seed / 100.0f * dot(_Pixel, float2(12.9898f, 78.233f))) * 43758.5453f);
    _Seed                                                                   += 1.0f;
    return                                                                  result;
}

/// <summary>
/// GetTransformMatrix
/// </summary>
float3x3 GetTransformMatrix(float3 normal)
{
    float3 helper                                                           = float3(1, 0, 0);
    if (abs(normal.x) > 0.99f)
        helper                                                              = float3(0, 0, 1);
                                                    
    float3 tangent                                                          = normalize(cross(normal, helper));
    float3 binormal                                                         = normalize(cross(normal, tangent));
    return                                                                  float3x3(tangent, binormal, normal);
}

/// <summary>
/// SampleHemisphere
/// </summary>
float3 SampleHemisphere(float3 normal, float alpha)
{
    float cosTheta                                                          = pow(rand(), 1.0f / (alpha + 1.0f));
    float sinTheta                                                          = sqrt(1.0f - cosTheta * cosTheta);
    float phi                                                               = 2 * PI * rand();
    float3 tangentSpaceDir                                                  = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
                                                
    return                                                                  mul(tangentSpaceDir, GetTransformMatrix(normal));
}

/// <summary>
/// CosinSamplingPDF
/// </summary>
float CosinSamplingPDF(float NdotL)
{
    return                                                                  NdotL / PI;
}

/// <summary>
/// FresnelSchlick
/// </summary>
float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return                                                                  F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

/// <summary>
/// DistributionGGX
/// </summary>
float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a                                                                 = roughness * roughness;
    float a2                                                                = a * a;
    float NdotH                                                             = max(dot(N, H), 0.0);
    float NdotH2                                                            = NdotH * NdotH;
                                                    
    float nom                                                               = a2;
    float denom                                                             = (NdotH2 * (a2 - 1.0) + 1.0);
    denom                                                                   = PI * denom * denom;
                                                    
    return                                                                  nom / denom;
}

/// <summary>
/// GeometrySchlickGGX
/// </summary>
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r                                                                 = (roughness + 1.0);
    float k                                                                 = (r * r) / 8.0;
                                                            
    float nom                                                               = NdotV;
    float denom                                                             = NdotV * (1.0 - k) + k;
                                                            
    return                                                                  nom / denom;
}

/// <summary>
/// GeometrySmith
/// </summary>
float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV                                                             = abs(dot(N, V));
    float NdotL                                                             = abs(dot(N, L));
    float ggx2                                                              = GeometrySchlickGGX(NdotV, roughness);
    float ggx1                                                              = GeometrySchlickGGX(NdotL, roughness);
                                                    
    return                                                                  ggx1 * ggx2;
}

/// <summary>
/// ImportanceSampleGGX
/// </summary>
float3 ImportanceSampleGGX(float2 Xi, float3 N, float3 V, float roughness)
{
    float a                                                                 = roughness * roughness;
                                                    
    float phi                                                               = 2.0 * PI * Xi.x;
    float cosTheta                                                          = sqrt((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
    float sinTheta                                                          = sqrt(1.0 - cosTheta * cosTheta);
                                                    
    float3 H;                                                   
    H.x                                                                     = cos(phi) * sinTheta;
    H.y                                                                     = sin(phi) * sinTheta;
    H.z                                                                     = cosTheta;

    float3 up                                                               = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
    float3 tangent                                                          = normalize(cross(up, N));
    float3 bitangent                                                        = cross(N, tangent);
                                                    
    float3 halfVec                                                          = tangent * H.x + bitangent * H.y + N * H.z;
    halfVec                                                                 = normalize(halfVec);
    
    return                                                                  halfVec;

}

/// <summary>
/// ImportanceSampleGGX_PDF
/// </summary>
float ImportanceSampleGGX_PDF(float NDF, float NdotH, float VdotH)
{
    return                                                                  NDF * NdotH / (4 * VdotH);
}

/// <summary>
/// Calculatefresnel
/// </summary>
float Calculatefresnel(const float3 I, const float3 N, const float3 ior)
{
    float kr;
    float cosi                                                              = clamp(-1, 1, dot(I, N));
    float etai                                                              = 1, etat = ior;
    if (cosi > 0)
    {
        float temp                                                          = etai;
        etai                                                                = etat;
        etat                                                                = temp;
    }

    float sint                                                              = etai / etat * sqrt(max(0.f, 1 - cosi * cosi));

    if (sint >= 1)
    {
        kr                                                                  = 1;
    }
    else
    {
        float cost                                                          = sqrt(max(0.f, 1 - sint * sint));
        cosi                                                                = abs(cosi);
        float Rs                                                            = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
        float Rp                                                            = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
        kr                                                                  = (Rs * Rs + Rp * Rp) / 2;
    }
    return kr;
}

/// <summary>
/// OneMinusReflectivityMetallic
/// </summary>
float OneMinusReflectivityMetallic(float metallic)
{
    float oneMinusDielectricSpec                                            = kDieletricSpec.a;
    return                                                                  oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

/// <summary>
/// GlossyEnvironmentReflection
/// </summary>
float3 GlossyEnvironmentReflection(float3 reflectVector, float mip, float occlusion)
{
    float4 encodedIrradiance                                                = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip);
    float3 irradiance                                                       = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    return                                                                  irradiance * occlusion;
}

/// <summary>
/// EnvBRDF
/// </summary>
half3 EnvBRDF(mBRDFData brdfData, half3 indirectDiffuse, half3 indirectSpecular, half fresnelTerm)
{
    half3 c                                                                 = indirectDiffuse * brdfData.diffuse;
    float surfaceReduction                                                  = 1.0 / (brdfData.roughness2 + 1.0);
    c                                                                       += surfaceReduction * indirectSpecular * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm);
    return c;
}

/// <summary>
/// InDirectionLight
/// </summary>
inline void InDirectionLight(mBRDFData brdfData, half occlusion, DirectionData directionData, inout float3 color)
{
    float3 reflectVector                                                    = reflect(- directionData.V, directionData.PosW);
    float fresnelTerm                                                       = Pow4(1.0 - saturate(dot(directionData.PosW, directionData.V)));
    float3 indirectDiffuse                                                  = occlusion;
    float3 indirectSpecular                                                 = GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, occlusion);
    color                                                                   = EnvBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
}

/// <summary>
/// DirectBRDFSpecular
/// </summary>
float DirectBRDFSpecular(mBRDFData brdfData, float3 normalWS, float3 lightDirectionWS, float3 viewDirectionWS)
{
    float3 halfDir                                                          = SafeNormalize(float3(lightDirectionWS) + float3(viewDirectionWS));
    float NoH                                                               = saturate(dot(normalWS, halfDir));
    float LoH                                                               = saturate(dot(lightDirectionWS, halfDir));
    float d                                                                 = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    float LoH2                                                              = LoH * LoH;
    float specularTerm                                                      = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);
    return specularTerm;
}

/// <summary>
/// DirectionLight
/// </summary>
inline void DirectionLight(mBRDFData brdfData, DirectionData dirData, inout float3 color)
{
    float NdotL                                                              = saturate(dot(dirData.N, dirData.L.direction));
    float3 radiance                                                          = dirData.L.color * (dirData.L.distanceAttenuation * NdotL);
    color                                                                    += DirectBRDFSpecular(brdfData, dirData.N, dirData.L.direction, dirData.V) * radiance;
}
#endif
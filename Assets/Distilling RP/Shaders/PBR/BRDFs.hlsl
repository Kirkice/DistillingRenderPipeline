/* $Header: /PBR/BRDFs.hlsl         6/27/21 15:11p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : BRDFs.hlsl                                                   *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef BRDF_INCLUDE
#define BRDF_INCLUDE
#include "Common.hlsl"

/// <summary>
/// DiffuseBRDF
/// </summary>
float3 DiffuseBRDF(float3 albedo)
{
    return                                                                      albedo / PI;
}

/// <summary>
/// fresnelSchlickRoughness
/// </summary>
float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    return                                                                      F0 + (max(float3(1 ,1, 1) * (1 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

#endif
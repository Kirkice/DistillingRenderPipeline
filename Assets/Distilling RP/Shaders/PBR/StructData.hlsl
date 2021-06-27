/* $Header: /PBR/InputData.hlsl         6/26/21 20:55p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : InputData.hlsl                                               *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef InputData_INCLUDE
#define InputData_INCLUDE

/// <summary>
/// PBR DATA
/// </summary>
struct PBR_Data
{
    float4                                              Albedo;
    float                                               Metallic;
    float                                               Roughness;
    float3                                              Normal;
    float                                               Occlusion;
    float4                                              Emission;
};

#endif
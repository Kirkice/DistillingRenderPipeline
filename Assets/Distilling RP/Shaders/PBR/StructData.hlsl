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
struct PBRData
{
    float4                                              Albedo;
    float                                               Metallic;
    float                                               Roughness;
    float3                                              Normal;
    float                                               Occlusion;
    float4                                              Emission;
};

/// <summary>
/// DirectionData
/// </summary>
struct DirectionData
{
    float4                                              PosL;
    float3                                              PosW;
    float4                                              PosS;
    float3                                              V;
    float3                                              N;
    Light                                               L;
    float4                                              shadowCorrd;
};

/// <summary>
/// mBRDFData
/// </summary>
struct mBRDFData
{
    float3                                              diffuse;
    float3                                              specular;
    float                                               perceptualRoughness;
    float                                               roughness;
    float                                               roughness2;
    float                                               grazingTerm;
    float                                               normalizationTerm;     
    float                                               roughness2MinusOne;
};

#endif
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
    float3                                              B;
    float3                                              T;
    Light                                               L;
    float4                                              shadowCorrd;
    float3                                              vertexLighting;
    float3                                              bakedGI; 
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

/// <summary>
/// PunctualLightData
/// </summary>
struct PunctualLightData
{
    float3 posWS;
    float radius2;
    float4 color;
    float4 attenuation;
    float3 spotDirection;
    int flags;
    float4 occlusionProbeInfo;
};
#endif
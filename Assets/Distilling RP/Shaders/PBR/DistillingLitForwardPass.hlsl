/* $Header: /PBR/DistillingLitForwardPass.hlsl         6/26/21 20:55p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : DistillingLitForwardPass.hlsl                                *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef DistillingLitForwardPass_INCLUDE
#define DistillingLitForwardPass_INCLUDE
#include "Assets/Distilling RP/Shaders/PBR/PassFunction.hlsl"

struct Vertex_Input
{
    float4 PosL                                             : POSITION;
    float2 TexC                                             : TEXCOORD0;
    float3 NormalL                                          : NORMAL;
};
            
struct Vertex_Output
{
    float4 PosH                                             : SV_POSITION;
    float2 TexC                                             : TEXCOORD0;
    float3 PosW                                             : TEXCOORD1;
    float3 V                                                : TEXCOORD2;
    float3 NormalW                                          : NORMAL;
};
            
            
Vertex_Output Lit_VS(Vertex_Input vin)
{
    Vertex_Output vout;
    vout.PosH                                               = TransformObjectToHClip(vin.PosL);
    vout.NormalW                                            = TransformObjectToWorldNormal(vin.NormalL);
    vout.PosW                                               = TransformObjectToWorld(vin.PosL);
    vout.V                                                  = normalize(_WorldSpaceCameraPos.xyz - vout.PosW.xyz);
    vout.TexC                                               = vin.TexC;
    return vout;
}
            
half4 Lit_PS(Vertex_Output pin) : SV_Target
{
    float4 shadowCoord                                      = TransformWorldToShadowCoord(pin.PosW);
    float4 outColor                                         = float4(0,0,0,1);
    PBR_Data data;
    InitBRDFData(pin.TexC, data);
    SetBRDFData(pin.NormalW, data);
    GetDirectLighting(pin.V, shadowCoord,data,outColor);
    GetInDirectLighting(pin.V, data, outColor);
    return                                                  outColor;
}

#endif
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
    float4 PosL                                                         : POSITION;
    float2 TexC                                                         : TEXCOORD0;
    float3 NormalL                                                      : NORMAL;
};
            
struct Vertex_Output
{
    float4 PosH                                                         : SV_POSITION;
    float3 NormalW                                                      : NORMAL;
    float2 TexC                                                         : TEXCOORD0;
    float3 PosW                                                         : TEXCOORD1;
    float3 V                                                            : TEXCOORD2;
    float4 PosL                                                         : TEXCOORD3;
    float4 PosS                                                         : TEXCOORD4;
};
            
            
Vertex_Output Lit_VS(Vertex_Input vin)
{
    Vertex_Output vout;
    vout.PosH                                                           = TransformObjectToHClip(vin.PosL);
    vout.NormalW                                                        = TransformObjectToWorldNormal(vin.NormalL);
    vout.PosW                                                           = TransformObjectToWorld(vin.PosL);
    vout.V                                                              = normalize(_WorldSpaceCameraPos.xyz - vout.PosW.xyz);
    vout.PosS                                                           = ComputeScreenPos(vout.PosH);
    vout.PosL                                                           = vin.PosL;
    vout.TexC                                                           = vin.TexC;
    return vout;
}

//  L = L(e) + L(o) * f * V * w cos0 ;
half4 Lit_PS(Vertex_Output pin) : SV_Target
{
    float4 shadowCoord                                                  = TransformWorldToShadowCoord(pin.PosW);
    float4 outColor                                                     = float4(0,0,0,1);
    //声明一些用到的结构体
    PBRData pbrData;
    DirectionData dirData;
    mBRDFData brdfData;
    //初始化PBR数据
    InitPBRData(pin.TexC, pbrData);
    //设置PBR数据
    SetPBRData(pin.NormalW, pbrData);
    //设置向量数据
    SetDirectionData(dirData, pin.NormalW, pin.V, pin.PosL, pin.PosW, pin.PosS);
    SetBRDFData(pbrData, brdfData);
    //剔除透明
    TransparentCut(pbrData.Albedo);
    //获取间接光照
    InDirectionLight(brdfData,pbrData.Occlusion,dirData,outColor.rgb);
    //获取直接光照
    DirectionLight(brdfData,dirData,outColor.rgb);
    return                                                              outColor;
}

#endif
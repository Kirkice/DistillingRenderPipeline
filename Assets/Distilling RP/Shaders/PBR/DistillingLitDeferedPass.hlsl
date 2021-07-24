/* $Header: /PBR/DistillingLitDeferedPass.hlsl         7/13/21 20:55p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : DistillingLitDeferedPass.hlsl                                *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef DistillingLitDeferedPass_INCLUDE
#define DistillingLitDeferedPass_INCLUDE

#include "Assets/Distilling RP/Shaders/PBR/PassFunction.hlsl"

struct Vertex_Input
{
    float4 PosL                                                         : POSITION;
    float3 NormalL                                                      : NORMAL;
    float4 TangentL                                                     : TANGENT;
    float2 TexC                                                         : TEXCOORD0;
    float2 lightmapUV                                                   : TEXCOORD1;
    float4 vColor                                                       : COLOR;
};
            
struct Vertex_Output
{
    float4 PosH                                                         : SV_POSITION;
    float3 TangentW                                                     : TEXCOORD0;
    float3 BitangentW                                                   : TEXCOORD1;
    float4 TexC                                                         : TEXCOORD2;
    float3 PosW                                                         : TEXCOORD3;
    float3 V                                                            : TEXCOORD4;
    float4 PosL                                                         : TEXCOORD5;
    float4 PosS                                                         : TEXCOORD6;
    float4 fogFactorAndVertexLight                                      : TEXCOORD7;
    float4 vColor                                                       : TEXCOORD8;
    float3 NormalW                                                      : TEXCOORD9;
};
            
struct Geom_Output
{
    float4 PosH                                                         : SV_POSITION;
    float3 TangentW                                                     : TEXCOORD0;
    float3 BitangentW                                                   : TEXCOORD1;
    float4 TexC                                                         : TEXCOORD2;
    float3 PosW                                                         : TEXCOORD3;
    float3 V                                                            : TEXCOORD4;
    float4 PosL                                                         : TEXCOORD5;
    float4 PosS                                                         : TEXCOORD6;
    float4 fogFactorAndVertexLight                                      : TEXCOORD7;
    float4 vColor                                                       : TEXCOORD8;
    float3  dist                                                        : TEXCOORD9;
    float3 NormalW                                                      : TEXCOORD10;
};

struct Pixel_Output
{
    half4 GBuffer0 : SV_Target0;
    half4 GBuffer1 : SV_Target1;
    half4 GBuffer2 : SV_Target2;
    half4 GBuffer3 : SV_Target3; // Camera color attachment
    #if OUTPUT_SHADOWMASK
    half4 GBuffer4 : SV_Target4;
    #endif
};

Vertex_Output Lit_VSGBuffer(Vertex_Input vin)
{
    Vertex_Output vout;
    vout.PosH                                                           = TransformObjectToHClip(vin.PosL);
    VertexNormalInputs normalInput                                      = GetVertexNormalInputs(vin.NormalL, vin.TangentL);
    vout.NormalW                                                        = normalInput.normalWS;
    vout.PosW                                                           = TransformObjectToWorld(vin.PosL);
    vout.V                                                              = normalize(_WorldSpaceCameraPos.xyz - vout.PosW.xyz);
    vout.PosS                                                           = ComputeScreenPos(vout.PosH);
    vout.TangentW                                                       = TransformObjectToWorldDir(vin.TangentL);
    vout.BitangentW                                                     = cross(vout.NormalW, vout.TangentW) * vin.TangentL.w;
    float3 vertexLight                                                  = VertexLighting(vout.PosW, vout.NormalW);
    float fogFactor                                                     = ComputeFogFactor(vout.PosH.z);
    vout.fogFactorAndVertexLight                                        = float4(fogFactor, vertexLight);
    vout.PosL                                                           = vin.PosL;
    vout.TexC.xy                                                        = vin.TexC;
    vout.TexC.z                                                         = dot(normalize(UNITY_MATRIX_IT_MV[0].xyz), normalize(vin.NormalL));
    vout.TexC.w                                                         = dot(normalize(UNITY_MATRIX_IT_MV[1].xyz), normalize(vin.NormalL));
    vout.vColor                                                         = vin.vColor;
    vout.TexC.zw                                                        = vout.TexC.zw * 0.5 + 0.5;
    return vout;
}

[maxvertexcount(3)]
void Lit_GSGBuffer(triangle Vertex_Output p[3], inout TriangleStream<Geom_Output> triStream)
{
    float2 p0                                                           = _ScreenParams.xy * p[0].PosH.xy / p[0].PosH.w;
    float2 p1                                                           = _ScreenParams.xy * p[1].PosH.xy / p[1].PosH.w;
    float2 p2                                                           = _ScreenParams.xy * p[2].PosH.xy / p[2].PosH.w;

    float2 v0                                                           = p2 - p1;
    float2 v1                                                           = p2 - p0;
    float2 v2                                                           = p1 - p0;

    //area of the triangle
    float area                                                          = abs(v1.x*v2.y - v1.y * v2.x);

    //values based on distance to the edges
    float dist0                                                         = area / length(v0);
    float dist1                                                         = area / length(v1);
    float dist2                                                         = area / length(v2);

    Geom_Output pIn;

    //add the first point
    pIn.PosH                                                            = p[0].PosH;
    pIn.NormalW                                                         = p[0].NormalW;
    pIn.PosW                                                            = p[0].PosW;
    pIn.V                                                               = p[0].V;
    pIn.PosS                                                            = p[0].PosS;
    pIn.TangentW                                                        = p[0].TangentW;
    pIn.BitangentW                                                      = p[0].BitangentW;
    pIn.fogFactorAndVertexLight                                         = p[0].fogFactorAndVertexLight;
    pIn.PosL                                                            = p[0].PosL;
    pIn.TexC                                                            = p[0].TexC;
    pIn.vColor                                                          = p[0].vColor;
    pIn.dist                                                            = float3(dist0,0,0);
    triStream.Append(pIn);

    //add the second point
    pIn.PosH                                                            = p[1].PosH;
    pIn.NormalW                                                         = p[1].NormalW;
    pIn.PosW                                                            = p[1].PosW;
    pIn.V                                                               = p[1].V;
    pIn.PosS                                                            = p[1].PosS;
    pIn.TangentW                                                        = p[1].TangentW;
    pIn.BitangentW                                                      = p[1].BitangentW;
    pIn.fogFactorAndVertexLight                                         = p[1].fogFactorAndVertexLight;
    pIn.PosL                                                            = p[1].PosL;
    pIn.TexC                                                            = p[1].TexC;
    pIn.vColor                                                          = p[1].vColor;
    pIn.dist                                                            = float3(0,dist1,0);
    triStream.Append(pIn);

    //add the third point
    pIn.PosH                                                            = p[2].PosH;
    pIn.NormalW                                                         = p[2].NormalW;
    pIn.PosW                                                            = p[2].PosW;
    pIn.V                                                               = p[2].V;
    pIn.PosS                                                            = p[2].PosS;
    pIn.TangentW                                                        = p[2].TangentW;
    pIn.BitangentW                                                      = p[2].BitangentW;
    pIn.fogFactorAndVertexLight                                         = p[2].fogFactorAndVertexLight;
    pIn.PosL                                                            = p[2].PosL;
    pIn.TexC                                                            = p[2].TexC;
    pIn.vColor                                                          = p[2].vColor;
    pIn.dist                                                            = float3(0,0,dist1);
    triStream.Append(pIn);
}

Pixel_Output Lit_PSGBuffer(Geom_Output pin) : SV_Target
{
    float4 shadowCoord                                                  = TransformWorldToShadowCoord(pin.PosW);
    float4 outColor                                                     = float4(0,0,0,1);
    //声明一些用到的结构体
    PBRData pbrData;
    DirectionData dirData;
    mBRDFData brdfData;
    //设置向量数据 
    SetDirectionData(dirData, pin.NormalW, pin.V, pin.PosL, pin.PosW, pin.PosS, pin.TangentW, pin.BitangentW, pin.fogFactorAndVertexLight);
    dirData.bakedGI                                                     = float3(0,0,0);
    //POM视差
    ParallaxOcclusionMapping(dirData,pin.V, pin.TexC.xy);
    //初始化PBR数据
    InitPBRData(pin.TexC, pbrData); 
    //设置PBR数据
    SetPBRData(pin.NormalW, pbrData);
    //设置法线贴图
    TBNMatrixDot(dirData, pbrData);
    //设置BRDF数据
    SetBRDFData(pbrData, brdfData);
    //剔除透明
    TransparentCut(pbrData.Albedo);
    //获取间接光照
    InDirectionLight(brdfData, pbrData.Occlusion, dirData, outColor.rgb);
    //自发光
    EmissionLight(pbrData, outColor.rgb);
    //MatCap
    MatCapColor(pin.TexC.zw, outColor.rgb);
    //多光源
    MultipleLight(brdfData, dirData, outColor.rgb);
    half3 InDirectionColor                                  = outColor.rgb;

    Pixel_Output                                            output;
    output.GBuffer0                                         = half4(brdfData.diffuse.rgb, 1);
    output.GBuffer1                                         = half4(brdfData.specular, 1);
    output.GBuffer2                                         = half4(dirData.N * 0.5 + 0.5, pbrData.Roughness);
    output.GBuffer3                                         = half4(InDirectionColor, 1);
    return                                                  output;
}

#endif
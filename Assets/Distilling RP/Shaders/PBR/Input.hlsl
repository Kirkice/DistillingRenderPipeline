/* $Header: /PBR/Input.hlsl         6/26/21 20:55p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : Input.hlsl                                                   *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef Input_INCLUDE
#define Input_INCLUDE

#include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
#include "Assets/Distilling RP/Shaders/PBR/StructData.hlsl" 

/// <summary>
/// CONST PARAMES
/// </summary>
static const float _TraceCount = 8;
static const float EPSILON = 1e-8;

#define PREFERRED_CBUFFER_SIZE (64 * 1024)
#define SIZEOF_VEC4_TILEDATA 1 
#define SIZEOF_VEC4_PUNCTUALLIGHTDATA 5 
#define MAX_DEPTHRANGE_PER_CBUFFER_BATCH (PREFERRED_CBUFFER_SIZE / 4) 
#define MAX_TILES_PER_CBUFFER_PATCH (PREFERRED_CBUFFER_SIZE / (16 * SIZEOF_VEC4_TILEDATA))
#define MAX_PUNCTUALLIGHT_PER_CBUFFER_BATCH (PREFERRED_CBUFFER_SIZE / (16 * SIZEOF_VEC4_PUNCTUALLIGHTDATA))
#define MAX_REL_LIGHT_INDICES_PER_CBUFFER_BATCH (PREFERRED_CBUFFER_SIZE / 4) 
#if defined(SHADER_API_SWITCH)
#define USE_CBUFFER_FOR_DEPTHRANGE 0
#define USE_CBUFFER_FOR_TILELIST 0
#define USE_CBUFFER_FOR_LIGHTDATA 1
#define USE_CBUFFER_FOR_LIGHTLIST 0
#elif defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)
#define USE_CBUFFER_FOR_DEPTHRANGE 1
#define USE_CBUFFER_FOR_TILELIST 1
#define USE_CBUFFER_FOR_LIGHTDATA 1
#define USE_CBUFFER_FOR_LIGHTLIST 1
#else
#define USE_CBUFFER_FOR_DEPTHRANGE 0
#define USE_CBUFFER_FOR_TILELIST 0
#define USE_CBUFFER_FOR_LIGHTDATA 1
#define USE_CBUFFER_FOR_LIGHTLIST 0
#endif



CBUFFER_START(UnityPerMaterial)

/// <summary>
/// TEXTURES
/// </summary>
uniform TEXTURE2D(_Albedo);                                 SAMPLER(sampler_Albedo);
uniform TEXTURE2D(_MetallicMap);                            SAMPLER(sampler_MetallicMap);
uniform TEXTURE2D(_RoughnessMap);                           SAMPLER(sampler_RoughnessMap);
uniform TEXTURE2D(_NormalMap);                              SAMPLER(sampler_NormalMap);
uniform TEXTURE2D(_HeightMap);                              SAMPLER(sampler_HeightMap);
uniform TEXTURE2D(_OcclusionMap);                           SAMPLER(sampler_OcclusionMap);
uniform TEXTURE2D(_EmissionMap);                            SAMPLER(sampler_EmissionMap);
uniform TEXTURE2D(_MatCapMap);                              SAMPLER(sampler_MatCapMap);
uniform TEXTURE2D(_CameraShadowMaskTexture);                SAMPLER(sampler_CameraShadowMaskTexture);
uniform TEXTURE2D(_LUT);                                    SAMPLER(sampler_LUT);
uniform TEXTURECUBE(_GlobalCubeMap);                        SAMPLER(sampler_GlobalCubeMap);

/// <summary>
/// INT
/// </summary>
uniform int                                                 _MeshIndex;
uniform int                                                 _CullMode;
uniform int                                                 _Rf0Mode;
uniform int                                                 _RenderingMode;
/// <summary>
/// FLOAT --> BOOL
/// </summary>
uniform float                                               _UseDRPLight;
uniform float                                               _UseMultipleLight;
uniform float                                               _UseRealTimeAreaLight;

uniform float                                               _UseDRPShadow;
uniform float                                               _UseVoxel;
uniform float                                               _UsePlanerShadow;
uniform float                                               _UsePCFShadow;
uniform float                                               _UsePCSSShadow;
uniform float                                               _UseVSMShadow;
uniform float                                               _UseSDFShadow;
uniform float                                               _UseMatCap;

uniform float                                               _UseGI;
uniform float                                               _UsePRT;
uniform float                                               _UseRSM;
uniform float                                               _UseSSDO;
uniform float                                               _UseSSR;
uniform float                                               _UseLPV;
uniform float                                               _UseVoxelGI;
uniform float                                               _UseDDGI;
uniform float                                               _UsePathTracing;
uniform float                                               _GIStrength;

uniform float                                               _UseMetallic;
uniform float                                               _UseRoughness;
uniform float                                               _UseEmission;
uniform float                                               _Cutoff;
uniform float                                               _MetallicStrength;
uniform float                                               _RoughnessStrength;
uniform float                                               _UseNormalMap;
uniform float                                               _NormalScale;
uniform float                                               _OcclusionStrength;
uniform float                                               _ReceiveShadows;
uniform float                                               _UseHeightMap;

uniform float                                               _DebugPosW;
uniform float                                               _DebugPosL;
uniform float                                               _DebugTangent;
uniform float                                               _DebugNormal;
uniform float                                               _DebugUVX;
uniform float                                               _DebugUVY;
uniform float                                               _DebugVColorR;
uniform float                                               _DebugVColorG;
uniform float                                               _DebugVColorB;
uniform float                                               _DebugWireframe;
/// <summary>
/// FLOAT
/// </summary>
uniform float                                               _IOR;
uniform float                                               _ColorAdd;
uniform float                                               _AbsorbIntensity;
uniform float                                               _ColorMultiply;
uniform float                                               _Height;
uniform float                                               _HeightAmount;

/// <summary>
/// FLOAT3
/// </summary>
uniform float3                                              _Rf0;
/// <summary>
/// FLOAT4
/// </summary>
uniform float4                                              _AlbeodColor;
uniform float4                                              _EmissionColor;
uniform float4                                              _MatCapColor;
uniform float4                                              _CubeMap_HDR;

CBUFFER_END

#endif
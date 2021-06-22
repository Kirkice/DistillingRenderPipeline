/* $Header: /SSR/SSR_Input.hlsl         6/09/21 23:27p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
 *                                                                                             *
 *                 Project Name : DistillingRenderPipeline                                     *
 *                                                                                             *
 *                    File Name : SSR_Input.hlsl                                               *
 *                                                                                             *
 *                   Programmer : Kirk Zhu                                                     *
 *                                                                                             *
 *---------------------------------------------------------------------------------------------*/
#include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
#include "Assets/Distilling RP/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Assets/Distilling RP/ShaderLibrary/Lighting.hlsl"

CBUFFER_START(UnityPerMaterial)

TEXTURE2D(_MainTex);                                    SAMPLER(sampler_MainTex);
TEXTURE2D(_ReflectionBuffer);                           SAMPLER(sampler_ReflectionBuffer);
TEXTURE2D(_PreviousBuffer);                             SAMPLER(sampler_PreviousBuffer);
TEXTURE2D(_RayCast);                                    SAMPLER(sampler_RayCast);
TEXTURE2D(_RayCastMask);                                SAMPLER(sampler_RayCastMask);

TEXTURE2D(_Noise);                                      SAMPLER(sampler_Noise);
TEXTURE2D(_GBuffer0);                                   SAMPLER(sampler_GBuffer0);
TEXTURE2D(_GBuffer1);                                   SAMPLER(sampler_GBuffer1);
TEXTURE2D(_GBuffer2);                                   SAMPLER(sampler_GBuffer2);
TEXTURE2D(_GBuffer3);                                   SAMPLER(sampler_GBuffer3);
TEXTURE2D(_CameraReflectionsTexture);                   SAMPLER(sampler_CameraReflectionsTexture);

TEXTURE2D(_CameraNormalWSTexture);                      SAMPLER(sampler_CameraNormalWSTexture);

TEXTURE2D(_CameraDepthBuffer);                          SAMPLER(sampler_CameraDepthBuffer);
TEXTURE2D_HALF(_CameraMotionVectorsTexture);            SAMPLER(sampler_CameraMotionVectorsTexture);


uniform     float4                                      _RayCastSize;
uniform     float4                                      _ResolveSize;
uniform     float4                                      _NoiseSize;
uniform     float4                                      _Project;
uniform     float4                                      _GaussianDir;
uniform     float4                                      _JitterSizeAndOffset;


uniform     float                                       _EdgeFactor;
uniform     float                                       _SmoothnessRange;
uniform     float                                       _BRDFBias;
uniform     float		                                     _Thickness;
uniform     float		                                     _TScale;
uniform     float		                                     _TMinResponse;
uniform     float		                                     _TMaxResponse;
uniform     float		                                     _TResponse;
uniform     float		                                     _StepSize;
uniform     float		                                     _Accumulation;


uniform     int			                                      _NumSteps;
uniform     int			                                      _RayReuse;
uniform     int			                                      _MipMapCount;
uniform     int			                                      _ReflectionVelocity;


uniform     int			                                       _UseTemporal;
uniform     int			                                       _UseFresnel;
uniform     int			                                       _DebugPass;
uniform     int			                                       _UseNormalization;
uniform     int			                                       _Fireflies;
uniform     int			                                       _MaxMipMap;


uniform     float4x4	                                    _ProjectionMatrix;
uniform     float4x4	                                    _ViewProjectionMatrix;
uniform     float4x4	                                    _InverseProjectionMatrix;
uniform     float4x4	                                    _InverseViewProjectionMatrix;
uniform     float4x4	                                    _WorldToCameraMatrix;
uniform     float4x4	                                    _CameraToWorldMatrix;
uniform     float4x4	                                    _PrevInverseViewProjectionMatrix;
uniform     float4x4	                                    _PrevViewProjectionMatrix;

CBUFFER_END
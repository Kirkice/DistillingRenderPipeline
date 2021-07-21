/* $Header: /SSR/ScreenSpaceRayTracingPass.hlsl         6/09/21 23:35p KirkZhu $ */
/*--------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : ScreenSpaceRayTracingPass.hlsl                               *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#include "SSR_Lib.hlsl"

struct VertexInput
{
    float4 PosL                                                                 : POSITION;
    float2 TexC                                                                 : TEXCOORD0;
};

struct VertexOutput
{
    float4 PosH                                                                 : SV_POSITION;
    float2 TexC                                                                 : TEXCOORD0;
};


VertexOutput VS(VertexInput vin)
{
    VertexOutput vout;
    vout.PosH                                                                   = TransformObjectToHClip(vin.PosL.xyz);
    vout.TexC                                                                   = vin.TexC;
    return vout;
}

/// <summary>
/// Temporal
/// </summary>
void PS_Temporal(VertexOutput pin, out half4 reflection : SV_Target)
{
    float depth;
    float2 uv, velocity, averageDepthMin, averageDepthMax, prevUV;
    float3 PosS, hitPacked;
    float4 current, previous, currentTopLeft, currentTopCenter, currentTopRight, currentMiddleLeft, currentMiddleCenter,
    currentMiddleRight, currentBottomLeft, currentBottomCenter, currentBottomRight, currentMin = float4(0,0,0,1), currentMax = float4(0,0,0,1);

    GetCommonVector_Temporal(pin.TexC, uv, depth, PosS, hitPacked, velocity);
    GetAverageDepthAndPreUV(hitPacked, velocity, uv, averageDepthMin, averageDepthMax, prevUV);
    GetSampleColorCurrent(uv, prevUV, current, previous, currentTopLeft, currentTopCenter,currentTopRight,
        currentMiddleLeft, currentMiddleCenter, currentMiddleRight, currentBottomLeft,currentBottomCenter, currentBottomRight);
    GetCurrentMaxMin(currentMin, currentMax);
    previous                                                                    = clamp(previous, currentMin, currentMax);

	float4 currentColor  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
	
    reflection                                                                  = currentColor;//lerp(current, previous, saturate(_TResponse *  (1 - length(velocity) * 8)) );
}

/// <summary>
/// Resolve
/// </summary>
float4 PS_Resolve (VertexOutput pin) : SV_Target
{
    float roughness, depth, NoV, coneTangent, maxMipLevel;
    int2 pos;
    float2 uv, random, blueNoise;
    float3 NormalV, PosS, PosW, PosV, V;
    float4 NormalW, Specular;

    GetCommonVector_Resolve(pin.TexC, roughness, depth, NoV, coneTangent, maxMipLevel, uv, pos, random, blueNoise, NormalV, PosS,
        PosW, PosV, V,NormalW, Specular);

    int NumResolve                                                              = 1;
    if(_RayReuse == 1)
        NumResolve                                                              = 4;

    float4 result                                                               = 0.0;
    float weightSum                                                             = 0.0;

	float2x2 offsetRotationMatrix												= GetOffsetRotationMatrix(blueNoise);

	float2 offsetUV																= offset[0] * (1.0 / _ResolveSize.xy);
	offsetUV																	=  mul(offsetRotationMatrix, offsetUV);
	float2 neighborUv															= uv + offsetUV;
	float4 color															= SAMPLE_TEXTURE2D(_RayCast, sampler_RayCast,pin.TexC);
 //   for(int i = 0; i < NumResolve; i++)
 //   {
	// 		float2 offsetUV														= offset[i] * (1.0 / _ResolveSize.xy);
	// 		offsetUV															=  mul(offsetRotationMatrix, offsetUV);
	// 		float2 neighborUv													= uv + offsetUV;
 //     
 //            float4 hitPacked													= SAMPLE_TEXTURE2D_LOD(_RayCast, sampler_RayCast,neighborUv,0);
 //            float2 hitUv														= hitPacked.xy;
 //            float hitZ															= hitPacked.z;
 //            float hitPDF														= hitPacked.w;
	// 		float hitMask														= SAMPLE_TEXTURE2D_LOD(_RayCastMask, sampler_RayCastMask,neighborUv,0).r;
 //   
	// 		float3 hitViewPos													= GetViewPos(GetScreenPos(hitUv, hitZ));
	// 		float weight														= 1.0;
	// 		if(_UseNormalization == 1)
	// 			 weight															=  BRDF_Unity_Weight(normalize(-V) /*V*/, normalize(hitViewPos - PosV) /*L*/, NormalV /*N*/, roughness) / max(1e-5, hitPDF);
 //   
	// 		float intersectionCircleRadius										= coneTangent * length(hitUv - uv);
	// 		float mip															= clamp(log2(intersectionCircleRadius * max(_ResolveSize.x, _ResolveSize.y)), 0.0, maxMipLevel);
 //   
	// 		float4 sampleColor													= float4(0.0,0.0,0.0,1.0);
	// 		sampleColor.rgb														= SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex,hitUv,mip).rgb;
	// 		sampleColor.a														= RayAttenBorder (hitUv, _EdgeFactor) * hitMask;
 //   
	// 		if(_Fireflies == 1)
	// 			sampleColor.rgb													/= 1 + Luminance(sampleColor.rgb);
 //   
 //            result																+= sampleColor * weight;
 //            weightSum															+= weight;
 //   }
	// result																		/= weightSum;
 //
	// if(_Fireflies == 1)
	// 	result.rgb																/= 1 - Luminance(result.rgb);
 //
	// return																		max(1e-5, result);
	return color;
}

/// <summary>
/// PS_RayCast
/// </summary>
float4 PS_RayCast (VertexOutput pin) : SV_Target
{	
	float2 uv																	= pin.TexC;
	int2 pos																	= uv;

	float4 worldNormal															= GetNormal (uv);
	float3 viewNormal															= GetViewNormal (worldNormal);
	float4 specular																= GetSpecular (uv);
	float roughness																= GetRoughness (worldNormal.a);

	float depth																	= SampleSceneDepth(uv);
	float3 screenPos															= GetScreenPos(uv, depth);

	float3 worldPos																= GetWorlPos(screenPos);
	float3 viewDir																= GetViewDir(worldPos);

	float3 viewPos																= GetViewPos(screenPos);

	float2 jitter																= SAMPLE_TEXTURE2D_LOD(_Noise, sampler_Noise,float4((uv + _JitterSizeAndOffset.zw) * _RayCastSize.xy / _NoiseSize.xy, 0, -255),0); // Blue noise generated by https://github.com/bartwronski/BlueNoiseGenerator/;

	float2 Xi = jitter;

	Xi.y																		= lerp(Xi.y, 0.0, _BRDFBias);

	float4 H																	= TangentToWorld(worldNormal, ImportanceSampleGGX(Xi, roughness));
	float3 dir																	= reflect(viewDir, H.xyz);
	dir																			= normalize(mul((float3x3)_WorldToCameraMatrix, dir));

	jitter																		+= 0.5f;

	float stepSize																= (1.0 / (float)_NumSteps);
	stepSize																	= stepSize * (jitter.x + jitter.y) + stepSize;

	float2 rayTraceHit															= 0.0;
	float rayTraceZ																= 0.0;
	float rayPDF																= 0.0;
	float rayMask																= 0.0;
	float4 rayTrace																= RayMarch(_ProjectionMatrix, dir, _NumSteps, viewPos, screenPos, uv, stepSize, 1.0);

	rayTraceHit																	= rayTrace.xy;
	rayTraceZ																	= rayTrace.z;
	rayPDF																		= H.w;
	rayMask																		= rayTrace.w;

	// return																		float4(float3(rayTraceHit, rayTraceZ), rayPDF);
	return																		rayTrace;
}

/// <summary>
/// PS
/// </summary>
float4 PS( VertexOutput pin ) : SV_Target
{	 
	float2 uv																	= pin.TexC;
	float4 cubemap																= GetCubeMap (uv);
	float4 sceneColor															= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);
	sceneColor.rgb																= max(1e-5, sceneColor.rgb - cubemap.rgb);
	return																		sceneColor;
}

/// <summary>
/// PS_Recursive
/// </summary>
float4 PS_Recursive( VertexOutput pin ) : SV_Target
{	 
	float2 uv																	= pin.TexC;
	float depth																	= SampleSceneDepth(uv);
	float3 screenPos															= GetScreenPos(uv, depth);
	float2 velocity																= GetVelocity(uv); // 5.4 motion vector
	float2 prevUV																= uv - velocity;
	float4 cubemap																= GetCubeMap (uv);
	float4 sceneColor															= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,  uv);
	return																		sceneColor;
}

/// <summary>
/// PS_Combine
/// </summary>
float4 PS_Combine( VertexOutput pin ) : SV_Target
{	 
	float2 uv																	= pin.TexC;
	float depth																	= SampleSceneDepth(uv);
	float3 screenPos															= GetScreenPos(uv, depth);
	float3 worldPos																= GetWorlPos(screenPos);
	float3 cubemap																= GetCubeMap (uv);
	float4 worldNormal															= GetNormal (uv);
	float4 diffuse																=  GetAlbedo(uv);
	float occlusion																= 1 - diffuse.a;
	float4 specular																= GetSpecular (uv);
	float roughness																= GetRoughness(uv);
	float4 sceneColor															= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,  uv);
	// sceneColor.rgb																= max(1e-5, sceneColor.rgb - cubemap.rgb);
	float4 reflection															= GetSampleColor(_ReflectionBuffer, sampler_ReflectionBuffer, uv);
	float3 viewDir																= GetViewDir(worldPos);
	float NdotV																	= saturate(dot(worldNormal, -viewDir));
	float3 reflDir																= normalize( reflect( -viewDir, worldNormal ) );
	float fade																	= saturate(dot(-viewDir, reflDir) * 2.0);
	float mask																	= sqr(reflection.a) /* fade*/;
	float oneMinusReflectivity;

	BRDFData brdf_data;
	InitializeBRDFData(diffuse, 0, specular, 0, 1, brdf_data);
	// if(_UseFresnel == 1)													
	// 	reflection.rgb															= EnvironmentBRDF(brdf_data,diffuse,specular,reflDir);
	// reflection.rgb																*= occlusion;
	// if(_DebugPass == 0)
	// 	sceneColor.rgb															+= lerp(cubemap.rgb, reflection.rgb, mask); // Combine reflection and cubemap and add it to the scene color 
	// else if(_DebugPass == 1)
	// 	sceneColor.rgb															= reflection.rgb * mask;
	// else if(_DebugPass == 2)
	// 	sceneColor.rgb															= cubemap;
	// else if(_DebugPass == 3)
	// 	sceneColor.rgb															= lerp(cubemap.rgb, reflection.rgb, mask);
	// else if(_DebugPass == 4)
	// 	sceneColor																= mask;
	// else if(_DebugPass == 5)
	// 	sceneColor.rgb															+= lerp(0.0, reflection.rgb, mask);
	// else if(_DebugPass == 6)
	// 	sceneColor.rgb															= GetSampleColor(_RayCast, sampler_RayCast, uv);
	// else if(_DebugPass == 7)
	// {
	// 	int2 pos = uv;
	// 	float2 random = RandN2(pos, _SinTime.xx * _UseTemporal);
	// 	float2 jitter = SAMPLE_TEXTURE2D_LOD(_Noise, sampler_Noise,float4((uv + random) * _RayCastSize.xy / _NoiseSize.xy, 0, -255),0); // Blue noise generated by https://github.com/bartwronski/BlueNoiseGenerator/
	// 	sceneColor.rg = jitter;
	// 	sceneColor.b = 0;
	// }
	return reflection;
}

/// <summary>
/// PS_Depth
/// </summary>
float4 PS_Depth(VertexOutput pin ) : SV_Target
{
	float2 uv																	= pin.TexC;
	return																		SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
}

static const int2 offsets[7]													= {{-3, -3}, {-2, -2}, {-1, -1}, {0, 0}, {1, 1}, {2, 2}, {3, 3}};
static const float weights[7]													= {0.001f, 0.028f, 0.233f, 0.474f, 0.233f, 0.028f, 0.001f};

/// <summary>
/// PS_MipMapBlur
/// </summary>
float4 PS_MipMapBlur( VertexOutput pin ) : SV_Target
{	 
	float2 uv																	= pin.TexC;
	int NumSamples																= 7;

	float4 result																= 0.0;
	for(int i = 0; i < NumSamples; i++)
	{
		float2 offset															= offsets[i] * _GaussianDir;
		float4 sampleColor														= SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex,uv + offset,_MipMapCount);
		if(_Fireflies == 1)
			sampleColor.rgb														/= 1 + Luminance(sampleColor.rgb);
		result																	+= sampleColor * weights[i];
	}

	if(_Fireflies == 1)
		result																	/= 1 - Luminance(result.rgb);
	return result;
}

/// <summary>
/// PS_Debug
/// </summary>
float4 PS_Debug( VertexOutput pin ) : SV_Target
{	 
	float2 uv																	= pin.TexC;
	return																		SAMPLE_TEXTURE2D_LOD(_ReflectionBuffer,sampler_ReflectionBuffer,uv,_SmoothnessRange * 4.0);
}
/* $Header: /SSR/ScreenSpaceRayTracingPass.hlsl         6/09/21 23:35p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
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

void Temporal(VertexOutput pin, out half4 reflection : SV_Target)
{
    float depth;
    float2 uv, velocity, averageDepthMin, averageDepthMax, prevUV;
    float3 PosS, hitPacked;
    float4 current, previous, currentTopLeft, currentTopCenter, currentTopRight, currentMiddleLeft, currentMiddleCenter,
    currentMiddleRight, currentBottomLeft, currentBottomCenter, currentBottomRight, currentMin, currentMax;

    GetCommonVector_Temporal(pin.TexC, uv, depth, PosS, hitPacked, velocity);
    GetAverageDepthAndPreUV(hitPacked, velocity, uv, averageDepthMin, averageDepthMax, prevUV);
    GetSampleColorCurrent(uv, prevUV, current, previous, currentTopLeft, currentTopCenter,currentTopRight,
        currentMiddleLeft, currentMiddleCenter, currentMiddleRight, currentBottomLeft,currentBottomCenter, currentBottomRight);
    GetCurrentMaxMin(currentMin, currentMax);
    previous                                                                    = clamp(previous, currentMin, currentMax);
    reflection                                                                  = lerp(current, previous, saturate(_TResponse *  (1 - length(velocity) * 8)) );
}

float4 Resolve (VertexOutput pin) : SV_Target
{
    float roughness, depth, NoV, coneTangent, maxMipLevel;
    int2 pos;
    float2 uv, random, blueNoise;
    float3 NormalV, PosS, PosW, PosV, V;
    float4 NormalW, Specular;

    GetCommonVector_Resolve(pin.TexC, roughness, depth, NoV, coneTangent, maxMipLevel,uv, pos, random, blueNoise, NormalV, PosS,
        PosW, PosV, V,NormalW, Specular);

    int NumResolve                                                              = 1;
    if(_RayReuse == 1)
        NumResolve                                                              = 4;

    float4 result                                                               = 0.0;
    float weightSum                                                             = 0.0;

   for(int i = 0; i < NumResolve; i++)
   {
			// float2 offsetUV = offset[i] * (1.0 / _ResolveSize.xy);
			// offsetUV =  mul(offsetRotationMatrix, offsetUV);
			// float2 neighborUv = uv + offsetUV;
   //
   //          float4 hitPacked = tex2Dlod(_RayCast, float4(neighborUv, 0.0, 0.0));
   //          float2 hitUv = hitPacked.xy;
   //          float hitZ = hitPacked.z;
   //          float hitPDF = hitPacked.w;
			// float hitMask = tex2Dlod(_RayCastMask, float4(neighborUv, 0.0, 0.0)).r;
   //
			// float3 hitViewPos = GetViewPos(GetScreenPos(hitUv, hitZ));
			// float weight = 1.0;
			// if(_UseNormalization == 1)
			// 	 weight =  BRDF_Unity_Weight(normalize(-viewPos) /*V*/, normalize(hitViewPos - viewPos) /*L*/, viewNormal /*N*/, roughness) / max(1e-5, hitPDF);
   //
			// float intersectionCircleRadius = coneTangent * length(hitUv - uv);
			// float mip = clamp(log2(intersectionCircleRadius * max(_ResolveSize.x, _ResolveSize.y)), 0.0, maxMipLevel);
   //
			// float4 sampleColor = float4(0.0,0.0,0.0,1.0);
			// sampleColor.rgb = tex2Dlod(_MainTex, float4(hitUv, 0.0, mip)).rgb;
			// sampleColor.a = RayAttenBorder (hitUv, _EdgeFactor) * hitMask;
   //
			// if(_Fireflies == 1)
			// 	sampleColor.rgb /= 1 + Luminance(sampleColor.rgb);
   //
   //          result += sampleColor * weight;
   //          weightSum += weight;
   }
}
/* $Header: /PBR/DistillingLitRayTracing.hlsl         7/24/21 20:55p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : DistillingLitRayTracing.hlsl                                 *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/
#ifndef DistillingLitRayTracing_INCLUDE
#define DistillingLitRayTracing_INCLUDE

#include "Assets/Distilling RP/Runtime/GlobalIllumation/RTRT/Common.hlsl"
#include "Assets/Distilling RP/Runtime/GlobalIllumation/RTRT/PRNG.hlsl"
#include "Assets/Distilling RP/Runtime/GlobalIllumation/RTRT/ONB.hlsl"

TEXTURE2D(_Albedo);                                                            SAMPLER(sampler_Albedo);
TEXTURE2D(_EmissionMap);                                                       SAMPLER(sampler_EmissionMap);
uniform TEXTURE2D(_LUT);                                                       SAMPLER(sampler_LUT);
CBUFFER_START(UnityPerMaterial) 
uniform float                                                                   _MetallicStrength;
uniform float                                                                   _RoughnessStrength;
uniform float3                                                                  _Rf0;
uniform float4                                                                  _AlbeodColor;
uniform float4                                                                  _EmissionColor;
CBUFFER_END

struct IntersectionVertex
{
    float3 normalOS;
    float2 uv;
    float2 screenUV;
};

void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
{
    outVertex.normalOS                                                          = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);//normal  
    outVertex.uv                                                                = UnityRayTracingFetchVertexAttribute2(vertexIndex, kVertexAttributeTexCoord0);//uv
}

void GetCurrentIntersectionVertex(AttributeData attributeData, out IntersectionVertex outVertex)
{
    // Fetch the indices of the currentr triangle
    uint3 triangleIndices                                                       = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

    // Fetch the 3 vertices
    IntersectionVertex v0, v1, v2;
    FetchIntersectionVertex(triangleIndices.x, v0);
    FetchIntersectionVertex(triangleIndices.y, v1);
    FetchIntersectionVertex(triangleIndices.z, v2);

    // Compute the full barycentric coordinates
    float3 barycentricCoordinates                                               = float3(1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);
    float3 normalOS                                                             = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
    outVertex.normalOS                                                          = normalOS;
    float2 uv                                                                   = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.uv, v1.uv, v2.uv, barycentricCoordinates);
    outVertex.uv                                                                = uv;
}

float ScatteringPDF(float3 inOrigin, float3 inDirection, float inT, float3 hitNormal, float3 scatteredDir)
{
    float cosine                                                                = dot(hitNormal, scatteredDir);
    //return (1/2*M_PI);        
    return                                                                      max(0.0f, cosine / M_PI);
}

[shader("closesthit")]
void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
{
    IntersectionVertex vertexN;
    GetCurrentIntersectionVertex(attributeData,vertexN);
    float3x3 objectToWorld                                                      = (float3x3)ObjectToWorld3x4();
    float3 normalWS                                                             = normalize(mul(objectToWorld, vertexN.normalOS));
    
    float2 uv                                                                   = vertexN.uv;

    float4 color = float4(0, 0, 0, 1);
    if (rayIntersection.remainingDepth > 0)
    {
        // Get position in world space.
        float3 origin                                                           = WorldRayOrigin();
        float3 direction                                                        = WorldRayDirection();
        float t                                                                 = RayTCurrent();
        float3 positionWS                                                       = origin + direction * t;
        // Make reflection ray. 
        float3 reflectDir                                                       = reflect(direction, normalWS);
        if (dot(reflectDir, normalWS) < 0.0f)   
            reflectDir                                                          = direction;
        
        ONB uvw;    
        ONBBuildFromW(uvw, normalWS);   
        RayDesc rayDescriptor;  
        rayDescriptor.Origin                                                    = positionWS + 0.001f * reflectDir;
        rayDescriptor.Direction                                                 =  reflectDir * _MetallicStrength + (1 - _MetallicStrength) * ONBLocal(uvw, GetRandomCosineDirection(rayIntersection.PRNGStates));
        rayDescriptor.TMin                                                      = 1e-5f;
        rayDescriptor.TMax                                                      = _CameraFarDistance;
        
        // Tracing reflection.
        RayIntersection reflectionRayIntersection;
        reflectionRayIntersection.remainingDepth                                = rayIntersection.remainingDepth - 1;
        reflectionRayIntersection.PRNGStates                                    = rayIntersection.PRNGStates;
        reflectionRayIntersection.color                                         = float4(0.0f, 0.0f, 0.0f, 0.0f);
        
        float pdf                                                               = dot(normalWS, rayDescriptor.Direction) / (2*M_PI);
    
        TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);
    
        rayIntersection.PRNGStates                                              = reflectionRayIntersection.PRNGStates;
        //color = reflectionRayIntersection.color;
        
        color                                                                   = ScatteringPDF(origin, direction, t, normalWS, rayDescriptor.Direction) * reflectionRayIntersection.color / pdf;
        color                                                                   = max(float4(0, 0, 0, 0), color);
    }

    float4 Emission                                                             = _EmissionMap.SampleLevel(sampler_EmissionMap, uv, 0);
    Emission                                                                    = float4(Emission.rgb * _EmissionColor ,1.0f);
    
    float4 texColor                                                             = _AlbeodColor * _Albedo.SampleLevel(sampler_Albedo, uv, 0);
    
    rayIntersection.color                                                       = texColor * 0.5 * color + Emission;
}

#endif
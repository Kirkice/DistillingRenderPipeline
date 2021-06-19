/* $Header: /SSR/BRDFLib.hlsl         6/09/21 23:35p KirkZhu $ */
/*---------------------------------------------------------------------------------------------*
*                                                                                             *
*                 Project Name : DistillingRenderPipeline                                     *
*                                                                                             *
*                    File Name : BRDFLib.hlsl                                                 *
*                                                                                             *
*                   Programmer : Kirk Zhu                                                     *
*                                                                                             *
*---------------------------------------------------------------------------------------------*/

#define     PI                                          3.141592

/// <summary>
/// D_GGX
/// </summary>
float D_GGX(float Roughness, float NdotH)
{
	float m																			= Roughness * Roughness;
	float m2																		= m * m;
	
	float D																			= m2 / (PI * sqrt(sqrt(NdotH) * (m2 - 1) + 1));
	
	return																			D;
}

/// <summary>
/// G_GGX
/// </summary>
float G_GGX(float Roughness, float NdotL, float NdotV)
{
	float m																			= Roughness * Roughness;
	float m2																		= m * m;

	float G_L																		= 1.0f / (NdotL + sqrt(m2 + (1 - m2) * NdotL * NdotL));
	float G_V																		= 1.0f / (NdotV + sqrt(m2 + (1 - m2) * NdotV * NdotV));
	float G																			= G_L * G_V;
	
	return																			G;
}

/// <summary>
/// BRDF_UE4
/// </summary>
float BRDF_UE4(float3 V, float3 L, float3 N, float Roughness)
{
		float3 H																	= normalize(L + V);

		float NdotH 																= saturate(dot(N,H));
		float NdotL 																= saturate(dot(N,L));
		float NdotV 																= saturate(dot(N,V));

		float D 																	= D_GGX(Roughness, NdotH);
		float G 																	= G_GGX(Roughness, NdotL, NdotV);

		return																		D * G;
}

/// <summary>
/// BRDF_Unity_Weight
/// </summary>
float BRDF_Unity_Weight(float3 V, float3 L, float3 N, float Roughness)
{
	float3 H																		= normalize(L + V);

	float NdotH 																	= saturate(dot(N,H));
	float NdotL 																	= saturate(dot(N,L));
	float NdotV 																	= saturate(dot(N,V));

	half G 																			= SmithJointGGXVisibilityTerm (NdotL, NdotV, Roughness);
	half D 																			= GGXTerm (NdotH, Roughness);

	return																			(D * G) * (PI / 4.0);
}

/// <summary>
/// TangentToWorld
/// </summary>
float4 TangentToWorld(float3 N, float4 H)
{
	float3 UpVector																	= abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
	float3 T 																		= normalize( cross( UpVector, N ) );
	float3 B 																		= cross( N, T );
				 
	return																			float4((T * H.x) + (B * H.y) + (N * H.z), H.w);
}

/// <summary>
/// ImportanceSampleGGX
/// </summary>
float4 ImportanceSampleGGX(float2 Xi, float Roughness)
{
	float m																			= Roughness * Roughness;
	float m2																		= m * m;
		
	float Phi																		= 2 * PI * Xi.x;
				 
	float CosTheta 																	= sqrt((1.0 - Xi.y) / (1.0 + (m2 - 1.0) * Xi.y));
	float SinTheta 																	= sqrt(max(1e-5, 1.0 - CosTheta * CosTheta));
				 
	float3 H;
	H.x 																			= SinTheta * cos(Phi);
	H.y 																			= SinTheta * sin(Phi);
	H.z 																			= CosTheta;
		
	float d 																		= (CosTheta * m2 - CosTheta) * CosTheta + 1;
	float D 																		= m2 / (PI * d * d);
	float pdf																		= D * CosTheta;

	return																			float4(H, pdf); 
}

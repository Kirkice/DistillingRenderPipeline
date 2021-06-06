Shader "Hidden/Distilling RP/CopyShadowMask" 
{
    SubShader 
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
			#include "Assets/Distilling RP/ShaderLibrary/Lighting.hlsl"

            #pragma vertex                                              VS
            #pragma fragment                                            PS

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ Anti_Aliasing_ON

            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END
            
            struct Vertex_Input
            {
                float4 positionOS                                       : POSITION;
            };

            struct Vertex_Output
            {
                float4 positionCS                                       : SV_POSITION;
                float3 PosW												: TEXCOORD0;
            };


            Vertex_Output VS(Vertex_Input vin)
            {
                Vertex_Output vout;
                vout.positionCS                                         = TransformObjectToHClip(vin.positionOS.xyz);
                vout.PosW		                                       = TransformObjectToWorld(vin.positionOS);
                return vout;
            }

            half4 PS(Vertex_Output pin) : SV_Target
            {
				float4 SHADOW_COORDS = TransformWorldToShadowCoord(pin.PosW);
				Light mainLight = GetMainLight(SHADOW_COORDS);
				half shadow = MainLightRealtimeShadow(SHADOW_COORDS);
            	return float4(shadow, shadow, shadow,1);
            }
            ENDHLSL
        }
		pass 
    	{
			Tags{ "LightMode" = "ShadowCaster" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
 
			struct appdata
			{
				float4 vertex : POSITION;
			};
 
			struct v2f
			{
				float4 pos : SV_POSITION;
			};
 
			sampler2D _MainTex;
			float4 _MainTex_ST;
 
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
				return o;
			}
			float4 frag(v2f i) : SV_Target
			{
				float4 color;
				color.xyz = float3(0.0, 0.0, 0.0);
				return color;
			}
			ENDHLSL
		}
    }
}
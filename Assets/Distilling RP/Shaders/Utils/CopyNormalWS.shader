Shader "Hidden/Distilling RP/CopyNormalWS" 
{
    SubShader 
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            HLSLPROGRAM
            #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
            #pragma vertex                                              VS
            #pragma fragment                                            PS

            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END
            
            struct Vertex_Input
            {
                float4 positionOS                                       : POSITION;
                float3 NormalL                                          : NORMAL;
            };

            struct Vertex_Output
            {
                float4 positionCS                                       : SV_POSITION;
                float3 NormalW                                          : TEXCOORD0;
            };


            Vertex_Output VS(Vertex_Input vin)
            {
                Vertex_Output vout;
                vout.positionCS                                         = TransformObjectToHClip(vin.positionOS.xyz);
                vout.NormalW                                            = TransformObjectToWorldNormal(vin.NormalL);
                return vout;
            }

            half4 PS(Vertex_Output pin) : SV_Target
            {
                return half4(pin.NormalW,1);
            }
            ENDHLSL
        }
    }
}
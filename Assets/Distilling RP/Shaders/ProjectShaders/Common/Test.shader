Shader "Custom/UnlitShaderExample" 
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
                float2 uv                                               : TEXCOORD0;
            };

            struct Vertex_Output
            {
                float4 positionCS                                       : SV_POSITION;
                float2 uv                                               : TEXCOORD0;
            };

            TEXTURE2D(_NormalWSTexture);                                SAMPLER(sampler_NormalWSTexture);

            Vertex_Output VS(Vertex_Input vin)
            {
                Vertex_Output vout;
                vout.positionCS                                         = TransformObjectToHClip(vin.positionOS.xyz);
                vout.uv                                                 = vin.uv;
                return vout;
            }

            half4 PS(Vertex_Output pin) : SV_Target
            {
                half4 NormalWS                                          = SAMPLE_TEXTURE2D(_NormalWSTexture,sampler_NormalWSTexture,pin.uv);
                return NormalWS;
            }
            ENDHLSL
        }
    }
}
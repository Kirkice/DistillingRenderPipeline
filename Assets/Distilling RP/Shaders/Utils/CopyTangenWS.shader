Shader "Hidden/Distilling RP/CopyTangentWS" 
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
                float4 TangnetL                                          : TANGENT;
            };

            struct Vertex_Output
            {
                float4 positionCS                                       : SV_POSITION;
                float4 TangentW                                         : TEXCOORD0;
            };


            Vertex_Output VS(Vertex_Input vin)
            {
                Vertex_Output vout;
                vout.positionCS                                         = TransformObjectToHClip(vin.positionOS.xyz);
                vout.TangentW.xyz                                       = TransformObjectToWorld(vin.TangnetL);
                return vout;
            }

            half4 PS(Vertex_Output pin) : SV_Target
            {
                return half4(pin.TangentW);
            }
            ENDHLSL
        }
    }
}
﻿Shader "Hidden/Distilling RP/CopyPosW" 
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
            };

            struct Vertex_Output
            {
                float4 positionCS                                       : SV_POSITION;
                float3 PosW                                             : TEXCOORD0;
            };


            Vertex_Output VS(Vertex_Input vin)
            {
                Vertex_Output vout;
                vout.positionCS                                         = TransformObjectToHClip(vin.positionOS.xyz);
                vout.PosW                                               = TransformObjectToWorld(vin.positionOS);
                return vout;
            }

            half4 PS(Vertex_Output pin) : SV_Target
            {
                return half4(pin.PosW,1);
            }
            ENDHLSL
        }
    }
}
Shader "Hidden/Distilling RP/DRPStencilDeferred" 
{
    Properties 
    {
        _StencilRef ("StencilRef", Int) = 0
        _StencilReadMask ("StencilReadMask", Int) = 0
        _StencilWriteMask ("StencilWriteMask", Int) = 0

        _LitPunctualStencilRef ("LitPunctualStencilWriteMask", Int) = 0
        _LitPunctualStencilReadMask ("LitPunctualStencilReadMask", Int) = 0
        _LitPunctualStencilWriteMask ("LitPunctualStencilWriteMask", Int) = 0

        _SimpleLitPunctualStencilRef ("SimpleLitPunctualStencilWriteMask", Int) = 0
        _SimpleLitPunctualStencilReadMask ("SimpleLitPunctualStencilReadMask", Int) = 0
        _SimpleLitPunctualStencilWriteMask ("SimpleLitPunctualStencilWriteMask", Int) = 0

        _LitDirStencilRef ("LitDirStencilRef", Int) = 0
        _LitDirStencilReadMask ("LitDirStencilReadMask", Int) = 0
        _LitDirStencilWriteMask ("LitDirStencilWriteMask", Int) = 0

        _SimpleLitDirStencilRef ("SimpleLitDirStencilRef", Int) = 0
        _SimpleLitDirStencilReadMask ("SimpleLitDirStencilReadMask", Int) = 0
        _SimpleLitDirStencilWriteMask ("SimpleLitDirStencilWriteMask", Int) = 0

        _ClearStencilRef ("ClearStencilRef", Int) = 0
        _ClearStencilReadMask ("ClearStencilReadMask", Int) = 0
        _ClearStencilWriteMask ("ClearStencilWriteMask", Int) = 0
    }
    
    HLSLINCLUDE
    #ifdef _DEFERRED_ADDITIONAL_LIGHT_SHADOWS
    #define _ADDITIONAL_LIGHT_SHADOWS 1
    #endif
        
    #include "Assets/Distilling RP/ShaderLibrary/Core.hlsl"
    #include "Assets/Distilling RP/Shaders/Utils/Deferred.hlsl"
    #include "Assets/Distilling RP/ShaderLibrary/Shadows.hlsl"
    struct Vertex_Input
    {
        float4 PosL                                             : POSITION;
        uint vertexID                                           : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Vertex_Output
    {
        float4 PosH                                             : SV_POSITION;
        float3 screenUV                                         : TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    TEXTURE2D_X(_CameraDepthTexture);
    TEXTURE2D_X_HALF(_GBuffer0);
    TEXTURE2D_X_HALF(_GBuffer1);
    TEXTURE2D_X_HALF(_GBuffer2);
        
    #ifdef _DEFERRED_MIXED_LIGHTING
    TEXTURE2D_X_HALF(_GBuffer4);
    #endif
        
    CBUFFER_START(UnityPerMaterial)
        
    #if defined(_SPOT)
    float4 _SpotLightScale;
    float4 _SpotLightBias;
    float4 _SpotLightGuard;
    #endif

    float4x4                                                    _ScreenToWorld[2];
    SamplerState                                                my_point_clamp_sampler;
    uniform float3                                              _LightPosWS;
    uniform half3                                               _LightColor;
    uniform half4                                               _LightAttenuation;
    uniform half3                                               _LightDirection;
    uniform half4                                               _LightOcclusionProbInfo;
    uniform int                                                 _LightFlags;
    uniform int                                                 _ShadowLightIndex;
        
    CBUFFER_END
        
    Vertex_Output VS(Vertex_Input vin)
    {
        Vertex_Output                                           vout;
        
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        
        float3 PosL                                             = vin.PosL.xyz;
        #if defined(_SPOT)
        [flatten] if (any(PosL.xyz))
        {
            PosL.xyz = _SpotLightBias.xyz + _SpotLightScale.xyz * PosL.xyz;
            PosL.xyz = normalize(PosL.xyz) * _SpotLightScale.w;
            PosL.xyz = (PosL.xyz - float3(0, 0, _SpotLightGuard.w)) * _SpotLightGuard.xyz + float3(0, 0, _SpotLightGuard.w);
        }
        #endif

        #if defined(_DIRECTIONAL) || defined(_FOG) || defined(_CLEAR_STENCIL_PARTIAL)
        vout.PosH                                               = float4(PosL.xy, UNITY_RAW_FAR_CLIP_VALUE, 1.0);
        #else
        VertexPositionInputs vertexInput = GetVertexPositionInputs(PosL.xyz);
        vout.PosH                                               = vertexInput.positionCS;
        #endif

        vout.screenUV                                           = vout.PosH.xyw;
        #if UNITY_UV_STARTS_AT_TOP
        vout.screenUV.xy                                        = vout.screenUV.xy * float2(0.5, -0.5) + 0.5 * vout.screenUV.z;
        #else
        vout.screenUV.xy                                        = vout.screenUV.xy * 0.5 + 0.5 * vout.screenUV.z;
        #endif
        
        return vout;
    }

    half4 PS(Vertex_Output pin) : SV_Target
    {
        return                                                  half4(1,1,1,1);
    }

    half4 DeferredShading(Vertex_Output pin) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        
        float2 screen_uv                                        = (pin.screenUV.xy / pin.screenUV.z);
        
        float d                                                 = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, my_point_clamp_sampler, screen_uv, 0).x;
        half4 gbuffer0                                          = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, my_point_clamp_sampler, screen_uv, 0);
        half4 gbuffer1                                          = SAMPLE_TEXTURE2D_X_LOD(_GBuffer1, my_point_clamp_sampler, screen_uv, 0);
        half4 gbuffer2                                          = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, my_point_clamp_sampler, screen_uv, 0);

        #ifdef _DEFERRED_MIXED_LIGHTING
        half4 gbuffer4                                          = SAMPLE_TEXTURE2D_X_LOD(_GBuffer4, my_point_clamp_sampler, screen_uv, 0);
        half4 shadowMask                                        = gbuffer4;
        #else
        half4 shadowMask                                        = 1.0;
        #endif

        uint materialFlags                                      = UnpackMaterialFlags(gbuffer0.a);
        bool materialReceiveShadowsOff                          = (materialFlags & kMaterialFlagReceiveShadowsOff) != 0;
        #if SHADER_API_MOBILE || SHADER_API_SWITCH
        bool materialSpecularHighlightsOff                      = false;
        #else
        bool materialSpecularHighlightsOff                      = (materialFlags & kMaterialFlagSpecularHighlightsOff);
        #endif

        #if defined(_DEFERRED_MIXED_LIGHTING)
        [branch] if ((_LightFlags & materialFlags)              == kMaterialFlagSubtractiveMixedLighting)
            return                                              half4(0.0, 0.0, 0.0, 0.0);
        #endif

        #if defined(USING_STEREO_MATRICES)
        int eyeIndex = unity_StereoEyeIndex;
        #else
        int eyeIndex = 0;
        #endif
        float4 posWS                                            = mul(_ScreenToWorld[eyeIndex], float4(pin.PosH.xy, d, 1.0));
        posWS.xyz                                               *= rcp(posWS.w);
        
        InputData inputData                                     = InputDataFromGbufferAndWorldPosition(gbuffer2, posWS.xyz);

        Light unityLight;
        #if defined(_DIRECTIONAL)
            unityLight.direction                                = _LightDirection;
            unityLight.color                                    = _LightColor.rgb;
            unityLight.distanceAttenuation                      = 1.0;
            if (materialReceiveShadowsOff)
                unityLight.shadowAttenuation                    = 1.0;
            else
            {
                #if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                        float4 shadowCoord                      = float4(screen_uv, 0.0, 1.0);
                    #else
                        float4 shadowCoord                      = TransformWorldToShadowCoord(posWS.xyz);
                    #endif
                    unityLight.shadowAttenuation                = MainLightShadow(shadowCoord, posWS.xyz, shadowMask, _MainLightOcclusionProbes); 
                #elif defined(_DEFERRED_ADDITIONAL_LIGHT_SHADOWS)
                    unityLight.shadowAttenuation                = AdditionalLightShadow(_ShadowLightIndex, posWS.xyz, _LightDirection, shadowMask, _LightOcclusionProbInfo);
                #else
                    unityLight.shadowAttenuation                = 1.0;
                #endif
            }
        #else
            PunctualLightData                                   light;
            light.posWS                                         = _LightPosWS;
            light.radius2                                       = 0.0;
            light.color                                         = float4(_LightColor, 0.0);
            light.attenuation                                   = _LightAttenuation;
            light.spotDirection                                 = _LightDirection;
            light.occlusionProbeInfo                            = _LightOcclusionProbInfo;
            light.flags                                         = _LightFlags;
            unityLight                                          = UnityLightFromPunctualLightDataAndWorldSpacePosition(light, posWS.xyz, shadowMask, _ShadowLightIndex, materialReceiveShadowsOff);
        #endif

        half3 color                                             = half3(0,0,0);
                                        
        BRDFData brdfData                                       = BRDFDataFromGbuffer(gbuffer0, gbuffer1, gbuffer2);
        color                                                   = LightingPhysicallyBased(brdfData, unityLight, inputData.normalWS, inputData.viewDirectionWS, materialSpecularHighlightsOff);
        
        return half4(color, 0.0);
    }

    half4 PSFog(Vertex_Output pin) : SV_Target
    {
        float d                                                 = LOAD_TEXTURE2D_X(_CameraDepthTexture, pin.PosH.xy).x;
        float eye_z                                             = LinearEyeDepth(d, _ZBufferParams);
        float clip_z                                            = UNITY_MATRIX_P[2][2] * -eye_z + UNITY_MATRIX_P[2][3];
        half fogFactor                                          = ComputeFogFactor(clip_z);
        half fogIntensity                                       = ComputeFogIntensity(fogFactor);
        return                                                  half4(unity_FogColor.rgb, fogIntensity);
    }
    
    ENDHLSL
    
    SubShader 
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        // 0 - Stencil pass
        Pass
        {
            Name "Stencil Volume"

            ZTest LEQual
            ZWrite Off
            ZClip false
            Cull Off
            ColorMask 0
            
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                CompFront NotEqual
                PassFront Keep
                ZFailFront Invert
                CompBack NotEqual
                PassBack Keep
                ZFailBack Invert
            }
            
            HLSLPROGRAM
            #pragma exclude_renderers                                   gles
            #pragma multi_compile_vertex _ _SPOT
            #pragma vertex                                              VS
            #pragma fragment                                            PS
            ENDHLSL
        }
        
        // 1 - Deferred Punctual Light (Lit)
        Pass
        {
            Name "Deferred Punctual Light (Lit)"

            ZTest GEqual
            ZWrite Off
            ZClip false
            Cull Front
            Blend One One, Zero One
            BlendOp Add, Add

            Stencil 
            {
                Ref         [_LitPunctualStencilRef]
                ReadMask    [_LitPunctualStencilReadMask]
                WriteMask   [_LitPunctualStencilWriteMask]
                Comp Equal
                Pass Zero
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma exclude_renderers gles

            #pragma multi_compile _POINT _SPOT
            #pragma multi_compile_fragment _LIT
            #pragma multi_compile_fragment _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _DEFERRED_ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _DEFERRED_MIXED_LIGHTING

            #pragma vertex      VS
            #pragma fragment    DeferredShading
            ENDHLSL
        }

        // 2 - Deferred Punctual Light (SimpleLit)
        Pass
        {
            Name "Deferred Punctual Light (SimpleLit)"

            ZTest GEqual
            ZWrite Off
            ZClip false
            Cull Front
            Blend One One, Zero One
            BlendOp Add, Add

            Stencil {
                Ref         [_SimpleLitPunctualStencilRef]
                ReadMask    [_SimpleLitPunctualStencilReadMask]
                WriteMask   [_SimpleLitPunctualStencilWriteMask]
                CompBack Equal
                PassBack Zero
                FailBack Keep
                ZFailBack Keep
            }

            HLSLPROGRAM
            #pragma exclude_renderers gles

            #pragma multi_compile _POINT _SPOT
            #pragma multi_compile_fragment _SIMPLELIT
            #pragma multi_compile_fragment _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _DEFERRED_ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _DEFERRED_MIXED_LIGHTING

            #pragma vertex      VS
            #pragma fragment    PS

            ENDHLSL
        }
        
        // 3 - Directional Light (Lit)
        Pass
        {
            Name "Deferred Directional Light (Lit)"

            ZTest NotEqual
            ZWrite Off
            Cull Off
            Blend One One, Zero One
            BlendOp Add, Add

            Stencil 
            {
                ReadMask    [_LitDirStencilReadMask]
                WriteMask   [_LitDirStencilWriteMask]
                Comp Equal
                Pass Keep
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma exclude_renderers gles

            #pragma multi_compile _DIRECTIONAL
            #pragma multi_compile_fragment _LIT
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _DEFERRED_ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _DEFERRED_MIXED_LIGHTING

            #pragma vertex      VS
            #pragma fragment    DeferredShading
            ENDHLSL
        }
        // 4 - Directional Light (SimpleLit)
        Pass
        {
            Name "Deferred Directional Light (SimpleLit)"

            ZTest NotEqual
            ZWrite Off
            Cull Off
            Blend One One, Zero One
            BlendOp Add, Add

            Stencil {
                Ref         [_SimpleLitDirStencilRef]
                ReadMask    [_SimpleLitDirStencilReadMask]
                WriteMask   [_SimpleLitDirStencilWriteMask]
                Comp Equal
                Pass Keep
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma exclude_renderers gles

            #pragma multi_compile _DIRECTIONAL
            #pragma multi_compile_fragment _SIMPLELIT
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _DEFERRED_ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _DEFERRED_MIXED_LIGHTING

            #pragma vertex      VS
            #pragma fragment    PS
            ENDHLSL
        }
        
        // 5 - Legacy fog
        Pass
        {
            Name "Fog"

            ZTest NotEqual
            ZWrite Off
            Cull Off
            Blend OneMinusSrcAlpha SrcAlpha, Zero One
            BlendOp Add, Add

            HLSLPROGRAM
            #pragma exclude_renderers gles

            #pragma multi_compile _FOG
            #pragma multi_compile FOG_LINEAR FOG_EXP FOG_EXP2

            #pragma vertex      VS
            #pragma fragment    PSFog
            ENDHLSL
        }

        // 6 - Clear stencil partial
        Pass
        {
            Name "ClearStencilPartial"

            ColorMask 0
            ZTest NotEqual
            ZWrite Off
            Cull Off

            Stencil 
            {
                Ref         [_ClearStencilRef]
                ReadMask    [_ClearStencilReadMask]
                WriteMask   [_ClearStencilWriteMask]
                Comp NotEqual
                Pass Zero
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma exclude_renderers gles

            #pragma multi_compile _CLEAR_STENCIL_PARTIAL
            #pragma vertex      VS
            #pragma fragment    PS
            ENDHLSL
        }
    }
}
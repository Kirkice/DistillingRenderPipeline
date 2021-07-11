Shader "Distilling RP/Lit" 
{
    Properties 
    {
        //BasicSettings   
        [Enum(OFF,0,FRONT,1,BACK,2)] _CullMode("Cull Mode", int) = 2
        [Enum(Opaque,0,Cut Out,1, Transparent,2)] _RenderingMode("Rendering Mode", int) = 2
        _Cutoff("Cut Off",Range(0,1)) = 0.5
        _ReceiveShadows("Receive Shadows",float) = 1
        
        //Basic Lighting Settings
        _UseDRPLight("Use DRP Light",float) = 0
        _UseMultipleLight("Use Multiple Light",float) = 1
        _UseRealTimeAreaLight("Use RealTime Area Light",float) = 0          
        
        //Basic Visibility Settings
        _OcclusionMap("Occlusion Map",2D) = "white"{}
        _OcclusionStrength("Occlusion Strength",Range(0,1)) = 1
        _UseDRPShadow("Use DRP Shadow",float) = 0
        _UseVoxel("Use Voxel",float) = 0
        _UsePlanerShadow("Use Planer Shadow",float) = 0
        _UsePCFShadow("Use PCF Shadow",float) = 0
        _UsePCSSShadow("Use PCSS Shadow",float) = 0
        _UseVSMShadow("Use VSM Shadow",float) = 0
        _UseSDFShadow("Use SDF Shadow",float) = 0
        
        //Basic BRDF Settings
        [Enum(Water,0,Glass,1,Plastic,2,Gem,3,Diamond,4,Au,5,Ag,6,Cu,7,Fe,8,Al,9)]_Rf0Mode("Rf0 Mode",int) = 8
        _Rf0("Rf0",vector) = (0.8,0.8,0.8,0)
        _Albedo("Base Map",2D) = "white"{}
        _AlbeodColor("Base Color",Color) = (1,1,1,1)
        _UseMetallic("Use Metallic",float) = 0
        _MetallicMap("Metallic Map",2D) = "white"{}
        _MetallicStrength("Metallic Strength", Range(0,1)) = 0.5
        _UseRoughness("Use Roughness",float) = 0
        _RoughnessMap("Roughness Map", 2D) = "white"{}
        _RoughnessStrength("Roughness Strength",Range(0,1)) = 0.5
        
        //Normal Settings
        _UseNormalMap("Use NormalMap",float) = 0
        _NormalMap("Normal Map",2D) = "Bump"{}
        _NormalScale("Normal Scale",Range(0,1)) = 1
        //Parallax Settings
        
        //MatCap Settings
        _UseMatCap("Use MatCap",float) = 0
        _MatCapMap("MatCap Map", 2D) = "white"{}
        _MatCapColor("MatCap Color", Color) = (1,1,1,1)
        
        //Emission Settings
        _UseEmission("Use Emission",float) = 0
        _EmissionMap("Emission Map",2D) = "white"{}
        [HDR]_EmissionColor("Emission Color",Color) = (0,0,0,0)
        
        //Height Settings
        _UseHeightMap("Use HeightMap",float) = 0
        _HeightMap("Height Map",2D) = "white"{}
        _Height("Displacement Amount",range(0,0.1)) = 0.01
        _HeightAmount("Turbulence Amount",range(0,100)) = 1
        
        //GI Settings
        _UseGI("Use GI",float) = 0
        _UsePRT("Use PRT",float) = 0
        _UseRSM("Use RSM",float) = 0
        _UseSSDO("Use SSDO",float) = 0
        _UseSSR("Use SSR",float) = 0
        _UseLPV("Use LPV",float) = 0
        _UseVoxelGI("Use VoxelGI",float) = 0
        _UseDDGI("Use DDGI",float) = 0
        _UsePathTracing("Use PathTracing",float) = 0
        _GIStrength("GI Strength",Range(0,1)) = 1
        
        //Debug Settings
        _DebugPosW("Debug PosW",float) = 0
        _DebugPosL("Debug PosL",float) = 0
        _DebugTangent("Debug Tangent",float) = 0
        _DebugNormal("Debug Normal",float) = 0
        _DebugUVX("Debug UV (X)",float) = 0
        _DebugUVY("Debug UV (Y)",float) = 0
        _DebugVColorR("Debug VColor (R)",float) = 0
        _DebugVColorG("Debug VColor (G)",float) = 0
        _DebugVColorB("Debug VColor (B)",float) = 0
        _DebugWireframe("Debug Wireframe",float) = 0
    }
    SubShader 
    { 
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        
        Pass
        {
            Cull [_CullMode]
            Tags{"LightMode"    = "UniversalForward"}
            HLSLPROGRAM
            #include "Assets/Distilling RP/Shaders/PBR/DistillingLitForwardPass.hlsl"
            #pragma vertex      Lit_VS
            #pragma geometry    Lit_GS
            #pragma fragment    Lit_PS
            ENDHLSL
        } 
        
        Pass
        {
            Tags{"LightMode" = "ShadowCaster"}
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Assets/Distilling RP/Shaders/LitInput.hlsl"
            #include "Assets/Distilling RP/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Tags{"LightMode" = "DepthOnly"}
            Cull[_Cull]
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Assets/Distilling RP/Shaders/LitInput.hlsl"
            #include "Assets/Distilling RP/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "DistillingShaderGUI"
}
#ifndef UNIVERSAL_BAKEDLIT_INPUT_INCLUDED
#define UNIVERSAL_BAKEDLIT_INPUT_INCLUDED

#include "Assets/Distilling RP/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;
    half _Glossiness;
    half _Metallic;
    half _Surface;
CBUFFER_END

#endif

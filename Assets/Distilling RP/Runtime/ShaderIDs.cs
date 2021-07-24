using System;
using System.Linq;
using System.ComponentModel;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Distilling
{
    public class ShaderIDs
    {
        #region RenderPipeline
        public static readonly string BlueNoiseTexPath = "Assets/ProjectAssets/Resources/Textures/tex_BlueNoise_1024x1024_UNI.tga";
        
        public static readonly int OutputTargetShaderId = Shader.PropertyToID("_OutputTarget");
        public static readonly int PRNGStatesShaderId = Shader.PropertyToID("_PRNGStates");
        public static readonly int FrameIndexShaderId = Shader.PropertyToID("_FrameIndex");
        
        public static readonly int AccelerationStructureShaderId = Shader.PropertyToID("_AccelerationStructure");
        public static readonly int WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
        public static readonly int InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj");
        public static readonly int CameraFarDistance = Shader.PropertyToID("_CameraFarDistance");
        
        public static readonly int FocusCameraLeftBottomCorner = Shader.PropertyToID("_FocusCameraLeftBottomCorner");
        public static readonly int FocusCameraRight = Shader.PropertyToID("_FocusCameraRight");
        public static readonly int FocusCameraUp = Shader.PropertyToID("_FocusCameraUp");
        public static readonly int FocusCameraSize = Shader.PropertyToID("_FocusCameraSize");
        public static readonly int FocusCameraHalfAperture = Shader.PropertyToID("_FocusCameraHalfAperture");
        
        #endregion

        #region Distilling-Lit-Parames
        //BasicSettings
        public static readonly string cullMode = "_CullMode";
        public static readonly string renderingMode = "_RenderingMode";
        public static readonly string cutoff = "_Cutoff";
        public static readonly string receiveShadows = "_ReceiveShadows";
        
        //Basic Lighting Settings
        public static readonly string useDRPLight = "_UseDRPLight";
        public static readonly string useMultipleLight = "_UseMultipleLight";
        public static readonly string useRealTimeAreaLight = "_UseRealTimeAreaLight";

        //Basic Visibility Settings
        public static readonly string occlusionMap = "_OcclusionMap";
        public static readonly string occlusionStrength = "_OcclusionStrength";
        public static readonly string useDRPShadow = "_UseDRPShadow";
        public static readonly string useVoxel = "_UseVoxel";
        public static readonly string usePlanerShadow = "_UsePlanerShadow";
        public static readonly string usePCFShadow = "_UsePCFShadow";
        public static readonly string usePCSSShadow = "_UsePCSSShadow";
        public static readonly string useVSMShadow = "_UseVSMShadow";
        public static readonly string useSDFShadow = "_UseSDFShadow";
        
        //Basic BRDF Settings
        public static readonly string Rf0 = "_Rf0";
        public static readonly string Rf0Mode = "_Rf0Mode";
        public static readonly string albedo = "_Albedo";
        public static readonly string albeodColor = "_AlbeodColor";
        public static readonly string useMetallic = "_UseMetallic";
        public static readonly string metallicMap = "_MetallicMap";
        public static readonly string metallicStrength = "_MetallicStrength";
        public static readonly string useRoughness = "_UseRoughness";
        public static readonly string roughnessMap = "_RoughnessMap";
        public static readonly string roughnessStrength = "_RoughnessStrength";
        
        //Normal Settings
        public static readonly string useNormalMap = "_UseNormalMap";
        public static readonly string normalMap = "_NormalMap";
        public static readonly string normalScale = "_NormalScale";
        
        //Parallax Settings
        public static readonly string useHeightMap = "_UseHeightMap";
        public static readonly string heightMap = "_HeightMap";
        public static readonly string height = "_Height"; 
        public static readonly string heightAmount = "_HeightAmount";
        
        //MatCap Settings
        public static readonly string useMatCap = "_UseMatCap";
        public static readonly string matMap = "_MatCapMap";
        public static readonly string matMapColor = "_MatCapColor";
        
        //Emission Settings
        public static readonly string useEmission = "_UseEmission";
        public static readonly string emissionMap = "_EmissionMap";
        public static readonly string emissionColor = "_EmissionColor";
        
        //GI Settings
        public static readonly string useGI = "_UseGI";
        public static readonly string usePRT = "_UsePRT";
        public static readonly string useRSM = "_UseRSM";
        public static readonly string useSSDO = "_UseSSDO";
        public static readonly string useSSR = "_UseSSR";
        public static readonly string useLPV = "_UseLPV";
        public static readonly string useVoxelGI = "_UseVoxelGI";
        public static readonly string useDDGI = "_UseDDGI";
        public static readonly string usePathTracing = "_UsePathTracing";
        public static readonly string GIStrength = "_GIStrength";

        //Debug Settings
        public static readonly string debugPosW = "_DebugPosW";
        public static readonly string debugPosL = "_DebugPosL";
        public static readonly string debugTangent = "_DebugTangent";
        public static readonly string debugNormal = "_DebugNormal";
        public static readonly string debugUVX = "_DebugUVX";
        public static readonly string debugUVY = "_DebugUVY";
        public static readonly string debugVColorR = "_DebugVColorR";
        public static readonly string debugVColorG = "_DebugVColorG";
        public static readonly string debugVColorB = "_DebugVColorB";
        public static readonly string debugWireframe = "_DebugWireframe";
        #endregion
    }
    
    #region Rf(0°)
    public class Rf0
    {
        public static readonly Vector3 Water = new Vector3(0.02f, 0.02f, 0.02f);
        public static readonly Vector3 Glass = new Vector3(0.03f,0.03f,0.03f);
        public static readonly Vector3 Plastic = new Vector3(0.05f,0.05f,0.05f);
        public static readonly Vector3 Gem = new Vector3(0.08f,0.08f,0.08f);
        public static readonly Vector3 Diamond = new Vector3(0.17f,0.17f,0.17f);
        public static readonly Vector3 Au = new Vector3(1f,0.71f,0.29f);
        public static readonly Vector3 Ag = new Vector3(0.95f,0.93f,0.88f);
        public static readonly Vector3 Cu = new Vector3(0.95f,0.64f,0.54f);
        public static readonly Vector3 Fe = new Vector3(0.56f,0.57f,0.58f);
        public static readonly Vector3 Al = new Vector3(0.91f,0.92f,0.92f);   
    }
    #endregion
}
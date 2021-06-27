using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Distilling;

public class DistillingShaderGUI : ShaderGUI
{
    #region Enum
    /// <summary>
    /// CullingMode
    /// </summary>
    public enum CullingMode
    {
        CullingOff, 
        FrontCulling, 
        BackCulling
    }

    /// <summary>
    /// RenderingMode
    /// </summary>
    public enum RenderingMode
    {
        Opaque,
        CutOut,
        Transparent
    }
    
    public enum Rf0Type
    {
        Water,
        Glass,
        Plastic,
        Gem,
        Diamond,
        Au,
        Ag,
        Cu,
        Fe,
        Al
    }
    #endregion

    #region Public
    public CullingMode cullingMode;
    public RenderingMode renderingMode;
    public Rf0Type rf0Type;

    public GUILayoutOption[] shortButtonStyle = new GUILayoutOption[]{ GUILayout.Width(130) }; 
    public GUILayoutOption[] middleButtonStyle = new GUILayoutOption[]{ GUILayout.Width(130) }; 
    
    public void FindProperties(MaterialProperty[] props)
    {
        cutOff = FindProperty(ShaderIDs.cutoff, props, false);
        albedoMap = FindProperty(ShaderIDs.albedo, props, false);
        albedoColor = FindProperty(ShaderIDs.albeodColor, props, false);
        
        metallicMap = FindProperty(ShaderIDs.metallicMap, props, false);
        metallicStrength = FindProperty(ShaderIDs.metallicStrength, props, false);
        
        roughnessMap = FindProperty(ShaderIDs.roughnessMap, props, false);
        roughnessStrength = FindProperty(ShaderIDs.roughnessStrength, props, false);
        
        normalMap = FindProperty(ShaderIDs.normalMap, props, false);
        normalScale = FindProperty(ShaderIDs.normalScale, props, false);
        
        matCapMap = FindProperty(ShaderIDs.matMap, props, false);
        matCapColor = FindProperty(ShaderIDs.matMapColor, props, false);
        
        occlusionMap = FindProperty(ShaderIDs.occlusionMap, props, false);
        occlusionStrength = FindProperty(ShaderIDs.occlusionStrength, props, false);
        
        emissionMap = FindProperty(ShaderIDs.emissionMap, props, false);
        emissionColor = FindProperty(ShaderIDs.emissionColor, props, false);
        
        gi_Intensity = FindProperty(ShaderIDs.GIStrength, props,false);
    }
    #endregion

    #region Private
    private  MaterialEditor m_MaterialEditor;
    
    static bool _BasicShaderSettings_Foldout = false;
    
    static bool _BasicLightingSettings_Foldout = false;
    static bool _BasicBRDFSettings_Foldout = false;
    static bool _BasicVisibilitySettings_Foldout = false;
    
    static bool _NormalSettings_Foldout = false;
    static bool _ParallaxSettings_Foldout = false;
    static bool _MatCapSettings_Foldout = false;
    static bool _EmissionSettings_Foldout = false;
    
    static bool _GlobalIllumationSettings_Foldout = false;
    static bool _DebugSettings_Foldout = false;
    
    private  MaterialProperty cutOff = null;
    private MaterialProperty albedoMap = null;
    private MaterialProperty albedoColor = null;
    
    private MaterialProperty metallicMap = null;
    private MaterialProperty metallicStrength = null;
    
    private MaterialProperty roughnessMap = null;
    private MaterialProperty roughnessStrength = null;
    
    private MaterialProperty normalMap = null;
    private MaterialProperty normalScale = null;

    private MaterialProperty matCapMap = null;
    private MaterialProperty matCapColor = null;
    
    private MaterialProperty occlusionMap = null;
    private MaterialProperty occlusionStrength = null;
    
    private MaterialProperty emissionMap = null;
    private MaterialProperty emissionColor = null;

    private MaterialProperty gi_Intensity = null;
    
    private static class Styles
    {
        public static GUIContent AlbedoMapText = new GUIContent("AlbedoMap","Albedo Color : Texture(sRGB) × Color(RGB) Default:White");
        public static GUIContent MetallicMapText = new GUIContent("MetallicMap","Metallic Parames : Texture(sRGB) × Scale Default:0.5f");
        public static GUIContent RoughnessMapText = new GUIContent("RoughnessMap","Roughness Parames : Texture(sRGB) × Scale Default:0.5f");
        public static GUIContent OcclusionMapText = new GUIContent("OcclusionMap","Occlusion Parames : Texture(sRGB) × Scale Default:1f");
        public static GUIContent normalMapText = new GUIContent("NormalMap","NormalMap : Texture(bump)");
        public static GUIContent matMapText = new GUIContent("MatMap","MatMap : Texture(white)");
        public static GUIContent emissionMapText = new GUIContent("EmissionMap","EmissionMap : Texture(white)");
    }
    #endregion

    #region OnGUI
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        EditorGUIUtility.fieldWidth = 0;
        FindProperties(props);
        m_MaterialEditor = materialEditor;
        Material material = materialEditor.target as Material;
        
        //LinkButton
        EditorGUILayout.BeginHorizontal();
        OpenManualLink();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        //BasicSettings
        _BasicShaderSettings_Foldout = Foldout(_BasicShaderSettings_Foldout, "【Basic Shader Settings】");
        if(_BasicShaderSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_SetCullingMode(material);
            GUI_SetRenderingMode(material);
            GUI_ReceiveShadow(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //Basic Lighting Settings
        _BasicLightingSettings_Foldout = Foldout(_BasicLightingSettings_Foldout, "【Basic Lighting Settings】");
        if(_BasicLightingSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_BasicLightingSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //Basic BRDF Settings
        _BasicBRDFSettings_Foldout = Foldout(_BasicBRDFSettings_Foldout, "【Basic BRDF Settings】");
        if(_BasicBRDFSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_BasicBRDFSettings(material);
            GUI_SetRf0Mode(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //Basic Visibility Settings
        _BasicVisibilitySettings_Foldout = Foldout(_BasicVisibilitySettings_Foldout, "【Basic Visibility Settings】");
        if(_BasicVisibilitySettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_BasicVisibilitySettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //Normal Settings
        _NormalSettings_Foldout = Foldout(_NormalSettings_Foldout, "【Normal Settings】");
        if(_NormalSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_NormalSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //Parallax Settings
        _ParallaxSettings_Foldout = Foldout(_ParallaxSettings_Foldout, "【Parallax Settings】");
        if(_ParallaxSettings_Foldout)
        {
            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //MatCap Settings
        _MatCapSettings_Foldout = Foldout(_MatCapSettings_Foldout, "【MatCap Map Settings】");
        if(_MatCapSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_MatCapSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //Emission Settings
        _EmissionSettings_Foldout = Foldout(_EmissionSettings_Foldout, "【Emission : Self-luminescence Settings】");
        if(_EmissionSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_EmissionSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //GI Settings
        _GlobalIllumationSettings_Foldout = Foldout(_GlobalIllumationSettings_Foldout, "【GlobalIllumation Type and Settings】");
        if(_GlobalIllumationSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_GISettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //Debug Settings
        _DebugSettings_Foldout = Foldout(_DebugSettings_Foldout, "【Debug and Test】");
        if(_DebugSettings_Foldout)
        {
            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }
    #endregion

    #region Foldout
    /// <summary>
    /// Foldout
    /// </summary>
    /// <param name="display"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    static bool Foldout(bool display, string title)
    {
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.boldLabel).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = 22;
        style.contentOffset = new Vector2(20f, -2f);

        var rect = GUILayoutUtility.GetRect(16f, 22f, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }
    #endregion

    #region OpenManualLink
    /// <summary>
    /// OpenManualLink
    /// </summary>
    void OpenManualLink()
    {
        if (GUILayout.Button("DRP Introduce",middleButtonStyle))
        {
            Application.OpenURL("https://www.zhihu.com/column/c_1384961728715816960");
        }
        if (GUILayout.Button("DRP Lit Introduce",middleButtonStyle))
        {
            Application.OpenURL("https://www.zhihu.com/column/c_1384961728715816960");
        }
        if (GUILayout.Button("Repository Link",middleButtonStyle))
        {
            Application.OpenURL("https://github.com/Kirkice/DistillingRenderPipeline");
        }
    }
    #endregion
    
    #region GUI_SetCullingMode
    /// <summary>
    /// GUI_SetCullingMode
    /// </summary>
    /// <param name="material"></param>
    void GUI_SetCullingMode(Material material)
    {
        int _CullMode_Setting = material.GetInt(ShaderIDs.cullMode);
        if ((int)CullingMode.CullingOff == _CullMode_Setting)
        {
            cullingMode = CullingMode.CullingOff;
        }
        else if((int)CullingMode.FrontCulling == _CullMode_Setting)
        {
            cullingMode = CullingMode.FrontCulling;
        }
        else 
        {
            cullingMode = CullingMode.BackCulling;
        }
        cullingMode = (CullingMode)EditorGUILayout.EnumPopup("Cull Mode", cullingMode);
        if(cullingMode == CullingMode.CullingOff)
        {
            material.SetFloat(ShaderIDs.cullMode,0);
        }
        else if(cullingMode == CullingMode.FrontCulling)
        {
            material.SetFloat(ShaderIDs.cullMode,1);
        }
        else
        {
            material.SetFloat(ShaderIDs.cullMode,2);
        }
    }
    #endregion

    #region GUI_SetRenderingMode
    /// <summary>
    /// GUI_SetRenderingMode
    /// </summary>
    /// <param name="material"></param>
    void GUI_SetRenderingMode(Material material)
    {
        int _RenderingMode_Setting = material.GetInt(ShaderIDs.renderingMode);
        if ((int)RenderingMode.Opaque == _RenderingMode_Setting)
        {
            renderingMode = RenderingMode.Opaque;
        }
        else if((int)RenderingMode.CutOut == _RenderingMode_Setting)
        {
            renderingMode = RenderingMode.CutOut;
        }
        else
        {
            renderingMode = RenderingMode.Transparent;
        }
        renderingMode = (RenderingMode)EditorGUILayout.EnumPopup("Rendering Mode", renderingMode);
        if(renderingMode == RenderingMode.Opaque)
        {
            material.SetFloat(ShaderIDs.renderingMode,0);
        }
        else if(renderingMode == RenderingMode.CutOut)
        {
            material.SetFloat(ShaderIDs.renderingMode,1);
        }
        else
        {
            material.SetFloat(ShaderIDs.renderingMode,2);
        }

        if (renderingMode == RenderingMode.CutOut)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.RangeProperty(cutOff, "Thresould");
            EditorGUI.indentLevel--;
        }
    }
    #endregion

    #region GUI_ReceiveShadow
    /// <summary>
    /// GUI_ReceiveShadow
    /// </summary>
    /// <param name="material"></param>
    void GUI_ReceiveShadow(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Receive Shadows");
        if(material.GetFloat(ShaderIDs.receiveShadows) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.receiveShadows,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.receiveShadows,0);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region GUI_BasicLightingSettings
    /// <summary>
    /// GUI_BasicLightingSettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_BasicLightingSettings(Material material)
    {
        GUILayout.Label("Lighting Basic Parames Settings : Bools ", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Distilling RP Lights");
        if(material.GetFloat(ShaderIDs.useDRPLight) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useDRPLight,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useDRPLight,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Multiple Lights");
        if(material.GetFloat(ShaderIDs.useMultipleLight) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useMultipleLight,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useMultipleLight,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("RealTime Area Light");
        if(material.GetFloat(ShaderIDs.useRealTimeAreaLight) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useRealTimeAreaLight,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useRealTimeAreaLight,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region GUI_BasicBRDFSettings
    /// <summary>
    /// GUI_BasicBRDFSettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_BasicBRDFSettings(Material material)
    {
        GUILayout.Label("PBR Basic Parames Settings : Textures × Colors", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.TexturePropertySingleLine(Styles.AlbedoMapText, albedoMap, albedoColor);
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.TexturePropertySingleLine(Styles.MetallicMapText, metallicMap, metallicStrength);
        if (metallicMap != null) {
            material.SetFloat(ShaderIDs.useMetallic,1);
        }else {
            material.SetFloat(ShaderIDs.useMetallic,0);
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.TexturePropertySingleLine(Styles.RoughnessMapText, roughnessMap, roughnessStrength);
        if (roughnessMap != null) {
            material.SetFloat(ShaderIDs.useRoughness,1);
        }else {
            material.SetFloat(ShaderIDs.useRoughness,0);
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region GUI_SetRf0Mode
    /// <summary>
    /// GUI_SetRf0Mode
    /// </summary>
    /// <param name="material"></param>
    void GUI_SetRf0Mode(Material material)
    {
        int _Rf0Mode_Setting = material.GetInt(ShaderIDs.Rf0Mode);
        if ((int)Rf0Type.Water == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Water;
        }
        else if((int)Rf0Type.Glass == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Glass;
        }
        else if((int)Rf0Type.Plastic == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Plastic;
        }
        else if((int)Rf0Type.Gem == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Gem;
        }
        else if((int)Rf0Type.Diamond == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Diamond;
        }
        else if((int)Rf0Type.Ag == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Ag;
        }
        else if((int)Rf0Type.Au == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Au;
        }
        else if((int)Rf0Type.Cu == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Cu;
        }
        else if((int)Rf0Type.Fe == _Rf0Mode_Setting)
        {
            rf0Type = Rf0Type.Fe;
        }
        else
        {
            rf0Type = Rf0Type.Al;
        }
        rf0Type = (Rf0Type)EditorGUILayout.EnumPopup("Rf(0°) Factory", rf0Type);
        if(rf0Type == Rf0Type.Water)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,0);
            material.SetVector(ShaderIDs.Rf0,Rf0.Water);
        }
        else if(rf0Type == Rf0Type.Glass)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,1);
            material.SetVector(ShaderIDs.Rf0,Rf0.Glass);
        }
        else if(rf0Type == Rf0Type.Plastic)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,2);
            material.SetVector(ShaderIDs.Rf0,Rf0.Plastic);
        }
        else if(rf0Type == Rf0Type.Gem)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,3);
            material.SetVector(ShaderIDs.Rf0,Rf0.Gem);
        }
        else if(rf0Type == Rf0Type.Diamond)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,4);
            material.SetVector(ShaderIDs.Rf0,Rf0.Diamond);
        }
        else if(rf0Type == Rf0Type.Au)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,5);
            material.SetVector(ShaderIDs.Rf0,Rf0.Au);
        }
        else if(rf0Type == Rf0Type.Ag)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,6);
            material.SetVector(ShaderIDs.Rf0,Rf0.Ag);
        }
        else if(rf0Type == Rf0Type.Cu)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,7);
            material.SetVector(ShaderIDs.Rf0,Rf0.Cu);
        }
        else if(rf0Type == Rf0Type.Fe)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,8);
            material.SetVector(ShaderIDs.Rf0,Rf0.Fe);
        }
        else if(rf0Type == Rf0Type.Al)
        {
            material.SetFloat(ShaderIDs.Rf0Mode,9);
            material.SetVector(ShaderIDs.Rf0,Rf0.Al);
        }
    }
    #endregion
    
    #region GUI_BasicVisibilitySettings
    /// <summary>
    /// GUI_BasicVisibilitySettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_BasicVisibilitySettings(Material material)
    {
        GUILayout.Label("Visibility Basic Parames Settings : Textures × Scale", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.TexturePropertySingleLine(Styles.OcclusionMapText, occlusionMap, occlusionStrength);
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Voxel Active");
        if(material.GetFloat(ShaderIDs.useVoxel) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useVoxel,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useVoxel,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Distilling RP Shadow");
        if(material.GetFloat(ShaderIDs.useDRPShadow) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useDRPShadow,1);
            }
        }
        else
        {
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useDRPShadow,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.GetFloat(ShaderIDs.useDRPShadow) == 1)
        {
            EditorGUI.indentLevel++;
            GUI_DRPShadowSettings(material);
            EditorGUI.indentLevel--;
        }
    }
    #endregion

    #region GUI_DRPShadowSettings
    /// <summary>
    /// GUI_DRPShadowSettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_DRPShadowSettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Planar Shadow Active");
        if(material.GetFloat(ShaderIDs.usePlanerShadow) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.usePlanerShadow,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.usePlanerShadow,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("PCF Shadow Active");
        if(material.GetFloat(ShaderIDs.usePCFShadow) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.usePCFShadow,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.usePCFShadow,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("PCSS Shadow Active");
        if(material.GetFloat(ShaderIDs.usePCSSShadow) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.usePCSSShadow,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.usePCSSShadow,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("VSM Shadow Active");
        if(material.GetFloat(ShaderIDs.useVSMShadow) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useVSMShadow,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useVSMShadow,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Voxel-SDF Shadow Active");
        if(material.GetFloat(ShaderIDs.useSDFShadow) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useSDFShadow,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useSDFShadow,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
    }
    #endregion
    
    #region GUI_NormalSettings
    /// <summary>
    /// GUI_NormalSettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_NormalSettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Normal Active");
        if(material.GetFloat(ShaderIDs.useNormalMap) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useNormalMap,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useNormalMap,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.GetFloat(ShaderIDs.useNormalMap) == 1)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, normalMap, normalScale);
            m_MaterialEditor.TextureScaleOffsetProperty(normalMap);
            EditorGUI.indentLevel--;
        }
    }
    #endregion

    #region GUI_MatCapSettings
    /// <summary>
    /// GUI_MatCapSettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_MatCapSettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("MatCap Active");
        if(material.GetFloat(ShaderIDs.useMatCap) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useMatCap,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useMatCap,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.GetFloat(ShaderIDs.useMatCap) == 1)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.TexturePropertySingleLine(Styles.matMapText, matCapMap, matCapColor);
            m_MaterialEditor.TextureScaleOffsetProperty(matCapMap);
            EditorGUI.indentLevel--;
        }
    }
    #endregion

    #region GUI_EmissionSettings
    /// <summary>
    /// GUI_EmissionSettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_EmissionSettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Emission Active");
        if(material.GetFloat(ShaderIDs.useEmission) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useEmission,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useEmission,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.GetFloat(ShaderIDs.useEmission) == 1)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.TexturePropertySingleLine(Styles.emissionMapText, emissionMap, emissionColor);
            m_MaterialEditor.TextureScaleOffsetProperty(emissionMap);   
            EditorGUI.indentLevel--;
        }
    }
    #endregion

    #region GUI_GISettings
    /// <summary>
    /// GUI_GISettings
    /// </summary>
    /// <param name="material"></param>
    void GUI_GISettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("GlobalIllumation Active");
        if(material.GetFloat(ShaderIDs.useGI) == 0){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useGI,1);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(ShaderIDs.useGI,0);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.GetFloat(ShaderIDs.useGI) == 1)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("PRT Active");
            if(material.GetFloat(ShaderIDs.usePRT) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.usePRT,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.usePRT,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("RSM Active");
            if(material.GetFloat(ShaderIDs.useRSM) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useRSM,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useRSM,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("SSDO Active");
            if(material.GetFloat(ShaderIDs.useSSDO) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useSSDO,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useSSDO,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("SSR Active");
            if(material.GetFloat(ShaderIDs.useSSR) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useSSR,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useSSR,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("LPV Active");
            if(material.GetFloat(ShaderIDs.useLPV) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useLPV,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useLPV,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Cone Trace Active");
            if(material.GetFloat(ShaderIDs.useVoxel) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useVoxel,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useVoxel,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("DDGI Active");
            if(material.GetFloat(ShaderIDs.useDDGI) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useDDGI,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.useDDGI,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Path Tracing Active");
            if(material.GetFloat(ShaderIDs.usePathTracing) == 0){
                if (GUILayout.Button("Off",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.usePathTracing,1);
                }
            }else{
                if (GUILayout.Button("Active",shortButtonStyle))
                {
                    material.SetFloat(ShaderIDs.usePathTracing,0);
                }
            }
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            m_MaterialEditor.RangeProperty(gi_Intensity, "GI Intensity");
            
            EditorGUI.indentLevel--;
        }
    }
    #endregion
}

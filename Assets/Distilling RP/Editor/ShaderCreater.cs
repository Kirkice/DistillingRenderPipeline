using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ShaderCreater : MonoBehaviour
{
    [MenuItem("Assets/Create/Shader/Distilling RP-Shader", false, 2)]
    static void CreateURPShader()
    {
        UnityEngine.Object[] arr=Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
        string shaderSavePath = AssetDatabase.GetAssetPath(arr[0]) + "/NewShader.shader";
        string shaderSourcePath = "Assets/Distilling RP/Shaders/Base.shader";
        CreateShader(shaderSourcePath, shaderSavePath);
        AssetDatabase.Refresh();
    }
    
    static public void CreateShader(string sourcePath, string savePath)
    {
        if (sourcePath != null && savePath !=null)
        {
            File.Copy(sourcePath, savePath);
        }
    }
}

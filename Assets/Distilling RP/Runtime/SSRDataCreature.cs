using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using UnityEditor;


namespace UnityEngine.Rendering.Distilling
{
    public class SSRDataCreature
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Rendering/Distilling Render Pipeline/Screen Space Ray Tracing Data", priority = 201)]
        static void MenuCreateSSRData()
        {
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateSSRData>(), "New ScreenSpaceRayTracingData.asset", icon, null);
        }
        
        internal static SSRData CreateScreenSpaceRayTracingAtPath(string path)
        {
            var profile = ScriptableObject.CreateInstance<SSRData>();
            profile.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }
        class DoCreateSSRData : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                SSRData profile = SSRDataCreature.CreateScreenSpaceRayTracingAtPath(pathName);
                ProjectWindowUtil.ShowCreatedAsset(profile);
            }
        }
#endif
    }
}

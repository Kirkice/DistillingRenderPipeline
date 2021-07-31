using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using UnityEditor;

namespace UnityEngine.Rendering.Distilling
{
    public class VoxelGIDataCreature
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Rendering/Distilling Render Pipeline/Voxel GI Data", priority = 201)]
        static void MenuCreateSSRData()
        {
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateVoxelGIData>(), "New VoxelGIData.asset", icon, null);
        }
        
        internal static VoxelGIData CreateVoxelGIAtPath(string path)
        {
            var profile = ScriptableObject.CreateInstance<VoxelGIData>();
            profile.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }
        class DoCreateVoxelGIData : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                VoxelGIData profile = VoxelGIDataCreature.CreateVoxelGIAtPath(pathName);
                ProjectWindowUtil.ShowCreatedAsset(profile);
            }
        }
#endif
    }   
}

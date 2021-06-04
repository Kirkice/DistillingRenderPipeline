using System.IO;
using UnityEngine;
using UnityEditorInternal;

namespace UnityEditor.Rendering.Distilling
{
    internal class DistillinglProjectSettings : ScriptableObject
    {
        public static string filePath => "ProjectSettings/URPProjectSettings.asset";

        //preparing to eventual migration later
        enum Version
        {
            None,
            First
        }

        [SerializeField]
        int m_LastMaterialVersion = k_NeverProcessedMaterialVersion;

        internal const int k_NeverProcessedMaterialVersion = -1;

        public static int materialVersionForUpgrade
        {
            get => instance.m_LastMaterialVersion;
            set
            {
                instance.m_LastMaterialVersion = value;
            }
        }

        //singleton pattern
        static DistillinglProjectSettings s_Instance;
        static DistillinglProjectSettings instance => s_Instance ?? CreateOrLoad();
        DistillinglProjectSettings()
        {
            s_Instance = this;
        }

        static DistillinglProjectSettings CreateOrLoad()
        {
            //try load
            InternalEditorUtility.LoadSerializedFileAndForget(filePath);

            //else create
            if (s_Instance == null)
            {
                DistillinglProjectSettings created = CreateInstance<DistillinglProjectSettings>();
                created.hideFlags = HideFlags.HideAndDontSave;
            }

            System.Diagnostics.Debug.Assert(s_Instance != null);
            return s_Instance;
        }

        internal static void Save()
        {
            if (s_Instance == null)
            {
                Debug.Log("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            string folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { s_Instance }, filePath, allowTextSerialization: true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    public class LocalizationConfigObject : ScriptableObject
    {
        public const string dataDirectory = "Assets/UIDocumentLocalization";

        const string k_ConfigObjectName = "com.dartworks.uidocumentlocalization";
        const string k_ConfigObjectPath = dataDirectory + "/ConfigObject.asset";

        static LocalizationConfigObject s_Instance;

        [SerializeField] LocalizationSettings m_Settings;

        public static LocalizationConfigObject instance
        {
            get
            {
                if (s_Instance == null)
                {
#if UNITY_EDITOR
                    s_Instance = GetOrCreateConfigObject();
#else
                    Debug.LogWarning("Config Object does not exist. Temporary one will be created for this session.");
                    s_Instance = CreateInstance<ConfigObject>();
#endif
                }

                return s_Instance;
            }
        }

        public static LocalizationSettings settings
        {
            get
            {
                var settings = instance.m_Settings;
#if !UNITY_EDITOR
                if (settings == null)
                {
                    Debug.LogWarning("Settings asset does not exist. Temporary one will be created for this session.");
                    settings = CreateInstance<LocalizationSettings>();
                    instance.m_Settings = settings;
                }
#endif
                return settings;
            }
            set
            {
                instance.m_Settings = value;
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (s_Instance == null)
            {
                s_Instance = GetOrCreateConfigObject();
            }
        }

        static LocalizationConfigObject GetOrCreateConfigObject()
        {
            LocalizationConfigObject configObject;
            if (!EditorBuildSettings.TryGetConfigObject(k_ConfigObjectName, out configObject))
            {
                if (!Directory.Exists(dataDirectory))
                {
                    Directory.CreateDirectory(dataDirectory);
                }

                configObject = CreateInstance<LocalizationConfigObject>();
                AssetDatabase.CreateAsset(configObject, k_ConfigObjectPath);
                AssetDatabase.SaveAssets();
                EditorBuildSettings.AddConfigObject(k_ConfigObjectName, configObject, true);
            }

            return configObject;
        }
#endif

        void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }
    }
}

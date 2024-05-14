using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static event SettingsChangedCallback onSettingsChanged;

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

        public LocalizationSettings settings
        {
            get
            {
#if !UNITY_EDITOR
                if (m_Settings == null)
                {
                    Debug.LogWarning("Settings asset does not exist. Temporary one will be created for this session.");
                    m_Settings = CreateInstance<LocalizationSettings>();
                }
#endif
                return m_Settings;
            }
            set
            {
                var previousSettings = m_Settings;
                m_Settings = value;
                onSettingsChanged?.Invoke(previousSettings, m_Settings);
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

        public delegate void SettingsChangedCallback(LocalizationSettings previousSettings, LocalizationSettings newSettings);

        void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }
    }
}

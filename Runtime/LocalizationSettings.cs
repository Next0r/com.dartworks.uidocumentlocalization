using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    public class LocalizationSettings : ScriptableObject
    {
        public const string dataDirectory = "Assets/UIDocumentLocalization";

        const string k_ConfigObjectName = "com.dartworks.uidocumentlocalization";

        static LocalizationSettings s_Instance;

        [SerializeField] List<string> m_Locales = new List<string>();
        [SerializeField] string m_SelectedLocale = null;
        [SerializeField] LocalizationData m_Database;

        public static LocalizationSettings instance
        {
            get
            {
                if (s_Instance == null)
                {
#if UNITY_EDITOR
                    if (!EditorBuildSettings.TryGetConfigObject(k_ConfigObjectName, out s_Instance))
                    {
                        if (!Directory.Exists(dataDirectory))
                        {
                            Directory.CreateDirectory(dataDirectory);
                        }

                        s_Instance = CreateInstance<LocalizationSettings>();
                        AssetDatabase.CreateAsset(s_Instance, dataDirectory + "/LocalizationSettings.asset");
                        AssetDatabase.SaveAssets();
                        EditorBuildSettings.AddConfigObject(k_ConfigObjectName, s_Instance, true);
                    }
#else
                    s_Instance = FindObjectOfType<LocalizationSettings>();
                    if (s_Instance == null)
                    {
                        Debug.LogWarning("Failed to load localization settings, will use default.");
                        s_Instance = CreateInstance<LocalizationSettings>();
                    }
#endif
                }

                return s_Instance;
            }
        }

        public static LocalizationData database
        {
            get => instance?.m_Database;
            set
            {
                if (value == null)
                {
                    instance.m_Database = null;
                    return;
                }

#if UNITY_EDITOR
                var path = AssetDatabase.GetAssetPath(value);
                if (path == null)
                {
                    Debug.LogError("Unable to retrieve database path. Check whether it has been saved into asset database.");
                    return;
                }
#endif

                instance.m_Database = value;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Test/Create Settings Asset")]
        static void CreateSettingsAsset()
        {
            // Simply fire getter to generate settings asset.
            var settings = instance;
        }
#endif

        /// <summary>
        /// OnEnable gets called when scriptable object is loaded by the player as preloaded asset. Preloaded asset is
        /// actually the same scriptable object which is stored as editor build config file while in editor, and is passed
        /// to player by UIDocumentLocalizationPlayerBuild class methods.
        /// </summary>
        void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public List<string> locales
        {
            get => m_Locales;
            set => m_Locales = value;
        }

        public string selectedLocale
        {
            get => m_SelectedLocale;
            set => m_SelectedLocale = value;
        }

        public int selectedLocaleIndex
        {
            get => locales.IndexOf(selectedLocale);
            set
            {
                if (value >= 0 && value < locales.Count)
                {
                    m_SelectedLocale = locales[value];
                }
            }
        }
    }
}

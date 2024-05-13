using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    [CreateAssetMenu(menuName = "UIDocument Localization/Settings", fileName = "Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        public const string dataDirectory = "Assets/UIDocumentLocalization";

        const string k_ConfigObjectName = "com.dartworks.uidocumentlocalization";

        static LocalizationSettings s_Instance;

        [SerializeField] List<string> m_Locales = new List<string>();
        [SerializeField] string m_SelectedLocale = null;
        [SerializeField] LocalizationData m_Database;

        public static LocalizationData database
        {
            get => LocalizationConfigObject.settings?.m_Database;
            set
            {
                var settings = LocalizationConfigObject.settings;
                if (settings == null)
                {
                    return;
                }

                if (value == null)
                {
                    settings.m_Database = null;
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

                settings.m_Database = value;
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

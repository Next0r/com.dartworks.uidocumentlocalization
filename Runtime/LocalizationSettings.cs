using System;
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

        public event DatabaseChangedCallback onDatabaseChanged;
        public event LocaleChangedCallback onLocaleChanged;

        [SerializeField] List<string> m_Locales = new List<string>();
        [SerializeField] string m_SelectedLocale = null;
        [SerializeField] LocalizationData m_Database;

        public LocalizationData database
        {
            get => m_Database;
            set
            {
                var previousDatabase = m_Database;
                if (value == null)
                {
                    m_Database = null;
                    onDatabaseChanged?.Invoke(previousDatabase, m_Database);
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

                m_Database = value;
                onDatabaseChanged?.Invoke(previousDatabase, m_Database);
            }
        }

        public delegate void DatabaseChangedCallback(LocalizationData previousDatabase, LocalizationData newDatabase);

        public delegate void LocaleChangedCallback(string previousLocale, string newLocale);

        public List<string> locales
        {
            get => m_Locales;
            set => m_Locales = value;
        }

        public string selectedLocale
        {
            get => m_SelectedLocale;
            set
            {
                var previousLocale = m_SelectedLocale;
                m_SelectedLocale = value;
                onLocaleChanged?.Invoke(previousLocale, m_SelectedLocale);
            }
        }

        public int selectedLocaleIndex
        {
            get => locales.IndexOf(selectedLocale);
            set
            {
                if (value >= 0 && value < locales.Count)
                {
                    var previousLocale = m_SelectedLocale;
                    m_SelectedLocale = locales[value];
                    onLocaleChanged?.Invoke(previousLocale, m_SelectedLocale);
                }
            }
        }
    }
}

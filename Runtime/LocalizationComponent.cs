using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    [ExecuteAlways]
    public class LocalizationComponent : MonoBehaviour
    {
        UIDocument m_UIDocument;
        bool m_PreviousIsDirty;
        IPanel m_Panel;
        LocalizationAsyncOperation m_AsyncOperation;

        public UIDocument uiDocument
        {
            get => m_UIDocument;
            set
            {
                m_UIDocument = value;
                m_Panel = m_UIDocument?.rootVisualElement.panel;
            }
        }

        public LocalizationAsyncOperation asyncOperation
        {
            get => m_AsyncOperation;
        }

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);   // Serialize component settings in edit mode.
#endif
        }

#if !UNITY_EDITOR
        /// <summary>
        /// Actual update will be used only in build. By default we will use editor application update
        /// because of necessity of panel dirty pooling.
        /// </summary>
        void Update()
        {
            UpdatePanelIsDirty();
        }
#endif

        void OnEnable()
        {
            m_Panel = uiDocument?.rootVisualElement.panel;

#if UNITY_EDITOR
            EditorApplication.update += UpdatePanelIsDirty;
            LocalizationConfigObject.onSettingsChanged += OnSettingsChanged;
            OnSettingsChanged(null, LocalizationConfigObject.instance.settings);
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= UpdatePanelIsDirty;
            LocalizationConfigObject.onSettingsChanged -= OnSettingsChanged;
            var settings = LocalizationConfigObject.instance.settings;
            if (settings != null)
            {
                settings.onDatabaseChanged -= OnDatabaseChanged;
                settings.onLocaleChanged -= OnLocaleChanged;
                var database = settings.database;
                if (database != null)
                {
                    database.onUpdated -= LocalizeDocument;
                }
            }
#endif
        }

        void OnSettingsChanged(LocalizationSettings previousSettings, LocalizationSettings newSettings)
        {
            if (previousSettings?.database != null)
            {
                previousSettings.onDatabaseChanged -= OnDatabaseChanged;
                previousSettings.onLocaleChanged -= OnLocaleChanged;
            }

            if (newSettings?.database != null)
            {
                newSettings.onDatabaseChanged += OnDatabaseChanged;
                newSettings.onLocaleChanged += OnLocaleChanged;
                OnDatabaseChanged(previousSettings?.database, newSettings?.database);
            }
        }

        void OnDatabaseChanged(LocalizationData previousDatabase, LocalizationData newDatabase)
        {
            if (previousDatabase != null)
            {
                previousDatabase.onUpdated -= LocalizeDocument;
            }

            if (newDatabase != null)
            {
                newDatabase.onUpdated += LocalizeDocument;
                LocalizeDocument();
            }
        }

        void OnLocaleChanged(string previousLocale, string newLocale)
        {
            LocalizeDocument();
        }

        void UpdatePanelIsDirty()
        {
            if (m_Panel == null)
            {
                return;
            }

            bool isDirty = m_Panel.isDirty;
            if (m_PreviousIsDirty && !isDirty)
            {
                LocalizeDocument();
            }

            m_PreviousIsDirty = isDirty;
        }

        void LocalizeDocument()
        {
            if (m_AsyncOperation != null)
            {
                m_AsyncOperation.Cancel();
            }

            m_AsyncOperation = LocalizationUtility.LocalizeSubTreeAsync(uiDocument.rootVisualElement);
        }
    }
}

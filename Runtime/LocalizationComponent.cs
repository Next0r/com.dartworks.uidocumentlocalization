using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class LocalizationComponent : MonoBehaviour
    {
        [SerializeField] UIDocument m_UIDocument;

        LocalizationAsyncOperation m_AsyncOperation;
        int m_PreviousDescendantCount;

        public UIDocument uiDocument
        {
            get => m_UIDocument;
            set => m_UIDocument = value;
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
        /// because of necessity of descendant count pooling.
        /// </summary>
        void Update()
        {
            UpdateDescendantCount();
        }
#endif

#if UNITY_EDITOR
        void OnEnable()
        {
            EditorApplication.update += UpdateDescendantCount;
            LocalizationConfigObject.instance.onSettingsChanged += OnSettingsChanged;

            // Fake event invocation to attach all required callbacks and perform initial localization.
            OnSettingsChanged(null, LocalizationConfigObject.instance.settings);
        }

        void OnDisable()
        {
            EditorApplication.update -= UpdateDescendantCount;
            LocalizationConfigObject.instance.onSettingsChanged -= OnSettingsChanged;
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
        }

        [MenuItem("CONTEXT/UIDocument/Add Localization Component", true)]
        static bool ValidateAddLocalizationComponent(MenuCommand command)
        {
            UIDocument uiDocument = (UIDocument)command.context;
            return uiDocument.GetComponent<LocalizationComponent>() == null;
        }

        [MenuItem("CONTEXT/UIDocument/Add Localization Component")]
        static void AddLocalizationComponent(MenuCommand command)
        {
            UIDocument uiDocument = (UIDocument)command.context;
            Undo.AddComponent<LocalizationComponent>(uiDocument.gameObject);
        }
#endif

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

        void UpdateDescendantCount()
        {
            if (uiDocument?.rootVisualElement == null)
            {
                return;
            }

            int descendantCount = uiDocument.rootVisualElement.GetDescendantCount();
            if (descendantCount != m_PreviousDescendantCount)
            {
                LocalizeDocument();
            }

            m_PreviousDescendantCount = descendantCount;
        }

        void LocalizeDocument()
        {
            if (uiDocument?.rootVisualElement == null)
            {
                return;
            }

            if (m_AsyncOperation != null)
            {
                m_AsyncOperation.Cancel();
            }

            m_AsyncOperation = LocalizationUtility.LocalizeSubTreeAsync(uiDocument.rootVisualElement);
        }
    }
}

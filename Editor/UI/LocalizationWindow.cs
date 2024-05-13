using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class LocalizationWindow : EditorWindow
    {
        const string k_UssClassName = "localization-window";
        const string k_SelectionContainerUssClassName = k_UssClassName + "__selection-container";
        const string k_HintBoxUssClassName = k_UssClassName + "__hint-box";
        const string k_StyleSheetPath = "Packages/com.dartworks.uidocumentlocalization/Editor/UI/StyleSheets/LocalizationWindowStyle.uss";

        VisualElement m_SelectionContainer;
        List<VisualElement> m_Selection;
        HintBox m_HintBox;
        StyleSheet m_StyleSheet;

        public static LocalizationWindow activeWindow
        {
            get
            {
                var windows = Resources.FindObjectsOfTypeAll<LocalizationWindow>();
                if (windows.Length > 0)
                {
                    return windows.First();
                }

                return null;
            }
        }

        public HintBox hintBox
        {
            get => m_HintBox;
        }

        [MenuItem("Window/UIDocument Localization/Localization Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationWindow>();
            window.titleContent = new GUIContent("Localization");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddToClassList(k_UssClassName);
            if (m_StyleSheet == null)
            {
                m_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);
            }
            
            root.styleSheets.Add(m_StyleSheet);

            m_SelectionContainer = new ScrollView();
            m_SelectionContainer.AddToClassList(k_SelectionContainerUssClassName);
            root.Add(m_SelectionContainer);

            m_HintBox = new HintBox();
            root.Add(m_HintBox);
        }

        public void Clear()
        {
            if (m_SelectionContainer != null)
            {
                m_SelectionContainer.Clear();
            }
        }

        public void AddSelectedElement(LocalizationWindowSelectedElement selectedElement)
        {
            if (m_SelectionContainer == null)
            {
                return;
            }

            m_SelectionContainer.Add(selectedElement);
        }
    }
}

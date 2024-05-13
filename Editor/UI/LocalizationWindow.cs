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
        public const string ussClassName = "localization-window";
        public const string selectionContainerUssClassName = ussClassName + "__selection-container";
        public const string hintBoxUssClassName = ussClassName + "__hint-box";

        VisualElement m_SelectionContainer;
        List<VisualElement> m_Selection;
        HintBox m_HintBox;

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

        [MenuItem("Test/Localization Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationWindow>();
            window.titleContent = new GUIContent("Localization");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddToClassList(ussClassName);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.dartworks.uidocumentlocalization/Editor/UI/StyleSheets/LocalizationWindowStyle.uss");
            root.styleSheets.Add(styleSheet);

            m_SelectionContainer = new ScrollView();
            m_SelectionContainer.AddToClassList(selectionContainerUssClassName);
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

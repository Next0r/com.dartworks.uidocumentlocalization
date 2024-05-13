using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    [CustomEditor(typeof(LocalizationTable))]
    class LocalizationTableEditor : Editor
    {
        const string k_UssClassName = "localization-table";
        const string k_EntriesFieldUssClassName = k_UssClassName + "__entries-field";
        const string k_StyleSheetPath = "Packages/com.dartworks.uidocumentlocalization/Editor/UI/StyleSheets/LocalizationTableStyle.uss";

        ObjectField m_TextAssetField;
        StyleSheet m_StyleSheet;

        LocalizationTable table => target as LocalizationTable;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddToClassList(k_UssClassName);
            if (m_StyleSheet == null)
            {
                m_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);
            }

            root.styleSheets.Add(m_StyleSheet);
            m_TextAssetField = new ObjectField()
            {
                label = "TextAsset",
                objectType = typeof(TextAsset),
            };
            m_TextAssetField.AddToClassList("unity-base-field__aligned");
            m_TextAssetField.BindProperty(serializedObject.FindProperty("m_TextAsset"));
            m_TextAssetField.RegisterValueChangedCallback(OnTextAssetChanged);
            root.Add(m_TextAssetField);

            var contentHashPropertyField = new PropertyField(serializedObject.FindProperty("m_ContentHash"));
            contentHashPropertyField.SetEnabled(false);
            root.Add(contentHashPropertyField);

            var entriesPropertyField = new PropertyField(serializedObject.FindProperty("m_Entries"));
            entriesPropertyField.AddToClassList(k_EntriesFieldUssClassName);
            entriesPropertyField.SetEnabled(false);
            root.Add(entriesPropertyField);

            return root;
        }

        void OnTextAssetChanged(ChangeEvent<Object> evt)
        {
            if (table == null)
            {
                return;
            }

            TextAsset textAsset = (TextAsset)evt.newValue;
            string path = AssetDatabase.GetAssetPath(textAsset);
            if (string.IsNullOrEmpty(path))
            {
                m_TextAssetField.SetValueWithoutNotify(null);
                return;
            }

            string extension = Path.GetExtension(path);
            if (extension != ".csv")
            {
                Debug.LogWarning("Table text asset can only be a .csv file.");
                m_TextAssetField.SetValueWithoutNotify(null);
                return;
            }

            table.Rebuild();
        }
    }
}

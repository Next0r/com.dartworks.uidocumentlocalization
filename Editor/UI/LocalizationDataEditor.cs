using System.Collections;
using System.Collections.Generic;
using UIDocumentLocalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LocalizationData))]
class LocalizationDataEditor : Editor
{

    const string k_UssClassName = "localization-data-editor";
    const string k_EntriesFieldUssClassName = k_UssClassName + "__entries-field";
    const long k_RebuildButtonUpdateIntervalMs = 100L;
    const string k_StyleSheetPath = "Packages/com.dartworks.uidocumentlocalization/Editor/UI/StyleSheets/LocalizationDataStyle.uss";

    Button m_RebuildButton;
    StyleSheet m_StyleSheet;

    LocalizationData localizationData => target as LocalizationData;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        root.AddToClassList(k_UssClassName);
        if (m_StyleSheet == null)
        {
            m_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);
            root.styleSheets.Add(m_StyleSheet);
        }

        m_RebuildButton = new Button() { text = "Rebuild" };
        m_RebuildButton.schedule.Execute(HandleRebuildButtonEnabled).Every(k_RebuildButtonUpdateIntervalMs);
        m_RebuildButton.clicked += OnRebuildButtonClicked;
        root.Add(m_RebuildButton);

        var entriesPropertyField = new PropertyField(serializedObject.FindProperty("m_Entries"));
        entriesPropertyField.AddToClassList(k_EntriesFieldUssClassName);
        entriesPropertyField.SetEnabled(false);
        root.Add(entriesPropertyField);

        return root;
    }

    void HandleRebuildButtonEnabled()
    {
        m_RebuildButton.SetEnabled(BuilderDocumentManager.builderWindowOpened);
        m_RebuildButton.tooltip = BuilderDocumentManager.builderWindowOpened
            ? "Synchronizes database with currently open document and it's dependencies."
            : "Available only with active UI Builder document.";
    }

    void OnRebuildButtonClicked()
    {
        BuilderDocumentManager.instance.UpdateDatabase();
    }
}

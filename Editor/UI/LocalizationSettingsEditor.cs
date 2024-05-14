using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    [CustomEditor(typeof(LocalizationSettings))]
    class LocalizationSettingsEditor : Editor
    {
        const string k_UssClassName = "localization-settings";
        const string k_ListViewUssClassName = k_UssClassName + "__list-view";
        const string k_ListItemUssClassName = k_UssClassName + "__list-item";
        const string k_ListItemTextFieldUssClassName = k_UssClassName + "__list-item-text-field";
        const string k_StyleSheetPath = "Packages/com.dartworks.uidocumentlocalization/Editor/UI/StyleSheets/LocalizationSettingsStyle.uss";

        const float k_ListItemHeight = 20f;

        public class ListItem
        {
            VisualElement m_Root;
            TextField m_TextField;
            Toggle m_Toggle;

            public object userData
            {
                get => m_Root.userData;
                set => m_Root.userData = value;
            }

            public string textFieldValue
            {
                get => m_TextField.value;
                set => m_TextField.SetValueWithoutNotify(value);
            }

            public bool toggleValue
            {
                get => m_Toggle.value;
                set => m_Toggle.SetValueWithoutNotify(value);
            }

            public Toggle toggle => m_Toggle;
            public TextField textField => m_TextField;
            public VisualElement root => m_Root;

            public ListItem()
            {
                m_Root = new VisualElement();
                m_Root.AddToClassList(k_ListItemUssClassName);

                m_TextField = new TextField();
                m_TextField.AddToClassList(k_ListItemTextFieldUssClassName);
                m_Root.Add(m_TextField);

                m_Toggle = new Toggle();
                m_Root.Add(m_Toggle);
            }

            public ListItem(VisualElement root)
            {
                m_Root = root;
                m_TextField = m_Root.Q<TextField>();
                m_Toggle = m_Root.Q<Toggle>();
            }
        }

        StyleSheet m_StyleSheet;
        List<VisualElement> m_ListViewElements = new List<VisualElement>();

        public LocalizationSettings settings => target as LocalizationSettings;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddToClassList(k_UssClassName);
            if (m_StyleSheet == null)
            {
                m_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);
            }

            root.styleSheets.Add(m_StyleSheet);

            var listView = new ListView(settings.locales, k_ListItemHeight, MakeItem, BindItem);
            listView.AddToClassList(k_ListViewUssClassName);
            listView.headerTitle = "Locales";
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.showFoldoutHeader = true;
            listView.showAddRemoveFooter = true;
            listView.showBoundCollectionSize = true;
            listView.showBorder = true;
            root.Add(listView);

            var databasePropertyField = new PropertyField(serializedObject.FindProperty("m_Database"));
            root.Add(databasePropertyField);

            return root;
        }

        /// <summary>
        /// Called when order of elements in list view changes.
        /// </summary>
        void BindItem(VisualElement ve, int index)
        {
            var listItem = new ListItem(ve);

            // User data null check allows to verify whether we are performing bind operation for the first time.
            if (listItem.userData == null && settings.selectedLocaleIndex == index)
            {
                listItem.toggleValue = true;
            }

            listItem.userData = index;
            listItem.textFieldValue = settings.locales[index];
        }

        /// <summary>
        /// Called when element is added to list view.
        /// </summary>
        VisualElement MakeItem()
        {
            var listItem = new ListItem();
            listItem.textField.RegisterValueChangedCallback(evt =>
            {
                var idx = (int)listItem.userData;
                var selectedLocaleIndex = settings.selectedLocaleIndex; // Cache before locales update.
                settings.locales[idx] = evt.newValue;
                if (idx == selectedLocaleIndex)
                {
                    settings.selectedLocale = evt.newValue;
                }

                EditorUtility.SetDirty(settings);
            });

            listItem.toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == true)
                {
                    foreach (var ve in m_ListViewElements)
                    {
                        var li = new ListItem(ve);
                        li.toggleValue = false;
                    }

                    listItem.toggleValue = true;
                }
                else
                {
                    listItem.toggleValue = true;    // Checkbox should work like a radio button.
                }

                settings.selectedLocale = listItem.textFieldValue;
                EditorUtility.SetDirty(settings);
            });

            m_ListViewElements.Add(listItem.root);
            return listItem.root;
        }
    }
}

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
    class LocalizationWindowSelectedElement : VisualElement
    {
        const string k_UssClassName = "selected-element";

        Label m_OverrideLabel;
        TextField m_NameTextField;
        Foldout m_LocalizedPropertiesFoldout;
        List<LocalizedPropertyElement> m_LocalizedPropertyElements;

        public string selectedElementName
        {
            get => m_NameTextField.value;
            set => m_NameTextField.SetValueWithoutNotify(value);
        }

        public bool overrideLabelDisplayed
        {
            get => m_OverrideLabel.style.display == DisplayStyle.Flex;
            set => m_OverrideLabel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public LocalizationWindowSelectedElement()
        {
            m_LocalizedPropertyElements = new List<LocalizedPropertyElement>();

            AddToClassList(k_UssClassName);

            m_OverrideLabel = new Label { text = "Override" };
            m_OverrideLabel.style.display = DisplayStyle.None;
            Add(m_OverrideLabel);

            m_NameTextField = new TextField { label = "Name" };
            m_NameTextField.SetEnabled(false);
            Add(m_NameTextField);

            m_LocalizedPropertiesFoldout = new Foldout() { text = "Localized Properties" };
            m_LocalizedPropertiesFoldout.value = true;  // Unfolded
            Add(m_LocalizedPropertiesFoldout);
        }

        public void GenerateLocalizedPropertyElements(int elementsCount)
        {
            for (int i = 0; i < elementsCount; i++)
            {
                var localizedPropertyElement = new LocalizedPropertyElement();
                m_LocalizedPropertiesFoldout.Add(localizedPropertyElement);
                m_LocalizedPropertyElements.Add(localizedPropertyElement);
            }
        }

        public LocalizedPropertyElement GetLocalizedPropertyElement(int index)
        {
            if (index >= 0 && index < m_LocalizedPropertyElements.Count)
            {
                return m_LocalizedPropertyElements[index];
            }

            return null;
        }
    }
}

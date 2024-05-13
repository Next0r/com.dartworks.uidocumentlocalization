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
        public const string ussClassName = "selected-element";
        public const string hintBoxUssClassName = ussClassName + "__hint-box";

        Label m_OverrideLabel;
        TextField m_NameTextField;
        LocalizationAddressElement m_AddressElement;
        Foldout m_BaseAddressFoldout;
        LocalizationAddressElement m_BaseAddressElement;
        // Foldout m_OverridesFoldout;

        public string selectedElementName
        {
            get => m_NameTextField.value;
            set => m_NameTextField.SetValueWithoutNotify(value);
        }

        public LocalizationWindowSelectedElement()
        {
            AddToClassList(ussClassName);

            m_OverrideLabel = new Label { text = "Override" };
            m_OverrideLabel.style.display = DisplayStyle.None;
            Add(m_OverrideLabel);

            m_NameTextField = new TextField { label = "Name" };
            m_NameTextField.SetEnabled(false);
            Add(m_NameTextField);

            m_AddressElement = new LocalizationAddressElement();
            Add(m_AddressElement);

            m_BaseAddressFoldout = new Foldout() { text = "Base Address" };
            m_BaseAddressFoldout.value = false; // Collapsed
            Add(m_BaseAddressFoldout);

            m_BaseAddressElement = new LocalizationAddressElement();
            m_BaseAddressElement.displayBorder = true;
            m_BaseAddressElement.displayVisualTreeAsset = true;
            m_BaseAddressFoldout.Add(m_BaseAddressElement);

            // m_OverridesFoldout = new Foldout() { text = "Override Addresses" };
            // m_OverridesFoldout.value = false;
            // Add(m_OverridesFoldout);
        }

        public void BindProperty(SerializedProperty entrySp, int overrideIndex = -1)
        {
            bool isOverride = overrideIndex != -1;
            m_OverrideLabel.style.display = isOverride ? DisplayStyle.Flex : DisplayStyle.None;
            m_BaseAddressFoldout.style.display = isOverride ? DisplayStyle.Flex : DisplayStyle.None;

            if (isOverride)
            {
                var overrideSp = entrySp.FindPropertyRelative($"m_Overrides.Array.data[{overrideIndex}]");
                m_AddressElement.BindProperty(overrideSp);
                m_BaseAddressElement.BindProperty(entrySp);
            }
            else
            {
                m_AddressElement.BindProperty(entrySp);
            }

            // var overridesSp = entrySp.FindPropertyRelative("m_Overrides");
            // for (int i = 0; i < overridesSp.arraySize; i++)
            // {
            //     if (i != overrideIndex)
            //     {
            //         var element = new LocalizationAddressElement();
            //         element.BindProperty(overridesSp.FindPropertyRelative($"Array.data[{i}]"));
            //         element.displayBorder = true;
            //         element.displayVisualTreeAsset = true;
            //         m_OverridesFoldout.Add(element);
            //     }
            // }

            // m_OverridesFoldout.style.display = m_OverridesFoldout.childCount == 0
            //     ? DisplayStyle.None
            //     : DisplayStyle.Flex;
        }
    }
}

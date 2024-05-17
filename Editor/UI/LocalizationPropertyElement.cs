using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class LocalizedPropertyElement : VisualElement
    {
        const string k_UssClassName = "localized-property";
        const string k_BaseAddressContainer = k_UssClassName + "__base-address-container";

        TextField m_PropertyTextField;
        LocalizationAddressElement m_AddressElement;
        Foldout m_BaseAddressElementFoldout;
        ObjectField m_BaseVisualTreeAssetObjectField;
        LocalizationAddressElement m_BaseAddressElement;

        public TextField propertyTextField
        {
            get => m_PropertyTextField;
        }

        public LocalizationAddressElement addressElement
        {
            get => m_AddressElement;
        }

        public LocalizationAddressElement baseAddressElement
        {
            get => m_BaseAddressElement;
        }

        public ObjectField baseVisualTreeAssetObjectField
        {
            get => m_BaseVisualTreeAssetObjectField;
        }

        public bool baseAddressFoldoutDisplayed
        {
            get => m_BaseAddressElementFoldout.style.display == DisplayStyle.Flex;
            set => m_BaseAddressElementFoldout.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public LocalizedPropertyElement()
        {
            AddToClassList(k_UssClassName);

            m_PropertyTextField = new TextField() { label = "Property" };
            m_PropertyTextField.SetEnabled(false);
            Add(m_PropertyTextField);

            m_AddressElement = new LocalizationAddressElement();
            Add(m_AddressElement);

            m_BaseAddressElementFoldout = new Foldout()
            {
                text = "Base Address",
                value = false,  // folded by default
            };
            Add(m_BaseAddressElementFoldout);

            var baseAddressContainer = new VisualElement();
            baseAddressContainer.AddToClassList(k_BaseAddressContainer);
            m_BaseAddressElementFoldout.Add(baseAddressContainer);

            m_BaseVisualTreeAssetObjectField = new ObjectField()
            {
                label = "Document",
                objectType = typeof(VisualTreeAsset),
            };
            m_BaseVisualTreeAssetObjectField.SetEnabled(false);
            baseAddressContainer.Add(m_BaseVisualTreeAssetObjectField);

            m_BaseAddressElement = new LocalizationAddressElement();
            baseAddressContainer.Add(m_BaseAddressElement);
        }
    }
}

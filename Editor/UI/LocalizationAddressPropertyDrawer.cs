using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    [CustomPropertyDrawer(typeof(LocalizationAddress))]
    class LocalizationAddressPropertyDrawer : PropertyDrawer
    {
        const float k_LabelWidth = 36f;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new TextField()
            {
                label = property.displayName,
                name = "root"
            };
            root.AddToClassList("unity-base-field__aligned");

            var container = root[1];
            container.Clear();
            container.name = "container";
            container.ClearClassList();
            container.style.flexGrow = 1f;

            var tableObjectField = new ObjectField() { label = "Table", objectType = typeof(LocalizationTable) };
            tableObjectField.BindProperty(property.FindPropertyRelative("m_Table"));
            tableObjectField.style.marginLeft = 0f;
            tableObjectField.style.marginRight = 0f;
            tableObjectField.labelElement.style.minWidth = k_LabelWidth;
            container.Add(tableObjectField);

            var keyTextField = new TextField() { label = "Key" };
            keyTextField.BindProperty(property.FindPropertyRelative("m_Key"));
            keyTextField.style.marginLeft = 0f;
            keyTextField.style.marginRight = 0f;
            keyTextField.labelElement.style.minWidth = k_LabelWidth;
            container.Add(keyTextField);

            return root;
        }
    }
}

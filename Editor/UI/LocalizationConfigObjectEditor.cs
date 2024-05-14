using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.UI
{
    [CustomEditor(typeof(LocalizationConfigObject))]
    class LocalizationConfigObjectEditor : Editor
    {
        LocalizationConfigObject configObject => target as LocalizationConfigObject;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var settingsPropertyField = new PropertyField(serializedObject.FindProperty("m_Settings"));
            settingsPropertyField.RegisterValueChangeCallback(OnSettingsPropertyChanged);
            root.Add(settingsPropertyField);

            return root;
        }

        /// <summary>
        /// We want to invoke property changed event for config object, so in addition to setting serialized
        /// property let's also use actual setter method.
        /// </summary>
        void OnSettingsPropertyChanged(SerializedPropertyChangeEvent evt)
        {
            LocalizationConfigObject.instance.settings = (LocalizationSettings)evt.changedProperty.objectReferenceValue;
        }
    }
}

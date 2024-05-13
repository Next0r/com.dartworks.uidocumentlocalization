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
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var settingsPropertyField = new PropertyField(serializedObject.FindProperty("m_Settings"));
            root.Add(settingsPropertyField);

            return root;
        }
    }
}

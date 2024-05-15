using System.Collections;
using System.Collections.Generic;
using UIDocumentLocalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LocalizationComponent))]
public class LocalizationComponentEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        return root;
    }
}

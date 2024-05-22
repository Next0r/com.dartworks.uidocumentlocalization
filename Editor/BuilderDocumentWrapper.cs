using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class BuilderDocumentWrapper
    {
        public static Type type = BuilderWrapper.assembly.GetType("Unity.UI.Builder.BuilderDocument");

        static PropertyInfo s_HasUnsavedChangesProperty = type.GetProperty("hasUnsavedChanges");
        static PropertyInfo s_VisualTreeAssetProperty = type.GetProperty("visualTreeAsset");
        static PropertyInfo s_ActiveOpenUXMLFile = type.GetProperty("activeOpenUXMLFile");

        object m_Obj;

        public object obj => m_Obj;

        public BuilderDocumentOpenUXMLWrapper activeOpenUXMLFile
        {
            get => new BuilderDocumentOpenUXMLWrapper(s_ActiveOpenUXMLFile.GetValue(m_Obj));
        }

        public bool hasUnsavedChanges
        {
            get => (bool)s_HasUnsavedChangesProperty.GetValue(m_Obj);
            set => s_HasUnsavedChangesProperty.SetValue(m_Obj, value);
        }

        public VisualTreeAsset visualTreeAsset
        {
            get => (VisualTreeAsset)s_VisualTreeAssetProperty.GetValue(m_Obj);
        }

        public BuilderDocumentWrapper(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (obj.GetType() != type)
            {
                throw new ArgumentException();
            }

            m_Obj = obj;
        }
    }
}
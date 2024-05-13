using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UIDocumentLocalization.Wrappers;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class BuilderWrapper
    {
        public static Assembly assembly = Assembly.Load("UnityEditor");
        public static Type type = assembly.GetType("Unity.UI.Builder.Builder");

        static MethodInfo s_SaveChangesMethod = type.GetMethod("SaveChanges");
        static MethodInfo s_LoadDocumentMethod = type.GetMethod("LoadDocument", BindingFlags.Instance | BindingFlags.Public);

        static PropertyInfo s_ActiveWindowProperty = type.GetProperty("ActiveWindow");
        static PropertyInfo s_DocumentProperty = type.GetProperty("document");
        static PropertyInfo s_DocumentRootElementProperty = type.GetProperty("documentRootElement");
        static PropertyInfo s_SelectionProperty = type.GetProperty("selection");
        static PropertyInfo s_ViewportProperty = type.GetProperty("viewport");
        static PropertyInfo s_HierarchyProperty = type.GetProperty("hierarchy");

        object m_Obj;

        public static BuilderWrapper activeWindow
        {
            get
            {
                var activeWindow = s_ActiveWindowProperty.GetValue(null);
                if (activeWindow != null)
                {
                    return new BuilderWrapper(activeWindow);
                }

                return null;
            }
        }

        public object obj => m_Obj;
        public BuilderDocumentWrapper document => new BuilderDocumentWrapper(s_DocumentProperty.GetValue(m_Obj));
        public VisualElement documentRootElement => (VisualElement)s_DocumentRootElementProperty.GetValue(m_Obj);
        public BuilderSelectionWrapper selection => new BuilderSelectionWrapper(s_SelectionProperty.GetValue(m_Obj));
        public BuilderViewportWrapper viewport => new BuilderViewportWrapper(s_ViewportProperty.GetValue(m_Obj));
        public object hierarchy => s_HierarchyProperty.GetValue(m_Obj);

        BuilderWrapper(object obj)
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

        public void SaveChanges()
        {
            s_SaveChangesMethod.Invoke(m_Obj, new object[] { });
        }

        public void LoadDocument(VisualTreeAsset vta)
        {
            s_LoadDocumentMethod.Invoke(m_Obj, new object[] { vta, true });
        }
    }
}
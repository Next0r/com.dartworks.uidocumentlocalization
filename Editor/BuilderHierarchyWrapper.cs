using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UIDocumentLocalization.Wrappers
{
    class BuilderHierarchyWrapper
    {
        public static Type type = BuilderWrapper.assembly.GetType("Unity.UI.Builder.BuilderHierarchy");
        static MethodInfo s_UpdateHierarchyAndSelectionMethod = type.GetMethod("UpdateHierarchyAndSelection", BindingFlags.Instance | BindingFlags.Public);

        object m_Obj;

        public object obj => m_Obj;

        public BuilderHierarchyWrapper(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (!type.IsAssignableFrom(obj.GetType()))
            {
                throw new ArgumentException();
            }

            m_Obj = obj;
        }

        public void UpdateHierarchyAndSelection(bool hasUnsavedChanges)
        {
            s_UpdateHierarchyAndSelectionMethod.Invoke(m_Obj, new object[] { hasUnsavedChanges });
        }
    }
}

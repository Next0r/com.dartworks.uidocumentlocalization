using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UIDocumentLocalization.Wrappers
{
    class BuilderDocumentOpenUXMLWrapper
    {
        public static Type type = BuilderWrapper.assembly.GetType("Unity.UI.Builder.BuilderDocumentOpenUXML");
        static MethodInfo s_SaveUnsavedChanges = type.GetMethod("SaveUnsavedChanges", BindingFlags.Instance | BindingFlags.Public);

        object m_Obj;

        public object obj => m_Obj;

        public BuilderDocumentOpenUXMLWrapper(object obj)
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

        public bool SaveUnsavedChanges(string manualUxmlPath = null, bool isSaveAs = false)
        {
            return (bool)s_SaveUnsavedChanges.Invoke(m_Obj, new object[] { manualUxmlPath, isSaveAs });
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UIDocumentLocalization.Wrappers;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class BuilderViewportWrapper
    {
        public static Type type = BuilderWrapper.assembly.GetType("Unity.UI.Builder.BuilderViewport");

        static MethodInfo s_SetInnerSelectionMethod = type.GetMethod("SetInnerSelection", BindingFlags.Instance | BindingFlags.NonPublic);

        object m_Obj;

        public object obj => m_Obj;

        public BuilderViewportWrapper(object obj)
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

        public void SetInnerSelection(VisualElement ve)
        {
            s_SetInnerSelectionMethod.Invoke(m_Obj, new object[] { ve });
        }
    }
}

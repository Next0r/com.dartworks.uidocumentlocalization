using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class BuilderSelectionWrapper
    {
        public static Type type = BuilderWrapper.assembly.GetType("Unity.UI.Builder.BuilderSelection");
        static PropertyInfo s_SelectionProperty = type.GetProperty("selection");
        static MethodInfo s_SelectMethod = type.GetMethod("Select");
        static MethodInfo s_AddToSelectionMethod = type.GetMethod("AddToSelection", BindingFlags.Instance | BindingFlags.Public);

        object m_Obj;

        public object obj => m_Obj;

        public List<VisualElement> selection
        {
            get
            {
                IList sel = (IList)s_SelectionProperty.GetValue(m_Obj);
                return sel.Cast<VisualElement>().ToList();
            }
        }

        public BuilderSelectionWrapper(object obj)
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

        public void Select(BuilderViewportWrapper source, VisualElement ve)
        {
            s_SelectMethod.Invoke(m_Obj, new object[] { source.obj, ve });
        }

        public void AddToSelection(object source, VisualElement ve)
        {
            s_AddToSelectionMethod.Invoke(m_Obj, new object[] { source, ve });
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class StylePropertyWrapper
    {
        public static Type type = typeof(VisualTreeAsset).Assembly.GetType("UnityEngine.UIElements.StyleProperty");

        static PropertyInfo s_ValuesProperty = type.GetProperty("values");
        static PropertyInfo s_NameProperty = type.GetProperty("name");

        object m_Obj;
        List<StyleValueHandleWrapper> m_Values;
        string m_Name;

        public object obj => m_Obj;
        public List<StyleValueHandleWrapper> values => new List<StyleValueHandleWrapper>(m_Values);
        public string name => m_Name;

        public StylePropertyWrapper(object obj)
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
            m_Values = new List<StyleValueHandleWrapper>();
            foreach (var element in (Array)s_ValuesProperty.GetValue(obj))
            {
                m_Values.Add(new StyleValueHandleWrapper(element));
            }

            m_Name = (string)s_NameProperty.GetValue(m_Obj);
        }
    }
}

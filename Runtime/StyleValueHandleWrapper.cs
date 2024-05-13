using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class StyleValueHandleWrapper
    {
        static Type s_Type = typeof(VisualTreeAsset).Assembly.GetType("UnityEngine.UIElements.StyleValueHandle");

        static PropertyInfo s_ValueTypePropInfo = s_Type.GetProperty("valueType");
        static FieldInfo s_ValueIndexFieldInfo = s_Type.GetField("valueIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        object m_Obj;
        StyleValueType m_ValueType;
        int m_ValueIndex;

        public object obj => m_Obj;
        public StyleValueType valueType => m_ValueType;
        public int valueIndex => m_ValueIndex;

        public StyleValueHandleWrapper(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (obj.GetType() != s_Type)
            {
                throw new ArgumentException();
            }

            m_Obj = obj;
            m_ValueType = (StyleValueType)((int)s_ValueTypePropInfo.GetValue(m_Obj));
            m_ValueIndex = (int)s_ValueIndexFieldInfo.GetValue(m_Obj);
        }
    }
}

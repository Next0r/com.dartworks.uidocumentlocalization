using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UIDocumentLocalization.Wrappers
{
    class AttributeOverrideWrapper
    {
        public static Type type = TemplateAssetWrapper.type.GetNestedType("AttributeOverride");

        static FieldInfo s_ElementNameField = type.GetField("m_ElementName", BindingFlags.Instance | BindingFlags.Public);
        static FieldInfo s_AttributeNameField = type.GetField("m_AttributeName", BindingFlags.Instance | BindingFlags.Public);
        static FieldInfo s_ValueField = type.GetField("m_Value", BindingFlags.Instance | BindingFlags.Public);

        object m_Obj;

        public object obj => m_Obj;

        public string elementName => s_ElementNameField.GetValue(m_Obj) as string;
        public string attributeName => s_AttributeNameField.GetValue(m_Obj) as string;
        public string value => s_ValueField.GetValue(m_Obj) as string;

        public AttributeOverrideWrapper(object obj)
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
    }
}

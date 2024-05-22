using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class VisualElementAssetWrapper
    {
        public static Type type = typeof(VisualTreeAsset).Assembly.GetType("UnityEngine.UIElements.VisualElementAsset");

        static PropertyInfo s_FullTypeNameProperty = type.GetProperty("fullTypeName");
        static PropertyInfo s_ParentIdProperty = type.GetProperty("parentId");
        static PropertyInfo s_IdProperty = type.GetProperty("id");
        static MethodInfo s_GetAttributeValueMethod = type.GetMethod("GetAttributeValue");
        static PropertyInfo s_RuleIndex = type.GetProperty("ruleIndex");
        static MethodInfo s_SetAttributeMethod = type.GetMethod("SetAttribute");

        protected object m_Obj;
        string m_FullTypeName;
        int m_ParentId;
        int m_Id;

        public object obj => m_Obj;
        public string fullTypeName => (string)s_FullTypeNameProperty.GetValue(m_Obj);
        public int parentId => (int)s_ParentIdProperty.GetValue(m_Obj);
        public int id => (int)s_IdProperty.GetValue(m_Obj);
        
        public int ruleIndex
        {
            get => (int)s_RuleIndex.GetValue(obj);
            set => s_RuleIndex.SetValue(obj, value);
        }

        public string typeName
        {
            get
            {
                string[] splittedType = m_FullTypeName.Split('.');
                if (splittedType.Length > 0)
                {
                    return splittedType[splittedType.Length - 1];
                }

                return string.Empty;
            }
        }

        public VisualElementAssetWrapper(object obj)
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

        public string GetAttributeValue(string attributeName)
        {
            return (string)s_GetAttributeValueMethod.Invoke(m_Obj, new object[] { attributeName });
        }

        public void SetAttribute(string name, string value)
        {
            s_SetAttributeMethod.Invoke(m_Obj, new object[] { name, value });
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class StyleRuleWrapper
    {
        public static Type type = typeof(VisualTreeAsset).Assembly.GetType("UnityEngine.UIElements.StyleRule");

        static PropertyInfo s_PropertiesProperty = type.GetProperty("properties");

        object m_Obj;

        public object obj => m_Obj;

        public List<StylePropertyWrapper> properties
        {
            get
            {
                var properties = new List<StylePropertyWrapper>();
                if (s_PropertiesProperty.GetValue(obj) is Array propertiesArray)
                {
                    foreach (var property in propertiesArray)
                    {
                        properties.Add(new StylePropertyWrapper(property));
                    }
                }

                return properties;
            }
        }

        public StyleRuleWrapper(object obj)
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
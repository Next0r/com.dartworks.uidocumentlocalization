using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class TemplateAssetWrapper : VisualElementAssetWrapper
    {
        public static new Type type = typeof(VisualTreeAsset).Assembly.GetType("UnityEngine.UIElements.TemplateAsset");

        static PropertyInfo s_AttributeOverridesProperty = type.GetProperty("attributeOverrides", BindingFlags.Instance | BindingFlags.Public);

        public List<AttributeOverrideWrapper> attributeOverrides
        {
            get
            {
                var wrappers = new List<AttributeOverrideWrapper>();
                var attributeOverrideObjs = s_AttributeOverridesProperty.GetValue(m_Obj) as IList;
                foreach (var attributeOverrideObj in attributeOverrideObjs)
                {
                    wrappers.Add(new AttributeOverrideWrapper(attributeOverrideObj));
                }

                return wrappers;
            }
        }

        public TemplateAssetWrapper(object visualElementAsset) : base(visualElementAsset) { }

        public AttributeOverrideWrapper GetAttributeOverride(string attributeName)
        {
            foreach (var attributeOverride in attributeOverrides)
            {
                if (attributeOverride.attributeName == attributeName)
                {
                    return attributeOverride;
                }
            }

            return null;
        }
    }
}

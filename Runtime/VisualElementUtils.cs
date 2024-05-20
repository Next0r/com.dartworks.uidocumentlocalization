using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UIDocumentLocalization.Wrappers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    static class VisualElementUtils
    {

#if UNITY_EDITOR

        static Assembly s_Assembly = Assembly.Load("UnityEditor");
        static Type s_VeeType = s_Assembly.GetType("Unity.UI.Builder.VisualElementExtensions");
        static MethodInfo s_GetVisualElementAssetMethod = s_VeeType.GetMethod("GetVisualElementAsset", BindingFlags.Static | BindingFlags.Public);
        static MethodInfo s_GetVisualTreeAssetMethod = s_VeeType.GetMethod("GetVisualTreeAsset", BindingFlags.Static | BindingFlags.Public);
        static MethodInfo s_GetPropertyMethod = typeof(VisualElement).GetMethod("GetProperty", BindingFlags.Instance | BindingFlags.NonPublic);
        // static MethodInfo s_GetAttributeDescriptionsMethod = s_VeeType.GetMethod("GetAttributeDescriptions", BindingFlags.Static | BindingFlags.Public);

#endif

        public class StringProperty
        {
            public string name { get; }
            public string value { get; }

            public StringProperty(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public override string ToString()
            {
                return $"[{name}: {value}]";
            }
        }

        public static List<StringProperty> GetStringStyleProperties(this VisualElement ve)
        {
            var stringProperties = new List<StringProperty>();
            var styleAccess = new InlineStyleAccessWrapper(ve.style);
            var properties = styleAccess.inlineRule.styleRule?.properties;
            if (properties == null)
            {
                return stringProperties;
            }

            foreach (var property in properties)
            {
                var values = property.values;
                if (values.Count <= 0)
                {
                    continue;
                }

                if (values[0].valueType == StyleValueType.String)
                {
                    stringProperties.Add(new StringProperty(property.name, styleAccess.inlineRule.styleSheet.ReadString(values[0])));
                }
            }

            return stringProperties;
        }

        public static string GetStringStylePropertyByName(this VisualElement ve, string name)
        {
            var properties = ve.GetStringStyleProperties();
            foreach (var property in properties)
            {
                if (property.name == name)
                {
                    return property.value;
                }
            }

            return null;
        }

#if UNITY_EDITOR

        /// <summary>
        /// This method will read original attribute value from visual element asset, template override or
        /// custom UxmlTraits class and apply it to visual element via reflection. Note that attributes are
        /// stored as strings, so we are relying on System.Convert.ChangeType to handle parsing.
        /// Attribute name should be the same as in UXML file and follow HTML naming convention to match 
        /// visual element's property, e.g. 'user-defined-attribute' -> 'userDefinedAttribute'. Matching attribute
        /// against property is non-case-sensitive, so property can be defined as 'MyProperty' or 'myProperty'.
        /// Generally that's the way Unity matches attributes and control's properties in builder.
        /// </summary>
        public static void ResetAttribute(this VisualElement ve, string attributeName)
        {
            var propertyName = attributeName.Replace("-", string.Empty);

            // Element is defined in active document.
            var vea = ve.GetVisualElementAsset();
            if (vea != null)
            {
                string attributeValue = vea.GetAttributeValue(attributeName);
                if (attributeValue != null)
                {
                    ve.TrySetReflectionProperty(propertyName, attributeValue, caseSensitive: false);
                    return;
                }
            }

            // Element is defined in other document (template).
            if (ve.visualTreeAssetSource != null)
            {
                // First get attribute definition for this element.
                vea = ve.GetLinkedVisualElementAssetInTemplate();
                string attributeValue = vea.GetAttributeValue(attributeName);

                // Now let's look for override. Visual element has vta source, so must have template ancestor.
                var templateAncestor = ve.GetFirstAncestorOfType<TemplateContainer>();

                // Template ancestor source vta will be null if defined in active document.
                var ta = templateAncestor.visualTreeAssetSource != null
                    ? templateAncestor.GetLinkedVisualElementAssetInTemplate() as TemplateAssetWrapper
                    : templateAncestor.GetVisualElementAsset() as TemplateAssetWrapper;

                var newAttributeOverride = ta.GetAttributeOverride(attributeName);
                if (newAttributeOverride != null)
                {
                    attributeValue = newAttributeOverride.value;
                }

                if (attributeValue != null)
                {
                    ve.TrySetReflectionProperty(propertyName, attributeValue, caseSensitive: false);
                    return;
                }
            }

            // We are dealing with custom control child, attribute descriptions are defined in UxmlTraits class which
            // is probably hidden by custom implementation (if not, will use default VisualElement.UxmlTraits).
            Type uxmlTraitsType = ve.GetType().GetNestedType("UxmlTraits", BindingFlags.NonPublic | BindingFlags.Public);
            if (uxmlTraitsType != null)
            {
                var uxmlTraits = Activator.CreateInstance(uxmlTraitsType);
                var fieldInfos = uxmlTraitsType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType.IsSubclassOf(typeof(UxmlAttributeDescription)))
                    {
                        var attributeDescription = (UxmlAttributeDescription)fieldInfo.GetValue(uxmlTraits);
                        if (attributeDescription.name == attributeName)
                        {
                            ve.TrySetReflectionProperty(propertyName, attributeDescription.defaultValueAsString, caseSensitive: false);
                            return;
                        }
                    }
                }
            }
        }

        static bool TrySetReflectionProperty(this VisualElement ve, string propertyName, object value, bool caseSensitive = false)
        {
            PropertyInfo propertyInfo = null;
            if (caseSensitive)
            {
                propertyInfo = ve.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
            else
            {
                var propertyInfos = ve.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var p in propertyInfos)
                {
                    if (p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyInfo = p;
                        break;
                    }
                }
            }

            if (propertyInfo != null)
            {
                try
                {
                    propertyInfo.SetValue(ve, Convert.ChangeType(value, propertyInfo.PropertyType));
                    return true;
                }
                catch (Exception) { }
            }

            return false;
        }

        public static object GetProperty(this VisualElement ve, PropertyName key)
        {
            return s_GetPropertyMethod.Invoke(ve, new object[] { key });
        }

        /// <summary>
        /// This method works similar to GetVisualElementAsset, but it allows to "look inside" template
        /// which is not currently active inside builder window. Generally if you want to get visual element
        /// asset of grayed out template child then use this method, otherwise relay on GetVisualElementAsset.
        /// Note that this method will still return null in case of custom control's children.
        /// </summary>
        public static VisualElementAssetWrapper GetLinkedVisualElementAssetInTemplate(this VisualElement ve)
        {
            var visualElementAsset = ve.GetProperty(BuilderConstants.ElementLinkedVisualElementAssetInTemplatePropertyName);
            if (visualElementAsset != null)
            {
                return visualElementAsset.GetType() == TemplateAssetWrapper.type
                    ? new TemplateAssetWrapper(visualElementAsset)
                    : new VisualElementAssetWrapper(visualElementAsset);
            }

            return null;
        }

        public static VisualElementAssetWrapper GetVisualElementAsset(this VisualElement ve)
        {
            var visualElementAsset = s_GetVisualElementAssetMethod.Invoke(null, new object[] { ve });
            if (visualElementAsset != null)
            {
                return visualElementAsset.GetType() == TemplateAssetWrapper.type
                    ? new TemplateAssetWrapper(visualElementAsset)
                    : new VisualElementAssetWrapper(visualElementAsset);
            }

            return null;
        }

        public static bool IsDefinedInTreeAsset(this VisualElement ve, VisualTreeAsset vta)
        {
            var visualElementAssets = vta.GetVisualElementAssets();
            visualElementAssets.AddRange(vta.GetTemplateAssets());
            var vea = ve.GetVisualElementAsset();
            foreach (var asset in visualElementAssets)
            {
                // Id check for some reason is more reliable than reference comparison.
                if (vea != null && asset.id == vea.id)
                {
                    return true;
                }
            }

            return false;
        }

        public static VisualElement GetAncestorDefinedInTreeAsset(this VisualElement ve, VisualTreeAsset vta)
        {
            var ancestor = ve.hierarchy.parent;
            while (ancestor != null)
            {
                if (ancestor.IsDefinedInTreeAsset(vta))
                {
                    return ancestor;
                }

                ancestor = ancestor.hierarchy.parent;
            }

            return null;
        }

        public static List<string> GetDescendantGuids(this VisualElement ve)
        {
            var guids = new List<string>();
            var descendants = ve.Query<VisualElement>().ToList();
            descendants.Remove(ve);
            foreach (var descendant in descendants)
            {
                string guid = descendant.GetStringStylePropertyByName("guid");
                if (!string.IsNullOrEmpty(guid))
                {
                    guids.Add(guid);
                }
            }

            guids.Sort();
            return guids;
        }
#endif

        public static void ApplyTranslations(this VisualElement ve, TranslationInfo translationInfo)
        {
            foreach (var info in translationInfo)
            {
                if (!string.IsNullOrEmpty(info.translation))
                {
                    var propertyInfo = ve.GetType().GetProperty(info.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    propertyInfo.SetValue(ve, info.translation);
                }
            }
        }

        public static List<VisualElement> GetLocalizableDescendants(this VisualElement ve)
        {
            var elements = new List<VisualElement>();
            GetLocalizableDescendantsRecursive(ve, elements);
            return elements;
        }

        static void GetLocalizableDescendantsRecursive(VisualElement ve, List<VisualElement> elements)
        {
            foreach (VisualElement child in ve.hierarchy.Children())
            {
                if (child is TextElement)
                {
                    elements.Add(child);
                }
                else
                {
                    var propertyInfos = child.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var propertyInfo in propertyInfos)
                    {
                        if (typeof(string).IsAssignableFrom(propertyInfo.PropertyType) &&
                            Attribute.IsDefined(propertyInfo, typeof(LocalizeProperty)))
                        {
                            elements.Add(child);
                            break;
                        }
                    }
                }

                GetLocalizableDescendantsRecursive(child, elements);
            }
        }

        public static bool TryGetGuid(this VisualElement ve, out string guid, out VisualElement ancestor)
        {
            guid = ve.GetStringStylePropertyByName("guid");
            ancestor = null;

            if (!string.IsNullOrEmpty(guid))
            {
                return true;
            }
            else
            {
                // Presumably we are dealing with custom control child, let's find it's ancestor with guid assigned
                var currentAncestor = ve.hierarchy.parent;
                while (currentAncestor != null)
                {
                    guid = currentAncestor.GetStringStylePropertyByName("guid");
                    if (!string.IsNullOrEmpty(guid))
                    {
                        ancestor = currentAncestor;
                        return true;
                    }

                    currentAncestor = currentAncestor.hierarchy.parent;
                }

                guid = null;
                return false;
            }
        }

        public static int GetDescendantCount(this VisualElement ve)
        {
            int count = 0;
            GetDescendantElementsRecursive(ve, ref count);
            return count;
        }

        static void GetDescendantElementsRecursive(VisualElement ve, ref int count)
        {
            for (int i = 0; i < ve.hierarchy.childCount; i++)
            {
                count++;
                GetDescendantElementsRecursive(ve.hierarchy[i], ref count);
            }
        }
    }
}

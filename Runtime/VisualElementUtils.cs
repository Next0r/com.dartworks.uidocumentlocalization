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

        public static void ApplyTranslations(this VisualElement ve, TranslationInfo translationInfo)
        {
            var elementPropertiesInfo = PropertyTracker.instance.GetOrCreateElementPropertiesInfo(ve);

            // Save use defined values as default ones for tracked properties.
            elementPropertiesInfo.UpdateTrackedPropertiesDefaultValues();

            foreach (var info in translationInfo)
            {
                elementPropertiesInfo.TryGetTrackedProperty(info.propertyName, out var trackedProperty);
                if (!string.IsNullOrEmpty(info.translation))
                {
                    // Set property values and save them as localized (applied by localization system).
                    trackedProperty.Localize(info.translation);
                }
                else
                {
                    // Restore default value.
                    trackedProperty.Restore();
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
            // We could use GetLocalizedProperties extension method here, but as reflection is already
            // quite slow it should be faster to just check type or first occurrence of LocalizeProperty
            // attribute rather than getting all of them. 
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

        /// <summary>
        /// This method is optimization of GetDescendants() extension method, it's faster
        /// and allocates less memory. 
        /// </summary>
        public static int GetDescendantCount(this VisualElement ve)
        {
            int count = 0;
            GetDescendantCountRecursive(ve, ref count);
            return count;
        }

        static void GetDescendantCountRecursive(VisualElement ve, ref int count)
        {
            for (int i = 0; i < ve.hierarchy.childCount; i++)
            {
                count++;
                GetDescendantCountRecursive(ve.hierarchy[i], ref count);
            }
        }

        /// <summary>
        /// This method is optimization of VisualELement.Query() extension method, it's faster
        /// and does not include root in output list.
        /// </summary>
        public static List<VisualElement> GetDescendants(this VisualElement ve)
        {
            var descendants = new List<VisualElement>();
            GetDescendantsRecursive(ve, descendants);
            return descendants;
        }

        static void GetDescendantsRecursive(VisualElement ve, List<VisualElement> elements)
        {
            for (int i = 0; i < ve.hierarchy.childCount; i++)
            {
                elements.Add(ve.hierarchy[i]);
                GetDescendantsRecursive(ve.hierarchy[i], elements);
            }
        }

        public static List<PropertyInfo> GetLocalizedProperties(this VisualElement ve)
        {
            var localizedProperties = new List<PropertyInfo>();
            if (ve is TextElement)
            {
                localizedProperties.Add(typeof(TextElement).GetProperty("text"));
            }

            foreach (var propertyInfo in ve.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (typeof(string).IsAssignableFrom(propertyInfo.PropertyType) &&
                    Attribute.IsDefined(propertyInfo, typeof(LocalizeProperty)))
                {
                    localizedProperties.Add(propertyInfo);
                }
            }

            return localizedProperties;
        }
    }
}

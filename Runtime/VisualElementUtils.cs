using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

        /// <summary>
        /// This method ensures that text elements are fetched out of visual element in the same order
        /// unless hierarchy gets changed.
        /// </summary>
        public static List<TextElement> GetDescendantTextElements(this VisualElement ve)
        {
            var textElements = new List<TextElement>();
            GetDescendantTextElementsRecursive(ve, textElements);
            return textElements;
        }

        static void GetDescendantTextElementsRecursive(VisualElement ve, List<TextElement> textElements)
        {
            foreach (VisualElement child in ve.hierarchy.Children())
            {
                if (child is TextElement textElement)
                {
                    textElements.Add(textElement);
                }

                GetDescendantTextElementsRecursive(child, textElements);
            }
        }

#if UNITY_EDITOR

        public static VisualElementAssetWrapper GetVisualElementAsset(this VisualElement ve)
        {
            var visualElementAsset = s_GetVisualElementAssetMethod.Invoke(null, new object[] { ve });
            if (visualElementAsset != null)
            {
                return new VisualElementAssetWrapper(visualElementAsset);
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

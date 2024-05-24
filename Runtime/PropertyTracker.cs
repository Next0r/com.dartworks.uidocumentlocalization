using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class PropertyTracker
    {
        static PropertyTracker s_Instance;

        public class ElementPropertiesInfo
        {
            VisualElement m_VisualElement;
            List<TrackedProperty> m_TrackedProperties;

            public VisualElement visualElement
            {
                get => m_VisualElement;
            }

            public List<TrackedProperty> trackedProperties
            {
                get => m_TrackedProperties;
            }

            public ElementPropertiesInfo(VisualElement visualElement)
            {
                if (visualElement == null)
                {
                    throw new ArgumentNullException();
                }

                m_VisualElement = visualElement;
                m_TrackedProperties = new List<TrackedProperty>();
                foreach (var propertyInfo in m_VisualElement.GetLocalizedProperties())
                {
                    m_TrackedProperties.Add(new TrackedProperty(m_VisualElement, propertyInfo));
                }
            }

            public void UpdateTrackedPropertiesDefaultValues()
            {
                foreach (var trackedProperty in m_TrackedProperties)
                {
                    // If current property value for visual element is different from recently
                    // stored localized value, it means that user has changed property value
                    // and we have to save it as default.
                    if (trackedProperty.currentValue != trackedProperty.localizedValue)
                    {
                        trackedProperty.UpdateDefault();
                    }
                }
            }

            public bool TryGetTrackedProperty(string propertyName, out TrackedProperty trackedProperty)
            {
                foreach (var tp in m_TrackedProperties)
                {
                    if (tp.propertyInfo.Name == propertyName)
                    {
                        trackedProperty = tp;
                        return true;
                    }
                }

                trackedProperty = null;
                return false;
            }
        }

        public class TrackedProperty
        {
            VisualElement m_VisualElement;
            PropertyInfo m_PropertyInfo;
            string m_DefaultValue;
            string m_LocalizedValue;

            public PropertyInfo propertyInfo
            {
                get => m_PropertyInfo;
            }

            public string defaultValue
            {
                get => m_DefaultValue;
            }

            public string localizedValue
            {
                get => m_LocalizedValue;
            }

            public string currentValue
            {
                get => (string)m_PropertyInfo.GetValue(m_VisualElement);
                private set => m_PropertyInfo.SetValue(m_VisualElement, value);
            }

            public TrackedProperty(VisualElement ve, PropertyInfo propertyInfo)
            {
                m_VisualElement = ve;
                m_PropertyInfo = propertyInfo;
            }

            public void Localize(string translation)
            {
                currentValue = translation;
                m_LocalizedValue = translation;
            }

            public void Restore()
            {
                currentValue = m_DefaultValue;
            }

            public void UpdateDefault()
            {
                m_DefaultValue = currentValue;
            }
        }

        List<ElementPropertiesInfo> m_ElementPropertiesInfos;

        public static PropertyTracker instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new PropertyTracker();
                }

                return s_Instance;
            }
        }

        PropertyTracker()
        {
            m_ElementPropertiesInfos = new List<ElementPropertiesInfo>();
        }

        public bool TryGetTrackedProperty(VisualElement visualElement, string propertyName, out TrackedProperty trackedProperty)
        {
            if (GetOrCreateElementPropertiesInfo(visualElement).TryGetTrackedProperty(propertyName, out trackedProperty))
            {
                return true;
            }

            trackedProperty = null;
            return false;
        }

        public void RemoveElementPropertiesInfo(VisualElement ve)
        {
            for (int i = 0; i < m_ElementPropertiesInfos.Count; i++)
            {
                if (m_ElementPropertiesInfos[i].visualElement == ve)
                {
                    m_ElementPropertiesInfos.RemoveAt(i);
                    break;
                }
            }
        }

        public ElementPropertiesInfo GetOrCreateElementPropertiesInfo(VisualElement ve)
        {
            foreach (var propertiesInfo in m_ElementPropertiesInfos)
            {
                if (propertiesInfo.visualElement == ve)
                {
                    return propertiesInfo;
                }
            }

            var info = new ElementPropertiesInfo(ve);
            m_ElementPropertiesInfos.Add(info);
            return info;
        }
    }
}

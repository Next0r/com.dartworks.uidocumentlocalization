using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    [CreateAssetMenu(menuName = "UIDocument Localization/Database", fileName = "Database")]
    public class LocalizationData : ScriptableObject, ISerializationCallbackReceiver
    {
        public static EntriesComparer entriesComparer = new EntriesComparer();

        [Serializable]
        public class Override
        {
            [SerializeField] string m_OverridingElementGuid;
            [SerializeField] VisualTreeAsset m_OverridingElementVisualTreeAsset;
            [SerializeField] List<LocalizedProperty> m_LocalizedProperties;
            [NonSerialized] Entry m_Entry;

            public string overridingElementGuid
            {
                get => m_OverridingElementGuid;
                set => m_OverridingElementGuid = value;
            }

            public VisualTreeAsset overridingElementVisualTreeAsset
            {
                get => m_OverridingElementVisualTreeAsset;
                set => m_OverridingElementVisualTreeAsset = value;
            }

            public List<LocalizedProperty> localizedProperties
            {
                get => m_LocalizedProperties;
                set => m_LocalizedProperties = value;
            }

            public Entry entry
            {
                get => m_Entry;
                set => m_Entry = value;
            }

            public bool isEmpty
            {
                get
                {
                    foreach (var localizedProperty in m_LocalizedProperties)
                    {
                        if (!localizedProperty.address.isEmpty)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            public Override()
            {
                m_LocalizedProperties = new List<LocalizedProperty>();
            }

            public void Remove()
            {
                if (m_Entry == null)
                {
                    Debug.LogWarning("Unable to remove override, localized property reference not set. First call 'RebuildNonSerializedReferences' on database object.");
                    return;
                }

                m_Entry.overrides.Remove(this);
            }

            public bool TryGetLocalizedProperty(string name, out LocalizedProperty localizedProperty)
            {
                foreach (var lp in m_LocalizedProperties)
                {
                    if (lp.name == name)
                    {
                        localizedProperty = lp;
                        return true;
                    }
                }

                localizedProperty = null;
                return false;
            }
        }

        [Serializable]
        public class LocalizedProperty
        {
            [SerializeField] string m_Name;
            [SerializeField] LocalizationAddress m_Address;

            public string name
            {
                get => m_Name;
                set => m_Name = value;
            }

            public LocalizationAddress address
            {
                get => m_Address;
                set => m_Address = value;
            }
        }

        [Serializable]
        public class Entry
        {
            [SerializeField] string m_FullTypeName;
            [SerializeField] string m_Guid;
            [SerializeField] string m_Name;
            [SerializeField] VisualTreeAsset m_VisualTreeAsset;
            [SerializeField] List<LocalizedProperty> m_LocalizedProperties;
            [SerializeField] List<Override> m_Overrides;

            public string fullTypeName
            {
                get => m_FullTypeName;
                set => m_FullTypeName = value;
            }

            public string guid
            {
                get => m_Guid;
                set => m_Guid = value;
            }

            public string name
            {
                get => m_Name;
                set => m_Name = value;
            }

            public VisualTreeAsset visualTreeAsset
            {
                get => m_VisualTreeAsset;
                set => m_VisualTreeAsset = value;
            }

            public List<LocalizedProperty> localizedProperties
            {
                get => m_LocalizedProperties;
                set => m_LocalizedProperties = value;
            }

            public List<Override> overrides
            {
                get => m_Overrides;
                set => m_Overrides = value;
            }

            public Entry()
            {
                m_Name = string.Empty;
                m_LocalizedProperties = new List<LocalizedProperty>();
                m_Overrides = new List<Override>();
            }

            public bool TryGetOverride(string guid, out Override ovr)
            {
                foreach (var o in m_Overrides)
                {
                    if (o.overridingElementGuid == guid)
                    {
                        ovr = o;
                        return true;
                    }
                }

                ovr = null;
                return false;
            }

            public void AddOrReplaceOverride(Override newOverride)
            {
                if (TryGetOverride(newOverride.overridingElementGuid, out var currentOverride))
                {
                    currentOverride.overridingElementVisualTreeAsset = newOverride.overridingElementVisualTreeAsset;
                    currentOverride.localizedProperties = newOverride.localizedProperties;
                    currentOverride.entry = newOverride.entry;
                }
                else
                {
                    m_Overrides.Add(newOverride);
                    newOverride.entry = this;
                }
            }

            public void MergeLocalizedProperties(List<LocalizedProperty> newLocalizedProperties)
            {
                // Remove excessive
                for (int i = 0; i < m_LocalizedProperties.Count; i++)
                {
                    if (GetLocalizedProperty(newLocalizedProperties, m_LocalizedProperties[i].name) == null)
                    {
                        m_LocalizedProperties.RemoveAt(i);
                        i--;
                    }
                }

                // Add missing
                foreach (var newLocalizedProperty in newLocalizedProperties)
                {
                    var localizedProperty = GetLocalizedProperty(m_LocalizedProperties, newLocalizedProperty.name);
                    if (localizedProperty == null)
                    {
                        m_LocalizedProperties.Add(localizedProperty);
                    }
                }
            }

            static LocalizedProperty GetLocalizedProperty(List<LocalizedProperty> localizedProperties, string name)
            {
                if (!localizedProperties.Any())
                {
                    return null;
                }

                foreach (var localizedProperty in localizedProperties)
                {
                    if (localizedProperty != null && localizedProperty.name == name)
                    {
                        return localizedProperty;
                    }
                }

                return null;
            }
        }

        public class EntriesComparer : IComparer<Entry>
        {
            public int Compare(Entry x, Entry y)
            {
                int result = x.guid.CompareTo(y.guid);
                if (result == 0)
                {
                    return x.name.CompareTo(y.name);
                }

                return result;
            }
        }

        public event Action onUpdated;

        [SerializeField] List<Entry> m_Entries = new List<Entry>();

#if UNITY_EDITOR
        void OnEnable()
        {
            // We are recreating references for override objects as these cannot be serialized.
            RebuildNonSerializedReferences();
        }
#endif

        public void RebuildNonSerializedReferences()
        {
            foreach (var entry in m_Entries)
            {
                foreach (var ovr in entry.overrides)
                {
                    ovr.entry = entry;
                }
            }
        }

        public List<Override> GetOverrides()
        {
            var overrides = new List<Override>();
            foreach (var entry in m_Entries)
            {
                foreach (var ovr in entry.overrides)
                {
                    overrides.Add(ovr);
                }
            }

            return overrides;
        }

        public List<Entry> GetEntriesByVisualTree(VisualTreeAsset vta)
        {
            var entries = new List<Entry>();
            for (int i = 0; i < m_Entries.Count; i++)
            {
                if (m_Entries[i].visualTreeAsset == vta)
                {
                    entries.Add(m_Entries[i]);
                }
            }

            return entries;
        }

        public bool TryAddEntry(Entry entry)
        {
            int index = m_Entries.BinarySearch(entry, entriesComparer);
            if (index >= 0)
            {
                return false;
            }

            m_Entries.Add(entry);
            m_Entries.Sort(entriesComparer);
            return true;
        }

        public bool TryGetEntry(string guid, out Entry entry, string name = "")
        {
            entry = null;
            int idx = m_Entries.BinarySearch(new Entry() { guid = guid, name = name }, entriesComparer);
            if (idx < 0)
            {
                return false;
            }

            entry = m_Entries[idx];
            return true;
        }

        public void RemoveEntry(Entry entry)
        {
            int idx = m_Entries.BinarySearch(entry, entriesComparer);
            if (idx >= 0)
            {
                m_Entries.RemoveAt(idx);
            }
        }

        public int IndexOf(Entry entry)
        {
            return m_Entries.BinarySearch(entry, entriesComparer);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            // Data management is limited during deserialization phase, so wait until next frame.
            InvokeOnUpdatedAsync();
        }

        async void InvokeOnUpdatedAsync()
        {
            await Task.Yield();
            onUpdated?.Invoke();
        }
    }
}

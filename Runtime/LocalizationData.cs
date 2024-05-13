using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    public class LocalizationData : ScriptableObject
    {
        public static EntriesComparer entriesComparer = new EntriesComparer();

        [Serializable]
        public class Override
        {
            [SerializeField] string m_Guid;
            [SerializeField] VisualTreeAsset m_VisualTreeAsset;
            [SerializeField] LocalizationAddress m_Address;
            [NonSerialized] Entry m_Entry;

            public string instanceGuid
            {
                get => m_Guid;
                set => m_Guid = value;
            }

            public VisualTreeAsset instanceVisualTreeAsset
            {
                get => m_VisualTreeAsset;
                set => m_VisualTreeAsset = value;
            }

            public LocalizationAddress address
            {
                get => m_Address;
                set => m_Address = value;
            }

            public Entry entry
            {
                get => m_Entry;
                set => m_Entry = value;
            }

            public void Remove()
            {
                if (entry == null)
                {
                    Debug.LogWarning("Unable to remove override, entry reference not set. First call 'RebuildNonSerializedReferences' on database object.");
                    return;
                }

                entry.overrides.Remove(this);
            }
        }

        [Serializable]
        public class Entry
        {
            [SerializeField] string m_FullTypeName;
            [SerializeField] string m_Guid;
            [SerializeField] string m_Name;
            [SerializeField] VisualTreeAsset m_VisualTreeAsset;
            [SerializeField] LocalizationAddress m_Address;
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

            public LocalizationAddress address
            {
                get => m_Address;
                set => m_Address = value;
            }

            public List<Override> overrides
            {
                get => m_Overrides;
                set => m_Overrides = value;
            }

            public Entry()
            {
                m_Name = string.Empty;
                m_Overrides = new List<Override>();
            }

            public bool TryGetOverride(string guid, out Override ovr)
            {
                foreach (var o in m_Overrides)
                {
                    if (o.instanceGuid == guid)
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
                Override currentOverride = null;
                foreach (var or in m_Overrides)
                {
                    if (or.instanceGuid == newOverride.instanceGuid)
                    {
                        currentOverride = or;
                        break;
                    }
                }

                if (currentOverride != null)
                {
                    currentOverride.address = newOverride.address;
                }
                else
                {
                    m_Overrides.Add(newOverride);
                    newOverride.entry = this;
                }
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

        [SerializeField] List<Entry> m_Entries = new List<Entry>();

#if UNITY_EDITOR
        [MenuItem("Test/Create Database Asset")]
        static void CreateDatabaseAsset()
        {
            if (!Directory.Exists(LocalizationSettings.dataDirectory))
            {
                Directory.CreateDirectory(LocalizationSettings.dataDirectory);
            }

            var database = CreateInstance<LocalizationData>();
            var path = LocalizationSettings.dataDirectory + "/Database.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();

            var settings = LocalizationSettings.instance;
            if (settings != null && LocalizationSettings.database == null)
            {
                LocalizationSettings.database = database;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                Debug.LogFormat("Created new database at: '{0}'. It has been automatically attached to localization settings.", path);
            }
            else
            {
                Debug.LogFormat("Created new database at: '{0}'. You can now assign it to localization settings as active database.", path);
            }
        }

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
    }
}

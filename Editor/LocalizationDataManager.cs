using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UIDocumentLocalization.Wrappers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class LocalizationDataManager
    {
        public static void UpdateDatabase(VisualElement documentRootElement, VisualTreeAsset activeVisualTreeAsset)
        {
            var database = LocalizationConfigObject.instance.database;
            if (database == null)
            {
                return;
            }

            // First remove all entries, so database is clean.
            var entries = database.GetEntriesByVisualTree(activeVisualTreeAsset);
            foreach (var entry in entries)
            {
                database.RemoveEntry(entry);
            }

            // Sort entries which have been removed for faster search and re-adding.
            entries.Sort(LocalizationData.entriesComparer);
            var localizableElements = documentRootElement.GetLocalizableDescendants();
            foreach (var element in localizableElements)
            {
                if (!element.TryGetGuid(out var guid, out var ancestor))
                {
                    continue;
                }

                bool isCustomControlChild = ancestor != null;
                var entry = new LocalizationData.Entry()
                {
                    guid = guid,
                    name = isCustomControlChild ? element.name : string.Empty,
                };

                int idx = entries.BinarySearch(entry, LocalizationData.entriesComparer);
                if (idx >= 0)
                {
                    entry = entries[idx];
                }
                else
                {
                    if (isCustomControlChild)
                    {
                        entry.fullTypeName = ancestor.GetType().FullName;
                        entry.visualTreeAsset = ancestor.visualTreeAssetSource != null
                            ? ancestor.visualTreeAssetSource
                            : activeVisualTreeAsset;
                    }
                    else
                    {
                        entry.fullTypeName = element.GetType().FullName;
                        entry.visualTreeAsset = element.visualTreeAssetSource != null
                            ? element.visualTreeAssetSource
                            : activeVisualTreeAsset;
                    }
                }

                // Update list of properties which should be available for localization.
                var localizedProperties = new List<LocalizationData.LocalizedProperty>();
                foreach (var propertyInfo in element.GetLocalizedProperties())
                {
                    localizedProperties.Add(new LocalizationData.LocalizedProperty() { name = propertyInfo.Name });
                }

                // Here we are removing unused properties and adding new ones to entry.
                entry.MergeLocalizedProperties(localizedProperties);
                database.TryAddEntry(entry);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }

        public static void RemoveUnusedOverrides(List<string> removedElementGuids)
        {
            var database = LocalizationConfigObject.instance.database;
            if (database == null)
            {
                return;
            }

            int removedCount = 0;

            // Group overrides by overriding element guid. 
            var guidToOverrides = new Dictionary<string, List<LocalizationData.Override>>();
            foreach (var ovr in database.GetOverrides())
            {
                // Immediately remove empty overrides.
                if (ovr.isEmpty)
                {
                    ovr.Remove();
                    removedCount++;
                    continue;
                }

                List<LocalizationData.Override> overrides;
                if (!guidToOverrides.TryGetValue(ovr.overridingElementGuid, out overrides))
                {
                    overrides = new List<LocalizationData.Override>();
                    guidToOverrides.Add(ovr.overridingElementGuid, overrides);
                }

                overrides.Add(ovr);
            }

            // Remove overrides which are in group linked to removed element's guid.
            foreach (string guid in removedElementGuids)
            {
                List<LocalizationData.Override> overrides;
                if (!guidToOverrides.TryGetValue(guid, out overrides))
                {
                    continue;
                }

                foreach (var ovr in overrides)
                {
                    ovr.Remove();
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

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
        public static void UpdateDatabase(List<TextElement> textElements, VisualTreeAsset activeVisualTreeAsset)
        {
            var database = LocalizationConfigObject.instance.settings.database;
            if (database == null)
            {
                return;
            }

            // First remove all entries, so database is clean.
            var entries = database.GetEntriesByVisualTree(activeVisualTreeAsset);
            entries.Sort(LocalizationData.entriesComparer);
            foreach (var entry in entries)
            {
                database.RemoveEntry(entry);
            }

            foreach (var textElement in textElements)
            {
                if (!textElement.TryGetGuid(out var guid, out var ancestor))
                {
                    continue;
                }

                bool isCustomControlChild = ancestor != null;
                var entry = new LocalizationData.Entry()
                {
                    guid = guid,
                    name = isCustomControlChild ? textElement.name : string.Empty,
                };

                // If entry with similar guid and name already existed in database then there is
                // no reason to create it from scratch, just make a copy and re-add it to database.
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
                        entry.fullTypeName = textElement.GetType().FullName;
                        entry.visualTreeAsset = textElement.visualTreeAssetSource != null
                            ? textElement.visualTreeAssetSource
                            : activeVisualTreeAsset;
                    }
                }

                database.TryAddEntry(entry);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }

        public static void RemoveUnusedOverrides(List<string> removedElementGuids)
        {
            var database = LocalizationConfigObject.instance.settings.database;
            if (database == null)
            {
                return;
            }

            int removedCount = 0;

            // Group overrides by overriding element guid. 
            var guidToOverrides = new Dictionary<string, List<LocalizationData.Override>>();
            foreach (var ovr in database.GetOverrides())
            {
                // Immediately remove any override which address is empty.
                if (ovr.address.isEmpty)
                {
                    ovr.Remove();
                    removedCount++;
                    continue;
                }

                List<LocalizationData.Override> overrides;
                if (!guidToOverrides.TryGetValue(ovr.instanceGuid, out overrides))
                {
                    overrides = new List<LocalizationData.Override>();
                    guidToOverrides.Add(ovr.instanceGuid, overrides);
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UIDocumentLocalization.Wrappers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    [InitializeOnLoad]
    class BuilderDocumentManager
    {
        const float k_SelectionUpdateIntervalSec = 0.1f;
        const float k_VisualTreeAssetUpdateIntervalSec = 0.1f;
        const float k_LocalizationUpdateIntervalSec = 0.1f;

        static BuilderDocumentManager s_Instance;

        VisualTreeAsset m_ActiveVisualTreeAsset;
        VisualElement m_DocumentRootElement;
        Timer m_VisualTreeAssetUpdateTimer;

        Selection m_Selection;
        Timer m_SelectionUpdateTimer;

        Timer m_LocalizationUpdateTimer;

        int m_PreviousContentHash;
        List<string> m_DescendantsGuids;

        public static BuilderDocumentManager instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new BuilderDocumentManager();
                }

                return s_Instance;
            }
        }

        public static VisualTreeAsset activeVisualTreeAsset => instance.m_ActiveVisualTreeAsset;

        public static VisualElement documentRootElement => instance.m_DocumentRootElement;

        public static bool builderWindowOpened
        {
            get => instance.m_ActiveVisualTreeAsset != null;
        }

        /// <summary>
        /// Called on editor start.
        /// </summary>
        static BuilderDocumentManager()
        {
            s_Instance = new BuilderDocumentManager();
        }

        BuilderDocumentManager()
        {
            m_Selection = new Selection();
            m_DescendantsGuids = new List<string>();

            m_VisualTreeAssetUpdateTimer = new Timer(k_VisualTreeAssetUpdateIntervalSec, true);
            m_VisualTreeAssetUpdateTimer.onTimeout += UpdateActiveVisualTreeAsset;

            m_SelectionUpdateTimer = new Timer(k_SelectionUpdateIntervalSec, true);
            m_SelectionUpdateTimer.onTimeout += UpdateSelection;

            m_LocalizationUpdateTimer = new Timer(k_LocalizationUpdateIntervalSec, true);
            m_LocalizationUpdateTimer.onTimeout += UpdateDocumentLocalization;

            VisualTreeAssetPostprocessor.onUxmlImported += OnUxmlImported;
        }

        void UpdateSelection()
        {
            var builderWindow = BuilderWrapper.activeWindow;
            if (builderWindow == null)
            {
                return;
            }

            var selection = new Selection(builderWindow.selection.selection);
            if (m_Selection.CompareTo(selection) != 0)
            {
                m_Selection = selection;
                OnSelectionChanged();
            }
        }

        void OnSelectionChanged()
        {
            var database = LocalizationConfigObject.instance.database;
            var localizationWindow = LocalizationWindow.activeWindow;
            if (database == null || localizationWindow == null)
            {
                return;
            }

            localizationWindow.Clear();
            foreach (VisualElement ve in m_Selection)
            {
                if (!ve.TryGetGuid(out string guid, out VisualElement ancestor))
                {
                    continue;
                }

                bool isCustomControlChild = ancestor != null;
                string name = isCustomControlChild ? ve.name : string.Empty;
                if (!database.TryGetEntry(guid, out var entry, name))
                {
                    continue;
                }

                bool isOverride = isCustomControlChild
                    ? ancestor.visualTreeAssetSource != null
                    : ve.visualTreeAssetSource != null;

                var selectedElement = new LocalizationWindowSelectedElement { selectedElementName = ve.name };
                selectedElement.GenerateLocalizedPropertyElements(entry.localizedProperties.Count);
                selectedElement.overrideLabelDisplayed = isOverride;
                localizationWindow.AddSelectedElement(selectedElement);

                var databaseSO = new SerializedObject(database);
                var entrySP = databaseSO.FindProperty($"m_Entries.Array.data[{database.IndexOf(entry)}]");
                if (isOverride)
                {
                    VisualElement overridingElement;
                    overridingElement = ve.GetAncestorDefinedInTreeAsset(m_ActiveVisualTreeAsset);
                    if (overridingElement == null)
                    {
                        continue;
                    }

                    string overridingElementGuid = null;
                    overridingElementGuid = overridingElement.GetStringStylePropertyByName("guid");
                    if (string.IsNullOrEmpty(overridingElementGuid))
                    {
                        continue;
                    }

                    LocalizationData.Override ovr;
                    if (!entry.TryGetOverride(overridingElementGuid, out ovr))
                    {
                        // If override for overriding element does not exist in database, we have to create new one, as something
                        // has to be passed to BindProperty method of visual element.
                        ovr = new LocalizationData.Override()
                        {
                            overridingElementGuid = overridingElementGuid,
                            overridingElementVisualTreeAsset = m_ActiveVisualTreeAsset,
                        };

                        for (int i = 0; i < entry.localizedProperties.Count; i++)
                        {
                            var localizedProperty = new LocalizationData.LocalizedProperty() { name = entry.localizedProperties[i].name };
                            ovr.localizedProperties.Add(localizedProperty);
                        }

                        entry.AddOrReplaceOverride(ovr);

                        // As adding or replacing override in database makes database serialized object outdated, we have to update
                        // such SO and then fetch out serialized properties once again, otherwise we would not be able to get
                        // serialized property of recently added override object.
                        databaseSO.Update();
                        entrySP = databaseSO.FindProperty($"m_Entries.Array.data[{database.IndexOf(entry)}]");
                    }

                    int overrideIndex = entry.overrides.IndexOf(ovr);
                    var overrideSP = entrySP.FindPropertyRelative($"m_Overrides.Array.data[{overrideIndex}]");
                    for (int i = 0; i < entry.localizedProperties.Count; i++)
                    {
                        var localizedPropertyElement = selectedElement.GetLocalizedPropertyElement(i);
                        var localizedPropertySP = entrySP.FindPropertyRelative($"m_LocalizedProperties.Array.data[{i}]");

                        localizedPropertyElement.propertyTextField.BindProperty(localizedPropertySP.FindPropertyRelative("m_Name"));
                        localizedPropertyElement.baseAddressElement.BindProperty(localizedPropertySP.FindPropertyRelative("m_Address"));
                        localizedPropertyElement.baseAddressFoldoutDisplayed = true;

                        var overrideLocalizedPropertySP = overrideSP.FindPropertyRelative($"m_LocalizedProperties.Array.data[{i}]");
                        localizedPropertyElement.addressElement.BindProperty(overrideLocalizedPropertySP.FindPropertyRelative("m_Address"));
                    }
                }
                else
                {
                    for (int i = 0; i < entry.localizedProperties.Count; i++)
                    {
                        var localizedPropertyElement = selectedElement.GetLocalizedPropertyElement(i);
                        var localizedPropertySP = entrySP.FindPropertyRelative($"m_LocalizedProperties.Array.data[{i}]");

                        localizedPropertyElement.propertyTextField.BindProperty(localizedPropertySP.FindPropertyRelative("m_Name"));
                        localizedPropertyElement.addressElement.BindProperty(localizedPropertySP.FindPropertyRelative("m_Address"));
                        localizedPropertyElement.baseAddressFoldoutDisplayed = false;
                    }
                }
            }
        }

        void UpdateActiveVisualTreeAsset()
        {
            var activeVisualTreeAsset = BuilderWrapper.activeWindow?.document.visualTreeAsset;
            if (activeVisualTreeAsset != m_ActiveVisualTreeAsset)
            {
                m_ActiveVisualTreeAsset = activeVisualTreeAsset;
                OnActiveVisualTreeAssetChanged();
            }
        }

        void OnActiveVisualTreeAssetChanged()
        {
            if (builderWindowOpened)
            {
                m_DocumentRootElement = BuilderWrapper.activeWindow.documentRootElement;
                m_PreviousContentHash = m_ActiveVisualTreeAsset.contentHash;
                UpdateDatabase();
            }
        }

        void OnUxmlImported(string path)
        {
            if (!builderWindowOpened ||
                m_ActiveVisualTreeAsset.contentHash == m_PreviousContentHash ||
                AssetDatabase.GetAssetPath(m_ActiveVisualTreeAsset) != path)
            {
                return;
            }

            // Keep track of content hash to avoid infinite asset reimport loop.
            m_PreviousContentHash = m_ActiveVisualTreeAsset.contentHash;
            UpdateDatabase();
        }

        public void UpdateDatabase()
        {
            if (!builderWindowOpened)
            {
                return;
            }

            AssignOrUpdateGuids();
            LocalizationDataManager.UpdateDatabase(m_DocumentRootElement, m_ActiveVisualTreeAsset);

            var previousDescendantsGuids = m_DescendantsGuids;
            m_DescendantsGuids = m_DocumentRootElement.GetDescendantGuids();
            var removedDescendantGuids = GetRemovedDescendantsGuids(previousDescendantsGuids);
            LocalizationDataManager.RemoveUnusedOverrides(removedDescendantGuids);
        }

        List<String> GetRemovedDescendantsGuids(List<string> previousDescendantsGuids)
        {
            var removedDescendantGuids = new List<string>();
            foreach (var guid in previousDescendantsGuids)
            {
                if (m_DescendantsGuids.BinarySearch(guid) < 0)
                {
                    removedDescendantGuids.Add(guid);
                }
            }

            return removedDescendantGuids;
        }

        void AssignOrUpdateGuids()
        {
            // We want to restore selection after reloading document. Cache it as it might get
            // overwritten by selection update.
            var selection = m_Selection;
            selection.Store(m_DocumentRootElement);
            BuilderWrapper activeWindow = BuilderWrapper.activeWindow;

            // First save changes done to active visual tree asset to avoid 'unsaved changes' popup.
            activeWindow.SaveChanges();
            List<VisualTreeAsset> visualTreeAssets = new List<VisualTreeAsset>() { m_ActiveVisualTreeAsset };
            visualTreeAssets.AddRange(m_ActiveVisualTreeAsset.templateDependencies);
            foreach (var vta in visualTreeAssets)
            {
                // First load document which inline style sheet will be modified.
                activeWindow.LoadDocument(vta);
                List<VisualElementAssetWrapper> visualElementAssets = vta.GetVisualElementAssets();
                List<TemplateAssetWrapper> templateAssets = vta.GetTemplateAssets();
                int veaCount = visualElementAssets.Count;
                int tplCount = templateAssets.Count;
                for (int i = 0; i < veaCount + tplCount; i++)
                {
                    var asset = i < veaCount ? visualElementAssets[i] : templateAssets[i - veaCount];
                    if (asset.parentId == 0)
                    {
                        continue;
                    }

                    BuilderStyleSheetUtils.GetInlineStyleSheetAndRule(vta, asset, out StyleSheet styleSheet, out StyleRuleWrapper styleRule);
                    var styleProperty = BuilderStyleSheetUtils.GetOrCreateStylePropertyByStyleName(styleSheet, styleRule, "guid");
                    var isNewValue = styleProperty.values.Count == 0;

                    if (isNewValue)
                    {
                        styleSheet.AddValue(styleProperty, Guid.NewGuid().ToString("N"));
                    }
                    else
                    {
                        string currentGuid = styleSheet.GetString(styleProperty.values[0]);
                        if (!Guid.TryParse(currentGuid, out Guid result))
                        {
                            styleSheet.SetValue(styleProperty.values[0], Guid.NewGuid().ToString("N"));
                        }
                    }
                }

                // Save changes made to loaded document.
                // At this stage style will be correct in inline style sheet scriptable object and UXML file, but not in visual tree asset.
                activeWindow.SaveChanges();
            }

            // Load back first document, so sequence does not end up with loaded dependency document.
            activeWindow.LoadDocument(m_ActiveVisualTreeAsset);

            // Force reimport of UXML assets to reflect inline style in visual element assets of each visual tree asset.
            foreach (var vta in visualTreeAssets)
            {
                string uxmlPath = AssetDatabase.GetAssetPath(vta);
                AssetDatabase.ImportAsset(uxmlPath, ImportAssetOptions.ForceUpdate);
            }

            // Selection might only be restored when all asset database operations are finished,
            // so let's wait one frame.
            DelayedCall.New(() =>
            {
                selection.Restore(m_DocumentRootElement);
                if (selection.Any())
                {
                    var ve = selection.Last();
                    var viewport = activeWindow.viewport;
                    activeWindow.selection.Select(viewport, ve);
                    viewport.SetInnerSelection(ve);
                }
            });
        }

        void UpdateDocumentLocalization()
        {
            if (!builderWindowOpened)
            {
                return;
            }

            var database = LocalizationConfigObject.instance.database;
            if (database == null)
            {
                return;
            }

            var localizableDescendants = m_DocumentRootElement.GetLocalizableDescendants();
            foreach (var localizableDescendant in localizableDescendants)
            {
                if (!localizableDescendant.TryGetGuid(out string guid, out VisualElement ancestor))
                {
                    continue;
                }

                bool isCustomControlChild = ancestor != null;
                string name = isCustomControlChild ? localizableDescendant.name : string.Empty;

                if (!database.TryGetEntry(guid, out var entry, name))
                {
                    continue;
                }

                foreach (var localizedProperty in entry.localizedProperties)
                {
                    var address = localizedProperty.address;
                    var currentAncestor = localizableDescendant.hierarchy.parent;
                    while (currentAncestor != null)
                    {
                        string ancestorGuid = currentAncestor.GetStringStylePropertyByName("guid");
                        if (!string.IsNullOrEmpty(ancestorGuid))
                        {
                            if (entry.TryGetOverride(ancestorGuid, out var ovr) &&
                                ovr.TryGetLocalizedProperty(localizedProperty.name, out var overrideLocalizedProperty) &&
                                !overrideLocalizedProperty.address.isEmpty)
                            {
                                address = overrideLocalizedProperty.address;
                            }
                        }

                        currentAncestor = currentAncestor.hierarchy.parent;
                    }

                    if (!address.isEmpty)
                    {
                        var propertyInfo = localizableDescendant.GetType().GetProperty(localizedProperty.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        propertyInfo.SetValue(localizableDescendant, address.translation);
                    }
                }
            }
        }
    }
}

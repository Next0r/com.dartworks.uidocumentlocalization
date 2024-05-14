using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIDocumentLocalization.Wrappers;
using Unity.VisualScripting;
using UnityEditor;
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
            var database = LocalizationConfigObject.instance.settings.database;
            var localizationWindow = LocalizationWindow.activeWindow;
            if (database == null || localizationWindow == null)
            {
                return;
            }

            localizationWindow.Clear();
            foreach (VisualElement ve in m_Selection)
            {
                if (ve is not TextElement textElement)
                {
                    continue;
                }

                if (textElement.TryGetGuid(out string guid, out VisualElement ancestor))
                {
                    bool isCustomControlChild = ancestor != null;
                    string name = isCustomControlChild ? textElement.name : string.Empty;
                    if (!database.TryGetEntry(guid, out var entry, name))
                    {
                        continue;
                    }

                    bool isOverride = isCustomControlChild
                        ? ancestor.visualTreeAssetSource != null
                        : textElement.visualTreeAssetSource != null;

                    var selectedElement = new LocalizationWindowSelectedElement { selectedElementName = ve.name };

                    var serializedObject = new SerializedObject(database);
                    var entrySp = serializedObject.FindProperty($"m_Entries.Array.data[{database.IndexOf(entry)}]");
                    var overridesSp = entrySp.FindPropertyRelative("m_Overrides");
                    int overrideIndex = -1;
                    if (isOverride)
                    {
                        var overridingVe = textElement.GetAncestorDefinedInTreeAsset(m_ActiveVisualTreeAsset);
                        if (overridingVe == null)
                        {
                            continue;
                        }

                        string overridingVeGuid = overridingVe.GetStringStylePropertyByName("guid");
                        if (string.IsNullOrEmpty(overridingVeGuid))
                        {
                            continue;
                        }

                        // We are trying to bind override property, let's find index of one that matches instance guid.
                        for (int i = 0; i < overridesSp.arraySize; i++)
                        {
                            var oSp = overridesSp.FindPropertyRelative($"Array.data[{i}]");
                            var oGuid = oSp.FindPropertyRelative("m_Guid").stringValue;
                            if (oGuid == overridingVeGuid)
                            {
                                overrideIndex = i;
                                break;
                            }
                        }

                        // If override index is still -1 it means that we have to add new empty override in order to generate
                        // serialized property which contents can be bound to address fields.
                        if (overrideIndex == -1)
                        {
                            var ovr = new LocalizationData.Override()
                            {
                                instanceGuid = overridingVeGuid,
                                instanceVisualTreeAsset = m_ActiveVisualTreeAsset,
                            };
                            entry.AddOrReplaceOverride(ovr);

                            // After adding empty override we have to sync serialized object and once again fetch it's properties.
                            serializedObject.Update();
                            entrySp = serializedObject.FindProperty($"m_Entries.Array.data[{database.IndexOf(entry)}]");
                            overridesSp = entrySp.FindPropertyRelative("m_Overrides");

                            // Entry overrides are not being sorted, so override index is last one in array (list).
                            overrideIndex = overridesSp.arraySize - 1;
                        }
                    }

                    selectedElement.BindProperty(entrySp, overrideIndex);
                    localizationWindow.AddSelectedElement(selectedElement);
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
            LocalizationDataManager.UpdateDatabase(m_DocumentRootElement.GetDescendantTextElements(), m_ActiveVisualTreeAsset);

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

            var database = LocalizationConfigObject.instance.settings?.database;
            if (database == null)
            {
                return;
            }

            var textElements = m_DocumentRootElement.GetDescendantTextElements();
            foreach (var textElement in textElements)
            {
                if (!textElement.TryGetGuid(out string guid, out VisualElement ancestor))
                {
                    continue;
                }

                bool isCustomControlChild = ancestor != null;
                string name = isCustomControlChild ? textElement.name : string.Empty;
                if (database.TryGetEntry(guid, out var entry, name))
                {
                    string fallbackTranslation = entry.address.translation;
                    string translation = null;
                    var currentAncestor = textElement.hierarchy.parent;
                    while (currentAncestor != null)
                    {
                        string ancestorGuid = currentAncestor.GetStringStylePropertyByName("guid");
                        if (!string.IsNullOrEmpty(ancestorGuid))
                        {
                            if (entry.TryGetOverride(ancestorGuid, out var ovr) && !ovr.address.isEmpty)
                            {
                                translation = ovr.address.translation;
                            }
                        }

                        currentAncestor = currentAncestor.hierarchy.parent;
                    }

                    if (translation != null)
                    {
                        textElement.text = translation;
                    }
                    else if (!entry.address.isEmpty)
                    {
                        textElement.text = fallbackTranslation;
                    }
                }
            }
        }
    }
}

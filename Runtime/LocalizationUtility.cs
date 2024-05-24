using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    public class LocalizationUtility
    {
        static int m_OperationsPerBudgetCheck = 1;
        static float m_TimeBudgetMs = 8f;

        /// <summary>
        /// Measuring execution time also consumes some frame budget, increasing this value reduces amount of
        /// time measure operations but it also reduces measurement precision, what might result in stalling of main thread.
        /// </summary>
        public static int operationsPerBudgetCheck
        {
            get => m_OperationsPerBudgetCheck;
            set
            {
                m_OperationsPerBudgetCheck = Mathf.Max(1, value);
            }
        }

        public static float timeBudgetMs
        {
            get => m_TimeBudgetMs;
            set
            {
                m_TimeBudgetMs = Mathf.Max(0f, value);
            }
        }

        public static LocalizationAsyncOperation LocalizeSubTreeAsync(VisualElement subTreeRoot)
        {
            var op = new LocalizationAsyncOperation();
            LocalizeSubTreeAsyncTask(subTreeRoot, op);
            return op;
        }

        static async void LocalizeSubTreeAsyncTask(VisualElement subTreeRoot, LocalizationAsyncOperation op)
        {
            // We have to yield at least once, otherwise async operation events might be invoked before
            // caller manages to register any callbacks.
            await Task.Yield();

            var timeBudgetSec = m_TimeBudgetMs / 1000f;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Getting localizable descendants is expensive operation as it looks for property attributes,
            // so it's also included in time budget.
            var localizableDescendants = subTreeRoot.GetLocalizableDescendants();
            if (stopwatch.GetElapsedSeconds() > timeBudgetSec)
            {
                await Task.Yield();
                stopwatch.Restart();
            }

            var operationsCount = 0;
            // We are running through localizables twice, when getting translation info, and when
            // applying translations. 
            var maxOperationsCount = localizableDescendants.Count * 2;

            var veTranslationInfos = new Dictionary<VisualElement, TranslationInfo>();
            foreach (var localizableDescendant in localizableDescendants)
            {
                // We need to check whether text element still exists as loop is being executed asynchronously.
                if (localizableDescendant != null)
                {
                    var translations = GetTranslations(localizableDescendant);
                    if (translations.Any())
                    {
                        veTranslationInfos.Add(localizableDescendant, translations);
                    }
                }

                operationsCount += 1;
                op.progress = (float)operationsCount / (localizableDescendants.Count * 2);
                if (operationsCount % m_OperationsPerBudgetCheck != 0)
                {
                    continue;
                }

                if (stopwatch.GetElapsedSeconds() > timeBudgetSec)
                {
                    await Task.Yield();
                    stopwatch.Restart();
                }
            }

            var elementPropertiesInfos = new List<PropertyTracker.ElementPropertiesInfo>();
            foreach (var veTranslationInfo in veTranslationInfos)
            {
                var visualElement = veTranslationInfo.Key;

                // Running asynchronously, check whether element is still there.
                if (visualElement != null)
                {
                    elementPropertiesInfos.Add(PropertyTracker.instance.GetOrCreateElementPropertiesInfo(visualElement));
                }
                else
                {
                    PropertyTracker.instance.RemoveElementPropertiesInfo(visualElement);
                }

                operationsCount += 1;
                op.progress = (float)operationsCount / (localizableDescendants.Count * 2);
                if (stopwatch.GetElapsedSeconds() > timeBudgetSec)
                {
                    await Task.Yield();
                    stopwatch.Restart();
                }
            }

            // This entire part has to be executed synchronously as we don't want to allow user to change
            // 'default value' of localized property in the meantime, and we don't want to force multiple
            // layout updates of UI when text properties are changed in different frames.
            for (int i = 0; i < elementPropertiesInfos.Count; i++)
            {
                elementPropertiesInfos[i].UpdateTrackedPropertiesDefaultValues();
                var translationInfo = veTranslationInfos.Values.ElementAt(i);
                foreach (var info in translationInfo)
                {
                    elementPropertiesInfos[i].TryGetTrackedProperty(info.propertyName, out var trackedProperty);
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

            op.isDone = true;
            op.InvokeCompleted();
        }

        public static void Localize(VisualElement localizableElement)
        {
            var translations = GetTranslations(localizableElement);
            localizableElement.ApplyTranslations(translations);
        }

        static TranslationInfo GetTranslations(VisualElement visualElement)
        {
            var database = LocalizationConfigObject.instance.database;
            if (database == null)
            {
                Debug.LogWarning("Localization failed. Database is missing.");
                return TranslationInfo.Empty;
            }

            if (!visualElement.TryGetGuid(out string guid, out VisualElement ancestor))
            {
                Debug.LogWarningFormat("Localization failed. '{0}' has no guid assigned.", visualElement.name);
                return TranslationInfo.Empty;
            }

            bool isCustomControlChild = ancestor != null;
            string name = isCustomControlChild ? visualElement.name : string.Empty;
            if (!database.TryGetEntry(guid, out var entry, name))
            {
                Debug.LogWarningFormat("Localization failed. '{0}' has no corresponding entry in localization database.", visualElement.name);
                return TranslationInfo.Empty;
            }

            var translations = new TranslationInfo();
            foreach (var localizedProperty in entry.localizedProperties)
            {
                var address = localizedProperty.address;
                var currentAncestor = visualElement.hierarchy.parent;
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

                if (address.isEmpty)
                {
                    translations.Add(localizedProperty.name, null);
                }
                else if (address.TryGetTranslation(out string translation))
                {
                    translations.Add(localizedProperty.name, translation);
                }
                else
                {
                    translations.Add(localizedProperty.name, address.ToString());
                }
            }

            return translations;
        }
    }
}

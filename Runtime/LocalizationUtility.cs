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
        static float m_timeBudgetMs = 8f;

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
            get => m_timeBudgetMs;
            set
            {
                m_timeBudgetMs = Mathf.Max(0f, value);
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
            var timeBudgetSec = m_timeBudgetMs / 1000f;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Building a query and turning it into list also costs significant amount of time,
            // so it's also included in time budget.
            var localizableDescendants = subTreeRoot.GetLocalizableDescendants();
            if (stopwatch.GetElapsedSeconds() > timeBudgetSec)
            {
                await Task.Yield();
                stopwatch.Restart();
            }

            int processedElements = 0;
            var localizableToTranslationsDict = new Dictionary<VisualElement, TranslationInfo>();
            foreach (var localizableDescendant in localizableDescendants)
            {
                // We need to check whether text element still exists as loop is being executed asynchronously.
                if (localizableDescendant != null)
                {
                    // string translation = GetTranslations(textElement);
                    var translations = GetTranslations(localizableDescendant);
                    if (translations.Any())
                    {
                        localizableToTranslationsDict.Add(localizableDescendant, translations);
                    }
                }

                processedElements += 1;
                op.progress = ((float)processedElements / localizableDescendants.Count);
                if (processedElements % m_OperationsPerBudgetCheck != 0)
                {
                    continue;
                }

                if (stopwatch.GetElapsedSeconds() > timeBudgetSec)
                {
                    await Task.Yield();
                    stopwatch.Restart();
                }
            }

            // Set all text element properties at once to avoid multiple canvas rebuild calls.
            foreach (var localizableToTranslations in localizableToTranslationsDict)
            {
                var localizableElement = localizableToTranslations.Key;
                var translations = localizableToTranslations.Value;

                // Once again text element could be removed in 'meantime'.
                if (localizableElement != null)
                {
                    localizableElement.ApplyTranslations(translations);
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
                return null;
            }

            if (!visualElement.TryGetGuid(out string guid, out VisualElement ancestor))
            {
                Debug.LogWarningFormat("Localization failed. '{0}' has no guid assigned.", visualElement.name);
                return null;
            }

            bool isCustomControlChild = ancestor != null;
            string name = isCustomControlChild ? visualElement.name : string.Empty;
            if (!database.TryGetEntry(guid, out var entry, name))
            {
                Debug.LogWarningFormat("Localization failed. '{0}' has no corresponding entry in localization database.", visualElement.name);
                return null;
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

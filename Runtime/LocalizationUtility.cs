using System;
using System.Collections;
using System.Collections.Generic;
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
            var textElements = subTreeRoot.Query<TextElement>().ToList();
            if (stopwatch.GetElapsedSeconds() > timeBudgetSec)
            {
                await Task.Yield();
                stopwatch.Restart();
            }

            int processedElements = 0;
            var elementToTranslationDict = new Dictionary<TextElement, string>();
            foreach (var textElement in textElements)
            {
                // We need to check whether text element still exists as loop is being executed asynchronously.
                if (textElement != null)
                {
                    string translation = GetTranslation(textElement);
                    if (!string.IsNullOrEmpty(translation))
                    {
                        elementToTranslationDict.Add(textElement, translation);
                    }
                }

                processedElements += 1;
                op.progress = ((float)processedElements / textElements.Count);
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
            foreach (var elementToTranslation in elementToTranslationDict)
            {
                TextElement textElement = elementToTranslation.Key;
                string translation = elementToTranslation.Value;

                // Once again text element could be removed in 'meantime'.
                if (textElement != null)
                {
                    textElement.text = translation;
                }
            }

            op.isDone = true;
            op.InvokeCompleted();
        }

        public static void Localize(TextElement textElement)
        {
            string translation = GetTranslation(textElement);
            if (!string.IsNullOrEmpty(translation))
            {
                textElement.text = translation;
            }
        }

        static string GetTranslation(TextElement textElement)
        {
            var database = LocalizationConfigObject.instance.settings.database;
            if (database == null)
            {
                Debug.LogWarning("Localization failed. Database is missing.");
                return null;
            }

            if (!textElement.TryGetGuid(out string guid, out VisualElement ancestor))
            {
                Debug.LogWarningFormat("Localization failed. '{0}' has no guid assigned.", textElement.name);
                return null;
            }

            bool isCustomControlChild = ancestor != null;
            string name = isCustomControlChild ? textElement.name : string.Empty;
            if (!database.TryGetEntry(guid, out var entry, name))
            {
                Debug.LogWarningFormat("Localization failed. '{0}' has no corresponding entry in localization database.", textElement.name);
                return null;
            }

            entry.address.TryGetTranslation(out string fallbackTranslation);
            string translation = null;
            var currentAncestor = textElement.hierarchy.parent;
            while (currentAncestor != null)
            {
                string ancestorGuid = currentAncestor.GetStringStylePropertyByName("guid");
                if (!string.IsNullOrEmpty(ancestorGuid))
                {
                    if (entry.TryGetOverride(ancestorGuid, out var ovr) && !ovr.address.isEmpty)
                    {
                        ovr.address.TryGetTranslation(out translation);
                    }
                }

                currentAncestor = currentAncestor.hierarchy.parent;
            }

            return translation != null ? translation : fallbackTranslation;
        }
    }
}

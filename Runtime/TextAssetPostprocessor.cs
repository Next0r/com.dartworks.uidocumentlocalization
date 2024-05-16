using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// In runtime we are stripping entire class definition as it uses UnityEditor namespace classes and methods.
// TextAssetPostprocessor has not been moved to Editor directory as it's a dependency of LocalizationTable
// used both in editor and runtime and would become inaccessible.
#if UNITY_EDITOR
namespace UIDocumentLocalization
{
    class TextAssetPostprocessor : AssetPostprocessor
    {
        static List<ITextAssetPostprocessorListener> s_CsvImportedListeners;

        static TextAssetPostprocessor()
        {
            s_CsvImportedListeners = new List<ITextAssetPostprocessorListener>();
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (string path in importedAssets)
            {
                string extension = Path.GetExtension(path);
                if (extension == ".csv")
                {
                    InvokeListeners(path);
                }
            }
        }

        static void InvokeListeners(string path)
        {
            for (int i = 0; i < s_CsvImportedListeners.Count; i++)
            {
                // Whenever listeners are being invoked check whether they are actually saved on disk
                // as Unity tends to be quite lazy in case of removing Scriptable Objects what might
                // result in exception when we are trying to mark object for saving.
                string p = AssetDatabase.GetAssetPath((UnityEngine.Object)s_CsvImportedListeners[i]);
                if (string.IsNullOrEmpty(p))
                {
                    s_CsvImportedListeners.RemoveAt(i);
                    i--;
                }
                else
                {
                    s_CsvImportedListeners[i].OnCsvImported(path);
                }
            }
        }

        internal static void RegisterListener(ITextAssetPostprocessorListener listener)
        {
            if (s_CsvImportedListeners.Contains(listener))
            {
                return;
            }

            s_CsvImportedListeners.Add(listener);
        }

        internal static void UnRegisterListener(ITextAssetPostprocessorListener listener)
        {
            s_CsvImportedListeners.Remove(listener);
        }
    }
}
#endif


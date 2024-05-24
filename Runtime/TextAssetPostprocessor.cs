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
        public static event Action<string> onCsvImported;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (string path in importedAssets)
            {
                string extension = Path.GetExtension(path);
                if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    onCsvImported?.Invoke(path);
                }
            }
        }
    }
}
#endif


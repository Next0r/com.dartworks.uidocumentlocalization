using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

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
                if (extension == ".csv")
                {
                    onCsvImported?.Invoke(path);
                }
            }
        }
    }
}
#endif


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    class VisualTreeAssetPostprocessor : AssetPostprocessor
    {
        public static event Action<string> onUxmlImported;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (string path in importedAssets)
            {
                string extension = Path.GetExtension(path);
                if (extension == ".uxml")
                {
                    onUxmlImported?.Invoke(path);
                }
            }
        }
    }
}

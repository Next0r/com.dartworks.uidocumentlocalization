using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    class LocalizationTextAssetCreator
    {
        const string k_NewFileName = "RawTable.csv";
        const string k_DefaultFileText = "# Here add your localization data as key and set of translations e.g. myKey1,\"My Translation\",\"Moje Tłumaczenie\".";

        [MenuItem("Assets/Create/UIDocument Localization/Raw Table", true)]
        static bool ValidateCreateTextAsset(MenuCommand menuCommand)
        {
            string selectionDirectoryPath = GetSelectionDirectoryPath();
            string pathRootFolder = selectionDirectoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).First();
            return pathRootFolder == "Assets";
        }

        [MenuItem("Assets/Create/UIDocument Localization/Raw Table")]
        static void CreateTextAsset(MenuCommand menuCommand)
        {
            string selectionDirectoryPath = GetSelectionDirectoryPath();
            string filePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(selectionDirectoryPath, k_NewFileName));
            using (StreamWriter outputFile = new StreamWriter(filePath))
            {
                outputFile.WriteLine(k_DefaultFileText);
            }

            AssetDatabase.Refresh();
        }

        static string GetSelectionDirectoryPath()
        {
            var guids = UnityEditor.Selection.assetGUIDs;
            if (guids.Any())
            {
                string path = AssetDatabase.GUIDToAssetPath(guids.First());
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                // If file was selected.
                if (!Directory.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }

                return path;
            }
            else
            {
                return null;
            }
        }
    }
}

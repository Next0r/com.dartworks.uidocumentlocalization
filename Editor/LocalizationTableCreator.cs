using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    class LocalizationTableCreator
    {
        const string k_NewFileName = "RawTable.csv";
        const string k_DefaultFileText = @"# Here add your localization data as key and set of translations, e.g. myKey1,""My Translation"",""Moje Tłumaczenie"".
        # Usage of '""' at the beginning and end of string is not necessary, but it allows to create longer
        # sentences containing commas which would normally be interpreted as column separators.
        #
        # Translation strings order is not enforced in any way, as locales are bound to columns and can be swapped 
        # any time inside Localization Settings Asset.
        #
        # To use this file as data input for Localization Table Asset drag this file and drop it into object
        # field in Table Asset inspector.
        # Empty lines and lines preceded with '#' character will be discarded when this file is processed.
        # As long as this file is attached to Localization Table Asset, such table will update every time you make
        # a change to this file.";

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

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
        const string k_NewCsvFileName = "RawTable.csv";
        const string k_NewTableAssetName = "Table.asset";
        const string k_DefaultCsvFileText = @"# Here add your localization data as key and set of translations, e.g. myKey1,""My Translation"",""Moje Tłumaczenie"".
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

        [MenuItem("Assets/Create/UIDocument Localization/Raw Table (.csv)")]
        static void CreateRawTableAsset()
        {
            ProjectWindowUtil.CreateAssetWithContent(k_NewCsvFileName, k_DefaultCsvFileText, AssetPreview.GetMiniTypeThumbnail(typeof(TextAsset)));
        }

        [MenuItem("Assets/Create/UIDocument Localization/Table")]
        static void CreateTableAsset()
        {
            var asset = ScriptableObject.CreateInstance<LocalizationTable>();
            if (UnityEditor.Selection.activeObject == null)
            {
                Debug.LogError("Unable to resolve selected directory path.");
                return;
            }

            string pathName = null;
            var instanceId = UnityEditor.Selection.activeObject.GetInstanceID();
            var selectionPathName = AssetDatabase.GetAssetPath(instanceId);
            if (ProjectWindowUtil.IsFolder(instanceId))
            {
                pathName = Path.Combine(selectionPathName, k_NewTableAssetName);
            }
            else
            {
                pathName = Path.Combine(ProjectWindowUtil.GetContainingFolder(selectionPathName), k_NewTableAssetName);
            }

            TextAsset selectedCsvFile = null;
            if (Path.HasExtension(selectionPathName) && Path.GetExtension(selectionPathName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                selectedCsvFile = UnityEditor.Selection.activeObject as TextAsset;
            }

            var actionObject = ScriptableObject.CreateInstance<DoCreateNewAsset>();
            actionObject.onFinished += () =>
            {
                if (selectedCsvFile != null)
                {
                    asset.textAsset = selectedCsvFile;
                    asset.Rebuild();
                }
            };

            // This is exactly what ProjectWindowUtil.CreateAsset() does.
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), actionObject, pathName, AssetPreview.GetMiniThumbnail(asset), null);
        }
    }
}

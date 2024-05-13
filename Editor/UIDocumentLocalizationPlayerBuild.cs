using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIDocumentLocalization;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class UIDocumentLocalizationBuildPlayer : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    LocalizationConfigObject m_ConfigObject;

    public int callbackOrder => 0;

    /// <summary>
    /// Before building a player make sure that config object is passed from editor build config
    /// to preloaded assets, so it's OnEnable gets called.
    /// </summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        m_ConfigObject = LocalizationConfigObject.instance;
        if (m_ConfigObject != null)
        {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            if (!preloadedAssets.Contains(m_ConfigObject))
            {
                preloadedAssets.Add(m_ConfigObject);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
        }

    }

    /// <summary>
    /// There is no need to keep config object in player after build is completed, so remove it here.
    /// </summary>
    public void OnPostprocessBuild(BuildReport report)
    {
        var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
        if (m_ConfigObject != null)
        {
            preloadedAssets.Remove(m_ConfigObject);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            m_ConfigObject = null;
        }
    }
}

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
    LocalizationSettings m_Settings;

    public int callbackOrder => 0;

    /// <summary>
    /// Before building a player make sure that localization settings instance is passed from editor build config
    /// to preloaded assets, so LocalizationSettings.OnEnable gets called.
    /// </summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        m_Settings = LocalizationSettings.instance;
        if (m_Settings != null)
        {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            if (!preloadedAssets.Contains(m_Settings))
            {
                preloadedAssets.Add(m_Settings);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
        }

    }

    /// <summary>
    /// There is no need to keep localization settings in player after build is completed, so remove it here.
    /// </summary>
    public void OnPostprocessBuild(BuildReport report)
    {
        var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
        if (m_Settings != null)
        {
            preloadedAssets.Remove(m_Settings);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            m_Settings = null;
        }
    }
}

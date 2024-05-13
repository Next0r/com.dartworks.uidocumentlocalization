using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UIDocumentLocalization.Wrappers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    static class VisualTreeAssetUtils
    {
        public static Type type = typeof(VisualTreeAsset);

        static PropertyInfo s_VisualElementAssetsProperty = type.GetProperty("visualElementAssets", BindingFlags.Instance | BindingFlags.NonPublic);
        static PropertyInfo s_TemplateAssetsProperty = type.GetProperty("templateAssets", BindingFlags.Instance | BindingFlags.NonPublic);

        public static List<VisualElementAssetWrapper> GetVisualElementAssets(this VisualTreeAsset vta)
        {
            IList assets = (IList)s_VisualElementAssetsProperty.GetValue(vta);
            List<VisualElementAssetWrapper> visualElementAssets = new List<VisualElementAssetWrapper>();
            foreach (var asset in assets)
            {
                visualElementAssets.Add(new VisualElementAssetWrapper(asset));
            }

            return visualElementAssets;
        }

        public static List<TemplateAssetWrapper> GetTemplateAssets(this VisualTreeAsset vta)
        {
            IList assets = (IList)s_TemplateAssetsProperty.GetValue(vta);
            List<TemplateAssetWrapper> templateAssets = new List<TemplateAssetWrapper>();
            foreach (var asset in assets)
            {
                templateAssets.Add(new TemplateAssetWrapper(asset));
            }

            return templateAssets;
        }
    }
}

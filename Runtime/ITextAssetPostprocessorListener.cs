using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIDocumentLocalization
{
    internal interface ITextAssetPostprocessorListener
    {
        /// <summary>
        /// This interface method does nothing by default, so actual implementation
        /// might be stripped off for runtime build.
        /// </summary>
        public void OnCsvImported(string path) { }
    }
}

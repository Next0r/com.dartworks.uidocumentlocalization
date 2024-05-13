using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIDocumentLocalization
{
    /// <summary>
    /// UnityCsReference/ModuleOverrides/com.unity.ui/Core/StyleSheets/StyleValueType.cs
    /// </summary>
    internal enum StyleValueType
    {
        Invalid,
        Keyword,
        Float,
        Dimension,
        Color,
        ResourcePath, // When using resource("...")
        AssetReference,
        Enum, // A literal value that is not quoted
        Variable, // A literal value starting with "--"
        String, // A quoted value or any other value that is not recognized as a primitive
        Function,
        CommaSeparator,
        ScalableImage,
        MissingAssetReference,
    }
}

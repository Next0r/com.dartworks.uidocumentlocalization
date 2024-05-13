using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UIDocumentLocalization.Wrappers;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    static class StyleSheetUtils
    {
        static Type s_Type = typeof(StyleSheet);

        static MethodInfo s_ReadStringMethod = s_Type.GetMethod("ReadString", BindingFlags.Instance | BindingFlags.NonPublic);

        public static string ReadString(this StyleSheet styleSheet, StyleValueHandleWrapper styleValueHandleWrapper)
        {
            return (string)s_ReadStringMethod.Invoke(styleSheet, new object[] { styleValueHandleWrapper.obj });
        }
    }
}

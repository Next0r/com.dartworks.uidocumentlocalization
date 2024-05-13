using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UIDocumentLocalization.Wrappers;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    static class BuilderStyleSheetUtils
    {
        static Type s_BsuType = BuilderWrapper.assembly.GetType("Unity.UI.Builder.BuilderStyleUtilities");
        static Type s_SpeType = BuilderWrapper.assembly.GetType("Unity.UI.Builder.StylePropertyExtensions");
        static Type s_SvhType = BuilderWrapper.assembly.GetType("Unity.UI.Builder.StyleValueHandleExtensions");

        static MethodInfo s_GetInlineStyleSheetAndRuleMethod = s_BsuType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList()
            .Find(m => m.Name == "GetInlineStyleSheetAndRule" && m.GetParameters()[1].ParameterType == VisualElementAssetWrapper.type);

        static MethodInfo s_GetOrCreateStylePropertyByStyleNameMethod = s_BsuType.GetMethod("GetOrCreateStylePropertyByStyleName",
            BindingFlags.Static | BindingFlags.NonPublic);

        static MethodInfo s_AddStringValueMethod = s_SpeType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList()
            .Find(m => m.Name == "AddValue" && m.GetParameters()[2].ParameterType == typeof(string));

        static MethodInfo s_SetStringValueMethod = s_SvhType.GetMethods(BindingFlags.Static | BindingFlags.Public).ToList()
            .Find(m => m.Name == "SetValue" && m.GetParameters()[2].ParameterType == typeof(string));

        static MethodInfo s_GetStringMethod = s_SvhType.GetMethod("GetString", BindingFlags.Static | BindingFlags.Public);


        public static void GetInlineStyleSheetAndRule(VisualTreeAsset vta, VisualElementAssetWrapper vea, out StyleSheet styleSheet, out StyleRuleWrapper styleRule)
        {
            object ss = null;
            object sr = null;
            var args = new object[] { vta, vea.obj, ss, sr };
            s_GetInlineStyleSheetAndRuleMethod.Invoke(null, args);

            styleSheet = (StyleSheet)args[2];
            styleRule = new StyleRuleWrapper(args[3]);
        }

        public static StylePropertyWrapper GetOrCreateStylePropertyByStyleName(StyleSheet styleSheet, StyleRuleWrapper ruleWrapper, string styleName)
        {
            return new StylePropertyWrapper(s_GetOrCreateStylePropertyByStyleNameMethod.Invoke(null, new object[] { styleSheet, ruleWrapper.obj, styleName }));
        }

        public static void AddValue(this StyleSheet styleSheet, StylePropertyWrapper stylePropertyWrapper, string value)
        {
            s_AddStringValueMethod.Invoke(null, new object[] { styleSheet, stylePropertyWrapper.obj, value });
        }

        public static void SetValue(this StyleSheet styleSheet, StyleValueHandleWrapper styleValueHandleWrapper, string value)
        {
            s_SetStringValueMethod.Invoke(null, new object[] { styleSheet, styleValueHandleWrapper.obj, value });
        }

        public static string GetString(this StyleSheet styleSheet, StyleValueHandleWrapper styleValueHandleWrapper)
        {
            return (string)s_GetStringMethod.Invoke(null, new object[] { styleSheet, styleValueHandleWrapper.obj });
        }
    }
}
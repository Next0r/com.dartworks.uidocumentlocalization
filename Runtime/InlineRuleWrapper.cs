using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class InlineRuleWrapper
    {
        public static Type type = typeof(VisualTreeAsset).Assembly.GetType("UnityEngine.UIElements.InlineStyleAccess+InlineRule");

        static FieldInfo s_SheetField = type.GetField("sheet");
        static FieldInfo s_RuleField = type.GetField("rule");

        object m_Obj;
        StyleSheet m_StyleSheet;
        StyleRuleWrapper m_StyleRule;

        public object obj => m_Obj;
        public StyleSheet styleSheet => m_StyleSheet;
        public StyleRuleWrapper styleRule => m_StyleRule;

        public InlineRuleWrapper(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (obj.GetType() != type)
            {
                throw new ArgumentException();
            }

            m_Obj = obj;
            m_StyleSheet = (StyleSheet)s_SheetField.GetValue(obj);

            // If visual element has been created from script, it's inline style rule will be null.
            var wrappedStyleRule = s_RuleField.GetValue(obj);
            m_StyleRule = wrappedStyleRule != null ? new StyleRuleWrapper(s_RuleField.GetValue(obj)) : null;
        }
    }
}
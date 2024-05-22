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

        public object obj => m_Obj;
        public StyleSheet styleSheet => (StyleSheet)s_SheetField.GetValue(obj);

        public StyleRuleWrapper styleRule
        {
            get
            {
                var wrappedStyleRule = s_RuleField.GetValue(obj);
                return wrappedStyleRule != null ? new StyleRuleWrapper(s_RuleField.GetValue(obj)) : null;
            }
        }

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
        }
    }
}
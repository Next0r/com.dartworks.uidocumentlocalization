using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization.Wrappers
{
    class InlineStyleAccessWrapper
    {
        public static Type type = typeof(VisualTreeAsset).Assembly.GetType("UnityEngine.UIElements.InlineStyleAccess");
        static FieldInfo s_InlineRuleField = type.GetField("m_InlineRule", BindingFlags.Instance | BindingFlags.NonPublic);

        IStyle m_Obj;
        InlineRuleWrapper m_InlineRule;

        public IStyle obj => m_Obj;
        public InlineRuleWrapper inlineRule => new InlineRuleWrapper(s_InlineRuleField.GetValue(m_Obj));

        InlineStyleAccessWrapper() { }

        public InlineStyleAccessWrapper(IStyle obj)
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

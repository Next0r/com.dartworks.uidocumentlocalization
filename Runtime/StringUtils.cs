using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UIDocumentLocalization
{
    static class StringUtils
    {
        public static string ToUxmlAttributeName(this string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return propertyName;
            }

            // We are adding hyphen before every upper case character ([A-Z]) or number (\d),
            // so property name 'CustomProperty1' becomes 'Custom-Property-1'
            var attributeName = Regex.Replace(propertyName, @"(?<!^)(?=[A-Z]|\d)", "-");
            return attributeName.ToLower();
        }
    }
}

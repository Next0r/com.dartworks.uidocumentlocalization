using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UIDocumentLocalization
{
    static class StringUtils
    {
        /// <summary>
        /// Converts 'CsPropertyName1' to 'cs-property-name-1'.
        /// </summary>
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

        /// <summary>
        /// Converts 'uxml-alike-name-1' to 'UxmlAlikeName1';
        /// </summary>
        public static string ToPropertyName(this string uxmlAttributeName)
        {
            var propertyName = string.Empty;
            var substrings = uxmlAttributeName.Split('-', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var substring in substrings)
            {
                propertyName += char.ToUpper(substring[0]) + substring.Substring(1);
            }

            return propertyName;
        }
    }
}

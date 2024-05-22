using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace UIDocumentLocalization
{
    static class XmlDocumentUtils
    {
        public static List<XmlNode> GetDescendants(this XmlNode xmlNode)
        {
            var xmlNodes = new List<XmlNode>();
            GetDescendantsRecursive(xmlNode, xmlNodes);
            return xmlNodes;
        }

        static void GetDescendantsRecursive(XmlNode xmlNode, List<XmlNode> xmlNodes)
        {
            foreach (var child in xmlNode.ChildNodes.Cast<XmlNode>())
            {
                xmlNodes.Add(child);
                GetDescendantsRecursive(child, xmlNodes);
            }
        }

        public static string GetInlineStyleProperty(this XmlNode xmlNode, string propertyName)
        {
            var xmlElement = (XmlElement)xmlNode;
            string styleString = xmlElement.GetAttribute("style");
            var pattern = $"{propertyName}:\\s*(.*?);";
            var match = Regex.Match(styleString, pattern);
            if (match.Success && match.Groups.Count >= 1)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        public static void SetInlineStyleProperty(this XmlNode xmlNode, string propertyName, string value)
        {
            var xmlElement = (XmlElement)xmlNode;
            if (!xmlElement.HasAttribute("style"))
            {
                xmlElement.SetAttribute("style", $"{propertyName}: {value};");
            }
            else
            {
                string styleString = xmlElement.GetAttribute("style");
                var pattern = $"{propertyName}:\\s*(.*?);";
                var match = Regex.Match(styleString, pattern);
                if (match.Success)
                {
                    styleString = Regex.Replace(styleString, pattern, $"{propertyName}: {value};");
                    xmlElement.SetAttribute("style", styleString);
                }
                else
                {
                    xmlElement.SetAttribute("style", $"{propertyName}: {value}; " + styleString);
                }
            }
        }
    }
}

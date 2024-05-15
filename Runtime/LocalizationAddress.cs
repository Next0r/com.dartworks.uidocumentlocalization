using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIDocumentLocalization
{
    [Serializable]
    public class LocalizationAddress
    {
        const string k_InvalidTableMessage = "Invalid table reference.";
        const string k_MissingEntryMessage = "Table '{0}' does not contain '{1}' entry.";
        const string k_MissingTranslationMessage = "Entry '{0}' in table '{1}' does not contain translation for locale '{2}'.";

        [SerializeField] LocalizationTable m_Table;
        [SerializeField] string m_Key;

        public LocalizationTable table
        {
            get => m_Table;
            set => m_Table = value;
        }

        public string key
        {
            get => m_Key;
            set => m_Key = value;
        }

        public bool isEmpty => table == null && string.IsNullOrEmpty(key);

        public string translation
        {
            get
            {
                if (table == null)
                {
                    return k_InvalidTableMessage;
                }

                var entry = table.GetEntry(key);
                if (entry == null)
                {
                    return string.Format(k_MissingEntryMessage, table.name, key);
                }

                var settings = LocalizationConfigObject.instance.settings;
                if (!entry.TryGetTranslation(settings.selectedLocaleIndex, out string translation))
                {
                    return string.Format(k_MissingTranslationMessage, key, table.name, settings.selectedLocale);
                }

                return translation;
            }
        }

        public override string ToString()
        {
            string tableName = table?.name == null ? "null" : table.name;
            string keyName = string.IsNullOrEmpty(key) ? "null" : key;
            return string.Format("[table:{0}, key:{1}]", tableName, keyName);
        }

        public bool TryGetTranslation(out string translation)
        {
            translation = null;
            if (table == null)
            {
                return false;
            }

            var entry = table.GetEntry(key);
            if (entry == null)
            {
                return false;
            }

            var settings = LocalizationConfigObject.instance.settings;
            if (!entry.TryGetTranslation(settings.selectedLocaleIndex, out translation))
            {
                return false;
            }

            return true;
        }
    }
}


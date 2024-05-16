using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    public class LocalizationTable : ScriptableObject, ITextAssetPostprocessorListener
    {
        static EntryComparer s_EntryComparer = new EntryComparer();

#if UNITY_EDITOR
        const int k_DefaultKeySetSize = 8;
        const int k_OperationsPerBudgetCheck = 5;
        const long k_GetMatchingKeysBudgetMs = 10L;
        const char k_CsvFileCommentChar = '#';
#endif


        [Serializable]
        public class Entry
        {
            [SerializeField] string m_Key;
            [SerializeField] List<string> m_Translations = new List<string>();

            public string key
            {
                get => m_Key;
                set => m_Key = value;
            }

            public List<string> translations
            {
                get => m_Translations;
                set => m_Translations = value;
            }

            public bool TryGetTranslation(int localeIndex, out string translation)
            {
                if (localeIndex >= 0 && localeIndex < m_Translations.Count)
                {
                    translation = m_Translations[localeIndex];
                    return true;
                }

                translation = null;
                return false;
            }
        }

        public class EntryComparer : IComparer<Entry>
        {
            public int Compare(Entry x, Entry y)
            {
                return x.key.CompareTo(y.key);
            }
        }

        [SerializeField] TextAsset m_TextAsset;
        [SerializeField] int m_ContentHash;
        [SerializeField] List<Entry> m_Entries = new List<Entry>();

        public TextAsset textAsset
        {
            get => m_TextAsset;
            set => m_TextAsset = value;
        }

        public int contentHash
        {
            get => m_ContentHash;
            set => m_ContentHash = value;
        }

        public List<Entry> entries
        {
            get => m_Entries;
            set => m_Entries = value;
        }

        public List<string> keys
        {
            get
            {
                var keys = new List<string>();
                foreach (var entry in m_Entries)
                {
                    keys.Add(entry.key);
                }

                return keys;
            }
        }

        public static int GenerateHash(string contents)
        {
            var h = new Hash128();
            byte[] b = Encoding.UTF8.GetBytes(contents);
            if (b.Length > 0)
            {
                HashUtilities.ComputeHash128(b, ref h);
            }

            return h.GetHashCode();
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            Rebuild();
            TextAssetPostprocessor.RegisterListener(this);
        }

        void ITextAssetPostprocessorListener.OnCsvImported(string path)
        {
            if (textAsset == null)
            {
                return;
            }

            string p = AssetDatabase.GetAssetPath(textAsset);
            if (p == path)
            {
                Rebuild();
            }
        }

        public void Rebuild()
        {
            if (textAsset == null)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(textAsset);
            string extension = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(extension) || extension != ".csv")
            {
                return;
            }

            int hash = GenerateHash(textAsset.text);
            if (hash == m_ContentHash)
            {
                return;
            }

            m_Entries.Clear();
            m_ContentHash = hash;
            string[] rows = textAsset.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (string row in rows)
            {
                if (row.TrimStart().StartsWith(k_CsvFileCommentChar))
                {
                    continue;
                }

                var cellValues = ParseCsvRow(row);
                if (cellValues.Count == 0)
                {
                    continue;
                }

                var entry = new Entry()
                {
                    key = cellValues.First(),
                    translations = cellValues.GetRange(1, cellValues.Count - 1),
                };

                AddEntry(entry);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }


        List<string> ParseCsvRow(string row)
        {
            // https://gist.github.com/awwsmm/886ac0ce0cef517ad7092915f708175f
            Regex regex = new Regex("(?:,|\\n|^)(\"(?:(?:\"\")*[^\"]*)*\"|[^\",\\n]*|(?:\\n|$))");
            var cellValues = new List<string>();
            foreach (Match match in regex.Matches(row))
            {
                var cellValue = match.Value.Trim(',', '"', ' ');
                if (!string.IsNullOrWhiteSpace(cellValue))
                {
                    cellValues.Add(cellValue);
                }
            }

            return cellValues;
        }

        public LocalizationAsyncOperation<List<string>> GetMatchingKeysAsync(string input)
        {
            var op = new LocalizationAsyncOperation<List<string>>();
            GetMatchingKeysAsyncTask(input, op);
            return op;
        }

        async void GetMatchingKeysAsyncTask(string input, LocalizationAsyncOperation<List<string>> op)
        {
            await Task.Yield();
            if (op.cancelled)
            {
                return;
            }

            var keys = this.keys;
            if (!keys.Any())
            {
                op.isDone = true;
                op.InvokeCompleted(new List<string>());
                return;
            }

            var matchingKeys = new List<string>();
            if (string.IsNullOrEmpty(input))
            {
                matchingKeys = keys.GetRange(0, Mathf.Min(k_DefaultKeySetSize, keys.Count));
            }
            else
            {
                float timeBudgetSec = k_GetMatchingKeysBudgetMs / 1000f;
                int processedElements = 0;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                foreach (string key in keys)
                {
                    if (key.Contains(input, StringComparison.InvariantCultureIgnoreCase))
                    {
                        matchingKeys.Add(key);
                    }

                    processedElements++;
                    if (processedElements % k_OperationsPerBudgetCheck == 0 && stopwatch.GetElapsedSeconds() > timeBudgetSec)
                    {
                        await Task.Yield();
                        if (op.cancelled)
                        {
                            return;
                        }
                        stopwatch.Restart();
                    }
                }
            }

            op.isDone = true;
            op.InvokeCompleted(matchingKeys);
        }
#endif

        public void AddEntry(Entry entry)
        {
            m_Entries.Add(entry);
            m_Entries.Sort(s_EntryComparer);
        }

        public Entry GetEntry(string key)
        {
            int index = m_Entries.BinarySearch(new Entry() { key = key }, s_EntryComparer);
            if (index >= 0)
            {
                return m_Entries[index];
            }

            return null;
        }
    }
}

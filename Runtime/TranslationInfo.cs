using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIDocumentLocalization
{
    public class TranslationInfo : IEnumerable<TranslationInfo.Entry>
    {
        public struct Entry
        {
            public string propertyName;
            public string translation;
        }

        public class TranslationsInfoEnumerator : IEnumerator<Entry>
        {
            List<Entry> m_Entries;
            int m_Position = -1;

            public object Current => m_Entries[m_Position];

            Entry IEnumerator<Entry>.Current => m_Entries[m_Position];

            public TranslationsInfoEnumerator(List<Entry> entries)
            {
                m_Entries = entries;
            }

            public bool MoveNext()
            {
                m_Position++;
                return m_Position < m_Entries.Count;
            }

            public void Reset()
            {
                m_Position = -1;
            }

            public void Dispose()
            {
                m_Entries = null;
            }
        }

        List<Entry> m_Entries;

        public List<Entry> entries
        {
            get => m_Entries;
            set => m_Entries = value;
        }

        public static TranslationInfo Empty => new TranslationInfo();

        public TranslationInfo()
        {
            m_Entries = new List<Entry>();
        }

        public void Add(string propertyName, string translation)
        {
            var entry = new Entry()
            {
                propertyName = propertyName,
                translation = translation
            };

            m_Entries.Add(entry);
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            return new TranslationsInfoEnumerator(m_Entries);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TranslationsInfoEnumerator(m_Entries);
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class CachedSelection : IEnumerable<CachedSelection.Entry>
    {
        public struct Entry
        {
            public string name;
            public int position;
        }

        public class CachedSelectionEnumerator : IEnumerator<Entry>
        {
            List<Entry> m_Entries;
            int m_Position = -1;

            public object Current => m_Entries[m_Position];

            Entry IEnumerator<Entry>.Current => m_Entries[m_Position];

            public CachedSelectionEnumerator(List<Entry> entries)
            {
                m_Entries = entries;
            }

            public void Dispose()
            {
                m_Entries = null;
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
        }

        List<Entry> m_Entries;

        public CachedSelection()
        {
            m_Entries = new List<Entry>();
        }

        public void AddEntry(string name, int position)
        {
            m_Entries.Add(new Entry() { name = name, position = position });
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            return new CachedSelectionEnumerator(m_Entries);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CachedSelectionEnumerator(m_Entries);
        }
    }

    class Selection : IEnumerable<VisualElement>, IEquatable<Selection>
    {
        public class SelectionEnumerator : IEnumerator<VisualElement>
        {
            List<VisualElement> m_VisualElements;
            int m_Position = -1;

            public VisualElement Current => m_VisualElements[m_Position];

            object IEnumerator.Current => m_VisualElements[m_Position];

            public SelectionEnumerator(List<VisualElement> visualElements)
            {
                m_VisualElements = visualElements;
            }

            public bool MoveNext()
            {
                m_Position++;
                return m_Position < m_VisualElements.Count;
            }

            public void Reset()
            {
                m_Position = 0;
            }

            public void Dispose()
            {
                m_VisualElements = null;
            }
        }

        List<VisualElement> m_VisualElements;
        // List<int> m_Positions;

        public Selection()
        {
            m_VisualElements = new List<VisualElement>();
            // m_Positions = new List<int>();
        }

        public Selection(List<VisualElement> visualElements)
        {
            if (visualElements == null)
            {
                throw new ArgumentNullException();
            }

            m_VisualElements = visualElements;
            // m_Positions = new List<int>();
        }

        public CachedSelection Store(VisualElement documentRootElement)
        {
            var cachedSelection = new CachedSelection();
            var visualElements = documentRootElement.GetDescendants();
            foreach (var ve in m_VisualElements)
            {
                int position = visualElements.IndexOf(ve);
                if (position >= 0)
                {
                    cachedSelection.AddEntry(ve.name, position);
                }
            }

            return cachedSelection;
        }

        public void Restore(VisualElement documentRootElement, CachedSelection cachedSelection)
        {
            m_VisualElements.Clear();
            var visualElements = documentRootElement.GetDescendants();
            foreach (var entry in cachedSelection)
            {
                if (entry.position < visualElements.Count)
                {
                    var element = visualElements[entry.position];
                    if (element.name == entry.name)
                    {
                        m_VisualElements.Add(element);
                    }
                }
            }
        }

        public void Clear()
        {
            m_VisualElements.Clear();
        }

        public IEnumerator<VisualElement> GetEnumerator()
        {
            return new SelectionEnumerator(m_VisualElements);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SelectionEnumerator(m_VisualElements);
        }

        public bool Equals(Selection other)
        {
            if (m_VisualElements.Count != other.m_VisualElements.Count)
            {
                return false;
            }

            for (int i = 0; i < m_VisualElements.Count; i++)
            {
                if (m_VisualElements[i] != other.m_VisualElements[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}

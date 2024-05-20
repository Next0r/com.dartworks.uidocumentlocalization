using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
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
        List<int> m_Positions;

        public bool isStored => m_Positions.Any();

        public Selection()
        {
            m_VisualElements = new List<VisualElement>();
            m_Positions = new List<int>();
        }

        public Selection(List<VisualElement> visualElements)
        {
            if (visualElements == null)
            {
                throw new ArgumentNullException();
            }

            m_VisualElements = visualElements;
            m_Positions = new List<int>();
        }

        public void Store(VisualElement documentRootElement)
        {
            m_Positions.Clear();
            var visualElements = documentRootElement.Query<VisualElement>().ToList();
            foreach (var ve in m_VisualElements)
            {
                m_Positions.Add(visualElements.IndexOf(ve));
            }
        }

        public void Restore(VisualElement documentRootElement)
        {
            m_VisualElements.Clear();
            var visualElements = documentRootElement.Query<VisualElement>().ToList();
            foreach (int position in m_Positions)
            {
                m_VisualElements.Add(visualElements[position]);
            }

            m_Positions.Clear();
        }

        public void Clear()
        {
            m_VisualElements.Clear();
            m_Positions.Clear();
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

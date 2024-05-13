using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class HintBox : VisualElement
    {
        const string k_UssClassName = "hint-box";
        const string k_ScrollViewUssClassName = k_UssClassName + "__scroll-view";
        const string k_ListViewUssClassName = k_UssClassName + "__list-view";
        const float k_ListViewLineHeight = 16f;

        List<string> m_Hints;
        TextField m_TrackedElement;
        ScrollView m_ScrollView;
        ListView m_ListView;
        Label m_MessageLabel;

        public List<string> hints
        {
            get => m_Hints;
            set
            {
                m_Hints = value;
                m_ListView.itemsSource = m_Hints;
            }
        }

        public TextField trackedElement
        {
            get => m_TrackedElement;
            set
            {
                var previousTrackedElement = m_TrackedElement;
                m_TrackedElement = value;
                if (m_TrackedElement != null)
                {
                    // Make sure we are not stacking up Track calls on delegate list.
                    if (previousTrackedElement == null)
                    {
                        EditorApplication.update += Track;
                    }

                    style.display = DisplayStyle.Flex;
                }
                else
                {
                    EditorApplication.update -= Track;
                    style.display = DisplayStyle.None;
                    m_ListView.ClearSelection();
                }
            }
        }

        public HintBox()
        {
            m_Hints = new List<string>();

            AddToClassList(k_UssClassName);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<FocusInEvent>(OnFocusIn);

            m_ScrollView = new ScrollView();
            m_ScrollView.AddToClassList(k_ScrollViewUssClassName);
            Add(m_ScrollView);

            m_ListView = new ListView(m_Hints, k_ListViewLineHeight, MakeItem, BindItem);
            m_ListView.AddToClassList(k_ListViewUssClassName);
            m_ListView.selectionChanged += OnSelectionChanged;
            m_ScrollView.Add(m_ListView);

            m_MessageLabel = new Label();
            Add(m_MessageLabel);

            HideMessage();
            trackedElement = null;
        }

        public void ShowMessage(string message)
        {
            m_MessageLabel.text = message;
            m_ScrollView.style.display = DisplayStyle.None;
            m_MessageLabel.style.display = DisplayStyle.Flex;
        }

        public void HideMessage()
        {
            m_ScrollView.style.display = DisplayStyle.Flex;
            m_MessageLabel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Close when focus lost.
        /// </summary>
        void OnFocusOut(FocusOutEvent evt)
        {
            trackedElement = null;
        }

        /// <summary>
        /// Close when focus gained but there are no elements to select.
        /// </summary>
        void OnFocusIn(FocusInEvent evt)
        {
            if (!hints.Any())
            {
                trackedElement = null;
            }
        }

        /// <summary>
        /// If anything was selected and we are tracking text field, set it's value and close.
        /// </summary>
        void OnSelectionChanged(IEnumerable<object> items)
        {
            if (items.Any() && trackedElement != null)
            {
                string item = (string)items.First();
                trackedElement.value = item;
            }

            trackedElement = null;
        }

        VisualElement MakeItem()
        {
            return new Label();
        }

        void BindItem(VisualElement ve, int index)
        {
            Label label = ve as Label;
            label.text = m_Hints[index];
        }

        void Track()
        {
            if (trackedElement != null)
            {
                // We are tracking input field position and width rather than entire text field.
                var rect = trackedElement.Q("unity-text-input").worldBound;
                style.top = rect.position.y;
                style.left = rect.position.x;
                style.width = rect.width;
            }
            else
            {
                EditorApplication.update -= Track;
            }
        }
    }
}

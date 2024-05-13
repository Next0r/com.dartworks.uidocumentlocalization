using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDocumentLocalization
{
    class LocalizationAddressElement : VisualElement
    {
        public const string ussClassName = "address-entry-element";
        public const string leftBoxUssClassName = ussClassName + "__left-box";
        public const string rightBoxUssClassName = ussClassName + "__right-box";
        public const string borderVariantUssClassName = ussClassName + "--border";

        const string k_NoKeyHintBoxMessage = "No match";
        const string k_SearchingHintBoxMessage = "Searching...";
        const string k_NoTableHintBoxMessage = "No table selected";

        ObjectField m_VisualTreeAsset;
        ObjectField m_TableField;
        TextField m_KeyTextField;
        Button m_ClearButton;
        bool m_DisplayBorder;
        bool m_DisplayVisualTreeAsset;
        LocalizationAsyncOperation<List<string>> m_AsyncOperation;

        public bool displayBorder
        {
            get => m_DisplayBorder;
            set
            {
                m_DisplayBorder = value;
                if (m_DisplayBorder)
                {
                    AddToClassList(borderVariantUssClassName);
                }
                else
                {
                    RemoveFromClassList(borderVariantUssClassName);
                }
            }
        }

        public bool displayVisualTreeAsset
        {
            get => m_DisplayVisualTreeAsset;
            set
            {
                m_DisplayVisualTreeAsset = value;
                m_VisualTreeAsset.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public LocalizationAddressElement()
        {
            AddToClassList(ussClassName);

            var leftBox = new VisualElement() { name = "left-box" };
            leftBox.AddToClassList(leftBoxUssClassName);
            Add(leftBox);

            m_VisualTreeAsset = new ObjectField() { label = "Source Visual Tree", objectType = typeof(VisualTreeAsset) };
            m_VisualTreeAsset.SetEnabled(false);
            leftBox.Add(m_VisualTreeAsset);

            m_TableField = new ObjectField() { label = "Table", objectType = typeof(LocalizationTable) };
            leftBox.Add(m_TableField);

            m_KeyTextField = new TextField() { label = "Key" };
            m_KeyTextField.RegisterCallback<FocusEvent>(OnKeyFieldFocus);
            m_KeyTextField.RegisterCallback<BlurEvent>(OnKeyFieldBlur);
            m_KeyTextField.RegisterValueChangedCallback(OnKeyFieldValueChange);
            leftBox.Add(m_KeyTextField);

            var rightBox = new VisualElement() { name = "right-box" };
            rightBox.AddToClassList(rightBoxUssClassName);
            Add(rightBox);

            m_ClearButton = new Button() { text = "Clear" };
            m_ClearButton.clicked += OnClearButtonClicked;
            rightBox.Add(m_ClearButton);

            displayVisualTreeAsset = false;
        }

        void RequestHintBoxContentUpdate()
        {
            m_AsyncOperation?.Cancel();
            var hintBox = LocalizationWindow.activeWindow.hintBox;

            if (m_TableField.value is not LocalizationTable localizationTable)
            {
                hintBox.ShowMessage(k_NoTableHintBoxMessage);
                return;
            }

            hintBox.ShowMessage(k_SearchingHintBoxMessage);
            m_AsyncOperation = localizationTable.GetMatchingKeysAsync(m_KeyTextField.value);
            m_AsyncOperation.completed += (List<string> result) =>
            {
                hintBox.hints = result;
                if (!hintBox.hints.Any())
                {
                    hintBox.ShowMessage(k_NoKeyHintBoxMessage);
                }
                else
                {
                    hintBox.HideMessage();
                }
            };
        }

        void OnKeyFieldValueChange(ChangeEvent<string> evt)
        {
            RequestHintBoxContentUpdate();
        }

        void OnKeyFieldFocus(FocusEvent evt)
        {
            var hintBox = LocalizationWindow.activeWindow.hintBox;
            hintBox.trackedElement = m_KeyTextField;
            RequestHintBoxContentUpdate();
        }

        void OnKeyFieldBlur(BlurEvent evt)
        {
            var nextFocusElement = evt.relatedTarget as VisualElement;
            var hintBox = nextFocusElement?.GetFirstAncestorOfType<HintBox>();

            // Focused element is null or is not child of hint box.
            if (hintBox == null)
            {
                LocalizationWindow.activeWindow.hintBox.trackedElement = null;
                m_AsyncOperation?.Cancel();
            }
        }

        public void BindProperty(SerializedProperty overrideOrEntrySp)
        {
            m_VisualTreeAsset.BindProperty(overrideOrEntrySp.FindPropertyRelative("m_VisualTreeAsset"));
            m_TableField.BindProperty(overrideOrEntrySp.FindPropertyRelative("m_Address.m_Table"));
            m_KeyTextField.BindProperty(overrideOrEntrySp.FindPropertyRelative("m_Address.m_Key"));
        }

        void OnClearButtonClicked()
        {
            m_TableField.value = null;
            m_KeyTextField.value = string.Empty;
        }
    }
}

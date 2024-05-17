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
        const string k_UssClassName = "address-entry-element";
        const string k_LeftBoxUssClassName = k_UssClassName + "__left-box";
        const string k_RightBoxUssClassName = k_UssClassName + "__right-box";

        const string k_NoKeyHintBoxMessage = "No match";
        const string k_SearchingHintBoxMessage = "Searching...";
        const string k_NoTableHintBoxMessage = "No table selected";

        ObjectField m_TableObjectField;
        TextField m_KeyTextField;
        Button m_ClearButton;
        LocalizationAsyncOperation<List<string>> m_AsyncOperation;

        public LocalizationAddressElement()
        {
            AddToClassList(k_UssClassName);

            var leftBox = new VisualElement() { name = "left-box" };
            leftBox.AddToClassList(k_LeftBoxUssClassName);
            Add(leftBox);

            m_TableObjectField = new ObjectField()
            {
                label = "Table",
                objectType = typeof(LocalizationTable)
            };
            leftBox.Add(m_TableObjectField);

            m_KeyTextField = new TextField() { label = "Key" };
            m_KeyTextField.RegisterCallback<FocusEvent>(OnKeyFieldFocus);
            m_KeyTextField.RegisterCallback<BlurEvent>(OnKeyFieldBlur);
            m_KeyTextField.RegisterValueChangedCallback(OnKeyFieldValueChange);
            leftBox.Add(m_KeyTextField);

            var rightBox = new VisualElement() { name = "right-box" };
            rightBox.AddToClassList(k_RightBoxUssClassName);
            Add(rightBox);

            m_ClearButton = new Button() { text = "Clear" };
            m_ClearButton.clicked += OnClearButtonClicked;
            rightBox.Add(m_ClearButton);
        }

        public void BindProperty(SerializedProperty serializedProperty)
        {
            m_TableObjectField.BindProperty(serializedProperty.FindPropertyRelative("m_Table"));
            m_KeyTextField.BindProperty(serializedProperty.FindPropertyRelative("m_Key"));
        }

        void RequestHintBoxContentUpdate()
        {
            m_AsyncOperation?.Cancel();
            var hintBox = LocalizationWindow.activeWindow.hintBox;

            if (m_TableObjectField.value is not LocalizationTable localizationTable)
            {
                hintBox.ShowMessage(k_NoTableHintBoxMessage);
                return;
            }

            hintBox.ShowMessage(k_SearchingHintBoxMessage);
            m_AsyncOperation = localizationTable.GetMatchingKeysAsync(m_KeyTextField.value);
            m_AsyncOperation.onCompleted += (List<string> result) =>
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

            // Next focus element might be null, especially when entire window loses focus.
            var hintBox = nextFocusElement?.GetFirstAncestorOfType<HintBox>();

            // Focused element is null or is not child of hint box.
            if (hintBox == null)
            {
                LocalizationWindow.activeWindow.hintBox.trackedElement = null;
                m_AsyncOperation?.Cancel();
            }
        }

        void OnClearButtonClicked()
        {
            m_TableObjectField.value = null;
            m_KeyTextField.value = string.Empty;
        }
    }
}

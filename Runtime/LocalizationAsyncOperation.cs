using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIDocumentLocalization
{
    public class LocalizationAsyncOperation
    {
        public event Action completed;

        bool m_IsDone;
        float m_Progress;
        bool m_Cancelled;

        public bool isDone
        {
            get => m_IsDone;
            internal set => m_IsDone = value;
        }

        public float progress
        {
            get => m_Progress;
            internal set => m_Progress = value;
        }

        public bool cancelled
        {
            get => m_Cancelled;
        }

        internal LocalizationAsyncOperation() { }

        internal void InvokeCompleted()
        {
            completed?.Invoke();
        }

        public void Cancel()
        {
            m_Cancelled = true;
        }
    }

    public class LocalizationAsyncOperation<T> : LocalizationAsyncOperation
    {
        public new event Action<T> completed;

        internal LocalizationAsyncOperation() : base() { }

        internal void InvokeCompleted(T result)
        {
            completed?.Invoke(result);
        }
    }
}

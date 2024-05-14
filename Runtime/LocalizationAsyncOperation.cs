using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIDocumentLocalization
{
    public abstract class LocalizationAsyncOperationBase
    {
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

        public void Cancel()
        {
            m_Cancelled = true;
        }
    }

    public class LocalizationAsyncOperation : LocalizationAsyncOperationBase
    {
        public event Action onCompleted;

        internal LocalizationAsyncOperation() { }

        internal void InvokeCompleted()
        {
            onCompleted?.Invoke();
        }
    }

    public class LocalizationAsyncOperation<T> : LocalizationAsyncOperationBase
    {
        public event Action<T> onCompleted;

        internal LocalizationAsyncOperation() { }

        internal void InvokeCompleted(T result)
        {
            onCompleted?.Invoke(result);
        }
    }
}

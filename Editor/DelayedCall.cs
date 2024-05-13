using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    class DelayedCall
    {
        int m_FrameCount;
        Action m_Callback;

        public static DelayedCall New(Action callback)
        {
            return new DelayedCall(callback);
        }

        public DelayedCall(Action callback)
        {
            m_Callback = callback;
            m_FrameCount = Time.frameCount;
            EditorApplication.update += Update;
        }

        void Update()
        {
            if (Time.frameCount > m_FrameCount)
            {
                m_Callback?.Invoke();
                EditorApplication.update -= Update;
            }
        }
    }
}

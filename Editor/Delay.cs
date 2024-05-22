using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    class Delay
    {
        public event Action onTimeout;

        int m_Ticks;
        int m_ElapsedTicks;

        public int ticks
        {
            get => m_Ticks;
            set
            {
                m_Ticks = Mathf.Max(0, value);
            }
        }

        public Delay(int ticks = 1)
        {
            this.ticks = ticks;
            m_ElapsedTicks = 0;
            EditorApplication.update += Update;
        }

        void Update()
        {
            m_ElapsedTicks += 1;
            if (m_ElapsedTicks >= m_Ticks)
            {
                try
                {
                    // Fire at the end of the frame.
                    EditorApplication.delayCall += () => onTimeout?.Invoke();
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    EditorApplication.update -= Update;
                }
            }
        }
    }
}

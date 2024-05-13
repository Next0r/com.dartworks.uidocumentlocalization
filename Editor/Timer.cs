using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    class Timer
    {
        public event Action onTimeout;

        float m_Interval;
        float m_Time;
        bool m_Repeat;
        double m_PreviousTimeSinceStartup;

        public float interval
        {
            get => m_Interval;
            set
            {
                m_Interval = Mathf.Max(0f, value);
            }
        }

        public float time
        {
            get => m_Time;
        }

        public bool repeat
        {
            get => m_Repeat;
            set => m_Repeat = value;
        }

        public Timer(float interval, bool repeat = false, bool start = true)
        {
            this.interval = interval;
            this.repeat = repeat;

            if (start)
            {
                Start();
            }
        }

        public void Start()
        {
            m_PreviousTimeSinceStartup = EditorApplication.timeSinceStartup;
            EditorApplication.update += Update;
        }

        public void Stop()
        {
            EditorApplication.update -= Update;
        }

        void Update()
        {
            float delta = (float)(EditorApplication.timeSinceStartup - m_PreviousTimeSinceStartup);
            m_PreviousTimeSinceStartup = EditorApplication.timeSinceStartup;

            m_Time += delta;
            if (m_Time < m_Interval)
            {
                return;
            }

            onTimeout.Invoke();
            if (repeat)
            {
                m_Time = 0f;
            }
            else
            {
                Stop();
            }
        }
    }
}

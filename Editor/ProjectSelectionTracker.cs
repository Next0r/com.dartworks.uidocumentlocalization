using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    [InitializeOnLoad]
    class ProjectSelectionTracker
    {
        static UnityEngine.Object[] s_PreviousSelectionObjects;
        static UnityEngine.Object[] s_SelectionObjects;

        /// <summary>
        /// Instead of event we are using hash set of callbacks to avoid issues with the same callback
        /// being registered multiple times. This tends to become problem when callbacks are added
        /// during initialization as Unity may call said initialization methods multiple times.
        /// </summary>
        static HashSet<OnSelectionChangedCallback> s_SelectionChangedCallbacks = new HashSet<OnSelectionChangedCallback>();

        static bool s_SuppressNextSelectionChange;

        static ProjectSelectionTracker()
        {
            s_PreviousSelectionObjects = UnityEditor.Selection.objects;
            s_SelectionObjects = UnityEditor.Selection.objects;
            UnityEditor.Selection.selectionChanged += OnSelectionChanged;
        }

        public delegate void OnSelectionChangedCallback();

        static void OnSelectionChanged()
        {
            s_PreviousSelectionObjects = s_SelectionObjects;
            s_SelectionObjects = UnityEditor.Selection.objects;

            if (s_SuppressNextSelectionChange)
            {
                s_SuppressNextSelectionChange = false;
                return;
            }

            foreach (var callback in s_SelectionChangedCallbacks)
            {
                callback?.Invoke();
            }
        }

        public static void RestoreSelectionWithoutNotify()
        {
            EditorApplication.delayCall += () =>
            {
                s_SuppressNextSelectionChange = true;
                var newSelection = new List<UnityEngine.Object>();
                foreach (var obj in s_PreviousSelectionObjects)
                {
                    if (obj != null)
                    {
                        newSelection.Add(obj);
                    }
                }

                UnityEditor.Selection.objects = newSelection.ToArray();
            };
        }

        public static void RegisterCallback(OnSelectionChangedCallback callback)
        {
            s_SelectionChangedCallbacks.Add(callback);
        }

        public static void UnregisterCallback(OnSelectionChangedCallback callback)
        {
            s_SelectionChangedCallbacks.Remove(callback);
        }

    }
}

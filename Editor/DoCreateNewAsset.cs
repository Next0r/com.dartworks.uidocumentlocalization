using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UIDocumentLocalization
{
    /// <summary>
    /// This class is basically a copy of DoCreateNewAsset class inside UnityEditor.ProjectWindowCallback namespace,
    /// but it does contain additional event handles which allow to call user defined methods when either action
    /// finishes successfully or is cancelled. 
    /// </summary>
    class DoCreateNewAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        static MethodInfo s_FrameObjectInProjectWindowMethod = typeof(ProjectWindowUtil).GetMethod("FrameObjectInProjectWindow", BindingFlags.Static | BindingFlags.NonPublic);

        public event Action onFinished;
        public event Action onCancelled;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));

            // Reflection is necessary as we have no access to ProjectBrowser class hidden in UnityEditor namespace
            // and it's FrameObject method.
            s_FrameObjectInProjectWindowMethod.Invoke(null, new object[] { instanceId });
            onFinished?.Invoke();
        }

        public override void Cancelled(int instanceId, string pathName, string resourceFile)
        {
            UnityEditor.Selection.activeObject = null;
            onCancelled?.Invoke();
        }
    }
}

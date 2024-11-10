using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MOYV.AssetsCheckerBase
{
    public static class AssetCheckerUtils
    {
        public static string GetSelectedFolder()
        {
            var guids = Selection.assetGUIDs;

            if (guids == null || guids.Length == 0)
            {
                return string.Empty;
            }

            foreach (var g in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(g);
                if (Directory.Exists(assetPath))
                {
                    return assetPath;
                }
            }

            return string.Empty;
        }

        public static void ActionWithProgress<T>(T[] guids, Action<T> action, Action onFinished = null)
        {
            var startIndex = 0;
            if (guids == null || guids.Length == 0)
            {
                onFinished?.Invoke();
                return;
            }

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            void OnEditorUpdate()
            {
                try
                {
                    if (startIndex >= 0 && guids.Length > startIndex)
                    {
                        var guid = guids[startIndex];

                        var isCancel = EditorUtility.DisplayCancelableProgressBar("Processing... ",
                            startIndex + " / " + guids.Length + " " + guid.ToString(),
                            (float)startIndex / (float)guids.Length);

                        if (action != null)
                        {
                            action(guid);
                        }

                        startIndex++;
                        if (isCancel || startIndex >= guids.Length)
                        {
                            EditorUtility.ClearProgressBar();
                            EditorApplication.update -= OnEditorUpdate;
                            startIndex = 0;

                            onFinished?.Invoke();
                        }
                    }
                }
                catch (Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update -= OnEditorUpdate;
                    startIndex = 0;
                    Debug.LogError(e);
                    onFinished?.Invoke();
                    throw;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MOYV
{
    public class EditorUtils
    {
        public static void Merge(string srcPath, string dstPath, bool ignoreMeta = false,
            Action<string> onProccessedCallbak = null)
        {
            if (!Directory.Exists(dstPath))
                Directory.CreateDirectory(dstPath);
            foreach (string file in Directory.GetFiles(srcPath))
            {
                if (ignoreMeta && file.EndsWith(".meta"))
                    continue;
                
                string des;
                des = Path.Combine(dstPath, Path.GetFileName(file));
                File.Copy(file, des, true);
                if (onProccessedCallbak != null)
                    onProccessedCallbak(des);
            }
            
            foreach (string dir in Directory.GetDirectories(srcPath))
                Merge(dir, Path.Combine(dstPath, Path.GetFileName(dir)), ignoreMeta, onProccessedCallbak);
        }
        
        #region Copy Path
        
        [MenuItem("Assets/Copy Asset Path to ClipBoard")]
        public static void CopyFilepath()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            TextEditor text2Editor = new TextEditor();
            text2Editor.text = path;
            text2Editor.OnFocus();
            text2Editor.Copy();
            Debug.Log(path);
#endif
        }
        
        [MenuItem("Assets/Copy Resources Load Path to ClipBoard")]
        public static void GetResourcesPath()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            int startIndex = path.LastIndexOf("Resources/", StringComparison.Ordinal);
            if (startIndex == -1)
            {
                Debug.LogError("Please Choose A Resouces Load Path!Current Path :" + path + " IS WRONG");
                path = "";
            }
            else
            {
                int length = "Resources/".Length;
                path = path.Substring(startIndex + length);
                int lastIndex = path.LastIndexOf(".", StringComparison.Ordinal);
                if (lastIndex == -1)
                {
                    lastIndex = path.Length;
                }
                
                path = path.Substring(0, lastIndex);
                
                Debug.Log(path);
            }
            
            TextEditor text2Editor = new TextEditor();
            text2Editor.text = path;
            text2Editor.OnFocus();
            text2Editor.Copy();
#endif
        }
        
        [MenuItem("Assets/Copy Game Relative Path to ClipBoard")]
        public static void GetGameRelativePath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            
            if (path.Contains("Assets/MainApp/"))
            {
            }
            else if (path.Contains("Assets/Game/"))
            {
                path = path.Replace("Assets/Game/", "");
                path = path.Substring(path.IndexOf("/", StringComparison.Ordinal) + 1);
            }
            
            TextEditor text2Editor = new TextEditor();
            text2Editor.text = path;
            text2Editor.OnFocus();
            text2Editor.Copy();
            Debug.Log(path);
        }
        
        #endregion
        
        #region 查找资源引用
        
        [MenuItem("Assets/Find References", true)]
        private static bool IsFindValid()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return (!string.IsNullOrEmpty(path));
        }
        
        [MenuItem("Assets/Find References", false, 10)]
        private static void Find()
        {
            var currentOpenScenePath = SceneManager.GetActiveScene().path;
            
            var selectObject = Selection.activeObject;
            var selectPath = AssetDatabase.GetAssetPath(selectObject);
            if (!string.IsNullOrEmpty(selectPath))
            {
                EditorSettings.serializationMode = SerializationMode.ForceText;
                
                var assetPaths = AssetDatabase.FindAssets("t:Object", new string[] {"Assets"})
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(x => x != selectPath &&
                                (x.EndsWith(".prefab") ||
                                 x.EndsWith(".unity") ||
                                 x.EndsWith(".mat") ||
                                 x.EndsWith(".asset")))
                    .ToArray();
                
                FindReferences(selectPath, selectObject, assetPaths);
            }
            
            if (currentOpenScenePath != SceneManager.GetActiveScene().path)
            {
                EditorSceneManager.OpenScene(currentOpenScenePath, OpenSceneMode.Single);
            }
        }
        
        private static void FindReferences(string searchPath, Object searchAsset, string[] assetPaths)
        {
            var searchInfos = new List<(string text, string name, string guid, long fileId)>();
            if (AssetDatabase.IsSubAsset(searchAsset))
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(searchAsset, out var searchGuid,
                        out long searchFileId))
                {
                    searchInfos.Add(($"{{fileID: {searchFileId}, guid: {searchGuid},", searchAsset.name, searchGuid,
                        searchFileId));
                }
            }
            else
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(searchAsset, out var searchGuid,
                        out long searchFileId))
                {
                    searchInfos.Add(($"{{fileID: {searchFileId}, guid: {searchGuid},", searchAsset.name, searchGuid,
                        -1));
                }
            }
            
            var startIndex = 0;
            
            void EditorUpdate()
            {
                bool isCancel = false;
                var assetPath = assetPaths[startIndex];
                try
                {
                    isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", Path.GetFileName(assetPath),
                        (float) startIndex / assetPaths.Length);
                    
                    var text = File.ReadAllText(assetPath);
                    foreach (var searchInfo in searchInfos)
                    {
                        if (Regex.IsMatch(text, searchInfo.text))
                        {
                            if (assetPath.EndsWith(".prefab"))
                            {
                                var prefab = PrefabUtility.LoadPrefabContents(assetPath);
                                FindReferenceInGameObjectRecursively(assetPath, prefab, string.Empty,
                                    searchInfo.name, searchInfo.guid, searchInfo.fileId);
                                PrefabUtility.UnloadPrefabContents(prefab);
                            }
                            else if (assetPath.EndsWith(".unity"))
                            {
                                var scene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
                                var roots = scene.GetRootGameObjects();
                                foreach (var root in roots)
                                {
                                    FindReferenceInGameObjectRecursively(assetPath, root, root.name,
                                        searchInfo.name, searchInfo.guid, searchInfo.fileId);
                                }
                            }
                            else
                            {
                                Debug.Log($"{assetPath}使用了该资源中的：{searchAsset.name}");
                            }
                        }
                    }
                    
                    startIndex++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"执行到资源：{assetPath}时报错\n{e}");
                    isCancel = true;
                }
                finally
                {
                    if (isCancel || startIndex >= assetPaths.Length)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorApplication.update -= EditorUpdate;
                        startIndex = 0;
                        Debug.Log("匹配结束");
                    }
                }
            }
            
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
            
            void FindReferenceInGameObjectRecursively(string assetPath, GameObject gameObject,
                string parentNodePath, string searchName, string searchGuid, long searchFileId)
            {
                var components = gameObject.GetComponents<Component>();
                var nodePath = string.IsNullOrEmpty(parentNodePath)
                    ? $"{gameObject.name}"
                    : $"{parentNodePath}/{gameObject.name}";
                foreach (var component in components)
                {
                    if (component != null)
                    {
                        var so = new SerializedObject(component);
                        var sp = so.GetIterator();
                        
                        while (sp.NextVisible(true))
                        {
                            if (sp.propertyType == SerializedPropertyType.ObjectReference &&
                                sp.objectReferenceValue != null)
                            {
                                var obj = sp.objectReferenceValue;
                                
                                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid,
                                        out long fileId) && guid == searchGuid)
                                {
                                    if (searchFileId > 0)
                                    {
                                        if (fileId == searchFileId)
                                        {
                                            Debug.Log(
                                                $"{assetPath}使用了该资源中的：{searchName}，在{nodePath}中的{component.GetType().Name}上");
                                        }
                                    }
                                    else
                                    {
                                        var searchAssets = AssetDatabase.LoadAllAssetsAtPath(searchPath);
                                        var isBelonged = false;
                                        foreach (var asset in searchAssets)
                                        {
                                            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var assetGuid,
                                                    out long assetFileId))
                                            {
                                                if (fileId == assetFileId)
                                                {
                                                    Debug.Log(
                                                        $"{assetPath}使用了该资源中的：{asset.name}，在{nodePath}中的{component.GetType().Name}上");
                                                    isBelonged = true;
                                                    break;
                                                }
                                            }
                                        }
                                        
                                        if (!isBelonged)
                                        {
                                            Debug.Log(
                                                $"{assetPath}使用了该资源，但Missing了，在{nodePath}中的{component.GetType().Name}上");
                                        }
                                    }
                                }
                            }
                        }
                        
                        sp.Dispose();
                        so.Dispose();
                    }
                }
                
                var childCount = gameObject.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i).gameObject;
                    FindReferenceInGameObjectRecursively(assetPath, child, nodePath, searchName, searchGuid,
                        searchFileId);
                }
            }
        }
        
        #endregion
    }
}
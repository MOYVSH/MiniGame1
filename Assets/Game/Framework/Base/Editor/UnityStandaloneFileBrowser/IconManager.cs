using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityStandaloneFileBrowser
{
    public static class IconManager
    {
        private static readonly Dictionary<string, GUIContent> IconData = new Dictionary<string, GUIContent>();

        /// <summary>
        /// 获取unity的内置icon
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="fileType">文件类型</param>
        /// <returns>根据文件类型获取对应内置icon，创建GUIContent并返回</returns>
        public static GUIContent GetIconByType(FileBrowserEnum.FileType fileType)
        {
            var iconName = fileType switch {
                FileBrowserEnum.FileType.Director => "d_project",
                FileBrowserEnum.FileType.CSharp => "cs Script Icon",
                FileBrowserEnum.FileType.Music => "AudioClip Icon",
                FileBrowserEnum.FileType.Prefab => "Prefab Icon",
                _ => "d_UnityEditor.ConsoleWindow"
            };
            return GetGUIContentByIconName(iconName);
        }

        public static GUIContent GetGUIContentByIconName(string iconName)
        {
            if (IconData.TryGetValue(iconName, out var guiContent)) return guiContent;
            guiContent = EditorGUIUtility.IconContent(iconName);
            IconData[iconName] = guiContent;
            return guiContent;
        }

        public static GUIContent GetGUIContentBySortState(FileBrowserEnum.SortState state)
        {
            return state switch {
                FileBrowserEnum.SortState.Up => EditorGUIUtility.IconContent("CollabPush"),
                FileBrowserEnum.SortState.Down => EditorGUIUtility.IconContent("CollabPull"),
                _ => new GUIContent()
            };
        }
    }
}
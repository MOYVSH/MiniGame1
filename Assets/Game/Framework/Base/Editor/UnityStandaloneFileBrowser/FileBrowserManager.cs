using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityStandaloneFileBrowser
{
    public static class FileBrowserManager
    {
        [MenuItem("Tools/FileBrowser")]
        static void TestOpen()
        {
            OpenFilePanel(openType: FileBrowserEnum.OpenType.All);
        }

        /// <summary>
        /// 打开选择文件的弹窗
        /// </summary>
        /// <param name="extensionStr">文件的后缀列表</param>
        /// <param name="call">带有所选文件地址的回调</param>
        /// <param name="openType">仅文件；仅文件夹；所有文件</param>
        public static void OpenFilePanel(List<string> extensionStr = null, Action<List<string>> call = null,
            FileBrowserEnum.OpenType openType = FileBrowserEnum.OpenType.OnlyFile)
        {
            FileBrowserWindow.OpenFileBrowserWindow(extensionStr, call, openType);
        }

        public static void FileElementSort(List<FileElement> elements, bool compareForType = true)
        {
            QuickSort(elements, 0, elements.Count - 1, compareForType);
        }

        private static void QuickSort(List<FileElement> data, int start, int end, bool compareForType = true)
        {
            if (start >= end)
                return;
            var i = start;
            var j = end;
            var baseValue = data[start];
            var index = start;
            while (i < j)
            {
                while (i < j)
                {
                    if (CompareFileElement(data[j], baseValue, compareForType))
                    {
                        data[i] = data[j];
                        index = j;
                        i++;
                        break;
                    }
                    j--;
                }
                while (i < j)
                {
                    if (CompareFileElement(baseValue, data[i], compareForType))
                    {
                        data[j] = data[i];
                        index = i;
                        j--;
                        break;
                    }
                    i++;
                }
            }
            data[index] = baseValue;
            QuickSort(data, start, index - 1, compareForType);
            QuickSort(data, index + 1, end, compareForType);
        }

        private static bool CompareFileElement(FileElement element1, FileElement element2, bool compareType = true)
        {
            if (!compareType) return CompareName(element1.fileName, element2.fileName);
            if ((int) element1.fileType == (int) element2.fileType)
            {
                return CompareName(element1.fileName, element2.fileName);
            }
            return (int) element1.fileType <= (int) element2.fileType;
        }

        /// <summary>
        /// 比较字符串的char来决定哪个排在前面
        /// </summary>
        /// <returns>a在b前面，返回true，否则返回false</returns>
        private static bool CompareName(string a, string b)
        {
            var aChar = a.ToCharArray();
            var bChar = b.ToCharArray();
            for (var i = 0; i < aChar.Length; i++)
            {
                if (bChar.Length <= i)
                    return false;
                if (aChar[i] == bChar[i])
                    continue;
                return aChar[i] < bChar[i];
            }
            return true;
        }
    }
}
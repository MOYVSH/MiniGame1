using System;
using System.IO;

namespace UnityStandaloneFileBrowser
{
    [Serializable]
    public class FileElement
    {
        public string fileName;
        public string filePath;
        public bool isSelect;
        public FileBrowserEnum.FileType fileType;

        public FileElement(string path)
        {
            filePath = path.Replace("\\","/");
            fileName = Path.GetFileName(path);
            isSelect = true;
            GetFileType();
        }

        private void GetFileType()
        {
            if (Directory.Exists(filePath))
                fileType = FileBrowserEnum.FileType.Director;
            else if (filePath.EndsWith(".cs"))
                fileType = FileBrowserEnum.FileType.CSharp;
            else if (filePath.EndsWith(".wav") || filePath.EndsWith(".ogg") || filePath.EndsWith(".mp3"))
                fileType = FileBrowserEnum.FileType.Music;
            else if (filePath.EndsWith(".prefab"))
                fileType = FileBrowserEnum.FileType.Prefab;
            else
                fileType = FileBrowserEnum.FileType.Other;
        }
    }
}
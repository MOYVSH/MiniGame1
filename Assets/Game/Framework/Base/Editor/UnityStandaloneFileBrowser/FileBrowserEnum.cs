namespace UnityStandaloneFileBrowser
{
    public static class FileBrowserEnum
    {
        public enum OpenType
        {
            OnlyFile,
            OnlyFolder,
            All
        }

        public enum FileType
        {
            Director,
            CSharp,
            Prefab,
            Music,
            Other
        }

        public enum SortState
        {
            None,
            Up,
            Down
        }
    }
}
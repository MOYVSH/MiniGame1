using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityStandaloneFileBrowser
{
    public class FileBrowserWindow : EditorWindow
    {
        //后缀列表
        private static List<string> _extensionStr;

        private static Action<List<string>> _returnSelectPaths;

        private static FileBrowserEnum.OpenType _openType;

        //选择的文件夹
        private string _currentSelectFolderPath;
        //最终选择的文件
        private List<string> _selectFilesPath;

        [SerializeField]
        private List<FileElement> fileElements;

        private bool _isFinishInit;
        private SerializedObject _serializedObject;
        private SerializedProperty _elementList;

        private Event _currentEvent;

        public static void OpenFileBrowserWindow(List<string> extensionStr = null,
            Action<List<string>> call = null, FileBrowserEnum.OpenType openType = FileBrowserEnum.OpenType.OnlyFile)
        {
            _extensionStr = extensionStr;
            _returnSelectPaths = call;
            _openType = openType;
            var window = GetWindow<FileBrowserWindow>();
            window.Focus();
            window.Repaint();
        }

        private void OnGUI()
        {
            if (!_isFinishInit)
                Init();
            DrawSelectFolderPath();
            DrawControlBtn();
            DrawSortBtn();
            DrawReorderList();
        }

        private void Init()
        {
            _isFinishInit = true;
            _currentEvent = Event.current;
            _currentSelectFolderPath = EditorPrefs.GetString("FileBrowserSelectFolder", Application.dataPath);
            fileElements = new List<FileElement>();
            InitSelectFolderPath();
            GetFilesPath();
            _serializedObject = new SerializedObject(this);
            _elementList = _serializedObject.FindProperty("fileElements");
            CreateReorderableList();
        }

        #region 存储打开的文件夹路径
        //存储上层文件夹的路径
        private List<string> _preFolderPath;
        //存储下层文件夹的路径
        private List<string> _nextFolderPath;
        //当前层数
        private int _currentLayer;

        private void InitSelectFolderPath()
        {
            _preFolderPath = new List<string>();
            _nextFolderPath = new List<string>();
            _currentLayer = 0;
        }

        private void ResetSelectFolderPath()
        {
            _preFolderPath.Clear();
            _nextFolderPath.Clear();
            _currentLayer = 0;
            _startIndex = -1;
        }
        /// <summary>
        /// 通过返回按钮返回到上一层级文件夹
        /// </summary>
        private void MoveToPrePath()
        {
            _currentLayer--;
            var path = _preFolderPath[_currentLayer];
            _nextFolderPath.Add(_currentSelectFolderPath);
            _startIndex = -1;
            RefreshSelectFolder(path);
        }
        /// <summary>
        /// 通过前进按钮进入下一层级文件夹
        /// </summary>
        private void MoveToNextPath()
        {
            var path = _nextFolderPath[^1];
            _nextFolderPath.Remove(path);
            _currentLayer++;
            _startIndex = -1;
            RefreshSelectFolder(path);
        }
        /// <summary>
        /// 双击进入下一层级文件夹
        /// </summary>
        private void MoveToNextPath(string path)
        {
            if (_preFolderPath.Count == _currentLayer)
                _preFolderPath.Add(_currentSelectFolderPath);
            else
                _preFolderPath[_currentLayer] = _currentSelectFolderPath;
            _currentLayer++;
            _nextFolderPath.Clear();
            _startIndex = -1;
            RefreshSelectFolder(path);
        }
        #endregion

        private void GetFilesPath()
        {
            fileElements.Clear();
            if (string.IsNullOrEmpty(_currentSelectFolderPath))
                return;
            var filesPath = _openType switch {
                FileBrowserEnum.OpenType.OnlyFile => Directory.GetFiles(_currentSelectFolderPath).Where(FileFilter).ToList(),
                FileBrowserEnum.OpenType.OnlyFolder => Directory.GetDirectories(_currentSelectFolderPath).Where(FileFilter).ToList(),
                _ => Directory.GetFileSystemEntries(_currentSelectFolderPath).Where(FileFilter).ToList()
            };
            for (var i = 0; i < filesPath.Count; i++)
            {
                var path = filesPath[i];
                if(path.EndsWith(".meta"))
                    continue;
                var element = new FileElement(path);
                fileElements.Add(element);
            }

            FileBrowserManager.FileElementSort(fileElements);
            _typeSortState = FileBrowserEnum.SortState.Up;
            _hasTypeSort = true;
        }

        private bool FileFilter(string p)
        {
            if (_extensionStr == null || _extensionStr.Count == 0 || Directory.Exists(p))
                return true;
            foreach (var str in _extensionStr)
            {
                if (p.EndsWith(str))
                    return true;
            }
            return false;
        }

        private void DrawSelectFolderPath()
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            var temp = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            EditorGUILayout.TextField("Folder Path :", _currentSelectFolderPath);
            GUI.enabled = true;
            EditorGUIUtility.labelWidth = temp;
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_project"), GUILayout.Width(40),GUILayout.Height(19)))
            {
                var selectPath = EditorUtility.OpenFolderPanel("Select Folder", _currentSelectFolderPath, "");
                if (selectPath != _currentSelectFolderPath)
                {
                    RefreshSelectFolder(selectPath);
                    ResetSelectFolderPath();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void RefreshSelectFolder(string path)
        {
            ReorderableListDeselect();
            _currentSelectFolderPath = path;
            EditorPrefs.SetString("FileBrowserSelectFolder", _currentSelectFolderPath);
            GetFilesPath();
            ChangeData();
        }

        private void DrawControlBtn()
        {
            GUILayout.BeginHorizontal();
            float offset = 227;
            if (_preFolderPath.Count > 0)
            {
                DrawMovePathBtn();
                offset = 377;
            }
            if (GUILayout.Button("Select All", GUILayout.Width(70)))
            {
                foreach (var element in fileElements)
                    element.isSelect = true;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Clear All", GUILayout.Width(70)))
            {
                foreach (var element in fileElements)
                    element.isSelect = false;
            }

            offset = Mathf.Max(position.width - offset, 5);
            GUILayout.Space(offset);
            if (GUILayout.Button("确认选择", GUILayout.Width(70)))
            {
                _selectFilesPath = new List<string>();
                foreach (var element in fileElements.Where(element => element.isSelect))
                {
                    _selectFilesPath.Add(element.filePath);
                }
                _returnSelectPaths?.Invoke(_selectFilesPath);
                _serializedObject = null;
                Close();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawMovePathBtn()
        {
            GUI.enabled = _currentLayer > 0;
            if (GUILayout.Button("后退",GUILayout.Width(70)))
            {
                MoveToPrePath();
            }
            GUILayout.Space(5);
            GUI.enabled = _nextFolderPath.Count > 0;
            if (GUILayout.Button("前进",GUILayout.Width(70)))
            {
                MoveToNextPath();
            }
            GUI.enabled = true;
        }

        private bool _hasTypeSort;
        private bool _hasNameSort;
        private FileBrowserEnum.SortState _typeSortState;
        private FileBrowserEnum.SortState _nameSortState;
        private void DrawSortBtn()
        {
            GUILayout.BeginHorizontal();
            var typeContent = IconManager.GetGUIContentBySortState(_typeSortState);
            typeContent.text = "类型";
            if (GUILayout.Button(typeContent,GUILayout.Width(70)))
            {
                ChangeState(ref _typeSortState, ref _nameSortState);
                _hasNameSort = false;
                if (_hasTypeSort)
                    fileElements.Reverse();
                else
                {
                    _hasTypeSort = true;
                    FileBrowserManager.FileElementSort(fileElements);
                }
            }
            GUILayout.Space(5);
            var nameContent = IconManager.GetGUIContentBySortState(_nameSortState);
            nameContent.text = "名称";
            if (GUILayout.Button(nameContent,GUILayout.Width(70)))
            {
                ChangeState(ref _nameSortState, ref _typeSortState);
                _hasTypeSort = false;
                if (_hasNameSort)
                    fileElements.Reverse();
                else
                {
                    _hasNameSort = true;
                    FileBrowserManager.FileElementSort(fileElements, false);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void ChangeState(ref FileBrowserEnum.SortState state, ref FileBrowserEnum.SortState other)
        {
            if (state == FileBrowserEnum.SortState.Up)
                state = FileBrowserEnum.SortState.Down;
            else
                state = FileBrowserEnum.SortState.Up;
            other = FileBrowserEnum.SortState.None;
        }

#region ReorderableList
        private ReorderableList _reorderableList;
        void CreateReorderableList()
        {
            _selectElements = new List<int>();
            _reorderableList = new ReorderableList(_serializedObject, _elementList,
                false, false, false, false);
            _reorderableList.drawElementCallback += DrawElementCallback;
            _reorderableList.onSelectCallback += SelectCall;
        }

        private Vector2 _scrollPos;
        private void DrawReorderList()
        {
            if (_serializedObject == null)
                return;
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            ChangeData();
            GUILayout.EndScrollView();
        }

        private void ChangeData()
        {
            _serializedObject.Update();
            _reorderableList.DoLayoutList();
            _serializedObject.ApplyModifiedProperties();
        }

        #region Draw Expand
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = _elementList.GetArrayElementAtIndex(index);
            DrawSelectToggle(rect, index, item);
            DrawIcon(rect, item);
            DrawFileName(rect, item);
        }

        private void DrawSelectToggle(Rect rect, int index, SerializedProperty property)
        {
            var isSelect = property.FindPropertyRelative("isSelect");
            rect.width = 18;
            EditorGUI.BeginChangeCheck();
            isSelect.boolValue = EditorGUI.Toggle(rect, isSelect.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (IsSelect(index))
                {
                    ReorderableListSelect();
                    foreach (var s in _selectElements)
                    {
                        var item = _elementList.GetArrayElementAtIndex(s);
                        var hasSelect = item.FindPropertyRelative("isSelect");
                        hasSelect.boolValue = isSelect.boolValue;
                    }
                }
                else
                {
                    SelectElement(index);
                }
            }
        }

        private void DrawIcon(Rect rect, SerializedProperty property)
        {
            var fileType = (FileBrowserEnum.FileType) property.FindPropertyRelative("fileType").enumValueFlag;
            rect.x += 23;
            rect.width = 18;
            EditorGUI.LabelField(rect, IconManager.GetIconByType(fileType));
        }

        private void DrawFileName(Rect rect, SerializedProperty property)
        {
            rect.x += 48;
            rect.width -= 48;
            var fileName = property.FindPropertyRelative("fileName").stringValue;
            EditorGUI.LabelField(rect, fileName);
        }
        #endregion

        #region Select Expand
        private List<int> _selectElements;
        private int _startIndex = -1;
        private long _clickTime;
        private void SelectCall(ReorderableList list)
        {
#if UNITY_2021_2_OR_NEWER
            if (list.selectedIndices.Count <= 0)
                return;

            var selectID = list.selectedIndices[0];
#else
            var selectID = list.index;
#endif

            if (DoubleClick(selectID))
            {
                var element = fileElements[selectID];
                if (element.fileType == FileBrowserEnum.FileType.Director)
                {
                    MoveToNextPath(element.filePath);
                    return;
                }
            }

            _clickTime = DateTime.Now.Ticks;
            SelectElement(selectID);
        }

        private bool DoubleClick(int index)
        {
            if (_clickTime == 0)
                _clickTime = DateTime.Now.Ticks;
            return DateTime.Now.Ticks - _clickTime < 5000000 && _startIndex == index &&
                   !_currentEvent.shift && !DownControlBtn();
        }

        private void SelectElement(int index)
        {
            if (!DownControlBtn())
                ReorderableListDeselect();
            if (_currentEvent.shift)
            {
                var firstIndex = _startIndex;
                var secondIndex = index;
                if (_startIndex > index)
                {
                    firstIndex = index;
                    secondIndex = _startIndex;
                }
                for (var i = firstIndex; i <= secondIndex; i++)
                {
                    _selectElements.Add(i);
                }
                ReorderableListSelect();
                return;
            }
            _startIndex = index;
            if (DownControlBtn() && IsSelect(index))
            {
                _selectElements.Remove(index);
                _reorderableList.Deselect(index);
            }
            else
                _selectElements.Add(index);
            ReorderableListSelect();
        }

        private bool DownControlBtn()
        {
#if UNITY_EDITOR_WIN
            return _currentEvent.control;
#endif
            return _currentEvent.command;
        }

        private void ReorderableListDeselect()
        {
            _reorderableList.ClearSelection();
            _selectElements.Clear();
        }

        private void ReorderableListSelect()
        {
            if (_selectElements.Count <= 0)
                return;
            foreach (var index in _selectElements)
            {
                _reorderableList.Select(index, true);
            }
        }

        private bool IsSelect(int index)
        {
            return _selectElements.Contains(index);
        }
        #endregion
#endregion
    }
}
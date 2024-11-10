using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MOYV
{
    public class EditorScriptableSingleton<T> : ScriptableObject
        where T : ScriptableObject, ISerializationCallbackReceiver
    {
        protected static T instance;
        
        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    LoadOrCreate();
                }
                
                return instance;
            }
        }
        
        public static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                instance = (arr.Length > 0 ? arr[0] as T : instance) ?? CreateInstance<T>();
                instance.OnAfterDeserialize();
            }
            else
            {
                Debug.LogError($"{nameof(ScriptableSingleton<T>)}: 请指定单例存档路径！ ");
            }
            
            return instance;
        }
        
        public static void Save(bool saveAsText = true)
        {
            if (!instance)
            {
                Debug.LogError("Cannot save ScriptableSingleton: no instance!");
                return;
            }
            
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                
                UnityEngine.Object[] obj = new T[1] {instance};
                instance.OnBeforeSerialize();
                InternalEditorUtility.SaveToSerializedFileAndForget(obj, filePath, saveAsText);
            }
        }
        
        protected static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Cast<EditorScriptableObjectCreatePathAttribute>()
                .FirstOrDefault(v => v != null)
                ?.filePath;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorScriptableObjectCreatePathAttribute : Attribute
    {
        public string filePath;
        
        /// <summary>
        /// 单例存放路径
        /// </summary>
        /// <param name="path">相对 Project 路径</param>
        public EditorScriptableObjectCreatePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }
            
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }
            
            filePath = path;
        }
    }
}
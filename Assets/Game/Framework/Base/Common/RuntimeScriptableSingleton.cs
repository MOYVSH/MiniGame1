using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace MOYV
{
    public interface ISingleton
    {
        void Initialize();
    }

    public class RuntimeScriptableSingleton<T> : ScriptableObject, ISingleton where T : ScriptableObject, ISingleton
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    LoadOrCreate();
                }

                return _instance;
            }
        }

        public static void LoadOrCreate()
        {
            var assetName = typeof(T).Name;
            _instance = Resources.Load<T>(assetName);
            if (_instance)
            {
                _instance.Initialize();
                return;
            }
#if UNITY_EDITOR
            if (!_instance)
            {
                var filePath = $"Assets/Resources/{assetName}.asset";
                if (!File.Exists(filePath))
                {
                    Save(filePath);
                }

                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                _instance = (arr.Length > 0 ? arr[0] as T : _instance) ?? CreateInstance<T>();
                _instance.Initialize();
                return;
            }
#endif
            _instance = CreateInstance<T>();
            _instance.Initialize();
        }

        public static void Save(string filePath)
        {
            if (!_instance)
            {
                _instance = CreateInstance<T>();
            }

            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            var obj = new T[1] { _instance };
#if UNITY_EDITOR
            InternalEditorUtility.SaveToSerializedFileAndForget(obj, filePath, true);
#endif
        }

        public virtual void Initialize()
        {
        }
    }
}
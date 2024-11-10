using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace MOYV.AssetsCheckerBase
{
    [EditorScriptableObjectCreatePath("ProjectSettings/AssetCheckerData.asset")]
    public class AssetCheckerData : EditorScriptableSingleton<AssetCheckerData>, ISerializationCallbackReceiver
    {
        [FieldName("启用", "#3388FF")] [SerializeField]
        public bool IsEnabled;
        
        [FieldName("输出Log", "#3388FF")] [SerializeField]
        public bool IsEnableLog;

        #region rule check standard
        
        // Texture
        public enum ETextureSize
        {
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192,
            _16384 = 16384,
        }
        
        [Header("纹理和图集设置")] [FieldName("最大纹理尺寸", "#3388FF")]
        public ETextureSize TEXTURE_IMPORT_SIZE = ETextureSize._2048;
        
        [FieldName("最大纹理文件（MB）", "#3388FF")] public int TEXTURE_FILE_SIZE_LIMITATION = 2;
        
        [FieldName("图集最低利用率", "#3388FF")] [Range(0.1f, .9f)]
        public float ATLAS_USED_RATIO_MIN = .6f;
        
        [FieldName("检测利用率最小尺寸", "#3388FF")] public ETextureSize ATLAS_MIN_SIZE = ETextureSize._512;
        
        // Transparent
        [FieldName("单个像素透明判断阈值", "#3388FF")] [Range(0, 255)]
        public int TRANSPARENCY_MIN = 10;
        
        [FieldName("MipMaps设置", "#3388FF")] public bool MIPMAPS_SETTING = false;
        
        [FieldName("Android纹理压缩格式", "#3388FF")]
        public TextureImporterFormat ANDROID_COMPRESS_SETTING = TextureImporterFormat.ETC2_RGBA8Crunched;
        
        [FieldName("iOS纹理压缩格式", "#3388FF")]
        public TextureImporterFormat IOS_COMPRESS_SETTING = TextureImporterFormat.ASTC_6x6;
        
        // [Header("音效设置")]
        //
        // //Audio
        // [FieldName("超过15秒的AudioClip没有设置LoadType为Streaming", "#3388FF")]
        // public bool CheckIfLongAudioClipLoadTypeIsNotStreaming = true;
        //
        // [FieldName("AudioClip被错误设置为Force To Mono", "#3388FF")] 
        // public bool CheckIfAudioClipIsIncorrectlySetToMono = true;
        //
        // [FieldName("低于15秒的AudioClip是双声道", "#3388FF")]
        // public bool CheckIfShortAudioClipIsNotStereo = true;
        
        [Header("字体设置")] [FieldName("字体文件大小限制", "#3388FF")]
        public int FONT_FILE_SIZE_LIMITATION = 1024 * 1024 * 4;
        
        [Header("模型设置")] [FieldName("包围盒体积", "#3388FF")][Tooltip("包围盒的体积,对应长宽高的积")] [Range(1, 15000)]
        public int MAX_BOUNDS_VOLUME = 12000;

        [FieldName("带动画模型基准面数", "#3388FF")][Tooltip("基准面数, 默认值3000")][Min(1)]
        public int ANIMATED_MODEL_BASE_TRIANGLE_COUNT = 3000;
        
        [FieldName("带动画模型最小体积", "#3388FF")][Tooltip("最小体积, 默认值0.01")][Min(0.01f)]
        public float ANIMATED_MODEL_MINIMUM_VOLUME = 0.1f;
        
        [FieldName("带动画模型最小密度", "#3388FF")][Tooltip("最小密度, 默认值0.5")][Min(0.1f)]
        public float ANIMATED_MODEL_MINIMUM_DENSITY = 1f;
        
        [FieldName("普通模型基准面数", "#3388FF")][Tooltip("基准面数, 默认值800")][Min(1)]
        public int NORMAL_MODEL_BASE_TRIANGLE_COUNT = 800;
        
        [FieldName("普通模型最小体积", "#3388FF")][Tooltip("最小体积, 默认值0.0175")][Min(0.01f)]
        public float NORMAL_MODEL_MINIMUM_VOLUME = 0.0175f;
        
        [FieldName("普通模型最小密度", "#3388FF")][Tooltip("最小密度, 默认值0.4")][Min(0.1f)]
        public float NORMAL_MODEL_MINIMUM_DENSITY = 0.4f;

        [FieldName("是否检查UV2（光照烘焙不需要检查UV2）", "#3388FF")][Tooltip("最小密度, 默认值0.4")][Min(0.1f)]
        [SerializeField] public List<bool> MODEL_UVS_CHECK_ENABLE = new List<bool>()
        {
            false,
            false,
            true,
            true,
            true,
            true,
            true,
            true,
        };
        
        #endregion
        
        [SerializeField] public List<string> IgnoredFolders = new List<string>();
        [SerializeField] public List<RuleInfo> RuleInfos = new List<RuleInfo>();
        [SerializeField] public List<IgnoreAssetRule> IgnoredAssetRules = new List<IgnoreAssetRule>();
        
        [SerializeField] public BaseRule xxx;
        
        [Serializable]
        public class IgnoreAssetRule
        {
            [SerializeField] public string rule;
            [SerializeField] public Object asset;
            
            public override string ToString()
            {
                return $"{asset}_{rule}";
            }
        }
        
        [Serializable]
        public class RuleInfo
        {
            [SerializeField] public bool enable;
            [SerializeField] public string rule;
            [SerializeField] public string targetType;
            
            public string DisplayRule => $"[{targetType}]:{rule}";
        }
        
        static void RemoveNotIgnored()
        {
            if (instance != null && instance.IgnoredAssetRules != null)
            {
                for (var i = 0; i < instance.IgnoredAssetRules.Count; i++)
                {
                    var obj = instance.IgnoredAssetRules[i];
                    var assetPath = AssetDatabase.GetAssetPath(obj.asset);
                    
                    if (obj.asset == null || string.IsNullOrEmpty(assetPath))
                    {
//                        Debug.Log("-->" + assetPath + "\n" + obj.IsIgnored + "\n" +obj.Guid);
                        instance.IgnoredAssetRules.Remove(obj);
                    }
                }
            }
        }
        
        public static bool IsStrict
        {
            get => Instance.IsEnabled;
            set
            {
                Instance.IsEnabled = value;
                Save();
            }
        }
        
        public static void AddIgnoreFolder(string folder)
        {
            Instance.IgnoredFolders ??= new List<string>();
            
            if (Instance.IgnoredFolders.Contains(folder))
            {
                return;
            }
            
            Instance.IgnoredFolders.Add(folder);
            Save();
        }
        
        public static void AddIgnoreRule(string rule, Object asset)
        {
            Instance.IgnoredAssetRules ??= new List<IgnoreAssetRule>();
            
            var iar = Instance.IgnoredAssetRules.Find(t => t.rule == rule && t.asset == asset);
            if (iar != null)
            {
            }
            else
            {
                Instance.IgnoredAssetRules.Add(new IgnoreAssetRule()
                {
                    rule = rule,
                    asset = asset,
                });
            }
            
            Save();
        }
        
        public void DeleteIgnoredFolder(string folder)
        {
            Instance.IgnoredFolders ??= new List<string>();
            
            if (Instance.IgnoredFolders.Contains(folder))
            {
                Instance.IgnoredFolders.Remove(folder);
                Save();
            }
        }
        
        public void OnBeforeSerialize()
        {
            
        }
        
        public void OnAfterDeserialize()
        {
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            RemoveNotIgnored();
        }
#endif
    }
    
    [InitializeOnLoad]
    public static class EditorStatusWatcher
    {
        public static Action OnEditorFocused;
        static bool isFocused;
        
        static EditorStatusWatcher()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }
        
        static void Update()
        {
            if (isFocused != InternalEditorUtility.isApplicationActive)
            {
                isFocused = InternalEditorUtility.isApplicationActive;
                if (isFocused)
                {
                    AssetCheckerData.LoadOrCreate();
                    OnEditorFocused?.Invoke();
                }
            }
        }
    }
}
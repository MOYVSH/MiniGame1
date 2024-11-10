using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MOYV.AssetsCheckerBase
{
    public delegate CheckResult MFunc(Object asset, RuleAttribute attr, string path);
    
    
    public struct CheckResult
    {
        // 检查的对象
        public readonly Object Asset;
        public readonly string Info;
        public readonly bool IsPassed;
        public readonly RuleAttribute Attribute;
        public readonly Action<AssetImporter> ActionFix;
        public bool IsIgnored;
        public readonly float weight;
        
        public CheckResult(Object asset, RuleAttribute attr, string info, bool pass,
            Action<AssetImporter> actionFix = null,float weight = 1)
        {
            Asset = asset;
            Info = info;
            IsPassed = pass;
            Attribute = attr;
            ActionFix = actionFix;
            IsIgnored = false;
            this.weight = weight;
        }
        
        public static CheckResult Passed(string info)
        {
            return new CheckResult(null, null, info, true, null);
        }
        
        public void DoFix()
        {
            ActionFix?.Invoke(AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Asset)));
        }
    }
    
    public class BaseRule
    {
        public List<CheckResult> CheckResults = new();
        
        public static Dictionary<Type, List<RuleAttribute>> AttributeDict = new();
        public static List<RuleAttribute> Attributes = new();
        
        [InitializeOnLoadMethod]
        protected static void InitializeAttribute()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        var attrs =
                            method.GetCustomAttributes(typeof(RuleAttribute), true) as RuleAttribute[];
                        if (attrs.Length == 0)
                            continue;
                        
                        var cbm =
                            Delegate.CreateDelegate(typeof(MFunc), method, false) as
                                MFunc;
                        if (cbm == null)
                        {
                            Debug.LogError(string.Format("Method {0}.{1} takes the wrong arguments for a rule checker.",
                                type, method.Name));
                            continue;
                        }
                        
                        // try with a bare action
                        foreach (var rule in attrs)
                        {
                            rule.CheckFunc = cbm;
                            Attributes.Add(rule);
                            var t = rule.GetType();
                            if (AttributeDict.TryGetValue(t, out var v))
                            {
                                if (v == null)
                                {
                                    v = new List<RuleAttribute>();
                                }
                                
                                v.Add(rule);
                            }
                            else
                            {
                                AttributeDict[t] = new List<RuleAttribute>() {rule};
                            }

                            if (AssetCheckerData.Instance &&
                                AssetCheckerData.Instance.RuleInfos != null &&
                                AssetCheckerData.Instance.RuleInfos.All(ruleInfo => ruleInfo.rule != rule.RuleDescription))
                            {
                                AssetCheckerData.Instance.RuleInfos.Add(new AssetCheckerData.RuleInfo()
                                {
                                    enable = rule.EnabledByDefault,
                                    rule = rule.RuleDescription,
                                    targetType = rule.CheckTargetType
                                });
                                
                                AssetCheckerData.Save();
                            }
                        }
                    }
                }
            }
        }
        
        public static void AddThirdRule<T>(string ruleMsg,
            Func<Object, string, (bool, string, Action<AssetImporter>)> checkAction)
        {
            var t = typeof(RuleAttribute);
            
            
            CheckResult MyMethod(Object asset, RuleAttribute attr, string path)
            {
                if (checkAction != null)
                {
                    var result = checkAction(asset, path);
                    return new CheckResult(asset, attr, result.Item2, result.Item1, result.Item3);
                }
                
                return CheckResult.Passed("");
            }
            
            var rule = new RuleAttribute(ruleMsg)
            {
                CheckTargetType = typeof(T).Name,
                CheckFunc = MyMethod
            };
            
            if (AttributeDict.TryGetValue(t, out var l))
            {
                if (l == null)
                {
                    l = new List<RuleAttribute>();
                }
                
                l.Add(rule);
            }
            else
            {
                AttributeDict[t] = new List<RuleAttribute>() {rule};
            }
        }
        
        public virtual void Execute(string folder = "", Action onFinished = null)
        {
            Execute<RuleAttribute, Object>(folder, onFinished);
        }
        
        /// <summary>
        /// 收集不合规的文件
        /// </summary>
        /// <param name="folder"></param>
        public virtual void CollectNonConformingAssets(string folder = "")
        {
            CollectNonConformingAssets<RuleAttribute, Object>(folder);
        }
        
        protected void CollectNonConformingAssets<T1, T2>(string folder = "") where T1 : RuleAttribute where T2 : Object
        {
            if (!Application.isBatchMode)
            {
                if (string.IsNullOrEmpty(folder))
                {
                    EditorUtility.DisplayDialog("提示", "选择一个目录再进行操作", "好的");
                    return;
                }
                
                if (CheckIgnore(folder))
                {
                    EditorUtility.DisplayDialog("提示", "你选择了忽略的文件夹进行检查，所以我啥事都没做!", "好");
                    return;
                }
            }
            
            var attrs = AttributeDict[typeof(T1)];
            
            if (typeof(T2) == typeof(Object))
            {
                foreach (var attribute in attrs)
                {
                    if (string.IsNullOrEmpty(attribute.CheckTargetType))
                    {
                        continue;
                    }
                    
                    var assetPaths = AssetDatabase.FindAssets($" t:{attribute.CheckTargetType}", new[] {folder});
                    foreach (var t in assetPaths)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(t);
                        var asset = AssetDatabase.LoadAssetAtPath<T2>(assetPath);
                        if (asset == null || asset != null && AssetDatabase.IsSubAsset(asset)) continue;
                        Execute<T1>(new List<RuleAttribute>() {attribute}, asset, assetPath);
                    }
                }
            }
            else
            {
                var assetPaths = AssetDatabase.FindAssets($" t:{typeof(T2).Name}", new[] {folder});
                foreach (var t in assetPaths)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(t);
                    var asset = AssetDatabase.LoadAssetAtPath<T2>(assetPath);
                    if (asset == null || asset != null && AssetDatabase.IsSubAsset(asset)) continue;
                    Execute<T1>(attrs, asset, assetPath);
                }
            }
        }
        
        public void Execute<T1, T2>(string folder = "", Action onFinished = null)
            where T1 : RuleAttribute where T2 : Object
        {
            if (!Application.isBatchMode)
            {
                if (string.IsNullOrEmpty(folder))
                {
                    EditorUtility.DisplayDialog("提示", "选择一个目录再进行操作", "好的");
                    return;
                }
                
                if (CheckIgnore(folder))
                {
                    EditorUtility.DisplayDialog("提示", "你选择了忽略的文件夹进行检查，所以我啥事都没做!", "好");
                    return;
                }
            }
            
            var attrs = AttributeDict[typeof(T1)];
            
            if (typeof(T2) == typeof(Object))
            {
                foreach (var attribute in attrs)
                {
                    if (string.IsNullOrEmpty(attribute.CheckTargetType))
                    {
                        continue;
                    }
                    
                    
                    var assetPaths = AssetDatabase.FindAssets($" t:{attribute.CheckTargetType}", new[] {folder});
                    
                    AssetCheckerUtils.ActionWithProgress(assetPaths, t =>
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(t);
                        var asset = AssetDatabase.LoadAssetAtPath<T2>(assetPath);
                        if (asset == null || asset != null && AssetDatabase.IsSubAsset(asset)) return;
                        Execute<T1>(new List<RuleAttribute>() {attribute}, asset, assetPath);
                    }, onFinished);
                }
            }
            else
            {
                var assetPaths = AssetDatabase.FindAssets($" t:{typeof(T2).Name}", new[] {folder});
                
                AssetCheckerUtils.ActionWithProgress(assetPaths, t =>
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(t);
                    var asset = AssetDatabase.LoadAssetAtPath<T2>(assetPath);
                    if (asset == null || asset != null && AssetDatabase.IsSubAsset(asset)) return;
                    Execute<T1>(attrs, asset, assetPath);
                }, onFinished);
            }
        }
        
        public void Execute<T>(List<RuleAttribute> attrs, Object asset, string path) where T : RuleAttribute
        {
            foreach (var attr in attrs)
            {
                if (attr is not T rule) continue;
                if (CheckIgnore(asset, attr, path)) continue;
                var result = rule.CheckFunc.Invoke(asset, attr, path);
                if (!result.IsPassed)
                    CheckResults.Add(result);
                if (AssetCheckerData.Instance.IsEnableLog)
                {
                    rule.LogAssert(result, asset);
                }
            }
        }
        
        /// <summary>
        /// 获取建议文本
        /// </summary>
        /// <param name="now"></param>
        /// <param name="expect"></param>
        /// <returns></returns>
        protected static string GetRecommendedText(object now, object expect)
        {
            return string.Format($"建议：{now} => {expect}");
        }
        
        protected bool CheckIgnore(string folder)
        {
            return AssetCheckerData.Instance.IgnoredFolders.Any(s => { return folder.Contains(s); });
        }
        
        protected bool CheckIgnore(Object asset, RuleAttribute attr, string path)
        {
            var info = AssetCheckerData.Instance.RuleInfos.FirstOrDefault(t => t.rule == attr.RuleDescription);
            if (info != null && !info.enable)
            {
                return true;
            }
            
            if (AssetCheckerData.Instance.IgnoredFolders.Any(s => { return path.Contains(s); }))
            {
                return true;
            }
            
            var fitRule = AssetCheckerData.Instance.IgnoredAssetRules.Where(t =>
                t.rule == attr.RuleDescription && t.asset == asset);
            if (fitRule.Any())
            {
                return true;
            }
            
            return false;
        }
    }
}
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MOYV.AssetsCheckerBase
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuleAttribute : Attribute
    {
        /// <summary>
        /// 规则描述
        /// </summary>
        public readonly string RuleDescription;
        
        /// <summary>
        /// 检查目标类型
        /// </summary>
        public string CheckTargetType;
        
        /// <summary>
        /// 检查方法
        /// </summary>
        public MFunc CheckFunc;

        /// <summary>
        /// 是否默认启用
        /// </summary>
        public bool EnabledByDefault;

        public RuleAttribute(string str, bool enabledByDefault = true)
        {
            RuleDescription = str;
            EnabledByDefault = enabledByDefault;
        }

        public void LogAssert(CheckResult cr, Object asset)
        {
            if (Equals(cr, default(CheckResult)))
            {
                return;
            }

            if (asset)
            {
                Debug.Assert(cr.IsPassed, RuleDescription + " : " + asset.name + "\n" + cr.Info, asset);
            }
        }
    }
}
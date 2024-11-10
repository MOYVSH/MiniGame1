using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NUnityExtends
{
    /// <summary>
    /// 带图集的image组件
    /// </summary>
    [AddComponentMenu("NUnityExtends/UI/NImage")]
    public class NImage : Image
    {
        /// <summary>
        /// 设置图集的sprite
        /// </summary>
        /// <param name="spriteName"></param>
        public void SetSprite(string spriteName)
        {
            if (sprite != null && sprite.name == spriteName)
            {
                return;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            sprite = null;
            overrideSprite = null;
        }
    }

}

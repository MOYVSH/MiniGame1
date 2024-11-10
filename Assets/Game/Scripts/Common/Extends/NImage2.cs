using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnitySpriteAtlas = UnityEngine.U2D.SpriteAtlas;

namespace NUnityExtends
{
    /// <summary>
    /// 带图集的image组件(使用Unity的图集)
    /// </summary>
    [AddComponentMenu("NUnityExtends/UI/NImage2")]
    public class NImage2 : Image
    {
        [SerializeField]
        private UnitySpriteAtlas m_Atlas;

        public UnitySpriteAtlas atlas
        {
            get { return m_Atlas; }
            set { m_Atlas = value; }
        }

        /// <summary>
        /// 设置图集的sprite
        /// </summary>
        /// <param name="spriteName"></param>
        public void SetSprite(string spriteName)
        {
            if (atlas != null)
            {
                overrideSprite = atlas.GetSprite(spriteName);
            }
        }
    }

}

using UnityEngine;
using UnityEngine.UI;

namespace NUnityExtends
{
    [AddComponentMenu("UI/ExtendImage")]
    public class ExtendImage : NImage
    {
        [SerializeField]
        private bool m_SlicedClipMode = false;

        protected override void OnPopulateMesh(VertexHelper vh)   
        {
            switch (type)
            {
                case Type.Filled when m_SlicedClipMode && (fillMethod == FillMethod.Horizontal || fillMethod == FillMethod.Vertical) && hasBorder:
                    GenerateSlicedSprite(vh);
                    break;
                default:
                    base.OnPopulateMesh(vh);
                    break;
            }
        }

        private Vector2[] s_VertScratch = new Vector2[4];
        private Vector2[] s_UVScratch = new Vector2[4];

        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            var activeSprite = overrideSprite ?? sprite;

            Vector4 outer, inner, padding, border;

            if (activeSprite != null)
            {
                outer = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = UnityEngine.Sprites.DataUtility.GetInnerUV(activeSprite);
                padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite);
                border = activeSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }
            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
            padding = padding / pixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            float xLength = s_VertScratch[3].x - s_VertScratch[0].x;
            float yLength = s_VertScratch[3].y - s_VertScratch[0].y;
            float len1XRatio = (s_VertScratch[1].x - s_VertScratch[0].x) / xLength;
            float len1YRatio = (s_VertScratch[1].y - s_VertScratch[0].y) / yLength;
            float len2XRatio = (s_VertScratch[2].x - s_VertScratch[1].x) / xLength;
            float len2YRatio = (s_VertScratch[2].y - s_VertScratch[1].y) / yLength;
            float len3XRatio = (s_VertScratch[3].x - s_VertScratch[2].x) / xLength;
            float len3YRatio = (s_VertScratch[3].y - s_VertScratch[2].y) / yLength;
            int xLen = 3, yLen = 3;
            if (fillMethod == FillMethod.Horizontal)
            {
                if (fillAmount >= (len1XRatio + len2XRatio))
                {
                    float ratio = 1 - (fillAmount - (len1XRatio + len2XRatio)) / len3XRatio;
                    s_VertScratch[3].x = s_VertScratch[3].x - (s_VertScratch[3].x - s_VertScratch[2].x) * ratio;
                    s_UVScratch[3].x = s_UVScratch[3].x - (s_UVScratch[3].x - s_UVScratch[2].x) * ratio;
                }
                else if (fillAmount >= len1XRatio)
                {
                    xLen = 2;
                    float ratio = 1 - (fillAmount - len1XRatio) / len2XRatio;
                    s_VertScratch[2].x = s_VertScratch[2].x - (s_VertScratch[2].x - s_VertScratch[1].x) * ratio;
                }
                else
                {
                    xLen = 1;
                    float ratio = 1 - fillAmount / len1XRatio;
                    s_VertScratch[1].x = s_VertScratch[1].x - (s_VertScratch[1].x - s_VertScratch[0].x) * ratio;
                    s_UVScratch[1].x = s_UVScratch[1].x - (s_UVScratch[1].x - s_UVScratch[0].x) * ratio;
                }
            }
            else if (fillMethod == FillMethod.Vertical)
            {
                if (fillAmount >= (len1YRatio + len2YRatio))
                {
                    float ratio = 1 - (fillAmount - (len1YRatio + len2YRatio)) / len3YRatio;
                    s_VertScratch[3].y = s_VertScratch[3].y - (s_VertScratch[3].y - s_VertScratch[2].y) * ratio;
                    s_UVScratch[3].y = s_UVScratch[3].y - (s_UVScratch[3].y - s_UVScratch[2].y) * ratio;
                }
                else if (fillAmount >= len1YRatio)
                {
                    yLen = 2;
                    float ratio = 1 - (fillAmount - len1YRatio) / len2YRatio;
                    s_VertScratch[2].y = s_VertScratch[2].y - (s_VertScratch[2].y - s_VertScratch[1].y) * ratio;
                }
                else
                {
                    yLen = 1;
                    float ratio = 1 - fillAmount / len1YRatio;
                    s_VertScratch[1].y = s_VertScratch[1].y - (s_VertScratch[1].y - s_VertScratch[0].y) * ratio;
                    s_UVScratch[1].y = s_UVScratch[1].y - (s_UVScratch[1].y - s_UVScratch[0].y) * ratio;
                }
            }

            toFill.Clear();

            for (int x = 0; x < xLen; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < yLen; ++y)
                {
                    if (!fillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;


                    AddQuad(toFill,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
                }
            }
        }

        static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }
    }
}


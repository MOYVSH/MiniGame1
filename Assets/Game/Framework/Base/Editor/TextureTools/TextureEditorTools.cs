using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace MOYV
{
    public static class TextureEditorTools
    {
        const int LOW_ALPHA = 1;

        public static Texture2D FixTransparency3(this Texture2D tex)
        {
            int w = tex.width;
            int h = tex.height;
            int max = (int)Mathf.Sqrt(Mathf.Max(w, h));
            var imageInvisible2 = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
            var temp = new Texture2D(w, h, TextureFormat.ARGB32, false);
            var rectOld = new Rect(0, 0, w, h);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, w, h, 0);
            Graphics.SetRenderTarget(imageInvisible2);
            for (int i = 0; i <= max; i++)
            {
                var rectUL = new Rect(i, i, w, h);
                var rectU = new Rect(0, i, w, h);
                var rectUR = new Rect(-i, i, w, h);
                var rectR = new Rect(-i, 0, w, h);
                var rectDR = new Rect(-i, -i, w, h);
                var rectD = new Rect(0, -i, w, h);
                var rectDL = new Rect(i, -i, w, h);
                var rectL = new Rect(i, 0, w, h);

                Graphics.DrawTexture(rectUL, tex);
                Graphics.DrawTexture(rectU, tex);
                Graphics.DrawTexture(rectUR, tex);
                Graphics.DrawTexture(rectR, tex);
                Graphics.DrawTexture(rectDR, tex);
                Graphics.DrawTexture(rectD, tex);
                Graphics.DrawTexture(rectDL, tex);
                Graphics.DrawTexture(rectL, tex);
            }

            temp.ReadPixels(rectOld, 0, 0);
            GameObject.DestroyImmediate(imageInvisible2);
            Graphics.SetRenderTarget(null);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var p = temp.GetPixel(x, y);
                    var s = tex.GetPixel(x, y);
                    if (Math.Abs(p.a) > Mathf.Epsilon && Math.Abs(s.a) < Mathf.Epsilon)
                    {
                        p.a = LOW_ALPHA / 255f;
                        tex.SetPixel(x, y, p);
                    }
                }
            }

            return tex;
        }


        public static Texture2D FixTransparency2(this Texture2D tex)
        {
            int w = tex.width, h = tex.height, all = w * h;
            Color32[] colors = tex.GetPixels32(), cs = new Color32[all];
            bool[] hasColor = new bool[all], newHasColor = new bool[all];

            for (int i = 0; i < all; i++)
            {
                if (colors[i].a != 0)
                {
                    hasColor[i] = true;
                    newHasColor[i] = true;
                    cs[i] = colors[i];
                }
            }

            int count = 2;

            for (int k = 0; k < count; k++)
            {
                for (int i = 0; i < all; i++)
                {
                    if (!hasColor[i])
                    {
                        int cnt = 0, r = 0, g = 0, b = 0;
                        if (i > 1 && hasColor[i - 1])
                        {
                            Color32 c = colors[i - 1];
                            r += c.r;
                            g += c.g;
                            b += c.b;
                            cnt++;
                        }

                        if (i < all - 1 && hasColor[i + 1])
                        {
                            Color32 c = colors[i + 1];
                            r += c.r;
                            g += c.g;
                            b += c.b;
                            cnt++;
                        }

                        if (i > w && hasColor[i - w])
                        {
                            Color32 c = colors[i - w];
                            r += c.r;
                            g += c.g;
                            b += c.b;
                            cnt++;
                        }

                        if (i < all - w - 1 && hasColor[i + w])
                        {
                            Color32 c = colors[i + w];
                            r += c.r;
                            g += c.g;
                            b += c.b;
                            cnt++;
                        }

                        if (cnt > 0)
                        {
                            cs[i] = new Color32((byte)(r / cnt), (byte)(g / cnt), (byte)(b / cnt), LOW_ALPHA);
                            newHasColor[i] = true;
                        }
                    }
                }

                for (int i = 0; i < all; i++)
                {
                    colors[i] = cs[i];
                    hasColor[i] = newHasColor[i];
                }
            }

            var texture = tex.Clone(w, h, colors);
            return texture;
        }

        //=========================================================================
        /// <summary>
        /// Copy the values of adjacent pixels to transparent pixels color info, to
        /// remove the white border artifact when importing transparent .PNGs.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D FixTransparency(this Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            int w = texture.width;
            int h = texture.height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    Color32 pixel = pixels[idx];
                    if (pixel.a == 0)
                    {
                        pixel.a = (byte)LOW_ALPHA;
                        bool done = false;
                        if (!done && x > 0)
                        {
                            done = TryAdjacent(ref pixel, pixels[idx - 1]); // Left   pixel
                        }

                        if (!done && x < w - 1)
                        {
                            done = TryAdjacent(ref pixel, pixels[idx + 1]); // Right  pixel
                        }

                        if (!done && y > 0)
                        {
                            done = TryAdjacent(ref pixel, pixels[idx - w]); // Top    pixel
                        }

                        if (!done && y < h - 1)
                        {
                            done = TryAdjacent(ref pixel, pixels[idx + w]); // Bottom pixel
                        }

                        pixels[idx] = pixel;
                    }
                }
            }

            var tex = texture.Clone(w, h, pixels);
            return tex;
        }

        //=========================================================================
        private static bool TryAdjacent(ref Color32 pixel, Color32 adjacent)
        {
            if (adjacent.a == 0) return false;

            pixel.r = adjacent.r;
            pixel.g = adjacent.g;
            pixel.b = adjacent.b;
            return true;
        }
#if UNITY_EDITOR
        static System.Collections.Generic.Dictionary<string, TextureImporterType> mOriginal =
            new Dictionary<string, TextureImporterType>();

        public static string MarkReadable(this Texture2D tex, bool readable = true, int size = 0)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            if (!string.IsNullOrEmpty(path))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

                if (textureImporter != null)
                {
                    if (textureImporter.isReadable != readable)
                    {
                        textureImporter.isReadable = readable;

                        if (readable)
                        {
                            mOriginal[path] = textureImporter.textureType;
#if UNITY_5_5_OR_NEWER
                            textureImporter.textureType = TextureImporterType.Default;
#else
					textureImporter.textureType = TextureImporterType.Image;
#endif
                        }
                        else
                        {
                            TextureImporterType type;

                            if (mOriginal.TryGetValue(path, out type))
                            {
                                textureImporter.textureType = type;
                                mOriginal.Remove(path);
                            }
                        }
                    }

                    if (size > 0)
                    {
                        var iOSSetting = textureImporter.GetPlatformTextureSettings("iPhone");
                        if (iOSSetting.overridden)
                        {
                            iOSSetting.maxTextureSize = size;
                            textureImporter.SetPlatformTextureSettings(iOSSetting);
                        }

                        var androidSetting = textureImporter.GetPlatformTextureSettings("Android");
                        if (androidSetting.overridden)
                        {
                            androidSetting.maxTextureSize = size;
                            textureImporter.SetPlatformTextureSettings(androidSetting);
                        }

                        textureImporter.npotScale = TextureImporterNPOTScale.None;
                        textureImporter.mipmapEnabled = false;
                        textureImporter.maxTextureSize = size;
                    }


                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }

            return path;
        }
#endif

        public static Texture2D Clone(this Texture2D tex, int width, int height,
            TextureFormat format = TextureFormat.RGBA32)
        {
            return new Texture2D(width, height, format, tex.mipmapCount > 0)
            {
                name = tex.name + "_clone283",
                filterMode = tex.filterMode,
                alphaIsTransparency = tex.alphaIsTransparency,
                wrapMode = tex.wrapMode,
                wrapModeU = tex.wrapModeU,
                wrapModeV = tex.wrapModeV,
                wrapModeW = tex.wrapModeW,
                anisoLevel = tex.anisoLevel,
            };
        }

        public static Texture2D Clone(this Texture2D tex, TextureFormat format = TextureFormat.RGBA32)
        {
            return new Texture2D(tex.width, tex.height, format, false)
            {
                name = tex.name + "_clone297",
                filterMode = tex.filterMode,
                alphaIsTransparency = tex.alphaIsTransparency,
                wrapMode = tex.wrapMode,
                wrapModeU = tex.wrapModeU,
                wrapModeV = tex.wrapModeV,
                wrapModeW = tex.wrapModeW,
                anisoLevel = tex.anisoLevel,
                minimumMipmapLevel = tex.minimumMipmapLevel,
                mipMapBias = tex.mipMapBias,
                hideFlags = HideFlags.DontSave
            };
        }

        public static Texture2D Clone(this Texture2D tex, TextureFormat format, bool markNonReadable = false)
        {
            try
            {
                var nt = new Texture2D(tex.width, tex.height, format, false)
                {
                    filterMode = tex.filterMode,
                    alphaIsTransparency = tex.alphaIsTransparency,
                    wrapMode = tex.wrapMode,
                    wrapModeU = tex.wrapModeU,
                    wrapModeV = tex.wrapModeV,
                    wrapModeW = tex.wrapModeW,
                    anisoLevel = tex.anisoLevel,
                    name = tex.name + "_clone324"
                };
                nt.LoadImage(tex.SafeEncodePNG());
                nt.Apply(false, markNonReadable);
                return nt;
            }
            catch (Exception e)
            {
                Debug.LogError("Clone error: " + e);
                return null;
            }
        }

        public static Texture2D Clone(this Texture2D tex, byte[] bytes, bool markNonReadable = false)
        {
            var t = tex.Clone();
            t.LoadImage(bytes, markNonReadable);
            t.Apply();
            return t;
        }

        public static Texture2D Clone(this Texture2D tex, Color32[] colors)
        {
            var t = tex.Clone();
            t.SetPixels32(colors);
            t.Apply();
            return t;
        }

        public static Texture2D Clone(this Texture2D tex, Color[] colors)
        {
            var t = tex.Clone();
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static Texture2D Clone(this Texture2D tex, int width, int height, Color[] colors)
        {
            var t = tex.Clone(width, height);
            t.name = tex.name + "_clone362";
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static Texture2D Clone(this Texture2D tex, int width, int height, Color32[] colors)
        {
            var t = tex.Clone(width, height);
            t.name = tex.name + "_clone371";

            t.SetPixels32(colors);
            t.Apply();
            return t;
        }

        public static Texture2D Clone(this Texture2D tex, int width, int height, Color32[] colors, int x,
            int y,
            int blockWidth, int blockHeight)
        {
            var t = tex.Clone(width, height);
            t.SetPixels32(x, y, blockWidth, blockHeight, colors);
            t.Apply();
            return t;
        }


        public static Texture2D Trim(this Texture2D oldTex
#if UNITY_EDITOR
            , int size
#endif
            , out Rect rect, float maxValue = 0f, bool avoidBleed = true)
        {
#if UNITY_EDITOR
            oldTex.MarkReadable(true, size);
#endif
            if (Mathf.Approximately(maxValue, 0)) maxValue = 1;

            // If we want to trim transparent pixels, there is more work to be done
            Color32[] pixels = oldTex.GetPixels32();

            int xmin = oldTex.width;
            int xmax = 0;
            int ymin = oldTex.height;
            int ymax = 0;
            int oldWidth = oldTex.width;
            int oldHeight = oldTex.height;

            for (int y = 0, yw = oldHeight; y < yw; ++y)
            {
                for (int x = 0, xw = oldWidth; x < xw; ++x)
                {
                    Color32 c = pixels[y * xw + x];

                    if (c.a >= maxValue)
                    {
                        if (y < ymin) ymin = y;
                        if (y > ymax) ymax = y;
                        if (x < xmin) xmin = x;
                        if (x > xmax) xmax = x;
                    }
                }
            }

            if (avoidBleed)
            {
                xmin = Mathf.Max(0, xmin - 4);
                ymin = Mathf.Max(0, ymin - 4);
                xmax = Mathf.Min(oldTex.width - 1, xmax + 4);
                ymax = Mathf.Min(oldTex.height - 1, ymax + 4);
            }

            int newWidth = (xmax - xmin) + 1;
            int newHeight = (ymax - ymin) + 1;


            rect = new Rect(xmin, ymin, newWidth, newHeight);
            if (newWidth > 0 && newHeight > 0)
            {
                // Copy the non-trimmed texture data into a temporary buffer
                Color[] newPixels = new Color[newWidth * newHeight];

                for (int y = 0; y < newHeight; ++y)
                {
                    for (int x = 0; x < newWidth; ++x)
                    {
                        int newIndex = y * newWidth + x;
                        int oldIndex = (ymin + y) * oldWidth + (xmin + x);
                        newPixels[newIndex] = pixels[oldIndex];
                    }
                }

                // Create a new texture
                var tex = oldTex.Clone(newWidth, newHeight, newPixels);
                return tex;
            }

            return null;
        }

        public static Texture2D TrimWithRect(this Texture2D oldTex
#if UNITY_EDITOR
            , int size
#endif
            , out Rect rect, Rect customRect)
        {
#if UNITY_EDITOR
            oldTex.MarkReadable(true, size);
#endif
            Color32[] pixels = oldTex.GetPixels32();

            int xmin = customRect.min.x > oldTex.width - 1 || customRect.min.x < 0 ? 0 : (int)customRect.min.x;
            int ymin = customRect.min.y > oldTex.height - 1 || customRect.min.y < 0 ? 0 : (int)customRect.min.y;
            int oldWidth = oldTex.width;
            int oldHeight = oldTex.height;

            int newWidth = customRect.width == 0 ? oldTex.width : (int)customRect.width;
            int newHeight = customRect.height == 0 ? oldTex.height : (int)customRect.height;
            
            rect = new Rect(xmin, ymin, newWidth, newHeight);
            if (newWidth > 0 && newHeight > 0)
            {
                // Copy the non-trimmed texture data into a temporary buffer
                Color[] newPixels = new Color[newWidth * newHeight];

                for (int y = 0; y < newHeight; ++y)
                {
                    for (int x = 0; x < newWidth; ++x)
                    {
                        int newIndex = y * newWidth + x;
                        int oldIndex = (ymin + y) * oldWidth + (xmin + x);
                        newPixels[newIndex] = pixels[oldIndex];
                    }
                }

                // Create a new texture
                var tex = oldTex.Clone(newWidth, newHeight, newPixels);
                return tex;
            }

            return null;
        }

        public static Texture2D AddOnePixelBorder(this Texture2D oldTex, ref Rect rect, int size = 2048)
        {
#if UNITY_EDITOR
            oldTex.MarkReadable(true, size);
#endif
            // If we want to trim transparent pixels, there is more work to be done
            Color32[] pixels = oldTex.GetPixels32();
            rect.x = rect.x - 1;
            rect.y = rect.y - 1;
            rect.width = oldTex.width + 2;
            rect.height = oldTex.height + 2;


            // Create a new texture
            var tex = oldTex.Clone(oldTex.width + 2, oldTex.height + 2, pixels, 1, 1, oldTex.width,
                oldTex.height);
            for (int i = 0; i < tex.width; i++)
            {
                tex.SetPixel(i, 0, Color.clear);
                tex.SetPixel(i, tex.height - 1, Color.clear);
            }

            for (int j = 0; j < tex.height; j++)
            {
                tex.SetPixel(0, j, Color.clear);
                tex.SetPixel(tex.width - 1, j, Color.clear);
            }

            return tex;
        }

        public static Texture2D Size4X4(this Texture2D texture2D, out Rect originRect, out Rect curRect)
        {
            int widthRemainder4 = texture2D.width % 4;
            int heightRemainder4 = texture2D.height % 4;
            originRect = new Rect(0, 0, texture2D.width, texture2D.height);
            curRect = new Rect(0, 0, texture2D.width, texture2D.height);
            if (widthRemainder4 != 0 || heightRemainder4 != 0)
            {
                Color32[] pixels = texture2D.GetPixels32();

                int addWidth = widthRemainder4 > 0 ? 4 - widthRemainder4 : 0;
                int addHeight = heightRemainder4 > 0 ? 4 - heightRemainder4 : 0;
                int newWidth = texture2D.width + addWidth;
                int newHeight = texture2D.height + addHeight;
                originRect = new Rect(0, addHeight, texture2D.width, texture2D.height);
                if (newWidth > 0 && newHeight > 0)
                {
                    Color[] newPixels = new Color[newWidth * newHeight];
                    for (int y = 0; y < newHeight; ++y)
                    {
                        for (int x = 0; x < newWidth; ++x)
                        {
                            bool needNewPixel = x >= newWidth - addWidth || y < addHeight;
                            int newIndex = y * newWidth + x;
                            if (needNewPixel)
                            {
                                int oldIndex =
                                    (y < addHeight ? 0 : y - addHeight) * texture2D.width +
                                    (x >= newWidth - addWidth ? newWidth - addWidth - 1 : x);
                                newPixels[newIndex] = pixels[oldIndex];
                            }
                            else
                            {
                                int oldIndex = (y - addHeight) * texture2D.width + x;
                                newPixels[newIndex] = pixels[oldIndex];
                                /*Debug.LogError("(" + y + " - " + ymin + ") * " + oldTex.width + " + (" + x + " - " +
                                               xmin +
                                               ") = " + oldIndex);*/
                            }
                        }
                    }

                    Texture2D newTexture2D = texture2D.Clone(newWidth, newHeight, newPixels);
                    Object.DestroyImmediate(texture2D);
                    curRect = new Rect(0, 0, newTexture2D.width, newTexture2D.height);
                    return newTexture2D;
                }
            }

            return texture2D;
        }
    }
}
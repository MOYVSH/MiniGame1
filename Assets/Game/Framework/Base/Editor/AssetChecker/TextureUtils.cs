using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MOYV.AssetsCheckerBase
{
    public static class TextureUtils
    {
        private static Dictionary<string, TextureImporterType> mOriginal = new();
        public static float GetOccupiedPercent(this Texture2D tex, int alphaThreshold = 10)
        {
            if (AssetDatabase.IsSubAsset(tex))
            {
                return -1;
            }

            var workTex = MarkReadableFromFileBytes(tex);

            if (!workTex)
            {
                return -1;
            }

            var r = CalculateOccupiedPercent(workTex, alphaThreshold);
            if (workTex != tex)
            {
                Object.DestroyImmediate(workTex);
            }

            return r;
        }

        public static float CalculateOccupiedPercent(Texture2D workTex, float alphaThreshold)
        {
            var pixels = workTex.GetPixels32();

            var w = workTex.width;
            var h = workTex.height;

            var count = 0;
            for (int y = 0, yw = h; y < yw; ++y)
            {
                for (int x = 0, xw = w; x < xw; ++x)
                {
                    var c = pixels[y * xw + x];

                    if (c.a >= alphaThreshold)
                    {
                        count++;
                    }
                }
            }


            return count * 1f / pixels.Length;
        }


        public static Texture2D MarkReadableViaRender(this Texture2D texture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            var tmp = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);


// Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);


// Backup the currently set RenderTexture
            var previous = RenderTexture.active;


// Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;


// Create a new readable Texture2D to copy the pixels to it
            var myTexture2D = new Texture2D(texture.width, texture.height);


// Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();


// Reset the active RenderTexture
            RenderTexture.active = previous;


// Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            return myTexture2D;
        }

        public static (Rect, Rect) GetEmptyArea(this Texture2D tex, int alphaThreshold = 10)
        {
            if (AssetDatabase.IsSubAsset(tex))
            {
                return default;
            }

            var workTex = MarkReadableFromFileBytes(tex);

            if (!workTex) return default;

            var r = CalculateEmptyArea(workTex, alphaThreshold);
            if (workTex != tex)
            {
                Object.DestroyImmediate(workTex);
            }

            return r;
        }

        private static Texture2D MarkReadableFromFileBytes(Texture2D tex)
        {
            var workTex = tex;
            if (!tex.isReadable)
            {
                var path = AssetDatabase.GetAssetPath(tex);

                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    workTex = new Texture2D(2, 2);
                    workTex.LoadImage(bytes, false);

                    if (workTex.width < 64 || workTex.height < 64)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return workTex;
        }

        public static (bool, int, int,int,int) GetPreferredSize4x4(int width, int height)
        {
            var widthRemainder4 = width % 4;
            var heightRemainder4 = height % 4;
            var isPrefer = widthRemainder4 == 0 && heightRemainder4 == 0;
            var addedWidth = widthRemainder4 > 0 ? 4 - widthRemainder4 : 0;
            var addedHeight = heightRemainder4 > 0 ? 4 - heightRemainder4 : 0;
            var newWidth = width + addedWidth;
            var newHeight = height + addedHeight;
            return (isPrefer, newWidth, newHeight,addedWidth,addedHeight);
        }

        public static (Rect, Rect) CalculateEmptyArea(Texture2D workTex, int alphaThreshold = 10)
        {
            var w = workTex.width;
            var h = workTex.height;
            var pixels = workTex.GetPixels32();

            var xmin = w;
            var ymin = h;
            var xmax = 0;
            var ymax = 0;
            var maxValue = alphaThreshold;
            if (Mathf.Approximately(maxValue, 0)) maxValue = 1;
            for (var y = 0; y < h; ++y)
            {
                for (var x = 0; x < w; ++x)
                {
                    var c = pixels[y * w + x];
                    if (c.a < maxValue) continue;
                    if (y < ymin) ymin = y;
                    if (y > ymax) ymax = y;
                    if (x < xmin) xmin = x;
                    if (x > xmax) xmax = x;
                }
            }

            return (Rect.MinMaxRect(0, 0, w, h), Rect.MinMaxRect(xmin, ymin, xmax, ymax));
        }
        
        public static string MarkReadable(this Texture2D tex, bool readable = true, int size = 0)
        {
            var path = AssetDatabase.GetAssetPath(tex);

            if (!string.IsNullOrEmpty(path))
            {
                var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

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
                            iOSSetting.maxTextureSize = 2048;
                            textureImporter.SetPlatformTextureSettings(iOSSetting);
                        }

                        var androidSetting = textureImporter.GetPlatformTextureSettings("Android");
                        if (androidSetting.overridden)
                        {
                            androidSetting.maxTextureSize = 2048;
                            textureImporter.SetPlatformTextureSettings(androidSetting);
                        }

                        textureImporter.npotScale = TextureImporterNPOTScale.None;
                        textureImporter.mipmapEnabled = false;
                        textureImporter.maxTextureSize = 2048;
                    }


                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }

            return path;
        }
    }
}
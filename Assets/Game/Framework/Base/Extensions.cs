using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MOYV.MODULE;
using UnityEngine;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MOYV
{
    public static class Extensions
    {
        public static string FormatPath(this string path)
        {
            return path.Replace('\\', '/').Replace("\\\\", "/").Replace("//", "/");
        }

        public static Texture2D Scale(this Texture2D src, float scale, Material mat = null)
        {
            if (scale >= 1)
            {
                if (scale > 1)
                    Debug.LogError("Error scale");

                return src;
            }

            var targetX = (int)(src.width * scale);
            var targetY = (int)(src.height * scale);
            return src.Resize(targetX, targetY, mat);
        }

#if UNITY_EDITOR
        public static Texture2D POT(this Texture2D src, TextureImporterNPOTScale nearType, Material mat = null)
        {
            var targetX = src.width;
            var targetY = src.height;
            (targetX, targetY) = PowerOfTwoRect(targetX, targetY, nearType);
            return Resize(src, targetX, targetY, mat);
        }
#endif

        public static Texture2D Resize(this Texture2D src, int targetX, int targetY, Material mat = null)
        {
            if (src == null) return null;
            var ori = RenderTexture.active;
            RenderTexture rt = new RenderTexture(targetX, targetY, 24, RenderTextureFormat.ARGB32);
            RenderTexture.active = rt;
            if (mat)
            {
                Graphics.Blit(src, rt, mat);
            }
            else
            {
                Graphics.Blit(src, rt);
            }

            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();
            RenderTexture.active = ori;
            rt.Release();
            Object.DestroyImmediate(rt);
            result.name = src.name;
            return result;
        }
#if UNITY_EDITOR
        public static Texture2D ScaleThenPot(this Texture2D src, float scale = 1,
            Material mat = null, TextureImporterNPOTScale _nearType = TextureImporterNPOTScale.None)
        {
            if (src == null) return null;
            if (scale >= 1 && _nearType == TextureImporterNPOTScale.None)
            {
                if (scale > 1)
                    Debug.LogError("Error scale");

                return src;
            }

            var targetX = (int)(src.width * scale);
            var targetY = (int)(src.height * scale);

            if (_nearType != TextureImporterNPOTScale.None)
            {
                (targetX, targetY) = PowerOfTwoRect(targetX, targetY, _nearType);
            }

            return Resize(src, targetX, targetY, mat);
        }
#endif
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();

            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string GetHierarchyPath(this GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }

            return path.Substring(1);
        }

        public static string GetHierarchyPath(this Transform obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.parent;
                path = "/" + obj.name + path;
            }

            return path;
        }

        public static Transform FindByHierarchyPath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] names = path.Split('/');
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            Transform parent = null;
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == names[0])
                {
                    parent = roots[i].transform;
                    break;
                }
            }

            int index = 1;
            while (parent != null && index < names.Length)
            {
                var child = parent.Find(names[index]);
                parent = child;

                index++;
            }

            return parent;
        }

        public static Texture2D CreateTextureFromAtlas(this Texture2D mainTexture, Rect rect, int fullWidth,
            int fullHeight)
        {
            var fullPixels = mainTexture.GetPixels32();
            int spriteX = (int)rect.x;
            int spriteY = (int)rect.y;
            int spriteWidth = (int)rect.width;
            int spriteHeight = (int)rect.height;

            int xmin = Mathf.Clamp(spriteX, 0, fullWidth);
            int ymin = Mathf.Clamp(spriteY, 0, fullHeight);
            int xmax = Mathf.Min(xmin + spriteWidth, fullWidth - 1);
            int ymax = Mathf.Min(ymin + spriteHeight, fullHeight - 1);
            int newWidth = Mathf.Clamp(spriteWidth, 0, fullWidth);
            int newHeight = Mathf.Clamp(spriteHeight, 0, fullHeight);

            if (newWidth == 0 || newHeight == 0) return null;

            Color32[] newPixels = new Color32[newWidth * newHeight];

            for (int y = 0; y < newHeight; ++y)
            {
                int cy = ymin + y;
                if (cy > ymax) cy = ymax;

                for (int x = 0; x < newWidth; ++x)
                {
                    int cx = xmin + x;
                    if (cx > xmax) cx = xmax;

                    int newIndex = y * newWidth + x;
                    int oldIndex = cy * fullWidth + cx;

                    newPixels[newIndex] = fullPixels[oldIndex];
                }
            }

            var tex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
            tex.SetPixels32(newPixels);
            return tex;
        }

        public static bool IsEmpty<T>(this ICollection<T> list)
        {
            if (list != null && list.Count > 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotEmpty<T>(this ICollection<T> list)
        {
            if (list != null && list.Count > 0)
            {
                return true;
            }

            return false;
        }

        public static byte[] SafeEncodePNG(this Texture2D tex)
        {
            var colors = tex.GetPixels32();
            var nt = new Texture2D(tex.width, tex.height);
            nt.SetPixels32(colors);
            var bytes = nt.EncodeToPNG();
            Object.DestroyImmediate(nt);
            return bytes;
        }

        public static Rect BestFit(this Texture tex, float width, float height, float scale = 1,
            bool isRotate = false)
        {
            float tw, th, x, y, texRatio;
            if (!isRotate)
            {
                texRatio = tex.width * 1f / tex.height;
            }
            else
            {
                texRatio = tex.height * 1f / tex.width;
            }

            if (width <= 0)
            {
                th = height * scale;
                tw = th * texRatio;
                y = (height - th) * .5f;
                x = (width - tw) * .5f;
            }
            else if (height <= 0)
            {
                tw = width * scale;
                th = tw / texRatio;
                y = (height - th) * .5f;
                x = (width - tw) * .5f;
            }
            else
            {
                float ratio = width / height;

                if (ratio >= texRatio)
                {
                    th = height * scale;
                    tw = th * texRatio;
                    y = (height - th) * .5f;
                    x = (width - tw) * .5f;
                }
                else
                {
                    tw = width * scale;
                    th = tw / texRatio;
                    y = (height - th) * .5f;
                    x = (width - tw) * .5f;
                }
            }


            Rect rect = new Rect(x, y, tw, th);

            return rect;
        }

        // public static void ChangeSkin(this SkeletonGraphic sg, string skinName)
        // {
        //     sg.Skeleton.SetSkin(skinName);
        //     sg.Skeleton.SetSlotsToSetupPose();
        //     sg.AnimationState.Apply(sg.Skeleton);
        //     sg.LateUpdate();
        // }

        public static Texture2D[] ToTextureArray(this List<Object> list)
        {
            var array = new Texture2D[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                array[i] = list[i] as Texture2D;
            }

            return array;
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour script) where T : Component
        {
            var result = script.GetComponent<T>();
            if (result == null)
            {
                result = script.gameObject.AddComponent<T>();
            }

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delimiter">lua端调用需要传入对应字符的ASCII码</param>
        /// <returns></returns>
        public static float[] ToFloatArray(string str, char delimiter)
        {
            var strs = str.Split(delimiter);
            var array = new float[strs.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = float.Parse(strs[i]);
            }

            return array;
        }

        public static float[] ToFloatArray(string str, string delimiter)
        {
            var chara = delimiter[0];
            return ToFloatArray(str, chara);
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delimiter">lua端调用需要传入对应字符的ASCII码</param>
        /// <returns></returns>
        public static int[] ToIntArray(string str, char delimiter)
        {
            var strs = str.Split(delimiter);
            var array = new int[strs.Length];
            for (int i = 0; i < array.Length; i++)
            {
                Debug.Log("===>" + strs[i] + "    with " + delimiter);
                array[i] = int.Parse(strs[i]);
            }

            return array;
        }

        public static int[] ToIntArray(string str, string delimiter)
        {
            var chara = delimiter[0];
            return ToIntArray(str, chara);
        }

        public static void OnUpClick(this GameObject go, Action<GameObject, PointerEventData> onClick,
            bool doScale = true)
        {
            // var key = go.GetInstanceID() + "_OnUpClick";
            EventTriggerListener.Get(go)
                .SetCommonClick(
                    (o, data) =>
                    {
                        if (doScale)
                            go.transform.DOScale(0.8f, .1f).SetEase(Ease.InBack).SetId("common_button_rotate");
                    },
                    ((o, data) =>
                    {
                        go.transform.DOScale(1, .1f).SetEase(Ease.OutBack)
                            .OnComplete((() =>
                            {
                                if (DOTween.IsTweening("common_button_rotate", true))
                                    return;
                                onClick(o, data);
                            })).SetDelay(.1f).SetId("common_button_rotate");
                    }));
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace("\\", "/").Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }

            Directory.Delete(source, true);
        }

        public static string GetDateString(this DateTime time)
        {
            return string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}",
                time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
        }

        public static Rect IntersectionWith(this Rect rect, Rect other)
        {
            var intersect = new Rect
            {
                min = Vector2.Max(rect.min, other.min),
                max = Vector2.Min(rect.max, other.max)
            };
            return intersect;
        }


        /// <summary>
        /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
        /// </summary>
        /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            Rect screenBounds =
                new Rect(0f, 0f, Screen.width,
                    Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            Vector3 tempScreenSpaceCorner; // Cached
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                tempScreenSpaceCorner =
                    camera.WorldToScreenPoint(
                        objectCorners[i]); // Transform world space position of corner to screen space
                if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                {
                    visibleCorners++;
                }
            }

            return visibleCorners;
        }

        /// <summary>
        /// Determines if this RectTransform is fully visible from the specified camera.
        /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
        }

        /// <summary>
        /// Determines if this RectTransform is at least partially visible from the specified camera.
        /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
        }


        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }


        private static readonly int[] mPowerOfTwo = new int[]
            { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        private static readonly int mPowerLength = 13;

#if UNITY_EDITOR
        public static (int, int) PowerOfTwoRect(int width, int height,
            TextureImporterNPOTScale _nearType = TextureImporterNPOTScale.None)
        {
            if (!IsPowerOfTwo(width))
            {
                width = NearPowerOfTwe(width, _nearType);
            }

            if (!IsPowerOfTwo(height))
            {
                height = NearPowerOfTwe(height, _nearType);
            }

            return (width, height);
        }
#endif

        private static bool IsPowerOfTwo(this int value)
        {
            if (value <= 1)
                return false;

            return ((value - 1) & value) == 0;
        }

        public static bool IsPowerOfFour(this int value)
        {
            if (value <= 0)
                return false;
            return (value & 3) == 0;
        }
#if UNITY_EDITOR
        public static int NearPowerOfTwe(this int value,
            TextureImporterNPOTScale nearType = TextureImporterNPOTScale.ToNearest)
        {
            if (value >= mPowerOfTwo[mPowerLength - 1])
            {
                return mPowerOfTwo[mPowerLength - 1];
            }

            var index = Convert.ToString(value, 2).Length - 2;
            if (index < 0)
            {
                return mPowerOfTwo[0];
            }

            if (nearType == TextureImporterNPOTScale.ToNearest)
            {
                index = (value - mPowerOfTwo[index] < mPowerOfTwo[index + 1] - value) ? index : index + 1;
            }
            else if (nearType == TextureImporterNPOTScale.ToLarger)
            {
                index = index + 1;
            }

            value = mPowerOfTwo[index];

            return value;
        }
#endif
        /// <summary>
        /// 0 不裁
        /// -1 全裁
        /// num  部分裁
        /// </summary>
        public static Texture2D TrimCustom(this Texture2D src, int up = 0, int down = 0, int left = 0,
            int right = 0)
        {
            var width = src.width;
            var height = src.height;

            var oldColors = src.GetPixels32();

            var (mUpMaxLen, mDownMaxLen, mLeftMaxLen, mRightMaxLen) = GetTrimMaxValue(src);

            up = up == -1 ? mUpMaxLen : up;
            down = down == -1 ? mDownMaxLen : down;
            left = left == -1 ? mLeftMaxLen : left;
            right = right == -1 ? mRightMaxLen : right;

            var newWidth = width - left - right;
            var newHeight = height - up - down;

            Color32[] newPixels = new Color32[newWidth * newHeight];

            var xIndex = left;
            var yIndex = down;

            for (int y = 0; y < newHeight; ++y)
            {
                for (int x = 0; x < newWidth; ++x)
                {
                    int newIndex = y * newWidth + x;
                    int oldIndex = (yIndex + y) * width + (xIndex + x);
                    newPixels[newIndex] = oldColors[oldIndex];
                }
            }

            var tex = new Texture2D(newWidth, newHeight) { name = src.name };
            tex.SetPixels32(newPixels);
            tex.Apply();
            return tex;
        }

        public static (int, int, int, int) GetTrimMaxValue(this Texture2D texture)
        {
            var tempTexture = texture;

            var pixels = tempTexture.GetPixels32();

            var xmin = int.MaxValue;
            var xmax = 0;
            var ymin = int.MaxValue;
            var ymax = 0;
            var oldWidth = tempTexture.width;
            var oldHeight = tempTexture.height;

            for (int y = 0, yw = oldHeight; y < yw; ++y)
            {
                for (int x = 0, xw = oldWidth; x < xw; ++x)
                {
                    Color32 c = pixels[y * xw + x];

                    if (c.a != 0)
                    {
                        if (y < ymin) ymin = y;
                        if (y > ymax) ymax = y;
                        if (x < xmin) xmin = x;
                        if (x > xmax) xmax = x;
                    }
                }
            }

            return (oldHeight - ymax - 1, ymin, xmin, oldWidth - xmax - 1);
        }
    }
}
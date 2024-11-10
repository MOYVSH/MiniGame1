using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// png处理
/// Author:zhouzhanglin
/// </summary>
public class PngProcess : Editor
{
    [MenuItem("Tools/Premulity Alpha(文件夹)", false, 64)]
    static void PremulityAlphaFolder()
    {
        if (Selection.activeObject is DefaultAsset)
        {
            Dictionary<string, string> texturePathKV = new Dictionary<string, string>();
            string dirPath = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);
            if (Directory.Exists(dirPath))
            {
                foreach (string path in Directory.GetFiles(dirPath))
                {
                    if (path.LastIndexOf(".meta") == -1)
                    {
                        if (System.IO.Path.GetExtension(path) == ".png")
                        {
                            int start = path.LastIndexOf("/") + 1;
                            int end = path.LastIndexOf(".png");
                            texturePathKV[path.Substring(start, end - start)] = path.Substring(6);
                            continue;
                        }
                    }
                }


                if (texturePathKV.Count > 0)
                {
                    foreach (string fileName in texturePathKV.Keys)
                    {
                        string path = texturePathKV[fileName];
                        Texture2D t = LoadPNG(Application.dataPath + "/" + path);
                        Color32[] colors = t.GetPixels32();
                        for (int i = 0; i < colors.Length; ++i)
                        {
                            Color32 c = colors[i];
                            c.r = (byte) ((c.a * c.r) / 255);
                            c.g = (byte) ((c.a * c.g) / 255);
                            c.b = (byte) ((c.a * c.b) / 255);
                            colors[i] = c;
                        }

                        t.SetPixels32(colors);
                        byte[] bytes = t.EncodeToPNG();
                        SavePNG(Application.dataPath + path, bytes);
                    }
                }
            }
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Premulity Alpha(图片)", false, 65)]
    static void PremulityAlphaFile()
    {
        if (Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0)
        {
            var guids = Selection.assetGUIDs;
            for (int inx = 0; inx < guids.Length; inx++)
            {
                var guid = guids[inx];

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                if (asset is Texture2D)
                {
                    string dirPath = AssetDatabase.GetAssetOrScenePath(asset);
                    if (File.Exists(dirPath) && dirPath.LastIndexOf(".meta") == -1 &&
                        System.IO.Path.GetExtension(dirPath) == ".png")
                    {
                        Debug.Log("premulity alpha--->"+assetPath);
//                        continue;
                        string path = dirPath.Substring(6);

                        Texture2D t = LoadPNG(Application.dataPath + path);
                        Color32[] colors = t.GetPixels32();
                        for (int i = 0; i < colors.Length; ++i)
                        {
                            Color32 c = colors[i];
                            c.r = (byte) ((c.a * c.r) / 255);
                            c.g = (byte) ((c.a * c.g) / 255);
                            c.b = (byte) ((c.a * c.b) / 255);
                            colors[i] = c;
                        }

                        t.SetPixels32(colors);
                        byte[] bytes = t.EncodeToPNG();
                        SavePNG(Application.dataPath + path, bytes);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }

    static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }

        return tex;
    }

    static void SavePNG(string filePath, byte[] fileData)
    {
        File.WriteAllBytes(filePath, fileData);
    }
}
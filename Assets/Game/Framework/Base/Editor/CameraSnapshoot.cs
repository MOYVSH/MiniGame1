using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
namespace MOYV
{
    public class CameraSnapshoot
    {
        static void _Snapshoot(int size)
        {
            if (Selection.gameObjects.Length != 1 || Selection.gameObjects[0].GetComponent<Camera>() == null)
            {
                EditorUtility.DisplayDialog("失败", ":::需要选中一个相机节点:::", "了解");
                return;
            }

            Camera camera;
            camera = Selection.gameObjects[0].GetComponent<Camera>();

            Texture2D     texture2d;
            RenderTexture renderTexture;
            
            texture2d = new UnityEngine.Texture2D(size, size, TextureFormat.RGB24, false);
            renderTexture = RenderTexture.GetTemporary(size, size, 24);

            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = null;

            RenderTexture.active = renderTexture;
            texture2d.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            texture2d.Apply(false);
            RenderTexture.active = null;

            string path;
            path = Application.dataPath + "/CameraSnapshoot.png";
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllBytes(path, texture2d.EncodeToPNG());

            RenderTexture.ReleaseTemporary(renderTexture);
            GameObject.DestroyImmediate(texture2d);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/CameraSnapshoot.png");
        }

        [MenuItem("Tools/相机截图/128x128")]
        public static void _Snapshoot128x128()
        {
            _Snapshoot(128);
        }
        [MenuItem("Tools/相机截图/256x256")]
        public static void _Snapshoot256x256()
        {
            _Snapshoot(256);
        }
        [MenuItem("Tools/相机截图/512x512")]
        public static void _Snapshoot512x512()
        {
            _Snapshoot(512);
        }
        [MenuItem("Tools/相机截图/1024x1024")]
        public static void _Snapshoot1024x1024()
        {
            _Snapshoot(1024);
        }
        [MenuItem("Tools/相机截图/2048x2048")]
        public static void _Snapshoot2048x2048()
        {
            _Snapshoot(2048);
        }
        [MenuItem("Tools/相机截图/4096x4096")]
        public static void _Snapshoot4096x4096()
        {
            _Snapshoot(4096);
        }
        
        [MenuItem("Tools/相机截图/Camera")]
        public static void _ScreenShot()
        {
            ScreenCapture.CaptureScreenshot(Application.dataPath + "/CameraSnapshoot.png");
        }
    }
}

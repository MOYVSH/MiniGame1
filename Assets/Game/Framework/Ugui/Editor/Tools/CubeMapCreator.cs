using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
namespace MOYV.UGUI.Editor
{
    public class CubeMapCreator
    {
        static void GenerateCubemap(int size)
        {
            if (Selection.gameObjects.Length != 1)
            {
                EditorUtility.DisplayDialog(":::环境贴图生成失败:::", ":::需要选中一个节点(有且仅有一个节点来做定位信息!):::", "了解");
                return;
            }

            Camera camera;
            camera                    = new GameObject("cmc").AddComponent<Camera>();
            camera.enabled            = false;
            camera.transform.position = Selection.gameObjects[0].transform.position;
            camera.transform.rotation = Selection.gameObjects[0].transform.rotation;

            Cubemap cm;
            cm = new Cubemap(size, TextureFormat.ARGB32, false);
            camera.RenderToCubemap(cm);
            Object.DestroyImmediate(camera.gameObject);

            string path;
            string scene;
            scene = Application.isPlaying ? SceneManager.GetActiveScene().name : EditorSceneManager.GetActiveScene().name;
            if (Directory.Exists(Application.dataPath + "/Game/Resources/CubeMaps"))
            {
                path = string.Format("Assets/Game/Resources/CubeMaps/{0}-cubemap.cubemap", Path.GetFileNameWithoutExtension(scene));
            }
            else
            {
                path = string.Format("Assets/{0}-cubemap.cubemap", Path.GetFileNameWithoutExtension(scene));
            }
            AssetDatabase.CreateAsset(cm, path);
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        [MenuItem("Tools/生成环境贴图/128x128 &1")]
        public static void CreateCubemap128x128()
        {
            GenerateCubemap(128);
        }
        [MenuItem("Tools/生成环境贴图/256x256 &2")]
        public static void CreateCubemap256x256()
        {
            GenerateCubemap(256);
        }
        [MenuItem("Tools/生成环境贴图/512x512 &3")]
        public static void CreateCubemap512x512()
        {
            GenerateCubemap(512);
        }
        [MenuItem("Tools/生成环境贴图/1024x1024 &4")]
        public static void CreateCubemap1024x1024()
        {
            GenerateCubemap(1024);
        }
    }
}

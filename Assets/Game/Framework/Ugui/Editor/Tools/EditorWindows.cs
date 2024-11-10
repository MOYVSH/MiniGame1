 using UnityEngine;
 using System.Collections;
 using UnityEditor;

#region EditorWindows.

public sealed class RenderQueueInfoWindow : EditorWindow
{
    Material[] mMaterials = null;

    [MenuItem("Tools/[查看]+[编辑] 材质渲染队列  &r")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<RenderQueueInfoWindow>().OnSelectionChange();
    }

    public void OnSelectionChange()
    {
        mMaterials = null;
        if (Selection.assetGUIDs.Length == 1)
        {
            string path;
            path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
            if (mat)
            {
                mMaterials = new Material[1];
                mMaterials[0] = mat;
            }
            else
            {
                GameObject go;
                go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (go && go.GetComponent<Renderer>())
                    mMaterials = go.GetComponent<Renderer>().sharedMaterials;  
            }
        }
        else if (Selection.gameObjects != null && Selection.gameObjects.Length == 1)
        {
            Renderer render;
            render = Selection.gameObjects[0].GetComponent<Renderer>();

            if (render != null)
                mMaterials = render.sharedMaterials; 
        }
        Repaint();
    }

    void OnGUI()
    {
        GUI.color = Color.white; 
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.alignment = TextAnchor.MiddleCenter;
        
        if (mMaterials == null || mMaterials.Length == 0)
        {
            style.normal.textColor = Color.red;
            GUILayout.Label("请选择一个有材质的对象", style);
        }
        else
        {
            style.normal.textColor = Color.green;
            GUILayout.Label("修改查看材质队列", style);
            for (int i = 0; i < mMaterials.Length; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                int last = mMaterials[i].renderQueue;
                int now = EditorGUILayout.IntField(mMaterials[i].name, mMaterials[i].renderQueue);
                if (last != now)
                {
                    Undo.RecordObject(mMaterials[i], "-renderQueue Adjust-");
                    mMaterials[i].renderQueue = now;
                }
                if (GUILayout.Button("转到材质➻"))
                {
                    Selection.activeObject = mMaterials[i];
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}

#endregion
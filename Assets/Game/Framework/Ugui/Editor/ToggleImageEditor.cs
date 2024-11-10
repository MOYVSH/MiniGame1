using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MOYV.UGUI.Editor
{
    [CustomEditor(typeof(ToggleImage), true),CanEditMultipleObjects]
    public class ToggleImageEditor : ImageEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            ToggleImage toggle = serializedObject.targetObject as ToggleImage;
            EditorGUI.BeginChangeCheck();
            var m_IsOnProperty = serializedObject.FindProperty("_IsOn");
            var m_SpriteProperty = serializedObject.FindProperty("m_Sprite");
            var m_OffSpriteProperty = serializedObject.FindProperty("OffSprite");
            var m_OnSpriteProperty = serializedObject.FindProperty("OnSprite");
            EditorGUILayout.PropertyField(m_IsOnProperty, true);
            EditorGUILayout.PropertyField(m_OffSpriteProperty, true);
            EditorGUILayout.PropertyField(m_OnSpriteProperty, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);

                toggle.IsOn = m_IsOnProperty.boolValue;
                m_SpriteProperty.objectReferenceValue =
                    toggle.IsOn ? m_OnSpriteProperty.objectReferenceValue : m_OffSpriteProperty.objectReferenceValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("GameObject/UI/Toggle Image")]
        public static void CreateToggleImage()
        {
            var selection = Selection.activeGameObject;
            if (!selection)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    canvas = new GameObject("Canvas").AddComponent<Canvas>();
                    canvas.gameObject.AddComponent<CanvasScaler>();
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                }

                selection = canvas.gameObject;
            }

            var img = new GameObject("ToggleImage").AddComponent<ToggleImage>();
            img.transform.SetParent(selection.transform);
        }
    }
}
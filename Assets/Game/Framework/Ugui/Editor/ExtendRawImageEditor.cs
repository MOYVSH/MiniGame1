using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace MOYV.UGUI.Editor
{
	[CustomEditor(typeof(ExtendRawImage), true)]
	public class ExtendRawImageEditor : RawImageEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Extend"), true);
			serializedObject.ApplyModifiedProperties();
		}

		[MenuItem("GameObject/UI/Extend RawImage")]
		public static void CreateExtendImage()
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

			var img = new GameObject("ExtendImage").AddComponent<ExtendRawImage>();
			img.transform.SetParent(selection.transform);
		}
	}
}


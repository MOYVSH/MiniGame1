using System.Linq;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

public class RoundCornerRawImageEditor : MonoBehaviour
{
    [CustomEditor(typeof(RoundCornerRawImage), true)]
    //[CanEditMultipleObjects]
    public class SimpleRoundedImageEditor : RawImageEditor
    {
        SerializedProperty m_Radius;
        SerializedProperty m_TriangleNum;


        protected override void OnEnable()
        {
            base.OnEnable();

            m_Radius = serializedObject.FindProperty("Radius");
            m_TriangleNum = serializedObject.FindProperty("TriangleNum");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_Radius);
            EditorGUILayout.PropertyField(m_TriangleNum);
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
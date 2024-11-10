using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.UI;
using UnityEngine;

namespace MOYV.UGUI.Editor
{
    /// <summary>
    /// Custom editor for UI Particles component
    /// </summary>
    [CustomEditor(typeof(UGUIParticle))]
    public class UiParticlesEditor : GraphicEditor
    {

        private SerializedProperty m_RenderMode;
        private SerializedProperty m_StretchedSpeedScale;
        private SerializedProperty m_StretchedLenghScale;
        private SerializedProperty m_IgnoreTimescale;
        
        private SerializedProperty m_IsSpriteMode;
        private SerializedProperty m_Sprite;
        private SerializedProperty m_BlendMode;
        private SerializedProperty m_CustomMaterial;

        private SerializedProperty m_Material;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_RenderMode = serializedObject.FindProperty("m_RenderMode");
            m_StretchedSpeedScale = serializedObject.FindProperty("m_StretchedSpeedScale");
            m_StretchedLenghScale = serializedObject.FindProperty("m_StretchedLenghScale");
            m_IgnoreTimescale = serializedObject.FindProperty("m_IgnoreTimescale");
            
            m_IsSpriteMode = serializedObject.FindProperty("m_IsSpriteMode");
            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_BlendMode = serializedObject.FindProperty("m_BlendMode");
            
            m_Material = serializedObject.FindProperty("m_Material");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();

            UGUIParticle uguiParticleSystem = (UGUIParticle) target;

            if (GUILayout.Button("Apply to nested particle systems"))
            {
                var nested = uguiParticleSystem.gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var particleSystem in nested)
                {
                    if (particleSystem.GetComponent<UGUIParticle>() == null)
                        particleSystem.gameObject.AddComponent<UGUIParticle>();
                }
            }

            EditorGUILayout.PropertyField(m_RenderMode);

            if (uguiParticleSystem.RenderMode == UiParticleRenderMode.StreachedBillboard)
            {
                EditorGUILayout.PropertyField(m_StretchedSpeedScale);
                EditorGUILayout.PropertyField(m_StretchedLenghScale);
            }

            EditorGUILayout.PropertyField(m_IgnoreTimescale);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_IsSpriteMode);
            if (EditorGUI.EndChangeCheck())
            {
                if (!m_IsSpriteMode.boolValue)
                {
                    m_Sprite.objectReferenceValue = null;
                }
            }

            if (m_IsSpriteMode.boolValue)
            {
                EditorGUILayout.PropertyField(m_Sprite);
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_BlendMode);

                if (EditorGUI.EndChangeCheck())
                {
                    m_Material.objectReferenceValue = UGUIParticle.GetMaterial((ParticleBlendMode) m_BlendMode.enumValueIndex);
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}

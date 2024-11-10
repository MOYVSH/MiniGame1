using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace NUnityExtends
{
    [AddComponentMenu("NUnityExtends/UI/NToggle")]
    public class NToggle : Toggle
    {
        public enum TransitionType
        {
            None,
            Checked,
            Swap,
        }

        [Serializable]
        public class NToggleEvent : UnityEvent<NToggle, bool> { }

        [SerializeField]
        private TransitionType m_TransitionType;
        [SerializeField]
        private GameObject m_BaseGameObject;
        [SerializeField]
        private List<GameObject> m_CheckedGameObjects;
        [SerializeField]
        private NToggleEvent m_OnValueChanged2 = new NToggleEvent();

        public TransitionType transitionType
        {
            get { return m_TransitionType; }
            set { m_TransitionType = value; }
        }
        public GameObject baseGameObject
        {
            get { return m_BaseGameObject; }
            set { m_BaseGameObject = value; }
        }
        public List<GameObject> checkedGameObjects
        {
            get { return m_CheckedGameObjects; }
            set { m_CheckedGameObjects = value; }
        }
        public NToggleEvent onValueChanged2
        {
            get { return m_OnValueChanged2; }
            set { m_OnValueChanged2 = value; }
        }

        public object data { get; set; }

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnExtendClicked);
        }

        private void OnExtendClicked(bool isOn)
        {
            if (m_TransitionType == TransitionType.Checked)
            {
                if (m_BaseGameObject != null) m_BaseGameObject.SetActive(true);
                if (m_CheckedGameObjects != null)
                {
                    for (int i = 0; i < m_CheckedGameObjects.Count; i++)
                    {
                        m_CheckedGameObjects[i]?.SetActive(isOn);
                    }
                }
            }
            else if (m_TransitionType == TransitionType.Swap)
            {
                if (m_BaseGameObject != null) m_BaseGameObject.SetActive(!isOn);
                for (int i = 0; i < m_CheckedGameObjects.Count; i++)
                {
                    m_CheckedGameObjects[i]?.SetActive(isOn);
                }
            }
            if (onValueChanged2 != null) onValueChanged2.Invoke(this, isOn);
        }
    }

}

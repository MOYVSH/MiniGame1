using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MOYV.UGUI
{
    [ExecuteInEditMode]
    public class ToggleImage : Image, IPointerClickHandler
    {
        [SerializeField] private bool _IsOn;

        public Action<bool> OnValueChanged;

        public bool IsOn
        {
            get { return _IsOn; }
            set
            {
                _IsOn = value;

                if (OnValueChanged != null)
                    OnValueChanged(IsOn);
                sprite = _IsOn ? OnSprite : OffSprite;
                this.SetMaterialDirty();
            }
        }

        public Sprite OffSprite, OnSprite;

        protected override void OnEnable()
        {
            base.OnEnable();
            sprite = _IsOn ? OnSprite : OffSprite;
        }

        protected override void Start()
        {
            base.Start();
            sprite = _IsOn ? OnSprite : OffSprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IsOn = !IsOn;
        }
    }
}
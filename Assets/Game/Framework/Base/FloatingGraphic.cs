using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MOYV.MODULE
{
    public class FloatingGraphic : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,IInitializePotentialDragHandler
    {
        public Action<RectTransform> OnAdhereLeft, OnAdhereRight, OnAdhereUp, OnAdhereDown;
        public Action<PointerEventData> OnPotentialDrag;

        public Vector4 offset;

        private RectTransform _rectTransform;
        private Canvas _canvas;

        private RectTransform rectTransform
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = gameObject.GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private void Start()
        {
            _canvas = rectTransform.root.GetComponent<Canvas>();
            AdhereEdge();
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.delta != Vector2.zero)
            {
                this.rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            AdhereEdge();
        }

        private void AdhereEdge()
        {
            var container = this.rectTransform.parent as RectTransform;
            if (!container)
            {
                return;
            }

            var rect = container.rect;
            var halfWidth = rect.width * 0.5f;
            var halfHeight = rect.height * 0.5f;

            var position = this.rectTransform.anchoredPosition;

            var toLeft = new Vector2(-halfWidth + offset.x, position.y);
            var toRight = new Vector2(halfWidth + offset.z, position.y);
            var toUp = new Vector2(position.x, halfHeight + offset.y);
            var toDown = new Vector2(position.x, -halfHeight + offset.w);

            if (position.x <= 0)
            {
                if (position.y <= 0)
                {
                    if (Vector2.Distance(toLeft, position) <= Vector2.Distance(toDown, position))
                    {
                        StartCoroutine(Move(toLeft, OnAdhereLeft));
                    }
                    else
                    {
                        StartCoroutine(Move(toDown, OnAdhereDown));
                    }
                }
                else
                {
                    if (Vector2.Distance(toLeft, position) <= Vector2.Distance(toUp, position))
                    {
                        StartCoroutine(Move(toLeft, OnAdhereLeft));
                    }
                    else
                    {
                        StartCoroutine(Move(toUp, OnAdhereUp));
                    }
                }
            }
            else
            {
                if (position.y <= 0)
                {
                    if (Vector2.Distance(toRight, position) <= Vector2.Distance(toDown, position))
                    {
                        StartCoroutine(Move(toRight, OnAdhereRight));
                    }
                    else
                    {
                        StartCoroutine(Move(toDown, OnAdhereDown));
                    }
                }
                else
                {
                    if (Vector2.Distance(toRight, position) <= Vector2.Distance(toUp, position))
                    {
                        StartCoroutine(Move(toRight, OnAdhereRight));
                    }
                    else
                    {
                        StartCoroutine(Move(toUp, OnAdhereUp));
                    }
                }
            }
        }


        IEnumerator Move(Vector2 target, Action<RectTransform> callback)
        {
            yield return null;
            var t = 0f;
            while (t >= 1)
            {
                t += Time.deltaTime * 2;
                this.rectTransform.anchoredPosition = Vector2.Lerp(this.rectTransform.anchoredPosition, target, t);
            }

            this.rectTransform.anchoredPosition = target;

            if (callback != null)
            {
                callback(this.rectTransform);
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (OnPotentialDrag != null)
            {
                OnPotentialDrag(eventData);
            }
        }
    }
}
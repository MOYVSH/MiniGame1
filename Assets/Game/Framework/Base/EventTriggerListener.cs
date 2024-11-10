using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MOYV.MODULE
{
    public class EventTriggerListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler,
        IDragHandler, IEndDragHandler, IBeginDragHandler, IInitializePotentialDragHandler,
        IPointerExitHandler, IPointerUpHandler
    {
        public delegate void VoidDelegate1(GameObject go, PointerEventData eventData);

        public event Action<GameObject, PointerEventData> OnClick;
        public event Action<GameObject, PointerEventData> OnDown;
        public event Action<GameObject, PointerEventData> OnEnter;
        public event Action<GameObject, PointerEventData> OnExit;
        public event Action<GameObject, PointerEventData> OnUp;
        public event Action<GameObject, PointerEventData> OnDragging;


        public event Action<GameObject, PointerEventData> OnBeginDragging;

        public event Action<GameObject, PointerEventData> OnEndDragging;

        public event Action<GameObject, PointerEventData> OnInitializePotentialDragging;

        public UnityEvent OnUnityClick;

        public static EventTriggerListener Get(GameObject go)
        {
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null)
            {
                listener = go.AddComponent<EventTriggerListener>();
                listener.OnUnityClick = new UnityEvent();
            }

            return listener;
        }

        public static void Remove(GameObject go)
        {
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener)
            {
                Destroy(listener);
            }
        }

        public static void Remove(Graphic graphic)
        {
            Remove(graphic.gameObject);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClick != null) OnClick(gameObject, eventData);

            if (OnUnityClick != null)
            {
                OnUnityClick.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (OnDown != null) OnDown(gameObject, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (OnEnter != null) OnEnter(gameObject, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (OnExit != null) OnExit(gameObject, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (OnUp != null) OnUp(gameObject, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (OnDragging != null) OnDragging(gameObject, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (OnBeginDragging != null) OnBeginDragging(gameObject, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (OnEndDragging != null) OnEndDragging(gameObject, eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (OnInitializePotentialDragging != null) OnInitializePotentialDragging(gameObject, eventData);
        }

        public void SetCommonClick(Action<GameObject, PointerEventData> onPointDown,
            Action<GameObject, PointerEventData> onPointUp)
        {
            this.OnDown = onPointDown;
            this.OnUp = onPointUp;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MOYV.UGUI
{
    public class ColliderEventTriggerListener : MonoBehaviour
    {
        public event Action<GameObject> OnClick;
        public event Action<GameObject> OnDown;
        public event Action<GameObject> OnEnter;
        public event Action<GameObject> OnExit;
        public event Action<GameObject> OnUp;

        public event Action<GameObject> OnDragging;


        private static ColliderEventChecker mChecker;

        static void InitChecker()
        {
            if (mChecker)
            {
                return;
            }

            mChecker = FindObjectOfType<ColliderEventChecker>();

            if (!mChecker)
                mChecker = new GameObject("Checker").AddComponent<ColliderEventChecker>();
        }


        static public ColliderEventTriggerListener Get(GameObject go)
        {
            InitChecker();

            ColliderEventTriggerListener listener = go.GetComponent<ColliderEventTriggerListener>();
            if (listener == null) listener = go.AddComponent<ColliderEventTriggerListener>();
            return listener;
        }


        internal void FixedOnMouseUpAsButton()
        {
            if (OnClick != null) OnClick(gameObject);
        }

        internal void FixedOnMouseDown()
        {
            if (OnDown != null) OnDown(gameObject);
        }

        internal void FixedOnMouseUp()
        {
            if (OnUp != null) OnUp(gameObject);
        }

//        internal void FixedOnMouseDrag()
//        {
//            if (OnDragging != null) OnDragging(gameObject);
//        }
//
//        internal void FixedOnMouseEnter()
//        {
//            if (OnEnter != null) OnEnter(gameObject);
//        }
//
//        internal void FixedOnMouseExit()
//        {
//            if (OnExit != null) OnExit(gameObject);
//        }
    }
}
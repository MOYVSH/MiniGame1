using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MOYV.UGUI
{
    public class ColliderEventChecker : MonoBehaviour
    {
        private ColliderEventTriggerListener mListener;


        void Update()
        {
            if (Input.GetMouseButtonDown(0) && !InputTool.CheckMouseOnUI())
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                List<RaycastHit> orderedHits =
                    new List<RaycastHit>(Physics.RaycastAll(mouseRay, float.MaxValue, ~0,
                        QueryTriggerInteraction.Collide));
                orderedHits.Sort((h1, h2) => h1.distance.CompareTo(h2.distance));
                foreach (RaycastHit hit in orderedHits)
                {
                    mListener = hit.collider.gameObject.GetComponent<ColliderEventTriggerListener>();
                    if (!mListener)
                    {
                        continue;
                    }

                    mListener.FixedOnMouseUpAsButton();
                    mListener.FixedOnMouseDown();
                    break;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (mListener)
                {
                    mListener.FixedOnMouseUp();
                    mListener = null;
                }
            }
        }
    }
}
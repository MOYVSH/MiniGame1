using System;
using UnityEngine;

namespace MOYV.MODULE
{
    public class SortLayerHelperScript : MonoBehaviour
    {
        public Renderer[] renderers;
        public bool autoSet;


        [SerializeField] private string layerName;
        [SerializeField] private int layerValue;

        private void Awake()
        {
            if (autoSet)
            {
                AutoGetContent();

                ResetValue();
            }
        }

        private void Start()
        {
            if (autoSet)
            {
                AutoGetContent();

                ResetValue();
            }
        }

        public string LayerName
        {
            get { return layerName; }
            set
            {
                layerName = value;
                AutoGetContent();
                ResetValue();
            }
        }

        public int LayerValue
        {
            get { return layerValue; }
            set
            {
                layerValue = value;
                ResetValue();
            }
        }

        public void AutoGetContent()
        {
            renderers = this.GetComponentsInChildren<Renderer>();
        }

        public void ResetValue()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var render = renderers[i];
                render.sortingLayerName = layerName;
                render.sortingOrder = layerValue;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using MOYV;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// UI分组置灰
/// </summary>
public class UIMaterialGroup : MonoBehaviour
{
    public Material targetMat;

    public bool isChange;
    private Image[] _elements;
    public void Awake()
    {
        _elements = GetComponentsInChildren<Image>();
        if (isChange)
        {
            Change(isChange);
        }
    }

    public void Change(bool flag)
    {
        if (isChange == flag) return;
        
        isChange = flag;
        if (_elements != null)
        {
            _elements.ForEach((element) =>
            {
                if (isChange)
                {
                    var tempMat = element.material;
                    targetMat.mainTexture = element.mainTexture;
                    element.material = targetMat;
                }
                else
                {
                    element.material = null;
                }
            });
        }
    }
}

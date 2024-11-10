using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FocusInputFields : MonoBehaviour, ISelectHandler
{
    private InputField inputField;
    private void Start()
    {
        inputField = GetComponent<InputField>();
    }

    public void OnSelect(BaseEventData data)
    {
        inputField.text = "";
    }
}
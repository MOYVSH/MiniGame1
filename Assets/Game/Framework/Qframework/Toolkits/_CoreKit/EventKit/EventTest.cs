using System;
using QFramework;
using UnityEngine;

public class EventTest : MonoBehaviour
{
    public Collider2D playerCol;

    private void Start()
    {
        playerCol.OnTriggerEnter2DEvent( (col) =>
        {
            Debug.Log("OnTriggerEnter2D");
        });
    }
}

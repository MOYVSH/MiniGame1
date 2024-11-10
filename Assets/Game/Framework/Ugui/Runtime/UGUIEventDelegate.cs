using UnityEngine;
using System.Collections;

public class UGUIEventDelegate : MonoBehaviour 
{
    void OnDoubleClick()
    {
        if (OnDoubleTouch != null)
            OnDoubleTouch(this);
    }

    void OnClick()
    {
        if (OnSingleClick != null)
            OnSingleClick(this);
    }

    void OnPress(bool isPressed) 
    {
        if(isPressed)
        {   
            PixelTouchBeginPos = Input.GetTouch(0).position;
            PixelLastTouchPos  = PixelTouchBeginPos;
            if (OnTouchBegin != null)
                OnTouchBegin(this);
        }
        else
        {  
            PixelOffsetPos      = Input.GetTouch(0).deltaPosition;
            PixelLastTouchPos   = Input.GetTouch(0).position;
            PixelTotalOffsetPos = PixelLastTouchPos - PixelTouchBeginPos;
            if (OnTouchEnd != null)
                OnTouchEnd(this);
        }
    }

    void OnDrag(Vector2 delta)
    {   
        PixelOffsetPos      = Input.GetTouch(0).deltaPosition;
        PixelLastTouchPos   = Input.GetTouch(0).position;
        PixelTotalOffsetPos = PixelLastTouchPos - PixelTouchBeginPos;
        if (OnMoving != null)
            OnMoving(this);
    }

    public void RemoveEventFocus()
    {
        //UICamera.MouseOrTouch touch;
        //touch = UICamera.currentTouch;
        //if (touch != null)
        //{
        //    touch.dragStarted = false;
        //    touch.pressed = null;
        //    touch.dragged = null;
        //    touch.current = null;
        //}
    }

    public delegate void EventParams(UGUIEventDelegate sender);
    public event EventParams OnTouchBegin  = null;
    public event EventParams OnTouchEnd    = null;
    public event EventParams OnMoving      = null;
    public event EventParams OnDoubleTouch = null;
    public event EventParams OnSingleClick = null;

    [HideInInspector]
    public Vector2 PixelTouchBeginPos = Vector2.zero;

    [HideInInspector]
    public Vector2 PixelLastTouchPos = Vector2.zero;

    [HideInInspector]
    public Vector2 PixelOffsetPos = Vector2.zero;

    [HideInInspector]
    public Vector2 PixelTotalOffsetPos = Vector2.zero;

//    public Vector2 NguiTouchBeginPos    { get { return WJUtils.Pixel2Ngui(PixelTouchBeginPos); } }
//    public Vector2 NguiLastTouchPos     { get { return WJUtils.Pixel2Ngui(PixelLastTouchPos); } }
//    public Vector2 NguiOffsetPos        { get { return WJUtils.Pixel2Ngui(PixelOffsetPos); } }
//    public Vector2 NguiTotalOffsetPos   { get { return WJUtils.Pixel2Ngui(PixelTotalOffsetPos); } }
}

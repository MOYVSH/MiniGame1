using System;
using System.Collections;
using System.Collections.Generic;
using MOYV;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class LoadingViewData : BasePanelData
{
    public Action completeFunc;
}
public class LoadingView : AbstractBasePanel
{
    public Image currentBar = null;
    private Action _completeFunc = null;
    private void Awake()
    {
        
    }

    public override IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    public override void InitWithPanelData(BasePanelData data)
    {
        currentBar.fillAmount = 0;
        if (data != null)
        {
            LoadingViewData loadingData = data as LoadingViewData;
            _completeFunc = loadingData.completeFunc;
        }
    }

    public void SetProgress(float progress)
    {
        currentBar.fillAmount = progress;
    }

    public void Complete()
    {
        SetProgress(1);
        _completeFunc?.Invoke();
        _completeFunc = null;
    }
}

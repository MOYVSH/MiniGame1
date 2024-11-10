using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using MOYV;
using QFramework;

public class ApplicationScene : AbstractMonoBehaviourController
{
    #region Notify
    
    protected void Awake()
    {
        Initialize();
    }
    
    #endregion
    
    public static Action<bool> onApplicationPause;
    
    void Initialize()
    {
        var framework = GameAOT.Interface;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        onApplicationPause?.Invoke(pauseStatus);
    }
    
    public override IArchitecture GetArchitecture()
    {
        return GameAOT.Interface;
    }
}
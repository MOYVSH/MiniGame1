using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;

public class LaunchController : AbstractMonoBehaviourController
{
    /// <summary>
    /// UI系统的缓存
    /// </summary>
    private UISystem _uiSystem;
    
    public override IArchitecture GetArchitecture()
    {
        return AOT.Interface;
    }

    private IEnumerator Start()
    {
        yield return AOT.Interface;
        Log($"This Architecture is {this.GetArchitecture().GetType().Name}");
        
        //获取UI系统
        _uiSystem = this.GetSystem<UISystem>();

        GameStart();
    }
    
    private void GameStart()
    {
        GameSceneController.Instance.FirstLoadMainScene().Forget();
    }
}

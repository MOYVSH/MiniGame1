using UnityEngine;

public class AOT : ArchitectureProxy<AOT>
{
    public override void Init()
    {
        Debug.Log("<color=green>AOT Architecture Init</color>");
        RegisterSystem(new UISystem());      //注册UI系统
    }

}
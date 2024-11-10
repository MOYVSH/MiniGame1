using System;
using MOYV;

public class GameAOT : ArchitectureProxy<GameAOT>
{
    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init()
    {
        RegisterSystem(new TimerSystem());
    }
}
using QFramework;

public class AfterSceneInitLogicCmd : AbstractCommand
{
    protected override void OnExecute()
    {
        Log("执行场景加载完成后初始化工作");
        // 初始化数据
    }
}
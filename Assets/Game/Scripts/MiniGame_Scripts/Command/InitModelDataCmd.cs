using QFramework;

public class InitModelDataCmd : AbstractCommand
{
    protected override void OnExecute()
    {
        Log("执行场景加载前加载数据");
        // 初始化数据
    }
}
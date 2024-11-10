using UnityEngine;

public class MiniGame : ArchitectureProxy<MiniGame>
{
    public override void Init()
    {
        Debug.Log("<color=green>MiniGame Architecture Init</color>");
        RegisterUtility();
        RegisterModel();
        RegisterSystem();
    }

    private void RegisterModel()
    {
        
    }

    private void RegisterSystem()
    {
        
    }

    private void RegisterUtility()
    {
        //this.RegisterUtility<YooassetUtility>(new YooassetUtility());
    }
}

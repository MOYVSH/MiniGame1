using QFramework;
using UnityEngine;

public class CameraController : AbstractMonoBehaviourController
{
    
    public override IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}

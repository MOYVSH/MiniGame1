using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraAdditional : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var architecture = GameArchitecture.Interface;
        var uiSystem = architecture.GetSystem<UISystem>();
        var cam = uiSystem.UICamera;
        var data = cam.GetUniversalAdditionalCameraData();
        data.renderType = CameraRenderType.Overlay;
        Camera currentCamera = GetComponent<Camera>();
        var additional = currentCamera.GetUniversalAdditionalCameraData();
        additional.cameraStack.Add(cam);
    }
}

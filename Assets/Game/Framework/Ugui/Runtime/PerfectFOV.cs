// ---
// PerfectFOV: Slap this on a camera and it'll automatically adjust the field of view to keep your designed aspect ratio visible
// by Alexander Ocias
// https://ocias.com
// ---

using UnityEngine;

[ExecuteInEditMode]
public class PerfectFOV : MonoBehaviour
{
    Camera Camera;
    [SerializeField] float targetAspectRatio = 9f / 16f;
    public float targetVerticalFieldOfView = 30f;
    float targetHorizontalFieldOfView;

    void Start()
    {
        Camera = GetComponent<Camera>();
        UpdateFOV();
    }

#if UNITY_EDITOR
    void Update()
    {
        UpdateFOV();
    }
#endif

    void UpdateFOV()
    {
        float currentAspectRatio = (float) Screen.width / (float) Screen.height;

        if (currentAspectRatio < targetAspectRatio)
        {
            // View narrower than target
            targetHorizontalFieldOfView = VerticalFOVtoHorizontalFOV(targetVerticalFieldOfView, targetAspectRatio);
            Camera.fieldOfView = HorizontalFOVtoVerticalFOV(targetHorizontalFieldOfView, currentAspectRatio);
        }
        else
        {
            // View wider than target
            Camera.fieldOfView = targetVerticalFieldOfView;
        }
    }

    float VerticalFOVtoHorizontalFOV(float verticalFOV, float aspectRatio)
    {
        float vFOVInRads = verticalFOV * Mathf.Deg2Rad;
        float hFOVInRads = 2f * Mathf.Atan(Mathf.Tan(vFOVInRads / 2f) * aspectRatio);
        return hFOVInRads * Mathf.Rad2Deg;
    }

    float HorizontalFOVtoVerticalFOV(float horizontalFOV, float aspectRatio)
    {
        float hFOVInRads = horizontalFOV * Mathf.Deg2Rad;
        float vFOVInRads = Mathf.Tan(hFOVInRads * 0.5f) / aspectRatio;
        vFOVInRads = Mathf.Atan(vFOVInRads) * 2f;
        return vFOVInRads * Mathf.Rad2Deg;
    }
}
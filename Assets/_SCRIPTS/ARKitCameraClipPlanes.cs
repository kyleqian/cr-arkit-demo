using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ARKitCameraClipPlanes : MonoBehaviour
{
    new Camera camera;
    float currentNearZ;
    float currentFarZ;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void Start()
    {
        UpdateCameraClipPlanes();
    }

    void UpdateCameraClipPlanes()
    {
        currentNearZ = camera.nearClipPlane;
        currentFarZ = camera.farClipPlane;
        ARKitManager.Instance.Session.SetCameraClipPlanes(currentNearZ, currentFarZ);
    }

    void Update()
    {
        if (currentNearZ != camera.nearClipPlane || currentFarZ != camera.farClipPlane)
        {
            UpdateCameraClipPlanes();
        }
    }
}

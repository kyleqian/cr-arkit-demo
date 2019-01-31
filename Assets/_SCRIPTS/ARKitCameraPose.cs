using UnityEngine;
using UnityEngine.XR.iOS;

[RequireComponent(typeof(Camera))]
public class ARKitCameraPose : MonoBehaviour
{
    bool sessionStarted;
    new Camera camera;

    void FirstFrameUpdate(UnityARCamera cam)
    {
        sessionStarted = true;
        UnityARSessionNativeInterface.ARFrameUpdatedEvent -= FirstFrameUpdate;
    }

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void Start()
    {
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += FirstFrameUpdate;
    }

    void Update()
    {
        if (sessionStarted)
        {
            Matrix4x4 matrix = ARKitManager.Instance.Session.GetCameraPose();
            camera.transform.localPosition = UnityARMatrixOps.GetPosition(matrix);
            camera.transform.localRotation = UnityARMatrixOps.GetRotation(matrix);
            camera.projectionMatrix = ARKitManager.Instance.Session.GetCameraProjection();
        }
    }
}

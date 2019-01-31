using UnityEngine;
using UnityEngine.XR.iOS;

public class ARKitManager : MonoBehaviour
{
    public static ARKitManager Instance;

    [Header("AR Config Options")]
    public UnityARAlignment startAlignment = UnityARAlignment.UnityARAlignmentGravity;
    public UnityARPlaneDetection planeDetection = UnityARPlaneDetection.Horizontal;
    public bool getPointCloud = true;
    public bool enableLightEstimation = true;
    public bool enableAutoFocus = true;
    public UnityAREnvironmentTexturing environmentTexturing = UnityAREnvironmentTexturing.UnityAREnvironmentTexturingNone;

    [Header("Image Tracking")]
    public ARReferenceImagesSet detectionImages;
    public int maximumNumberOfTrackedImages;

    [Header("Object Tracking")]
    public ARReferenceObjectsSetAsset detectionObjects;

    public UnityARSessionNativeInterface Session { get; private set; }

    public ARKitWorldTrackingSessionConfiguration DefaultSessionConfiguration
    {
        get
        {
            ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration
            {
                planeDetection = planeDetection,
                alignment = startAlignment,
                getPointCloudData = getPointCloud,
                enableLightEstimation = enableLightEstimation,
                enableAutoFocus = enableAutoFocus,
                maximumNumberOfTrackedImages = maximumNumberOfTrackedImages,
                environmentTexturing = environmentTexturing
            };
            if (detectionImages != null)
            {
                config.referenceImagesGroupName = detectionImages.resourceGroupName;
            }
            if (detectionObjects != null)
            {
                config.referenceObjectsGroupName = "";  //lets not read from XCode asset catalog right now
                config.dynamicReferenceObjectsPtr = Session.CreateNativeReferenceObjectsSet(detectionObjects.LoadReferenceObjectsInSet());
            }
            return config;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        Session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        Application.targetFrameRate = 60;
        var config = DefaultSessionConfiguration;
        if (config.IsSupported)
        {
            Session.RunWithConfig(config);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

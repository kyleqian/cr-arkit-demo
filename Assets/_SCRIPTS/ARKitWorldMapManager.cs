using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class ARKitWorldMapManager : MonoBehaviour
{
    public static ARKitWorldMapManager Instance;

    enum RotationState { None, CW, CCW }
    enum ScalingState { None, Up, Down }

    [SerializeField] Image referenceImage;
    [SerializeField] Toggle testModeToggle;
    [SerializeField] GameObject modelPrefab;
    [SerializeField] float maxRayDistance;
    [SerializeField] float rotationSpeed;
    [SerializeField] float scalingSpeed;
    [SerializeField] LayerMask collisionLayer; // ARKitPlane layer

    public bool IsTestMode { get { return testModeToggle.isOn; } }

#if !UNITY_EDITOR
    ARHitTestResultType[] hitTestResultPriorityOrder = {
        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
        //ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
        ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane, 
        //ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane, 
        //ARHitTestResultType.ARHitTestResultTypeFeaturePoint
    };
#endif
    RotationState rotationState = RotationState.None;
    ScalingState scalingState = ScalingState.None;
    GameObject modelInstance;

    string TestModePrefixIfNeeded { get { return IsTestMode ? "TEST_" : ""; } }

    UnityARSessionNativeInterface Session
    {
        get { return UnityARSessionNativeInterface.GetARSessionNativeInterface(); }
    }

    string WorldMapSavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, TestModePrefixIfNeeded + "save.worldmap");
        }
    }

    string ReferenceImageSaveName
    {
        get
        {
            string saveName = "";
#if UNITY_EDITOR
            saveName += "ReferenceImages/";
#endif
            return saveName + TestModePrefixIfNeeded + "ReferenceImage.png";
        }
    }

    string PlayerPrefAnchorIdKey { get { return TestModePrefixIfNeeded + "AnchorId"; } }
    string PlayerPrefScaleXKey { get { return TestModePrefixIfNeeded + "ScaleX"; } }
    string PlayerPrefScaleYKey { get { return TestModePrefixIfNeeded + "ScaleY"; } }
    string PlayerPrefScaleZKey { get { return TestModePrefixIfNeeded + "ScaleZ"; } }

    public void StartCWRotation()
    {
        rotationState = RotationState.CW;
    }

    public void StartCCWRotation()
    {
        rotationState = RotationState.CCW;
    }

    public void ResetRotationState()
    {
        rotationState = RotationState.None;
    }

    public void StartScalingUp()
    {
        scalingState = ScalingState.Up;
    }

    public void StartScalingDown()
    {
        scalingState = ScalingState.Down;
    }

    public void ResetScalingState()
    {
        scalingState = ScalingState.None;
    }

    public void ResetARSession()
    {
        modelInstance.SetActive(false);
        Session.RunWithConfigAndOptions(ARKitCameraManager.Instance.sessionConfiguration, UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking);
    }

    public void SaveWorldMap()
    {
        SaveModel();
        Session.GetCurrentWorldMapAsync(OnWorldMap);
    }

    public void LoadWorldMap()
    {
#if UNITY_EDITOR
        referenceImage.sprite = UtilitiesCR.LoadNewSprite(Application.dataPath + "/../" + ReferenceImageSaveName);
        referenceImage.color = new Color(1, 1, 1, 1);
#else
        Debug.LogFormat("Loading ARWorldMap {0}", WorldMapSavePath);
        var worldMap = ARWorldMap.Load(WorldMapSavePath);
        if (worldMap != null)
        {
            Debug.LogFormat("Map loaded. Center: {0} Extent: {1}", worldMap.center, worldMap.extent);

            var config = ARKitCameraManager.Instance.sessionConfiguration;
            config.worldMap = worldMap;
            UnityARSessionRunOption runOption = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking;

            Debug.Log("Restarting session with worldMap");
            Session.RunWithConfigAndOptions(config, runOption);

            referenceImage.sprite = UtilitiesCR.LoadNewSprite(Application.persistentDataPath + "/" + ReferenceImageSaveName);
            referenceImage.color = new Color(1, 1, 1, 1);
        }
#endif
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        modelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        modelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        modelInstance.SetActive(true);

        Debug.Log("Added anchor: " + anchorData.identifier + " " + modelInstance.transform.position.ToString("F2"));

        if (PlayerPrefs.HasKey(PlayerPrefScaleXKey) && PlayerPrefs.HasKey(PlayerPrefScaleYKey) && PlayerPrefs.HasKey(PlayerPrefScaleZKey))
        {
            modelInstance.transform.localScale = new Vector3(PlayerPrefs.GetFloat(PlayerPrefScaleXKey), PlayerPrefs.GetFloat(PlayerPrefScaleYKey), PlayerPrefs.GetFloat(PlayerPrefScaleZKey));
        }
    }

    void UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent(ARUserAnchor anchorData)
    {
        modelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        modelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

        Debug.Log("Updated anchor: " + anchorData.identifier + " " + modelInstance.transform.position.ToString("F2"));
    }

    void UnityARSessionNativeInterface_ARUserAnchorRemovedEvent(ARUserAnchor anchorData)
    {
        modelInstance.SetActive(false);

        Debug.Log("Removed anchor: " + modelInstance.transform.position.ToString("F2"));
    }

    void OnWorldMap(ARWorldMap worldMap)
    {
#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot(ReferenceImageSaveName);
#else
        if (worldMap != null)
        {
            worldMap.Save(WorldMapSavePath);
            ScreenCapture.CaptureScreenshot(ReferenceImageSaveName);
            Debug.LogFormat("ARWorldMap saved to {0}", WorldMapSavePath);
        }
#endif
    }

    void SaveModel()
    {
        PlayerPrefs.SetFloat(PlayerPrefScaleXKey, modelInstance.transform.localScale.x);
        PlayerPrefs.SetFloat(PlayerPrefScaleYKey, modelInstance.transform.localScale.y);
        PlayerPrefs.SetFloat(PlayerPrefScaleZKey, modelInstance.transform.localScale.z);

        if (PlayerPrefs.HasKey(PlayerPrefAnchorIdKey))
        {
            Session.RemoveUserAnchor(PlayerPrefs.GetString(PlayerPrefAnchorIdKey));
            PlayerPrefs.DeleteKey(PlayerPrefAnchorIdKey);
        }
        PlayerPrefs.SetString(PlayerPrefAnchorIdKey, Session.AddUserAnchorFromGameObject(modelInstance).identifierStr);
    }

    void RotateModel(bool clockwise)
    {
        modelInstance.transform.Rotate((clockwise ? 1 : -1) * rotationSpeed * Vector3.up * Time.deltaTime);
    }

    void ScaleModel(bool scaleUp)
    {
        Vector3 newScale = modelInstance.transform.localScale + ((scaleUp ? 1 : -1) * scalingSpeed * Vector3.one * Time.deltaTime);
        if (newScale.x < 0 || newScale.y < 0 || newScale.z < 0)
        {
            return;
        }
        modelInstance.transform.localScale = newScale;
    }

    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
    {
        List<ARHitTestResult> hitResults = Session.HitTest(point, resultTypes);
        if (hitResults.Count > 0)
        {
            foreach (var hitResult in hitResults)
            {
                modelInstance.transform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                modelInstance.SetActive(true);
                return true;
            }
        }
        return false;
    }

    void HandleHitTest()
    {
#if !UNITY_EDITOR
        if (Input.touchCount > 0 && !IsPointerOverUIObject())
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                ARPoint point = new ARPoint
                {
                    x = screenPosition.x,
                    y = screenPosition.y
                };

                foreach (ARHitTestResultType resultType in hitTestResultPriorityOrder)
                {
                    if (HitTestWithResultType(point, resultType))
                    {
                        break;
                    }
                }
            }
        }
#endif
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        modelInstance = Instantiate(modelPrefab);
        modelInstance.SetActive(false);
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent += UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent += UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
    }

    void Start()
    {
        UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization = true;
    }

    void Update()
    {
        HandleHitTest();

        switch (rotationState)
        {
            case RotationState.None:
                break;
            case RotationState.CW:
                RotateModel(true);
                break;
            case RotationState.CCW:
                RotateModel(false);
                break;
        }

        switch (scalingState)
        {
            case ScalingState.None:
                break;
            case ScalingState.Up:
                ScaleModel(true);
                break;
            case ScalingState.Down:
                ScaleModel(false);
                break;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Destroy(modelInstance);
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent -= UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent -= UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
    }
}

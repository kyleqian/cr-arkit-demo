using System;
using System.Collections;
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

    // Because you can't have dictionaries in the inspector?
    enum ModelType { Plaque, Statue }
    [SerializeField] GameObject plaquePrefab;
    [SerializeField] GameObject statuePrefab;
    Dictionary<ModelType, GameObject> modelTypeToModelInstance = new Dictionary<ModelType, GameObject>();
    GameObject ModelInstance { get { return modelTypeToModelInstance[modelType]; } }

    [SerializeField] Text modelTypeText;
    [SerializeField] GameObject referenceImage;
    [SerializeField] Toggle testModeToggle;
    [SerializeField] float maxRayDistance;
    [SerializeField] float rotationSpeed;
    [SerializeField] float scalingSpeed;
    [SerializeField] LayerMask collisionLayer; // ARKitPlane layer

    [Header("GameObjects to hide for screenshot")]
    [SerializeField] PointCloudParticleExample PointCloudGenerator;
    [SerializeField] GameObject ScreenUI;

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
    ModelType modelType = ModelType.Plaque;

    string WorldMapSavedAssetPrefixes
    {
        get
        {
            return string.Format("{0}{1}_",
                                 IsTestMode ? "TEST_" : "",
                                 modelType.ToString());
        }
    }

    UnityARSessionNativeInterface Session
    {
        get { return UnityARSessionNativeInterface.GetARSessionNativeInterface(); }
    }

    string WorldMapSavePath
    {
        get { return Path.Combine(Application.persistentDataPath, WorldMapSavedAssetPrefixes + "save.worldmap"); }
    }

    string ReferenceImageSavePath
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, Path.Combine("../ReferenceImages", ReferenceImageSaveName));
#else
            return Path.Combine(Application.persistentDataPath, ReferenceImageSaveName);
#endif
        }
    }

    string ReferenceImageSaveName
    {
        get { return WorldMapSavedAssetPrefixes + "ReferenceImage.png"; }
    }

    string PlayerPrefAnchorIdKey { get { return WorldMapSavedAssetPrefixes + "AnchorId"; } }
    string PlayerPrefScaleXKey { get { return WorldMapSavedAssetPrefixes + "ScaleX"; } }
    string PlayerPrefScaleYKey { get { return WorldMapSavedAssetPrefixes + "ScaleY"; } }
    string PlayerPrefScaleZKey { get { return WorldMapSavedAssetPrefixes + "ScaleZ"; } }

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

    public void ChangeModelType()
    {
        ResetARSession();
        modelType++;
        if ((int)modelType == Enum.GetNames(typeof(ModelType)).Length)
        {
            modelType = 0;
        }
        modelTypeText.text = modelType.ToString();
    }

    public void ResetARSession()
    {
        ModelInstance.SetActive(false);
        referenceImage.SetActive(false);
        Session.RunWithConfigAndOptions(ARKitCameraManager.Instance.sessionConfiguration, UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking);
    }

    // Adding an anchor to the ARSession happens across two frames, so in order
    // to make sure `Session.GetCurrentWorldMapAsync` gets the map AFTER we add
    // our new anchor, we must call it in the next frame.
    public void SaveWorldMap()
    {
        StartCoroutine(SaveWorldMapCoroutine());
    }

    IEnumerator SaveWorldMapCoroutine()
    {
        SaveModel();

        // Wait one frame for model to save.
        yield return null;

        // Wait two frames? There seems to be a rare bug where the map doesn't save correctly.
        yield return null;

        Session.GetCurrentWorldMapAsync(OnWorldMap);
    }

    void OnWorldMap(ARWorldMap worldMap)
    {
#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot(ReferenceImageSaveName);
#else
        if (worldMap != null)
        {
            worldMap.Save(WorldMapSavePath);

            // Temporarily hide elements just for screenshot
            PointCloudGenerator.ToggleParticles(false);
            ScreenUI.SetActive(false);
            ModelInstance.SetActive(false);

            ScreenCapture.CaptureScreenshot(ReferenceImageSaveName);
            StartCoroutine(BecauseIOSScreenshotBehaviorIsUndefined());

            Debug.LogFormat("ARWorldMap saved to {0}", WorldMapSavePath);
        }
#endif
    }

    // Un-hide elements after waiting for screenshot to happen
    IEnumerator BecauseIOSScreenshotBehaviorIsUndefined()
    {
        // Wait a few frames
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        ScreenUI.SetActive(true);
        ModelInstance.SetActive(true);
        PointCloudGenerator.ToggleParticles(true);
    }

    public void LoadWorldMap()
    {
#if UNITY_EDITOR
        referenceImage.GetComponent<Image>().sprite = UtilitiesCR.LoadNewSprite(ReferenceImageSavePath);
        referenceImage.SetActive(true);
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

            referenceImage.GetComponent<Image>().sprite = UtilitiesCR.LoadNewSprite(ReferenceImageSavePath);
            referenceImage.SetActive(true);
        }
#endif
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        ModelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        ModelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        Debug.LogFormat("Added anchor: {0} | {1}", anchorData.identifier, ModelInstance.transform.position.ToString("F2"));

        if (PlayerPrefs.HasKey(PlayerPrefScaleXKey) && PlayerPrefs.HasKey(PlayerPrefScaleYKey) && PlayerPrefs.HasKey(PlayerPrefScaleZKey))
        {
            ModelInstance.transform.localScale = new Vector3(PlayerPrefs.GetFloat(PlayerPrefScaleXKey), PlayerPrefs.GetFloat(PlayerPrefScaleYKey), PlayerPrefs.GetFloat(PlayerPrefScaleZKey));
        }

        ModelInstance.SetActive(true);
    }

    void UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent(ARUserAnchor anchorData)
    {
        ModelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        ModelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

        Debug.LogFormat("Updated anchor: {0} | {1}", anchorData.identifier, ModelInstance.transform.position.ToString("F2"));
    }

    void UnityARSessionNativeInterface_ARUserAnchorRemovedEvent(ARUserAnchor anchorData)
    {
        ModelInstance.SetActive(false);

        Debug.LogFormat("Removed anchor: {0} | {1}", anchorData.identifier, ModelInstance.transform.position.ToString("F2"));
    }

    void SaveModel()
    {
        PlayerPrefs.SetFloat(PlayerPrefScaleXKey, ModelInstance.transform.localScale.x);
        PlayerPrefs.SetFloat(PlayerPrefScaleYKey, ModelInstance.transform.localScale.y);
        PlayerPrefs.SetFloat(PlayerPrefScaleZKey, ModelInstance.transform.localScale.z);

        if (PlayerPrefs.HasKey(PlayerPrefAnchorIdKey))
        {
            Session.RemoveUserAnchor(PlayerPrefs.GetString(PlayerPrefAnchorIdKey));
            PlayerPrefs.DeleteKey(PlayerPrefAnchorIdKey);
        }
        PlayerPrefs.SetString(PlayerPrefAnchorIdKey, Session.AddUserAnchorFromGameObject(ModelInstance).identifierStr);
    }

    void RotateModel(bool clockwise)
    {
        ModelInstance.transform.Rotate((clockwise ? 1 : -1) * rotationSpeed * Vector3.up * Time.deltaTime);
    }

    void ScaleModel(bool scaleUp)
    {
        Vector3 newScale = ModelInstance.transform.localScale + ((scaleUp ? 1 : -1) * scalingSpeed * Vector3.one * Time.deltaTime);
        if (newScale.x < 0 || newScale.y < 0 || newScale.z < 0)
        {
            return;
        }
        ModelInstance.transform.localScale = newScale;
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
                ModelInstance.transform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                ModelInstance.SetActive(true);
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

    void PopulateModelDictionary()
    {
        modelTypeToModelInstance[ModelType.Plaque] = Instantiate(plaquePrefab);
        modelTypeToModelInstance[ModelType.Plaque].SetActive(false);
        modelTypeToModelInstance[ModelType.Statue] = Instantiate(statuePrefab);
        modelTypeToModelInstance[ModelType.Statue].SetActive(false);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PopulateModelDictionary();
        modelTypeText.text = modelType.ToString();
        referenceImage.SetActive(false);
    }

    void Start()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent += UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent += UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
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
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent -= UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent -= UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
    }
}

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class WorldMapManager : MonoBehaviour
{
    [SerializeField] UnityARCameraManager m_ARCameraManager;
    [SerializeField] Image ReferenceImage;
    [SerializeField] Toggle TestModeToggle;

    ARWorldMap m_LoadedMap;
    serializableARWorldMap serializedWorldMap;
    ARTrackingStateReason m_LastReason;
    bool testMode;

    void Start()
    {
        testMode = TestModeToggle.isOn;
        TestModeToggle.onValueChanged.AddListener(OnTestModeToggle);
        UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization = true;
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += OnFrameUpdate;
    }

    void OnDestroy()
    {
        TestModeToggle.onValueChanged.RemoveListener(OnTestModeToggle);
        UnityARSessionNativeInterface.ARFrameUpdatedEvent -= OnFrameUpdate;
    }

    void OnTestModeToggle(bool isOn)
    {
        testMode = isOn;
    }

    void OnFrameUpdate(UnityARCamera arCamera)
    {
        if (arCamera.trackingReason != m_LastReason)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            //Debug.LogFormat("worldTransform: {0}", arCamera.worldTransform.column3);
            //Debug.LogFormat("trackingState: {0} {1}", arCamera.trackingState, arCamera.trackingReason);
            m_LastReason = arCamera.trackingReason;
        }
    }

    static UnityARSessionNativeInterface session
    {
        get { return UnityARSessionNativeInterface.GetARSessionNativeInterface(); }
    }

    string WorldMapSavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, testMode ? "TESTmyFirstWorldMap.worldmap" : "myFirstWorldMap.worldmap");
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
            return saveName + (testMode ? "TEST_" : "") + "ReferenceImage.png";
        }
    }

    void OnWorldMap(ARWorldMap worldMap)
    {
        // TODO
        UnityARHitTestExample.Instance.RecordModelScale();
        PlayerPrefs.Save();

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

    public void Save()
    {
        session.GetCurrentWorldMapAsync(OnWorldMap);
    }

    public void Load()
    {
#if UNITY_EDITOR
        ReferenceImage.sprite = UtilitiesCR.LoadNewSprite(Application.dataPath + "/../" + ReferenceImageSaveName);
        ReferenceImage.color = new Color(1, 1, 1, 1);
#else
        Debug.LogFormat("Loading ARWorldMap {0}", WorldMapSavePath);
        var worldMap = ARWorldMap.Load(WorldMapSavePath);
        if (worldMap != null)
        {
            m_LoadedMap = worldMap;
            Debug.LogFormat("Map loaded. Center: {0} Extent: {1}", worldMap.center, worldMap.extent);

            var config = m_ARCameraManager.sessionConfiguration;
            config.worldMap = worldMap;
            UnityARSessionRunOption runOption = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking;

            Debug.Log("Restarting session with worldMap");
            session.RunWithConfigAndOptions(config, runOption);
            
            ReferenceImage.sprite = UtilitiesCR.LoadNewSprite(Application.persistentDataPath + "/" + ReferenceImageSaveName);
            ReferenceImage.color = new Color(1, 1, 1, 1);
        }
#endif
    }

    public void ResetARSession()
    {
        UnityARHitTestExample.Instance.ResetModel();
        session.RunWithConfigAndOptions(m_ARCameraManager.sessionConfiguration, UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking);
    }
}

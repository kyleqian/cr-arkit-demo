using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class PrototypeManager : MonoBehaviour
{
    [SerializeField] GameObject referenceImage;
    [SerializeField] string referenceImageFilename;
    [SerializeField] string worldMapFilename;
    [SerializeField] string playerPrefScaleXKey;
    [SerializeField] string playerPrefScaleYKey;
    [SerializeField] string playerPrefScaleZKey;

    [SerializeField] GameObject modelPrefab;
    GameObject modelInstance;

    UnityARSessionNativeInterface Session
    {
        get { return UnityARSessionNativeInterface.GetARSessionNativeInterface(); }
    }

    string WorldMapPath
    {
        get { return Path.Combine(Application.persistentDataPath, worldMapFilename); }
    }

    string ReferenceImagePath
    {
        get { return Path.Combine(Application.persistentDataPath, referenceImageFilename); }
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        modelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        modelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

        Debug.LogFormat("Added anchor: {0} | {1}", anchorData.identifier, modelInstance.transform.position.ToString("F2"));

        if (PlayerPrefs.HasKey(playerPrefScaleXKey) && PlayerPrefs.HasKey(playerPrefScaleYKey) && PlayerPrefs.HasKey(playerPrefScaleZKey))
        {
            modelInstance.transform.localScale = new Vector3(PlayerPrefs.GetFloat(playerPrefScaleXKey), PlayerPrefs.GetFloat(playerPrefScaleYKey), PlayerPrefs.GetFloat(playerPrefScaleZKey));
        }

        // TODO
        modelInstance.SetActive(true);
        referenceImage.SetActive(false);
    }

    void UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent(ARUserAnchor anchorData)
    {
        modelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        modelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

        Debug.LogFormat("Updated anchor: {0} | {1}", anchorData.identifier, modelInstance.transform.position.ToString("F2"));
    }

    void UnityARSessionNativeInterface_ARUserAnchorRemovedEvent(ARUserAnchor anchorData)
    {
        modelInstance.SetActive(false);

        Debug.LogFormat("Removed anchor: {0} | {1}", anchorData.identifier, modelInstance.transform.position.ToString("F2"));
    }

    void LoadWorldMap()
    {
        Debug.LogFormat("Loading ARWorldMap {0}", WorldMapPath);
        var worldMap = ARWorldMap.Load(WorldMapPath);
        if (worldMap != null)
        {
            Debug.LogFormat("Map loaded. Center: {0} Extent: {1}", worldMap.center, worldMap.extent);

            var config = ARKitCameraManager.Instance.sessionConfiguration;
            config.worldMap = worldMap;
            UnityARSessionRunOption runOption = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking;

            Debug.Log("Restarting session with worldMap");
            Session.RunWithConfigAndOptions(config, runOption);

            referenceImage.GetComponent<Image>().sprite = UtilitiesCR.LoadNewSprite(ReferenceImagePath);
            referenceImage.SetActive(true);
        }
    }

    void Awake()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        modelInstance = Instantiate(modelPrefab);
        modelInstance.SetActive(false);
        referenceImage.SetActive(false);
    }

    void Start()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent += UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent += UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
        UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization = true;
        LoadWorldMap();
    }

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent -= UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent -= UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
    }
}

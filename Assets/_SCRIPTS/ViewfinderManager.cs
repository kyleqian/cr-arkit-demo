using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class ViewfinderManager : MonoBehaviour
{
    [SerializeField] string worldMapFilename;
    [SerializeField] string playerPrefScaleXKey;
    [SerializeField] string playerPrefScaleYKey;
    [SerializeField] string playerPrefScaleZKey;

    [SerializeField] GameObject modelPrefab;
    GameObject modelInstance;

    string WorldMapPath
    {
        get { return Path.Combine(Application.persistentDataPath, worldMapFilename); }
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        modelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        modelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        modelInstance.transform.localScale = new Vector3(PlayerPrefs.GetFloat(playerPrefScaleXKey), PlayerPrefs.GetFloat(playerPrefScaleYKey), PlayerPrefs.GetFloat(playerPrefScaleZKey));
        modelInstance.SetActive(true);

        Debug.LogFormat("Added anchor: {0} | {1}", anchorData.identifier, modelInstance.transform.position.ToString("F2"));
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

            var config = ARKitManager.Instance.DefaultSessionConfiguration;
            config.worldMap = worldMap;
            UnityARSessionRunOption runOption = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking;

            Debug.Log("Restarting session with worldMap");
            ARKitManager.Instance.Session.RunWithConfigAndOptions(config, runOption);
        }
    }

    void Awake()
    {
        modelInstance = Instantiate(modelPrefab);
        modelInstance.SetActive(false);
    }

    void Start()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
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

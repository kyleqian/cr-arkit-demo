using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class Viewfinder : MonoBehaviour
{
    [Header("Worldmap & Model")]
    [SerializeField] string worldMapFilename;
    [SerializeField] string playerPrefScaleXKey;
    [SerializeField] string playerPrefScaleYKey;
    [SerializeField] string playerPrefScaleZKey;
    [SerializeField] GameObject modelPrefab;
    [SerializeField] float modelFadeInDuration;
    GameObject modelInstance;

    [Header("Anchoring Mode")]
    [Tooltip("UI elements that help the user anchor")]
    [SerializeField] MaskableGraphic[] anchoringModeUIElements;
    [SerializeField] float anchoringUIFadeOutDuration;

    string WorldMapPath
    {
        get { return Path.Combine(Application.persistentDataPath, worldMapFilename); }
    }

    void FadeOutAnchoringUIElements()
    {
        foreach (var elem in anchoringModeUIElements)
        {
            elem.CrossFadeAlpha(0, anchoringUIFadeOutDuration, false);
        }
    }

    void FadeInModel()
    {
        StartCoroutine(FadeInModelCoroutine());
    }

    IEnumerator FadeInModelCoroutine()
    {
        // Change rendering mode to "Fade" to fade properly
        UpdateModelRenderingMode(UtilitiesCR.BlendMode.Fade);

        // Make model invisible
        UpdateModelAlpha(0);

        for (float t = 0.0f; t < 1.0; t += Time.deltaTime / modelFadeInDuration)
        {
            UpdateModelAlpha(t);
            yield return null;
        }

        // Change rendering mode to "Opaque" to display without weird transparency issues
        UpdateModelRenderingMode(UtilitiesCR.BlendMode.Opaque);
    }

    void UpdateModelRenderingMode(UtilitiesCR.BlendMode blendMode)
    {
        foreach (Transform child in modelInstance.transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer.material.shader.name == "Standard")
            {
                UtilitiesCR.ChangeRenderMode(renderer.material, blendMode);
            }
        }
    }

    void UpdateModelAlpha(float newAlpha)
    {
        foreach (Transform child in modelInstance.transform)
        {
            var tmpro = child.GetComponent<TextMeshPro>();
            if (tmpro != null)
            {
                tmpro.alpha = newAlpha;
            }
            else
            {
                Renderer renderer = child.GetComponent<Renderer>();
                Color newColor = renderer.material.color;
                newColor.a = newAlpha;
                renderer.material.color = newColor;
            }
        }
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        // Position model
        modelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        modelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        modelInstance.transform.localScale = new Vector3(PlayerPrefs.GetFloat(playerPrefScaleXKey), PlayerPrefs.GetFloat(playerPrefScaleYKey), PlayerPrefs.GetFloat(playerPrefScaleZKey));

        // Fade out anchoring UI now that anchoring was achieved
        FadeOutAnchoringUIElements();


        // Fade in newly placed model
        FadeInModel();
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

using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ViewfinderAR : MonoBehaviour
{
    [SerializeField] string worldMapFilename;
    [SerializeField] string playerPrefScaleXKey;
    [SerializeField] string playerPrefScaleYKey;
    [SerializeField] string playerPrefScaleZKey;
    [SerializeField] GameObject modelPrefab;
    [SerializeField] float modelFadeInDuration;
    GameObject modelInstance;

    string WorldMapPath
    {
        get { return Path.Combine(Application.persistentDataPath, worldMapFilename); }
    }

    void FadeInModel()
    {
        StartCoroutine(FadeInModelCoroutine());
    }

    IEnumerator FadeInModelCoroutine()
    {
        // Change rendering mode to "Fade" to fade properly
        UpdateModelRenderingMode(UtilitiesCR.BlendMode.Fade);

        // Start with model fully transparent
        UpdateModelAlpha(0);

        for (float alpha = 0.0f; alpha < 1.0; alpha += Time.deltaTime / modelFadeInDuration)
        {
            UpdateModelAlpha(alpha);
            yield return null;
        }

        // Change rendering mode to "Opaque" to display without weird transparency issues
        UpdateModelRenderingMode(UtilitiesCR.BlendMode.Opaque);
    }

    void UpdateModelAlpha(float newAlpha)
    {
        UpdateAlphaRecursive(modelInstance.transform, newAlpha);
    }

    void UpdateAlphaRecursive(Transform target, float newAlpha)
    {
        foreach (Transform child in target.transform)
        {
            UpdateAlphaRecursive(child, newAlpha);
        }

        var tmpro = target.GetComponent<TextMeshPro>();
        if (tmpro != null)
        {
            tmpro.alpha = newAlpha;
        }
        else
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color newColor = renderer.material.color;
                newColor.a = newAlpha;
                renderer.material.color = newColor;
            }
        }
    }

    void UpdateModelRenderingMode(UtilitiesCR.BlendMode blendMode)
    {
        UpdateRenderingModeRecursive(modelInstance.transform, blendMode);
    }

    void UpdateRenderingModeRecursive(Transform target, UtilitiesCR.BlendMode blendMode)
    {
        foreach (Transform child in target.transform)
        {
            UpdateRenderingModeRecursive(child, blendMode);
        }

        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null && renderer.material.shader.name == "Standard")
        {
            UtilitiesCR.ChangeRenderMode(renderer.material, blendMode);
        }
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        // Position model
        modelInstance.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        modelInstance.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        modelInstance.transform.localScale = new Vector3(PlayerPrefs.GetFloat(playerPrefScaleXKey), PlayerPrefs.GetFloat(playerPrefScaleYKey), PlayerPrefs.GetFloat(playerPrefScaleZKey));

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

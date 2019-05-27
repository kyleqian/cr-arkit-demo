using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ViewfinderAR : MonoBehaviour
{
    [SerializeField] string playerPrefKeyPrefix;
    [SerializeField] float modelFadeInDuration;
    [SerializeField] GameObject scenePrefab;
    [SerializeField] GameObject tapToPlaceParent;
    GameObject sceneInstance;

    bool AnchoringFound = false;
    float TimeElapsed = 0f;
    float MaxTimeAnchorSearch = 10f;

    void FadeInModel(Transform model)
    {
        StartCoroutine(FadeInModelCoroutine(model));
    }

    IEnumerator FadeInModelCoroutine(Transform model)
    {
        // Change rendering mode to "Fade" to fade properly
        UpdateModelRenderingMode(model, BlendMode.Fade);

        // Start with model fully transparent
        UpdateModelAlpha(model, 0);

        for (float alpha = 0.0f; alpha < 1.0; alpha += Time.deltaTime / modelFadeInDuration)
        {
            UpdateModelAlpha(model, alpha);
            yield return null;
        }

        // Change rendering mode to "Opaque" to display without weird transparency issues
        UpdateModelRenderingMode(model, BlendMode.Opaque);
    }

    void UpdateModelAlpha(Transform model, float newAlpha)
    {
        UpdateAlphaRecursive(model, newAlpha);
    }

    void UpdateAlphaRecursive(Transform target, float newAlpha)
    {
        foreach (Transform child in target)
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
                foreach (Material m in renderer.materials)
                {
                    Color newColor = m.color;
                    newColor.a = newAlpha;
                    m.color = newColor;
                }
            }
        }
    }

    void UpdateModelRenderingMode(Transform model, BlendMode blendMode)
    {
        UpdateRenderingModeRecursive(model, blendMode);
    }

    void UpdateRenderingModeRecursive(Transform target, BlendMode blendMode)
    {
        foreach (Transform child in target)
        {
            UpdateRenderingModeRecursive(child, blendMode);
        }

        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (Material m in renderer.materials)
            {
                if (m.shader.name == "Standard")
                {
                    UtilitiesCR.ChangeRenderMode(m, blendMode);
                }
            }
        }
    }

    // TODO: Remove these?
    Transform GetChildForAnchorId(string anchorId)
    {
        foreach (Transform child in sceneInstance.transform)
        {
            if (anchorId == PlayerPrefs.GetString(GlobalMapManager.Instance.GetPlayerPrefAnchorIdKey(child.gameObject)))
            {
                return child;
            }
        }
        return null;
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        AnchoringFound = true;

        Transform child = GetChildForAnchorId(anchorData.identifier);

        child.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        child.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        child.localScale = new Vector3(PlayerPrefs.GetFloat(GlobalMapManager.Instance.GetPlayerPrefScaleXKey(anchorData.identifier)), PlayerPrefs.GetFloat(GlobalMapManager.Instance.GetPlayerPrefScaleYKey(anchorData.identifier)), PlayerPrefs.GetFloat(GlobalMapManager.Instance.GetPlayerPrefScaleZKey(anchorData.identifier)));

        // Fade in newly placed model
        FadeInModel(child);
        child.gameObject.SetActive(true);

        Debug.LogFormat("Added anchor: {0} | {1}", anchorData.identifier, child.position.ToString("F2"));
    }

    void UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent(ARUserAnchor anchorData)
    {
        Transform child = GetChildForAnchorId(anchorData.identifier);

        child.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        child.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

        Debug.LogFormat("Updated anchor: {0} | {1}", anchorData.identifier, child.position.ToString("F2"));
    }

    void UnityARSessionNativeInterface_ARUserAnchorRemovedEvent(ARUserAnchor anchorData)
    {
        Transform child = GetChildForAnchorId(anchorData.identifier);

        child.gameObject.SetActive(false);

        Debug.LogFormat("Removed anchor: {0} | {1}", anchorData.identifier, child.position.ToString("F2"));
    }

    void LoadWorldMap()
    {
        Debug.Log("Attempting to load ARWorldMap.");

        //GlobalMapManager.Instance.DownloadLatestMap(() =>
        //{
        //    var worldMap = ARWorldMap.Load(GlobalMapManager.Instance.WorldMapPath);
        //    if (worldMap != null)
        //    {
        //        Debug.LogFormat("Map loaded. Center: {0} Extent: {1}", worldMap.center, worldMap.extent);

        //        var config = ARKitManager.Instance.DefaultSessionConfiguration;
        //        config.worldMap = worldMap;
        //        UnityARSessionRunOption runOption = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking;

        //        Debug.Log("Restarting session with worldMap");
        //        ARKitManager.Instance.Session.RunWithConfigAndOptions(config, runOption);
        //    }
        //});

        var worldMap = ARWorldMap.Load(GlobalMapManager.Instance.WorldMapPath);
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

    void SetChildrenActive(bool active)
    {
        foreach (Transform child in sceneInstance.transform)
        {
            child.gameObject.SetActive(active);
        }
    }

    IEnumerator InitializeTapToPlace()
    {
        FindObjectOfType<AnchoringUI>().FadeOut();
        yield return new WaitForSeconds(1f);
        tapToPlaceParent.SetActive(true);
    }

    void Awake()
    {
        sceneInstance = Instantiate(scenePrefab);
        SetChildrenActive(false);
    }

    void Start()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent += UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent += UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
        UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization = true;
        LoadWorldMap();
    }

    void Update()
    {
        // wait for 10 sec; if no worldmap found; go into
        if (TimeElapsed > MaxTimeAnchorSearch)
        {
            TimeElapsed = MaxTimeAnchorSearch;
            // Start tap to place here
            StartCoroutine(InitializeTapToPlace()); 
        }
        else if (TimeElapsed < MaxTimeAnchorSearch)
        {
            TimeElapsed += Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent -= UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent -= UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
    }
}

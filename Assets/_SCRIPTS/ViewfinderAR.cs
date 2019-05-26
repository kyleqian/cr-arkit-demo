using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ViewfinderAR : MonoBehaviour
{
    [SerializeField] string playerPrefKeyPrefix;
    [SerializeField] float modelFadeInDuration;
    [SerializeField] GameObject scenePrefab;
    GameObject sceneInstance;

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
            if (anchorId == PlayerPrefs.GetString(GetPlayerPrefAnchorIdKey(child.gameObject)))
            {
                return child;
            }
        }
        return null;
    }

    string GetPlayerPrefAnchorIdKey(GameObject g)
    {
        return string.Format("{0}_{1}_AnchorId", playerPrefKeyPrefix, g.name);
    }

    string GetPlayerPrefScaleXKey(string anchorId)
    {
        return string.Format("{0}_{1}_ScaleX", playerPrefKeyPrefix, anchorId);
    }

    string GetPlayerPrefScaleYKey(string anchorId)
    {
        return string.Format("{0}_{1}_ScaleY", playerPrefKeyPrefix, anchorId);
    }

    string GetPlayerPrefScaleZKey(string anchorId)
    {
        return string.Format("{0}_{1}_ScaleZ", playerPrefKeyPrefix, anchorId);
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        Transform child = GetChildForAnchorId(anchorData.identifier);

        child.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        child.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        child.localScale = new Vector3(PlayerPrefs.GetFloat(GetPlayerPrefScaleXKey(anchorData.identifier)), PlayerPrefs.GetFloat(GetPlayerPrefScaleYKey(anchorData.identifier)), PlayerPrefs.GetFloat(GetPlayerPrefScaleZKey(anchorData.identifier)));

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
        GlobalMapManager.Instance.DownloadLatestMap(() =>
        {
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
        });
    }

    void SetChildrenActive(bool active)
    {
        foreach (Transform child in sceneInstance.transform)
        {
            child.gameObject.SetActive(active);
        }
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

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent -= UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
        UnityARSessionNativeInterface.ARUserAnchorRemovedEvent -= UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
    }
}

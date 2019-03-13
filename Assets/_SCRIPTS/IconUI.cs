using TMPro;
using UnityEngine;
using UnityEngine.XR.iOS;

public class IconUI : MonoBehaviour
{
    [SerializeField] GameObject trackerInfo;
    [SerializeField] TextMeshProUGUI trackerCount;

    void Start()
    {
        trackerInfo.SetActive(false);
        OnDatabaseUpdated();
        GlobalDatabase.Instance.DatabaseUpdated.AddListener(OnDatabaseUpdated);
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
    }

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
        GlobalDatabase.Instance.DatabaseUpdated.RemoveListener(OnDatabaseUpdated);
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        // Show tracker instrutions/text
        trackerInfo.SetActive(true);
    }

    void OnDatabaseUpdated()
    {
        var voiceCounts = GlobalDatabase.Instance.GetVoiceCounts();
        trackerCount.text = voiceCounts.Item1.ToString() + "/" + voiceCounts.Item2.ToString();
    }
}

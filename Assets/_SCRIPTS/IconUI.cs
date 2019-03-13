using TMPro;
using UnityEngine;
using UnityEngine.XR.iOS;

public class IconUI : MonoBehaviour
{
    [SerializeField] GameObject trackerText;
    [SerializeField] TextMeshProUGUI trackerCount;

    void Start()
    {
        trackerText.SetActive(false);
        //trackerCount.text = Globa
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
    }

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        // Show tracker instrutions/text
        trackerText.SetActive(true);
    }
}

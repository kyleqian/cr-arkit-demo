using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.iOS;

public class ViewfinderUI : MonoBehaviour
{
    [SerializeField] AnchoringUI anchoringUI;
    [SerializeField] ContentUI contentUI;

    public void HomeButton()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }

    public void ResetButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    // Extra level of indirection because ContentUI cannot be found with Find() if it's disabled
    public void ShowContentUI()
    {
        contentUI.ShowSelf();
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        // Hide anchoring canvas now that anchoring was achieved
        anchoringUI.FadeOut();
    }

    void Start()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
    }

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
    }
}

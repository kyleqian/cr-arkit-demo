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

    void Start()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
    }

    void Update()
    {
        DetectLetterTouch();
    }

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
    }

    void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
    {
        // Hide anchoring canvas now that anchoring is achieved
        anchoringUI.FadeOut();
    }

    void DetectLetterTouch()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(raycast, out RaycastHit raycastHit))
            {
                contentUI.HideSelf();
                switch (raycastHit.collider.tag)
                {
                    case "LetterCups":
                        contentUI.ShowSelf(GlobalDatabase.Instance.FindVoiceByName("SL"));
                        break;
                    case "LetterFlashlight":
                        contentUI.ShowSelf(GlobalDatabase.Instance.FindVoiceByName("AK"));
                        break;
                    case "LetterFlower":
                        contentUI.ShowSelf(GlobalDatabase.Instance.FindVoiceByName("MS"));
                        break;
                    case "LetterPlaque":
                        contentUI.ShowSelf(GlobalDatabase.Instance.FindVoiceByName("JT"));
                        break;
                }
            }
            else
            {
                // Tapped outside letter, so exit.
                contentUI.HideSelf();
            }
        }
    }
}

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
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit))
            {
                switch (raycastHit.collider.tag)
                {
                    case "LetterCups":
                        contentUI.ShowSelf(GlobalDatabase.Instance.FindVoiceByName("Gabby"));
                        break;
                    case "LetterFlashlight":
                        break;
                    case "LetterFlower":
                        break;
                    case "LetterPlaque":
                        break;
                }
            }
        }
    }
}

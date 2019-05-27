using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

/// <summary>
/// Controls all Dandelion AR features.
/// </summary>
public class TapToPlaceManager : MonoBehaviour
{
    public static TapToPlaceManager Instance;
    public Transform hitTransform;
    public SearchReticle searchReticle;

    // ARCore Related
    public Camera mainCamera;
    public GameObject detectedPlanePrefab;

    public float dropSpeed;
    public GameObject dropShadowObject;
    public Vector3 dropShadowSize;

    public GameObject displayStorySeed;
    public GameObject storySeed;

    [SerializeField] Animator tapToPlaceAnimator;
    [SerializeField] GameObject trackerInfo; // TODO: FIX ALL THIS SHIT.

    public enum StoryState
    {
        BeforeFirstTap,
        Playing,
        Replaying
    }

    public StoryState storyState;

    public GameObject problemText;
    public GameObject lookAroundText;
    public GameObject tapText;
    public AudioSource dropSound;

    private void Awake()
    {
        Instance = this;
        storyState = StoryState.BeforeFirstTap;
        // Set up the camera correctly
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    #region Story State

    public void InitializeStorySeed()
    {
        // Hide the square that shows "found"
        searchReticle.HideReticle = true;
        // Hide the display object and drop shadow
        displayStorySeed.SetActive(false);
        dropShadowObject.SetActive(false);
        // Vibrate the device for HAPTIC PLEASUREZ
        Handheld.Vibrate();
        // Play sound for AUDITORY PLEASUREZ
        dropSound.Play();

        // TODO: FIX ALL THIS SHIT.
        trackerInfo.SetActive(true);

        // Destroy all intro text sequences
        Destroy(problemText);
        Destroy(lookAroundText);
        Destroy(tapText);
#if UNITY_ANDROID
        // ARCoreManager.detectedPlanesVisualizer.SetActive(false);
        ARCoreManager.pointCloudVisualizer.SetActive(false);
#endif
        StartCoroutine(DropStorySeed());
    }

    IEnumerator DropStorySeed()
    {
        tapToPlaceAnimator.SetTrigger("FadeOff");

        float time = 0f;
        float verticalDropOffset = -0.1f;
        Vector3 startPos = storySeed.transform.localPosition;
        while (time < 1f)
        {
            // move the seed towards the ground
            storySeed.transform.localPosition = Vector3.Lerp(storySeed.transform.localPosition,
                new Vector3(0f, verticalDropOffset, 0f), dropSpeed * time);
            // shadow grows as the seed approaches ground
            dropShadowObject.transform.localScale = Vector3.Lerp(dropShadowObject.transform.localScale,
                dropShadowSize, dropSpeed * time);
            time += Time.deltaTime;
            yield return null;
        }
        // turn off shadow once seed hits ground to prevent z-fighting etc
        dropShadowObject.SetActive(false);
        tapToPlaceAnimator.gameObject.SetActive(false);

    }
    #endregion
    #region Universal Functions
    public bool IsSeedOnScreen()
    {
        if (mainCamera == null)
        {
            return false;
        }
        Vector3 screenPointCenter = mainCamera.WorldToViewportPoint(hitTransform.position);
        Vector3 screenPointHigh = mainCamera.WorldToViewportPoint(hitTransform.position
            + new Vector3(0f, 0.25f, 0f));
        Vector3 screenPointHighBack = mainCamera.WorldToViewportPoint(hitTransform.position
            + new Vector3(0f, 0.3f, 0f) + mainCamera.transform.forward.normalized);
        bool onScreenCenter = screenPointCenter.z > 0 && screenPointCenter.x > 0 && screenPointCenter.x < 1
            && screenPointCenter.y > 0 && screenPointCenter.y < 1;
        bool onScreenHigh = screenPointHigh.z > 0 && screenPointHigh.x > 0 && screenPointHigh.x < 1 &&
            screenPointHigh.y > 0 && screenPointHigh.y < 1;
        bool onScreenHighBack = screenPointHighBack.z > 0 && screenPointHighBack.x > 0 && screenPointHighBack.x < 1
            && screenPointHighBack.y > 0 && screenPointHighBack.y < 1;
        bool closeToGlobe = (mainCamera.transform.position - hitTransform.position).magnitude < 0.5f;
        return onScreenCenter || onScreenHigh || onScreenHighBack || closeToGlobe;
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        //if (StateManager.Instance.currentState == StateManager.GameState.Story && IsSeedOnScreen())
        //{
        //    searchReticle.HideReticle = true;
        //}
        //else
        //{
        //    searchReticle.HideReticle = false;
        //}

        if (storyState == StoryState.Replaying)
        {
            storyState = StoryState.Playing;
        }
        // When user taps the screen
        else if (Input.touchCount > 0 && hitTransform != null)
        {
            if (searchReticle.ReticleState != SearchReticle.FocusState.Found)
            {
                // no surface to anchor to yet
                return;
            }

            // Get the touch
            var touch = Input.GetTouch(0);
            // When touch begins or is dragging
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                var touchScreenPosition = mainCamera.ScreenToViewportPoint(touch.position);
                // TODO: fix this jank customized replay button!
                //if (StateManager.Instance.currentState == StateManager.GameState.End && touchScreenPosition.x >= 0.4f && touchScreenPosition.x <= 0.6f && touchScreenPosition.y >= 0.1f && touchScreenPosition.y <= 0.3f)
                //{
                //    return;
                //}

#if UNITY_IOS
                ARKitProcessHit(touchScreenPosition);
#endif
            }
        }
#if UNITY_EDITOR
        // Using editor
#endif
    }

    #region ARKit
    // iOS - ARKit related functions
    private void ARKitProcessHit(Vector3 touchScreenPosition)
    {
        // Using iOS - ARKit
        ARPoint point = new ARPoint
        {
            x = touchScreenPosition.x,
            y = touchScreenPosition.y
        };
        // prioritize reults types
        ARHitTestResultType[] resultTypes = {
                    ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                    // if you want to use infinite planes use this:
                    //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                    ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane,
                    ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                };
        if (storyState == StoryState.BeforeFirstTap)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2 - Screen.height/12, 0f);

            touchScreenPosition = mainCamera.ScreenToViewportPoint(screenCenter);
            point = new ARPoint
            {
                x = touchScreenPosition.x,
                y = touchScreenPosition.y
            };
        }

        foreach (ARHitTestResultType resultType in resultTypes)
        {
            if (storyState == StoryState.BeforeFirstTap)
            {
                if (ARKitHitTestWithResultType(point, resultType))
                {
                    storyState = StoryState.Playing;
                    InitializeStorySeed();
                    // play sounds, vibrate

                    displayStorySeed.SetActive(false); //transform.GetChild (0).gameObject.SetActive (false);
                    dropShadowObject.SetActive(false);
                }
                return;
            }
        }
    }
    bool ARKitHitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
    {
        List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
        if (hitResults.Count > 0)
        {
            foreach (var hitResult in hitResults)
            {
                Debug.Log("Got hit!");
                hitTransform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                hitTransform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                hitTransform.LookAt(new Vector3(mainCamera.transform.position.x, hitTransform.position.y, mainCamera.transform.position.z));
                Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", hitTransform.position.x, hitTransform.position.y, hitTransform.position.z));
                return true;
            }
        }
        return false;
    }
    #endregion
}

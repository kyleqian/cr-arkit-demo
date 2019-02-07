using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContentUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] GameObject text;
    [SerializeField] GameObject volumeOn;
    [SerializeField] GameObject volumeOff;
    [SerializeField] RectTransform canvasTransform;

    [Header("Audio")]
    [SerializeField] AudioClip[] audioClips;

    [Header("Animations")]
    [SerializeField] AnimationCurve canvasEaseCurve;

    const string VOLUME_ONOFF_KEY = "VOLUME_ONOFF_KEY";

    AudioSource audioSource;
    Coroutine audioCoroutine;
    Vector3 canvasTransformInitialPosition;

    public void ShowSelf()
    {
        if (canvasTransform.gameObject.activeSelf)
        {
            return;
        }

        // Start below screen
        Vector3 newPosition = canvasTransformInitialPosition;
        newPosition.y -= canvasTransform.rect.height;
        canvasTransform.position = newPosition;

        // Play audio
        audioCoroutine = StartCoroutine(PlayAudio());

        // Ease in
        StartCoroutine(EaseCanvas(true, 1.5f));
    }

    public void HideSelf()
    {
        if (!canvasTransform.gameObject.activeSelf)
        {
            return;
        }

        // Start above screen
        canvasTransform.position = canvasTransformInitialPosition;

        // Stop audio
        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
        }
        audioSource.Stop();

        // Ease out
        StartCoroutine(EaseCanvas(false, 1.5f));
    }

    public void ToggleVolume()
    {
        if (volumeOn.activeSelf)
        {
            volumeOn.SetActive(false);
            volumeOff.SetActive(true);
            audioSource.mute = true;
            PlayerPrefs.SetInt(VOLUME_ONOFF_KEY, 0);
        }
        else
        {
            volumeOn.SetActive(true);
            volumeOff.SetActive(false);
            audioSource.mute = false;
            PlayerPrefs.SetInt(VOLUME_ONOFF_KEY, 1);
        }
    }

    IEnumerator EaseCanvas(bool easeIn, float duration)
    {
        if (easeIn)
        {
            canvasTransform.gameObject.SetActive(true);
        }

        float startingY = canvasTransform.position.y;
        float targetY = easeIn ? canvasTransformInitialPosition.y : canvasTransformInitialPosition.y - canvasTransform.rect.height;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            Vector3 newPosition = canvasTransform.position;
            newPosition.y = (targetY - startingY) * canvasEaseCurve.Evaluate(t) + startingY;
            canvasTransform.position = newPosition;
            yield return null;
        }

        if (!easeIn)
        {
            canvasTransform.gameObject.SetActive(false);
        }
    }

    void OnAudioFinished()
    {
        HideSelf();
    }

    IEnumerator PlayAudio()
    {
        // Call IndestructibleSceneTracker here because SceneManager.activeSceneChanged seems to be called in Awake
        // UPDATE TO USE SCRITABLEOBJECTS
        if (GlobalSceneTracker.Instance.GetCountForSceneIndex(SceneManager.GetActiveScene().buildIndex) <= 1)
        {
            audioSource.clip = audioClips[0];
        }
        else
        {
            audioSource.clip = audioClips[1];
        }

        // Delay
        yield return new WaitForSeconds(5);

        // Play audio
        audioSource.Play();
        Invoke("OnAudioFinished", audioSource.clip.length);
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        canvasTransform.gameObject.SetActive(false);
        canvasTransformInitialPosition = canvasTransform.position;
        if (PlayerPrefs.GetInt(VOLUME_ONOFF_KEY, 1) == 0)
        {
            volumeOn.SetActive(false);
            audioSource.mute = true;
        }
        else
        {
            volumeOff.SetActive(false);
        }
    }
}

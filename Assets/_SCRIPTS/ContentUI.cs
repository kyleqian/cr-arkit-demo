using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContentUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] GameObject text;
    [SerializeField] GameObject signature;
    [SerializeField] GameObject button;
    [SerializeField] GameObject icon;

    [Header("Audio")]
    [SerializeField] AudioClip[] audioClips;
    AudioSource audioSource;
    Coroutine audioCoroutine;

    public void ShowSelf()
    {
        // TODO: Make sure this isn't called multiple times.
        gameObject.SetActive(true);
    }

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }

    void OnAudioFinished()
    {
        icon.SetActive(false);
        text.SetActive(false);
        signature.SetActive(false);
        button.SetActive(true);
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
        yield return new WaitForSeconds(3);

        // Play audio
        audioSource.Play();
        Invoke("OnAudioFinished", audioSource.clip.length);
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        button.SetActive(false);
        icon.SetActive(true);
        text.SetActive(true);
        signature.SetActive(true);
        audioCoroutine = StartCoroutine(PlayAudio());
    }

    void OnDisable()
    {
        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
        }
        audioSource.Stop();
    }
}

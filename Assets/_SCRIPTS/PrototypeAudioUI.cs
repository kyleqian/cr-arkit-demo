using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeAudioUI : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] GameObject button;
    [SerializeField] GameObject background;

    [Header("Audio Clips")]
    [SerializeField] AudioClip clip1;
    [SerializeField] AudioClip clip2;

    AudioSource audioSource;

    public void ReturnToPlaqueScene()
    {
        SceneManager.LoadScene("PrototypeMode2", LoadSceneMode.Single);
    }

    void OnAudioFinished()
    {
        background.SetActive(false);
        text.SetActive(false);
        button.SetActive(true);
    }

    IEnumerator PlayAudio()
    {
        // Delay
        yield return new WaitForSeconds(3);

        // Play audio
        audioSource.Play();
        Invoke("OnAudioFinished", audioSource.clip.length);
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Input.multiTouchEnabled = false;
        button.SetActive(false);
    }

    // Call IndestructibleSceneTracker here because SceneManager.activeSceneChanged seems to be called in Awake.
    void Start()
    {
        if (IndestructibleSceneTracker.Instance.GetCountForSceneIndex(SceneManager.GetActiveScene().buildIndex) <= 1)
        {
            audioSource.clip = clip1;
        }
        else
        {
            audioSource.clip = clip2;
        }
        StartCoroutine(PlayAudio());
    }
}

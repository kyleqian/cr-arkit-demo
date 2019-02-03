using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Letter : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] GameObject signature;
    [SerializeField] GameObject button;
    [SerializeField] GameObject icon;
    [Space]
    [SerializeField] AudioClip[] audioClips;

    AudioSource audioSource;

    public void ReturnToViewfinder()
    {
        SceneManager.LoadScene("Viewfinder", LoadSceneMode.Single);
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
        // Delay
        yield return new WaitForSeconds(3);

        // Play audio
        audioSource.Play();
        Invoke("OnAudioFinished", audioSource.clip.length);
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        button.SetActive(false);
    }

    // Call IndestructibleSceneTracker here because SceneManager.activeSceneChanged seems to be called in Awake
    void Start()
    {
        if (IndestructibleSceneTracker.Instance.GetCountForSceneIndex(SceneManager.GetActiveScene().buildIndex) <= 1)
        {
            audioSource.clip = audioClips[0];
        }
        else
        {
            audioSource.clip = audioClips[1];
        }
        StartCoroutine(PlayAudio());
    }
}

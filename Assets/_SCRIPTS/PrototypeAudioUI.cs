using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeAudioUI : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] GameObject button;
    [SerializeField] GameObject background;

    AudioSource recording;

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
        recording.Play();
        Invoke("OnAudioFinished", recording.clip.length);
    }

    void Awake()
    {
        recording = GetComponent<AudioSource>();
        button.SetActive(false);
        Input.multiTouchEnabled = false;
        StartCoroutine(PlayAudio());
    }
}

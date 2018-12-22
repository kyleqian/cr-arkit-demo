using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeAudioUI : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] GameObject button;

    public void ReturnToPlaqueScene()
    {
        SceneManager.LoadScene("PrototypeMode2", LoadSceneMode.Single);
    }

    IEnumerator PlayAudio()
    {
        // Play audio
        yield return new WaitForSeconds(3);
        text.SetActive(false);
        button.SetActive(true);
    }

    void Start()
    {
        button.SetActive(false);
        StartCoroutine(PlayAudio());
    }
}

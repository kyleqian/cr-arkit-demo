using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject enterButton;
    [SerializeField] GameObject continueButton;
    [Space]
    [SerializeField] float textFadeDuration;
    [SerializeField] string[] introText;

    int introTextIndex;
    Coroutine textChangeCoroutine;

    void Start()
    {
        enterButton.SetActive(false);
        text.text = "";
        textChangeCoroutine = StartCoroutine(FadeReplaceText(introText[0], textFadeDuration));
    }

    public void AdvanceText()
    {
        if (textChangeCoroutine != null)
        {
            StopCoroutine(textChangeCoroutine);
        }

        introTextIndex++;
        textChangeCoroutine = StartCoroutine(FadeReplaceText(introText[introTextIndex], textFadeDuration));

        if (introTextIndex == introText.Length - 1)
        {
            continueButton.SetActive(false);
            enterButton.SetActive(true);
        }
    }

    public void LoadViewfinder()
    {
        StartCoroutine(LoadViewFinderAsync());
    }

    IEnumerator LoadViewFinderAsync()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("Viewfinder", LoadSceneMode.Single);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void ExitToTitle()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }

    // TODO: Duplicated from ContentUI; put all the fading code together.
    IEnumerator FadeReplaceText(string newText, float speed)
    {
        // Fade out
        for (float t = 1f; t > 0f; t -= Time.deltaTime / speed)
        {
            text.alpha = t;
            yield return null;
        }

        // Replace text
        text.text = newText;

        // Fade in
        for (float t = 0f; t < 1f; t += Time.deltaTime / speed)
        {
            text.alpha = t;
            yield return null;
        }
    }
}

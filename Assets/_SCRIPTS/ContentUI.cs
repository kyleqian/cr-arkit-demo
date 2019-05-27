﻿using System.Collections;
using TMPro;
using UnityEngine;

public class ContentUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI signature;
    [SerializeField] GameObject volumeOn;
    [SerializeField] GameObject volumeOff;
    [SerializeField] RectTransform canvasTransform;

    [Header("Animations")]
    [SerializeField] AnimationCurve canvasEaseCurve;

    const string VOLUME_ONOFF_KEY = "VOLUME_ONOFF_KEY";

    bool currentlyActive;
    Voice activeVoice;
    AudioSource audioSource;
    Vector3 canvasTransformInitialPosition;

    public void ShowSelf(Voice voice)
    {
        if (currentlyActive)
        {
            return;
        }
        currentlyActive = true;
        StopAllCoroutines();
        activeVoice = voice;
        text.text = "";
        signature.text = activeVoice.signature.Length > 0 ? "- " + activeVoice.signature : "";

        // Start below screen
        Vector3 newPosition = canvasTransformInitialPosition;
        newPosition.y -= canvasTransform.rect.height;
        canvasTransform.position = newPosition;

        // Play audio and transcription
        StartCoroutine(PlayAudioAndTranscription());

        // Ease in
        StartCoroutine(EaseCanvas(true, 1.5f));
    }

    public void HideSelf()
    {
        if (!currentlyActive)
        {
            return;
        }
        currentlyActive = false;
        StopAllCoroutines();
        activeVoice = null;

        // Stop audio playback
        audioSource.Stop();

        // Start above screen
        canvasTransform.position = canvasTransformInitialPosition;

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

    IEnumerator EaseCanvas(bool easeIn, float duration)
    {
        if (easeIn)
        {
            canvasTransform.gameObject.SetActive(true);
        }

        float startingY = canvasTransform.position.y;
        // TODO: Fix the magic -450f for iPad.
        float targetY = easeIn ? canvasTransformInitialPosition.y : canvasTransformInitialPosition.y - canvasTransform.rect.height - 450f;
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

    IEnumerator OnAudioFinished(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Remember that the user has completed this Voice
        PlayerPrefs.SetInt(activeVoice.GetPlayerPrefKey(), 1);
        GlobalDatabase.Instance.DatabaseUpdated.Invoke();

        HideSelf();
    }

    IEnumerator PlayAudioAndTranscription()
    {
        audioSource.clip = activeVoice.recording;

        yield return new WaitForSeconds(2);

        audioSource.Play();
        StartCoroutine(PlayTranscription());
        StartCoroutine(OnAudioFinished(audioSource.clip.length + 1f));
    }

    IEnumerator PlayTranscription()
    {
        for (int i = 0; i < activeVoice.timestamps.Length; ++i)
        {
            if (i > 0)
            {
                yield return new WaitForSeconds(activeVoice.timestamps[i] - activeVoice.timestamps[i - 1]);
            }
            StartCoroutine(FadeReplaceText(activeVoice.transcriptions[i].Trim(), 0.5f));
        }
    }

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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnchoringUI : MonoBehaviour
{
    [SerializeField] float fadeOutDuration;

    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        for (float alpha = 1.0f; alpha > 0.0f; alpha -= Time.deltaTime / fadeOutDuration)
        {
            foreach (Transform elem in transform)
            {
                MaskableGraphic maskableGraphic = elem.GetComponent<MaskableGraphic>();
                if (maskableGraphic)
                {
                    Color newColor = maskableGraphic.color;
                    newColor.a = Mathf.Min(alpha, newColor.a);
                    maskableGraphic.color = newColor;
                }
            }
            yield return null;
        }
        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleUp : MonoBehaviour {
    public float growLength = 0.2f;
    public float shrinkLength = 0.07f;
    public Vector3 fullSize = new Vector3(0.65f, 0.65f, 0.65f);
    public Vector3 shrinkToSize = new Vector3(0.54f, 0.54f, 0.54f);
    public Vector3 growFromSize = new Vector3 (0.162f, 0.162f, 0.162f);
    // Use this for initialization
    void OnEnable(){
        transform.localScale = growFromSize;
        StartCoroutine (GrowToFull ());
    }

    void OnDisable(){
        transform.localScale = growFromSize;
    }

    IEnumerator GrowToFull(){
        float time = 0;
        Vector3 originalScale = transform.localScale;
        while (time < growLength) {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp (originalScale, fullSize, time / growLength);
            yield return null;
        }
        StartCoroutine (ShrinkToRegular ());
    }

    IEnumerator ShrinkToRegular(){
        float time = 0;
        Vector3 originalScale = transform.localScale;
        while (time < shrinkLength) {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp (originalScale, shrinkToSize, time / shrinkLength);
            yield return null;
        }
    }
}

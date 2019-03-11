using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
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
}

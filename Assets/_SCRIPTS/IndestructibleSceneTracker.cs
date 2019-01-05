using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class IndestructibleSceneTracker : MonoBehaviour
{
    public static IndestructibleSceneTracker Instance;
    Dictionary<int, int> sceneCountDict = new Dictionary<int, int>();

    public int GetCountForSceneIndex(int index)
    {
        return sceneCountDict.ContainsKey(index) ? sceneCountDict[index] : 0;
    }

    void OnSceneChange(Scene current, Scene next)
    {
        if (!sceneCountDict.ContainsKey(next.buildIndex))
        {
            sceneCountDict[next.buildIndex] = 0;
        }
        sceneCountDict[next.buildIndex]++;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);

            sceneCountDict[SceneManager.GetActiveScene().buildIndex] = 1;
            SceneManager.activeSceneChanged += OnSceneChange;
        }
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChange;
    }
}

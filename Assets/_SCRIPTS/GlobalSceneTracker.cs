using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GlobalSceneTracker : MonoBehaviour
{
    public static GlobalSceneTracker Instance;
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
        }
    }

    void Start()
    {
        if (Instance == this)
        {
            sceneCountDict[SceneManager.GetActiveScene().buildIndex] = 1;
            SceneManager.activeSceneChanged += OnSceneChange;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SceneManager.activeSceneChanged -= OnSceneChange;
        }
    }
}

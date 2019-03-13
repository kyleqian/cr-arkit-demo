using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalApplicationSettings : MonoBehaviour
{
    public static GlobalApplicationSettings Instance;

    int initialSleepTimeout;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (Instance == this)
        {
            DontDestroyOnLoad(this);
            Application.targetFrameRate = 60;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            initialSleepTimeout = Screen.sleepTimeout;
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

    void OnSceneChange(Scene current, Scene next)
    {
        if (next.name == "Viewfinder")
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        else
        {
            Screen.sleepTimeout = initialSleepTimeout;
        }
    }
}

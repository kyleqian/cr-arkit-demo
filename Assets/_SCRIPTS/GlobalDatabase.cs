using System;
using UnityEngine;
using UnityEngine.Events;

public class GlobalDatabase : MonoBehaviour
{
    public static GlobalDatabase Instance;
    public UnityEvent DatabaseUpdated;

    [SerializeField] Voice[] voices;

    public Voice FindVoiceByName(string name)
    {
        return Array.Find(voices, v => v.name == name);
    }

    public Tuple<int, int> GetVoiceCounts()
    {
        int completedCount = 0;
        foreach (Voice v in voices)
        {
            if (PlayerPrefs.HasKey(v.GetPlayerPrefKey()))
            {
                ++completedCount;
            }
        }
        return new Tuple<int, int>(completedCount, voices.Length);
    }

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
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

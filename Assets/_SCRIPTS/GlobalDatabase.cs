using System;
using System.Collections;
using UnityEngine;

public class GlobalDatabase : MonoBehaviour
{
    public static GlobalDatabase Instance;

    [SerializeField] Voice[] voices;

    public Voice FindVoiceByName(string name)
    {
        return Array.Find(voices, v => v.name == name);
    }

    public void GetVoiceCounts()
    { }

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

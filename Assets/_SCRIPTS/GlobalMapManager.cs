using Firebase.Storage;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GlobalMapManager : MonoBehaviour
{
    public static GlobalMapManager Instance;
    public bool MapExists { get { return File.Exists(WorldMapPath); } }
    public bool DownloadInProgress { get; private set; }
    public delegate void DownloadCallback();

    [SerializeField] string firebaseDownloadUrl;
    [SerializeField] string worldMapFilename;

    FirebaseStorage storage;
    StorageReference reference;
    string persistentDataPath; // Not sure why, but can't call Application.persistentDataPath in Tasks.

    public string WorldMapPath
    {
        get { return Path.Combine(persistentDataPath, worldMapFilename); }
    }

    string FirebaseWorldMapPath
    {
        get { return "file://" + WorldMapPath; }
    }

    public void DownloadLatestMap(DownloadCallback callback)
    {
        reference.GetMetadataAsync().ContinueWith((Task<StorageMetadata> metadataTask) =>
        {
            if (metadataTask.IsFaulted || metadataTask.IsCanceled)
            {
                Debug.Log("Failed to download metadata.");
                Debug.Log(metadataTask.Exception.InnerException.Message);
            }
            else if (metadataTask.IsCompleted)
            {
                Debug.Log("Metadata successfully downloaded.");
                StorageMetadata meta = metadataTask.Result;

                // If there's no local map or local map is old, download new map from cloud.
                if (!MapExists || meta.CreationTimeMillis > File.GetLastWriteTimeUtc(WorldMapPath))
                {
                    Debug.Log("Beginning map download.");
                    DownloadInProgress = true;
                    reference.GetFileAsync(FirebaseWorldMapPath).ContinueWith(mapTask =>
                    {
                        DownloadInProgress = false;

                        if (mapTask.IsFaulted || mapTask.IsCanceled)
                        {
                            Debug.Log("Failed to download map.");
                            Debug.Log(mapTask.Exception.InnerException.Message);
                        }
                        else if (mapTask.IsCompleted)
                        {
                            Debug.Log("Map successfully downloaded.");
                            callback();
                        }
                    });
                }
                else
                {
                    Debug.Log("No need to download new map.");
                    callback();
                }
            }
        });
    }

    public void UploadMap()
    {
        reference.PutFileAsync(FirebaseWorldMapPath).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Failed to upload map.");
                Debug.Log(task.Exception.InnerException.Message);
            }
            else
            {
                Debug.Log("Successfully uploaded map.");
            }
        });
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
            storage = FirebaseStorage.DefaultInstance;
            reference = storage.GetReferenceFromUrl(firebaseDownloadUrl);
            persistentDataPath = Application.persistentDataPath;
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

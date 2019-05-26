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

    [SerializeField] GameObject ARScenePrefab;

    FirebaseStorage storage;
    StorageReference reference;
    string persistentDataPath; // Not sure why, but can't call Application.persistentDataPath in callbacks.

    public string WorldMapPath
    {
        get { return Path.Combine(persistentDataPath, WorldMapFilename); }
    }

    public string WorldMapFilename
    {
        get { return WorldMapAssetPrefix + ".worldmap"; }
    }

    public string WorldMapAssetPrefix
    {
        get
        {
            return string.Format("{0}{1}",
                                 ARKitWorldMapManager.Instance != null &&
                                 ARKitWorldMapManager.Instance.IsTestMode ? "TEST_" : "",
                                 ARScenePrefab.name);
        }
    }

    public string GetPlayerPrefAnchorIdKey(GameObject g)
    {
        return string.Format("{0}_{1}_AnchorId", WorldMapAssetPrefix, g.name);
    }

    public string GetPlayerPrefScaleXKey(string anchorId)
    {
        return string.Format("{0}_{1}_ScaleX", WorldMapAssetPrefix, anchorId);
    }

    public string GetPlayerPrefScaleYKey(string anchorId)
    {
        return string.Format("{0}_{1}_ScaleY", WorldMapAssetPrefix, anchorId);
    }

    public string GetPlayerPrefScaleZKey(string anchorId)
    {
        return string.Format("{0}_{1}_ScaleZ", WorldMapAssetPrefix, anchorId);
    }

    string FirebaseDownloadUrl
    {
        get { return string.Format("gs://dear-visitor.appspot.com/{0}", WorldMapFilename); }
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
            reference = storage.GetReferenceFromUrl(FirebaseDownloadUrl);
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

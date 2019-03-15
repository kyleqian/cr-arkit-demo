using UnityEngine;
using UnityEngine.SceneManagement;

public class Root : MonoBehaviour
{
    public void LoadTitle()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }

    public void LoadCreatorMode()
    {
        SceneManager.LoadScene("CreatorMode", LoadSceneMode.Single);
    }

    public void LoadPrototypeMode()
    {
        SceneManager.LoadScene("PrototypeMode1", LoadSceneMode.Single);
    }

    public void LoadAudioTest()
    {
        SceneManager.LoadScene("PrototypeMode3", LoadSceneMode.Single);
    }

    public void LoadGpsTest()
    {
        SceneManager.LoadScene("TestGPS", LoadSceneMode.Single);
    }

    public void LoadSandbox()
    {
        SceneManager.LoadScene("Sandbox", LoadSceneMode.Single);
    }

    public void ClearVoicePlayerPrefs()
    {
        GlobalDatabase.Instance.ClearVoicePlayerPrefs();
    }

    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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
}

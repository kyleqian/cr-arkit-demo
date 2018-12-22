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
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public void LoadIntro()
    {
        SceneManager.LoadScene("Intro", LoadSceneMode.Single);
    }

    public void LoadResources()
    {
        SceneManager.LoadScene("Resources", LoadSceneMode.Single);
    }
}

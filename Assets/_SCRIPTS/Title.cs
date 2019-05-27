using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    int debugRoseTaps;

    public void LoadIntro()
    {
        SceneManager.LoadScene("Intro", LoadSceneMode.Single);
    }

    public void LoadResources()
    {
        SceneManager.LoadScene("Resources", LoadSceneMode.Single);
    }

    public void DebugRose()
    {
        if (++debugRoseTaps >= 20)
        {
            SceneManager.LoadScene("Root", LoadSceneMode.Single);
        }
    }
}

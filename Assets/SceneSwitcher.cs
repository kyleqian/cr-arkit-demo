using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void GoToScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

}

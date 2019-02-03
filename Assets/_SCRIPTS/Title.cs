using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public void LoadViewfinder()
    {
        SceneManager.LoadScene("Viewfinder", LoadSceneMode.Single);
    }
}

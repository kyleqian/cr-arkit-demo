using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeStartUI : MonoBehaviour
{
    public void StartPrototype()
    {
        SceneManager.LoadScene("PrototypeMode2", LoadSceneMode.Single);
    }
}

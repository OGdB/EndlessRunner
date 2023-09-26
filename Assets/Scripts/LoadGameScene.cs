using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScene : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("Game");
    }
}

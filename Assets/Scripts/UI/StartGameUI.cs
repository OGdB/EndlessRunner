using UnityEngine;

public class StartGameUI : MonoBehaviour
{
    [SerializeField] private GameObject startUiWrapper;

    private void OnEnable()
    {
        GameController.OnGameStart += GameController_OnGameStart;
    }

    private void OnDisable()
    {
        GameController.OnGameStart -= GameController_OnGameStart;
    }

    private void GameController_OnGameStart()
    {
        startUiWrapper.SetActive(false);
    }
}


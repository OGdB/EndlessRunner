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

    public static void SetRandomSeed(TMPro.TMP_InputField inputField)
    {
        inputField.text = Random.Range(0, int.MaxValue).ToString();
    }
}


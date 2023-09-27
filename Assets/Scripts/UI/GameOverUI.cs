using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private float appearSpeed = 1f;
    private CanvasGroup _canvasGroup;

    private void Awake() => _canvasGroup = GetComponentInChildren<CanvasGroup>();
    private void OnEnable() => GameController.OnGameOver += Show;
    private void OnDisable() => GameController.OnGameOver -= Show;

    private void Show()
    {
        _ = StartCoroutine(StandardLerpCoroutines.FadeCanvasAlpha(_canvasGroup, 1f, appearSpeed));
    }
}

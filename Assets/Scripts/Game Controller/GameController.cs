using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Responsible for 'general' fields such as score, lives and playtime.
/// Also contains the functionality for game start, restart and quit actions.
/// </summary>
[DefaultExecutionOrder(0)]
public class GameController : MonoBehaviour
{
    #region Properties
    private static GameController Singleton;
    public static bool GameStarted { get; private set; } = false;

    [SerializeField] private TextMeshProUGUI countdownTimerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timeText;

    private static int _score = 0;
    private static int _lives = 3;
    private static double _totalTime = 0;
    private static double _startTime = 0;
    private static double _endTime = 0;

    public static Action OnGameStart { get; set; }
    public static Action OnGameOver { get; set; }
    public static int Score
    {
        get => _score; set
        {
            _score = value;
            Singleton.scoreText.SetText($"Score: {Score}");
        }
    }
    public static int Lives
    {
        get => _lives; set
        {
            _lives = value;
            Singleton.livesText.SetText($"Lives: {Lives}");

            if (_lives <= 0)
                GameOver();
        }
    }
    public static double TotalTime
    {
        get => _totalTime; set
        {
            _totalTime = value;
            Singleton.timeText.SetText($"Time: {TotalTime}");
        }
    }
    #endregion

    private void Awake()
    {
        if (Singleton)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
    }

    private void Start()
    {
        // Initialize values (so they'll be displayed to player).
        Score = _score;
        TotalTime = _totalTime;
        Lives = _lives;
    }

    private void OnDestroy()
    {
        Singleton = null;
        GameStarted = false;
        _totalTime = 0;
        _startTime = 0;
        _endTime = 0;
        _lives = 3;
        _score = 0;

        StopAllCoroutines();
    }

    public static void StartGame()
    {
        Singleton.StartCoroutine(CountdownCR());

        static IEnumerator CountdownCR()
        {
            OnGameStart?.Invoke();

            Singleton.countdownTimerText.gameObject.SetActive(true);

            WaitForSeconds second = new(1f);
            int timeLeft = 3;

            while (timeLeft > 0)
            {
                timeLeft--;
                yield return second;
                Singleton.countdownTimerText.SetText(timeLeft.ToString());
            }

            Singleton.countdownTimerText.gameObject.SetActive(false);
            GameStarted = true;
            _startTime = Time.time;
        }
    }
    public static void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LoadScene");
    }
    public static void GameOver()
    {
        GameStarted = false;
        _endTime = Time.time;
        _totalTime = _endTime - _startTime;
        Singleton.timeText.SetText($"Time: {_totalTime:F2}");
        Time.timeScale = 0;

        OnGameOver?.Invoke();
    }
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}

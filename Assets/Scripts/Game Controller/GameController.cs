using System;
using System.Collections;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class GameController : MonoBehaviour
{
    #region Properties
    private static GameController Singleton;
    public static bool GameStarted { get; private set; } = false;

    [SerializeField] private GameObject startCanvas;
    [SerializeField] private TextMeshProUGUI countdownTimerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timeText;

    private static int _score = 0;
    private static int _lives = 3;
    private static double _totalTime = 0;
    private static double _startTime = 0;
    private static double _endTime = 0;

    public static Action OnGameStart;
    public static Action OnGameOver;
    public static Action OnGameRestart;


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

            Singleton.startCanvas.SetActive(false);

            GameStarted = true;
            _startTime = Time.time;
        }
    }
    public static void GameOver()
    {
        GameStarted = false;
        _endTime = Time.time;
        _totalTime = _endTime - _startTime;
        Singleton.timeText.SetText($"Time: {_totalTime}");

        OnGameOver?.Invoke();
    }
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

        if (LevelGenerator.Seed != default)
            StartGame();
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
}

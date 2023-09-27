using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsible for the procedural level generation.
/// Takes a seed either randomly generated or provided by the player to generate 
/// the obstacles and powerups.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    #region Variables & Properties    
    public static int Seed { get; set; }
    private static bool IsFirstTimeLoading = true;

    [Tooltip("The number of obstacle lines that will be generated and maintained during the game.")]
    [SerializeField, Range(0.1f, 1f)] private float rowObstaclePercentage = 0.8f;
    private float _nextRowZPos;

    [Header("Obstacles")]
    [SerializeField] private GreyObstacle greyObstacle;
    [SerializeField] private BlueObstacle blueObstacle;
    [SerializeField] private OrangeObstacle orangeObstacle;

    [Space(5)]
    [SerializeField, Range(0, 1f)] private float greyObstacleChance = 0.4f; // Probability for grey obstacle
    [SerializeField, Range(0, 1f)] private float blueObstacleChance = 0.3f; // Probability for blue obstacle

    [Header("Powerups")]
    [SerializeField] private GreenPowerup greenPowerup;
    [SerializeField] private RedPowerup redPowerup;

    [Header("Start UI")]
    [SerializeField] private Button startGameButton; 
    [SerializeField] private TMPro.TMP_InputField seedInputField;
    [SerializeField] private TMPro.TMP_InputField numberOfStartLanesInput;
    [SerializeField] private TMPro.TMP_InputField visObstRowsInput;
    [SerializeField] private TMPro.TMP_InputField distToFirstRowInput;
    [SerializeField] private TMPro.TMP_InputField distBetweenRowsInput;
    [SerializeField] private Toggle randomSeedToggle;

    private static int _numberOfGeneratedObstacles = 0;

    private static Transform _obstacleParent;
    private static Transform _powerupsParent;

    private static Queue<InteractableBlock> _greyObstaclesPool = new();
    private static Queue<InteractableBlock> _blueObstaclesPool = new();
    private static Queue<InteractableBlock> _orangeObstaclesPool = new();

    public static int NumberOfVisibleObstacleRows;
    public static float DistanceToFirstRow;
    public static float DistanceBetweenRows;

    public LevelGenerationSettings _currentSettings = null;

    public void SetNumberOfVisibleObstacleRows(string value) => NumberOfVisibleObstacleRows = int.Parse(value);
    public void SetDistanceToFirstRow(string value) => DistanceToFirstRow = float.Parse(value);
    public void SetDistanceBetweenRows(string value) => DistanceBetweenRows = float.Parse(value);

    /// <summary>
    /// Set the seed to the string input. Used by the inputfield.
    /// </summary>
    /// <param name="input">The input string representing the seed.</param>
    public void SetSeed(string input)
    {
        if (!randomSeedToggle.isOn)  // If not a randomized seed
        {
            if (int.TryParse(input, out int value))
            {
                Seed = value;
            }
            else
            {
                Debug.LogWarning("Couldn't parse string input to integer value for seed.");
                RandomSeed();
            }
        }
        else
        {
            RandomSeed();
        }

        void RandomSeed() => Seed = Random.Range(0, int.MaxValue);
    }
    #endregion

    private void Awake()
    {
        _obstacleParent = new GameObject("Obstacles").transform;
        _powerupsParent = new GameObject("Powerups").transform;

        // Populate the obstacle pools
        for (int i = 0; i < 12; i++)
        {
            InstantiatePoolInstance(greyObstacle, _greyObstaclesPool);
            InstantiatePoolInstance(blueObstacle, _blueObstaclesPool);
            InstantiatePoolInstance(orangeObstacle, _orangeObstaclesPool);
        }
    }
    private void Start()
    {
        // Import level settings only the first time loading the scene.
        if (IsFirstTimeLoading)
        {
            IsFirstTimeLoading = false;
            TryImportLevelSettings();
        }

        seedInputField.SetTextWithoutNotify(Seed.ToString());
        numberOfStartLanesInput.SetTextWithoutNotify(LaneManager.StartLaneAmount.ToString());
        visObstRowsInput.SetTextWithoutNotify(NumberOfVisibleObstacleRows.ToString());
        distToFirstRowInput.SetTextWithoutNotify(DistanceToFirstRow.ToString());
        distBetweenRowsInput.SetTextWithoutNotify(DistanceBetweenRows.ToString());
    }
    private void OnEnable()
    {
        BlockCleaner.PassedObstacleLine += SpawnNextObstacleLine;
        GameController.OnGameStart += GameController_OnGameStart;
    }
    private void OnDisable()
    {
        BlockCleaner.PassedObstacleLine -= SpawnNextObstacleLine;
        GameController.OnGameStart -= GameController_OnGameStart;
    }

    private void TryImportLevelSettings()
    {
        _currentSettings = LevelGenerationSettings.ImportFromJson();

        if (_currentSettings != null)
        {
            SetLevelSettingValues(_currentSettings);
        }
        else
        {
            // Invalid settings = Go with default values.
            Seed = Random.Range(0, int.MaxValue);

            Notifier.SetNotification("Imported settings were invalid.");
            Debug.LogWarning("Imported settings are invalid.");
        }
    }

    private void SetLevelSettingValues(LevelGenerationSettings _settings)
    {
        Seed = _settings.seed; 
        Random.InitState(Seed);

        NumberOfVisibleObstacleRows = _settings.numberOfVisibleObstacleRows;
        DistanceToFirstRow = _settings.distanceToFirstRow;
        DistanceBetweenRows = _settings.distanceBetweenRows;
        greyObstacleChance = _settings.greyObstacleChance;
        blueObstacleChance = _settings.blueObstacleChance;
        LaneManager.StartLaneAmount = _settings.startNumberOfLanes;
    }

    private void GameController_OnGameStart()
    {
        // Set the seed based on the input of the inputfield // random seed toggle.
        startGameButton.gameObject.SetActive(false);
        seedInputField.gameObject.SetActive(false);
        randomSeedToggle.gameObject.SetActive(false);

        // Update the level generator settings with the values set.
        _currentSettings = new(NumberOfVisibleObstacleRows,
                                   DistanceToFirstRow,
                                   DistanceBetweenRows,
                                   greyObstacleChance,
                                   blueObstacleChance,
                                   LaneManager.StartLaneAmount,
                                   Seed);

        SetLevelSettingValues(_currentSettings);

        _nextRowZPos = DistanceToFirstRow;

        // First line spawns at 'distanceUntilFirstObstacle' distance. From there, it follows the rule.
        for (int i = 0; i < NumberOfVisibleObstacleRows; i++)
        {
            SpawnObstacleLine(_nextRowZPos);
            _nextRowZPos += DistanceBetweenRows;
        }
    }

    private void SpawnNextObstacleLine()
    {
        SpawnObstacleLine(_nextRowZPos);
        _nextRowZPos += DistanceBetweenRows;
    }

    private void SpawnObstacleLine(float zPosition)
    {
        // More obstacles
        int amountOfObstacles = Random.Range(Mathf.FloorToInt(LaneManager.NumberOfLanes * 0.7f), Mathf.FloorToInt(LaneManager.NumberOfLanes * rowObstaclePercentage));
        List<int> laneIntegers = LaneManager.GetRandomUniqueLaneInts(amountOfObstacles);

        for (int i = 0; i < amountOfObstacles; i++)
        {
            InteractableBlock obstacle = GetRandomObstacle();

            int laneInteger = laneIntegers[i];
            // Set variables
            obstacle.CurrentLaneInt = laneInteger;
            obstacle.SetPosition(new(LaneManager.GetLaneX(laneIntegers[i]), 0, zPosition));

            obstacle.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Exports the current level generation settings to a JSON file.
    /// </summary>
    public void ExportLevelSettings() => _currentSettings.ExportToJson();

    private InteractableBlock GetRandomObstacle()
    {
        float randomValue = Random.value;

        if (randomValue < greyObstacleChance)
        {
            return DequeueFromPool(_greyObstaclesPool, greyObstacle);
        }
        else if (randomValue < greyObstacleChance + blueObstacleChance)
        {
            return DequeueFromPool(_blueObstaclesPool, blueObstacle);
        }
        else
        {
            return DequeueFromPool(_orangeObstaclesPool, orangeObstacle);
        }
    }

    private static InteractableBlock DequeueFromPool(Queue<InteractableBlock> pool, InteractableBlock original)
    {
        if (pool.Count > 0)
        {
            InteractableBlock poolObject = pool.Dequeue();
            return poolObject;
        }
        else  // Nothing left in pool
        {
            for (int i = 0; i < 5; i++)
            {
                InstantiatePoolInstance(original, pool);
            }

            return Instantiate(original, _obstacleParent);
        }
    }

    private static void EnqueueObject(InteractableBlock go, Queue<InteractableBlock> pool)
    {
        go.gameObject.SetActive(false);
        go.ResetValues();
        pool.Enqueue(go);
    }

    public static void EnqueueInteractable(InteractableBlock interactableBlock)
    {
        if (interactableBlock is GreyObstacle greyObstacle)
        {
            EnqueueObject(greyObstacle, _greyObstaclesPool);
        }
        else if (interactableBlock is BlueObstacle blueObstacle)
        {
            EnqueueObject(blueObstacle, _blueObstaclesPool);
        }
        else if (interactableBlock is OrangeObstacle orangeObstacle)
        {
            EnqueueObject(orangeObstacle, _orangeObstaclesPool);
        }
    }

    private static void InstantiatePoolInstance(InteractableBlock original, Queue<InteractableBlock> pool)
    {
        InteractableBlock instance = Instantiate(original, _obstacleParent);
        instance.name = $"{original.GetType().Name} {_numberOfGeneratedObstacles}";
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
        _numberOfGeneratedObstacles++;
    }

    private void OnDestroy()
    {
        _obstacleParent = null;
        _powerupsParent = null;

        _greyObstaclesPool.Clear();
        _blueObstaclesPool.Clear();
        _orangeObstaclesPool.Clear();

        _greyObstaclesPool = new();
        _blueObstaclesPool = new();
        _orangeObstaclesPool = new();
    }
}

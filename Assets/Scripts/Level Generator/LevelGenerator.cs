using System.Collections.Generic;
using System.Globalization;
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
    /// <summary>
    /// The current level generation settings.
    /// </summary>
    public static LevelGenerationSettings CurrentLevelSettings = null;

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
    [Tooltip("The MINIMUM amount of rows before a new powerup can spawn.")]
    [SerializeField] private int powerupSpawnRate = 6;
    [Tooltip("The chance for a powerup to appear in a row, after its 'cooldown' period")]
    [SerializeField] private float powerupSpawnChance = 0.5f;
    [SerializeField] private GreenPowerup greenPowerup;
    [SerializeField] private RedPowerup redPowerup;
    private int _nextPowerupThreshold;

    [Header("Start & Settings UI")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMPro.TMP_InputField seedInputField;

    [SerializeField] private TMPro.TMP_InputField numberOfStartLanesInput;
    [SerializeField] private TMPro.TMP_InputField visObstRowsInput;
    [SerializeField] private TMPro.TMP_InputField distToFirstRowInput;
    [SerializeField] private TMPro.TMP_InputField distBetweenRowsInput;

    private static int _numberOfRows = 0;
    private static int _numberOfGeneratedObstacles = 0;

    private static Transform _obstacleParent;
    private static Transform _powerupsParent;

    private static Queue<InteractableBlock> _greyObstaclesPool = new();
    private static Queue<InteractableBlock> _blueObstaclesPool = new();
    private static Queue<InteractableBlock> _orangeObstaclesPool = new();
    private static Queue<InteractableBlock> _greenPowerupPool = new();
    private static Queue<InteractableBlock> _redPowerupPool = new();

    public void SetNumberOfStartLanes(string value) => CurrentLevelSettings.startNumberOfLanes = int.Parse(value);
    public void SetNumberOfVisibleObstacleRows(string value) => CurrentLevelSettings.numberOfVisibleObstacleRows = int.Parse(value);
    public void SetDistanceToFirstRow(string value) => CurrentLevelSettings.distanceToFirstRow = float.Parse(value);
    public void SetDistanceBetweenRows(string value) => CurrentLevelSettings.distanceBetweenRows = float.Parse(value);


    /// <summary>
    /// Set the seed to the string input. Used by the inputfield.
    /// </summary>
    /// <param name="input">The input string representing the seed.</param>
    public void SetSeed(string input)
    {
        if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            CurrentLevelSettings.seed = value;
        }
        else
        {
            Debug.LogWarning("Couldn't parse string input to integer value for seed.");
            CurrentLevelSettings.seed = Random.Range(0, int.MaxValue);
        }
    }
    #endregion

    private void Awake()
    {
        _obstacleParent = new GameObject("Obstacles").transform;
        _powerupsParent = new GameObject("Powerups").transform;
        _nextPowerupThreshold = powerupSpawnRate;
        // Populate the pools
        for (int i = 0; i < 12; i++)
        {
            InstantiatePoolInstance(greyObstacle, _greyObstaclesPool, _obstacleParent);
            InstantiatePoolInstance(blueObstacle, _blueObstaclesPool, _obstacleParent);
            InstantiatePoolInstance(orangeObstacle, _orangeObstaclesPool, _obstacleParent);

            InstantiatePoolInstance(greenPowerup, _greenPowerupPool, _powerupsParent);
            InstantiatePoolInstance(redPowerup, _redPowerupPool, _powerupsParent);
        }
    }
    private void Start()
    {
        // Import level settings only the first time loading the scene.
        if (IsFirstTimeLoading)
        {
            IsFirstTimeLoading = false;
            CurrentLevelSettings = LevelGenerationSettings.ImportFromJson();
        }

        seedInputField.SetTextWithoutNotify(CurrentLevelSettings.seed.ToString());
        numberOfStartLanesInput.SetTextWithoutNotify(CurrentLevelSettings.startNumberOfLanes.ToString());
        visObstRowsInput.SetTextWithoutNotify(CurrentLevelSettings.numberOfVisibleObstacleRows.ToString());
        distToFirstRowInput.SetTextWithoutNotify(CurrentLevelSettings.distanceToFirstRow.ToString());
        distBetweenRowsInput.SetTextWithoutNotify(CurrentLevelSettings.distanceBetweenRows.ToString());
    }
    private void OnEnable()
    {
        BlockCleaner.PassedObstacleLine += BlockCleaner_PassedObstacleLine;
        GameController.OnGameStart += GameController_OnGameStart;
    }
    private void OnDisable()
    {
        BlockCleaner.PassedObstacleLine -= BlockCleaner_PassedObstacleLine;
        GameController.OnGameStart -= GameController_OnGameStart;
    }

    private void GameController_OnGameStart()
    {
        // Set the seed based on the input of the inputfield // random seed toggle.
        startGameButton.gameObject.SetActive(false);
        seedInputField.gameObject.SetActive(false);

        Random.InitState(CurrentLevelSettings.seed);

        _nextRowZPos = CurrentLevelSettings.distanceToFirstRow;

        // First line spawns at 'distanceUntilFirstObstacle' distance. From there, it follows the rule.
        for (int i = 0; i < CurrentLevelSettings.numberOfVisibleObstacleRows; i++)
        {
            SpawnObstacleRow(_nextRowZPos);
            _nextRowZPos += CurrentLevelSettings.distanceBetweenRows;
        }
    }

    /// <summary>
    /// Invoked when the player passed a row of obstacles.
    /// </summary>
    private void BlockCleaner_PassedObstacleLine()
    {
        SpawnObstacleRow(_nextRowZPos);
        _numberOfRows++;

        if (_numberOfRows >= _nextPowerupThreshold && Random.value < powerupSpawnChance)
        {
            // Placing powerups in-between obstacle rows.
            _nextPowerupThreshold += powerupSpawnRate;
            float zPosition = _nextRowZPos + CurrentLevelSettings.distanceBetweenRows / 2f;
            SpawnPowerupOnRandomLane(zPosition);
        }

        _nextRowZPos += CurrentLevelSettings.distanceBetweenRows;
    }

    /// <summary>
    /// Spawn a row of obstacles at the provided Z position.
    /// </summary>
    /// <param name="zPosition">The z-position of the row</param>
    private void SpawnObstacleRow(float zPosition)
    {
        // More obstacles
        int amountOfObstacles = Random.Range(Mathf.FloorToInt(LaneManager.NumberOfLanes * 0.7f), Mathf.FloorToInt(LaneManager.NumberOfLanes * rowObstaclePercentage));
        List<float> LaneXPositions = LaneManager.GetRandomLaneXPositions(amountOfObstacles, out List<int> laneInts);

        for (int i = 0; i < amountOfObstacles; i++)
        {
            InteractableBlock obstacle = GetRandomObstacle();

            float laneX = LaneXPositions[i];
            // Set variables
            obstacle.CurrentLaneInt = laneInts[i];
            obstacle.SetPosition(new(laneX, 0, zPosition));

            obstacle.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Spawns a single powerup at the z Position on a random lane.
    /// </summary>
    /// <param name="zPosition"></param>
    private void SpawnPowerupOnRandomLane(float zPosition)
    {
        InteractableBlock powerup = GetRandomPowerup();
        float LaneX = LaneManager.GetRandomLane(out int LaneInt);
        powerup.CurrentLaneInt = LaneInt;
        powerup.SetPosition(new(LaneX, 0, zPosition));
        powerup.gameObject.SetActive(true);
    }

    /// <summary>
    /// Exports the current level generation settings to a JSON file.
    /// </summary>
    public void ExportLevelSettings() => CurrentLevelSettings.ExportToJson();

    /// <summary>
    /// Return a random obstacle from a pool.
    /// </summary>
    /// <returns>The random obstacle</returns>
    private InteractableBlock GetRandomObstacle()
    {
        float randomValue = Random.value;

        if (randomValue < greyObstacleChance)
        {
            return DequeueFromPool(_greyObstaclesPool, greyObstacle, _obstacleParent);
        }
        else if (randomValue < greyObstacleChance + blueObstacleChance)
        {
            return DequeueFromPool(_blueObstaclesPool, blueObstacle, _obstacleParent);
        }
        else
        {
            return DequeueFromPool(_orangeObstaclesPool, orangeObstacle, _obstacleParent);
        }
    }
    /// <summary>
    /// Return a random powerup from a pool.
    /// </summary>
    /// <returns>The random obstacle</returns>
    private InteractableBlock GetRandomPowerup()
    {
        float randomValue = Random.value;

        if (randomValue < 0.5f)
        {
            return DequeueFromPool(_greenPowerupPool, greenPowerup, _powerupsParent);
        }
        else
        {
            return DequeueFromPool(_redPowerupPool, redPowerup, _powerupsParent);
        }
    }

    /// <summary>
    /// Dequeue an object from a pool. Re-populates the pool if it is empty.
    /// </summary>
    /// <param name="pool">The pool to retrieve from</param>
    /// <param name="original">the prefab found in the pool.</param>
    /// <param name="parent">The parent in the scene hierarchy</param>
    /// <returns></returns>
    private static InteractableBlock DequeueFromPool(Queue<InteractableBlock> pool, InteractableBlock original, Transform parent)
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
                InstantiatePoolInstance(original, pool, parent);
            }

            return Instantiate(original, parent);
        }
    }

    /// <summary>
    /// Return a pool object to its pool.
    /// </summary>
    /// <param name="go"></param>
    /// <param name="pool"></param>
    private static void EnqueueObject(InteractableBlock go, Queue<InteractableBlock> pool)
    {
        go.gameObject.SetActive(false);
        go.ResetValues();
        pool.Enqueue(go);
    }

    /// <summary>
    /// Finds the correct pool of an interactable and returns it to it.
    /// </summary>
    /// <param name="interactableBlock"></param>
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
        else if (interactableBlock is GreenPowerup greenPowerup)
        {
            EnqueueObject(greenPowerup, _greenPowerupPool);
        }
        else if (interactableBlock is RedPowerup redPowerup)
        {
            EnqueueObject(redPowerup, _redPowerupPool);
        }
    }

    private static void InstantiatePoolInstance(InteractableBlock original, Queue<InteractableBlock> pool, Transform parent)
    {
        InteractableBlock instance = Instantiate(original, parent);
        instance.name = $"{original.GetType().Name} {_numberOfGeneratedObstacles}";
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
        _numberOfGeneratedObstacles++;
    }

    private void OnDestroy()
    {
        _greyObstaclesPool.Clear();
        _greyObstaclesPool = new();

        _blueObstaclesPool.Clear();
        _blueObstaclesPool = new();

        _orangeObstaclesPool.Clear();
        _orangeObstaclesPool = new();

        _greenPowerupPool.Clear();
        _greenPowerupPool = new();

        _redPowerupPool.Clear();
        _redPowerupPool = new();

        _obstacleParent = null;
        _powerupsParent = null;
    }
}
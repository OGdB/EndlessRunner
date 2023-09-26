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
    [SerializeField]
    private int seed; // Player-provided or randomly generated seed
    public static int Seed { get; set; }

    [SerializeField]
    private int amountOfLinesOnStart = 6;
    [SerializeField]
    private float distanceUntilFirstObstacle = 15f;
    [SerializeField]
    private float distanceBetweenObstacles = 10f;
    private float _nextObstacleSpawnZ;

    [Header("Obstacles"), SerializeField]
    private GreyObstacle greyObstacle;
    [SerializeField]
    private BlueObstacle blueObstacle;
    [SerializeField]
    private OrangeObstacle orangeObstacle;
    [Space(5), SerializeField, Range(0, 1f)]
    private float greyObstacleChance = 0.4f; // Probability for grey obstacle
    [SerializeField, Range(0, 1f)]
    private float blueObstacleChance = 0.3f; // Probability for blue obstacle
    [SerializeField, Range(0, 1f)]
    private float orangeObstacleChance = 0.3f; // Probability for orange obstacle

    [Header("Powerups"), SerializeField]
    private GreenPowerup greenPowerup;
    [SerializeField]
    private RedPowerup redPowerup;

    [Header("Start UI"), SerializeField]
    private Button startGameButton; 
    [SerializeField]
    private TMPro.TMP_InputField seedInput;
    [SerializeField]
    private Toggle randomSeedToggle;

    private int _numberOfGeneratedObstacles = 0;

    private static Transform _obstacleParent;
    private static Transform _powerupsParent;

    // Pools
    private static Queue<InteractableBlock> _greyObstaclesPool = new();
    private static Queue<InteractableBlock> _blueObstaclesPool = new();
    private static Queue<InteractableBlock> _orangeObstaclesPool = new();

    // Value getters/setters.
    public void SetSeed(string input)
    {
        if (!randomSeedToggle.isOn)  // If not a randomized seed
        {
            if (int.TryParse(input, out int value))
            {
                seed = value;
                Seed = seed;
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

        void RandomSeed()
        {
            seed = Random.Range(0, int.MaxValue);
            Seed = seed;
        }
    }
    #endregion


    private void Awake()
    {
        _obstacleParent = new GameObject("Obstacles").transform;
        _powerupsParent = new GameObject("Powerups").transform;

        seedInput.SetTextWithoutNotify(Random.Range(0, int.MaxValue).ToString());

        // Populate the obstacle pools
        for (int i = 0; i < 12; i++)
        {
            InstantiatePoolInstance(greyObstacle, _greyObstaclesPool);

            InstantiatePoolInstance(blueObstacle, _blueObstaclesPool);

            InstantiatePoolInstance(orangeObstacle, _orangeObstaclesPool);
        }
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

    private void GameController_OnGameStart()
    {
        // Set the seed based on the input of the inputfield // random seed toggle.
        startGameButton.gameObject.SetActive(false);
        seedInput.gameObject.SetActive(false);
        randomSeedToggle.gameObject.SetActive(false);

        // If a seed is already set use that seed.
        if (Seed == default)
            SetSeed(seedInput.text);

        Random.InitState(Seed);

        _nextObstacleSpawnZ = distanceUntilFirstObstacle;

        // First line spawns at 'distanceUntilFirstObstacle' distance. From there, it follows the rule.
        for (int i = 0; i < amountOfLinesOnStart; i++)
        {
            SpawnObstacleLine(_nextObstacleSpawnZ);
            _nextObstacleSpawnZ += distanceBetweenObstacles;
        }
    }

    private void SpawnNextObstacleLine()
    {
        SpawnObstacleLine(_nextObstacleSpawnZ);
        _nextObstacleSpawnZ += distanceBetweenObstacles;
    }

    private void SpawnObstacleLine(float zPosition)
    {
        // More obstacles
        int amountOfObstacles = Random.Range(1, LaneManager.Lanes.Count);
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
        else
        {
            // Fill up the queue more.
            for (int i = 0; i < 5; i++)
            {
                InteractableBlock obstacle = Instantiate(original, _obstacleParent);
                obstacle.gameObject.SetActive(false);
                pool.Enqueue(obstacle);
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

    private void InstantiatePoolInstance(InteractableBlock original, Queue<InteractableBlock> pool)
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
